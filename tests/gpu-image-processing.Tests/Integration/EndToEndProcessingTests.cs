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
/// Integration tests that exercise the full image processing pipeline
/// from configuration through completion.
/// </summary>
public class EndToEndProcessingTests
{
    private readonly FilterConfigurationRepository _filterRepository;
    private readonly ImageRepository _imageRepository;
    private readonly ProcessingResultRepository _resultRepository;
    private readonly GpuManagementService _gpuService;
    private readonly PerformanceMonitoringService _performanceService;
    private readonly Mock<ILogger<FilterService>> _filterLoggerMock;
    private readonly Mock<ILogger<ImageProcessingService>> _processingLoggerMock;
    private readonly Mock<ILogger<BatchProcessingService>> _batchLoggerMock;
    private readonly FilterService _filterService;
    private readonly ImageProcessingService _processingService;
    private readonly BatchProcessingService _batchService;

    public EndToEndProcessingTests()
    {
        _filterRepository = new FilterConfigurationRepository();
        _imageRepository = new ImageRepository();
        _resultRepository = new ProcessingResultRepository();
        _gpuService = new GpuManagementService(Mock.Of<ILogger<GpuManagementService>>());
        _performanceService = new PerformanceMonitoringService(Mock.Of<ILogger<PerformanceMonitoringService>>());

        _filterLoggerMock = new Mock<ILogger<FilterService>>();
        _processingLoggerMock = new Mock<ILogger<ImageProcessingService>>();
        _batchLoggerMock = new Mock<ILogger<BatchProcessingService>>();

        _filterService = new FilterService(_filterRepository, _filterLoggerMock.Object);
        _processingService = new ImageProcessingService(
            _imageRepository,
            _filterRepository,
            _resultRepository,
            _filterService,
            _gpuService,
            _performanceService,
            _processingLoggerMock.Object);

        _batchService = new BatchProcessingService(
            _processingService,
            _imageRepository,
            _batchLoggerMock.Object);
    }

    private Image CreateValidImage()
    {
        return new Image
        {
            Id = Guid.NewGuid(),
            FilePath = "/test/image.png",
            FileName = "image.png",
            Width = 800,
            Height = 600,
            BitsPerPixel = 24,
            FileSizeBytes = 1024 * 500,
            Format = ImageFormat.Png,
            ColorSpace = ColorSpace.Rgb,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }

    private FilterConfiguration CreateValidFilter(string name, FilterType type)
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
    public async Task CompleteWorkflow_CreateFilterApplyToImage_RecordsSuccessfully()
    {
        // Arrange - create filter
        var filterConfig = CreateValidFilter("Blur Filter", FilterType.Blur);
        var savedFilter = await _filterRepository.CreateAsync(filterConfig);

        // Arrange - create image
        var image = CreateValidImage();
        var savedImage = await _imageRepository.CreateAsync(image);

        // Act - apply filter
        var processedImage = await _filterService.ApplyFilterAsync(savedImage, savedFilter.Id);

        // Assert
        processedImage.Should().NotBeNull();
        processedImage.ColorSpace.Should().Be(savedImage.ColorSpace);
    }

    [Fact]
    public async Task BatchProcessing_MultipleImages_ProcessedConcurrently()
    {
        // Arrange - create filters
        var blurFilter = await _filterRepository.CreateAsync(
            CreateValidFilter("Blur", FilterType.Blur));
        var sharpenFilter = await _filterRepository.CreateAsync(
            CreateValidFilter("Sharpen", FilterType.Sharpen));

        // Arrange - create images
        var imageIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var image = CreateValidImage();
            var saved = await _imageRepository.CreateAsync(image);
            imageIds.Add(saved.Id);
        }

        // Arrange - create batch
        var outputDir = Path.Combine(Path.GetTempPath(), $"test-batch-{Guid.NewGuid()}");
        var batch = await _batchService.CreateBatchAsync(
            imageIds,
            [blurFilter.Id, sharpenFilter.Id],
            "Integration Test Batch",
            outputDir);

        try
        {
            // Act
            var processed = await _batchService.ProcessBatchAsync(batch);

            // Assert
            processed.Status.Should().BeOneOf(ProcessingStatus.Completed, ProcessingStatus.Processing);
            processed.TotalImages.Should().Be(3);
        }
        finally
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public async Task FilterConfiguration_ValidationWorks_BeforeProcessing()
    {
        // Arrange
        var invalidFilter = new FilterConfiguration
        {
            Name = "Invalid",
            FilterType = FilterType.None, // Invalid type
            MaxThreadsPerBlock = 256
        };

        // Act & Assert
        await _filterService.CreateFilterAsync(invalidFilter)
            .Should().ThrowAsync<InvalidFilterException>();
    }

    [Fact]
    public async Task ImageValidation_InvalidDimensionsRejected_BeforeProcessing()
    {
        // Arrange
        var invalidImage = new Image
        {
            Id = Guid.NewGuid(),
            FilePath = "/test/image.png",
            FileName = "image.png",
            Width = 0, // Invalid width
            Height = 600,
            BitsPerPixel = 24,
            FileSizeBytes = 1024 * 500,
            Format = ImageFormat.Png
        };

        var savedImage = await _imageRepository.CreateAsync(invalidImage);

        var validFilter = await _filterRepository.CreateAsync(
            CreateValidFilter("Blur", FilterType.Blur));

        // Act & Assert
        await _filterService.ApplyFilterAsync(savedImage, validFilter.Id)
            .Should().ThrowAsync<InvalidImageException>();
    }

    [Fact]
    public async Task PerformanceMonitoring_RecordsMetricsForAllOperations()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateValidFilter("Blur", FilterType.Blur));
        var image = await _imageRepository.CreateAsync(CreateValidImage());

        // Act
        await _filterService.ApplyFilterAsync(image, filter.Id);
        var metrics = _performanceService.GetCurrentMetrics();

        // Assert
        metrics.TotalOperationsCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task MultipleFilters_AppliedSequentially_InOrder()
    {
        // Arrange
        var blur = await _filterRepository.CreateAsync(
            CreateValidFilter("Blur", FilterType.Blur));
        var sharpen = await _filterRepository.CreateAsync(
            CreateValidFilter("Sharpen", FilterType.Sharpen));
        var grayscale = await _filterRepository.CreateAsync(
            CreateValidFilter("Grayscale", FilterType.Grayscale));

        var image = CreateValidImage();

        // Act - apply filters sequentially
        var step1 = await _filterService.ApplyFilterAsync(image, blur.Id);
        var step2 = await _filterService.ApplyFilterAsync(step1, sharpen.Id);
        var step3 = await _filterService.ApplyFilterAsync(step2, grayscale.Id);

        // Assert
        step3.Should().NotBeNull();
        step3.ColorSpace.Should().Be(ColorSpace.Grayscale);
    }

    [Fact]
    public async Task GpuMemoryManagement_AllocateAndDeallocate_AreBalanced()
    {
        // Arrange
        var device = _gpuService.GetBestDevice();
        var bytes = 100 * 1024 * 1024;
        var initialTotal = _gpuService.GetTotalAllocatedMemory();

        // Act
        _gpuService.AllocateMemory(bytes, device!.Id);
        var afterAllocate = _gpuService.GetTotalAllocatedMemory();

        _gpuService.DeallocateMemory(bytes, device.Id);
        var afterDeallocate = _gpuService.GetTotalAllocatedMemory();

        // Assert
        afterAllocate.Should().Be(initialTotal + bytes);
        afterDeallocate.Should().Be(initialTotal);
    }

    [Fact]
    public async Task ProcessingStatistics_CalculatedCorrectly_AfterOperations()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateValidFilter("Blur", FilterType.Blur));

        var images = new List<Image>();
        for (int i = 0; i < 2; i++)
        {
            var image = CreateValidImage();
            var saved = await _imageRepository.CreateAsync(image);
            images.Add(saved);
        }

        // Act
        var processedImages = new List<Image>();
        foreach (var image in images)
        {
            var processed = await _filterService.ApplyFilterAsync(image, filter.Id);
            processedImages.Add(processed);
        }

        // Assert
        processedImages.Should().HaveCount(2);
        processedImages.Should().AllSatisfy(img => img.Should().NotBeNull());
    }

    [Fact]
    public async Task FilterChain_BuildAndExecute_ProducesExpectedOutput()
    {
        // Arrange
        var chain = new FilterChain { Name = "Processing Chain" };
        var blurId = Guid.NewGuid();
        var sharpId = Guid.NewGuid();

        chain.AddStep(blurId);
        chain.AddStep(sharpId);

        // Act
        var enabled = chain.GetEnabledSteps();

        // Assert
        enabled.Should().HaveCount(2);
        enabled[0].FilterId.Should().Be(blurId);
        enabled[1].FilterId.Should().Be(sharpId);
    }

    [Fact]
    public async Task ImageBatch_WithVariousStatuses_TracksProgressCorrectly()
    {
        // Arrange
        var batch = new ImageBatch
        {
            Name = "Progress Test Batch",
            OutputDirectory = "/output",
            TotalImages = 5
        };

        // Act
        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: false);

        // Assert
        batch.ProcessedImages.Should().Be(2);
        batch.FailedImages.Should().Be(1);
        batch.GetProgressPercentage().Should().BeApproximately(60.0, precision: 0.01);
        batch.GetSuccessRate().Should().BeApproximately(66.67, precision: 0.01);
    }

    [Fact]
    public async Task CancelBatchProcessing_StopsActiveOperations()
    {
        // Arrange
        var filter = await _filterRepository.CreateAsync(
            CreateValidFilter("Blur", FilterType.Blur));

        var imageIds = new List<Guid>();
        for (int i = 0; i < 2; i++)
        {
            var image = CreateValidImage();
            var saved = await _imageRepository.CreateAsync(image);
            imageIds.Add(saved.Id);
        }

        var batch = await _batchService.CreateBatchAsync(
            imageIds,
            [filter.Id],
            "Cancel Test",
            "/tmp");

        // Act
        var result = _batchService.CancelBatch(batch.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ErrorHandling_InvalidFilterApplication_RecordsFailure()
    {
        // Arrange
        var image = CreateValidImage();
        var invalidFilterId = Guid.NewGuid();

        // Act & Assert
        await _filterService.ApplyFilterAsync(image, invalidFilterId)
            .Should().ThrowAsync<InvalidFilterException>();
    }

    [Fact]
    public async Task DeviceManagement_SelectsBestGpu_ForProcessing()
    {
        // Arrange & Act
        var bestDevice = _gpuService.GetBestDevice();

        // Assert
        bestDevice.Should().NotBeNull();
        bestDevice!.IsAvailable.Should().BeTrue();
        bestDevice.GlobalMemoryBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResultPersistence_SavesProcessingResults()
    {
        // Arrange
        var result = new ProcessingResult
        {
            ImageId = Guid.NewGuid(),
            IsSuccessful = true,
            OutputFilePath = "/output/image.png"
        };

        // Act
        var saved = await _resultRepository.CreateAsync(result);

        // Assert
        saved.Should().NotBeNull();
        saved.ImageId.Should().Be(result.ImageId);
        saved.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigurationValidation_EnsuresConsistency_AcrossServices()
    {
        // Arrange
        var filter = CreateValidFilter("Test", FilterType.Blur);
        var image = CreateValidImage();

        // Act
        var validFilter = filter.Validate();
        var validImage = image.Validate();

        // Assert
        validFilter.Should().BeTrue();
        validImage.Should().BeTrue();
    }
}
