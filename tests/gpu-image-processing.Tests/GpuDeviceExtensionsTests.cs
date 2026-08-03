using System;
using System.Collections.Generic;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Domain;

public class GpuDeviceExtensionsTests
{
    private static GpuDevice CreateValidDevice()
    {
        return new GpuDevice
        {
            GlobalMemoryBytes = 1024L * 1024 * 1024, // 1 GB
            MaxAllocatableMemoryBytes = 1024L * 1024 * 512, // 512 MB
            SupportedFormats = new List<string> { "RGBA", "sRGB" },
            DeviceType = GpuDeviceType.Gpu
        };
    }

    [Fact]
    public void GetTotalMemoryMb_ValidDevice_ReturnsCorrectMb()
    {
        var device = CreateValidDevice();
        Assert.Equal(1024, device.GetTotalMemoryMb());
    }

    [Fact]
    public void GetTotalMemoryMb_NullDevice_ThrowsArgumentNullException()
    {
        GpuDevice? device = null;
        Assert.Throws<ArgumentNullException>(() => device!.GetTotalMemoryMb());
    }

    [Fact]
    public void GetAvailableMemoryMb_ValidDevice_ReturnsCorrectMb()
    {
        var device = CreateValidDevice();
        Assert.Equal(512, device.GetAvailableMemoryMb());
    }

    [Fact]
    public void GetAvailableMemoryMb_NullDevice_ThrowsArgumentNullException()
    {
        GpuDevice? device = null;
        Assert.Throws<ArgumentNullException>(() => device!.GetAvailableMemoryMb());
    }

    [Fact]
    public void SupportsColorSpace_ValidFormat_ReturnsTrue()
    {
        var device = CreateValidDevice();
        Assert.True(device.SupportsColorSpace("RGBA"));
        Assert.True(device.SupportsColorSpace("srgb"));
    }

    [Fact]
    public void SupportsColorSpace_InvalidFormat_ReturnsFalse()
    {
        var device = CreateValidDevice();
        Assert.False(device.SupportsColorSpace("BGR"));
        Assert.False(device.SupportsColorSpace(""));
    }

    [Fact]
    public void SupportsColorSpace_NullDevice_ThrowsArgumentNullException()
    {
        GpuDevice? device = null;
        Assert.Throws<ArgumentNullException>(() => device!.SupportsColorSpace("RGBA"));
    }

    [Fact]
    public void GetDeviceTypeDisplayName_ValidDevice_ReturnsCorrectName()
    {
        var device = CreateValidDevice();
        device.DeviceType = GpuDeviceType.Gpu;
        Assert.Equal("GPU", device.GetDeviceTypeDisplayName());
        
        device.DeviceType = GpuDeviceType.Cpu;
        Assert.Equal("CPU", device.GetDeviceTypeDisplayName());
    }
}
