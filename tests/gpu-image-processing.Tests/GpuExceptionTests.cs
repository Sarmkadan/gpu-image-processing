// SPDX-License-Identifier: MIT
// Tests for GpuException
// -------------------------------------------------------------

using System;
using GpuImageProcessing.Core;
using Xunit;

namespace GpuImageProcessing.Tests;

public class GpuExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndOptionalParameters_SetsProperties()
    {
        // Arrange
        const string message = "GPU operation failed";
        const string deviceName = "NVIDIA GTX 3080";
        const int errorCode = 0xDEAD;

        // Act
        var ex = new GpuException(message, deviceName, errorCode);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Equal(deviceName, ex.DeviceName);
        Assert.Equal(errorCode, ex.ErrorCode);
        // OccurredAt should be set to a recent time (within a few seconds)
        Assert.True((DateTime.UtcNow - ex.OccurredAt).TotalSeconds < 5);
    }

    [Fact]
    public void Constructor_WithInnerException_SetsInnerExceptionAndProperties()
    {
        // Arrange
        var inner = new InvalidOperationException("inner exception");
        const string message = "GPU error with inner";
        const string deviceName = "AMD Radeon";
        const int errorCode = 1234;

        // Act
        var ex = new GpuException(message, inner, deviceName, errorCode);

        // Assert
        Assert.Equal(message, ex.Message);
        Assert.Same(inner, ex.InnerException);
        Assert.Equal(deviceName, ex.DeviceName);
        Assert.Equal(errorCode, ex.ErrorCode);
    }

    [Fact]
    public void ToString_IncludesDeviceWhenDeviceNameIsProvided()
    {
        // Arrange
        var ex = new GpuException("failure", "Intel HD", 42);

        // Act
        var result = ex.ToString();

        // Assert
        Assert.Contains("failure", result);
        Assert.Contains("Device: Intel HD", result);
    }

    [Fact]
    public void ToString_OmitsDeviceWhenDeviceNameIsNullOrEmpty()
    {
        // Arrange
        var ex = new GpuException("failure", null, 42);

        // Act
        var result = ex.ToString();

        // Assert
        Assert.Contains("failure", result);
        Assert.DoesNotContain("Device:", result);
    }

    [Fact]
    public void Constructor_NullMessage_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GpuException(null!));
    }
}
