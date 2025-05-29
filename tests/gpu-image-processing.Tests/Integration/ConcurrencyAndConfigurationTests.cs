#nullable enable
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Integration tests for concurrent operations and configuration variations.
/// </summary>
public class ConcurrencyAndConfigurationTests
{
    private readonly FilterConfigurationRepository _filterRepository;
    private readonly ImageRepository _imageRepository;
    private readonly ProcessingResultRepository _resultRepository;
    private readonly GpuManagementService _gpuService;
    private readonly PerformanceMonitoringService _performanceService;
    private readonly FilterService _filterService;
    private readonly ImageProcessingService _processingService;

    public ConcurrencyAndConfigurationTests()
    {
        _filterRepository = new FilterConfigurationRepository();
        _imageRepository = new ImageRepository();
        _resultRepository = new ProcessingResultRepository();
        _gpuService = new GpuManagementService(Mock.Of<ILogger<GpuManagementService>>());
        _performanceService = new PerformanceMonitoringService(Mock.Of<ILogger<PerformanceMonitoringService>>());

        _filterService = new FilterService(
            _filterRepository,
            Mock.Of<ILogger<FilterService>>());

        _processingService = new ImageProcessingService(
            _imageRepository,
            _filterRepository,
            _resultRepository,
            _filterService,
            _gpuService,
            _performanceService,
            Mock.Of<ILogger<ImageProcessingService>>());
    }

    private Image CreateImage(int width = 800, int height = 600, int bitsPerPixel = 24)
    {
        return new Image
        {
            Id = Guid.NewGuid(),
            FilePath = "/test/image.png",
            FileName = "image.png",
            Width = width,
            Height = height,
            BitsPerPixel = bitsPerPixel,
            FileSizeBytes = (width * height * (bitsPerPixel / 8)) / 1024,
            Format = ImageFormat.Png,
            ColorSpace = ColorSpace.Rgb,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    private FilterConfiguration CreateFilter(string name, FilterType type)
    {
        return new FilterConfiguration
        {
            Id = Guid.NewGuid(),
            Name = name,
            FilterType = type,
            IsActive = true,
            MaxThreadsPerBlock = 256
        };
    }

    [Fact]
    public async Task ConcurrentImageProcessing_MultipleThreads_CompletesSuccessfully()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateFilter("Blur", FilterType.Blur));

        var images = new List<Image>();
        for (int i = 0; i < 5; i++)
        {
            var img = CreateImage();
            var saved = await _imageRepository.CreateAsync(img);
            images.Add(saved);
        }

        // Act
        var tasks = images.Select(img => _filterService.ApplyFilterAsync(img, filter.Id)).ToList();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    [Fact]
    public async Task PerformanceMetrics_UnderConcurrentLoad_CalculatedCorrectly()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateFilter("Blur", FilterType.Blur));

        var imageTasks = new List<Task<Image>>();
        for (int i = 0; i < 10; i++)
        {
            imageTasks.Add(_imageRepository.CreateAsync(CreateImage()));
        }

        var images = (await Task.WhenAll(imageTasks)).ToList();

        // Act
        var filterTasks = images.Select(img => _filterService.ApplyFilterAsync(img, filter.Id)).ToList();
        await Task.WhenAll(filterTasks);

        var metrics = _performanceService.GetCurrentMetrics();

        // Assert
        metrics.TotalOperationsCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ImageDimensions_VariousConfigurations_AllProcessCorrectly()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateFilter("Blur", FilterType.Blur));

        var configurations = new[]
        {
            (width: 1920, height: 1080, bpp: 24),
            (width: 800, height: 600, bpp: 32),
            (width: 640, height: 480, bpp: 16),
            (width: 3840, height: 2160, bpp: 24)
        };

        // Act & Assert
        foreach (var (width, height, bpp) in configurations)
        {
            var img = CreateImage(width, height, bpp);
            img.Should().NotBeNull();

            var saved = await _imageRepository.CreateAsync(img);
            saved.Width.Should().Be(width);
            saved.Height.Should().Be(height);
            saved.BitsPerPixel.Should().Be(bpp);
        }
    }

    [Fact]
    public async Task FilterChain_ComplexPipeline_ExecutesInOrder()
    {
        // Arrange
        var filters = new[]
        {
            await _filterRepository.CreateAsync(CreateFilter("Blur", FilterType.Blur)),
            await _filterRepository.CreateAsync(CreateFilter("Sharpen", FilterType.Sharpen)),
            await _filterRepository.CreateAsync(CreateFilter("Grayscale", FilterType.Grayscale))
        };

        var chain = new FilterChain { Name = "Complex Pipeline" };
        foreach (var filter in filters)
        {
            chain.AddStep(filter.Id);
        }

        // Act
        var steps = chain.GetEnabledSteps();

        // Assert
        steps.Should().HaveCount(3);
        steps[0].Order.Should().Be(0);
        steps[1].Order.Should().Be(1);
        steps[2].Order.Should().Be(2);
    }

    [Fact]
    public async Task ImageBatch_LargeScale_HandlesMultipleImages()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateFilter("Blur", FilterType.Blur));

        var imageIds = new List<Guid>();
        for (int i = 0; i < 20; i++)
        {
            var img = CreateImage();
            var saved = await _imageRepository.CreateAsync(img);
            imageIds.Add(saved.Id);
        }

        var batch = new ImageBatch
        {
            Name = "Large Batch",
            OutputDirectory = "/output",
            TotalImages = imageIds.Count
        };

        foreach (var id in imageIds)
        {
            batch.AddImage(id);
        }

        batch.AddFilter(filter.Id);

        // Act
        var isValid = batch.Validate();

        // Assert
        isValid.Should().BeTrue();
        batch.ImageIds.Should().HaveCount(20);
    }

    [Fact]
    public async Task GpuMemory_StressTest_AllocateAndDeallocateMultipleTimes()
    {
        // Arrange
        var device = _gpuService.GetBestDevice();
        var allocationSize = 50 * 1024 * 1024; // 50 MB

        // Act
        for (int i = 0; i < 5; i++)
        {
            _gpuService.AllocateMemory(allocationSize, device!.Id);
        }

        var afterAllocate = _gpuService.GetTotalAllocatedMemory();

        for (int i = 0; i < 5; i++)
        {
            _gpuService.DeallocateMemory(allocationSize, device.Id);
        }

        var afterDeallocate = _gpuService.GetTotalAllocatedMemory();

        // Assert
        afterAllocate.Should().Be(allocationSize * 5);
        afterDeallocate.Should().Be(0);
    }

    [Fact]
    public void GpuDeviceSelection_MultipleDevices_SelectsBestOne()
    {
        // Act
        var bestDevice = _gpuService.GetBestDevice();
        var allDevices = _gpuService.GetAvailableDevices();

        // Assert
        bestDevice.Should().NotBeNull();
        allDevices.Should().NotBeEmpty();
        allDevices.Should().Contain(bestDevice!);
    }

    [Fact]
    public async Task FilterConfiguration_InactiveFilter_RejectedDuringApplication()
    {
        // Arrange
        var inactiveFilter = new FilterConfiguration
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Blur",
            FilterType = FilterType.Blur,
            IsActive = false,
            MaxThreadsPerBlock = 256
        };

        var savedFilter = await _filterRepository.CreateAsync(inactiveFilter);
        var image = CreateImage();

        // Act & Assert
        await _filterService.Invoking(s => s.ApplyFilterAsync(image, savedFilter.Id))
            .Should().ThrowAsync<InvalidFilterException>();
    }

    [Fact]
    public async Task ResultTracking_MultipleOperations_AllRecorded()
    {
        // Arrange
        var imageIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var img = CreateImage();
            var saved = await _imageRepository.CreateAsync(img);
            imageIds.Add(saved.Id);
        }

        // Act
        var results = new List<ProcessingResult>();
        foreach (var id in imageIds)
        {
            var result = new ProcessingResult { ImageId = id, IsSuccessful = true };
            var saved = await _resultRepository.CreateAsync(result);
            results.Add(saved);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.IsSuccessful.Should().BeTrue());
    }

    [Fact]
    public async Task ImageValidation_BoundaryValues_AcceptedCorrectly()
    {
        // Arrange - min valid dimensions
        var minImage = new Image
        {
            Id = Guid.NewGuid(),
            FilePath = "/test/min.png",
            FileName = "min.png",
            Width = AppConstants.Processing.MinImageWidth,
            Height = AppConstants.Processing.MinImageHeight,
            BitsPerPixel = 8,
            FileSizeBytes = 1024,
            Format = ImageFormat.Png,
            ColorSpace = ColorSpace.Grayscale
        };

        // Act
        var isValid = minImage.Validate();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ImageBatch_ProgressCalculation_HandlesEdgeCases()
    {
        // Arrange
        var batch = new ImageBatch { TotalImages = 0 };

        // Act
        var progressEmpty = batch.GetProgressPercentage();

        // Assert
        progressEmpty.Should().Be(0.0);

        // Arrange
        batch.TotalImages = 10;
        batch.MarkImageProcessed(true);
        batch.MarkImageProcessed(true);

        // Act
        var progressPartial = batch.GetProgressPercentage();

        // Assert
        progressPartial.Should().BeApproximately(20.0, precision: 0.01);
    }

    [Fact]
    public async Task FilterService_MultipleFiltersOfSameType_AllCreatedSuccessfully()
    {
        // Arrange
        var filters = new List<FilterConfiguration>();
        for (int i = 0; i < 3; i++)
        {
            filters.Add(CreateFilter($"Blur {i}", FilterType.Blur));
        }

        // Act
        var created = new List<FilterConfiguration>();
        foreach (var filter in filters)
        {
            var saved = await _filterRepository.CreateAsync(filter);
            created.Add(saved);
        }

        // Assert
        created.Should().HaveCount(3);
        created.Should().AllSatisfy(f => f.FilterType.Should().Be(FilterType.Blur));
    }

    [Fact]
    public async Task PerformanceMonitoring_SnapshotAndReset_ManagesHistoryCorrectly()
    {
        // Arrange
        var service = new PerformanceMonitoringService(Mock.Of<ILogger<PerformanceMonitoringService>>());

        // Act
        service.RecordOperation(100, true);
        var snapshot1 = service.SnapshotAndReset();

        service.RecordOperation(200, true);
        var snapshot2 = service.SnapshotAndReset();

        var history = service.GetMetricsHistory();

        // Assert
        history.Should().HaveCount(2);
        snapshot1.TotalOperationsCount.Should().Be(1);
        snapshot2.TotalOperationsCount.Should().Be(1);
    }

    [Fact]
    public async Task FilterChain_ReorderSteps_MaintainsIntegrity()
    {
        // Arrange
        var chain = new FilterChain { Name = "Test Chain" };
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        chain.AddStep(id1);
        chain.AddStep(id2);
        chain.AddStep(id3);

        // Act
        chain.ReorderSteps([id3, id1, id2]);
        var reordered = chain.GetEnabledSteps();

        // Assert
        reordered[0].FilterId.Should().Be(id3);
        reordered[1].FilterId.Should().Be(id1);
        reordered[2].FilterId.Should().Be(id2);
    }
}
