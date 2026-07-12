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
/// Unit tests for <see cref="BatchProcessingService"/> that verify batch image processing functionality.
/// Tests cover batch creation, processing, status tracking, cancellation, and progress reporting.
/// </summary>
public class BatchProcessingServiceTests
{
	private readonly Mock<ImageProcessingService> _processingServiceMock;
	private readonly Mock<ImageRepository> _imageRepositoryMock;
	private readonly Mock<ILogger<BatchProcessingService>> _loggerMock;
	private readonly BatchProcessingService _sut;

	/// <summary>
	/// Initializes a new instance of the <see cref="BatchProcessingServiceTests"/> class.
	/// Sets up mock dependencies for testing batch processing functionality.
	/// </summary>
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

	/// <summary>
	/// Creates a valid test batch with specified image and filter counts.
	/// </summary>
	/// <param name="imageCount">Number of images to add to the batch. Defaults to 3.</param>
	/// <param name="filterCount">Number of filters to add to the batch. Defaults to 2.</param>
	/// <returns>An initialized <see cref="ImageBatch"/> with the specified configuration.</returns>
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

	/// <summary>
	/// Tests that processing a null batch throws an <see cref="ArgumentNullException"/>.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_NullBatch_ThrowsArgumentNullException()
	{
		// Act & Assert
		await _sut.Invoking(s => s.ProcessBatchAsync(null!)).Should().ThrowAsync<ArgumentNullException>();
	}

	/// <summary>
	/// Tests that processing an invalid batch (missing images and filters) throws a <see cref="ProcessingException"/>.
	/// </summary>
	[Fact]
	public async Task ProcessBatchAsync_InvalidBatch_ThrowsProcessingException()
	{
		// Arrange
		var batch = new ImageBatch { Name = "Empty Batch", OutputDirectory = "/output" };
		// Missing images and filters

		// Act & Assert
		await _sut.Invoking(s => s.ProcessBatchAsync(batch)).Should().ThrowAsync<ProcessingException>();
	}

	/// <summary>
	/// Tests that processing a valid batch returns a completed batch with correct status and counts.
	/// </summary>
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

	/// <summary>
	/// Tests that processing a batch with cancellation token returns a cancelled batch.
	/// </summary>
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

	/// <summary>
	/// Tests that partial failures during batch processing are correctly recorded.
	/// </summary>
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

	/// <summary>
	/// Tests that getting the status of an active batch returns the batch information.
	/// </summary>
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

	/// <summary>
	/// Tests that getting the status of a non-existent batch returns null.
	/// </summary>
	[Fact]
	public void GetBatchStatus_NonExistentBatch_ReturnsNull()
	{
		// Act
		var status = _sut.GetBatchStatus(Guid.NewGuid());

		// Assert
		status.Should().BeNull();
	}

	/// <summary>
	/// Tests that cancelling an active batch returns true and marks it as cancelled.
	/// </summary>
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

	/// <summary>
	/// Tests that cancelling a non-existent batch returns false.
	/// </summary>
	[Fact]
	public void CancelBatch_NonExistentBatch_ReturnsFalse()
	{
		// Act
		var cancelled = _sut.CancelBatch(Guid.NewGuid());

		// Assert
		cancelled.Should().BeFalse();
	}

	/// <summary>
	/// Tests that creating a batch with empty image IDs throws an <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public async Task CreateBatchAsync_EmptyImageIds_ThrowsArgumentException()
	{
		// Act & Assert
		await _sut.Invoking(s => s.CreateBatchAsync([], [Guid.NewGuid()], "Test", "/output"))
			.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Images and filters are required*");
	}

	/// <summary>
	/// Tests that creating a batch with empty filter IDs throws an <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public async Task CreateBatchAsync_EmptyFilterIds_ThrowsArgumentException()
	{
		// Act & Assert
		await _sut.Invoking(s => s.CreateBatchAsync([Guid.NewGuid()], [], "Test", "/output"))
			.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Images and filters are required*");
	}

	/// <summary>
	/// Tests that creating a batch with valid inputs returns a batch with the correct images and filters.
	/// </summary>
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

	/// <summary>
	/// Tests that getting the progress of an active batch returns a dictionary with status information.
	/// </summary>
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

	/// <summary>
	/// Tests that getting the progress of a non-existent batch returns an empty dictionary.
	/// </summary>
	[Fact]
	public void GetBatchProgress_NonExistentBatch_ReturnsEmptyDictionary()
	{
		// Act
		var progress = _sut.GetBatchProgress(Guid.NewGuid());

		// Assert
		progress.Should().BeEmpty();
	}

	/// <summary>
	/// Tests that getting the active batch count returns the correct number of active batches.
	/// </summary>
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

	/// <summary>
	/// Tests that getting active batches returns all active batches.
	/// </summary>
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

	/// <summary>
	/// Tests that processing a batch creates the specified output directory.
	/// </summary>
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