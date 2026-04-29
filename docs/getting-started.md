# Getting Started Guide

## Prerequisites

Before you begin, ensure you have:

- **.NET 10.0 SDK** or later installed
- **Visual Studio 2022**, VS Code, or any C# IDE
- **OpenCL 1.2+** compatible GPU with drivers installed
- **2GB+ RAM** available

## Installation Steps

### Step 1: Install .NET 10.0

```bash
# macOS
brew install dotnet

# Ubuntu/Debian
sudo apt-get install dotnet-sdk-10.0

# Windows
# Download from https://dotnet.microsoft.com/download
```

### Step 2: Verify .NET Installation

```bash
dotnet --version
```

### Step 3: Install GPU Drivers

**NVIDIA GPU**:
```bash
# Download from https://www.nvidia.com/Download/driverDetails.aspx
# Install CUDA Toolkit 12.0+
```

**AMD GPU**:
```bash
# Download AMD Radeon drivers
# Or install ROCm for Linux
```

**Intel Arc GPU**:
```bash
# Download Intel Graphics drivers
```

### Step 4: Clone Repository

```bash
git clone https://github.com/Sarmkadan/gpu-image-processing.git
cd gpu-image-processing
```

### Step 5: Build Project

```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode (optimized)
dotnet build -c Release
```

### Step 6: Verify Installation

```bash
# Run the application
dotnet run

# Run tests
dotnet test
```

## OpenCL Device Selection

When multiple OpenCL-capable devices are present (for example, an integrated Intel GPU alongside a discrete NVIDIA or AMD card), you need to tell the library which one to use.

### Listing Available Platforms and Devices

```csharp
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Services;

var sp = await DependencyInjectionSetup.CreateAndInitializeServiceProviderAsync(settings);
var gpuManager = sp.GetRequiredService<GpuManagementService>();

var devices = gpuManager.GetAvailableDevices();
foreach (var d in devices)
{
    Console.WriteLine($"[{d.Id}] {d.Name}");
    Console.WriteLine($"  Vendor : {d.Vendor}");
    Console.WriteLine($"  Type   : {d.DeviceType}");       // Gpu / Cpu / Accelerator
    Console.WriteLine($"  VRAM   : {d.GlobalMemoryBytes / 1024 / 1024} MB");
    Console.WriteLine($"  CUs    : {d.MaxComputeUnits}");
    Console.WriteLine($"  Clock  : {d.MaxClockFrequencyMhz} MHz");
    Console.WriteLine();
}
```

### Selecting the Best Device Automatically

```csharp
var best = gpuManager.GetBestDevice();
if (best is null)
    throw new InvalidOperationException("No GPU device available.");

Console.WriteLine($"Auto-selected: {best.Name} (score {best.CalculatePerformanceScore():F0})");
```

### Targeting a Specific Device by ID

Copy the `Guid` printed by the listing above and pass it wherever a `deviceId` is required:

```csharp
// Replace with the GUID from the listing output.
var targetId = Guid.Parse("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");

// Validate the device has enough memory for your workload.
bool ready = gpuManager.ValidateDevice(targetId, requiredMemory: 512 * 1024 * 1024);
if (!ready)
    throw new InvalidOperationException("Device does not meet requirements.");

// Pass the device ID to the pipeline.
var pipeline = sp.GetRequiredService<IComputeShaderPipeline>();
var result = await pipeline.ExecuteAsync(passes, deviceId: targetId, cancellationToken);
```

### Verifying GPU vs CPU Fallback

The library logs a warning and falls back to a simulated device when no real OpenCL runtime is found. You can detect this at runtime:

```csharp
var devices = gpuManager.GetAvailableDevices().ToList();

bool runningOnGpu = devices.Any(d => d.DeviceType == GpuDeviceType.Gpu);
if (!runningOnGpu)
{
    Console.WriteLine("WARNING: no discrete GPU detected — running on CPU fallback.");
    Console.WriteLine("Install OpenCL drivers and restart the application.");
}
else
{
    var gpu = devices.First(d => d.DeviceType == GpuDeviceType.Gpu);
    Console.WriteLine($"GPU confirmed: {gpu.Name} ({gpu.Vendor})");
}
```

> **Tip:** Enable `Debug` logging (`settings.Logging.LogLevel = LogLevel.Debug`) to see
> detailed OpenCL initialisation messages including every platform and device detected,
> the selected workgroup size, and per-pass occupancy estimates.



Create a simple console application:

```csharp
// Program.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Services;

class Program
{
    static async Task Main()
    {
        // 1. Initialize settings
        var settings = ConfigurationValidator.CreateDefaultSettings();
        
        // 2. Create service provider
        var serviceProvider = await DependencyInjectionSetup
            .CreateAndInitializeServiceProviderAsync(settings);
        
        // 3. Get services
        var imageProcessing = serviceProvider
            .GetRequiredService<ImageProcessingService>();
        var deviceService = serviceProvider
            .GetRequiredService<DeviceService>();
        
        // 4. Check available devices
        var devices = await deviceService.GetAvailableDevicesAsync();
        Console.WriteLine($"Found {devices.Count} compute devices:");
        foreach (var device in devices)
        {
            Console.WriteLine($"  - {device.Name} ({device.DeviceType})");
        }
        
        // 5. Register an image
        if (File.Exists("image.jpg"))
        {
            var image = await imageProcessing
                .RegisterImageAsync("image.jpg", "TestImage");
            Console.WriteLine($"Registered image: {image.Id}");
        }
    }
}
```

## IDE Setup

### Visual Studio 2022

1. Open `gpu-image-processing.sln`
2. Wait for NuGet package restore
3. Set startup project to `GpuImageProcessing`
4. Press F5 to run
5. Press Ctrl+Shift+B to build

### Visual Studio Code

1. Install C# extension (ms-dotnettools.csharp)
2. Open the project folder
3. Press Ctrl+Shift+D to debug
4. Select ".NET Launch" configuration

### JetBrains Rider

1. Open project directory
2. Rider will auto-detect .NET SDK
3. Right-click `Program.cs` → Run
4. Use Shift+F9 for debugging

## Configuration Basics

### Development Settings

```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();
settings.ConfigureForDevelopment();

// This sets:
// - LogLevel: Debug
// - GPU acceleration: Enabled
// - Caching: Disabled
// - Validation: Strict
```

### Production Settings

```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();
settings.ConfigureForProduction();

// This sets:
// - LogLevel: Error
// - GPU acceleration: Enabled
// - Caching: Enabled
// - Validation: Moderate
```

### Custom Settings

```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();

settings.Processing.MaxParallelOperations = 2;
settings.Processing.TimeoutSeconds = 120;
settings.Storage.OutputDirectory = "./processed/";
settings.Cache.EnableDistributedCache = true;
```

## Processing Your First Image

```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();
var sp = await DependencyInjectionSetup
    .CreateAndInitializeServiceProviderAsync(settings);

var imageProcessing = sp.GetRequiredService<ImageProcessingService>();
var filterService = sp.GetRequiredService<FilterService>();

// Register image
var image = await imageProcessing.RegisterImageAsync("photo.jpg", "Photo");
Console.WriteLine($"Image registered: {image.Id}");

// Create filter
var blur = await filterService.CreateFilterAsync(
    FilterType.Gaussian,
    "Blur",
    "Gaussian blur"
);

// Set filter parameters
await filterService.UpdateFilterParametersAsync(blur.Id, 
    new Dictionary<string, float> { { "Sigma", 1.5f } }
);

// Process image
var result = await imageProcessing.ProcessImageAsync(
    image.Id,
    new List<Guid> { blur.Id },
    new List<Guid>(),
    Guid.Empty
);

Console.WriteLine($"Processing complete: {result.Status}");
Console.WriteLine($"Output: {result.OutputPath}");
```

## Working with Batch Processing

```csharp
var batchService = sp.GetRequiredService<BatchProcessingService>();
var imageService = sp.GetRequiredService<ImageProcessingService>();
var filterService = sp.GetRequiredService<FilterService>();

// Register multiple images
var imageIds = new List<Guid>();
foreach (var file in Directory.GetFiles("images/", "*.jpg"))
{
    var img = await imageService.RegisterImageAsync(file, Path.GetFileName(file));
    imageIds.Add(img.Id);
}

// Create filters
var blur = await filterService.CreateFilterAsync(
    FilterType.Gaussian, "Blur", "Blur"
);

// Create batch job
var job = await batchService.CreateJobAsync(
    "BlurBatch",
    imageIds,
    new List<Guid> { blur.Id },
    new List<Guid>(),
    Guid.Empty
);

Console.WriteLine($"Job created: {job.Id}");
Console.WriteLine($"Total images: {job.TotalCount}");

// Monitor progress
while (job.Status != "Completed" && job.Status != "Failed")
{
    await Task.Delay(1000);
    job = await batchService.GetJobAsync(job.Id);
    Console.WriteLine($"Progress: {job.ProcessedCount}/{job.TotalCount}");
}
```

## Common Tasks

### Task 1: Apply Multiple Filters

```csharp
var filters = new List<Guid>();

// Blur
var blur = await filterService.CreateFilterAsync(
    FilterType.Gaussian, "Blur", "");
filters.Add(blur.Id);

// Edge detection
var edge = await filterService.CreateFilterAsync(
    FilterType.Sobel, "Edge", "");
filters.Add(edge.Id);

// Apply all
var result = await imageProcessing.ProcessImageAsync(
    imageId, filters, new List<Guid>(), profileId);
```

### Task 2: Apply Transforms

```csharp
var transforms = new List<Guid>();

// Resize
var resize = await transformService.CreateTransformAsync(
    TransformType.Resize, "Scale2x", "");
await transformService.UpdateTransformParametersAsync(
    resize.Id, new Dictionary<string, float>
    {
        { "ScaleX", 2.0f },
        { "ScaleY", 2.0f }
    });
transforms.Add(resize.Id);

// Apply
var result = await imageProcessing.ProcessImageAsync(
    imageId, filterIds, transforms, profileId);
```

### Task 3: Monitor Performance

```csharp
var perfService = sp.GetRequiredService<PerformanceMonitoringService>();

var metrics = await perfService.GetMetricsAsync();
Console.WriteLine($"GPU Utilization: {metrics.GpuUtilization}%");
Console.WriteLine($"Memory Used: {metrics.MemoryUsedMB}MB");
Console.WriteLine($"Throughput: {metrics.ImagesPerSecond} img/sec");
```

## Debugging Tips

### Enable Debug Logging

```csharp
settings.Logging.LogLevel = LogLevel.Debug;
```

### Check Device Info

```csharp
var device = await deviceService.GetCurrentDeviceAsync();
Console.WriteLine($"Device: {device.Name}");
Console.WriteLine($"Memory: {device.GlobalMemoryMB}MB");
Console.WriteLine($"Units: {device.ComputeUnits}");
```

### Catch and Log Exceptions

```csharp
try
{
    var result = await imageProcessing.ProcessImageAsync(...);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
```

## Next Steps

1. **Read** the [API Reference](api-reference.md) for detailed documentation
2. **Explore** the [examples/](../examples/) directory for more code samples
3. **Review** [Architecture](architecture.md) to understand the design
4. **Check** [Deployment Guide](deployment.md) for production setup
5. **Visit** [FAQ](faq.md) for common questions

## Getting Help

- **Issues**: Report bugs on [GitHub Issues](https://github.com/Sarmkadan/gpu-image-processing/issues)
- **Discussions**: Join [GitHub Discussions](https://github.com/Sarmkadan/gpu-image-processing/discussions)
- **Documentation**: See [docs/](.) directory
- **Email**: Contact via GitHub profile

Happy processing! 🚀
