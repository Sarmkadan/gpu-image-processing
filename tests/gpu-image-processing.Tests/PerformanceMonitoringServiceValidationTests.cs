#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests;

public class PerformanceMonitoringServiceValidationTests
{
    private static PerformanceMetrics CreateValidMetrics()
    {
        return new PerformanceMetrics
        {
            CpuUsagePercent = 42.5,
            MemoryUsedBytes = 1_024_000,
            GpuMemoryUsedBytes = 512_000,
            GpuUtilizationPercent = 33.3,
            AverageExecutionTimeMs = 12.5,
            MaxExecutionTimeMs = 20,
            MinExecutionTimeMs = 5,
            TotalOperationsCount = 100,
            FailedOperationsCount = 0,
            ThroughputMegabytesPerSecond = 150.0,
            ImagePixelsProcessedPerSecond = 2_000,
            ExecutionTimes = new List<double> { 5, 10, 15 },
            RecordedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var mock = new Mock<PerformanceMonitoringService>();
        var metrics = CreateValidMetrics();
        mock.Setup(s => s.GetCurrentMetrics()).Returns(metrics);
        mock.Setup(s => s.GetMetricsHistory(It.IsAny<int>())).Returns(new[] { metrics });

        // Act
        var problems = mock.Object.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var mock = new Mock<PerformanceMonitoringService>();
        var metrics = CreateValidMetrics();
        mock.Setup(s => s.GetCurrentMetrics()).Returns(metrics);
        mock.Setup(s => s.GetMetricsHistory(It.IsAny<int>())).Returns(new[] { metrics });

        // Act
        var result = mock.Object.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var mock = new Mock<PerformanceMonitoringService>();
        var metrics = CreateValidMetrics();
        mock.Setup(s => s.GetCurrentMetrics()).Returns(metrics);
        mock.Setup(s => s.GetMetricsHistory(It.IsAny<int>())).Returns(new[] { metrics });

        // Act & Assert
        var exception = Record.Exception(() => mock.Object.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PerformanceMonitoringServiceValidation.Validate(null!));
    }

    [Fact]
    public void EnsureValid_InvalidMetrics_ThrowsArgumentException()
    {
        // Arrange: create metrics with an invalid value (CPU usage out of range)
        var invalidMetrics = CreateValidMetrics();
        invalidMetrics.CpuUsagePercent = -5; // invalid

        var mock = new Mock<PerformanceMonitoringService>();
        mock.Setup(s => s.GetCurrentMetrics()).Returns(invalidMetrics);
        mock.Setup(s => s.GetMetricsHistory(It.IsAny<int>())).Returns(new[] { invalidMetrics });

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => mock.Object.EnsureValid());
        Assert.Contains("Invalid PerformanceMonitoringService instance", ex.Message);
    }

    [Fact]
    public void IsValid_InvalidMetrics_ReturnsFalse()
    {
        // Arrange: metrics with failed operations exceeding total operations
        var invalidMetrics = CreateValidMetrics();
        invalidMetrics.FailedOperationsCount = 150; // exceeds TotalOperationsCount

        var mock = new Mock<PerformanceMonitoringService>();
        mock.Setup(s => s.GetCurrentMetrics()).Returns(invalidMetrics);
        mock.Setup(s => s.GetMetricsHistory(It.IsAny<int>())).Returns(new[] { invalidMetrics });

        // Act
        var result = mock.Object.IsValid();

        // Assert
        Assert.False(result);
    }
}
