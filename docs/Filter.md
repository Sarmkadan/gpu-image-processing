# Filter

The `Filter` class represents a configurable image processing unit within the `gpu-image-processing` pipeline, encapsulating metadata, execution order, and the specific GPU kernel code required for transformation. It serves as the primary abstraction for defining both predefined and custom filters, managing their parameters, validation state, and runtime configuration to ensure consistent application across image data streams.

## API

### `public Guid Id`
Gets the unique identifier for this filter instance. This value is immutable once assigned and is used to track the filter throughout the processing pipeline and logging systems.

### `public string Name`
Gets or sets the human-readable name of the filter. This property is used for UI display and diagnostic logging.

### `public FilterType Type`
Gets or sets the enumeration value indicating the category of the filter (e.g., Blur, EdgeDetection, ColorAdjustment). This determines which underlying GPU shader template is utilized.

### `public string Description`
Gets or sets a detailed textual description of the filter's purpose and behavior.

### `public List<FilterParameter> Parameters`
Gets the collection of parameters associated with this filter. These parameters define adjustable inputs such as radius, intensity, or threshold values required by the GPU kernel.

### `public bool IsActive`
Gets or sets a flag indicating whether the filter is currently enabled in the processing chain. If `false`, the pipeline skips this filter during execution.

### `public DateTime CreatedAt`
Gets the timestamp indicating when the filter instance was created.

### `public string? KernelCode`
Gets or sets the raw GLSL/HLSL source code for custom filters. For predefined filters, this property may be `null` as the kernel is resolved internally by type.

### `public int ProcessingOrder`
Gets or sets the integer value determining the sequence in which this filter is applied relative to other filters in the pipeline. Lower values are processed first.

### `public Dictionary<string, object> AppliedSettings`
Gets the dictionary containing the snapshot of settings actually applied during the last execution. This reflects the resolved values after default fallbacks and validation checks.

### `public Filter`
Represents the instance constructor for creating a new `Filter` object. Initialization typically requires setting core properties like `Name` and `Type` immediately after instantiation.

### `public static Filter CreatePredefined`
Factory method used to instantiate a `Filter` with standard configurations based on a specific `FilterType`.
*   **Parameters**: Implicitly relies on internal type definitions; specific overloads may accept a `FilterType` argument depending on implementation context.
*   **Returns**: A fully initialized `Filter` instance with default parameters and kernel code populated for the specified type.
*   **Throws**: May throw an `ArgumentException` if the requested predefined type is not supported.

### `public void AddParameter`
Adds a new parameter definition to the filter's configuration.
*   **Parameters**: Accepts a `FilterParameter` object containing the parameter name, type, default value, and constraints.
*   **Returns**: `void`.
*   **Throws**: Throws `InvalidOperationException` if a parameter with the same name already exists in the `Parameters` list.

### `public FilterParameter? GetParameter`
Retrieves a specific parameter by its name.
*   **Parameters**: `string name` – The unique identifier of the parameter to retrieve.
*   **Returns**: The `FilterParameter` object if found; otherwise, `null`.
*   **Throws**: Does not throw; returns `null` on failure.

### `public bool UpdateParameterValue`
Updates the value of an existing parameter.
*   **Parameters**: `string name` (the parameter identifier) and `object newValue` (the value to assign).
*   **Returns**: `true` if the parameter was found and the value was updated successfully; `false` if the parameter name does not exist or the value fails type validation.
*   **Throws**: Does not throw; returns `false` on failure.

### `public bool ValidateParameters`
Verifies that all current parameter values meet the constraints defined in their respective `FilterParameter` schemas (e.g., range checks, type compatibility).
*   **Parameters**: None.
*   **Returns**: `true` if all parameters are valid; `false` if any parameter violates its constraints.
*   **Throws**: Does not throw; returns `false` on validation failure.

### `public string GetConfiguration`
Serializes the current filter state into a string representation suitable for storage or transmission.
*   **Parameters**: None.
*   **Returns**: A JSON or XML formatted string containing the filter's ID, type, parameters, and kernel code.
*   **Throws**: May throw `InvalidOperationException` if the filter state is inconsistent (e.g., invalid parameter types).

## Usage

### Example 1: Creating and Configuring a Predefined Filter
This example demonstrates initializing a standard Gaussian Blur filter, modifying its radius parameter, and validating the configuration before use.

```csharp
// Create a predefined blur filter
var blurFilter = Filter.CreatePredefined(FilterType.GaussianBlur);
blurFilter.ProcessingOrder = 10;
blurFilter.Name = "High Strength Blur";

// Retrieve and update the radius parameter
var radiusParam = blurFilter.GetParameter("Radius");
if (radiusParam != null)
{
    // Attempt to update the value to 5.5f
    bool updated = blurFilter.UpdateParameterValue("Radius", 5.5f);
    
    if (!updated)
    {
        Console.WriteLine("Failed to update radius; value may be out of range.");
    }
}

// Validate the entire parameter set before adding to the pipeline
if (blurFilter.ValidateParameters())
{
    Console.WriteLine($"Filter '{blurFilter.Name}' is ready for execution.");
}
else
{
    Console.WriteLine("Parameter validation failed.");
}
```

### Example 2: Defining a Custom Kernel Filter
This example shows how to construct a custom filter with inline GPU code and manual parameter definitions.

```csharp
var customFilter = new Filter
{
    Name = "Invert Colors Custom",
    Type = FilterType.Custom,
    ProcessingOrder = 5,
    KernelCode = @"
        __kernel void invert(__read_only image2d_t src, __write_only image2d_t dst) {
            int2 pos = (int2)(get_global_id(0), get_global_id(1));
            float4 color = read_imagef(src, sampler, pos);
            write_imagef(dst, pos, 1.0f - color);
        }"
};

// Add a custom intensity parameter
customFilter.AddParameter(new FilterParameter
{
    Name = "Intensity",
    DefaultValue = 1.0f,
    MinValue = 0.0f,
    MaxValue = 1.0f
});

// Serialize configuration for saving to disk
string configJson = customFilter.GetConfiguration();
File.WriteAllText("custom_invert_filter.json", configJson);
```

## Notes

*   **Thread Safety**: The `Filter` class is not thread-safe. Properties such as `Parameters`, `AppliedSettings`, and `KernelCode` are mutable. If a filter instance is shared across multiple processing threads, external synchronization (e.g., `lock` statements) is required when calling `UpdateParameterValue`, `AddParameter`, or modifying properties directly. Read-only access to `Id`, `CreatedAt`, and `Type` is generally safe after initialization.
*   **Parameter Integrity**: The `UpdateParameterValue` method returns `false` rather than throwing an exception when a parameter name is not found or the type mismatch occurs. Callers must check the return value to ensure state consistency.
*   **Kernel Code Dependency**: If `Type` is set to `FilterType.Custom`, the `KernelCode` property must contain valid shader source code; otherwise, the GPU pipeline will fail at runtime. For predefined types, `KernelCode` is ignored and should ideally remain `null`.
*   **Execution Order**: The `ProcessingOrder` property does not automatically sort the filter list in the pipeline manager. It is the responsibility of the consuming pipeline service to sort filters by this property before execution.
*   **Serialization**: `GetConfiguration` includes the `AppliedSettings` dictionary. Ensure that objects stored within `AppliedSettings` are serializable, otherwise, this method may fail depending on the underlying serializer implementation.
