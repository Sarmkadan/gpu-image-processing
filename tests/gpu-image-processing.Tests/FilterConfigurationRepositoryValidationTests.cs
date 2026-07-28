using Xunit;
using GpuImageProcessing.Repository;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests;

public class FilterConfigurationRepositoryValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var repository = new FilterConfigurationRepository();

        // Act
        var result = FilterConfigurationRepositoryValidation.Validate(repository);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var repository = new FilterConfigurationRepository();

        // Act
        var result = FilterConfigurationRepositoryValidation.IsValid(repository);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var repository = new FilterConfigurationRepository();

        // Act and Assert
        FilterConfigurationRepositoryValidation.EnsureValid(repository);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterConfigurationRepositoryValidation.Validate(null));
    }

    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterConfigurationRepositoryValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterConfigurationRepositoryValidation.EnsureValid(null));
    }
}
