# FilterChainBenchmarksExtensions

Provides benchmarking-oriented extension methods for `FilterChain` to facilitate performance testing, validation, and manipulation of filter chains in GPU-accelerated image processing pipelines.

## API

### `AddStep_RepeatedFilters`

Appends a sequence of identical filter instances to the end of a `FilterChain` for benchmarking purposes. The filters are added in the order they appear in the input sequence.

- **Parameters**
  - `chain`: The target `FilterChain` to which filters will be appended.
  - `filters`: An `IEnumerable<IImageFilter>` representing the sequence of filter instances to add repeatedly.
  - `repeatCount`: The number of times each filter in `filters` should be appended to the chain.
- **Return Value**
  Returns the modified `FilterChain` instance for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `chain` or `filters` is `null`.
  Throws `ArgumentOutOfRangeException` if `repeatCount` is less than zero.

---

### `GetEnabledFilterCount_ForValidation`

Returns the count of filters in the chain that are currently enabled. This is useful for validation and benchmarking scenarios where only active filters should be considered.

- **Parameters**
  - `chain`: The `FilterChain` to analyze.
- **Return Value**
  Returns an `int` representing the number of enabled filters in the chain.
- **Exceptions**
  Throws `ArgumentNullException` if `chain` is `null`.

---

### `ValidateChain_WithDetails`

Performs detailed validation on a `FilterChain` and returns a human-readable report of issues, including disabled filters and potential configuration problems. Intended for debugging and pre-benchmark validation.

- **Parameters**
  - `chain`: The `FilterChain` to validate.
- **Return Value**
  Returns a `string` containing a newline-separated report of validation findings. Returns an empty string if the chain is valid.
- **Exceptions**
  Throws `ArgumentNullException` if `chain` is `null`.

---

### `CloneChain_WithDisabledFilters`

Creates a deep copy of the given `FilterChain`, preserving the structure and configuration of all filters, but with all filters explicitly disabled. Useful for generating baseline or control benchmarks.

- **Parameters**
  - `chain`: The `FilterChain` to clone.
- **Return Value**
  Returns a new `FilterChain` instance with the same structure as the input, but all filters disabled.
- **Exceptions**
  Throws `ArgumentNullException` if `chain` is `null`.

## Usage

```csharp
// Example 1: Benchmarking a repeated filter sequence
var originalChain = new FilterChain();
var blurFilter = new GaussianBlurFilter();
var filters = Enumerable.Repeat(blurFilter, 1);

var benchmarkChain = FilterChainBenchmarksExtensions.AddStep_RepeatedFilters(
    originalChain,
    filters,
    repeatCount: 10
);

// Validate the chain before benchmarking
var validationReport = FilterChainBenchmarksExtensions.ValidateChain_WithDetails(benchmarkChain);
if (!string.IsNullOrEmpty(validationReport))
{
    Console.WriteLine("Validation issues:\n" + validationReport);
}

// Count enabled filters for metric calculation
int enabledCount = FilterChainBenchmarksExtensions.GetEnabledFilterCount_ForValidation(benchmarkChain);
Console.WriteLine($"Enabled filters: {enabledCount}");
```

```csharp
// Example 2: Creating a disabled clone for baseline comparison
var processingChain = new FilterChain();
processingChain.Add(new EdgeDetectionFilter());
processingChain.Add(new SharpenFilter());

var baselineChain = FilterChainBenchmarksExtensions.CloneChain_WithDisabledFilters(processingChain);

var report = FilterChainBenchmarksExtensions.ValidateChain_WithDetails(baselineChain);
Console.WriteLine($"Baseline chain validation:\n{report}");
```

## Notes

- **Thread Safety**: All methods are thread-safe with respect to the `FilterChain` instance passed as a parameter, provided that the `FilterChain` itself is not being modified concurrently by another thread. The returned `FilterChain` or values are safe for immediate use, but any further mutation of the chain should be synchronized externally.
- **Performance Considerations**: `AddStep_RepeatedFilters` may significantly increase chain length; benchmarking pipelines should account for memory and initialization overhead when using high `repeatCount` values.
- **Validation Scope**: `ValidateChain_WithDetails` reports only structural and configuration issues (e.g., disabled filters). It does not validate filter parameters or GPU compatibility.
- **Deep Copy Behavior**: `CloneChain_WithDisabledFilters` performs a deep copy of the chain and its filters. Modifying the original chain after cloning will not affect the clone, and vice versa.
