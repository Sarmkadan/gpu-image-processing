using Xunit;
using GpuImageProcessing.Exceptions;

namespace GpuImageProcessing.Tests.Exceptions;

public class ConfigurationExceptionTests
{
    [Fact]
    public void Constructor_MessageOnly_CreatesExceptionWithMessage()
    {
        // Arrange
        var message = "Test configuration error";

        // Act
        var exception = new ConfigurationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.ConfigurationKey);
        Assert.Null(exception.ConfigurationValue);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void Constructor_MessageAndConfigurationKey_CreatesExceptionWithKey()
    {
        // Arrange
        var message = "Invalid configuration value";
        var configKey = "MaxThreads";

        // Act
        var exception = new ConfigurationException(message, configKey);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(configKey, exception.ConfigurationKey);
        Assert.Null(exception.ConfigurationValue);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void Constructor_MessageAndConfigurationKeyAndValue_CreatesExceptionWithKeyAndValue()
    {
        // Arrange
        var message = "Invalid configuration value";
        var configKey = "MaxThreads";
        var configValue = "100";

        // Act
        var exception = new ConfigurationException(message, configKey, configValue);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(configKey, exception.ConfigurationKey);
        Assert.Equal(configValue, exception.ConfigurationValue);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void Constructor_MessageAndConfigurationKeyAndValueAndErrorCode_CreatesExceptionWithAllProperties()
    {
        // Arrange
        var message = "Invalid configuration value";
        var configKey = "MaxThreads";
        var configValue = "100";
        var errorCode = 400;

        // Act
        var exception = new ConfigurationException(message, configKey, configValue, errorCode);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(configKey, exception.ConfigurationKey);
        Assert.Equal(configValue, exception.ConfigurationValue);
        Assert.Equal(errorCode, exception.ErrorCode);
    }

    [Fact]
    public void Constructor_MessageInnerExceptionAndConfigurationKey_CreatesExceptionWithInnerExceptionAndKey()
    {
        // Arrange
        var message = "Configuration failed";
        var innerException = new ArgumentException("Inner error");
        var configKey = "Timeout";

        // Act
        var exception = new ConfigurationException(message, innerException, configKey);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(configKey, exception.ConfigurationKey);
        Assert.Null(exception.ConfigurationValue);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void Constructor_MessageInnerExceptionAndConfigurationKeyAndErrorCode_CreatesExceptionWithAllProperties()
    {
        // Arrange
        var message = "Configuration failed";
        var innerException = new ArgumentException("Inner error");
        var configKey = "Timeout";
        var errorCode = 500;

        // Act
        var exception = new ConfigurationException(message, innerException, configKey, errorCode);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(configKey, exception.ConfigurationKey);
        Assert.Null(exception.ConfigurationValue);
        Assert.Equal(errorCode, exception.ErrorCode);
    }

    [Fact]
    public void Constructor_NullMessage_CreatesException()
    {
        // Arrange & Act
        var exception = new ConfigurationException(null!);

        // Assert
        Assert.NotNull(exception.Message);
        Assert.Null(exception.ConfigurationKey);
        Assert.Null(exception.ConfigurationValue);
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void Constructor_EmptyMessage_CreatesExceptionWithEmptyMessage()
    {
        // Arrange
        var message = string.Empty;

        // Act
        var exception = new ConfigurationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WhitespaceMessage_CreatesExceptionWithWhitespaceMessage()
    {
        // Arrange
        var message = "   ";

        // Act
        var exception = new ConfigurationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ConfigurationKey_IsReadOnlyProperty()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");

        // Act & Assert
        Assert.Null(exception.ConfigurationKey);
        Assert.Equal("ConfigurationKey", typeof(ConfigurationException).GetProperty("ConfigurationKey")?.Name);
    }

    [Fact]
    public void ConfigurationValue_IsReadOnlyProperty()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");

        // Act & Assert
        Assert.Null(exception.ConfigurationValue);
        Assert.Equal("ConfigurationValue", typeof(ConfigurationException).GetProperty("ConfigurationValue")?.Name);
    }

    [Fact]
    public void ErrorCode_IsReadOnlyProperty()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");

        // Act & Assert
        Assert.Null(exception.ErrorCode);
        Assert.Equal("ErrorCode", typeof(ConfigurationException).GetProperty("ErrorCode")?.Name);
    }

    [Fact]
    public void ToString_WithConfigurationKeyAndValue_ReturnsFormattedString()
    {
        // Arrange
        var message = "Configuration error";
        var configKey = "MaxThreads";
        var configValue = "invalid";
        var exception = new ConfigurationException(message, configKey, configValue, 400);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(message, result);
        Assert.Contains(configKey, result);
        Assert.Contains(configValue, result);
        Assert.Contains("400", result);
        Assert.Contains("Occurred:", result);
    }

    [Fact]
    public void ToString_WithOnlyConfigurationKey_ReturnsFormattedString()
    {
        // Arrange
        var message = "Configuration error";
        var configKey = "MaxThreads";
        var exception = new ConfigurationException(message, configKey);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(message, result);
        Assert.Contains(configKey, result);
        Assert.DoesNotContain("Configuration Value:", result);
    }

    [Fact]
    public void ToString_WithOnlyConfigurationValue_ReturnsFormattedString()
    {
        // Arrange
        var message = "Configuration error";
        var configValue = "invalid";
        var exception = new ConfigurationException(message, configurationKey: null, configurationValue: configValue);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(message, result);
        Assert.Contains(configValue, result);
        Assert.DoesNotContain("Configuration Key:", result);
    }

    [Fact]
    public void ToString_WithNullConfigurationKeyAndValue_ReturnsBaseToString()
    {
        // Arrange
        var message = "Configuration error";
        var exception = new ConfigurationException(message);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(message, result);
        Assert.DoesNotContain("Configuration Key:", result);
        Assert.DoesNotContain("Configuration Value:", result);
    }

    [Fact]
    public void ToString_IncludesErrorCodeAndTimestamp()
    {
        // Arrange
        var message = "Configuration error";
        var exception = new ConfigurationException(message, errorCode: 422);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(message, result);
        Assert.Contains("422", result);
        Assert.Contains("Occurred:", result);
        Assert.Matches(@"Occurred: \d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z", result);
    }

    [Fact]
    public void InheritsFromGpuImageProcessingException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");

        // Act & Assert
        Assert.IsAssignableFrom<GpuImageProcessingException>(exception);
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void OccurredAt_PropertyIsSetOnConstruction()
    {
        // Arrange
        var beforeConstruction = DateTime.UtcNow;
        var exception = new ConfigurationException("Test message");
        var afterConstruction = DateTime.UtcNow;

        // Act & Assert
        Assert.InRange(exception.OccurredAt, beforeConstruction, afterConstruction);
    }

    [Fact]
    public void ConfigurationKey_CanBeNull()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Test message", configurationKey: null);

        // Assert
        Assert.Null(exception.ConfigurationKey);
    }

    [Fact]
    public void ConfigurationValue_CanBeNull()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Test message", configurationValue: null);

        // Assert
        Assert.Null(exception.ConfigurationValue);
    }

    [Fact]
    public void ErrorCode_CanBeNull()
    {
        // Arrange & Act
        var exception = new ConfigurationException("Test message", errorCode: null);

        // Assert
        Assert.Null(exception.ErrorCode);
    }

    [Fact]
    public void ConfigurationKey_EmptyString_IsStored()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationKey: string.Empty);

        // Act & Assert
        Assert.Equal(string.Empty, exception.ConfigurationKey);
    }

    [Fact]
    public void ConfigurationValue_EmptyString_IsStored()
    {
        // Arrange
        var exception = new ConfigurationException("Test message", configurationValue: string.Empty);

        // Act & Assert
        Assert.Equal(string.Empty, exception.ConfigurationValue);
    }

    [Fact]
    public void ConfigurationKey_WhitespaceString_IsStored()
    {
        // Arrange
        var whitespaceKey = "   ";
        var exception = new ConfigurationException("Test message", configurationKey: whitespaceKey);

        // Act & Assert
        Assert.Equal(whitespaceKey, exception.ConfigurationKey);
    }

    [Fact]
    public void ConfigurationValue_WhitespaceString_IsStored()
    {
        // Arrange
        var whitespaceValue = "   ";
        var exception = new ConfigurationException("Test message", configurationValue: whitespaceValue);

        // Act & Assert
        Assert.Equal(whitespaceValue, exception.ConfigurationValue);
    }
}