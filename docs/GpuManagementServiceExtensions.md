# GpuManagementServiceExtensions

The `GpuManagementServiceExtensions` class provides a set of static utility methods designed to facilitate GPU device discovery, selection, and memory management within the `gpu-image-processing` pipeline. It acts as a high-level abstraction layer over the underlying hardware enumeration logic, allowing consumers to query device availability, identify the optimal compute device for specific workloads, retrieve detailed memory statistics per device, and attempt resource allocation without directly instantiating low-level hardware handles.

## API

### `HasAvailableDevices`

```csharp
public static bool HasAvailableDevices { get; }
```

Retrieves a boolean value indicating whether at least one compatible GPU device is currently detected and initialized by the system. This property performs a lightweight check against the internal device registry.

*   **Return Value**: `true` if one or more GPU devices are available for computation; otherwise, `false`.
*   **Exceptions**: This property does not throw exceptions under normal operation; it returns `false` if the underlying hardware subsystem is uninitialized or empty.

### `GetBestDeviceFor`

```csharp
public static GpuDevice? GetBestDeviceFor(string workloadHint)
```

Selects the most suitable `GpuDevice` instance based on a provided workload hint string. The selection algorithm evaluates available devices against capabilities required by the hint (e.g., memory bandwidth, compute tier, or specific shader support) defined in the system configuration.

*   **Parameters**:
    *   `workloadHint`: A string identifier describing the intended operation (e.g., "high-memory-transfer", "compute-intensive").
*   **Return Value**: Returns a `GpuDevice` instance representing the optimal device, or `null` if no devices match the criteria or if no devices are available.
*   **Exceptions**: Throws `ArgumentException` if `workloadHint` is null or empty. Throws `GpuException` if the device enumeration service is in an invalid state.

### `GetPerDeviceMemoryStatistics`

```csharp
public static Dictionary<string, Dictionary<string, object>> GetPerDeviceMemoryStatistics()
```

Captures a snapshot of current memory usage and capacity statistics for all initialized GPU devices. The returned structure maps device identifiers to a dictionary of metric names and their corresponding values.

*   **Return Value**: A dictionary where the key is the unique device ID (string) and the value is another dictionary containing metric keys (e.g., "TotalMemory", "UsedMemory", "FreeMemory") mapped to `object` values (typically `long` or `double`).
*   **Exceptions**: Throws `GpuException` if the underlying driver fails to report memory status for any active device.

### `TryAllocateOnBestDevice`

```csharp
public static bool TryAllocateOnBestDevice(long requiredBytes, out GpuDevice? assignedDevice)
```

Attempts to reserve a contiguous block of memory of the specified size on the best available device determined by default heuristics. This method combines device selection and allocation validation into a single atomic operation.

*   **Parameters**:
    *   `requiredBytes`: The size of the memory block required in bytes.
    *   `assignedDevice`: An output parameter that receives the `GpuDevice` instance if allocation succeeds, or `null` if it fails.
*   **Return Value**: `true` if a suitable device was found and the memory reservation was successful; otherwise, `false`.
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if `requiredBytes` is less than or equal to zero. Does not throw exceptions for insufficient memory; instead, it returns `false`.

## Usage

### Example 1: Conditional Processing Based on Device Availability

This example demonstrates how to check for hardware availability before initializing a heavy processing pipeline, falling back to a CPU-based implementation if no GPUs are detected.

```csharp
using GpuImageProcessing;

public void InitializeProcessingPipeline()
{
    if (!GpuManagementServiceExtensions.HasAvailableDevices)
    {
        Console.WriteLine("No GPU devices detected. Falling back to CPU backend.");
        StartCpuProcessingEngine();
        return;
    }

    var device = GpuManagementServiceExtensions.GetBestDeviceFor("image-filter-chain");
    if (device == null)
    {
        Console.WriteLine("Devices found, but none match the required profile for filtering.");
        StartCpuProcessingEngine();
        return;
    }

    Console.WriteLine($"Initializing on device: {device.Id}");
    StartGpuProcessingEngine(device);
}
```

### Example 2: Memory-Aware Allocation Strategy

This example illustrates retrieving memory statistics to log current usage and attempting to allocate a large buffer only if the best device has sufficient free memory.

```csharp
using GpuImageProcessing;
using System;

public bool PrepareLargeBuffer(long bufferSizeBytes)
{
    // Log current memory state for diagnostics
    var stats = GpuManagementServiceExtensions.GetPerDeviceMemoryStatistics();
    foreach (var deviceStat in stats)
    {
        Console.WriteLine($"Device {deviceStat.Key}: Free = {deviceStat.Value["FreeMemory"]} bytes");
    }

    // Attempt allocation
    if (GpuManagementServiceExtensions.TryAllocateOnBestDevice(bufferSizeBytes, out var device))
    {
        Console.WriteLine($"Successfully allocated {bufferSizeBytes} on device {device?.Id}");
        return true;
    }

    Console.WriteLine("Allocation failed: Insufficient memory on optimal device.");
    return false;
}
```

## Notes

*   **Thread Safety**: All members of `GpuManagementServiceExtensions` are thread-safe for read operations. However, `TryAllocateOnBestDevice` involves state mutation on the underlying device manager. While the method itself handles internal locking to prevent race conditions during the check-and-allocate sequence, callers should ensure that logical allocation workflows are coordinated if multiple threads are competing for limited global GPU memory.
*   **Null Handling**: `GetBestDeviceFor` and the `assignedDevice` output of `TryAllocateOnBestDevice` explicitly return `null` to indicate failure or absence of a match. Callers must perform null checks before accessing properties on the returned `GpuDevice` instances to avoid `NullReferenceException`.
*   **Dynamic Hardware Changes**: The `HasAvailableDevices` property and statistics methods reflect the state of the hardware at the time of invocation. If a GPU is physically removed or disabled by the OS while the application is running, subsequent calls may throw `GpuException` or return updated counts reflecting the loss of the device.
*   **Metric Types**: In the dictionary returned by `GetPerDeviceMemoryStatistics`, values are boxed as `object`. Consumers should cast these values to `long` for memory sizes or `double` for utilization percentages based on the specific metric key being accessed.
