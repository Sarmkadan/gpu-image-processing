using Xunit;
using GpuImageProcessing.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Exceptions;

public class ConfigurationExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");

        // Act
        var json = exception.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("message", json);
        Assert.Contains("stackTrace", json);

        // Verify it's valid JSON
        var deserialized = JsonSerializer.Deserialize<ConfigurationException>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("Test message", deserialized.Message);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ConfigurationException)null)!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"message\":\"Test message\",\"stackTrace\":\"Test stack trace\"}";

        // Act
        var exception = ConfigurationExceptionJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Test message", exception.Message);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ConfigurationExceptionJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ConfigurationExceptionJsonExtensions.FromJson(""));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{\"invalid\":json}";

        // Act & Assert
        Assert.Throws<JsonException>(() => ConfigurationExceptionJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var json = "{\"message\":\"Test message\",\"stackTrace\":\"Test stack trace\"}";

        // Act
        var success = ConfigurationExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        Assert.True(success);
        Assert.NotNull(exception);
        Assert.Equal("Test message", exception.Message);
    }

    [Fact]
    public void TryFromJson_NullJson_ReturnsFalseAndNull()
    {
        // Act
        var success = ConfigurationExceptionJsonExtensions.TryFromJson(null, out var exception);

        // Assert
        Assert.False(success);
        Assert.Null(exception);
    }

    [Fact]
    public void TryFromJson_EmptyOrWhitespaceJson_ReturnsFalseAndNull()
    {
        // Act
        var success1 = ConfigurationExceptionJsonExtensions.TryFromJson("", out var exception1);
        var success2 = ConfigurationExceptionJsonExtensions.TryFromJson("   ", out var exception2);

        // Assert
        Assert.False(success1);
        Assert.Null(exception1);

        Assert.False(success2);
        Assert.Null(exception2);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{\"invalid\":json}";

        // Act
        var success = ConfigurationExceptionJsonExtensions.TryFromJson(invalidJson, out var exception);

        // Assert
        Assert.False(success);
        Assert.Null(exception);
    }
}
