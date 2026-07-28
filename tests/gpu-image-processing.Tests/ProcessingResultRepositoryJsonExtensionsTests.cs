using System;
using System.Text.Json;
using Xunit;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Core.Repository; // added in case the type lives in this namespace

namespace GpuImageProcessing.Tests;

public class ProcessingResultRepositoryJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidRepository_ReturnsJsonString()
    {
        // Arrange
        var repository = new ProcessingResultRepository(); // assumes a public parameter‑less constructor

        // Act
        string json = repository.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_NullRepository_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResultRepository? repository = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => repository!.ToJson());
    }

    [Fact]
    public void FromJson_NullOrWhitespace_ReturnsNull()
    {
        // Null input
        Assert.Null(ProcessingResultRepositoryJsonExtensions.FromJson(null!));

        // Empty string
        Assert.Null(ProcessingResultRepositoryJsonExtensions.FromJson(string.Empty));

        // Whitespace only
        Assert.Null(ProcessingResultRepositoryJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        string invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<JsonException>(() => ProcessingResultRepositoryJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndDeserializedObject()
    {
        // Arrange
        var repository = new ProcessingResultRepository(); // assumes a public parameter‑less constructor
        string json = repository.ToJson();

        // Act
        bool success = ProcessingResultRepositoryJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        Assert.True(success);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        string invalidJson = "not a json";

        // Act
        bool success = ProcessingResultRepositoryJsonExtensions.TryFromJson(invalidJson, out var deserialized);

        // Assert
        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsFalse()
    {
        // Act
        bool success = ProcessingResultRepositoryJsonExtensions.TryFromJson(string.Empty, out var deserialized);

        // Assert
        Assert.False(success);
        Assert.Null(deserialized);
    }
}
