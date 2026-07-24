using System;
using Xunit;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests.Core;

public class ProcessingExceptionTests
{
    [Fact]
    public void ProcessingException_AllPropertiesSet_ShouldExposeValues()
    {
        // Arrange
        const string message = "Processing failed";
        const string imagePath = "/tmp/image.png";
        const string filterName = "Blur";
        const int attemptNumber = 2;

        // Act
        var ex = new ProcessingException(message, imagePath, filterName, attemptNumber);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Equal(imagePath, ex.ImagePath);
        Assert.Equal(filterName, ex.FilterName);
        Assert.Equal(attemptNumber, ex.AttemptNumber);
    }

    [Fact]
    public void ProcessingException_OnlyMessage_ShouldHaveNullOptionalProperties()
    {
        // Arrange
        const string message = "Simple failure";

        // Act
        var ex = new ProcessingException(message);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Null(ex.ImagePath);
        Assert.Null(ex.FilterName);
        Assert.Null(ex.AttemptNumber);
    }

    [Fact]
    public void ProcessingException_WithInnerException_ShouldPreserveInnerAndProperties()
    {
        // Arrange
        var inner = new InvalidOperationException("inner");
        const string message = "Failed with inner";
        const string imagePath = "/tmp/img.jpg";
        const string filterName = "Sharpen";

        // Act
        var ex = new ProcessingException(message, inner, imagePath, filterName);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal(imagePath, ex.ImagePath);
        Assert.Equal(filterName, ex.FilterName);
        Assert.Null(ex.AttemptNumber);
    }

    [Fact]
    public void InvalidFilterException_AllPropertiesSet_ShouldExposeValues()
    {
        // Arrange
        const string message = "Invalid filter configuration";
        const string filterType = "Gaussian";
        string[] invalidParams = { "sigma", "radius" };
        const string imagePath = "/tmp/img.tif";
        const int attemptNumber = 1;

        // Act
        var ex = new InvalidFilterException(message, filterType, invalidParams, imagePath, attemptNumber);

        // Assert
        Assert.IsType<InvalidFilterException>(ex);
        Assert.IsAssignableFrom<ProcessingException>(ex);
        Assert.Equal(message, ex.Message);
        Assert.Equal(filterType, ex.FilterType);
        Assert.Equal(invalidParams, ex.InvalidParameters);
        Assert.Equal(imagePath, ex.ImagePath);
        Assert.Equal(attemptNumber, ex.AttemptNumber);
    }

    [Fact]
    public void InvalidFilterException_WithInnerException_ShouldPreserveInnerAndProperties()
    {
        // Arrange
        var inner = new Exception("inner");
        const string message = "Invalid filter";
        const string filterType = "Median";

        // Act
        var ex = new InvalidFilterException(message, inner, filterType, "/tmp/img.bmp");

        // Assert
        Assert.IsType<InvalidFilterException>(ex);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal(filterType, ex.FilterType);
        Assert.Equal("/tmp/img.bmp", ex.ImagePath);
        Assert.Null(ex.AttemptNumber);
    }

    [Fact]
    public void InvalidImageException_AllPropertiesSet_ShouldExposeValues()
    {
        // Arrange
        const string message = "Corrupted image";
        const string imagePath = "/tmp/bad.jpg";
        const string imageFormat = "jpeg";
        const string filterName = "Contrast";
        const int attemptNumber = 3;

        // Act
        var ex = new InvalidImageException(message, imagePath, imageFormat, filterName, attemptNumber);

        // Assert
        Assert.IsType<InvalidImageException>(ex);
        Assert.IsAssignableFrom<ProcessingException>(ex);
        Assert.Equal(message, ex.Message);
        Assert.Equal(imagePath, ex.ImagePath);
        Assert.Equal(imageFormat, ex.ImageFormat);
        Assert.Equal(filterName, ex.FilterName);
        Assert.Equal(attemptNumber, ex.AttemptNumber);
    }

    [Fact]
    public void InvalidImageException_WithInnerException_ShouldPreserveInnerAndProperties()
    {
        // Arrange
        var inner = new Exception("inner");
        const string message = "Invalid image";
        const string imagePath = "/tmp/bad.png";

        // Act
        var ex = new InvalidImageException(message, inner, imagePath);

        // Assert
        Assert.IsType<InvalidImageException>(ex);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal(imagePath, ex.ImagePath);
        Assert.Null(ex.ImageFormat);
    }

    [Fact]
    public void Constructors_NullMessage_ShouldThrowArgumentNullException()
    {
        // ProcessingException
        Assert.Throws<ArgumentNullException>(() => new ProcessingException(null!));

        // InvalidFilterException
        Assert.Throws<ArgumentNullException>(() => new InvalidFilterException(null!));

        // InvalidImageException
        Assert.Throws<ArgumentNullException>(() => new InvalidImageException(null!));
    }
}
