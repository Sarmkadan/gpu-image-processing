using System;
using Xunit;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests.Core;

/// <summary>
/// Contains unit tests for the <see cref="GpuExceptionValidation"/> static class.
/// Tests the validation, IsValid, and EnsureValid extension methods for <see cref="GpuException"/>.
/// </summary>
public class GpuExceptionValidationTests
{
    [Fact]
    public void Validate_ValidGpuException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-001", 42);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidGpuExceptionWithNullDeviceName_ReturnsEmptyList()
    {
        // Arrange
        var exception = new GpuException("Test error", deviceName: null);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_ValidGpuExceptionWithEmptyDeviceName_ReturnsEmptyList()
    {
        // Arrange
        var exception = new GpuException("Test error", deviceName: string.Empty);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_GpuExceptionWithWhitespaceDeviceName_ReturnsEmptyList()
    {
        // Arrange
        var exception = new GpuException("Test error", deviceName: " ");

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_GpuExceptionWithNegativeErrorCode_ReturnsProblemList()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-007", -1);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ErrorCode is out of range.", result);
    }

    [Fact]
    public void Validate_GpuExceptionWithLargeNegativeErrorCode_ReturnsProblemList()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-008", int.MinValue);

        // Act
        var result = exception.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ErrorCode is out of range.", result);
    }

    [Fact]
    public void Validate_NullGpuException_ThrowsArgumentNullException()
    {
        // Arrange
        GpuException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.Validate());
    }

    [Fact]
    public void IsValid_ValidGpuException_ReturnsTrue()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-009", 123);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ValidGpuExceptionWithNullDeviceName_ReturnsTrue()
    {
        // Arrange
        var exception = new GpuException("Test error", deviceName: null);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_NullGpuException_ThrowsArgumentNullException()
    {
        // Arrange
        GpuException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.IsValid());
    }

    [Fact]
    public void EnsureValid_ValidGpuException_DoesNotThrow()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-011", 456);

        // Act
        var act = () => exception.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureValid_ValidGpuExceptionWithNullDeviceName_DoesNotThrow()
    {
        // Arrange
        var exception = new GpuException("Test error", deviceName: null);

        // Act
        var act = () => exception.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureValid_NullGpuException_ThrowsArgumentNullException()
    {
        // Arrange
        GpuException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.EnsureValid());
    }

    [Fact]
    public void Validate_InvalidGpuException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-012", 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => exception.Validate());
    }

    [Fact]
    public void IsValid_InvalidGpuException_ReturnsFalse()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-013", 0);

        // Act
        var result = exception.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_InvalidGpuException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new GpuException("Test error", "GPU-014", 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => exception.EnsureValid());
    }
}
