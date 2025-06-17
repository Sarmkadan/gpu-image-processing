# FilterChain

`FilterChain` represents a configurable sequence of image-processing steps that can be executed in order to transform an input image. It supports step reordering, conditional execution, parallel processing, and result caching to optimize performance.

## API

### `public Guid Id`
A unique identifier for the filter chain instance. Generated at creation time and immutable thereafter.

### `public string Name`
A human-readable name for the chain, used for display and identification. May be empty but not null.

### `public string Description`
A descriptive summary of the chain’s purpose or behavior. May be empty but not null.

### `public List<FilterStep> Steps`
The ordered collection of steps that comprise the chain. Steps are executed in the order they appear in this list. Never null; may be empty.

### `public bool IsEnabled`
Indicates whether the chain is enabled for execution. Disabled chains are skipped during processing.

### `public int ExecutionOrder`
A value used to sort multiple chains when executed as part of a larger pipeline. Lower values execute first.

### `public DateTime CreatedAt`
The timestamp when the chain was first created. Immutable after instantiation.

### `public DateTime ModifiedAt`
The timestamp of the last modification to the chain’s configuration. Updated automatically on structural changes.

### `public Dictionary<string, object> ChainOptions`
A collection of key-value pairs representing runtime or configuration options for the chain. Keys are case-sensitive. Never null.

### `public bool AllowParallelExecution`
Determines whether steps within the chain may be executed in parallel where dependencies permit.

### `public int MaxParallelSteps`
The maximum number of steps that may be executed concurrently when `AllowParallelExecution` is true. Must be positive.

### `public bool CacheIntermediateResults`
Indicates whether intermediate image results should be cached to avoid redundant computation.

### `public FilterChain()`
Constructs a new, empty filter chain with default options:
- `IsEnabled` = true
- `ExecutionOrder` = 0
- `AllowParallelExecution` = false
- `MaxParallelSteps` = 4
- `CacheIntermediateResults` = false
- `ChainOptions` = empty dictionary
- `Steps` = empty list
- `Id` = new Guid
- `CreatedAt` = `DateTime.UtcNow`
- `ModifiedAt` = `CreatedAt`

### `public void AddStep(FilterStep step)`
Appends the given step to the end of the `Steps` list and updates `ModifiedAt` to `DateTime.UtcNow`.
**Parameters**
- `step`: The `FilterStep` to add. Must not be null.
**Throws**
- `ArgumentNullException`: if `step` is null.

### `public bool RemoveStep(FilterStep step)`
Removes the first occurrence of the specified step from the `Steps` list and updates `ModifiedAt` to `DateTime.UtcNow`.
**Parameters**
- `step`: The `FilterStep` to remove. Must not be null.
**Return Value**
- `true` if the step was found and removed; otherwise `false`.
**Throws**
- `ArgumentNullException`: if `step` is null.

### `public void ReorderSteps(IEnumerable<FilterStep> newOrder)`
Replaces the current `Steps` list with a new order derived from `newOrder`. Only steps present in both collections are retained, preserving their relative order from `newOrder`. Updates `ModifiedAt` to `DateTime.UtcNow`.
**Parameters**
- `newOrder`: An enumerable of steps defining the desired order. Must not be null.
**Throws**
- `ArgumentNullException`: if `newOrder` is null.

### `public List<FilterStep> GetEnabledSteps()`
Returns a new list containing only the steps where `FilterStep.IsEnabled` is true, in the same order as in `Steps`.

### `public bool Validate()`
Checks whether the chain is structurally valid for execution. A chain is valid if:
- `Steps` is not null
- All steps in `Steps` are not null
- If `AllowParallelExecution` is true, then `MaxParallelSteps` > 0
Returns `true` if valid; otherwise `false`.

### `public double EstimateTotalProcessingTime()`
Estimates the total processing time in seconds for executing the chain on a reference GPU. The estimate assumes sequential execution unless `AllowParallelExecution` is true, in which case it accounts for parallel step execution up to `MaxParallelSteps`. Returns 0.0 if the chain is invalid or empty.

### `public FilterChain Clone()`
Creates a deep copy of the chain, including a new `Id`, a copy of `Steps`, and a copy of `ChainOptions`. The `CreatedAt` of the clone is set to `DateTime.UtcNow`, while `ModifiedAt` reflects the current time.

## Usage

```csharp
// Example 1: Building a simple grayscale conversion chain
var chain = new FilterChain
{
    Name = "Grayscale Pipeline",
    Description = "Converts input to grayscale using standard luminance",
    AllowParallelExecution = false,
    CacheIntermediateResults = true
};

chain.AddStep(new FilterStep
{
    Name = "Extract Luminance",
    FilterType = FilterType.Luminance,
    IsEnabled = true
});

chain.AddStep(new FilterStep
{
    Name = "Normalize Output",
    FilterType = FilterType.Normalize,
    IsEnabled = true
});

if (chain.Validate())
{
    var estimatedTime = chain.EstimateTotalProcessingTime();
    Console.WriteLine($"Estimated processing time: {estimatedTime:F2}s");
}
```

```csharp
// Example 2: Dynamic chain with conditional steps and options
var denoiseChain = new FilterChain
{
    Name = "Denoise & Sharpen",
    ExecutionOrder = 1,
    ChainOptions = new Dictionary<string, object>
    {
        ["Tolerance"] = 0.05,
        ["Iterations"] = 3
    }
};

denoiseChain.AddStep(new FilterStep
{
    Name = "Bilateral Denoise",
    FilterType = FilterType.BilateralFilter,
    IsEnabled = true
});

denoiseChain.AddStep(new FilterStep
{
    Name = "Unsharp Mask",
    FilterType = FilterType.UnsharpMask,
    IsEnabled = true
});

var enabledSteps = denoiseChain.GetEnabledSteps();
Console.WriteLine($"Enabled steps: {enabledSteps.Count}");
```

## Notes

- **Thread Safety**: `FilterChain` is not thread-safe for concurrent modifications. Concurrent reads are safe only if no modifications occur. Use external synchronization when accessing or modifying shared instances.
- **Validation**: Always call `Validate()` before execution to avoid runtime errors. Structural validation does not check semantic correctness of step parameters.
- **Cloning**: The `Clone()` method creates a new `Id`, so cloned chains are treated as distinct entities even if their configuration is identical.
- **Parallel Execution**: When `AllowParallelExecution` is enabled, ensure that step dependencies do not introduce race conditions. The system does not enforce dependency ordering automatically.
- **Intermediate Caching**: Enabling `CacheIntermediateResults` may increase memory usage significantly for long chains or high-resolution images.
