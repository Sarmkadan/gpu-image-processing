# GPU Image Processing

A high-performance GPU-accelerated image processing library in C# using OpenCL. This project provides a comprehensive framework for applying filters, transformations, and batch operations to images with GPU acceleration.

## Features

- **GPU Acceleration**: Leverages OpenCL for high-speed image processing
- **Multiple Filter Types**: Gaussian blur, bilateral, median, Sobel, Canny, and more
- **Geometric Transforms**: Rotation, resizing, color space conversion, normalization
- **Batch Processing**: Process multiple images with job queuing and progress tracking
- **Device Management**: Automatic detection and selection of compute devices (GPU/CPU)
- **Customizable Profiles**: Speed-optimized, quality-optimized, and balanced processing profiles
- **Comprehensive Statistics**: Track performance metrics and processing results
- **Asynchronous Operations**: Non-blocking image processing with async/await

## Architecture

The project is organized into layers:

- **Core Models**: Image, Filter, Transform, ProcessingJob, DeviceInfo, ProcessingProfile
- **Services**: ImageProcessingService, FilterService, TransformService, BatchProcessingService, DeviceService
- **Repository Layer**: Generic CRUD operations with specialized repositories for images, jobs, and results
- **Configuration**: Dependency injection, application settings, and environment configuration

## Requirements

- .NET 10.0 or higher
- OpenCL 1.2+ compatible device/driver
- 2GB+ available RAM for GPU acceleration
- Supported image formats: JPEG, PNG, BMP, TIFF, WebP

## Building

```bash
dotnet build -c Release
```

## Running

```bash
dotnet run
```

## API Examples

### Register an Image
```csharp
var imageProcessing = serviceProvider.GetRequiredService<ImageProcessingService>();
var image = await imageProcessing.RegisterImageAsync("path/to/image.jpg", "MyImage");
```

### Create and Apply Filters
```csharp
var filterService = serviceProvider.GetRequiredService<FilterService>();
var filter = await filterService.CreateFilterAsync(FilterType.Gaussian, "Blur", "Gaussian blur");
await filterService.UpdateFilterParametersAsync(filter.Id, new Dictionary<string, float> 
{
    { "Sigma", 1.5f }
});
```

### Process Images
```csharp
var result = await imageProcessing.ProcessImageAsync(
    imageId,
    new List<Guid> { filterId },
    new List<Guid> { transformId },
    profileId
);
```

### Batch Processing
```csharp
var batchService = serviceProvider.GetRequiredService<BatchProcessingService>();
var job = await batchService.CreateJobAsync("BatchJob", imageIds, filterIds, transformIds, profileId);
await batchService.ExecuteJobAsync(job.Id, profileId);
```

## Configuration

Application settings can be configured in code:

```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();
settings.Processing.MaxParallelOperations = 8;
settings.Processing.UseGPUAcceleration = true;
settings.Storage.OutputDirectory = "./output/";
```

## Performance Profiles

### Speed Optimized
- Fastest processing with reduced quality
- Uses float16 precision
- Higher parallel operations
- Larger batch sizes

### Quality Optimized
- Highest quality results with reduced speed
- Full float32 precision
- Lower parallel operations
- Smaller batch sizes
- Caches intermediate results

### Balanced
- Default profile balancing speed and quality
- float32 precision
- 4 parallel operations
- Batch size of 10

## License

MIT License - See LICENSE file for details

## Author

Vladyslav Zaiets  
CTO & Software Architect  
https://sarmkadan.com

## Contributing

Contributions are welcome. Please ensure:
- Code follows the existing style and patterns
- All public methods have XML documentation
- Performance-critical code is optimized for GPU execution
- Tests are included for new features

---

For more information and documentation, visit: https://sarmkadan.com
