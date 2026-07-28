using System;
using System.Collections.Generic;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests.Domain;

public class PerformanceMetricsValidationTests
{
    private static PerformanceMetrics CreateValidMetrics()
    {
        return new PerformanceMetrics
        {
            Id = Guid.NewGuid(),
            RecordedAt = DateTime.UtcNow,
            CpuUsagePercent = 42.0,
            MemoryUsedBytes = 1024 * 1024,
            GpuMemoryUsedBytes = 512 * 1024,
            GpuUtilizationPercent = 55.0,
            AverageExecutionTimeMs = 10.0,
            MaxExecutionTimeMs = 20.0,
            MinExecutionTimeMs = 5.0,
            ImagePixelsProcessedPerSecond = 1_000_000,
            TotalOperationsCount = 100,
            FailedOperationsCount = 0,
            ThroughputMegabytesPerSecond = 12.5,
            ExecutionTimes = new List<double> { 5.0, 10.0, 15.0 }
        };
    }

    [Fact]
    public void Validate_ReturnsEmpty_WhenMetricsAreValid()
    {
        // Arrange
        var metrics = CreateValidMetrics();

        // Act
        var errors = PerformanceMetricsValidation.Validate(metrics);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenMetricsAreValid()
    {
        // Arrange
        var metrics = CreateValidMetrics();

        // Act
        var result = PerformanceMetricsValidation.IsValid(metrics);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenMetricsAreValid()
    {
        // Arrange
        var metrics = CreateValidMetrics();

        // Act & Assert
        var exception = Record.Exception(() => PerformanceMetricsValidation.EnsureValid(metrics));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenMetricsAreNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PerformanceMetricsValidation.Validate(null!));
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenMetricsAreNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PerformanceMetricsValidation.EnsureValid(null!));
    }

    [Fact]
    public void Validate_ReturnsError_WhenIdIsEmpty()
    {
        // Arrange
        var metrics = CreateValidMetrics();
        metrics.Id = Guid.Empty;

        // Act
        var errors = PerformanceMetricsValidation.Validate(metrics);

        // Assert
        Assert.Contains("Id must be a non-empty GUID.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenFailedOperationsExceedTotal()
    {
        // Arrange
        var metrics = CreateValidMetrics();
        metrics.TotalOperationsCount = 10;
        metrics.FailedOperationsCount = 20;

        // Act
        var errors = PerformanceMetricsValidation.Validate(metrics);

        // Assert
        Assert.Contains("FailedOperationsCount cannot exceed TotalOperationsCount.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_WhenExecutionTimesIsNull()
    {
        // Arrange
        var metrics = CreateValidMetrics();
        metrics.ExecutionTimes = null!;

        // Act
        var errors = PerformanceMetricsValidation.Validate(metrics);

        // Assert
        Assert.Contains("ExecutionTimes list cannot be null.", errors);
    }

    [Fact]
    public void Validate_ReturnsError_ForBoundaryValues()
    {
        // Arrange
        var metrics = CreateValidMetrics();
        metrics.CpuUsagePercent = -0.1; // below min
        metrics.GpuUtilizationPercent = 101.0; // above max
        metrics.MemoryUsedBytes = -1; // negative
        metrics.AverageExecutionTimeMs = -0.5; // negative

        // Act
        var errors = PerformanceMetricsValidation.Validate(metrics);

        // Assert
        Assert.Contains("CpuUsagePercent must be between 0 and 100 (inclusive).", errors);
        Assert.Contains("GpuUtilizationPercent must be between 0 and 100 (inclusive).", errors);
        Assert.Contains("MemoryUsedBytes must be non-negative (>= 0).", errors);
        Assert.Contains("AverageExecutionTimeMs must be non-negative (>= 0).", errors);
    }
}
