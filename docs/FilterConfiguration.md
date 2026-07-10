# FilterConfiguration

`FilterConfiguration` is a configuration class used to define and manage image processing filters in the GPU-accelerated image processing pipeline. It encapsulates filter metadata, parameters, and execution settings required to apply a specific filter operation on image data.

## API

### `public Guid Id`
A unique identifier for the filter configuration. Used for tracking and referencing the filter instance within the system.

### `public string Name`
The human-readable name of the filter. This is used for display purposes and filter identification in user interfaces.

### `public FilterType FilterType`
Gets or sets the type of the filter, represented by the `FilterType` enum. Determines the category or algorithmic behavior of the filter (e.g., convolution, thresholding, color adjustment).

### `public string Description`
A textual description of the filter's purpose and behavior. Provided for documentation and user guidance.

### `public Dictionary<string, object> Parameters`
A dictionary mapping parameter names (keys) to their current values (values). Stores runtime or default values used during filter execution. Keys are case-sensitive.

### `public Dictionary<string, string> ParameterTypes`
A dictionary mapping parameter names to their expected type names (e.g., `"int"`, `"float"`, `"bool"`). Used to validate parameter values and guide UI input rendering.

### `public bool IsActive`
Indicates whether the filter is currently active and should be applied during image processing. Defaults to `true`.

### `public int Priority`
Determines the order in which filters are applied when multiple filters are chained. Lower values are processed first.

### `public DateTime CreatedAt`
The timestamp when the filter configuration was created. Immutable after initialization.

### `public DateTime ModifiedAt`
The timestamp when the filter configuration was last modified. Updated automatically on any change.

### `public string? KernelCode`
Gets or sets the CUDA/OpenCL kernel code string used for GPU execution. Optional; may be null for non-kernel-based filters.

### `public int MaxThreadsPerBlock`
The maximum number of threads per block to use when executing the filter on the GPU. Affects performance and resource utilization.

### `public float[]? ConvolutionKernel`
For convolution-based filters, holds the kernel matrix values as a flattened array. Null for non-convolution filters.

### `public bool NormalizeKernel`
Indicates whether the convolution kernel should be normalized (i.e., divided by its sum) before application. Relevant only when `ConvolutionKernel` is set.

### `public FilterConfiguration()`
Default constructor. Initializes a new filter configuration with default values:
- `Id` as a new `Guid`
- `IsActive` as `true`
- `Priority` as `0`
- `CreatedAt` and `ModifiedAt` as current UTC time
- Empty `Parameters`, `ParameterTypes`, and null `KernelCode`, `ConvolutionKernel`

### `public bool Validate()`
Validates the current configuration. Checks:
- All required parameters are present in `Parameters`
- Parameter values match the types specified in `ParameterTypes`
- If `FilterType` requires a kernel, `ConvolutionKernel` is non-null and valid
Returns `true` if valid; otherwise, `false`.

**Throws:** None.

### `public T? GetParameter<T>(string name)`
Retrieves the value of a parameter by name, cast to the specified type `T`.

**Parameters:**
- `name`: The name of the parameter to retrieve.

**Returns:** The parameter value cast to `T`, or `null` if the parameter does not exist or the cast fails.

**Throws:** None.

### `public void SetParameter(string name, object value)`
Sets the value of a parameter by name.

**Parameters:**
- `name`: The name of the parameter to set.
- `value`: The new value for the parameter.

**Throws:** None.

### `public FilterConfiguration Clone()`
Creates a deep copy of the current `FilterConfiguration` instance, including all dictionaries, arrays, and primitive values.

**Returns:** A new `FilterConfiguration` instance with identical values.

**Throws:** None.

## Usage

### Example 1: Creating and Configuring a Convolution Filter
```csharp
var filter = new FilterConfiguration
{
    Name = "Gaussian Blur",
    FilterType = FilterType.Convolution,
    Description = "Applies a Gaussian blur using a 3x3 kernel.",
    IsActive = true,
    Priority = 1,
    MaxThreadsPerBlock = 256,
    NormalizeKernel = true
};

filter.Parameters["sigma"] = 1.5f;
filter.ParameterTypes["sigma"] = "float";

float[] kernel = new float[] { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
filter.ConvolutionKernel = kernel;
filter.SetParameter("kernelSize", 3);

if (filter.Validate())
{
    Console.WriteLine("Filter configuration is valid.");
}
```

### Example 2: Cloning and Modifying a Filter
```csharp
var original = new FilterConfiguration
{
    Name = "Edge Detection",
    FilterType = FilterType.Convolution,
    IsActive = false,
    Priority = 2
};

original.Parameters["threshold"] = 0.5f;
original.ParameterTypes["threshold"] = "float";

var modified = original.Clone();
modified.IsActive = true;
modified.SetParameter("threshold", 0.7f);

Console.WriteLine($"Original active: {original.IsActive}"); // False
Console.WriteLine($"Modified active: {modified.IsActive}"); // True
```

## Notes

- **Thread Safety:** This class is not thread-safe. Concurrent access to `Parameters`, `ParameterTypes`, or mutable properties (e.g., `IsActive`, `Priority`) may lead to race conditions. External synchronization is required when used in multi-threaded contexts.
- **Parameter Validation:** `Validate()` does not enforce semantic correctness (e.g., valid kernel dimensions). It only checks type compatibility and presence. Additional validation logic may be required depending on `FilterType`.
- **Kernel Normalization:** When `NormalizeKernel` is `true`, the kernel is normalized during execution, not during configuration. The raw kernel values remain unchanged in `ConvolutionKernel`.
- **Immutability:** `CreatedAt` is immutable after construction. `ModifiedAt` is updated on any change, including `Clone()`.
- **Null Handling:** `KernelCode` and `ConvolutionKernel` are nullable. Methods like `GetParameter<T>` and `Validate()` handle nulls gracefully without throwing exceptions.
