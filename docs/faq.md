# Frequently Asked Questions

## General Questions

### Q: What is GPU Image Processing?

A: GPU Image Processing is a high-performance C# library that leverages OpenCL to accelerate image processing operations on GPUs. It provides filters, geometric transforms, batch processing, and device management capabilities for both real-time and offline image processing workflows.

### Q: Why should I use this library instead of alternatives?

A: 
- **Pure C# implementation** - No external dependencies
- **OpenCL support** - Works with NVIDIA, AMD, and Intel GPUs
- **Production-ready** - Comprehensive error handling and logging
- **Batch processing** - Optimized for processing thousands of images
- **Flexible** - Multiple processing profiles for different use cases
- **Open source** - MIT licensed, fully customizable

### Q: Is it free?

A: Yes! The project is released under the MIT License, which allows free use for commercial and private projects.

### Q: Can I use it commercially?

A: Yes, the MIT License permits commercial use. You're welcome to use it in production environments.

### Q: What platforms are supported?

A: Windows 10+, Windows Server 2019+, Ubuntu 20.04+, Debian 11+, and macOS 11+.

---

## Installation & Setup

### Q: Do I need a GPU to run this?

A: No. The library includes CPU fallback support. However, GPU acceleration provides significant performance improvements (10-20x faster).

### Q: What GPU do I need?

A: Any GPU with OpenCL 1.2+ support:
- NVIDIA: GTX 960+, RTX series
- AMD: Radeon RX series
- Intel: Iris Xe, Arc series

### Q: How do I install it?

A: Three methods:
1. **NuGet**: `dotnet add package GpuImageProcessing`
2. **Build from source**: Clone repository and run `dotnet build -c Release`
3. **Docker**: `docker build -t gpu-image-processing:latest .`

### Q: Why doesn't it detect my GPU?

A: Common causes:
1. GPU drivers not installed or outdated
2. OpenCL libraries not found
3. GPU disabled in BIOS
4. Wrong GPU architecture

**Solutions**:
- Update GPU drivers
- Install OpenCL headers: `apt-get install ocl-icd-opencl-dev` (Linux)
- Check BIOS settings
- Enable CPU fallback in settings

### Q: Can I use integrated graphics (iGPU)?

A: Yes, integrated graphics are supported and detected automatically. However, performance improvements are modest compared to discrete GPUs.

---

## Usage Questions

### Q: How do I process an image?

A: 
```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();
var sp = await DependencyInjectionSetup
    .CreateAndInitializeServiceProviderAsync(settings);

var imageProcessing = sp.GetRequiredService<ImageProcessingService>();
var filterService = sp.GetRequiredService<FilterService>();

var image = await imageProcessing.RegisterImageAsync("photo.jpg", "Photo");
var filter = await filterService.CreateFilterAsync(
    FilterType.Gaussian, "Blur", "");
var result = await imageProcessing.ProcessImageAsync(
    image.Id, new List<Guid> { filter.Id }, new List<Guid>(), Guid.Empty);
```

### Q: How do I apply multiple filters?

A: Create each filter and add to the list:
```csharp
var filters = new List<Guid>
{
    blurFilterId,
    sobelFilterId,
    cannyFilterId
};

var result = await imageProcessing.ProcessImageAsync(
    imageId, filters, new List<Guid>(), profileId);
```

### Q: Can I combine filters and transforms?

A: Yes, pass both lists:
```csharp
var result = await imageProcessing.ProcessImageAsync(
    imageId,
    filterIds,    // List of filter IDs
    transformIds, // List of transform IDs
    profileId);
```

### Q: How do I batch process images?

A: Use BatchProcessingService:
```csharp
var batch = serviceProvider.GetRequiredService<BatchProcessingService>();
var job = await batch.CreateJobAsync(
    "BatchJob", imageIds, filterIds, transformIds, profileId);
await batch.ExecuteJobAsync(job.Id, profileId);
```

### Q: What's the maximum image size?

A: Default is 8192×8192 pixels. This can be configured:
```csharp
settings.Security.MaxImageWidth = 8192;
settings.Security.MaxImageHeight = 8192;
```

### Q: Can I process images from URLs?

A: Yes, using the HTTP client:
```csharp
var client = sp.GetRequiredService<HttpImageClient>();
var image = await client.DownloadAndRegisterAsync(
    "https://example.com/image.jpg", "Downloaded");
```

### Q: How do I export results in different formats?

A: Use result formatters:
```csharp
var jsonFormatter = new JsonResultFormatter();
var csvFormatter = new CsvResultFormatter();
var xmlFormatter = new XmlResultFormatter();

var json = jsonFormatter.Format(result);
var csv = csvFormatter.Format(result);
var xml = xmlFormatter.Format(result);
```

---

## Performance & Optimization

### Q: Why is processing slower than expected?

A: Possible causes:
1. GPU not selected (using CPU fallback)
2. Batch size too small
3. Parallel operations too few
4. Image resolution too high
5. Complex filter chains

**Optimization**:
```csharp
settings.Processing.BatchSize = 20;
settings.Processing.MaxParallelOperations = 8;
settings.Processing.Precision = "float16";
```

### Q: What's the difference between processing profiles?

A: 
- **Speed**: Float16, 8 parallel ops, batch 20, ~30% faster
- **Quality**: Float32, 1 parallel op, batch 5, ~40% slower
- **Balanced**: Float32, 4 parallel ops, batch 10, default

Choose based on your needs.

### Q: How much GPU memory do I need?

A: Minimum 2GB, recommended 4GB+. For large batch processing, 8GB+ is ideal.

Monitor with:
```csharp
var device = await deviceService.GetCurrentDeviceAsync();
Console.WriteLine($"Memory: {device.GlobalMemoryMB}MB");
```

### Q: Can I process images larger than GPU memory?

A: Yes, the library uses tiling for large images:
```csharp
// Automatically handled, no configuration needed
// Processing is transparent to the user
```

### Q: How do I monitor performance?

A: Use PerformanceMonitoringService:
```csharp
var perfService = sp.GetRequiredService<PerformanceMonitoringService>();
var metrics = await perfService.GetMetricsAsync();

Console.WriteLine($"GPU Utilization: {metrics.GpuUtilization}%");
Console.WriteLine($"Memory Used: {metrics.MemoryUsedMB}MB");
Console.WriteLine($"Throughput: {metrics.ImagesPerSecond} img/sec");
```

### Q: What's the typical throughput?

A: On NVIDIA RTX 3080:
- Gaussian Blur (1080p): ~400 images/sec
- Sobel Edge (1080p): ~300 images/sec
- Batch (100 images): ~18x faster than CPU

---

## Troubleshooting

### Q: I'm getting "OutOfMemoryException"

A: Solutions:
1. Reduce batch size: `settings.Processing.BatchSize = 5;`
2. Reduce image dimensions
3. Enable result caching to free GPU memory
4. Use speed-optimized profile

### Q: Processing times are inconsistent

A: Possible causes:
1. GPU thermal throttling
2. Driver interference
3. System load variations
4. Cache misses

**Solutions**:
- Let GPU cool down
- Close other GPU applications
- Use exclusive GPU mode
- Warm up GPU with initial runs

### Q: OpenCL compilation errors?

A: The library includes pre-compiled kernels. If you get compilation errors:
1. Update GPU drivers
2. Verify OpenCL installation
3. Check OpenCL compiler logs
4. Try CPU fallback

### Q: Webhook callbacks not working?

A: Configuration:
```csharp
var webhookHandler = sp.GetRequiredService<WebhookHandler>();
await webhookHandler.RegisterWebhookAsync(
    "https://example.com/webhook",
    "processing.complete");
```

### Q: Docker container exits immediately?

A: Check logs:
```bash
docker logs container-id
```

Common issues:
- GPU drivers not available in container
- Missing NVIDIA Container Runtime
- Invalid configuration

**Solution**:
```bash
docker run --gpus all -it gpu-image-processing:latest
```

### Q: How do I debug issues?

A: Enable debug logging:
```csharp
settings.Logging.LogLevel = LogLevel.Debug;
settings.Logging.EnableFileLogging = true;
```

Then check logs:
```bash
tail -f logs/application.log
```

---

## Advanced Topics

### Q: Can I use custom OpenCL kernels?

A: Yes, extend FilterService:
```csharp
public class CustomFilterService : FilterService
{
    public async Task<Filter> CreateCustomFilterAsync(
        string kernelSource, string name)
    {
        // Implement custom kernel compilation
    }
}
```

### Q: How do I implement custom middleware?

A: Create a class implementing IProcessingMiddleware:
```csharp
public class CustomMiddleware : IProcessingMiddleware
{
    public async Task<MiddlewareContext> ExecuteAsync(
        MiddlewareContext context)
    {
        // Pre-processing
        await context.CallNextAsync();
        // Post-processing
        return context;
    }
}
```

Register in pipeline:
```csharp
var pipeline = new ProcessingPipeline();
pipeline.Add(new CustomMiddleware());
```

### Q: Can I process video frames?

A: Not directly, but you can process individual frames:
```csharp
using (var video = new VideoCapture("video.mp4"))
{
    while (video.Read(frame))
    {
        var image = await imageProcessing.RegisterImageAsync(
            frame, "Frame");
        var result = await imageProcessing.ProcessImageAsync(...);
    }
}
```

### Q: How do I distribute processing across multiple GPUs?

A: Use device selection:
```csharp
var devices = await deviceService.GetAvailableDevicesAsync();
foreach (var device in devices)
{
    await deviceService.SelectDeviceAsync(device.Id);
    // Process batch on this device
}
```

### Q: Can I scale to multiple machines?

A: Yes, implement distributed job processing:
```csharp
// Machine 1: Create job
var job = await batchService.CreateJobAsync(...);

// Machine 2: Process
var updated = await batchService.GetJobAsync(job.Id);
// Process assigned images
```

Requires shared database and job tracking.

---

## Integration Questions

### Q: How do I integrate with ASP.NET Core?

A: Add to Program.cs:
```csharp
var settings = ConfigurationValidator.CreateDefaultSettings();
await DependencyInjectionSetup.ConfigureServicesAsync(
    builder.Services, settings);
```

Then use in controllers:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly ImageProcessingService _service;
    
    public ProcessingController(ImageProcessingService service)
    {
        _service = service;
    }
}
```

### Q: Can I use it with Entity Framework?

A: Yes, repositories support EF:
```csharp
services.AddDbContext<ProcessingContext>(options =>
    options.UseSqlServer(connectionString));
```

### Q: How do I export metrics to Prometheus?

A: The MetricsPublisher handles this:
```csharp
var publisher = sp.GetRequiredService<MetricsPublisher>();
var metrics = await publisher.GetMetricsAsync();
// Metrics in Prometheus format
```

### Q: Can I use it with Docker?

A: Yes! See [Deployment Guide](deployment.md).

---

## Support & Contributing

### Q: Where can I get help?

A: 
- **Documentation**: See [docs/](.) directory
- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions
- **Email**: Via GitHub profile

### Q: How can I contribute?

A: 
1. Fork the repository
2. Create feature branch
3. Make changes
4. Submit pull request

See [README.md](../README.md#contributing) for guidelines.

### Q: Is there a roadmap?

A: Check GitHub Issues with "roadmap" label for planned features.

### Q: Can I report bugs?

A: Yes! Create GitHub Issue with:
- Description
- Steps to reproduce
- Expected behavior
- Actual behavior
- GPU model and drivers

---

Still have questions? Open an issue on GitHub or check the documentation!
