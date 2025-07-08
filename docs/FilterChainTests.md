# FilterChainTests

`FilterChainTests` is a unit test suite that validates the behavior of the `FilterChain` class in the `gpu-image-processing` project. It covers step management (addition, removal, reordering), enabled-step filtering, cloning semantics, and configuration validation. The tests ensure that filter chains maintain correct internal ordering, enforce invariants when manipulated, and produce independent copies with predictable naming.

## API

### AddStep_DefaultOrder_AppendsStepToEnd
Verifies that calling the standard `AddStep` method on a `FilterChain` places the new filter step at the end of the existing sequence. The test constructs a chain with one or more pre-existing steps, adds a new step without specifying a position, and asserts that the step occupies the last index in the internal step collection.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** not applicable (asserts no exception under normal conditions).

### RemoveStep_ExistingFilter_ReturnsTrue_AndReordersRemainingSteps
Confirms that removing a filter step that is present in the chain returns `true` and shifts subsequent steps to close the gap, preserving their relative order. The test inspects the return value and the final positions of the remaining steps.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** not applicable.

### RemoveStep_NonExistentFilter_ReturnsFalse
Ensures that attempting to remove a filter step not contained in the chain returns `false` and leaves the step collection unchanged. The test compares the chain state before and after the call.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** not applicable.

### ReorderSteps_MismatchedFilterCount_ThrowsArgumentException
Validates that `ReorderSteps` throws an `ArgumentException` when the supplied reordering collection does not contain exactly the same number of filters as the chain currently holds. The test passes a list with a different count and asserts the exception type and appropriate message.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** `ArgumentException` (expected in the test, surfaced from the production code under the mismatch condition).

### GetEnabledSteps_WithMixedEnabledState_ReturnsOnlyEnabledSteps
Demonstrates that `GetEnabledSteps` filters the chain’s steps to those whose `Enabled` property is `true`, ignoring any disabled steps. The test populates a chain with a mix of enabled and disabled filters and asserts the returned collection contains exactly the enabled ones in their original order.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** not applicable.

### Clone_CreatesIndependentCopy_WithSuffixedName
Verifies that `Clone` produces a new `FilterChain` instance whose name is the original name with a conventional suffix appended (e.g., “ - Copy”), and that subsequent mutations to the original chain do not affect the clone. The test modifies the original after cloning and asserts the clone’s step collection remains unchanged.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** not applicable.

### FilterConfiguration_Validate_ParameterWithoutMatchingType_ReturnsFalse
Tests the `Validate` method on a filter configuration object, confirming that when a required parameter is supplied with a value whose type does not match the expected parameter type, validation returns `false`. The test constructs a configuration with a deliberate type mismatch and asserts the negative result.

- **Parameters:** none (test method).
- **Return value:** `void`.
- **Throws:** not applicable.

## Usage

### Example 1: Building a chain and verifying step order after add and remove

```csharp
[TestMethod]
public void AddStep_DefaultOrder_AppendsStepToEnd()
{
    var chain = new FilterChain("TestChain");
    var blur = new GaussianBlurFilter();
    var sharpen = new SharpenFilter();

    chain.AddStep(blur);
    chain.AddStep(sharpen);

    var steps = chain.GetSteps();
    Assert.AreEqual(2, steps.Count);
    Assert.AreSame(sharpen, steps[1]); // appended at end
}

[TestMethod]
public void RemoveStep_ExistingFilter_ReturnsTrue_AndReordersRemainingSteps()
{
    var chain = new FilterChain("TestChain");
    var stepA = new BrightnessFilter();
    var stepB = new ContrastFilter();
    var stepC = new SaturationFilter();

    chain.AddStep(stepA);
    chain.AddStep(stepB);
    chain.AddStep(stepC);

    bool removed = chain.RemoveStep(stepB);
    Assert.IsTrue(removed);

    var remaining = chain.GetSteps();
    Assert.AreEqual(2, remaining.Count);
    Assert.AreSame(stepA, remaining[0]);
    Assert.AreSame(stepC, remaining[1]); // reordered to close gap
}
```

### Example 2: Cloning a chain and validating filter configuration

```csharp
[TestMethod]
public void Clone_CreatesIndependentCopy_WithSuffixedName()
{
    var original = new FilterChain("PostProcess");
    original.AddStep(new EdgeDetectionFilter());

    var clone = original.Clone();
    Assert.AreEqual("PostProcess - Copy", clone.Name);
    Assert.AreEqual(1, clone.GetSteps().Count);

    // Mutate original; clone must remain independent
    original.AddStep(new NoiseReductionFilter());
    Assert.AreEqual(1, clone.GetSteps().Count);
}

[TestMethod]
public void FilterConfiguration_Validate_ParameterWithoutMatchingType_ReturnsFalse()
{
    var config = new FilterConfiguration(typeof(GaussianBlurFilter));
    config.SetParameter("Radius", 5.0f); // expects float, but we supply int in validation

    bool isValid = config.Validate(new Dictionary<string, object>
    {
        { "Radius", 5 } // int instead of float
    });

    Assert.IsFalse(isValid);
}
```

## Notes

- **Ordering guarantees:** `AddStep` without an explicit index always appends. `RemoveStep` preserves the relative order of the remaining steps. Any reordering operation that supplies a mismatched number of filters immediately throws `ArgumentException` before modifying the chain.
- **Cloning semantics:** The `Clone` method creates a deep copy of the step collection. The name suffix is deterministic and applied by the production code; tests rely on this convention to verify independence. Modifications to either the original or the clone after cloning do not propagate.
- **Validation logic:** `Validate` performs strict type checking on parameter values. Implicit numeric conversions (e.g., `int` to `float`) are not considered matching types, so validation returns `false` for such mismatches. This behavior is by design and tested explicitly.
- **Thread safety:** These tests are single-threaded unit tests and do not assert thread safety. The production `FilterChain` methods tested here are not guaranteed to be safe for concurrent mutation unless explicitly documented elsewhere.
