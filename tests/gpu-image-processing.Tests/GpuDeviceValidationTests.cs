using System;
using System.Collections.Generic;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="GpuDeviceValidation"/> static class.
/// </summary>
public class GpuDeviceValidationTests
{
    private static GpuDevice CreateValidDevice()
    {
        return new GpuDevice
        {
            Name = "Test GPU",
            DeviceType = GpuDeviceType.Gpu,
            Vendor = "Test Vendor",
            Version = "1.0",
            Driver = "1.0.0",
            GlobalMemoryBytes = 1024 * 1024 * 1024, // 1 GB
            MaxAllocatableMemoryBytes = 1024 * 1024 * 512, // 512 MB
            MaxComputeUnits = 16,
            MaxWorkGroupSize = 256,
            MaxWorkItemDimensions = 3,
            MaxWorkItemSizes = new int[] { 256, 256, 256 },
            MaxClockFrequencyMhz = 1500,
            ComputeCapabilityMajor = 1,
            ComputeCapabilityMinor = 0,
            Extensions = new Dictionary<string, string> { { "ext1", "val1" } },
            SupportedFormats = new List<string> { "RGBA" },
            WavefrontSize = 64
        };
    }

    [Fact]
    public void ValidateDevice_ValidDevice_ReturnsEmptyList()
    {
        // Arrange
        var device = CreateValidDevice();

        // Act
        var result = device.ValidateDevice();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateDevice_InvalidDevice_ReturnsErrorList()
    {
        // Arrange
        var device = CreateValidDevice();
        device.Name = string.Empty;
        device.GlobalMemoryBytes = 0;
        device.DeviceType = GpuDeviceType.Unknown;

        // Act
        var result = device.ValidateDevice();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Name cannot be null, empty, or whitespace.", result);
        Assert.Contains("GlobalMemoryBytes must be greater than zero.", result);
        Assert.Contains("DeviceType cannot be Unknown.", result);
    }

    [Fact]
    public void ValidateDevice_NullDevice_ThrowsArgumentNullException()
    {
        // Arrange
        GpuDevice? device = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => device!.ValidateDevice());
    }

    [Fact]
    public void IsValid_ValidDevice_ReturnsTrue()
    {
        // Arrange
        var device = CreateValidDevice();

        // Act
        var result = device.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidDevice_ReturnsFalse()
    {
        // Arrange
        var device = CreateValidDevice();
        device.Name = " ";

        // Act
        var result = device.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_NullDevice_ReturnsFalse()
    {
        // Arrange
        GpuDevice? device = null;

        // Act
        var result = device.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidDevice_DoesNotThrow()
    {
        // Arrange
        var device = CreateValidDevice();

        // Act & Assert
        var exception = Record.Exception(() => device.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidDevice_ThrowsArgumentException()
    {
        // Arrange
        var device = CreateValidDevice();
        device.MaxComputeUnits = -1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => device.EnsureValid());
    }

    [Fact]
    public void EnsureValid_NullDevice_ThrowsArgumentNullException()
    {
        // Arrange
        GpuDevice? device = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => device!.EnsureValid());
    }
}
