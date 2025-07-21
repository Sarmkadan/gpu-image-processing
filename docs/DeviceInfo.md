# DeviceInfo

`DeviceInfo` represents a snapshot of a detected OpenCL-capable compute device within the `gpu-image-processing` project. It aggregates identification metadata, hardware capability limits, driver version information, and performance estimates into a single immutable record. Instances are typically obtained through device enumeration and serve as the primary input for selecting an appropriate processing target and configuring kernel execution parameters.

## API

### `public Guid Id`
Unique identifier assigned to the device at detection time. This value is stable for the lifetime of the application session and can be used to correlate the device across selection dialogs, configuration persistence, and log entries.

### `public string Name`
Human-readable name reported by the device driver, such as "NVIDIA GeForce RTX 3060" or "Intel(R) UHD Graphics 630". This string is not guaranteed to be unique across devices in a system.

### `public string Vendor`
Name of the device vendor as reported by the OpenCL platform layer. Typical values include "NVIDIA Corporation", "Intel(R) Corporation", or "Advanced Micro Devices, Inc.".

### `public string DeviceType`
OpenCL device type classification string. Common values are `"GPU"`, `"CPU"`, or `"ACCELERATOR"`. This field corresponds to the `CL_DEVICE_TYPE` query and determines which processing path the scheduler selects.

### `public long GlobalMemoryBytes`
Total global memory capacity in bytes. For a discrete GPU this is the dedicated video RAM; for a CPU device it typically reflects a portion of system memory accessible to the OpenCL runtime. Used to determine whether an input image and intermediate buffers fit on the device.

### `public long LocalMemoryBytes`
Size of local memory per compute unit in bytes. Local memory is shared within a work-group and is critical for optimising tiled convolution and reduction kernels. A value of zero indicates the device does not expose dedicated local memory.

### `public int ComputeUnits`
Number of parallel compute units (streaming multiprocessors, execution units, or cores depending on the architecture). This value directly influences the maximum occupancy and is used when partitioning work into sub-buffers.

### `public int MaxWorkGroupSize`
Maximum number of work-items permitted in a single work-group. Kernels that specify a larger local size will fail at enqueue time. The scheduler clamps requested work-group dimensions to this ceiling.

### `public int MaxWorkItemDimensions`
Maximum dimensionality supported by the device for work-item indices (typically 3). Indicates whether 1D, 2D, or 3D `NDRange` kernels can be launched.

### `public string OpenCLVersion`
OpenCL version string advertised by the device, e.g. `"OpenCL 3.0"` or `"OpenCL 1.2"`. This governs which API features and kernel language constructs are available.

### `public string DriverVersion`
Vendor-specific driver version string, such as `"535.129.03"` for NVIDIA or `"31.0.101.4953"` for Intel. Useful for diagnostics and minimum-version checks.

### `public bool IsAvailable`
Indicates whether the device is currently online and accessible. A device that was present during enumeration but subsequently detached (e.g., an eGPU unplugged) will report `false`. Code paths should check this flag before attempting to create a context or command queue.

### `public DateTime DetectedAt`
UTC timestamp of the moment the device was enumerated and this `DeviceInfo` instance was populated. Used for stale-device detection in long-running services.

### `public Dictionary<string, string> Extensions`
A dictionary of all OpenCL extension names and their versions reported by the device. Keys are extension identifiers (e.g., `"cl_khr_fp64"`), and values are version strings. Presence of a key indicates the extension is supported.

### `public float ClockFrequencyMHz`
Maximum clock frequency of the device in megahertz as reported by `CL_DEVICE_MAX_CLOCK_FREQUENCY`. For CPU devices this may reflect the base frequency of a single core. Used in conjunction with `ComputeUnits` to derive the `EstimatedFlops` value.

### `public float EstimatedFlops`
A rough estimate of single-precision floating-point operations per second, calculated from `ComputeUnits`, `ClockFrequencyMHz`, and an architecture-dependent operations-per-cycle factor. This is a heuristic for relative device ranking and is not a measured benchmark result.

### `public bool SupportsDoublePrecision`
`true` if the `cl_khr_fp64` extension is present in the `Extensions` dictionary; otherwise `false`. Kernels that use `double` types must guard execution with this flag or fall back to single-precision paths.

### `public string? Extensions_String`
Raw, unparsed extensions string as returned by the `CL_DEVICE_EXTENSIONS` query. This is retained for logging and debugging purposes. The parsed form is available in the `Extensions` dictionary. May be `null` if the query failed or the platform did not provide the information.

### `public DeviceInfo`
Parameterless constructor. Initialises all fields to their default values (`Guid.Empty`, zero numeric fields, `false` for booleans, `DateTime.MinValue`, empty collections, and `null` for `Extensions_String`). This constructor exists primarily for serialisation scenarios; normally instances are created by the device enumerator.

### `public string GetFormattedMemory()`
Returns a human-readable string representation of `GlobalMemoryBytes` with an appropriate unit suffix (e.g., `"8.00 GB"`, `"256.00 MB"`, `"4.00 KB"`). The formatting uses base-2 units (1 KB = 1024 bytes) and rounds to two decimal places. Takes no parameters and does not throw.

## Usage

### Example 1: Selecting the fastest available GPU

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public DeviceInfo? SelectBestDevice(IEnumerable<DeviceInfo> devices)
{
    return devices
        .Where(d => d.IsAvailable
                    && d.DeviceType == "GPU"
                    && d.GlobalMemoryBytes >= 4L * 1024 * 1024 * 1024) // at least 4 GB
        .OrderByDescending(d => d.EstimatedFlops)
        .FirstOrDefault();
}

// Usage:
// DeviceInfo? chosen = SelectBestDevice(enumeratedDevices);
// if (chosen is null) throw new InvalidOperationException("No suitable GPU found.");
```

### Example 2: Validating kernel launch parameters against device limits

```csharp
using System;

public void ValidateWorkGroupSize(DeviceInfo device, int requestedLocalSize0, int requestedLocalSize1)
{
    int totalWorkItems = requestedLocalSize0 * requestedLocalSize1;

    if (totalWorkItems > device.MaxWorkGroupSize)
    {
        throw new ArgumentException(
            $"Requested work-group size {totalWorkItems} exceeds device maximum {device.MaxWorkGroupSize}.");
    }

    if (device.MaxWorkItemDimensions < 2)
    {
        throw new NotSupportedException("Device does not support 2D NDRange kernels.");
    }

    if (device.LocalMemoryBytes > 0 && totalWorkItems * sizeof(float) * 16 > device.LocalMemoryBytes)
    {
        Console.WriteLine("Warning: local memory usage may exceed device capacity; consider tiling.");
    }
}

// Usage:
// ValidateWorkGroupSize(selectedDevice, 32, 8);
```

## Notes

- **Immutability:** `DeviceInfo` is designed as a snapshot. Once constructed by the enumerator, its fields do not change. To reflect updated device state (e.g., after an eGPU is reconnected), a new instance must be obtained through re-enumeration.
- **Thread safety:** Read access to any `DeviceInfo` instance is safe from multiple threads without synchronisation, as the object is effectively immutable after publication. The `Extensions` dictionary is not wrapped in a read-only facade; consumers must treat it as read-only to preserve thread safety.
- **`IsAvailable` staleness:** This flag reflects availability at the time the instance was created. A device that becomes unavailable later will still show `true` on an existing instance. Always re-enumerate before critical operations in environments where devices can be hot-unplugged.
- **`EstimatedFlops` accuracy:** The value is a static estimate based on nominal clock frequency and an assumed operations-per-cycle multiplier. It does not account for thermal throttling, boost clocks, or memory bandwidth bottlenecks. Use it for coarse-grained ranking only; micro-benchmarks are necessary for precise performance comparisons.
- **`GetFormattedMemory` precision:** The method rounds to two decimal places. For very small memory sizes (less than 1 KB), the output will show `"0.00 KB"`. Callers that require exact byte counts should use `GlobalMemoryBytes` directly.
- **`Extensions_String` nullability:** On some minimal OpenCL 1.0 implementations, the extensions string query may return an empty result, leading to a `null` value. Always null-check before calling methods on this field.
- **Default constructor:** The parameterless constructor exists to support deserialisation and object-initialiser syntax. Instances created this way contain no meaningful device data and should not be passed to processing pipelines without subsequent property assignment.
