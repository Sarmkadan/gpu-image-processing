# GPU Device Selection Guide

This guide explains how to enumerate OpenCL devices, choose the right one for your workload, and verify that the correct backend is active at runtime.

---

## Prerequisites

- .NET 10.0 SDK
- OpenCL 1.2-compatible GPU with drivers installed (NVIDIA, AMD, or Intel)
- Library configured via `AddGpuImageProcessing()` (see [Getting Started](getting-started.md))

---

## Listing Available Devices

`GpuManagementService` queries every OpenCL platform on the host at startup.
Use `GetAvailableDevices()` to retrieve the enumerated list:

```csharp
using GpuImageProcessing.Services;

var gpuManager = serviceProvider.GetRequiredService<GpuManagementService>();

foreach (var device in gpuManager.GetAvailableDevices())
{
    Console.WriteLine($"[{device.Id}] {device.Name}");
    Console.WriteLine($"  Type    : {device.DeviceType}");       // Gpu / Cpu / Accelerator
    Console.WriteLine($"  Vendor  : {device.Vendor}");
    Console.WriteLine($"  VRAM    : {device.GlobalMemoryBytes / 1024 / 1024} MB");
    Console.WriteLine($"  CUs     : {device.MaxComputeUnits}");
    Console.WriteLine($"  Clock   : {device.MaxClockFrequencyMhz} MHz");
    Console.WriteLine($"  Driver  : {device.Driver}");
    Console.WriteLine($"  Version : {device.Version}");
    Console.WriteLine();
}
```

### Device fields reference

| Property | Description |
|---|---|
| `Id` | Unique `Guid` assigned at startup — use this to target a specific device |
| `Name` | Human-readable device name (e.g. `NVIDIA GeForce RTX 4090`) |
| `DeviceType` | `Gpu`, `Cpu`, or `Accelerator` |
| `Vendor` | Driver vendor string |
| `GlobalMemoryBytes` | Total device memory in bytes |
| `MaxAllocatableMemoryBytes` | Memory currently available for allocation |
| `MaxComputeUnits` | Number of compute units (CUs / streaming multiprocessors) |
| `MaxWorkGroupSize` | Maximum threads per workgroup for this device |
| `WavefrontSize` | Native warp / wavefront width (32 on NVIDIA, 64 on AMD) |
| `SupportsDoublePrecision` | `true` when `cl_khr_fp64` is available |

---

## Automatic Device Selection

When you pass `Guid.Empty` as the `deviceId`, the pipeline selects the device with the highest *performance score*.
The score is a weighted function of compute units, clock frequency, and memory:

```csharp
var best = gpuManager.GetBestDevice();

if (best is null)
    throw new InvalidOperationException("No compute device available.");

Console.WriteLine($"Auto-selected : {best.Name}");
Console.WriteLine($"Score         : {best.CalculatePerformanceScore():F0}");
Console.WriteLine($"VRAM          : {best.GlobalMemoryBytes / 1024 / 1024} MB");
```

### Selecting the device with the most memory

Prefer this when processing very large images or batches:

```csharp
var memDevice = gpuManager.GetDeviceWithMostMemory();
Console.WriteLine($"Most memory : {memDevice?.Name} — {memDevice?.GlobalMemoryBytes / 1024 / 1024} MB");
```

---

## Targeting a Specific Device

1. Run the listing snippet above and note the `Guid` printed next to your target device.
2. Pass that `Guid` to the pipeline:

```csharp
var targetId = Guid.Parse("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");

// Verify the device can handle the expected workload before committing.
bool ready = gpuManager.ValidateDevice(
    targetId,
    requiredMemory: 512 * 1024 * 1024,   // 512 MB
    requiredComputeUnits: 4);

if (!ready)
    throw new InvalidOperationException("Device does not meet requirements.");

var pipeline = serviceProvider.GetRequiredService<IComputeShaderPipeline>();
var result   = await pipeline.ExecuteAsync(passes, deviceId: targetId, cancellationToken);
```

---

## Checking Whether CPU Fallback Is Active

When no OpenCL runtime is found the library initialises a CPU-backed device and sets `UseFallback = true`.
No exception is thrown; processing continues at reduced speed.

```csharp
if (gpuManager.UseFallback)
{
    Console.WriteLine("WARNING: running on CPU fallback — install OpenCL drivers for GPU acceleration.");
}
else
{
    var gpus = gpuManager.GetAvailableDevices().Where(d => d.DeviceType == GpuDeviceType.Gpu).ToList();
    Console.WriteLine($"GPU(s) active: {string.Join(", ", gpus.Select(g => g.Name))}");
}
```

---

## Memory Management

### Pre-flight memory check

```csharp
long imageBytes = width * height * bytesPerPixel;

if (!gpuManager.ValidateDevice(deviceId, requiredMemory: imageBytes))
    throw new InvalidOperationException("Insufficient GPU memory for this image.");

gpuManager.AllocateMemory(imageBytes, deviceId);
try
{
    // ... run your pipeline ...
}
finally
{
    gpuManager.DeallocateMemory(imageBytes, deviceId);
}
```

### Reading memory statistics

```csharp
var stats = gpuManager.GetMemoryStatistics();
Console.WriteLine($"Total GPU memory   : {(long)stats["TotalGpuMemory"]     / 1024 / 1024} MB");
Console.WriteLine($"Available          : {(long)stats["TotalAvailableMemory"] / 1024 / 1024} MB");
Console.WriteLine($"Allocated by app   : {(long)stats["TotalAllocatedMemory"] / 1024 / 1024} MB");
Console.WriteLine($"Usage              : {(double)stats["MemoryUsagePercent"]:F1} %");
```

---

## Multi-GPU Setups

On systems with more than one OpenCL device you can distribute work manually:

```csharp
var devices = gpuManager.GetAvailableDevices()
    .Where(d => d.DeviceType == GpuDeviceType.Gpu && d.Validate())
    .OrderByDescending(d => d.CalculatePerformanceScore())
    .ToList();

// Fan out batches across available GPUs
var tasks = batches.Zip(devices, (batch, device) =>
    pipeline.ExecuteAsync(batch, deviceId: device.Id, CancellationToken.None));

var results = await Task.WhenAll(tasks);
```

---

## Environment Variables

| Variable | Effect |
|---|---|
| `GPU_DEVICE_INDEX` | Zero-based index into the ordered device list to force selection |
| `GPU_PLATFORM_NAME` | Substring match on the OpenCL platform name (e.g. `NVIDIA`, `AMD`) |
| `GPU_FORCE_CPU_FALLBACK` | Set to `1` to bypass OpenCL and always use the CPU processor |

These are read by `AppSettings` when present and override any value set in `appsettings.json`.

---

## Troubleshooting

### No devices detected

```
WARN GpuManagementService: No OpenCL platforms found. Falling back to CPU-based processing.
```

- Ensure the OpenCL runtime (ICD) is installed for your GPU vendor.
- On Linux, install `ocl-icd-opencl-dev` and the vendor's ICD package.
- On Windows, updating GPU drivers usually installs the OpenCL runtime.
- Run `clinfo` (or `clinfo.exe`) to confirm the runtime is visible to the OS.

### Wrong device selected automatically

Pass the explicit `Guid` (see [Targeting a Specific Device](#targeting-a-specific-device)) instead of relying on auto-selection.

### Insufficient memory errors

Check `GetMemoryStatistics()` before processing large images and reduce batch size or image resolution if needed.

---

## See Also

- [Getting Started](getting-started.md)
- [OpenCL Kernel Debugging Guide](opencl-kernel-debugging.md)
- [API Reference](api-reference.md)
