# ComputeShaderPass

Represents a single compute shader pass within a GPU image processing pipeline. It encapsulates the shader source code, kernel entry point, execution parameters, input and output images, and metadata about previous executions. Instances are typically created, configured, and then submitted to a scheduler or executor that runs the shader on the GPU.

## API

### `public Guid Id`

A globally unique identifier for this pass. Assigned at construction and intended to remain immutable for the lifetime of the instance.

### `public string KernelName`

The name of the kernel function inside the shader source that should be invoked. Must match an entry point defined in `KernelSource`.

### `public string KernelSource`

The full HLSL/GLSL/Compute Shader source code containing the kernel. This string is compiled at runtime by the underlying GPU abstraction layer.

### `public ShaderPassType PassType`

The type of the shader pass (e.g., `Compute`, `Fragment`, etc.). Determines how the shader is dispatched and which resources are bound.

### `public int Priority`

An integer indicating the execution priority of this pass relative to others. Higher values typically indicate higher priority. The exact scheduling behavior depends on the executor implementation.

### `public WorkgroupConfiguration? WorkgroupConfiguration`

Optional workgroup dimensions (thread group size) for the compute shader. If `null`, the executor may use a default configuration or infer it from the shader.

### `public Dictionary<string, object> Parameters`

A dictionary of uniform parameters to be passed to the shader. Keys are parameter names; values are the corresponding data (e.g., `float`, `int`, `Vector4`). The dictionary is mutable and can be modified before execution.

### `public List<Image> InputImages`

A list of input images that the shader reads from. The order of images may correspond to texture binding slots. The list is mutable; images can be added or removed before execution.

### `public Image? OutputImage`

The single output image that the shader writes to. Can be `null` if the pass does not produce an output (e.g., a reduction pass that writes to a buffer).

### `public DateTime CreatedAt`

The UTC timestamp when this pass instance was created. Set automatically in the constructor.

### `public ComputeShaderPass()`

Initializes a new instance of the `ComputeShaderPass` class. Sets `Id` to a new `Guid`, `CreatedAt` to `DateTime.UtcNow`, and initializes `Parameters`, `InputImages`, and `PassRecords` to empty collections. All other properties are set to their default values (`null` for strings and nullable types, `0` for integers, etc.).

### `public bool IsReady`

Indicates whether the pass has been fully configured and is ready for execution. Typically returns `true` when `KernelName`, `KernelSource`, and at least one input image are set, and `OutputImage` is not `null` (if required by the pass type). The exact logic may vary by implementation.

### `public sealed record PassExecutionRecord`

A sealed record that stores the result of a single execution of a `ComputeShaderPass`. Each record is immutable after creation.

#### `public Guid ExecutionId`

A unique identifier for this specific execution attempt.

#### `public bool Succeeded`

`true` if the shader execution completed without errors; otherwise `false`.

#### `public string? ErrorMessage`

If `Succeeded` is `false`, contains a description of the error that occurred. `null` on success.

#### `public TimeSpan TotalDuration`

The total wall-clock time taken by the execution, including GPU dispatch and any synchronization overhead.

#### `public int PassesExecuted`

The number of sub-passes or iterations that were successfully executed (relevant for multi-pass algorithms).

#### `public int PassesFailed`

The number of sub-passes or iterations that failed during execution.

### `public IReadOnlyList<PassExecutionRecord> PassRecords`

A read-only list of `PassExecutionRecord` instances, one for each time this pass has been executed. The list is appended to by the executor after each run. It is not thread-safe for concurrent modification.

## Usage

### Example 1: Creating and configuring a compute shader pass

```csharp
var pass = new ComputeShaderPass
{
    KernelName = "MainKernel",
    KernelSource = @"
        [numthreads(8,8,1)]
        void MainKernel (uint3 id : SV_DispatchThreadID)
        {
            // ... shader body
        }",
    PassType = ShaderPassType.Compute,
    Priority = 5,
    WorkgroupConfiguration = new WorkgroupConfiguration(8, 8, 1),
    Parameters =
    {
        ["intensity"] = 1.5f,
        ["offset"] = new Vector2(0.1f, 0.2f)
    },
    InputImages = { inputImage },
    OutputImage = outputImage
};

// The pass is now ready for execution.
if (pass.IsReady)
{
    // Submit to executor...
}
```

### Example 2: Inspecting execution results

```csharp
// After the pass has been executed by a scheduler:
foreach (var record in pass.PassRecords)
{
    Console.WriteLine($"Execution {record.ExecutionId}: " +
                      $"Succeeded = {record.Succeeded}, " +
                      $"Duration = {record.TotalDuration.TotalMilliseconds}ms");

    if (!record.Succeeded)
    {
        Console.WriteLine($"  Error: {record.ErrorMessage}");
    }
}
```

## Notes

- **Edge Cases**:  
  - Setting `KernelName` or `KernelSource` to `null` or empty will cause `IsReady` to return `false`.  
  - An empty `InputImages` list may be valid for passes that generate data entirely from parameters, but most compute shaders require at least one input.  
  - If `WorkgroupConfiguration` is `null`, the executor must supply a default; this may lead to suboptimal performance or runtime errors if the shader expects specific thread group sizes.  
  - The `Parameters` dictionary accepts any object type; the executor must be able to marshal the value to the shader’s expected format. Passing an unsupported type may throw at execution time.

- **Thread Safety**:  
  - Instances of `ComputeShaderPass` are **not thread-safe**. Concurrent reads and writes to mutable properties (`Parameters`, `InputImages`, `OutputImage`, `Priority`, etc.) can cause data corruption.  
  - The `PassRecords` list is read-only, but the underlying collection is modified by the executor. Accessing `PassRecords` while an execution is in progress may yield an incomplete or inconsistent view.  
  - `PassExecutionRecord` is an immutable record and is safe for concurrent access once created.
