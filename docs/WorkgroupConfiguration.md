# WorkgroupConfiguration

Represents a computed GPU workgroup configuration for an image processing operation. This type encapsulates the optimal workgroup dimensions, global dispatch sizes, local memory requirements, and performance metrics derived by a `WorkgroupOptimizationStrategy` for a specific device. It serves as the immutable result of an optimization pass, providing all the parameters necessary to enqueue a compute kernel with tuned occupancy and resource usage.

## API

### WorkgroupSizeX
`public int WorkgroupSizeX`

The number of work items in the X dimension of a single workgroup. This value is determined by the optimization strategy and reflects the best-fit local size for the target device.

### WorkgroupSizeY
`public int WorkgroupSizeY`

The number of work items in the Y dimension of a single workgroup. Together with `WorkgroupSizeX` and `WorkgroupSizeZ`, this defines the total local work size.

### WorkgroupSizeZ
`public int WorkgroupSizeZ`

The number of work items in the Z dimension of a single workgroup. Typically set to 1 for 2D image processing workloads, but may be larger for volumetric operations.

### GlobalWorkSizeX
`public int GlobalWorkSizeX`

The total number of work items to dispatch in the X dimension. This is computed as the image width (or problem size) rounded up to the nearest multiple of `WorkgroupSizeX`, ensuring no partial workgroups cause out-of-bounds access.

### GlobalWorkSizeY
`public int GlobalWorkSizeY`

The total number of work items to dispatch in the Y dimension. Analogous to `GlobalWorkSizeX`, this rounds the image height up to a multiple of `WorkgroupSizeY`.

### GlobalWorkSizeZ
`public int GlobalWorkSizeZ`

The total number of work items to dispatch in the Z dimension. For 2D operations this is typically 1; for 3D workloads it rounds the depth up to a multiple of `WorkgroupSizeZ`.

### LocalMemoryRequiredBytes
`public long LocalMemoryRequiredBytes`

The amount of local memory (in bytes) required per workgroup for the optimized kernel. This value is derived from the algorithm’s tile size, pixel format, and any intermediate buffers. A value of zero indicates the kernel uses no local memory.

### EstimatedOccupancy
`public double EstimatedOccupancy`

The estimated occupancy ratio (0.0 to 1.0) of the configured workgroup on the target device. Higher values indicate better utilization of the device’s compute units. This is a theoretical estimate based on the device’s maximum workgroup size, local memory limits, and register pressure.

### OptimizationScore
`public double OptimizationScore`

A composite score assigned by the `WorkgroupOptimizationStrategy` that balances occupancy, local memory usage, and dispatch count. Higher scores indicate a more optimal configuration according to the strategy’s heuristics.

### Strategy
`public WorkgroupOptimizationStrategy Strategy`

The enumeration value indicating which optimization strategy produced this configuration. Possible values include strategies that prioritize occupancy, minimize local memory, or target a specific tile size.

### DeviceId
`public Guid DeviceId`

The unique identifier of the GPU or accelerator device for which this configuration was computed. The configuration is only valid when used with this specific device.

### ComputedAt
`public DateTime ComputedAt`

The UTC timestamp when this configuration was generated. Useful for cache invalidation when device capabilities or driver versions change.

### GetTotalWorkgroupSize
`public int GetTotalWorkgroupSize()`

Returns the product of `WorkgroupSizeX`, `WorkgroupSizeY`, and `WorkgroupSizeZ`. This is the total number of work items in a single workgroup. No parameters. Does not throw.

### GetTotalDispatchCount
`public long GetTotalDispatchCount()`

Returns the total number of workgroups that will be dispatched, computed as `(GlobalWorkSizeX / WorkgroupSizeX) * (GlobalWorkSizeY / WorkgroupSizeY) * (GlobalWorkSizeZ / WorkgroupSizeZ)`. No parameters. Does not throw.

### IsValidForDevice
`public bool IsValidForDevice(Guid deviceId)`

Checks whether this configuration is valid for the specified device. Returns `true` if `deviceId` matches `DeviceId`; otherwise returns `false`. This is a simple identity check and does not re-validate against device capabilities.

| Parameter | Type   | Description                                      |
|-----------|--------|--------------------------------------------------|
| deviceId  | `Guid` | The unique identifier of the device to check.    |

**Returns:** `true` if the configuration was computed for the given device; `false` otherwise.

**Throws:** Never.

### ToString
`public override string ToString()`

Returns a string representation of the configuration, including the workgroup dimensions, global sizes, occupancy estimate, and device ID. The format is intended for diagnostic logging and debugging. No parameters. Does not throw.

## Usage

### Example 1: Applying an optimized configuration to a 2D kernel

```csharp
// Assume 'optimizer' is a configured WorkgroupOptimizer and 'device' is the target GPU.
WorkgroupConfiguration config = optimizer.Optimize(
    imageWidth: 1920,
    imageHeight: 1080,
    pixelFormatBytes: 4,
    device: device
);

if (!config.IsValidForDevice(device.Id))
{
    throw new InvalidOperationException("Configuration was computed for a different device.");
}

Console.WriteLine(
    "Dispatching {0} workgroups of size {1}x{2} (occupancy: {3:P1})",
    config.GetTotalDispatchCount(),
    config.WorkgroupSizeX,
    config.WorkgroupSizeY,
    config.EstimatedOccupancy
);

// Enqueue the kernel with the computed sizes.
kernel.SetLocalMemorySize(config.LocalMemoryRequiredBytes);
commandQueue.Execute(
    kernel,
    globalWorkSize: new long[] { config.GlobalWorkSizeX, config.GlobalWorkSizeY },
    localWorkSize: new long[] { config.WorkgroupSizeX, config.WorkgroupSizeY }
);
```

### Example 2: Caching and reusing configurations across frames

```csharp
private readonly Dictionary<(int, int), WorkgroupConfiguration> _configCache = new();

public WorkgroupConfiguration GetOrComputeConfig(int width, int height, Device device)
{
    var key = (width, height);

    if (_configCache.TryGetValue(key, out var cached))
    {
        // Verify the cached config is still valid for the current device.
        if (cached.IsValidForDevice(device.Id))
        {
            return cached;
        }
    }

    // Compute a fresh configuration and store it.
    WorkgroupConfiguration fresh = optimizer.Optimize(width, height, 4, device);
    _configCache[key] = fresh;
    return fresh;
}
```

## Notes

- **Immutability:** All public properties are read-only. Once a `WorkgroupConfiguration` is constructed by an optimization strategy, its values do not change. This makes instances safe to share across threads without synchronization.
- **Device affinity:** The configuration is tied to a specific device via `DeviceId`. Using these workgroup parameters on a different device may result in kernel launch failures or suboptimal performance. Always verify with `IsValidForDevice` before enqueuing.
- **Occupancy estimate limitations:** `EstimatedOccupancy` is a theoretical upper bound. Actual occupancy may be lower due to runtime factors such as register spills, memory bandwidth contention, or concurrent kernel execution.
- **Local memory allocation:** `LocalMemoryRequiredBytes` must be set explicitly on the kernel before dispatch. Failing to allocate sufficient local memory will cause the kernel to fault or produce undefined results.
- **Global size rounding:** `GlobalWorkSizeX` and `GlobalWorkSizeY` are rounded up to multiples of the workgroup dimensions. Kernels must include bounds checks to ignore out-of-range work items in the padded region.
- **Thread safety:** Read-only property access and method calls (`GetTotalWorkgroupSize`, `GetTotalDispatchCount`, `IsValidForDevice`, `ToString`) are inherently thread-safe. No internal state is mutated after construction.
