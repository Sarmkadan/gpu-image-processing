// SPDX-License-Identifier: MIT
// tests for GpuImageProcessing.Domain.PerformanceMetrics
// -----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Core; // for AppConstants
using Xunit;

namespace GpuImageProcessing.Tests.Domain;

public sealed class PerformanceMetricsTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Act
        var metrics = new PerformanceMetrics();

        // Assert
        Assert.NotEqual(Guid.Empty, metrics.Id);
        Assert.True((DateTime.UtcNow - metrics.RecordedAt).TotalSeconds < 5,
            "RecordedAt should be set to a recent UTC time");
        Assert.Equal(double.MaxValue, metrics.MinExecutionTimeMs);
        Assert.Empty(metrics.ExecutionTimes);
        Assert.Equal(0, metrics.TotalOperationsCount);
        Assert.Equal(0, metrics.FailedOperationsCount);
        Assert.Equal(0.0, metrics.AverageExecutionTimeMs);
        Assert.Equal(0.0, metrics.MaxExecutionTimeMs);
        Assert.Equal(0.0, metrics.MinExecutionTimeMs);
    }

    [Fact]
    public void RecordExecution_UpdatesStatisticsCorrectly()
    {
        // Arrange
        var metrics = new PerformanceMetrics();
        var times = new[] { 10.0, 20.0, 5.0 };

        // Act
        foreach (var t in times)
            metrics.RecordExecution(t);

        // Assert
        Assert.Equal(times.Length, metrics.TotalOperationsCount);
        Assert.Equal(times.Average(), metrics.AverageExecutionTimeMs);
        Assert.Equal(times.Max(), metrics.MaxExecutionTimeMs);
        Assert.Equal(times.Min(), metrics.MinExecutionTimeMs);
        Assert.Equal(times, metrics.ExecutionTimes);
    }

    [Fact]
    public void GetSuccessRate_ReturnsZeroWhenNoOperations()
    {
        var metrics = new PerformanceMetrics();
        Assert.Equal(0.0, metrics.GetSuccessRate());
    }

    [Fact]
    public void GetSuccessRate_ComputesCorrectPercentage()
    {
        // Arrange
        var metrics = new PerformanceMetrics();
        // 5 total, 2 failed
        for (int i = 0; i < 5; i++)
            metrics.RecordExecution(1.0);
        metrics.FailedOperationsCount = 2;

        // Act
        var rate = metrics.GetSuccessRate();

        // Assert
        Assert.Equal(((5 - 2) / 5.0) * 100.0, rate);
    }

    [Fact]
    public void IsMemoryWarningRequired_RespectsThreshold()
    {
        // Arrange
        var metrics = new PerformanceMetrics
        {
            GpuMemoryUsedBytes = AppConstants.Memory.MemoryWarningThreshold - 1
        };
        Assert.False(metrics.IsMemoryWarningRequired());

        metrics.GpuMemoryUsedBytes = AppConstants.Memory.MemoryWarningThreshold;
        Assert.True(metrics.IsMemoryWarningRequired());
    }

    [Fact]
    public void GetMemoryUsagePercent_ReturnsCorrectPercentage()
    {
        // Arrange
        var metrics = new PerformanceMetrics
        {
            GpuMemoryUsedBytes = AppConstants.Memory.MaxTotalGpuMemory / 2
        };

        // Act
        var percent = metrics.GetMemoryUsagePercent();

        // Assert
        Assert.Equal(50.0, percent);
    }

    [Fact]
    public void Reset_ClearsAllMutableState()
    {
        // Arrange
        var metrics = new PerformanceMetrics();
        metrics.CpuUsagePercent = 42.0;
        metrics.GpuUtilizationPercent = 73.5;
        metrics.RecordExecution(12.3);
        metrics.FailedOperationsCount = 1;
        var oldRecordedAt = metrics.RecordedAt;

        // Act
        metrics.Reset();

        // Assert
        Assert.Empty(metrics.ExecutionTimes);
        Assert.Equal(0, metrics.TotalOperationsCount);
        Assert.Equal(0, metrics.FailedOperationsCount);
        Assert.Equal(0.0, metrics.AverageExecutionTimeMs);
        Assert.Equal(0.0, metrics.MaxExecutionTimeMs);
        Assert.Equal(double.MaxValue, metrics.MinExecutionTimeMs);
        Assert.NotEqual(oldRecordedAt, metrics.RecordedAt);
        // Ensure mutable fields are reset to defaults (they are public settable)
        Assert.Equal(0.0, metrics.CpuUsagePercent);
        Assert.Equal(0.0, metrics.GpuUtilizationPercent);
    }

    [Fact]
    public void ToString_ContainsKeyMetrics()
    {
        // Arrange
        var metrics = new PerformanceMetrics
        {
            CpuUsagePercent = 12.34,
            GpuUtilizationPercent = 56.78,
            ThroughputMegabytesPerSecond = 123.45
        };
        metrics.RecordExecution(10);
        metrics.RecordExecution(20);

        // Act
        var result = metrics.ToString();

        // Assert
        Assert.Contains("CPU: 12.34%", result);
        Assert.Contains("GPU: 56.78%", result);
        Assert.Contains("Avg Time:", result);
        Assert.Contains("Throughput: 123.45 MB/s", result);
        Assert.Contains("Success Rate:", result);
    }
}
