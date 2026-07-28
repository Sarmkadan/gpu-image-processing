// tests/gpu-image-processing.Tests/FilterServiceValidationTests.cs
using Xunit;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.Tests;

public class FilterServiceValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var filterService = new FilterService(null, null); // assuming FilterService has a constructor with two parameters

        // Act
        var result = FilterServiceValidation.Validate(filterService);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var filterService = new FilterService(null, null); // assuming FilterService has a constructor with two parameters

        // Act
        var result = FilterServiceValidation.IsValid(filterService);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_NullInput_ReturnsFalse()
    {
        // Act
        var result = FilterServiceValidation.IsValid(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var filterService = new FilterService(null, null); // assuming FilterService has a constructor with two parameters

        // Act and Assert
        FilterServiceValidation.EnsureValid(filterService);
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterServiceValidation.EnsureValid(null));
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterServiceValidation.Validate(null));
    }
}
