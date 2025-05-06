# OpenCL Kernel Debugging Guide

This guide covers how to debug and profile OpenCL kernels used by the GPU image processing pipeline, including configuration options, common failure modes, and step-by-step troubleshooting techniques.

---

## Prerequisites

- OpenCL 1.2+ runtime and drivers installed
- `clinfo` utility (optional but recommended)
- GPU vendor's profiling SDK (optional): NVIDIA Nsight, AMD Radeon GPU Profiler, or Intel VTune

---

## Enabling Debug Logging

Set the log level to `Debug` to capture every OpenCL call, workgroup layout decision, and per-pass timing:

```csharp
// In DI setup
builder.AddGpuImageProcessingLogging(); // already filters GpuImageProcessing to Debug

// Or via appsettings.json
```

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "GpuImageProcessing": "Debug"
    }
  }
}
```

With debug logging active you will see output similar to:

```
DBG GpuManagementService: Found OpenCL Platform: NVIDIA CUDA
DBG GpuManagementService: Detected device: NVIDIA GeForce RTX 4090 (Gpu) from NVIDIA
DBG WorkgroupOptimizer: Compute: device=NVIDIA GeForce RTX 4090 image=3840x2160 → [16x16] (wavefront=32)
DBG ComputeShaderPipeline: Pass 'GaussianBlur' dispatched in 4ms, occupancy est. 87%
```

---

## Understanding Workgroup Configuration

The `WorkgroupOptimizer` derives the best 2-D tile size for each pass based on three hardware constraints:

| Constraint | Source property |
|---|---|
| Max threads / workgroup | `GpuDevice.MaxWorkGroupSize` |
| Local (shared) memory | `GpuDevice.LocalMemoryBytes` |
| Wavefront width | `GpuDevice.WavefrontSize` (32 NVIDIA / 64 AMD) |

You can inspect or override the computed layout before a pipeline run:

```csharp
var optimizer = serviceProvider.GetRequiredService<IWorkgroupOptimizer>();
var device    = gpuManager.GetBestDevice()!;

// Synchronous calculation
var config = optimizer.Compute(
    device,
    imageWidth: 1920, imageHeight: 1080,
    localMemoryPerThreadBytes: 16,
    strategy: WorkgroupOptimizationStrategy.MaxOccupancy);

Console.WriteLine($"Workgroup : {config.Width} x {config.Height}");
Console.WriteLine($"Global ND : {config.GlobalWidth} x {config.GlobalHeight}");

// Async benchmark (tries multiple sizes, picks fastest)
var benchConfig = await optimizer.BenchmarkAsync(device, 1920, 1080);
Console.WriteLine($"Benchmarked: {benchConfig.Width} x {benchConfig.Height}");
```

### Optimization strategies

| Strategy | When to use |
|---|---|
| `Balanced` | Default; good for most workloads |
| `MaxOccupancy` | Latency-bound kernels (small images, many passes) |
| `MinLocalMemory` | Devices with limited shared memory (< 32 KB) |
| `MaxThroughput` | Throughput-bound workloads (large batch processing) |

---

## Kernel Compilation Errors

When an OpenCL kernel fails to compile the pipeline throws a `GpuException` with `ErrorCode = Constants.ErrorCodes.KernelCompilationFailed`.

### Capturing the build log

Enable `Debug` logging; the build log is written at `LogLevel.Error`:

```
ERR ComputeShaderPipeline: Kernel compilation failed for pass 'CustomConvolution'
ERR                        Build log: error: use of undeclared identifier 'fmaf'
```

### Common compilation errors

| Error message | Cause | Fix |
|---|---|---|
| `use of undeclared identifier` | Undefined built-in or missing extension pragma | Add `#pragma OPENCL EXTENSION cl_khr_fp64 : enable` at the top of your kernel source |
| `implicit conversion loses precision` | Float/double mismatch | Cast explicitly: `(float)myDouble` |
| `work-group size ... exceeds device limit` | Hardcoded local size too large | Use `get_local_size()` dynamically or lower the `[local]` array dimension |
| `clBuildProgram failed: -11 (CL_BUILD_PROGRAM_FAILURE)` | Generic compilation failure | Retrieve and log the full build log via `clGetProgramBuildInfo` |

---

## Runtime Error Codes

OpenCL error codes are surfaced through `GpuException.ErrorCode`.
The most common values:

| `ErrorCode` constant | Meaning |
|---|---|
| `GpuInitializationFailed` (1001) | OpenCL runtime could not be loaded at startup |
| `InsufficientMemory` (1002) | `clCreateBuffer` or similar allocation failed |
| `KernelCompilationFailed` (1008) | `clBuildProgram` returned an error |
| `MemoryAllocationFailed` (1009) | Host-side or pinned memory allocation failed |
| `DeviceNotAvailable` (1007) | Target device was lost or disconnected during execution |

### Catching and inspecting GPU exceptions

```csharp
try
{
    var result = await pipeline.ExecuteAsync(passes, deviceId, cancellationToken);
}
catch (GpuException ex)
{
    Console.WriteLine($"GPU error  : {ex.Message}");
    Console.WriteLine($"Error code : {ex.ErrorCode}");
    Console.WriteLine($"Device     : {ex.DeviceName ?? "unknown"}");
    Console.WriteLine($"Occurred   : {ex.OccurredAt:O}");

    if (ex.InnerException is not null)
        Console.WriteLine($"Cause      : {ex.InnerException.Message}");
}
```

---

## Profiling Individual Passes

Pipeline statistics accumulate automatically and are retrievable at any time:

```csharp
var stats = await pipeline.GetStatisticsAsync(cancellationToken);
Console.WriteLine($"Total passes executed : {stats.TotalPassesExecuted}");
Console.WriteLine($"Total GPU time        : {stats.TotalGpuTimeMs} ms");
Console.WriteLine($"Average occupancy     : {stats.AverageOccupancyPercent:F1} %");
Console.WriteLine($"Failed passes         : {stats.FailedPasses}");

// Reset between benchmarking runs
await pipeline.ResetStatisticsAsync(cancellationToken);
```

For per-image timings inspect `ProcessingResult`:

```csharp
var result = await imageProcessingService.ProcessImageAsync(imageId, filterIds);

foreach (var entry in result.FilterTimings)
    Console.WriteLine($"  {entry.FilterName,-24} {entry.FilterType,-18} {entry.ElapsedMilliseconds} ms");

Console.WriteLine($"Total: {result.ProcessingTimeMilliseconds} ms");
```

---

## Using Vendor Profiling Tools

### NVIDIA Nsight Systems

```bash
nsys profile --trace=opencl dotnet run -- process-image photo.jpg
nsys-ui report1.nsys-rep
```

### AMD Radeon GPU Profiler

1. Set `AMD_OCL_BUILD_OPTIONS=-g` to include debug symbols in kernel builds.
2. Launch the application under **Radeon GPU Profiler** → *Capture trace*.
3. Inspect kernel dispatch timelines and memory transfer bandwidth.

### Intel VTune

```bash
vtune -collect gpu-offload -- dotnet run -- process-image photo.jpg
vtune-gui result/
```

---

## Validating Device Extensions

Some kernels require optional OpenCL extensions. Check availability before dispatch:

```csharp
var device = gpuManager.GetBestDevice()!;

if (!device.SupportsExtension("cl_khr_fp64"))
    Console.WriteLine("WARNING: double-precision kernels will not compile on this device.");

if (!device.SupportsDoublePrecision)
    Console.WriteLine("WARNING: device does not support double-precision arithmetic.");

// List all extensions
foreach (var ext in device.Extensions.Keys)
    Console.WriteLine($"  {ext}");
```

---

## Step-by-Step Debug Checklist

1. **Enable debug logging** — set `GpuImageProcessing` log level to `Debug`.
2. **Run `clinfo`** — verify that the OpenCL ICD sees your device and its properties match the values reported by `GpuManagementService`.
3. **Check `UseFallback`** — if `gpuManager.UseFallback` is `true` the runtime is using the CPU processor; install GPU drivers to restore OpenCL.
4. **Inspect workgroup config** — log `WorkgroupConfiguration` for each pass; an unexpectedly small tile often indicates a local-memory overflow.
5. **Catch `GpuException`** — log `ErrorCode` and `InnerException.Message` to pinpoint the failing OpenCL call.
6. **Retrieve the build log** — a `KernelCompilationFailed` error always includes the vendor build log at `LogLevel.Error`.
7. **Profile with vendor tools** — attach Nsight / RGP / VTune when kernel execution time is unexpectedly high.
8. **Test on CPU fallback** — force `UseFallback` via `GPU_FORCE_CPU_FALLBACK=1` to isolate whether the problem is in the kernel or the host code.

---

## Best Practices

- **Keep kernels simple**: avoid dynamic branching inside compute loops; prefer precomputed lookup tables.
- **Align buffers**: ensure image row strides are multiples of `Constants.Processing.ImageBufferAlignment` (256 bytes) to maximise memory-access coalescing.
- **Match wavefront size**: choose workgroup widths that are multiples of `GpuDevice.WavefrontSize` (32 or 64) to avoid partial-wavefront underutilisation.
- **Avoid excessive small dispatches**: prefer processing an entire filter chain in a single multi-pass pipeline rather than dispatching one kernel per filter.
- **Release memory promptly**: call `DeallocateMemory` in a `finally` block to prevent GPU OOM on long batch runs.

---

## See Also

- [GPU Device Selection Guide](gpu-device-selection.md)
- [Getting Started](getting-started.md)
- [API Reference](api-reference.md)
