#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Performance benchmarks for GPU-accelerated image processing operations
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Benchmarks for GPU-accelerated image processing operations.
/// Measures throughput and latency of critical GPU operations including:
/// - Single filter application (Gaussian blur, edge detection, etc.)
/// - Multiple filter chains (realistic workflows)
/// - Batch processing throughput
/// - Memory allocation patterns
/// - Device initialization overhead
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class GpuPerformanceBenchmarks
{
    private Image _testImage = null!;
    private FilterConfiguration _gaussianBlurFilter = null!;
    private FilterConfiguration _edgeDetectionFilter = null!;
    private FilterConfiguration _sharpenFilter = null!;
    private FilterConfiguration _grayscaleFilter = null!;
    private FilterConfiguration _customConvolutionFilter = null!;
    private ImageProcessingService _imageProcessingService = null!;
    private FilterService _filterService = null!;
    private FilterConfigurationRepository _filterRepository = null!;
    private ImageRepository _imageRepository = null!;
    private GpuManagementService _gpuService = null!;
    private PerformanceMonitoringService _performanceService = null!;
    private ILogger<ImageProcessingService> _logger = null!;
    private ILogger<FilterService> _filterLogger = null!;
    private ILogger<GpuManagementService> _gpuLogger = null!;

    [Params(1920, 3840)]
    public int ImageWidth { get; set; }

    [Params(1080, 2160)]
    public int ImageHeight { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _logger.LogInformation("Setup started");
        // Initialize services for benchmarking
        var serviceProvider = BenchmarkServiceProvider.CreateServiceProvider();

        _imageProcessingService = serviceProvider.GetRequiredService<ImageProcessingService>();
        _filterService = serviceProvider.GetRequiredService<FilterService>();
        _filterRepository = serviceProvider.GetRequiredService<FilterConfigurationRepository>();
        _imageRepository = serviceProvider.GetRequiredService<ImageRepository>();
        _gpuService = serviceProvider.GetRequiredService<GpuManagementService>();
        _performanceService = serviceProvider.GetRequiredService<PerformanceMonitoringService>();
        _logger = serviceProvider.GetRequiredService<ILogger<ImageProcessingService>>();
        _filterLogger = serviceProvider.GetRequiredService<ILogger<FilterService>>();
        _gpuLogger = serviceProvider.GetRequiredService<ILogger<GpuManagementService>>();

        // Create test image
        _testImage = new Image
        {
            FilePath = "/tmp/test-image.jpg",
            FileName = "test-image.jpg",
            Format = ImageFormat.Jpeg,
            ColorSpace = ColorSpace.Rgb,
            Width = ImageWidth,
            Height = ImageHeight,
            BitsPerPixel = 24,
            FileSizeBytes = ImageWidth * ImageHeight * 3,
            Status = ProcessingStatus.Pending
        };

        // Create filters for benchmarking
        _gaussianBlurFilter = new FilterConfiguration
        {
            Name = "GaussianBlurBenchmark",
            FilterType = FilterType.GaussianBlur,
            Description = "Gaussian blur filter for performance testing",
            IsActive = true,
            Priority = 1,
            Parameters = new Dictionary<string, object> { { "radius", 2.5f } },
            ParameterTypes = new Dictionary<string, string> { { "radius", "float" } }
        };

        _edgeDetectionFilter = new FilterConfiguration
        {
            Name = "EdgeDetectionBenchmark",
            FilterType = FilterType.EdgeDetection,
            Description = "Edge detection filter for performance testing",
            IsActive = true,
            Priority = 2,
            Parameters = new Dictionary<string, object>(),
            ParameterTypes = new Dictionary<string, string>()
        };

        _sharpenFilter = new FilterConfiguration
        {
            Name = "SharpenBenchmark",
            FilterType = FilterType.Sharpen,
            Description = "Sharpen filter for performance testing",
            IsActive = true,
            Priority = 3,
            Parameters = new Dictionary<string, object> { { "strength", 1.5f } },
            ParameterTypes = new Dictionary<string, string> { { "strength", "float" } }
        };

        _grayscaleFilter = new FilterConfiguration
        {
            Name = "GrayscaleBenchmark",
            FilterType = FilterType.Grayscale,
            Description = "Grayscale conversion filter",
            IsActive = true,
            Priority = 4,
            Parameters = new Dictionary<string, object>(),
            ParameterTypes = new Dictionary<string, string>()
        };

        // Create a 3x3 convolution kernel (edge detection)
        _customConvolutionFilter = new FilterConfiguration
        {
            Name = "CustomConvolutionBenchmark",
            FilterType = FilterType.CustomConvolution,
            Description = "Custom convolution kernel for edge detection",
            IsActive = true,
            Priority = 5,
            Parameters = new Dictionary<string, object>(),
            ParameterTypes = new Dictionary<string, string>(),
            ConvolutionKernel = new float[] { -1, -1, -1, -1, 8, -1, -1, -1, -1 },
            NormalizeKernel = true
        };

        // Register filters
        _filterRepository.CreateAsync(_gaussianBlurFilter).Wait();
        _filterRepository.CreateAsync(_edgeDetectionFilter).Wait();
        _filterRepository.CreateAsync(_sharpenFilter).Wait();
        _filterRepository.CreateAsync(_grayscaleFilter).Wait();
        _filterRepository.CreateAsync(_customConvolutionFilter).Wait();

        // Register test image
        _imageRepository.CreateAsync(_testImage).Wait();
        _logger.LogInformation("Setup completed");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _logger.LogInformation("Cleanup started");
        // Clean up test data
        _imageRepository.DeleteAsync(_testImage.Id).Wait();
        _filterRepository.DeleteAsync(_gaussianBlurFilter.Id).Wait();
        _filterRepository.DeleteAsync(_edgeDetectionFilter.Id).Wait();
        _filterRepository.DeleteAsync(_sharpenFilter.Id).Wait();
        _filterRepository.DeleteAsync(_grayscaleFilter.Id).Wait();
        _filterRepository.DeleteAsync(_customConvolutionFilter.Id).Wait();
        _logger.LogInformation("Cleanup completed");
    }

    #region Single Filter Performance

    /// <summary>
    /// Measures the latency of applying a single Gaussian blur filter.
    /// This is a fundamental GPU operation that exercises memory allocation,
    /// kernel execution, and data transfer.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Single Filter", "Gaussian Blur")]
    public async Task<ProcessingResult> ApplyGaussianBlurFilter()
    {
        _logger.LogInformation("ApplyGaussianBlurFilter called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> { _gaussianBlurFilter.Id }
        );
        _logger.LogInformation("ApplyGaussianBlurFilter completed");
        return result;
    }

    /// <summary>
    /// Measures the latency of applying a single edge detection filter.
    /// Edge detection is computationally intensive and tests GPU parallelism.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Single Filter", "Edge Detection")]
    public async Task<ProcessingResult> ApplyEdgeDetectionFilter()
    {
        _logger.LogInformation("ApplyEdgeDetectionFilter called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> { _edgeDetectionFilter.Id }
        );
        _logger.LogInformation("ApplyEdgeDetectionFilter completed");
        return result;
    }

    /// <summary>
    /// Measures the latency of applying a sharpen filter.
    /// Sharpening involves convolution operations that test GPU texture sampling.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Single Filter", "Sharpen")]
    public async Task<ProcessingResult> ApplySharpenFilter()
    {
        _logger.LogInformation("ApplySharpenFilter called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> { _sharpenFilter.Id }
        );
        _logger.LogInformation("ApplySharpenFilter completed");
        return result;
    }

    /// <summary>
    /// Measures the latency of applying a grayscale conversion filter.
    /// Simple color space conversion that tests GPU memory bandwidth.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Single Filter", "Grayscale")]
    public async Task<ProcessingResult> ApplyGrayscaleFilter()
    {
        _logger.LogInformation("ApplyGrayscaleFilter called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> { _grayscaleFilter.Id }
        );
        _logger.LogInformation("ApplyGrayscaleFilter completed");
        return result;
    }

    /// <summary>
    /// Measures the latency of applying a custom convolution filter.
    /// Tests GPU kernel compilation and execution with user-defined kernels.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Single Filter", "Custom Convolution")]
    public async Task<ProcessingResult> ApplyCustomConvolutionFilter()
    {
        _logger.LogInformation("ApplyCustomConvolutionFilter called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> { _customConvolutionFilter.Id }
        );
        _logger.LogInformation("ApplyCustomConvolutionFilter completed");
        return result;
    }

    #endregion

    #region Filter Chain Performance

    /// <summary>
    /// Measures the latency of applying a realistic 3-filter chain:
    /// Grayscale → Gaussian Blur → Sharpen
    /// This represents a common image processing workflow.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filter Chain", "3 Filters")]
    public async Task<ProcessingResult> ApplyThreeFilterChain()
    {
        _logger.LogInformation("ApplyThreeFilterChain called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> { _grayscaleFilter.Id, _gaussianBlurFilter.Id, _sharpenFilter.Id }
        );
        _logger.LogInformation("ApplyThreeFilterChain completed");
        return result;
    }

    /// <summary>
    /// Measures the latency of applying a realistic 5-filter chain:
    /// Grayscale → Gaussian Blur → Edge Detection → Sharpen → Custom Convolution
    /// This represents a more complex professional workflow.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Filter Chain", "5 Filters")]
    public async Task<ProcessingResult> ApplyFiveFilterChain()
    {
        _logger.LogInformation("ApplyFiveFilterChain called with image {ImageId}", _testImage.Id);
        var result = await _imageProcessingService.ProcessImageAsync(
            _testImage.Id,
            new List<Guid> {
                _grayscaleFilter.Id,
                _gaussianBlurFilter.Id,
                _edgeDetectionFilter.Id,
                _sharpenFilter.Id,
                _customConvolutionFilter.Id
            }
        );
        _logger.LogInformation("ApplyFiveFilterChain completed");
        return result;
    }

    #endregion

    #region Memory Allocation Benchmarks

    /// <summary>
    /// Measures GPU memory allocation and deallocation overhead.
    /// Memory operations can be a bottleneck in GPU processing.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Memory", "Allocation")]
    public long CalculatePixelDataSize()
    {
        _logger.LogInformation("CalculatePixelDataSize called");
        var result = _testImage.CalculatePixelDataSize();
        _logger.LogInformation("CalculatePixelDataSize completed");
        return result;
    }

    /// <summary>
    /// Measures the memory footprint of different image sizes.
    /// Helps identify memory bottlenecks.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Memory", "1080p")]
    public long MemoryFootprint1080p()
    {
        _logger.LogInformation("MemoryFootprint1080p called");
        var image = new Image
        {
            Width = 1920,
            Height = 1080,
            BitsPerPixel = 24
        };
        var result = image.CalculatePixelDataSize();
        _logger.LogInformation("MemoryFootprint1080p completed");
        return result;
    }

    /// <summary>
    /// Measures the memory footprint of 4K images.
    /// 4K images are common in professional applications.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Memory", "4K")]
    public long MemoryFootprint4K()
    {
        _logger.LogInformation("MemoryFootprint4K called");
        var image = new Image
        {
            Width = 3840,
            Height = 2160,
            BitsPerPixel = 24
        };
        var result = image.CalculatePixelDataSize();
        _logger.LogInformation("MemoryFootprint4K completed");
        return result;
    }

    #endregion

    #region Throughput Benchmarks

    /// <summary>
    /// Measures the throughput of processing multiple images in sequence.
    /// Tests the overhead of repeated GPU operations.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Throughput", "10 Images")]
    public async Task ProcessTenImages()
    {
        _logger.LogInformation("ProcessTenImages called");
        for (int i = 0; i < 10; i++)
        {
            await _imageProcessingService.ProcessImageAsync(
                _testImage.Id,
                new List<Guid> { _gaussianBlurFilter.Id }
            );
        }
        _logger.LogInformation("ProcessTenImages completed");
    }

    #endregion

    #region Device Capabilities

    /// <summary>
    /// Measures the time to detect and initialize GPU devices.
    /// Device initialization overhead affects cold-start performance.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Device", "Initialization")]
    public async Task<GpuDevice?> GetBestDevice()
    {
        _gpuLogger.LogInformation("GetBestDevice called");
        var result = _gpuService.GetBestDevice();
        _gpuLogger.LogInformation("GetBestDevice completed");
        return result;
    }

    public override string ToString() => $"GpuPerformanceBenchmarks {{ ImageWidth = {ImageWidth}, ImageHeight = {ImageHeight} }}";

#endregion
}

/// <summary>
/// Helper class to create a service provider for benchmarks.
/// </summary>
public static class BenchmarkServiceProvider
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add required services
        services.AddSingleton<FilterConfigurationRepository>();
        services.AddSingleton<ImageRepository>();
        services.AddSingleton<ProcessingResultRepository>();
        services.AddSingleton<FilterService>();
        services.AddSingleton<GpuManagementService>();
        services.AddSingleton<PerformanceMonitoringService>();
        services.AddSingleton<ImageProcessingService>();

        return services.BuildServiceProvider();
    }
}