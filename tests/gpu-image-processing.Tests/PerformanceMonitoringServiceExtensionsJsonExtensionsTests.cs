#nullable enable

using System;
using System.Text.Json;
using Xunit;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace gpu_image_processing.Tests
{
    public class PerformanceMonitoringServiceExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ProducesCorrectJson()
        {
            var service = new PerformanceMonitoringService(NullLogger<PerformanceMonitoringService>.Instance);
            service.RecordOperation(100.5, true);
            service.UpdateSystemMetrics(50.0, 1024L * 1024 * 100, 512L * 1024 * 100, 75.0);
            service.UpdateThroughput(1000L, 2.5);

            var json = PerformanceMonitoringServiceExtensionsJsonExtensions.ToJson(service);
            Assert.NotNull(json);
            Assert.Contains("\"cpuUsagePercent\":50.0", json);
            Assert.Contains("\"memoryUsedBytes\":104857600", json);
            Assert.Contains("\"gpuMemoryUsedBytes\":536870912", json);
            Assert.Contains("\"gpuUtilizationPercent\":75.0", json);
            Assert.Contains("\"averageExecutionTimeMs\":100.5", json);
            Assert.Contains("\"totalOperationsCount\":1", json);
            Assert.Contains("\"failedOperationsCount\":0", json);
            Assert.Contains("\"throughputMegabytesPerSecond\":2.5", json);
            Assert.Contains("\"imagePixelsProcessedPerSecond\":1000", json);
        }

        [Fact]
        public void ToJson_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PerformanceMonitoringServiceExtensionsJsonExtensions.ToJson(null!));
        }

        [Fact]
        public void ToJson_Indented_ProducesFormattedJson()
        {
            var service = new PerformanceMonitoringService(NullLogger<PerformanceMonitoringService>.Instance);
            service.RecordOperation(50.0, true);

            var json = PerformanceMonitoringServiceExtensionsJsonExtensions.ToJson(service, indented: true);
            Assert.NotNull(json);
            Assert.Contains(Environment.NewLine, json); // formatted JSON contains line breaks
            Assert.Contains("\"cpuUsagePercent\":0.0", json);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsService()
        {
            var originalService = new PerformanceMonitoringService(NullLogger<PerformanceMonitoringService>.Instance);
            originalService.RecordOperation(200.0, false);
            originalService.UpdateSystemMetrics(30.0, 2048L * 1024 * 1024, 1024L * 1024 * 512, 60.0);
            originalService.UpdateThroughput(500L, 1.5);

            var json = PerformanceMonitoringServiceExtensionsJsonExtensions.ToJson(originalService);
            var deserialized = PerformanceMonitoringServiceExtensionsJsonExtensions.FromJson(json);

            Assert.NotNull(deserialized);
            Assert.Equal(30.0, deserialized.GetCurrentMetrics().CpuUsagePercent);
            Assert.Equal(2048L * 1024 * 1024, deserialized.GetCurrentMetrics().MemoryUsedBytes);
            Assert.Equal(1024L * 1024 * 512, deserialized.GetCurrentMetrics().GpuMemoryUsedBytes);
            Assert.Equal(60.0, deserialized.GetCurrentMetrics().GpuUtilizationPercent);
            Assert.Equal(200.0, deserialized.GetCurrentMetrics().AverageExecutionTimeMs);
            Assert.Equal(1, deserialized.GetCurrentMetrics().TotalOperationsCount);
            Assert.Equal(1, deserialized.GetCurrentMetrics().FailedOperationsCount);
            Assert.Equal(1.5, deserialized.GetCurrentMetrics().ThroughputMegabytesPerSecond);
            Assert.Equal(500L, deserialized.GetCurrentMetrics().ImagePixelsProcessedPerSecond);
        }

        [Fact]
        public void FromJson_NullOrEmpty_ReturnsNull()
        {
            Assert.Null(PerformanceMonitoringServiceExtensionsJsonExtensions.FromJson(null));
            Assert.Null(PerformanceMonitoringServiceExtensionsJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndService()
        {
            var service = new PerformanceMonitoringService(NullLogger<PerformanceMonitoringService>.Instance);
            service.RecordOperation(150.0, true);
            service.UpdateSystemMetrics(40.0, 4096L * 1024 * 1024, 2048L * 1024 * 1024, 80.0);
            service.UpdateThroughput(2000L, 5.0);

            var json = PerformanceMonitoringServiceExtensionsJsonExtensions.ToJson(service);
            var success = PerformanceMonitoringServiceExtensionsJsonExtensions.TryFromJson(json, out var result);

            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(40.0, result.GetCurrentMetrics().CpuUsagePercent);
            Assert.Equal(4096L * 1024 * 1024, result.GetCurrentMetrics().MemoryUsedBytes);
            Assert.Equal(2048L * 1024 * 1024, result.GetCurrentMetrics().GpuMemoryUsedBytes);
            Assert.Equal(80.0, result.GetCurrentMetrics().GpuUtilizationPercent);
            Assert.Equal(150.0, result.GetCurrentMetrics().AverageExecutionTimeMs);
            Assert.Equal(1, result.GetCurrentMetrics().TotalOperationsCount);
            Assert.Equal(0, result.GetCurrentMetrics().FailedOperationsCount);
            Assert.Equal(5.0, result.GetCurrentMetrics().ThroughputMegabytesPerSecond);
            Assert.Equal(2000L, result.GetCurrentMetrics().ImagePixelsProcessedPerSecond);
        }

        [Fact]
        public void TryFromJson_NullOrEmpty_ReturnsFalseWithNullResult()
        {
            var successNull = PerformanceMonitoringServiceExtensionsJsonExtensions.TryFromJson(null, out var resultNull);
            var successEmpty = PerformanceMonitoringServiceExtensionsJsonExtensions.TryFromJson(string.Empty, out var resultEmpty);

            Assert.False(successNull);
            Assert.Null(resultNull);
            Assert.False(successEmpty);
            Assert.Null(resultEmpty);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            var invalidJson = "{ invalid json {{{";
            var success = PerformanceMonitoringServiceExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

            Assert.False(success);
            Assert.Null(result);
        }
    }
}