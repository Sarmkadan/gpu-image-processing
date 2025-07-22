# TransformService

The `TransformService` acts as the central management interface for GPU-accelerated image processing transformations within the `gpu-image-processing` project. It provides an asynchronous API for the lifecycle management of `Transform` objects, including creation, retrieval, activation, deletion, and cloning. Beyond basic CRUD operations, the service facilitates complex pipeline orchestration through transform chaining, parameter manipulation, and the extraction of low-level kernel code or configuration states, all while maintaining real-time statistics on active and total transformation counts.

## API

### Constructors

#### `public TransformService()`
Initializes a new instance of the `TransformService` class.

### Transformation Lifecycle

#### `public async Task<Transform> CreateTransformAsync`
Creates a new transformation instance within the service context.
*   **Parameters**: None specified in signature (implementation likely relies on internal state or default configuration).
*   **Return Value**: A new `Transform` object.
*   **Exceptions**: May throw if the service is disposed or if resource limits for GPU contexts are reached.

#### `public async Task<Transform?> GetTransformAsync`
Retrieves a specific transformation by its identifier.
*   **Parameters**: Implicitly requires an identifier (likely passed via internal context or overload not shown, but based on signature, it retrieves a specific target).
*   **Return Value**: The `Transform` object if found; otherwise, `null`.
*   **Exceptions**: Throws if the underlying storage mechanism fails.

#### `public async Task<IEnumerable<Transform>> GetAllTransformsAsync`
Retrieves all transformations currently managed by the service, regardless of their active state.
*   **Parameters**: None.
*   **Return Value**: An enumerable collection of `Transform` objects.
*   **Exceptions**: None typical.

#### `public async Task<IEnumerable<Transform>> GetActiveTransformsAsync`
Retrieves only the transformations currently marked as active in the pipeline.
*   **Parameters**: None.
*   **Return Value**: An enumerable collection of active `Transform` objects.
*   **Exceptions**: None typical.

#### `public async Task<IEnumerable<Transform>> GetByTypeAsync`
Filters and retrieves transformations based on their specific implementation type.
*   **Parameters**: Implicitly requires a type filter.
*   **Return Value**: An enumerable collection of `Transform` objects matching the requested type.
*   **Exceptions**: Throws if the specified type is invalid or unrecognized.

#### `public async Task<bool> ActivateTransformAsync`
Marks a specific transformation as active, enabling it for pipeline execution.
*   **Parameters**: Implicitly requires a target transform identifier.
*   **Return Value**: `true` if the activation was successful; `false` if the transform was already active or not found.
*   **Exceptions**: Throws if the transform is in an invalid state for activation.

#### `public async Task<bool> DeactivateTransformAsync`
Marks a specific transformation as inactive, temporarily removing it from pipeline execution without deletion.
*   **Parameters**: Implicitly requires a target transform identifier.
*   **Return Value**: `true` if the deactivation was successful; `false` if the transform was already inactive or not found.
*   **Exceptions**: Throws if the transform is locked or in use.

#### `public async Task<bool> DeleteTransformAsync`
Permanently removes a transformation from the service and releases associated GPU resources.
*   **Parameters**: Implicitly requires a target transform identifier.
*   **Return Value**: `true` if the deletion was successful; `false` if the transform was not found.
*   **Exceptions**: Throws if the transform is currently executing.

#### `public async Task<Transform> CloneTransformAsync`
Creates a deep copy of an existing transformation, preserving its parameters and state.
*   **Parameters**: Implicitly requires a source transform identifier.
*   **Return Value**: A new `Transform` object identical to the source.
*   **Exceptions**: Throws if the source transform is invalid or cannot be duplicated.

### Parameter Management

#### `public async Task<bool> SetParameterAsync`
Sets a single parameter value on a specific transformation.
*   **Parameters**: Implicitly requires transform ID, parameter name, and value.
*   **Return Value**: `true` if the parameter was set successfully; `false` if the parameter name is invalid.
*   **Exceptions**: Throws if the value type mismatches the expected parameter type.

#### `public async Task<bool> SetParametersAsync`
Sets multiple parameters on a specific transformation in a batch operation.
*   **Parameters**: Implicitly requires transform ID and a collection of key-value pairs.
*   **Return Value**: `true` if all parameters were set successfully; `false` if any failed.
*   **Exceptions**: Throws if the batch contains invalid keys or types.

#### `public async Task<float> GetParameterAsync`
Retrieves the current value of a specific float parameter from a transformation.
*   **Parameters**: Implicitly requires transform ID and parameter name.
*   **Return Value**: The `float` value of the parameter.
*   **Exceptions**: Throws if the parameter does not exist or is not of type float.

### Pipeline and Execution

#### `public async Task<List<Transform>> ChainTransformsAsync`
Links multiple transformations together to form an execution sequence.
*   **Parameters**: Implicitly requires a list of transform IDs to chain.
*   **Return Value**: A list of `Transform` objects representing the established chain.
*   **Exceptions**: Throws if a circular dependency is detected or if any transform in the chain is invalid.

#### `public async Task<string> GetPipelineDescriptionAsync`
Generates a human-readable or structured description of the current transformation pipeline configuration.
*   **Parameters**: None.
*   **Return Value**: A `string` containing the pipeline description.
*   **Exceptions**: None typical.

#### `public async Task<string> GetKernelCodeAsync`
Retrieves the raw GPU kernel code (e.g., HLSL or CUDA) generated for the current pipeline or a specific transform.
*   **Parameters**: Implicitly may target a specific transform or the whole pipeline.
*   **Return Value**: A `string` containing the shader/kernel source code.
*   **Exceptions**: Throws if the kernel has not been compiled or generated yet.

#### `public async Task<string> ExportConfigurationAsync`
Serializes the current state of all transforms and pipeline settings into a portable configuration string (e.g., JSON or XML).
*   **Parameters**: None.
*   **Return Value**: A `string` containing the serialized configuration.
*   **Exceptions**: Throws if serialization fails due to invalid object states.

### Statistics

#### `public async Task<TransformStatistics> GetStatisticsAsync`
Retrieves detailed performance and execution statistics for the service.
*   **Parameters**: None.
*   **Return Value**: A `TransformStatistics` object containing metrics such as execution time, memory usage, and frame counts.
*   **Exceptions**: None typical.

#### `public int TotalTransforms`
Gets the total number of transformations currently managed by the service.
*   **Return Value**: An integer count.
*   **Remarks**: This is a synchronous property reflecting the current state.

#### `public int ActiveTransforms`
Gets the number of transformations currently marked as active.
*   **Return Value**: An integer count.
*   **Remarks**: This is a synchronous property reflecting the current state.

## Usage

### Example 1: Creating and Configuring a Transformation Pipeline
This example demonstrates creating a transform, setting parameters, and activating it within the pipeline.

```csharp
using var service = new TransformService();

// Create a new transformation
var blurTransform = await service.CreateTransformAsync();

// Configure specific parameters
await service.SetParameterAsync(blurTransform.Id, "Radius", 5.0f);
await service.SetParameterAsync(blurTransform.Id, "Sigma", 1.2f);

// Activate the transform to include it in the next render pass
bool activated = await service.ActivateTransformAsync(blurTransform.Id);

if (activated)
{
    Console.WriteLine($"Transform active. Total active: {service.ActiveTransforms}");
    
    // Retrieve the generated kernel code for debugging
    string kernel = await service.GetKernelCodeAsync();
    File.WriteAllText("debug_kernel.hlsl", kernel);
}
```

### Example 2: Chaining Transforms and Exporting Configuration
This example illustrates retrieving existing transforms, chaining them, and exporting the final configuration.

```csharp
var service = new TransformService();

// Retrieve all available edge detection transforms
var edgeTransforms = await service.GetByTypeAsync(typeof(EdgeDetectionTransform));

if (edgeTransforms.Any())
{
    // Chain the first two transforms if available
    var chain = await service.ChainTransformsAsync(edgeTransforms.Take(2).Select(t => t.Id).ToList());
    
    // Get statistics after chaining
    var stats = await service.GetStatisticsAsync();
    Console.WriteLine($"Pipeline memory usage: {stats.MemoryUsageBytes} bytes");

    // Export the full pipeline configuration for persistence
    string configJson = await service.ExportConfigurationAsync();
    await File.WriteAllTextAsync("pipeline_config.json", configJson);
}
```

## Notes

*   **Thread Safety**: All public methods returning `Task` are designed for asynchronous execution. While the `async` pattern suggests non-blocking I/O or compute offloading, the internal state of `TransformService` (specifically the counts `TotalTransforms` and `ActiveTransforms`) should be considered eventually consistent relative to ongoing async operations. It is recommended to await modification operations before reading synchronous count properties if strict consistency is required.
*   **Resource Management**: Transformations often hold unmanaged GPU resources (buffers, textures). The `DeleteTransformAsync` method must be explicitly called to release these resources; relying on garbage collection alone may lead to GPU memory leaks.
*   **Null Handling**: `GetTransformAsync` returns `null` if a transform is not found, whereas other retrieval methods return empty collections rather than `null`. Callers must handle the nullable return type appropriately.
*   **Parameter Typing**: `GetParameterAsync` strictly returns a `float`. Attempting to retrieve non-float parameters via this specific method will result in an exception; ensure parameter types match the method signature or use alternative inspection methods if available.
*   **Chaining Constraints**: `ChainTransformsAsync` validates the logical flow of the pipeline. Passing transforms with incompatible input/output formats or creating circular references will cause the method to throw.
