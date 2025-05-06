#nullable enable
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Services;

public class GpuManagementServiceTests
{
    private readonly Mock<ILogger<GpuManagementService>> _loggerMock;
    private readonly GpuManagementService _sut;

    public GpuManagementServiceTests()
    {
        _loggerMock = new Mock<ILogger<GpuManagementService>>();
        _sut = new GpuManagementService(_loggerMock.Object);
    }

    private static GpuDevice CreateTestDevice(
        string name = "Test GPU",
        long globalMemory = 4L * 1024 * 1024 * 1024,
        int computeUnits = 64)
    {
        return new GpuDevice
        {
            Id = Guid.NewGuid(),
            Name = name,
            GlobalMemoryBytes = globalMemory,
            MaxAllocatableMemoryBytes = globalMemory,
            LocalMemoryBytes = 96 * 1024,
            MaxComputeUnits = computeUnits,
            MaxWorkGroupSize = 256,
            DeviceType = GpuDeviceType.Gpu,
            Vendor = "Test Vendor",
            Version = "1.0",
            Driver = "Test Driver",
            IsAvailable = true
        };
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new GpuManagementService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAvailableDevices_AlwaysContainsAtLeastOneDevice()
    {
        // Act
        var devices = _sut.GetAvailableDevices();

        // Assert
        devices.Should().NotBeEmpty();
    }

    [Fact]
    public void GetBestDevice_ReturnsDeviceWithHighestPerformanceScore()
    {
        // Act
        var bestDevice = _sut.GetBestDevice();

        // Assert
        bestDevice.Should().NotBeNull();
        bestDevice!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void GetDeviceWithMostMemory_ReturnsDeviceWithLargestMemory()
    {
        // Act
        var device = _sut.GetDeviceWithMostMemory();

        // Assert
        device.Should().NotBeNull();
        device!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void AllocateMemory_ZeroBytes_ReturnsFalse()
    {
        // Arrange
        var device = _sut.GetBestDevice();

        // Act
        var allocated = _sut.AllocateMemory(0, device!.Id);

        // Assert
        allocated.Should().BeFalse();
    }

    [Fact]
    public void AllocateMemory_NegativeBytes_ReturnsFalse()
    {
        // Arrange
        var device = _sut.GetBestDevice();

        // Act
        var allocated = _sut.AllocateMemory(-100, device!.Id);

        // Assert
        allocated.Should().BeFalse();
    }

    [Fact]
    public void AllocateMemory_InvalidDevice_ThrowsGpuException()
    {
        // Arrange
        var invalidDeviceId = Guid.NewGuid();
        var bytes = 1024L;

        // Act & Assert
        _sut.Invoking(x => x.AllocateMemory(bytes, invalidDeviceId))
            .Should().Throw<GpuException>();
    }

    [Fact]
    public void AllocateMemory_SufficientMemory_ReturnsTrue()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var availableMemory = device!.GetAvailableMemory();
        var allocateBytes = availableMemory / 2; // Half of available

        // Act
        var allocated = _sut.AllocateMemory(allocateBytes, device.Id);

        // Assert
        allocated.Should().BeTrue();
    }

    [Fact]
    public void AllocateMemory_InsufficientMemory_ThrowsGpuException()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var tooMuchMemory = device!.GetAvailableMemory() * 2;

        // Act & Assert
        _sut.Invoking(x => x.AllocateMemory(tooMuchMemory, device.Id))
            .Should().Throw<GpuException>();
    }

    [Fact]
    public void AllocateMemory_DecreasesAvailableMemory()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var initialAvailable = device!.GetAvailableMemory();
        var allocateBytes = 100 * 1024 * 1024; // 100 MB

        // Act
        _sut.AllocateMemory(allocateBytes, device.Id);
        var afterAllocate = device.GetAvailableMemory();

        // Assert
        afterAllocate.Should().BeLessThan(initialAvailable);
    }

    [Fact]
    public void DeallocateMemory_ZeroBytes_DoesNothing()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var initialTotal = _sut.GetTotalAllocatedMemory();

        // Act
        _sut.DeallocateMemory(0, device!.Id);

        // Assert
        _sut.GetTotalAllocatedMemory().Should().Be(initialTotal);
    }

    [Fact]
    public void DeallocateMemory_NegativeBytes_DoesNothing()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var initialTotal = _sut.GetTotalAllocatedMemory();

        // Act
        _sut.DeallocateMemory(-100, device!.Id);

        // Assert
        _sut.GetTotalAllocatedMemory().Should().Be(initialTotal);
    }

    [Fact]
    public void DeallocateMemory_IncreaseAvailableMemory()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var allocateBytes = 100 * 1024 * 1024;
        _sut.AllocateMemory(allocateBytes, device!.Id);
        var afterAllocate = device.GetAvailableMemory();

        // Act
        _sut.DeallocateMemory(allocateBytes, device.Id);
        var afterDeallocate = device.GetAvailableMemory();

        // Assert
        afterDeallocate.Should().BeGreaterThan(afterAllocate);
    }

    [Fact]
    public void GetTotalAllocatedMemory_ReturnsAccumulatedAllocation()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var bytes1 = 50 * 1024 * 1024;
        var bytes2 = 100 * 1024 * 1024;

        // Act
        _sut.AllocateMemory(bytes1, device!.Id);
        _sut.AllocateMemory(bytes2, device.Id);
        var total = _sut.GetTotalAllocatedMemory();

        // Assert
        total.Should().Be(bytes1 + bytes2);
    }

    [Fact]
    public void ValidateDevice_ValidDeviceWithSufficientMemory_ReturnsTrue()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var requiredMemory = device!.GetAvailableMemory() / 2;

        // Act
        var valid = _sut.ValidateDevice(device.Id, requiredMemory, 1);

        // Assert
        valid.Should().BeTrue();
    }

    [Fact]
    public void ValidateDevice_InsufficientMemory_ReturnsFalse()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var tooMuchMemory = device!.GetAvailableMemory() * 2;

        // Act
        var valid = _sut.ValidateDevice(device.Id, tooMuchMemory, 1);

        // Assert
        valid.Should().BeFalse();
    }

    [Fact]
    public void ValidateDevice_InsufficientComputeUnits_ReturnsFalse()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var excessiveComputeUnits = device!.MaxComputeUnits + 100;

        // Act
        var valid = _sut.ValidateDevice(device.Id, 1024 * 1024, excessiveComputeUnits);

        // Assert
        valid.Should().BeFalse();
    }

    [Fact]
    public void ValidateDevice_InvalidDeviceId_ReturnsFalse()
    {
        // Act
        var valid = _sut.ValidateDevice(Guid.NewGuid(), 1024 * 1024, 1);

        // Assert
        valid.Should().BeFalse();
    }

    [Fact]
    public void GetMemoryStatistics_ReturnsAllMetrics()
    {
        // Act
        var stats = _sut.GetMemoryStatistics();

        // Assert
        stats.Should().ContainKey("TotalGpuMemory");
        stats.Should().ContainKey("TotalAvailableMemory");
        stats.Should().ContainKey("TotalAllocatedMemory");
        stats.Should().ContainKey("MemoryUsagePercent");
        stats.Should().ContainKey("DeviceCount");
        stats.Should().ContainKey("AvailableDeviceCount");
    }

    [Fact]
    public void GetMemoryStatistics_AfterAllocation_ShowsCorrectUsage()
    {
        // Arrange
        var device = _sut.GetBestDevice();
        var allocateBytes = 100 * 1024 * 1024;
        _sut.AllocateMemory(allocateBytes, device!.Id);

        // Act
        var stats = _sut.GetMemoryStatistics();

        // Assert
        ((long)stats["TotalAllocatedMemory"]).Should().BeGreaterThan(0);
        ((double)stats["MemoryUsagePercent"]).Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetDeviceById_ValidId_ReturnsDevice()
    {
        // Arrange
        var device = _sut.GetBestDevice();

        // Act
        var retrieved = _sut.GetDeviceById(device!.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(device.Id);
    }

    [Fact]
    public void GetDeviceById_InvalidId_ReturnsNull()
    {
        // Act
        var retrieved = _sut.GetDeviceById(Guid.NewGuid());

        // Assert
        retrieved.Should().BeNull();
    }
}
