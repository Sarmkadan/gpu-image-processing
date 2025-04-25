// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Domain;

public class ImageDomainTests
{
    private static Image CreateValidImage() => new()
    {
        FilePath = "/images/test.png",
        FileName = "test.png",
        Format = ImageFormat.Png,
        Width = 1920,
        Height = 1080,
        BitsPerPixel = 24,
        FileSizeBytes = 1024 * 1024
    };

    [Fact]
    public void Validate_ValidImage_ReturnsTrue()
    {
        // Arrange
        var image = CreateValidImage();

        // Act
        var result = image.Validate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WidthBelowMinimum_ReturnsFalse()
    {
        // Arrange
        var image = CreateValidImage();
        image.Width = Constants.Processing.MinImageWidth - 1;

        // Act
        var result = image.Validate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_UnsupportedBitsPerPixel_ReturnsFalse()
    {
        // Arrange
        var image = CreateValidImage();
        image.BitsPerPixel = 7; // only 8, 16, 24, 32 are valid

        // Act
        var result = image.Validate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculatePixelDataSize_32BitsPerPixel_ReturnsWidthTimesHeightTimesFour()
    {
        // Arrange
        var image = CreateValidImage();
        image.Width = 100;
        image.Height = 200;
        image.BitsPerPixel = 32;

        // Act
        var size = image.CalculatePixelDataSize();

        // Assert
        size.Should().Be(80_000L); // 100 * 200 * 4 bytes
    }

    [Fact]
    public void MarkAsCompleted_SetsCompletedStatusAndOutputPath()
    {
        // Arrange
        var image = CreateValidImage();
        image.MarkAsProcessing();
        const string outputPath = "/output/processed.png";

        // Act
        image.MarkAsCompleted(outputPath);

        // Assert
        image.Status.Should().Be(ProcessingStatus.Completed);
        image.ProcessedOutputPath.Should().Be(outputPath);
    }

    [Fact]
    public void ImageBatch_GetProgressPercentage_PartialCompletion_ReturnsCorrectValue()
    {
        // Arrange
        var batch = new ImageBatch { TotalImages = 10 };
        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: false);

        // Act
        var progress = batch.GetProgressPercentage();

        // Assert
        progress.Should().BeApproximately(30.0, precision: 0.001);
    }

    [Fact]
    public void ProcessingResult_GetTotalFilterExecutionTime_SumsAllAppliedFilters()
    {
        // Arrange
        var result = new ProcessingResult { ImageId = Guid.NewGuid() };
        result.AddFilterApplied("Blur", FilterType.Blur, 15.5);
        result.AddFilterApplied("Sharpen", FilterType.Sharpen, 8.0);
        result.AddFilterApplied("Grayscale", FilterType.Grayscale, 3.5);

        // Act
        var total = result.GetTotalFilterExecutionTime();

        // Assert
        total.Should().BeApproximately(27.0, precision: 0.001);
    }
}
