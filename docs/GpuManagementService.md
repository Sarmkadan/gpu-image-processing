# GpuManagementService

Provides centralized management of GPU devices for the `gpu-image-processing` library, enabling discovery, selection, and memory allocation control across available graphics hardware.

## API

### `public GpuManagementService()`
Initializes a new instance of the service. No parameters are required. Throws if the underlying GPU driver subsystem cannot be initialized.

### `public IEnumerable<GpuDevice> GetAvailableDevices`
Returns a collection representing all GPU devices detected on the host system. The enumeration is snapshot‑safe; modifications to the device list after the call do not affect the returned sequence. Throws if device discovery fails.

### `public GpuDevice? GetDeviceById(/* device identifier */)`
Attempts to retrieve a specific GPU device by its unique identifier. Returns the matching `GpuDevice` instance, or `null` if no device with the supplied identifier exists. Throws if the identifier format is invalid or if an internal error occurs during lookup.

### `public GpuDevice? GetBestDevice`
Selects and returns the GPU device considered optimal for general-purpose compute workloads based on internal heuristics (e.g., compute capability, memory bandwidth). Returns `null` when no suitable device is available. Throws if the selection algorithm encounters an unexpected error.

### `public GpuDevice? GetDeviceWithMostMemory`
Returns the GPU device that offers the largest total memory capacity. Returns `null` if no devices are present. Throws if querying device memory properties fails.

### `public bool AllocateMemory(/* GPU device, size in bytes */)`
Requests allocation of a memory block of the specified size on the given GPU device. Returns `true` when the allocation succeeds; returns `false` if the request cannot be satisfied (e.g., insufficient free memory) or if the device is invalid. Throws on invalid arguments or if the allocation call triggers a driver-level error.

### `public void DeallocateMemory(/* allocation handle */)`
Releases a previously allocated memory block identified by the supplied handle. No return value. Throws if the handle is null, does not correspond to an active allocation, or if the underlying deallocation operation fails.

### `public long GetTotalAllocatedMemory`
Reports the cumulative amount of memory currently allocated across all managed GPU devices, in bytes. Returns zero when no allocations exist. Throws if the internal bookkeeping structures are corrupted.

### `public bool ValidateDevice(/* GPU device */)`
Checks whether the supplied GPU device is in a usable state for allocation and computation. Returns `true` if the device passes validation; otherwise returns `false`. Throws if the device reference is null or if an internal validation routine encounters an error.

### `public Dictionary<string, object> GetMemoryStatistics`
Provides a detailed snapshot of memory usage statistics for all managed devices. The dictionary contains vendor‑specific keys (e.g., `"TotalMemory"`, `"FreeMemory"`, `"AllocatedMemory"`) with corresponding numeric or diagnostic values. Returns an empty dictionary if no statistics can be gathered. Throws if querying the statistics subsystem fails.

## Usage

```csharp
using GpuImageProcessing;

// Create the service instance
var gpuService = new GpuManagementService();

// Enumerate all available devices and print their names
foreach (var device in gpuService.GetAvailableDevices)
{
    Console.WriteLine($"Found GPU: {device.Name} (Id: {device.Id})");
}

// Select the best device for processing
GpuDevice? best = gpuService.GetBestDevice;
if (best != null)
{
    Console.WriteLine($"Selected best GPU: {best.Name}");
}
else
{
    Console.WriteLine("No suitable GPU detected.");
}
```

```csharp
using GpuImageProcessing;
using System;

var gpuService = new GpuManagementService();

// Attempt to allocate 256 MB on the device with the most memory
GpuDevice? target = gpuService.GetDeviceWithMostMemory;
if (target == null)
{
    throw new InvalidOperationException("No GPU devices available.");
}

IntPtr allocation = IntPtr.Zero;
bool success = gpuService.AllocateMemory(target, 256L * 1024 * 1024, out allocation);
if (!success)
{
    Console.WriteLine("Allocation failed – insufficient memory or invalid device.");
}
else
{
    Console.WriteLine($"Allocated {gpuService.GetTotalAllocatedMemory} bytes total.");
    // Use the allocation for image processing …
    gpuService.DeallocateMemory(allocation);
    Console.WriteLine("Memory released.");
}
```

## Notes

- Methods that return `GpuDevice?` may yield `null` when no matching device exists or when the underlying hardware cannot be queried; callers should check for null before using the result.
- `GetAvailableDevices` provides a snapshot; if devices are hot‑plugged or removed after the call, the returned collection does not reflect those changes until the method is invoked again.
- Allocation and deallocation operations are not internally synchronized; concurrent calls from multiple threads targeting the same device or allocation handle must be guarded by the caller to avoid race conditions.
- `ValidateDevice` does not alter device state; it merely queries driver-reported health flags and returns a boolean indication of usability.
- The dictionary returned by `GetMemoryStatistics` may contain keys that vary between GPU vendors; consumers should treat missing keys as optional and not rely on specific entries being present.
- The service does not retain ownership of allocated memory beyond the lifetime of the process; failure to deallocate memory will result in leaked GPU resources until the application terminates.
