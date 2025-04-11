[![Build](https://github.com/sarmkadan/gpu-image-processing/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/gpu-image-processing/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# GPU-Accelerated Image Processing

> High-performance GPU-powered image processing library in C# with OpenCL support, comprehensive filtering, geometric transforms, and batch operations.

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Performance Profiles](#performance-profiles)
- [Performance Benchmarks](#performance-benchmarks)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Capabilities

- **GPU Acceleration**: Leverages OpenCL 1.2+ for high-speed image processing on NVIDIA, AMD, and Intel devices
- **Advanced Filtering**: Gaussian blur, bilateral filtering, median filters, Sobel edge detection, Canny edge detection, morphological operations
- **Geometric Transforms**: Rotation, resizing, affine transforms, color space conversion (RGB/HSV/LAB), normalization, histogram equalization
- **Batch Processing**: Process thousands of images with job queuing, progress tracking, and distributed scheduling
- **Device Management**: Automatic detection and selection of compute devices (GPU/CPU) with fallback support
- **Customizable Profiles**: Speed-optimized, quality-optimized, and balanced processing profiles
- **Performance Analytics**: Detailed metrics on processing time, memory usage, GPU utilization, and throughput
- **Async/Await Support**: Non-blocking operations with full async/await integration
- **Distributed Caching**: Result caching with configurable TTL and distributed cache support
- **Security**: Input validation, safe type conversions, exception handling

### Enterprise Features

- **Background Workers**: Automated cache maintenance, health checks, metrics aggregation
- **Event-Driven Architecture**: Domain events, event aggregation, and event publishing
- **Middleware Pipeline**: Pluggable middleware for logging, compression, rate limiting, authorization
- **Integration APIs**: HTTP image client, webhook handlers, remote image services, database connection pooling
- **Multiple Output Formats**: Export results as CSV, JSON, XML, HTML, Markdown, or plain text
- **Monitoring & Telemetry**: Comprehensive health checks, metrics publishing, async task queues

## Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────┐
│  API & CLI Layer                    │
│  (Controllers, Commands)            │
├─────────────────────────────────────┤
│  Middleware Pipeline                │
│  (Logging, Auth, Compression)       │
├─────────────────────────────────────┤
│  Business Logic Services            │
│  (Processing, Filtering, Batching)  │
├─────────────────────────────────────┤
│  Repository Layer                   │
│  (Data Access & Caching)            │
├─────────────────────────────────────┤
│  OpenCL Integration                 │
│  (Device Management, Compute)       │
└─────────────────────────────────────┘
```

### Core Components

| Component | Purpose | Key Files |
|-----------|---------|-----------|
| **Models** | Domain entities and DTOs | `Image.cs`, `Filter.cs`, `ProcessingJob.cs` |
| **Services** | Business logic orchestration | `ImageProcessingService.cs`, `BatchProcessingService.cs` |
| **Repository** | Data persistence and access | `GenericRepository.cs`, `ImageRepository.cs` |
| **Configuration** | DI setup and settings management | `DependencyInjectionSetup.cs`, `ApplicationSettings.cs` |
| **Integration** | External services and APIs | `HttpImageClient.cs`, `WebhookHandler.cs` |
| **Events** | Domain event handling | `EventAggregator.cs`, `DomainEvents.cs` |
| **Middleware** | Request/response processing | `ProcessingPipeline.cs`, `ErrorHandlingMiddleware.cs` |

## System Requirements

### Minimum Requirements

- **Runtime**: .NET 10.0 or higher
- **Memory**: 2GB RAM
- **Storage**: 500MB disk space for installation
- **GPU**: OpenCL 1.2+ compatible device (NVIDIA, AMD, Intel)

### Recommended Requirements

- **Runtime**: .NET 10.0 (latest)
- **Memory**: 8GB+ RAM
- **Storage**: SSD with 10GB available space
- **GPU**: NVIDIA RTX series, AMD RDNA2+, or Intel Arc
- **OS**: Windows 11/Server 2022, Ubuntu 22.04 LTS, or macOS 13+

### Supported Image Formats

- **Input**: JPEG, PNG, BMP, TIFF, WebP, GIF
- **Output**: JPEG, PNG, BMP, TIFF, WebP

### Supported Platforms

- Windows 10/11, Windows Server 2019+
- Ubuntu 20.04+, Debian 11+
- macOS 11+
- Docker containers

## Installation

### Method 1: Build from Source

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/gpu-image-processing.git
cd gpu-image-processing

# Build the project
dotnet build -c Release

# Run tests (optional)
dotnet test

# Create a release package
dotnet publish -c Release -o ./publish
```

### Method 2: Install as NuGet Package

```bash
# Add the package to your project
dotnet add package GpuImageProcessing
```

### Method 3: Docker Installation

```bash
# Build Docker image
docker build -t gpu-image-processing:latest .

# Run in Docker
docker run --rm --gpus all -v $(pwd)/images:/app/images gpu-image-processing:latest
```

### Method 4: Using Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f app

# Stop services
docker-compose down
```

## Quick Start

### Console Application

```csharp
using GpuImageProcessing;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Services;

// Initialize
var settings = ConfigurationValidator.CreateDefaultSettings();
var serviceProvider = await DependencyInjectionSetup
    .CreateAndInitializeServiceProviderAsync(settings);

// Get services
var imageProcessing = serviceProvider.GetRequiredService<ImageProcessingService>();
var deviceService = serviceProvider.GetRequiredService<DeviceService>();

// Detect GPU devices
var devices = await deviceService.GetAvailableDevicesAsync();
Console.WriteLine($"Found {devices.Count} compute devices");

// Register an image
var image = await imageProcessing.RegisterImageAsync("photo.jpg", "MyPhoto");

// Create and apply a filter
var filterService = serviceProvider.GetRequiredService<FilterService>();
var filter = await filterService.CreateFilterAsync(
    FilterType.Gaussian, 
    "Blur", 
    "Gaussian blur filter"
);

// Process the image
var result = await imageProcessing.ProcessImageAsync(
    image.Id,
    new List<Guid> { filter.Id },
    new List<Guid>(),
    Guid.Empty
);

Console.WriteLine($"Processing complete: {result.Status}");
```

### ASP.NET Core Integration

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add GPU image processing
var settings = ConfigurationValidator.CreateDefaultSettings();
builder.Services.AddSingleton(settings);
await DependencyInjectionSetup.ConfigureServicesAsync(builder.Services, settings);

var app = builder.Build();
app.MapControllers();
app.Run();
```

## Usage Examples

### Example 1: Apply Gaussian Blur

```csharp
var imageService = serviceProvider.GetRequiredService<ImageProcessingService>();
var filterService = serviceProvider.GetRequiredService<FilterService>();

// Register image
var image = await imageService.RegisterImageAsync("image.jpg", "Original");

// Create Gaussian blur filter with sigma=2.0
var filter = await filterService.CreateFilterAsync(FilterType.Gaussian, "GaussianBlur", "Blur filter");
await filterService.UpdateFilterParametersAsync(filter.Id, new Dictionary<string, float>
{
    { "Sigma", 2.0f },
    { "KernelSize", 5f }
});

// Apply filter
var result = await imageService.ProcessImageAsync(
    image.Id,
    new List<Guid> { filter.Id },
    new List<Guid>(),
    Guid.Empty
);

Console.WriteLine($"Result saved to: {result.OutputPath}");
```

### Example 2: Batch Process Multiple Images

```csharp
var batchService = serviceProvider.GetRequiredService<BatchProcessingService>();
var imageService = serviceProvider.GetRequiredService<ImageProcessingService>();

// Register multiple images
var imageIds = new List<Guid>();
foreach (var filePath in Directory.GetFiles("images/", "*.jpg"))
{
    var image = await imageService.RegisterImageAsync(filePath, Path.GetFileNameWithoutExtension(filePath));
    imageIds.Add(image.Id);
}

// Create batch job
var job = await batchService.CreateJobAsync(
    "BatchBlur",
    imageIds,
    filterIds,
    transformIds,
    profileId
);

// Monitor progress
var updated = await batchService.GetJobAsync(job.Id);
Console.WriteLine($"Status: {updated.Status}, Processed: {updated.ProcessedCount}/{updated.TotalCount}");
```

### Example 3: Apply Transforms and Filters

```csharp
var imageService = serviceProvider.GetRequiredService<ImageProcessingService>();
var transformService = serviceProvider.GetRequiredService<TransformService>();

// Register image
var image = await imageService.RegisterImageAsync("photo.jpg", "Original");

// Create transforms (resize and rotate)
var resizeTransform = await transformService.CreateTransformAsync(
    TransformType.Resize,
    "Scale2x",
    "Scale to 2x size"
);
await transformService.UpdateTransformParametersAsync(resizeTransform.Id, new Dictionary<string, float>
{
    { "ScaleX", 2.0f },
    { "ScaleY", 2.0f }
});

var rotateTransform = await transformService.CreateTransformAsync(
    TransformType.Rotate,
    "Rotate45",
    "Rotate 45 degrees"
);
await transformService.UpdateTransformParametersAsync(rotateTransform.Id, new Dictionary<string, float>
{
    { "Angle", 45.0f }
});

// Apply both
var result = await imageService.ProcessImageAsync(
    image.Id,
    filterIds,
    new List<Guid> { resizeTransform.Id, rotateTransform.Id },
    profileId
);
```

### Example 4: Use Processing Profiles

```csharp
var imageService = serviceProvider.GetRequiredService<ImageProcessingService>();

// Create custom profile for speed
var profile = new ProcessingProfile
{
    Name = "SpeedOptimized",
    Description = "Fast processing with reduced quality",
    MaxParallelOperations = 8,
    BatchSize = 20,
    UseGPUAcceleration = true,
    Precision = "float16",
    CacheResults = false
};

// Apply with speed profile
var result = await imageService.ProcessImageAsync(
    imageId,
    filterIds,
    transformIds,
    profileId
);
```

### Example 5: Monitor Device Information

```csharp
var deviceService = serviceProvider.GetRequiredService<DeviceService>();

var devices = await deviceService.GetAvailableDevicesAsync();
foreach (var device in devices)
{
    Console.WriteLine($"Device: {device.Name}");
    Console.WriteLine($"  Type: {device.DeviceType}");
    Console.WriteLine($"  Memory: {device.GlobalMemoryMB}MB");
    Console.WriteLine($"  Compute Units: {device.ComputeUnits}");
    Console.WriteLine($"  Clock Speed: {device.ClockFrequencyMHz}MHz");
}

// Set preferred device
await deviceService.SelectDeviceAsync(devices.First().Id);
```

### Example 6: Export Results in Multiple Formats

```csharp
var result = await imageService.ProcessImageAsync(imageId, filterIds, transformIds, profileId);

// Export as JSON
var jsonFormatter = new JsonResultFormatter();
var json = jsonFormatter.Format(result);
File.WriteAllText("result.json", json);

// Export as CSV
var csvFormatter = new CsvResultFormatter();
var csv = csvFormatter.Format(result);
File.WriteAllText("result.csv", csv);

// Export as XML
var xmlFormatter = new XmlResultFormatter();
var xml = xmlFormatter.Format(result);
File.WriteAllText("result.xml", xml);
```

### Example 7: Handle Events

```csharp
var eventAggregator = serviceProvider.GetRequiredService<EventAggregator>();

// Subscribe to processing events
eventAggregator.Subscribe<ImageProcessedEvent>(@event =>
{
    Console.WriteLine($"Image processed: {_event.ImageId}");
});

eventAggregator.Subscribe<ProcessingFailedEvent>(@event =>
{
    Console.WriteLine($"Processing failed: {@event.Reason}");
});

// Events are published automatically during processing
var result = await imageService.ProcessImageAsync(imageId, filterIds, transformIds, profileId);
```

### Example 8: Implement Custom Middleware

```csharp
// Custom middleware for logging
public class CustomLoggingMiddleware : IProcessingMiddleware
{
    public async Task<MiddlewareContext> ExecuteAsync(MiddlewareContext context)
    {
        var startTime = DateTime.UtcNow;
        Console.WriteLine($"Processing started for {context.ImageId}");
        
        await context.CallNextAsync();
        
        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"Processing completed in {duration.TotalMilliseconds}ms");
        
        return context;
    }
}

// Register in pipeline
var pipeline = new ProcessingPipeline();
pipeline.Add(new CustomLoggingMiddleware());
pipeline.Add(new ErrorHandlingMiddleware());
```

## API Reference

### ImageProcessingService

```csharp
// Register an image for processing
Task<Image> RegisterImageAsync(string filePath, string name);

// Process image with filters and transforms
Task<ProcessingResult> ProcessImageAsync(
    Guid imageId,
    List<Guid> filterIds,
    List<Guid> transformIds,
    Guid profileId
);

// Retrieve processing result
Task<ProcessingResult> GetResultAsync(Guid resultId);

// List all registered images
Task<List<Image>> GetImagesAsync();
```

### FilterService

```csharp
// Create a new filter
Task<Filter> CreateFilterAsync(FilterType type, string name, string description);

// Update filter parameters
Task UpdateFilterParametersAsync(Guid filterId, Dictionary<string, float> parameters);

// Get filter details
Task<Filter> GetFilterAsync(Guid filterId);

// List available filters
Task<List<Filter>> GetFiltersAsync();

// Delete a filter
Task DeleteFilterAsync(Guid filterId);
```

### TransformService

```csharp
// Create a new transform
Task<Transform> CreateTransformAsync(TransformType type, string name, string description);

// Update transform parameters
Task UpdateTransformParametersAsync(Guid transformId, Dictionary<string, float> parameters);

// Get transform details
Task<Transform> GetTransformAsync(Guid transformId);

// List available transforms
Task<List<Transform>> GetTransformsAsync();
```

### BatchProcessingService

```csharp
// Create batch processing job
Task<ProcessingJob> CreateJobAsync(
    string name,
    List<Guid> imageIds,
    List<Guid> filterIds,
    List<Guid> transformIds,
    Guid profileId
);

// Execute batch job
Task ExecuteJobAsync(Guid jobId, Guid profileId);

// Get job status
Task<ProcessingJob> GetJobAsync(Guid jobId);

// Cancel running job
Task CancelJobAsync(Guid jobId);

// List all jobs
Task<List<ProcessingJob>> GetJobsAsync();
```

### DeviceService

```csharp
// Get all available compute devices
Task<List<DeviceInfo>> GetAvailableDevicesAsync();

// Select preferred device
Task SelectDeviceAsync(Guid deviceId);

// Get current selected device
Task<DeviceInfo> GetCurrentDeviceAsync();

// Get device capabilities
Task<Dictionary<string, string>> GetDeviceCapabilitiesAsync(Guid deviceId);
```

## Configuration

### ApplicationSettings Structure

```csharp
public class ApplicationSettings
{
    public ProcessingSettings Processing { get; set; }
    public StorageSettings Storage { get; set; }
    public CacheSettings Cache { get; set; }
    public DeviceSettings Device { get; set; }
    public SecuritySettings Security { get; set; }
    public BatchSettings Batch { get; set; }
}
```

### Programmatic Configuration

```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();

// Processing settings
settings.Processing.MaxParallelOperations = 4;
settings.Processing.UseGPUAcceleration = true;
settings.Processing.TimeoutSeconds = 300;

// Storage settings
settings.Storage.InputDirectory = "./input/";
settings.Storage.OutputDirectory = "./output/";
settings.Storage.TempDirectory = "./temp/";

// Cache settings
settings.Cache.EnableDistributedCache = true;
settings.Cache.CacheTTLSeconds = 3600;

// Device settings
settings.Device.PreferredDeviceType = "GPU";
settings.Device.AllowFallbackToCPU = true;

// Security settings
settings.Security.ValidateImageDimensions = true;
settings.Security.MaxImageWidth = 8192;
settings.Security.MaxImageHeight = 8192;
```

### Configuration File (appsettings.json)

```json
{
  "Application": {
    "Name": "GPU Image Processing",
    "Environment": "Production",
    "LogLevel": "Information"
  },
  "Processing": {
    "MaxParallelOperations": 4,
    "UseGPUAcceleration": true,
    "TimeoutSeconds": 300,
    "EnablePrecision": "float32"
  },
  "Storage": {
    "InputDirectory": "./input/",
    "OutputDirectory": "./output/",
    "TempDirectory": "./temp/"
  },
  "Cache": {
    "EnableDistributedCache": true,
    "CacheTTLSeconds": 3600,
    "MaxCacheSize": 1073741824
  },
  "Device": {
    "PreferredDeviceType": "GPU",
    "AllowFallbackToCPU": true
  }
}
```

## Performance Profiles

### Speed-Optimized Profile

**Use Case**: Real-time processing, high throughput requirements

```csharp
var profile = new ProcessingProfile
{
    Name = "SpeedOptimized",
    MaxParallelOperations = 8,
    BatchSize = 20,
    EnablePrecision = "float16",
    CacheResults = false,
    SkipValidation = false
};
```

**Characteristics**:
- Float16 precision for faster computation
- 8 parallel operations
- Batch size of 20 images
- No result caching
- ~30% faster processing

### Quality-Optimized Profile

**Use Case**: High-quality output, offline processing

```csharp
var profile = new ProcessingProfile
{
    Name = "QualityOptimized",
    MaxParallelOperations = 1,
    BatchSize = 5,
    EnablePrecision = "float32",
    CacheResults = true,
    SkipValidation = true
};
```

**Characteristics**:
- Full float32 precision
- Sequential processing
- Small batches for quality control
- Result caching enabled
- ~40% slower but higher quality

### Balanced Profile (Default)

**Use Case**: General-purpose processing

```csharp
var profile = new ProcessingProfile
{
    Name = "Balanced",
    MaxParallelOperations = 4,
    BatchSize = 10,
    EnablePrecision = "float32",
    CacheResults = true,
    SkipValidation = false
};
```

**Characteristics**:
- Float32 precision
- 4 parallel operations
- Batch size of 10
- Selective caching
- Balanced speed/quality

## Troubleshooting

### GPU Not Detected

**Problem**: "No OpenCL devices found"

**Solutions**:
1. Verify GPU drivers are installed and up-to-date
   ```bash
   # NVIDIA
   nvidia-smi
   
   # AMD
   rocm-smi
   ```

2. Check OpenCL libraries:
   ```bash
   # Linux
   ldconfig -p | grep OpenCL
   ```

3. Set `AllowFallbackToCPU = true` in settings:
   ```csharp
   settings.Device.AllowFallbackToCPU = true;
   ```

### Out of Memory Errors

**Problem**: "GPU out of memory" during batch processing

**Solutions**:
1. Reduce batch size:
   ```csharp
   settings.Processing.BatchSize = 5;
   ```

2. Reduce image dimensions or use compression

3. Implement result caching to free GPU memory

### Processing Timeout

**Problem**: Processing operation exceeds timeout

**Solutions**:
1. Increase timeout:
   ```csharp
   settings.Processing.TimeoutSeconds = 600;
   ```

2. Reduce image complexity or size

3. Use speed-optimized profile

4. Reduce parallel operations:
   ```csharp
   settings.Processing.MaxParallelOperations = 2;
   ```

### Performance Issues

**Problem**: Slower than expected processing

**Diagnosis**:
1. Check device utilization:
   ```csharp
   var metrics = await performanceService.GetMetricsAsync();
   Console.WriteLine($"GPU Utilization: {metrics.GpuUtilization}%");
   ```

2. Profile the code:
   - Use `PerformanceMonitoringService` to identify bottlenecks
   - Check if CPU-bound operations are blocking

3. Optimization steps:
   - Use speed-optimized profile
   - Increase batch size
   - Pre-allocate GPU memory
   - Use distributed processing

### Docker GPU Support

**Problem**: GPU not accessible in Docker container

**Solution**:
```bash
# Run with GPU support
docker run --gpus all -it gpu-image-processing:latest

# Or in docker-compose.yml
services:
  app:
    runtime: nvidia
    environment:
      - NVIDIA_VISIBLE_DEVICES=all
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov

# Run a specific test class
dotnet test --filter "FullyQualifiedName~FilterServiceTests"

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

Tests are organized under `tests/gpu-image-processing.Tests/` and cover domain models, filter chains, and service-layer behavior. GPU-specific paths fall back to CPU simulation when no OpenCL device is available, so the suite runs in any CI environment.

## Related Projects

- [ffmpeg-dotnet-wrapper](https://github.com/sarmkadan/ffmpeg-dotnet-wrapper) - Strongly-typed FFmpeg wrapper for .NET - transcode, trim, merge, watermark with fluent API

### Integration Examples

**Extract video frames and apply GPU filters:**

```csharp
// Use ffmpeg-dotnet-wrapper to pull frames, then sharpen each one with GPU acceleration
var frames = await ffmpeg.ExtractFramesAsync("input.mp4", fps: 24, outputDir: "./frames/");
foreach (var frame in frames)
{
    var image = await imageService.RegisterImageAsync(frame.Path, frame.FileName);
    var result = await imageService.ProcessImageAsync(image.Id, filterIds, transformIds, profileId);
    processedPaths.Add(result.OutputPath);
}
await ffmpeg.EncodeFromFramesAsync(processedPaths, outputPath: "sharpened.mp4", fps: 24);
```

**Watermark via FFmpeg then GPU color-correct:**

```csharp
// Stamp a watermark with ffmpeg-dotnet-wrapper, then pass the frame through GPU color correction
var watermarked = await ffmpeg.AddWatermarkAsync("raw.mp4", "logo.png", position: WatermarkPosition.BottomRight);
var image = await imageService.RegisterImageAsync(watermarked.OutputPath, "Watermarked");
var colorCorrect = await filterService.CreateFilterAsync(FilterType.ColorBalance, "ColorCorrect", "Color grading");
await filterService.UpdateFilterParametersAsync(colorCorrect.Id, new() { { "Saturation", 1.2f }, { "Brightness", 0.05f } });
var result = await imageService.ProcessImageAsync(image.Id, new[] { colorCorrect.Id }, Array.Empty<Guid>(), profileId);
```

## Contributing

Contributions are welcome! Please follow these guidelines:

### Code Standards

1. Follow C# naming conventions (PascalCase for public members, camelCase for private)
2. Include XML documentation on all public methods
3. Add unit tests for new functionality
4. Ensure performance-critical code is optimized

### Pull Request Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request with detailed description

### Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter TestClass=GpuImageProcessing.Tests.ImageProcessingTests
```

### Code Style

Use the provided `.editorconfig` file. Format code with:

```bash
dotnet format
```

## Performance Benchmarks

### Library Micro-benchmarks

Run the benchmark suite locally:

```bash
dotnet run -c Release --project benchmarks/gpu-image-processing.Benchmarks
```

**Environment**: .NET 10.0 · X64 RyuJIT AVX2 · `[MemoryDiagnoser]` enabled

#### Filter Chain Operations

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `AddStep_TenFilters` | 1.84 μs | 0.023 μs | 0.021 μs | 2.19 KB |
| `GetEnabledSteps_TenSteps` | 183 ns | 1.9 ns | 1.8 ns | 312 B |
| `Validate_TenSteps` | 198 ns | 2.1 ns | 2.0 ns | 312 B |
| `GetEnabledFilterCount` | 24 ns | 0.3 ns | 0.3 ns | - |
| `Clone_TenStepChain` | 1.12 μs | 0.015 μs | 0.014 μs | 1.73 KB |

#### Image Utilities

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `IsSupportedImageFile_Jpeg` | 41 ns | 0.5 ns | 0.5 ns | - |
| `IsSupportedImageFile_WebP` | 43 ns | 0.6 ns | 0.5 ns | - |
| `IsSupportedImageFile_Unsupported` | 38 ns | 0.4 ns | 0.4 ns | - |
| `FormatFileSize_Megabytes` | 57 ns | 0.8 ns | 0.7 ns | 56 B |
| `FormatFileSize_Gigabytes` | 62 ns | 0.9 ns | 0.8 ns | 64 B |
| `GetMimeType_Jpeg` | 7 ns | 0.1 ns | 0.1 ns | - |
| `GetImageFormat_Tiff` | 8 ns | 0.1 ns | 0.1 ns | - |
| `CalculateProportionalSize_2x` | 3 ns | 0.0 ns | 0.0 ns | - |

#### Enumerable Extensions

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `Shuffle_32Items` | 392 ns | 4.8 ns | 4.5 ns | 352 B |
| `Shuffle_1024Items` | 8.87 μs | 0.11 μs | 0.10 μs | 8.27 KB |
| `Batch_1000By32` | 11.9 μs | 0.15 μs | 0.14 μs | 4.22 KB |
| `Batch_1000By8` | 12.7 μs | 0.16 μs | 0.15 μs | 16.4 KB |
| `DistinctBy_1000Strings` | 18.4 μs | 0.23 μs | 0.21 μs | 3.77 KB |
| `SafeToDictionary_1000Items` | 22.8 μs | 0.29 μs | 0.27 μs | 48.1 KB |

---

### Hardware Configuration
- GPU: NVIDIA RTX 3080 (10GB VRAM)
- CPU: Intel i9-11900K @ 5.2GHz
- RAM: 64GB DDR4-3200
- OS: Ubuntu 22.04 LTS, .NET 10.0

### Single-Image Latency

| Operation | Image Size | GPU Time | CPU Time | Speedup |
|-----------|-----------|----------|----------|---------|
| Gaussian Blur (σ=2.0) | 1920×1080 | 2.5ms | 45ms | 18x |
| Sobel Edge Detection | 1920×1080 | 3.2ms | 60ms | 19x |
| Median Filter (5×5) | 1920×1080 | 5.1ms | 120ms | 24x |
| Bilateral Filter | 1920×1080 | 8.3ms | 190ms | 23x |
| Histogram Equalization | 1920×1080 | 1.1ms | 18ms | 16x |
| Resize (2×) + Rotate (45°) | 1920×1080 | 4.7ms | 82ms | 17x |

### Batch Throughput

| Batch Size | Image Size | GPU Throughput | CPU Throughput |
|------------|-----------|---------------|---------------|
| 100 images | 1920×1080 | ~400 img/s | ~22 img/s |
| 100 images | 3840×2160 | ~95 img/s | ~5 img/s |
| 1000 images | 1280×720 | ~950 img/s | ~55 img/s |

- Filter chain (3 filters) completes in **<15ms** per 1080p image on GPU
- End-to-end pipeline (load → process → cache → export) averages **~8ms** at 1080p
- Cold-start (first inference, JIT compile of OpenCL kernels) adds ~200ms one-time overhead
- Distributed cache hit reduces repeat processing to **<1ms**

## License

MIT License

Copyright (c) 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

## Acknowledgments

- OpenCL specifications and documentation
- Silk.NET for OpenCL bindings
- .NET Foundation and C# language team
- Community contributors and feedback

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
