#nullable enable
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Contains unit tests for the <see cref="ImageProcessingService"/> class, focusing on image processing pipelines,
/// error handling, and statistics calculation.
/// </summary>
public class ImageProcessingServiceTests
{
    private readonly Mock<ImageRepository> _imageRepositoryMock;
    private readonly Mock<FilterConfigurationRepository> _filterRepositoryMock;
    private readonly Mock<ProcessingResultRepository> _resultRepositoryMock;
    private readonly Mock<FilterService> _filterServiceMock;
    private readonly Mock<GpuManagementService> _gpuServiceMock;
    private readonly Mock<PerformanceMonitoringService> _performanceServiceMock;
    private readonly Mock<ILogger<ImageProcessingService>> _loggerMock;
    private readonly ImageProcessingService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingServiceTests"/> class and configures the mock dependencies.
    /// </summary>
    public ImageProcessingServiceTests()
    {
        _imageRepositoryMock = new Mock<ImageRepository>();
        _filterRepositoryMock = new Mock<FilterConfigurationRepository>();
        _resultRepositoryMock = new Mock<ProcessingResultRepository>();
        _filterServiceMock = new Mock<FilterService>();
        _gpuServiceMock = new Mock<GpuManagementService>();
        _performanceServiceMock = new Mock<PerformanceMonitoringService>();
        _loggerMock = new Mock<ILogger<ImageProcessingService>>();

        _sut = new ImageProcessingService(
            _imageRepositoryMock.Object,
            _filterRepositoryMock.Object,
            _resultRepositoryMock.Object,
            _filterServiceMock.Object,
            _gpuServiceMock.Object,
            _performanceServiceMock.Object,
            _loggerMock.Object);
    }

    private static Image CreateValidImage()
    {
        return new Image
        {
            Id = Guid.NewGuid(),
            FilePath = "/test/image.png",
            FileName = "image.png",
            Width = 1920,
            Height = 1080,
            BitsPerPixel = 24,
            FileSizeBytes = 1024 * 1024,
            Format = ImageFormat.Png,
            ColorSpace = ColorSpace.Rgb
        };
    }

    private static GpuDevice CreateValidDevice()
    {
        return new GpuDevice
        {
            Id = Guid.NewGuid(),
            Name = "Test GPU",
            GlobalMemoryBytes = 2L * 1024 * 1024 * 1024,
            MaxAllocatableMemoryBytes = 2L * 1024 * 1024 * 1024,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> throws an <see cref="InvalidImageException"/>
    /// if the target image is not found in the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_ImageNotFound_ThrowsInvalidImageException()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Image?)null);

        // Act & Assert
        await _sut.Invoking(s => s.ProcessImageAsync(imageId, [Guid.NewGuid()]))
            .Should().ThrowAsync<InvalidImageException>();
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> throws an <see cref="InvalidImageException"/>
    /// if the target image exists but is considered invalid (e.g., zero width).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_InvalidImage_ThrowsInvalidImageException()
    {
        // Arrange
        var image = CreateValidImage();
        image.Width = 0; // Invalid width
        var imageId = image.Id;

        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        // Act & Assert
        await _sut.Invoking(s => s.ProcessImageAsync(imageId, [Guid.NewGuid()]))
            .Should().ThrowAsync<InvalidImageException>();
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> throws a <see cref="GpuException"/>
    /// when no suitable GPU device is available for processing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_NoGpuDevice_ThrowsGpuException()
    {
        // Arrange
        var image = CreateValidImage();
        var imageId = image.Id;

        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _gpuServiceMock.Setup(x => x.GetBestDevice()).Returns((GpuDevice?)null);

        // Act & Assert
        await _sut.Invoking(s => s.ProcessImageAsync(imageId, [Guid.NewGuid()]))
            .Should().ThrowAsync<GpuException>();
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> successfully processes an image
    /// with a single filter and returns a successful <see cref="ProcessingResult"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_SuccessfulProcessing_ReturnsCompletedResult()
    {
        // Arrange
        var image = CreateValidImage();
        var imageId = image.Id;
        var filterId = Guid.NewGuid();
        var device = CreateValidDevice();
        var filterConfig = new FilterConfiguration
        {
            Id = filterId,
            Name = "Blur",
            FilterType = FilterType.Blur,
            IsActive = true,
            MaxThreadsPerBlock = 256
        };

        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _gpuServiceMock.Setup(x => x.GetBestDevice()).Returns(device);

        _gpuServiceMock.Setup(x => x.AllocateMemory(It.IsAny<long>(), device.Id))
            .Returns(true);

        _filterServiceMock.Setup(x => x.ApplyFilterAsync(image, filterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _filterRepositoryMock.Setup(x => x.GetByIdAsync(filterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filterConfig);

        _resultRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ProcessingResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        // Act
        var result = await _sut.ProcessImageAsync(imageId, [filterId]);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.ImageId.Should().Be(imageId);

        _gpuServiceMock.Verify(x => x.AllocateMemory(It.IsAny<long>(), device.Id), Times.Once);
        _gpuServiceMock.Verify(x => x.DeallocateMemory(It.IsAny<long>(), device.Id), Times.Once);
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> correctly applies multiple filters
    /// in sequential order to an image.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_MultipleFilters_AppliesAllSequentially()
    {
        // Arrange
        var image = CreateValidImage();
        var imageId = image.Id;
        var filter1Id = Guid.NewGuid();
        var filter2Id = Guid.NewGuid();
        var device = CreateValidDevice();

        var filter1Config = new FilterConfiguration
        {
            Id = filter1Id,
            Name = "Blur",
            FilterType = FilterType.Blur,
            IsActive = true,
            MaxThreadsPerBlock = 256
        };

        var filter2Config = new FilterConfiguration
        {
            Id = filter2Id,
            Name = "Sharpen",
            FilterType = FilterType.Sharpen,
            IsActive = true,
            MaxThreadsPerBlock = 256
        };

        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _gpuServiceMock.Setup(x => x.GetBestDevice()).Returns(device);
        _gpuServiceMock.Setup(x => x.AllocateMemory(It.IsAny<long>(), device.Id)).Returns(true);

        _filterServiceMock.Setup(x => x.ApplyFilterAsync(image, filter1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _filterServiceMock.Setup(x => x.ApplyFilterAsync(image, filter2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _filterRepositoryMock.Setup(x => x.GetByIdAsync(filter1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filter1Config);

        _filterRepositoryMock.Setup(x => x.GetByIdAsync(filter2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filter2Config);

        _resultRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ProcessingResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        // Act
        var result = await _sut.ProcessImageAsync(imageId, [filter1Id, filter2Id]);

        // Assert
        result.IsSuccessful.Should().BeTrue();

        _filterServiceMock.Verify(
            x => x.ApplyFilterAsync(image, filter1Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _filterServiceMock.Verify(
            x => x.ApplyFilterAsync(image, filter2Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> correctly handles filter application
    /// failures by marking the image as failed and throwing a <see cref="ProcessingException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_FilterApplicationFails_ReturnsFailedResult()
    {
        // Arrange
        var image = CreateValidImage();
        var imageId = image.Id;
        var filterId = Guid.NewGuid();
        var device = CreateValidDevice();

        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _gpuServiceMock.Setup(x => x.GetBestDevice()).Returns(device);
        _gpuServiceMock.Setup(x => x.AllocateMemory(It.IsAny<long>(), device.Id)).Returns(true);

        _filterServiceMock.Setup(x => x.ApplyFilterAsync(image, filterId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidFilterException("Filter not found"));

        // Act & Assert
        await _sut.Invoking(s => s.ProcessImageAsync(imageId, [filterId]))
            .Should().ThrowAsync<ProcessingException>();

        _imageRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Image>(i => i.Status == ProcessingStatus.Failed), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> sets the image status to
    /// <see cref="ProcessingStatus.Processing"/> during the operation.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessImageAsync_MarksImageAsProcessing()
    {
        // Arrange
        var image = CreateValidImage();
        var imageId = image.Id;
        var filterId = Guid.NewGuid();
        var device = CreateValidDevice();

        _imageRepositoryMock.Setup(x => x.GetByIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _gpuServiceMock.Setup(x => x.GetBestDevice()).Returns(device);
        _gpuServiceMock.Setup(x => x.AllocateMemory(It.IsAny<long>(), device.Id)).Returns(true);

        _filterServiceMock.Setup(x => x.ApplyFilterAsync(image, filterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(image);

        _filterRepositoryMock.Setup(x => x.GetByIdAsync(filterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FilterConfiguration { Name = "Test", FilterType = FilterType.Blur });

        _resultRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ProcessingResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        // Act
        await _sut.ProcessImageAsync(imageId, [filterId]);

        // Assert
        _imageRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Image>(i => i.Status == ProcessingStatus.Processing), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.GetProcessingResultAsync"/> returns the most recent
    /// <see cref="ProcessingResult"/> for a given image ID when multiple results exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetProcessingResultAsync_ExistingResults_ReturnsLatestResult()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var result1 = new ProcessingResult { ImageId = imageId, StartedAt = DateTime.UtcNow.AddHours(-1) };
        var result2 = new ProcessingResult { ImageId = imageId, StartedAt = DateTime.UtcNow };

        _resultRepositoryMock.Setup(x => x.GetByImageIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([result1, result2]);

        // Act
        var result = await _sut.GetProcessingResultAsync(imageId);

        // Assert
        result.Should().NotBeNull();
        result!.StartedAt.Should().Be(result2.StartedAt);
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.GetProcessingResultAsync"/> returns <c>null</c>
    /// when no processing results exist for a given image ID.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetProcessingResultAsync_NoResults_ReturnsNull()
    {
        // Arrange
        var imageId = Guid.NewGuid();

        _resultRepositoryMock.Setup(x => x.GetByImageIdAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.GetProcessingResultAsync(imageId);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.GetStatisticsAsync"/> calculates and returns the
    /// correct processing metrics, including success rates and average processing times.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectMetrics()
    {
        // Arrange
        var totalImages = 10;
        var successfulResults = new List<ProcessingResult>
        {
            new ProcessingResult { ImageId = Guid.NewGuid(), IsSuccessful = true, ProcessingTimeMilliseconds = 100 },
            new ProcessingResult { ImageId = Guid.NewGuid(), IsSuccessful = true, ProcessingTimeMilliseconds = 150 }
        };
        var failedResults = new List<ProcessingResult>
        {
            new ProcessingResult { ImageId = Guid.NewGuid(), IsSuccessful = false, ProcessingTimeMilliseconds = 50 }
        };

        _imageRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalImages);

        _resultRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(successfulResults.Concat(failedResults).ToList());

        // Act
        var stats = await _sut.GetStatisticsAsync();

        // Assert
        stats["TotalImages"].Should().Be(totalImages);
        stats["ProcessedImages"].Should().Be(3);
        stats["SuccessfulProcessing"].Should().Be(2);
        stats["FailedProcessing"].Should().Be(1);
        ((double)stats["SuccessRate"]).Should().BeApproximately(66.67, precision: 0.01);
        ((double)stats["AverageProcessingTime"]).Should().BeApproximately(100.0, precision: 1);
    }

    /// <summary>
    /// Validates that <see cref="ImageProcessingService.GetStatisticsAsync"/> returns zeroed metrics
    /// when no processing results are present in the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetStatisticsAsync_NoResults_ReturnsZeroMetrics()
    {
        // Arrange
        _imageRepositoryMock.Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _resultRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var stats = await _sut.GetStatisticsAsync();

        // Assert
        stats["SuccessRate"].Should().Be(0.0);
        stats["AverageProcessingTime"].Should().Be(0.0);
        stats["TotalProcessingTime"].Should().Be(0L);
    }
}
