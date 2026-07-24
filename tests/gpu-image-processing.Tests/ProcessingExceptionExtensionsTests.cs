using Xunit;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests.Core;

public class ProcessingExceptionExtensionsTests
{
    [Fact]
    public void GetDescription_HappyPath_ReturnsExpectedDescription()
    {
        // Arrange
        var exception = new ProcessingException("Test message", "Test image path", "Test filter name");

        // Act
        var description = exception.GetDescription();

        // Assert
        Assert.Equal("Test message Image path: Test image path Filter name: Test filter name", description);
    }

    [Fact]
    public void GetDescription_NullException_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new ProcessingException(null).GetDescription());
    }

    [Fact]
    public void IsInvalidImageException_HappyPath_ReturnsTrue()
    {
        // Arrange
        var exception = new InvalidImageException("Test message");

        // Act
        var result = exception.IsInvalidImageException();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInvalidImageException_NullException_ReturnsFalse()
    {
        // Act and Assert
        Assert.False((new ProcessingException("Test message")).IsInvalidImageException());
    }

    [Fact]
    public void IsInvalidFilterException_HappyPath_ReturnsTrue()
    {
        // Arrange
        var exception = new InvalidFilterException("Test message");

        // Act
        var result = exception.IsInvalidFilterException();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInvalidFilterException_NullException_ReturnsFalse()
    {
        // Act and Assert
        Assert.False((new ProcessingException("Test message")).IsInvalidFilterException());
    }
}
