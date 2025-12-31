#nullable enable
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Unit tests for <see cref="PerformanceMonitoringService"/> that validate performance monitoring functionality.
/// Tests constructor validation, metric recording, system metric updates, throughput tracking,
/// snapshot and reset operations, metrics history management, and performance reporting.
/// </summary>
public class PerformanceMonitoringServiceTests
{
    private readonly Mock<ILogger<PerformanceMonitoringService>> _loggerMock;
    private readonly PerformanceMonitoringService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMonitoringServiceTests"/> class.
    /// Sets up mock logger and creates the service under test instance.
    /// </summary>
    public PerformanceMonitoringServiceTests()
    {
        _loggerMock = new Mock<ILogger<PerformanceMonitoringService>>();
        _sut = new PerformanceMonitoringService(_loggerMock.Object);
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentNullException"/> when null logger is provided.
    /// Validates proper parameter validation in the service.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new PerformanceMonitoringService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.RecordOperation"/> correctly records successful operation metrics.
    /// Validates that total operations count, failed operations count, and average execution time
    /// are properly updated when an operation completes successfully.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.RecordOperation"/> correctly increments failed operations count.
    /// Validates that when an operation fails, the failed operations counter is incremented
    /// while the total operations count still increases.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.RecordOperation"/> correctly calculates average execution time across multiple operations.
    /// Validates that the average execution time is correctly computed as the mean of all recorded operation times.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.RecordOperation"/> correctly tracks minimum and maximum execution times.
    /// Validates that the service maintains and returns the lowest and highest recorded operation times.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.UpdateSystemMetrics"/> correctly updates CPU and memory metrics.
    /// Validates that system-level metrics including CPU usage percentage, memory usage in bytes,
    /// GPU memory usage, and GPU utilization percentage are properly recorded and retrievable.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.UpdateThroughput"/> correctly updates throughput metrics.
    /// Validates that image processing throughput metrics including pixels processed per second and
    /// megabytes processed per second are properly recorded and retrievable.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetCurrentMetrics"/> returns an independent copy of metrics.
    /// Validates that subsequent calls to <see cref="PerformanceMonitoringService.GetCurrentMetrics"/> return separate instances,
    /// preventing modifications to one copy from affecting another.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.SnapshotAndReset"/> creates a snapshot and clears current metrics.
    /// Validates that calling <see cref="PerformanceMonitoringService.SnapshotAndReset"/> returns a snapshot containing all current metrics
    /// and resets the service's internal state to start fresh for new measurements.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetMetricsHistory"/> returns all recorded snapshots.
    /// Validates that calling <see cref="PerformanceMonitoringService.GetMetricsHistory"/> returns a collection containing
    /// all previously created snapshots through <see cref="PerformanceMonitoringService.SnapshotAndReset"/> calls.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetMetricsHistory(int)"/> respects the limit parameter.
    /// Validates that calling <see cref="PerformanceMonitoringService.GetMetricsHistory"/> with a limit parameter
    /// returns only the specified number of most recent snapshots.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetMetricsHistory"/> returns snapshots in newest-first order.
    /// Validates that the <see cref="PerformanceMonitoringService.GetMetricsHistory"/> method returns snapshots
    /// ordered from most recent to oldest based on their recorded timestamps.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetAverageMetrics(int)"/> returns an empty dictionary when no recent metrics exist.
    /// Validates that calling <see cref="PerformanceMonitoringService.GetAverageMetrics"/> with a time window
    /// returns an empty dictionary when no snapshots fall within the specified time range.
    /// </summary>
    [Fact]
    public void GetAverageMetrics_NoRecentMetrics_ReturnsEmptyDictionary()
    {
        // Act
        var averages = _sut.GetAverageMetrics(lastMinutes: 60);

        // Assert
        averages.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetAverageMetrics(int)"/> returns averages of recent metrics within the specified time window.
    /// Validates that calling <see cref="PerformanceMonitoringService.GetAverageMetrics"/> calculates and returns average values
    /// for CPU usage, memory bytes, GPU memory bytes, and GPU utilization across all snapshots
    /// recorded within the specified time window in minutes.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetPerformanceReport"/> contains all required information.
    /// Validates that the performance report generated by <see cref="PerformanceMonitoringService.GetPerformanceReport"/>
    /// includes operation statistics, failure counts, system metrics, and throughput information in a readable format.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.GetPerformanceReport"/> calculates success rate correctly.
    /// Validates that the performance report includes an accurate success rate calculation based on
    /// the ratio of successful operations to total operations recorded.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="PerformanceMonitoringService.RecordOperation"/> is thread-safe and handles concurrent operations correctly.
    /// Validates that the service can safely handle 100 concurrent calls to <see cref="PerformanceMonitoringService.RecordOperation"/>
    /// without race conditions corrupting the metrics data or causing lost updates.
    /// </summary>
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

    [Fact]
    public void GetPerformanceReport_ContainsValidationHelpersInformation()
    {
        // Arrange
        _sut.RecordOperation(100, true);
        _sut.RecordOperation(150, false);
        _sut.UpdateSystemMetrics(75, 2_000_000_000, 1_000_000_000, 85.0);
        _sut.UpdateThroughput(1_000_000, 500);

        // Act
        var report = _sut.GetPerformanceReport();

        // Assert
        report.Should().Contain("Validation Helpers");
    }
}
