# FilterChainBuilderTests

`FilterChainBuilderTests` contains unit‑test methods that verify the behavior of the `FilterChainBuilder` class in the **gpu-image-processing** library. Each test method exercises a specific contract of the builder—such as validation of input arguments, correct ordering of steps, proper flag propagation, and fluent‑interface semantics—ensuring that the builder behaves predictably under normal and error conditions.

## API

| Test Method | Purpose | Parameters | Return Value | Throws When |
|-------------|---------|------------|--------------|--------------|
| `Create_BlankName_ThrowsArgumentException` | Confirms that invoking `FilterChainBuilder.Create` with an empty or whitespace‑only name throws an `ArgumentException`. | none | `void` | When the supplied name is `null`, empty, or consists only of white‑space characters. |
| `Build_NoStepsAdded_ThrowsInvalidOperationException` | Verifies that calling `Build` on a builder that has not received any filter steps throws an `InvalidOperationException`. | none | `void` | When the internal step collection is empty at build time. |
| `Build_SingleStep_ProducesValidChain` | Ensures that a builder with exactly one filter step produces a chain whose step count equals one and that the step matches the added filter. | none | `void` | — |
| `Build_MultipleSteps_PreservesDeclarationOrder` | Checks that the order of steps in the resulting chain matches the order in which they were added to the builder. | none | `void` | — |
| `Build_WithDescription_SetsChainDescription` | Asserts that supplying a description to the builder is correctly propagated to the built chain’s `Description` property. | none | `void` | — |
| `Build_AllowParallelExecution_SetsFlag` | Validates that enabling parallel execution via `AllowParallelExecution()` sets the corresponding flag on the resulting chain. | none | `void` | — |
| `Build_CacheIntermediates_SetsFlag` | Confirms that calling `CacheIntermediates()` sets the cache‑intermediates flag on the built chain. | none | `void` | — |
| `AddBlur_InvalidRadius_ThrowsArgumentOutOfRangeException` | Tests that providing a blur radius outside the allowed range (≤ 0 or > maximum) throws an `ArgumentOutOfRangeException`. | none | `void` | When the radius argument is less than or equal to zero or exceeds the library‑defined maximum blur radius. |
| `AddSharpen_StrengthAboveMax_ThrowsArgumentOutOfRangeException` | Ensures that a sharpen strength value greater than the permitted maximum triggers an `ArgumentOutOfRangeException`. | none | `void` | When the strength argument exceeds the maximum sharpen strength allowed by the implementation. |
| `AddRotation_AngleOutOfRange_ThrowsArgumentOutOfRangeException` | Verifies that rotation angles outside the valid range (e.g., less than 0° or greater than 360°) cause an `ArgumentOutOfRangeException`. | none | `void` | When the angle argument is negative or greater than 360 degrees. |
| `AddScaling_NegativeScaleX_ThrowsArgumentOutOfRangeException` | Confirms that a negative X‑scale factor results in an `ArgumentOutOfRangeException`. | none | `void` | When the `scaleX` parameter is less than zero. |
| `AddColorCorrection_BrightnessOutOfRange_ThrowsArgumentOutOfRangeException` | Checks that brightness values outside the permissible interval (commonly ‑1.0 to 1.0) throw an `ArgumentOutOfRangeException`. | none | `void` | When the brightness argument is less than ‑1.0 or greater than 1.0. |
| `AddCustomFilter_EmptyGuid_ThrowsArgumentException` | Ensures that supplying an empty `Guid` (`Guid.Empty`) to `AddCustomFilter` throws an `ArgumentException`. | none | `void` | When the provided `guid` equals `Guid.Empty`. |
| `AddCustomFilter_ValidGuid_AppearsInSteps` | Validates that a non‑empty `Guid` passed to `AddCustomFilter` results in a step whose identifier matches the supplied guid. | none | `void` | — |
| `Build_EstimatedProcessingTimeSumsStepEstimates` | Confirms that the `EstimatedProcessingTime` property of the built chain equals the sum of the individual step estimates. | none | `void` | — |
| `Build_FluentChaining_ReturnsSameBuilderInstance` | Tests that each configurator method (e.g., `AddBlur`, `AllowParallelExecution`) returns the builder instance, enabling fluent method chaining. | none | `void` | — |
| `ForCi_Preset_ProducesThreeCategories` | Ensures that the static factory method `ForCi` creates a builder pre‑configured with exactly three filter categories (e.g., blur, sharpen, color correction). | none | `void` | — |

## Usage

The following examples illustrate typical interaction with `FilterChainBuilder` (the class under test). They are **not** part of the test suite but demonstrate the API that the tests validate.

### Example 1: Building a simple blur chain

```csharp
using GpuImageProcessing.Filters;
using GpuImageProcessing.Builders;

var chain = FilterChainBuilder.Create("BlurPipeline")
                              .AddBlur(radius: 2.5)
                              .AllowParallelExecution()
                              .Build();

// chain.Description == "BlurPipeline"
// chain.Steps.Count == 1
// chain.AllowParallelExecution == true
```

### Example 2: Chaining multiple filters with custom guidance

```csharp
using System;
using GpuImageProcessing.Filters;
using GpuImageProcessing.Builders;

var customGuid = Guid.NewGuid();

var chain = FilterChainBuilder.Create("MultiStep")
                              .AddSharpen(strength: 0.8)
                              .AddRotation(angle: 90)
                              .AddScaling(scaleX: 1.5, scaleY: 1.5)
                              .AddColorCorrection(brightness: 0.2, contrast: 1.1)
                              .AddCustomFilter(customGuid)
                              .CacheIntermediates()
                              .Build();

// chain.Steps[0] is a Sharpen step
// chain.Steps[1] is a Rotation step (90°)
// chain.Steps[2] is a Scaling step (1.5, 1.5)
// chain.Steps[3] is a ColorCorrection step
// chain.Steps[4] has Identifier == customGuid
// chain.CacheIntermediates == true
```

## Notes

- **Argument validation**: All methods that accept numeric or guid parameters perform range or emptiness checks and throw the appropriate `ArgumentException` or `ArgumentOutOfRangeException`. Supplying values at the exact boundary of the allowed range is considered valid unless otherwise documented.
- **Order sensitivity**: The builder preserves the insertion order of steps; re‑ordering calls will produce a different chain. This property is relied upon by algorithms that depend on sequential processing.
- **Fluent interface**: Each configurator returns the same builder instance, allowing arbitrary chaining. The return value can be ignored, but doing so breaks the fluent pattern.
- **Thread safety**: `FilterChainBuilder` instances are **not** thread‑safe. Concurrent mutation of the same builder from multiple threads may result in undefined behavior. It is recommended to create a builder per thread or to synchronize access externally.
- **Reusability**: After calling `Build`, the builder retains its internal state and can be reused to create additional chains with the same or modified configuration. However, any changes made after a build will affect only subsequently built chains.
- **Estimated processing time**: The summation performed by `Build_EstimatedProcessingTimeSumsStepEstimates` assumes that each step’s estimate is independent and additive; steps that share resources may not reflect real‑world timing, but the builder’s contract is to expose the arithmetic sum.
