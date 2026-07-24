using Xunit;
using GpuImageProcessing.Exceptions;

namespace GpuImageProcessing.Tests.Exceptions;

public class ConfigurationExceptionValidationTests
{
    [Fact]
    public void Validate_ValidConfigurationException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidConfigurationExceptionWithErrorCode_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue", 50);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithNullKey_ReturnsProblemList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: null, configurationValue: "ValidValue");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ConfigurationKey is null or empty.", result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithEmptyKey_ReturnsProblemList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: string.Empty, configurationValue: "ValidValue");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ConfigurationKey is null or empty.", result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithWhitespaceKey_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: " ", configurationValue: "ValidValue");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithNullValue_ReturnsProblemList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", configurationValue: null);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ConfigurationValue is null or empty.", result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithEmptyValue_ReturnsProblemList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", configurationValue: string.Empty);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ConfigurationValue is null or empty.", result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithWhitespaceValue_ReturnsEmptyList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", configurationValue: " ");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithOutOfRangeErrorCode_ReturnsProblemList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue", -1);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ErrorCode is out of range (0-100).", result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithOutOfRangeErrorCodeAbove100_ReturnsProblemList()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue", 101);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ErrorCode is out of range (0-100).", result);
    }

    [Fact]
    public void Validate_ConfigurationExceptionWithMultipleProblems_ReturnsAllProblems()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: null, configurationValue: string.Empty, errorCode: -5);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("ConfigurationKey is null or empty.", result);
        Assert.Contains("ConfigurationValue is null or empty.", result);
        Assert.Contains("ErrorCode is out of range (0-100).", result);
    }

    [Fact]
    public void IsValid_ValidConfigurationException_ReturnsTrue()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue");

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ValidConfigurationExceptionWithErrorCode_ReturnsTrue()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue", 75);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidConfigurationException_ReturnsFalse()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: null);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_NullConfigurationException_ThrowsArgumentNullException()
    {
        // Arrange
        ConfigurationException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.IsValid());
    }

    [Fact]
    public void EnsureValid_ValidConfigurationException_DoesNotThrow()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue");

        // Act
        var act = () => exception.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureValid_ValidConfigurationExceptionWithErrorCode_DoesNotThrow()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", "ValidKey", "ValidValue", 25);

        // Act
        var act = () => exception.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureValid_InvalidConfigurationException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: null);

        // Act & Assert
        var exceptionResult = Assert.Throws<ArgumentException>(() => exception.EnsureValid());
        Assert.Contains("ConfigurationKey is null or empty.", exceptionResult.Message);
    }

    [Fact]
    public void EnsureValid_NullConfigurationException_ThrowsArgumentNullException()
    {
        // Arrange
        ConfigurationException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.EnsureValid());
    }

    [Fact]
    public void EnsureValid_MultipleProblems_ThrowsArgumentExceptionWithAllProblems()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: null, configurationValue: string.Empty);

        // Act & Assert
        var exceptionResult = Assert.Throws<ArgumentException>(() => exception.EnsureValid());
        Assert.Contains("ConfigurationKey is null or empty.", exceptionResult.Message);
        Assert.Contains("ConfigurationValue is null or empty.", exceptionResult.Message);
    }
}