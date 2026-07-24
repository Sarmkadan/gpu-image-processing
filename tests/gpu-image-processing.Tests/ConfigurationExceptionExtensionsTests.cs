#nullable enable
using System;
using FluentAssertions;
using GpuImageProcessing.Exceptions;
using Xunit;

namespace GpuImageProcessing.Tests;

public class ConfigurationExceptionExtensionsTests
{
    [Fact]
    public void IsInvalidKey_ReturnsTrue_WhenKeyIsNonEmptyAndValueIsNull()
    {
        // Arrange: key set, value null
        var ex = new ConfigurationException(
            message: "test",
            configurationKey: "SomeKey",
            configurationValue: (string?)null,
            errorCode: null);

        // Act
        bool result = ex.IsInvalidKey();

        // Assert
        result.Should().BeTrue();
        ex.IsInvalidValue().Should().BeFalse();
    }

    [Fact]
    public void IsInvalidValue_ReturnsTrue_WhenValueIsNonEmptyAndKeyIsNull()
    {
        // Arrange: key null, value set
        var ex = new ConfigurationException(
            message: "test",
            configurationKey: (string?)null,
            configurationValue: "SomeValue",
            errorCode: null);

        // Act
        bool result = ex.IsInvalidValue();

        // Assert
        result.Should().BeTrue();
        ex.IsInvalidKey().Should().BeFalse();
    }

    [Fact]
    public void IsInvalidKeyAndValue_ReturnsFalse_WhenBothKeyAndValueAreProvided()
    {
        // Arrange: both key and value set
        var ex = new ConfigurationException(
            message: "test",
            configurationKey: "Key",
            configurationValue: "Value",
            errorCode: null);

        // Act & Assert
        ex.IsInvalidKey().Should().BeFalse();
        ex.IsInvalidValue().Should().BeFalse();
    }

    [Fact]
    public void GetFormattedError_IncludesInvalidKeyMessage()
    {
        // Arrange
        var ex = new ConfigurationException(
            message: "test",
            configurationKey: "Key",
            configurationValue: (string?)null,
            errorCode: null);

        // Act
        string formatted = ex.GetFormattedError();

        // Assert
        formatted.Should().Contain("Invalid configuration key");
    }

    [Fact]
    public void GetFormattedError_IncludesInvalidValueMessage()
    {
        // Arrange
        var ex = new ConfigurationException(
            message: "test",
            configurationKey: (string?)null,
            configurationValue: "Value",
            errorCode: null);

        // Act
        string formatted = ex.GetFormattedError();

        // Assert
        formatted.Should().Contain("Invalid configuration value");
    }

    [Fact]
    public void GetFormattedError_IncludesGeneralMessage_WhenNeitherKeyNorValueInvalid()
    {
        // Arrange
        var ex = new ConfigurationException(
            message: "test",
            configurationKey: "Key",
            configurationValue: "Value",
            errorCode: null);

        // Act
        string formatted = ex.GetFormattedError();

        // Assert
        formatted.Should().Contain("Configuration validation failed");
    }

    [Fact]
    public void ExtensionMethods_ThrowArgumentNullException_WhenExceptionIsNull()
    {
        // Arrange
        ConfigurationException? nullEx = null;

        // Act
        Action actKey = () => ConfigurationExceptionExtensions.IsInvalidKey(nullEx!);
        Action actValue = () => ConfigurationExceptionExtensions.IsInvalidValue(nullEx!);
        Action actFmt = () => ConfigurationExceptionExtensions.GetFormattedError(nullEx!);

        // Assert
        actKey.Should().Throw<ArgumentNullException>();
        actValue.Should().Throw<ArgumentNullException>();
        actFmt.Should().Throw<ArgumentNullException>();
    }
}
