#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GpuImageProcessing.Tests.Pipeline;

public class BatchProcessingPipelineTests
{
    private readonly Mock<ImageProcessingService> _processingServiceMock;
    private readonly Mock<PerformanceMonitoringService> _performanceMonitorMock;
    private readonly Mock<ILogger<BatchProcessingPipeline>> _loggerMock;
    private readonly BatchProcessingPipeline _sut;

    public BatchProcessingPipelineTests()
    {
        _processingServiceMock = new Mock<ImageProcessingService>(
            Mock.Of<ImageRepository>(),
            Mock.Of<FilterConfigurationRepository>(),
            Mock.Of<ProcessingResultRepository>(),
            Mock.Of<FilterService>(),
            Mock.Of<GpuManagementService>(),
            Mock.Of<PerformanceMonitoringService>(),
            Mock.Of<ILogger<ImageProcessingService>>());

        _performanceMonitorMock = new Mock<PerformanceMonitoringService>(
            Mock.Of<ILogger<PerformanceMonitoringService>>());

        _loggerMock = new Mock<ILogger<BatchProcessingPipeline>>();

        var options = new BatchPipelineOptions
        {
            MaxConcurrency = 2,
            MaxRetries = 1,
            RetryBaseDelayMs = 0   // no real delay in tests
        };

        _sut = new BatchProcessingPipeline(
            _processingServiceMock.Object,
            _performanceMonitorMock.Object,
            options,
            _loggerMock.Object);
    }

    private static ImageBatch CreateValidBatch(int imageCount = 3, string? outputDir = null)
    {
        var dir = outputDir ?? Path.Combine(Path.GetTempPath(), $"pipeline-test-{Guid.NewGuid()}");
        var batch = new ImageBatch
        {
            Name = "Test Pipeline Batch",
            OutputDirectory = dir,
            TotalImages = imageCount
        };

        for (int i = 0; i < imageCount; i++)
            batch.AddImage(Guid.NewGuid());

        batch.AddFilter(Guid.NewGuid());
        return batch;
    }

    [Fact]
    public async Task RunAsync_NullBatch_ThrowsArgumentNullException()
    {
        await _sut.Invoking(s => s.RunAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("batch");
    }

    [Fact]
    public async Task RunAsync_InvalidBatch_ThrowsProcessingException()
    {
        var batch = new ImageBatch { Name = "Empty", OutputDirectory = "/out" };
        // no images, no filters — fails Validate()

        await _sut.Invoking(s => s.RunAsync(batch))
            .Should().ThrowAsync<ProcessingException>();
    }

    [Fact]
    public async Task RunAsync_AllImagesSucceed_ReturnsFullSuccessResult()
    {
        var batch = CreateValidBatch(imageCount: 3);
        try
        {
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

            var result = await _sut.RunAsync(batch);

            result.SucceededCount.Should().Be(3);
            result.FailedCount.Should().Be(0);
            result.SuccessRate.Should().BeApproximately(100.0, precision: 0.01);
            result.TotalImages.Should().Be(3);
        }
        finally
        {
            if (Directory.Exists(batch.OutputDirectory))
                Directory.Delete(batch.OutputDirectory);
        }
    }

    [Fact]
    public async Task RunAsync_AllImagesFail_ReturnsFullFailureResult()
    {
        var batch = CreateValidBatch(imageCount: 2);
        try
        {
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProcessingException("GPU error"));

            var result = await _sut.RunAsync(batch);

            result.FailedCount.Should().Be(2);
            result.SucceededCount.Should().Be(0);
            result.SuccessRate.Should().Be(0.0);
        }
        finally
        {
            if (Directory.Exists(batch.OutputDirectory))
                Directory.Delete(batch.OutputDirectory);
        }
    }

    [Fact]
    public async Task RunAsync_PartialFailure_ReturnsCorrectCounts()
    {
        var batch = CreateValidBatch(imageCount: 4);
        try
        {
            int call = 0;
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    call++;
                    if (call % 2 == 0)
                        throw new ProcessingException("fail");
                    return new ProcessingResult { IsSuccessful = true };
                });

            var result = await _sut.RunAsync(batch);

            result.TotalImages.Should().Be(4);
            result.Outcomes.Should().HaveCount(4);
        }
        finally
        {
            if (Directory.Exists(batch.OutputDirectory))
                Directory.Delete(batch.OutputDirectory);
        }
    }

    [Fact]
    public async Task RunAsync_RaisesProgressChangedForEachImage()
    {
        var batch = CreateValidBatch(imageCount: 3);
        try
        {
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

            var progressEvents = new List<BatchPipelineProgressEventArgs>();
            _sut.ProgressChanged += (_, e) => progressEvents.Add(e);

            await _sut.RunAsync(batch);

            progressEvents.Should().HaveCount(3);
            progressEvents.Should().OnlyContain(e => e.BatchId == batch.Id);
        }
        finally
        {
            if (Directory.Exists(batch.OutputDirectory))
                Directory.Delete(batch.OutputDirectory);
        }
    }

    [Fact]
    public async Task RunAsync_CreatesOutputDirectory()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), $"pipeline-out-{Guid.NewGuid()}");
        var batch = CreateValidBatch(imageCount: 1, outputDir: outputDir);

        try
        {
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

            await _sut.RunAsync(batch);

            Directory.Exists(outputDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir);
        }
    }

    [Fact]
    public async Task RunAsync_CompletedBatchHasCorrectStatus()
    {
        var batch = CreateValidBatch(imageCount: 2);
        try
        {
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessingResult { IsSuccessful = true });

            await _sut.RunAsync(batch);

            batch.Status.Should().Be(ProcessingStatus.Completed);
        }
        finally
        {
            if (Directory.Exists(batch.OutputDirectory))
                Directory.Delete(batch.OutputDirectory);
        }
    }

    [Fact]
    public async Task RunAsync_RetriesFailedImageUpToMaxRetries()
    {
        var batch = CreateValidBatch(imageCount: 1);
        try
        {
            int invocations = 0;
            _processingServiceMock
                .Setup(x => x.ProcessImageAsync(
                    It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    invocations++;
                    if (invocations < 2)
                        throw new ProcessingException("transient");
                    return new ProcessingResult { IsSuccessful = true };
                });

            var result = await _sut.RunAsync(batch);

            // MaxRetries = 1, so it retries once (2 total attempts)
            invocations.Should().Be(2);
            result.SucceededCount.Should().Be(1);
        }
        finally
        {
            if (Directory.Exists(batch.OutputDirectory))
                Directory.Delete(batch.OutputDirectory);
        }
    }

    [Fact]
    public void Constructor_NullProcessingService_ThrowsArgumentNullException()
    {
        var act = () => new BatchProcessingPipeline(
            null!,
            _performanceMonitorMock.Object,
            new BatchPipelineOptions(),
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("processingService");
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new BatchProcessingPipeline(
            _processingServiceMock.Object,
            _performanceMonitorMock.Object,
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }
}
