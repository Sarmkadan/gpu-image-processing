using Xunit;
using System.Text.Json;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Tests;

public class FilterConfigurationRepositoryJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var filterConfigurationRepository = new FilterConfigurationRepository();

        // Act
        var json = FilterConfigurationRepositoryJsonExtensions.ToJson(filterConfigurationRepository);

        // Assert
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterConfigurationRepositoryJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsFilterConfigurationRepository()
    {
        // Arrange
        var filterConfigurationRepository = new FilterConfigurationRepository();
        var json = FilterConfigurationRepositoryJsonExtensions.ToJson(filterConfigurationRepository);

        // Act
        var result = FilterConfigurationRepositoryJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterConfigurationRepositoryJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyInput_ThrowsArgumentException()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => FilterConfigurationRepositoryJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndFilterConfigurationRepository()
    {
        // Arrange
        var filterConfigurationRepository = new FilterConfigurationRepository();
        var json = FilterConfigurationRepositoryJsonExtensions.ToJson(filterConfigurationRepository);

        // Act
        var result = FilterConfigurationRepositoryJsonExtensions.TryFromJson(json, out var filterConfigurationRepositoryResult);

        // Assert
        Assert.True(result);
        Assert.NotNull(filterConfigurationRepositoryResult);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => FilterConfigurationRepositoryJsonExtensions.TryFromJson(null, out _));
    }

    [Fact]
    public void TryFromJson_EmptyInput_ThrowsArgumentException()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => FilterConfigurationRepositoryJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
