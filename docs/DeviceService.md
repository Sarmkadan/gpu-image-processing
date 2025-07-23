# DeviceService

Provides discovery, selection, and monitoring of compute devices (GPU/CPU) available on the system for image-processing workloads. It exposes asynchronous and synchronous APIs to enumerate devices, select a preferred device, and retrieve runtime statistics such as memory capacity and compute capability.

## API

### `DeviceService`
Initializes a new instance of the device discovery and management service. No device selection is performed until `SelectDeviceAsync` is called.

### `async Task InitializeAsync`
Prepares the service for device enumeration and statistics collection. Must be called before any other public method. Throws `InvalidOperationException` if already initialized.

### `async Task<IEnumerable<DeviceInfo>> GetAllDevicesAsync`
Returns an enumerable of all discovered devices (GPU and CPU). The collection is lazily evaluated and may throw `InvalidOperationException` if `InitializeAsync` has not been called.

### `async Task<DeviceInfo?> GetDeviceByNameAsync(string name)`
Finds and returns the device with the given name, or `null` if not found. The search is case-sensitive. Throws `ArgumentNullException` if `name` is `null`.

### `DeviceInfo? GetSelectedDevice`
Gets the currently selected device, or `null` if none is selected. This property is thread-safe for concurrent reads.

### `async Task<bool> SelectDeviceAsync(DeviceInfo device)`
Sets the active device to the specified instance. Returns `true` if the device was successfully selected; otherwise `false`. Throws `ArgumentNullException` if `device` is `null`. If the device is already selected, the call is a no-op and returns `true`.

### `async Task<IEnumerable<DeviceInfo>> GetGpuDevicesAsync`
Returns an enumerable of all GPU-capable devices discovered on the system. The collection is lazily evaluated and may throw `InvalidOperationException` if `InitializeAsync` has not been called.

### `async Task<IEnumerable<DeviceInfo>> GetCpuDevicesAsync`
Returns an enumerable of all CPU-capable devices discovered on the system. The collection is lazily evaluated and may throw `InvalidOperationException` if `InitializeAsync` has not been called.

### `async Task<DeviceInfo?> GetMostCapableDeviceAsync`
Returns the device with the highest capability score, or `null` if no devices are available. Capability is determined by a composite score derived from memory size and compute unit count. Throws `InvalidOperationException` if no devices have been discovered.

### `async Task<bool> HasSufficientMemoryAsync(long requiredBytes)`
Determines whether the currently selected device has at least the specified amount of memory available. Returns `true` if sufficient; otherwise `false`. Throws `InvalidOperationException` if no device is selected.

### `async Task<DeviceStatistics> GetStatisticsAsync`
Collects and returns a snapshot of runtime statistics for the currently selected device, including memory usage, compute unit utilization, and capability metrics. Throws `InvalidOperationException` if no device is selected.

### `async Task RefreshDevicesAsync`
Re-enumerates all devices on the system, updating internal caches and counters. This does not change the currently selected device unless it is no longer present, in which case selection is cleared. May throw `InvalidOperationException` if `InitializeAsync` has not been called.

### `async Task<string> GetCapabilitiesSummaryAsync`
Generates a human-readable summary of the capabilities of the currently selected device, including memory size, compute units, and capability score. Throws `InvalidOperationException` if no device is selected.

### `int TotalDevices`
Gets the total number of devices discovered (GPU + CPU). This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

### `int AvailableDevices`
Gets the number of devices currently available for selection. This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

### `int GpuDevices`
Gets the number of GPU-capable devices discovered. This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

### `int CpuDevices`
Gets the number of CPU-capable devices discovered. This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

### `long TotalMemoryBytes`
Gets the total memory across all discovered devices, in bytes. This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

### `int TotalComputeUnits`
Gets the total number of compute units across all discovered devices. This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

### `double AverageCapabilityScore`
Gets the average capability score across all discovered devices. Capability score is a normalized value combining memory and compute unit metrics. This value is updated after `RefreshDevicesAsync` completes. Thread-safe for concurrent reads.

## Usage

```csharp
// Example 1: Select the most capable GPU for processing
var deviceService = new DeviceService();
await deviceService.InitializeAsync();
var bestDevice = await deviceService.GetMostCapableDeviceAsync();
if (bestDevice != null)
{
    await deviceService.SelectDeviceAsync(bestDevice);
    var summary = await deviceService.GetCapabilitiesSummaryAsync();
    Console.WriteLine($"Selected device: {summary}");
}
else
{
    Console.WriteLine("No suitable device found.");
}
```

```csharp
// Example 2: Monitor memory availability before launching a large workload
var deviceService = new DeviceService();
await deviceService.InitializeAsync();
await deviceService.RefreshDevicesAsync();
var requiredMemory = 4L * 1024 * 1024 * 1024; // 4 GiB
if (await deviceService.HasSufficientMemoryAsync(requiredMemory))
{
    var stats = await deviceService.GetStatisticsAsync();
    Console.WriteLine($"Memory available: {stats.MemoryAvailableBytes / (1024.0 * 1024.0):F2} MiB");
}
else
{
    Console.WriteLine("Insufficient memory for the requested workload.");
}
```

## Notes

- Device enumeration and selection are asynchronous to avoid blocking the calling thread during hardware discovery.
- Counters such as `TotalDevices`, `GpuDevices`, etc., are updated only after `RefreshDevicesAsync` completes; they reflect the latest state and are safe for concurrent reads.
- If the currently selected device is removed from the system, subsequent calls to methods like `GetStatisticsAsync` will throw `InvalidOperationException` until a new device is selected or `RefreshDevicesAsync` is called.
- Thread safety: all public members are safe for concurrent reads. Write operations (`SelectDeviceAsync`, `RefreshDevicesAsync`) are not atomic with respect to reads; a brief window exists where cached counters may be inconsistent with the selected device. For atomicity, wrap calls in a lock or use a single-threaded context during critical sections.
