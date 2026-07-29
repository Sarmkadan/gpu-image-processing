// tests/gpu-image-processing.Tests/ImageProcessingServiceExtensionsTests.cs
// Unit tests for the ImageProcessingServiceExtensions static class.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests;

public class ImageProcessingServiceExtensionsTests
{
    // -------------------------------------------------------------------------
    // ProcessImagesAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ProcessImagesAsync_ReturnsResults_ForEachImage()
    {
        // Arrange
        var imageIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var filterIds = new List<Guid> { Guid.NewGuid() };
        var expectedResults = new[]
        {
            new ProcessingResult(),
            new ProcessingResult()
        };

        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        mock.Setup(s => s.ProcessImageAsync(imageIds[0], filterIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults[0]);
        mock.Setup(s => s.ProcessImageAsync(imageIds[1], filterIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults[1]);

        // Act
        var actual = await ImageProcessingServiceExtensions.ProcessImagesAsync(
            mock.Object,
            imageIds,
            filterIds,
            CancellationToken.None);

        // Assert
        Assert.Equal(expectedResults.Length, actual.Count);
        Assert.Same(expectedResults[0], actual[0]);
        Assert.Same(expectedResults[1], actual[1]);
    }

    [Fact]
    public async Task ProcessImagesAsync_EmptyImageIds_ReturnsEmpty()
    {
        // Arrange
        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        var filterIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = await ImageProcessingServiceExtensions.ProcessImagesAsync(
            mock.Object,
            Array.Empty<Guid>(),
            filterIds,
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ProcessImagesAsync_NullService_ThrowsArgumentNullException()
    {
        // Arrange
        var imageIds = new[] { Guid.NewGuid() };
        var filterIds = new List<Guid> { Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await ImageProcessingServiceExtensions.ProcessImagesAsync(
                null!,
                imageIds,
                filterIds,
                CancellationToken.None));
    }

    // -------------------------------------------------------------------------
    // ProcessImageWithIterationsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ProcessImageWithIterationsAsync_RepeatsFilterCorrectNumberOfTimes()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var filterId = Guid.NewGuid();
        int iterations = 3;
        var expected = new ProcessingResult();

        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        mock.Setup(s => s.ProcessImageAsync(
                imageId,
                It.Is<List<Guid>>(list => list.Count == iterations && list.TrueForAll(id => id == filterId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await ImageProcessingServiceExtensions.ProcessImageWithIterationsAsync(
            mock.Object,
            imageId,
            filterId,
            iterations,
            CancellationToken.None);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ProcessImageWithIterationsAsync_IterationCountLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        var imageId = Guid.NewGuid();
        var filterId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await ImageProcessingServiceExtensions.ProcessImageWithIterationsAsync(
                mock.Object,
                imageId,
                filterId,
                0,
                CancellationToken.None));
    }

    // -------------------------------------------------------------------------
    // GetStatisticsReportAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetStatisticsReportAsync_ReturnsFormattedReport()
    {
        // Arrange
        var stats = new Dictionary<string, object>
        {
            ["TotalImages"] = 10,
            ["ProcessedImages"] = 8,
            ["SuccessfulProcessing"] = 7,
            ["FailedProcessing"] = 1,
            ["SuccessRate"] = 87.5,
            ["AverageProcessingTime"] = 123.456,
            ["TotalProcessingTime"] = 987L
        };

        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        mock.Setup(s => s.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var report = await ImageProcessingServiceExtensions.GetStatisticsReportAsync(
            mock.Object,
            CultureInfo.InvariantCulture,
            CancellationToken.None);

        // Assert
        Assert.Contains("Total Images: 10", report);
        Assert.Contains("Processed Images: 8", report);
        Assert.Contains("Successful: 7", report);
        Assert.Contains("Failed: 1", report);
        Assert.Contains("Success Rate: 87.50%", report);
        Assert.Contains("Average Processing Time: 123.46 ms", report);
        Assert.Contains("Total Processing Time:", report);
    }

    [Fact]
    public async Task GetStatisticsReportAsync_MissingKey_ThrowsKeyNotFoundException()
    {
        // Arrange – omit one required key
        var stats = new Dictionary<string, object>
        {
            ["TotalImages"] = 1,
            // "ProcessedImages" missing
            ["SuccessfulProcessing"] = 1,
            ["FailedProcessing"] = 0,
            ["SuccessRate"] = 100.0,
            ["AverageProcessingTime"] = 10.0,
            ["TotalProcessingTime"] = 10L
        };

        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        mock.Setup(s => s.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await ImageProcessingServiceExtensions.GetStatisticsReportAsync(
                mock.Object,
                null,
                CancellationToken.None));
    }

    // -------------------------------------------------------------------------
    // GetLatestSuccessfulResultAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetLatestSuccessfulResultAsync_ReturnsResult_WhenExists()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var expected = new ProcessingResult();

        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        mock.Setup(s => s.GetProcessingResultAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var actual = await ImageProcessingServiceExtensions.GetLatestSuccessfulResultAsync(
            mock.Object,
            imageId,
            CancellationToken.None);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task GetLatestSuccessfulResultAsync_NoResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var imageId = Guid.NewGuid();

        var mock = new Mock<ImageProcessingService>(MockBehavior.Strict);
        mock.Setup(s => s.GetProcessingResultAsync(imageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProcessingResult?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await ImageProcessingServiceExtensions.GetLatestSuccessfulResultAsync(
                mock.Object,
                imageId,
                CancellationToken.None));
    }
}
