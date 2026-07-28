using System;
using Xunit;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Tests;

public class ImageRepositoryJsonExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidRepository_ReturnsNonEmptyJson()
    {
        // Arrange
        var repository = new ImageRepository(); // assumes a parameterless constructor

        // Act
        string json = ImageRepositoryJsonExtensionsJsonExtensions.ToJson(repository);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));

        // Verify round‑trip deserialization works
        var roundTrip = ImageRepositoryJsonExtensionsJsonExtensions.FromJson(json);
        Assert.NotNull(roundTrip);
    }

    [Fact]
    public void ToJson_NullRepository_ThrowsArgumentNullException()
    {
        // Arrange
        ImageRepository? repository = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ImageRepositoryJsonExtensionsJsonExtensions.ToJson(repository!));
    }

    [Fact]
    public void FromJson_NullOrWhiteSpace_ReturnsNull()
    {
        // Null input
        Assert.Null(ImageRepositoryJsonExtensionsJsonExtensions.FromJson(null));

        // Empty string
        Assert.Null(ImageRepositoryJsonExtensionsJsonExtensions.FromJson(string.Empty));

        // Whitespace only
        Assert.Null(ImageRepositoryJsonExtensionsJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndInstance()
    {
        // Arrange
        var repository = new ImageRepository();
        string json = ImageRepositoryJsonExtensionsJsonExtensions.ToJson(repository);

        // Act
        bool success = ImageRepositoryJsonExtensionsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act
        bool success = ImageRepositoryJsonExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
