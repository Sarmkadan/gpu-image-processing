using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.Tests;

public class PerformanceMonitoringServiceJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitoringService>>();
        var service = new PerformanceMonitoringService(loggerMock.Object);

        // Act
        var json = PerformanceMonitoringServiceJsonExtensions.ToJson(service);

        // Assert
        Assert.NotEmpty(json);
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsServiceInstance()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitoringService>>();
        var service = new PerformanceMonitoringService(loggerMock.Object);
        var json = PerformanceMonitoringServiceJsonExtensions.ToJson(service);

        // Act
        var deserializedService = PerformanceMonitoringServiceJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedService);
    }

    [Fact]
    public void FromJson_NullInput_ReturnsNull()
    {
        // Act
        var deserializedService = PerformanceMonitoringServiceJsonExtensions.FromJson(null);

        // Assert
        Assert.Null(deserializedService);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndServiceInstance()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitoringService>>();
        var service = new PerformanceMonitoringService(loggerMock.Object);
        var json = PerformanceMonitoringServiceJsonExtensions.ToJson(service);

        // Act
        var success = PerformanceMonitoringServiceJsonExtensions.TryFromJson(json, out var deserializedService);

        // Assert
        Assert.True(success);
        Assert.NotNull(deserializedService);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalseAndNull()
    {
        // Act
        var success = PerformanceMonitoringServiceJsonExtensions.TryFromJson(null, out var deserializedService);

        // Assert
        Assert.False(success);
        Assert.Null(deserializedService);
    }

    [Fact]
    public void ToJson_InvalidServiceInstance_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => PerformanceMonitoringServiceJsonExtensions.ToJson(null));
    }
}
