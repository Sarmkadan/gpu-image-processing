#nullable enable
using System;
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Pipeline;

public class WorkgroupOptimizerTests
{
    private readonly Mock<ILogger<WorkgroupOptimizer>> _loggerMock;
    private readonly WorkgroupOptimizer _optimizer;
    private readonly GpuDevice _testDevice;

    public WorkgroupOptimizerTests()
    {
        _loggerMock = new Mock<ILogger<WorkgroupOptimizer>>();
        _optimizer = new WorkgroupOptimizer(_loggerMock.Object);

        // Create a realistic test device
        _testDevice = new GpuDevice
        {
            Id = Guid.NewGuid(),
            Name = "Test GPU",
            Vendor = "Test Vendor",
            DeviceType = GpuDeviceType.Gpu,
            GlobalMemoryBytes = 8L * 1024 * 1024 * 1024, // 8 GB
            LocalMemoryBytes = 64 * 1024, // 64 KB
            MaxAllocatableMemoryBytes = 7L * 1024 * 1024 * 1024,
            MaxComputeUnits = 32,
            MaxWorkGroupSize = 1024,
            MaxWorkItemDimensions = 3,
            MaxWorkItemSizes = [1024, 1024, 64],
            MaxClockFrequencyMhz = 2000.0,
            WavefrontSize = 32,
            IsAvailable = true,
            ComputeCapabilityMajor = 8,
            ComputeCapabilityMinor = 0
        };
    }

    [Fact]
    public void Compute_WithTypicalPowerOfTwoDimensions_ReturnsValidConfiguration()
    {
        // Arrange
        int width = 1024;
        int height = 1024;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height);

        // Assert
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BePositive();
        config.WorkgroupSizeY.Should().BePositive();
        config.WorkgroupSizeX.Should().BeLessThanOrEqualTo(config.WorkgroupSizeY * 4); // Reasonable aspect ratio
        config.GlobalWorkSizeX.Should().BeGreaterThanOrEqualTo(width);
        config.GlobalWorkSizeY.Should().BeGreaterThanOrEqualTo(height);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
        config.OptimizationScore.Should().BeGreaterThan(0);
        config.EstimatedOccupancy.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void Compute_WithNonPowerOfTwoDimensions_ReturnsValidConfiguration()
    {
        // Arrange
        int width = 1920; // 1080p width
        int height = 1080; // 1080p height

        // Act
        var config = _optimizer.Compute(_testDevice, width, height);

        // Assert
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BePositive();
        config.WorkgroupSizeY.Should().BePositive();
        config.GlobalWorkSizeX.Should().BeGreaterThanOrEqualTo(width);
        config.GlobalWorkSizeY.Should().BeGreaterThanOrEqualTo(height);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_With1x1Image_ReturnsValidConfiguration()
    {
        // Arrange
        int width = 1;
        int height = 1;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height);

        // Assert
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BePositive();
        config.WorkgroupSizeY.Should().BePositive();
        config.GlobalWorkSizeX.Should().BeGreaterThanOrEqualTo(1);
        config.GlobalWorkSizeY.Should().BeGreaterThanOrEqualTo(1);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithVeryLargeDimensions_NoOverflow()
    {
        // Arrange
        int width = 32768; // 32K
        int height = 32768; // 32K

        // Act
        var config = _optimizer.Compute(_testDevice, width, height);

        // Assert - should not throw or overflow
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BePositive();
        config.WorkgroupSizeY.Should().BePositive();
        config.GlobalWorkSizeX.Should().BeGreaterThanOrEqualTo(width);
        config.GlobalWorkSizeY.Should().BeGreaterThanOrEqualTo(height);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();

        // Verify no overflow in calculations
        long totalThreads = config.GetTotalWorkgroupSize();
        totalThreads.Should().BeLessThan(int.MaxValue);
        long dispatchCount = config.GetTotalDispatchCount();
        dispatchCount.Should().BePositive();
    }

    [Fact]
    public void Compute_WithLargeImage_UsesSmallerTiles()
    {
        // Arrange - large image (> 8192 on either axis)
        int width = 16384;
        int height = 16384;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height);

        // Assert - should use smaller tiles for large images
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BeLessThanOrEqualTo(8);
        config.WorkgroupSizeY.Should().BeLessThanOrEqualTo(8);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithLocalMemoryConstraint_RespectsLimit()
    {
        // Arrange - device with small local memory
        var smallMemoryDevice = new GpuDevice
        {
            Id = Guid.NewGuid(),
            Name = "Small Memory GPU",
            Vendor = "Test Vendor",
            DeviceType = GpuDeviceType.Gpu,
            GlobalMemoryBytes = 2L * 1024 * 1024 * 1024,
            LocalMemoryBytes = 32 * 1024, // 32 KB
            MaxAllocatableMemoryBytes = 1L * 1024 * 1024 * 1024,
            MaxComputeUnits = 16,
            MaxWorkGroupSize = 512,
            MaxWorkItemDimensions = 3,
            MaxWorkItemSizes = [512, 512, 64],
            MaxClockFrequencyMhz = 1500.0,
            WavefrontSize = 32,
            IsAvailable = true,
            ComputeCapabilityMajor = 7,
            ComputeCapabilityMinor = 5
        };

        int localMemPerThread = 16; // 16 bytes per thread
        int width = 1024;
        int height = 1024;

        // Act
        var config = _optimizer.Compute(smallMemoryDevice, width, height, localMemPerThread);

        // Assert - should not exceed local memory
        config.Should().NotBeNull();
        config.LocalMemoryRequiredBytes.Should().BeLessThanOrEqualTo(smallMemoryDevice.LocalMemoryBytes);
        config.IsValidForDevice(smallMemoryDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithThroughputMaximizedStrategy_FavorsLargerWorkgroups()
    {
        // Arrange
        int width = 2048;
        int height = 2048;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height, 0, WorkgroupOptimizationStrategy.ThroughputMaximized);

        // Assert
        config.Should().NotBeNull();
        config.Strategy.Should().Be(WorkgroupOptimizationStrategy.ThroughputMaximized);
        config.OptimizationScore.Should().BeGreaterThan(0);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithLatencyMinimizedStrategy_FavorsSmallerWorkgroups()
    {
        // Arrange
        int width = 2048;
        int height = 2048;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height, 0, WorkgroupOptimizationStrategy.LatencyMinimized);

        // Assert
        config.Should().NotBeNull();
        config.Strategy.Should().Be(WorkgroupOptimizationStrategy.LatencyMinimized);
        config.WorkgroupSizeX.Should().BeInRange(4, 8);
        config.WorkgroupSizeY.Should().BeInRange(4, 8);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithMemoryOptimizedStrategy_MinimizesLocalMemory()
    {
        // Arrange
        int width = 2048;
        int height = 2048;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height, 0, WorkgroupOptimizationStrategy.MemoryOptimized);

        // Assert
        config.Should().NotBeNull();
        config.Strategy.Should().Be(WorkgroupOptimizationStrategy.MemoryOptimized);
        config.LocalMemoryRequiredBytes.Should().BeLessThanOrEqualTo(_testDevice.LocalMemoryBytes);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithBalancedStrategy_ReturnsConfiguration()
    {
        // Arrange
        int width = 2048;
        int height = 2048;

        // Act
        var config = _optimizer.Compute(_testDevice, width, height, 0, WorkgroupOptimizationStrategy.Balanced);

        // Assert
        config.Should().NotBeNull();
        config.Strategy.Should().Be(WorkgroupOptimizationStrategy.Balanced);
        config.OptimizationScore.Should().BeGreaterThan(0);
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void Compute_WithZeroWidth_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int width = 0;
        int height = 1024;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _optimizer.Compute(_testDevice, width, height));
    }

    [Fact]
    public void Compute_WithNegativeWidth_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int width = -100;
        int height = 1024;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _optimizer.Compute(_testDevice, width, height));
    }

    [Fact]
    public void Compute_WithZeroHeight_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int width = 1024;
        int height = 0;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _optimizer.Compute(_testDevice, width, height));
    }

    [Fact]
    public void Compute_WithNullDevice_ThrowsArgumentNullException()
    {
        // Arrange
        GpuDevice? nullDevice = null;
        int width = 1024;
        int height = 1024;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _optimizer.Compute(nullDevice!, width, height));
    }

    [Fact]
    public void Compute_WithDeviceWithZeroMaxWorkGroupSize_ReturnsFallbackConfiguration()
    {
        // Arrange - device with zero max workgroup size (invalid but should handle gracefully)
        var invalidDevice = new GpuDevice
        {
            Id = Guid.NewGuid(),
            Name = "Invalid Device",
            Vendor = "Test Vendor",
            DeviceType = GpuDeviceType.Gpu,
            GlobalMemoryBytes = 4L * 1024 * 1024 * 1024,
            LocalMemoryBytes = 32 * 1024,
            MaxAllocatableMemoryBytes = 3L * 1024 * 1024 * 1024,
            MaxComputeUnits = 8,
            MaxWorkGroupSize = 0, // Invalid
            MaxWorkItemDimensions = 3,
            MaxWorkItemSizes = [0, 0, 0],
            MaxClockFrequencyMhz = 1000.0,
            WavefrontSize = 0,
            IsAvailable = true
        };

        int width = 1024;
        int height = 1024;

        // Act
        var config = _optimizer.Compute(invalidDevice, width, height);

        // Assert - should return fallback (1, 1)
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().Be(1);
        config.WorkgroupSizeY.Should().Be(1);
        config.IsValidForDevice(invalidDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void BenchmarkAsync_WithTypicalDimensions_ReturnsConfiguration()
    {
        // Arrange
        int width = 1024;
        int height = 1024;

        // Act
        var config = _optimizer.BenchmarkAsync(_testDevice, width, height).Result;

        // Assert
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BePositive();
        config.WorkgroupSizeY.Should().BePositive();
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void BenchmarkAsync_WithLargeImage_ReturnsValidConfiguration()
    {
        // Arrange
        int width = 16384;
        int height = 16384;

        // Act
        var config = _optimizer.BenchmarkAsync(_testDevice, width, height).Result;

        // Assert
        config.Should().NotBeNull();
        config.WorkgroupSizeX.Should().BePositive();
        config.WorkgroupSizeY.Should().BePositive();
        config.IsValidForDevice(_testDevice.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void WorkgroupConfiguration_PropertiesAreValid()
    {
        // Arrange
        var config = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 1,
            GlobalWorkSizeX = 1024,
            GlobalWorkSizeY = 1024,
            GlobalWorkSizeZ = 1,
            LocalMemoryRequiredBytes = 1024,
            EstimatedOccupancy = 0.75,
            OptimizationScore = 85.5,
            Strategy = WorkgroupOptimizationStrategy.Balanced,
            DeviceId = Guid.NewGuid()
        };

        // Assert
        config.WorkgroupSizeX.Should().Be(16);
        config.WorkgroupSizeY.Should().Be(16);
        config.WorkgroupSizeZ.Should().Be(1);
        config.GlobalWorkSizeX.Should().Be(1024);
        config.GlobalWorkSizeY.Should().Be(1024);
        config.GlobalWorkSizeZ.Should().Be(1);
        config.LocalMemoryRequiredBytes.Should().Be(1024);
        config.EstimatedOccupancy.Should().Be(0.75);
        config.OptimizationScore.Should().Be(85.5);
        config.Strategy.Should().Be(WorkgroupOptimizationStrategy.Balanced);
        config.ComputedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WorkgroupConfiguration_GetTotalWorkgroupSize_ReturnsCorrectValue()
    {
        // Arrange
        var config1 = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 8,
            WorkgroupSizeY = 8,
            WorkgroupSizeZ = 1
        };

        var config2 = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 2
        };

        // Act & Assert
        config1.GetTotalWorkgroupSize().Should().Be(64);
        config2.GetTotalWorkgroupSize().Should().Be(512);
    }

    [Fact]
    public void WorkgroupConfiguration_GetTotalDispatchCount_ReturnsCorrectValue()
    {
        // Arrange
        var config = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 1,
            GlobalWorkSizeX = 100,
            GlobalWorkSizeY = 100,
            GlobalWorkSizeZ = 1
        };

        // Act & Assert
        long dispatchCount = config.GetTotalDispatchCount();
        // ceil(100/16) = 7, so 7*7*1 = 49
        dispatchCount.Should().Be(49);
    }

    [Fact]
    public void WorkgroupConfiguration_IsValidForDevice_WithSufficientMemory_ReturnsTrue()
    {
        // Arrange
        var device = new GpuDevice
        {
            LocalMemoryBytes = 1024 * 1024 // 1 MB
        };

        var config = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 1,
            GlobalWorkSizeX = 100,
            GlobalWorkSizeY = 100,
            GlobalWorkSizeZ = 1,
            LocalMemoryRequiredBytes = 1024 // 1 KB
        };

        // Act & Assert
        config.IsValidForDevice(device.LocalMemoryBytes).Should().BeTrue();
    }

    [Fact]
    public void WorkgroupConfiguration_IsValidForDevice_WithInsufficientMemory_ReturnsFalse()
    {
        // Arrange
        var device = new GpuDevice
        {
            LocalMemoryBytes = 1024 // 1 KB
        };

        var config = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 1,
            GlobalWorkSizeX = 100,
            GlobalWorkSizeY = 100,
            GlobalWorkSizeZ = 1,
            LocalMemoryRequiredBytes = 1024 * 1024 // 1 MB
        };

        // Act & Assert
        config.IsValidForDevice(device.LocalMemoryBytes).Should().BeFalse();
    }

    [Fact]
    public void WorkgroupConfiguration_ToString_ReturnsNonEmptyString()
    {
        // Arrange
        var config = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 1,
            GlobalWorkSizeX = 100,
            GlobalWorkSizeY = 100,
            GlobalWorkSizeZ = 1,
            LocalMemoryRequiredBytes = 1024,
            EstimatedOccupancy = 0.75,
            OptimizationScore = 85.5,
            Strategy = WorkgroupOptimizationStrategy.Balanced,
            DeviceId = Guid.NewGuid()
        };

        // Act
        string result = config.ToString();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("16×16");
        result.Should().Contain("Score=");
        result.Should().Contain("Occupancy=");
        result.Should().Contain("Balanced");
    }
}
