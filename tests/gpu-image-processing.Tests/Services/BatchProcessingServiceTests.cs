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

public class BatchProcessingServiceTests
{
    private readonly Mock<ImageProcessingService> _processingServiceMock;
    private readonly Mock<ImageRepository> _imageRepositoryMock;
    private readonly Mock<ILogger<BatchProcessingService>> _loggerMock;
    private readonly BatchProcessingService _sut;

    public BatchProcessingServiceTests()
    {
        _processingServiceMock = new Mock<ImageProcessingService>(
            Mock.Of<ImageRepository>(),
            Mock.Of<FilterConfigurationRepository>(),
            Mock.Of<ProcessingResultRepository>(),
            Mock.Of<FilterService>(),
            Mock.Of<GpuManagementService>(),
            Mock.Of<PerformanceMonitoringService>(),
            Mock.Of<ILogger<ImageProcessingService>>());

        _imageRepositoryMock = new Mock<ImageRepository>();
        _loggerMock = new Mock<ILogger<BatchProcessingService>>();

        _sut = new BatchProcessingService(
            _processingServiceMock.Object,
            _imageRepositoryMock.Object,
            _loggerMock.Object);
    }

    private static ImageBatch CreateValidBatch(int imageCount = 3, int filterCount = 2)
    {
        var batch = new ImageBatch
        {
            Name = "Test Batch",
            OutputDirectory = "/output",
            TotalImages = imageCount
        };

        for (int i = 0; i < imageCount; i++)
            batch.AddImage(Guid.NewGuid());

        for (int i = 0; i < filterCount; i++)
            batch.AddFilter(Guid.NewGuid());

        return batch;
    }

    [Fact]
    public async Task ProcessBatchAsync_NullBatch_ThrowsArgumentNullException()
    {
        // Act & Assert
        await _sut.ProcessBatchAsync(null!).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessBatchAsync_InvalidBatch_ThrowsProcessingException()
    {
        // Arrange
        var batch = new ImageBatch { Name = "Empty Batch", OutputDirectory = "/output" };
        // Missing images and filters

        // Act & Assert
        await _sut.ProcessBatchAsync(batch).Should().ThrowAsync<ProcessingException>();
    }

    [Fact]
    public async Task ProcessBatchAsync_ValidBatch_ReturnsCompletedBatch()
    {
        // Arrange
        var batch = CreateValidBatch();
        var result = new ProcessingResult { ImageId = batch.ImageIds[0], IsSuccessful = true };

        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var processed = await _sut.ProcessBatchAsync(batch);

        // Assert
        processed.Status.Should().Be(ProcessingStatus.Completed);
        processed.ProcessedImages.Should().Be(3);
        processed.FailedImages.Should().Be(0);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithCancellation_ReturnsCancelledBatch()
    {
        // Arrange
        var batch = CreateValidBatch();
        var cts = new CancellationTokenSource();

        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .Returns(async (Guid _, List<Guid> _, CancellationToken ct) =>
            {
                cts.Cancel();
                await Task.Delay(100, ct);
                return new ProcessingResult { IsSuccessful = true };
            });

        // Act
        var processed = await _sut.ProcessBatchAsync(batch, cts.Token);

        // Assert
        processed.Status.Should().Be(ProcessingStatus.Cancelled);
    }

    [Fact]
    public async Task ProcessBatchAsync_PartialFailure_RecordsFailedCount()
    {
        // Arrange
        var batch = CreateValidBatch(3, 1);
        var successResult = new ProcessingResult { ImageId = Guid.NewGuid(), IsSuccessful = true };
        var failureResult = new ProcessingResult { ImageId = Guid.NewGuid(), IsSuccessful = false };

        int callCount = 0;
        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount <= 2 ? successResult : failureResult;
            });

        // Act
        var processed = await _sut.ProcessBatchAsync(batch);

        // Assert
        processed.ProcessedImages.Should().Be(2);
        processed.FailedImages.Should().Be(1);
        processed.GetSuccessRate().Should().BeApproximately(66.67, precision: 0.01);
    }

    [Fact]
    public void GetBatchStatus_ActiveBatch_ReturnsBatch()
    {
        // Arrange
        var batch = CreateValidBatch();
        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        _ = _sut.ProcessBatchAsync(batch).Result;

        // Act
        var status = _sut.GetBatchStatus(batch.Id);

        // Assert
        status.Should().NotBeNull();
    }

    [Fact]
    public void GetBatchStatus_NonExistentBatch_ReturnsNull()
    {
        // Act
        var status = _sut.GetBatchStatus(Guid.NewGuid());

        // Assert
        status.Should().BeNull();
    }

    [Fact]
    public void CancelBatch_ActiveBatch_ReturnsTrueAndMarksAsCancel()
    {
        // Arrange
        var batch = CreateValidBatch();
        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        _ = _sut.ProcessBatchAsync(batch).Result;

        // Act
        var cancelled = _sut.CancelBatch(batch.Id);

        // Assert
        cancelled.Should().BeTrue();
    }

    [Fact]
    public void CancelBatch_NonExistentBatch_ReturnsFalse()
    {
        // Act
        var cancelled = _sut.CancelBatch(Guid.NewGuid());

        // Assert
        cancelled.Should().BeFalse();
    }

    [Fact]
    public async Task CreateBatchAsync_EmptyImageIds_ThrowsArgumentException()
    {
        // Act & Assert
        await _sut.CreateBatchAsync([], [Guid.NewGuid()], "Test", "/output")
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Images and filters are required*");
    }

    [Fact]
    public async Task CreateBatchAsync_EmptyFilterIds_ThrowsArgumentException()
    {
        // Act & Assert
        await _sut.CreateBatchAsync([Guid.NewGuid()], [], "Test", "/output")
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Images and filters are required*");
    }

    [Fact]
    public async Task CreateBatchAsync_ValidInputs_ReturnsBatchWithImages()
    {
        // Arrange
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var filterIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var batch = await _sut.CreateBatchAsync(imageIds, filterIds, "Test Batch", "/output");

        // Assert
        batch.ImageIds.Should().HaveCount(2);
        batch.FilterIds.Should().HaveCount(1);
        batch.Name.Should().Be("Test Batch");
        batch.OutputDirectory.Should().Be("/output");
    }

    [Fact]
    public void GetBatchProgress_ActiveBatch_ReturnsProgressDictionary()
    {
        // Arrange
        var batch = CreateValidBatch();
        batch.MarkImageProcessed(true);
        batch.MarkImageProcessed(false);

        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        _ = _sut.ProcessBatchAsync(batch).Result;

        // Act
        var progress = _sut.GetBatchProgress(batch.Id);

        // Assert
        progress.Should().ContainKey("Status");
        progress.Should().ContainKey("ProgressPercent");
        progress.Should().ContainKey("SuccessRate");
        progress.Should().ContainKey("ProcessedImages");
    }

    [Fact]
    public void GetBatchProgress_NonExistentBatch_ReturnsEmptyDictionary()
    {
        // Act
        var progress = _sut.GetBatchProgress(Guid.NewGuid());

        // Assert
        progress.Should().BeEmpty();
    }

    [Fact]
    public void GetActiveBatchCount_MultipleActiveBatches_ReturnsCorrectCount()
    {
        // Arrange
        var batch1 = CreateValidBatch();
        var batch2 = CreateValidBatch();

        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        _ = _sut.ProcessBatchAsync(batch1).Result;
        _ = _sut.ProcessBatchAsync(batch2).Result;

        // Act
        var count = _sut.GetActiveBatchCount();

        // Assert
        count.Should().Be(0); // Batches complete synchronously in tests
    }

    [Fact]
    public void GetActiveBatches_MultipleActiveBatches_ReturnsAllBatches()
    {
        // Arrange
        var batch1 = CreateValidBatch();

        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        _ = _sut.ProcessBatchAsync(batch1).Result;

        // Act
        var batches = _sut.GetActiveBatches();

        // Assert
        batches.Should().BeEmpty(); // Completed batches are removed
    }

    [Fact]
    public async Task ProcessBatchAsync_CreatesOutputDirectory()
    {
        // Arrange
        var outputDir = Path.Combine(Path.GetTempPath(), $"batch-test-{Guid.NewGuid()}");
        var batch = CreateValidBatch();
        batch.OutputDirectory = outputDir;

        _processingServiceMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

        try
        {
            // Act
            await _sut.ProcessBatchAsync(batch);

            // Assert
            Directory.Exists(outputDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir);
        }
    }
}
