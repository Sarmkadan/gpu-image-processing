#nullable enable

using FluentAssertions;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using GpuImageProcessing.Core;
using Xunit;

namespace GpuImageProcessing.Tests.Services;

public static class BatchProcessingServiceTestsExtensions
{
    /// <summary>
    /// Creates a valid batch with images and filters.
    /// </summary>
    /// <param name="_">The BatchProcessingServiceTests instance (unused)</param>
    /// <param name="imageCount">Number of images to include in the batch</param>
    /// <param name="filterCount">Number of filters to include in the batch</param>
    /// <returns>An ImageBatch with the specified configuration</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if imageCount or filterCount is negative</exception>
    public static ImageBatch CreateValidBatch(
        this BatchProcessingServiceTests _,
        int imageCount = 3,
        int filterCount = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(imageCount);
        ArgumentOutOfRangeException.ThrowIfNegative(filterCount);

        var batch = new ImageBatch
        {
            Name = "Extension Test Batch",
            OutputDirectory = "/output/extensions",
            TotalImages = imageCount
        };

        for (int i = 0; i < imageCount; i++)
        {
            batch.AddImage(Guid.NewGuid());
        }

        for (int i = 0; i < filterCount; i++)
        {
            batch.AddFilter(Guid.NewGuid());
        }

        return batch;
    }

    /// <summary>
    /// Asserts that a batch has the expected processing statistics.
    /// </summary>
    /// <param name="batch">The batch to assert</param>
    /// <param name="expectedProcessed">Expected number of processed images</param>
    /// <param name="expectedFailed">Expected number of failed images</param>
    /// <param name="expectedTotal">Expected total number of images</param>
    /// <exception cref="ArgumentNullException">Thrown if batch is null</exception>
    public static void ShouldHaveProcessingStats(
        this ImageBatch batch,
        int expectedProcessed,
        int expectedFailed,
        int expectedTotal)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedTotal);

        batch.ProcessedImages.Should().Be(expectedProcessed, "because {0} images should have been processed", expectedProcessed);
        batch.FailedImages.Should().Be(expectedFailed, "because {0} images should have failed", expectedFailed);
        batch.TotalImages.Should().Be(expectedTotal, "because total should be {0}", expectedTotal);

        var successRate = batch.GetSuccessRate();

        switch (expectedFailed)
        {
            case 0:
                successRate.Should().Be(1.0, "because all images should have succeeded");
                break;
            default:
                var expectedRate = (double)expectedProcessed / expectedTotal;
                successRate.Should().BeApproximately(expectedRate, precision: 0.01);
                break;
        }
    }

    /// <summary>
    /// Asserts that a batch has completed processing.
    /// </summary>
    /// <param name="batch">The batch to assert</param>
    /// <exception cref="ArgumentNullException">Thrown if batch is null</exception>
    public static void ShouldBeCompleted(this ImageBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        batch.Status.Should().Be(ProcessingStatus.Completed);
        batch.ProcessedImages.Should().BeGreaterThan(0);
        batch.FailedImages.Should().Be(0);
    }

    /// <summary>
    /// Asserts that a batch has the expected number of processed and failed images.
    /// </summary>
    /// <param name="batch">The batch to assert</param>
    /// <param name="expectedProcessed">Expected number of processed images</param>
    /// <param name="expectedFailed">Expected number of failed images</param>
    /// <exception cref="ArgumentNullException">Thrown if batch is null</exception>
    public static void ShouldHaveProcessedAndFailedCount(
        this ImageBatch batch,
        int expectedProcessed,
        int expectedFailed)
    {
        ArgumentNullException.ThrowIfNull(batch);

        batch.ProcessedImages.Should().Be(expectedProcessed);
        batch.FailedImages.Should().Be(expectedFailed);
    }
}