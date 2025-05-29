#nullable enable
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Services;

public class PerformanceMonitoringServiceTests
{
    private readonly Mock<ILogger<PerformanceMonitoringService>> _loggerMock;
    private readonly PerformanceMonitoringService _sut;

    public PerformanceMonitoringServiceTests()
    {
        _loggerMock = new Mock<ILogger<PerformanceMonitoringService>>();
        _sut = new PerformanceMonitoringService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new PerformanceMonitoringService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordOperation_SuccessfulOperation_RecordsMetrics()
    {
        // Act
        _sut.RecordOperation(100, true);
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.TotalOperationsCount.Should().Be(1);
        metrics.FailedOperationsCount.Should().Be(0);
        metrics.AverageExecutionTimeMs.Should().BeApproximately(100, precision: 0.01);
    }

    [Fact]
    public void RecordOperation_FailedOperation_IncrementsFailed()
    {
        // Act
        _sut.RecordOperation(50, false);
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.TotalOperationsCount.Should().Be(1);
        metrics.FailedOperationsCount.Should().Be(1);
    }

    [Fact]
    public void RecordOperation_MultipleOperations_CalculatesAverageCorrectly()
    {
        // Act
        _sut.RecordOperation(100, true);
        _sut.RecordOperation(200, true);
        _sut.RecordOperation(300, true);
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.TotalOperationsCount.Should().Be(3);
        metrics.AverageExecutionTimeMs.Should().BeApproximately(200, precision: 0.01);
    }

    [Fact]
    public void RecordOperation_SlowOperation_LogsWarning()
    {
        // Arrange
        var slowTimeMs = AppConstants.Performance.SlowOperationThresholdMs + 100;

        // Act
        _sut.RecordOperation(slowTimeMs, true);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecordOperation_TracksMinAndMaxTimes()
    {
        // Act
        _sut.RecordOperation(50, true);
        _sut.RecordOperation(150, true);
        _sut.RecordOperation(100, true);
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.MinExecutionTimeMs.Should().Be(50);
        metrics.MaxExecutionTimeMs.Should().Be(150);
    }

    [Fact]
    public void UpdateSystemMetrics_UpdatesCpuAndMemoryMetrics()
    {
        // Act
        _sut.UpdateSystemMetrics(cpuPercent: 75.5, memoryBytes: 2_000_000_000, gpuMemoryBytes: 1_000_000_000, gpuUtilization: 85.0);
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.CpuUsagePercent.Should().Be(75.5);
        metrics.MemoryUsedBytes.Should().Be(2_000_000_000);
        metrics.GpuMemoryUsedBytes.Should().Be(1_000_000_000);
        metrics.GpuUtilizationPercent.Should().Be(85.0);
    }

    [Fact]
    public void UpdateThroughput_UpdatesPixelsAndMegabytes()
    {
        // Act
        _sut.UpdateThroughput(pixelsPerSecond: 1_000_000, megabytesPerSecond: 500.5);
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.ImagePixelsProcessedPerSecond.Should().Be(1_000_000);
        metrics.ThroughputMegabytesPerSecond.Should().Be(500.5);
    }

    [Fact]
    public void GetCurrentMetrics_ReturnsIndependentCopy()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        var metrics1 = _sut.GetCurrentMetrics();

        // Act
        _sut.RecordOperation(200, true);
        var metrics2 = _sut.GetCurrentMetrics();

        // Assert
        metrics1.TotalOperationsCount.Should().Be(1);
        metrics2.TotalOperationsCount.Should().Be(2);
    }

    [Fact]
    public void SnapshotAndReset_CreatesSnapshotAndClearsMetrics()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        _sut.RecordOperation(200, false);

        // Act
        var snapshot = _sut.SnapshotAndReset();
        var afterReset = _sut.GetCurrentMetrics();

        // Assert
        snapshot.TotalOperationsCount.Should().Be(2);
        snapshot.FailedOperationsCount.Should().Be(1);

        afterReset.TotalOperationsCount.Should().Be(0);
        afterReset.FailedOperationsCount.Should().Be(0);
    }

    [Fact]
    public void GetMetricsHistory_ReturnsAllSnapshots()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        _sut.SnapshotAndReset();

        _sut.RecordOperation(200, true);
        _sut.SnapshotAndReset();

        // Act
        var history = _sut.GetMetricsHistory();

        // Assert
        history.Should().HaveCount(2);
    }

    [Fact]
    public void GetMetricsHistory_WithLimit_ReturnsOnlyRequestedCount()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            _sut.RecordOperation(100, true);
            _sut.SnapshotAndReset();
        }

        // Act
        var history = _sut.GetMetricsHistory(limit: 2);

        // Assert
        history.Should().HaveCount(2);
    }

    [Fact]
    public void GetMetricsHistory_ReturnsNewestFirst()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        _sut.SnapshotAndReset();

        _sut.RecordOperation(200, true);
        var snapshot2 = _sut.SnapshotAndReset();

        // Act
        var history = _sut.GetMetricsHistory();

        // Assert
        history.First().RecordedAt.Should().Be(snapshot2.RecordedAt);
    }

    [Fact]
    public void GetAverageMetrics_NoRecentMetrics_ReturnsEmptyDictionary()
    {
        // Act
        var averages = _sut.GetAverageMetrics(lastMinutes: 60);

        // Assert
        averages.Should().BeEmpty();
    }

    [Fact]
    public void GetAverageMetrics_ReturnsAverageOfRecentMetrics()
    {
        // Arrange
        _sut.UpdateSystemMetrics(50, 1_000_000_000, 500_000_000, 60.0);
        _sut.SnapshotAndReset();

        _sut.UpdateSystemMetrics(100, 2_000_000_000, 1_000_000_000, 80.0);
        _sut.SnapshotAndReset();

        // Act
        var averages = _sut.GetAverageMetrics(lastMinutes: 60);

        // Assert
        averages.Should().ContainKey("AverageCpuPercent");
        averages.Should().ContainKey("AverageMemoryBytes");
        averages.Should().ContainKey("AverageGpuMemoryBytes");
        averages.Should().ContainKey("AverageGpuUtilization");
    }

    [Fact]
    public void GetPerformanceReport_ContainsAllRequiredInformation()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        _sut.RecordOperation(150, false);
        _sut.UpdateSystemMetrics(75, 2_000_000_000, 1_000_000_000, 85.0);
        _sut.UpdateThroughput(1_000_000, 500);

        // Act
        var report = _sut.GetPerformanceReport();

        // Assert
        report.Should().Contain("Performance Report");
        report.Should().Contain("Operations: 2");
        report.Should().Contain("Failed: 1");
        report.Should().Contain("CPU Usage");
        report.Should().Contain("GPU Utilization");
        report.Should().Contain("Throughput");
    }

    [Fact]
    public void GetPerformanceReport_CalculatesSuccessRateCorrectly()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        _sut.RecordOperation(150, false);
        _sut.RecordOperation(200, true);

        // Act
        var report = _sut.GetPerformanceReport();

        // Assert
        // 2 successes / 3 total = 66.67%
        report.Should().Contain("Success Rate");
    }

    [Fact]
    public void ConcurrentRecordOperations_IsSafeFromRaceConditions()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => _sut.RecordOperation(10 + Random.Shared.Next(100), Random.Shared.Next(2) == 0)));
        }

        Task.WaitAll(tasks.ToArray());
        var metrics = _sut.GetCurrentMetrics();

        // Assert
        metrics.TotalOperationsCount.Should().Be(100);
    }
}
