using System;
using System.Text.Json;
using GpuImageProcessing.Benchmarking;
using Xunit;

namespace GpuImageProcessing.Tests
{
    public class BenchmarkSuiteConfigurationJsonExtensionsTests
    {
        private BenchmarkSuiteConfiguration CreateValidConfiguration()
        {
            return new BenchmarkSuiteConfiguration
            {
                RunName = "TestRun",
                AccuracyLevel = BenchmarkAccuracyLevel.Standard,
                IncludeFilterChainBenchmarks = true
            };
        }

        [Fact]
        public void ToJson_ValidConfig_ReturnsJsonString()
        {
            // Arrange
            var config = CreateValidConfiguration();

            // Act
            var json = config.ToJson();

            // Assert
            Assert.Contains("\"runName\":\"TestRun\"", json);
            Assert.Contains("\"accuracyLevel\":\"standard\"", json);
        }

        [Fact]
        public void ToJson_NullConfig_ThrowsArgumentNullException()
        {
            BenchmarkSuiteConfiguration? config = null;
            Assert.Throws<ArgumentNullException>(() => config!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsConfig()
        {
            // Arrange
            var json = "{\"runName\":\"TestRun\",\"accuracyLevel\":\"standard\",\"includeFilterChainBenchmarks\":true}";

            // Act
            var config = BenchmarkSuiteConfigurationJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(config);
            Assert.Equal("TestRun", config!.RunName);
            Assert.Equal(BenchmarkAccuracyLevel.Standard, config.AccuracyLevel);
            Assert.True(config.IncludeFilterChainBenchmarks);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            var json = "invalid json";
            Assert.Throws<JsonException>(() => BenchmarkSuiteConfigurationJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_NullOrEmptyJson_ReturnsNull()
        {
            Assert.Null(BenchmarkSuiteConfigurationJsonExtensions.FromJson(""));
            Assert.Null(BenchmarkSuiteConfigurationJsonExtensions.FromJson("   "));
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BenchmarkSuiteConfigurationJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndConfig()
        {
            // Arrange
            var json = "{\"runName\":\"TestRun\",\"accuracyLevel\":\"standard\",\"includeFilterChainBenchmarks\":true}";

            // Act
            var success = BenchmarkSuiteConfigurationJsonExtensions.TryFromJson(json, out var config);

            // Assert
            Assert.True(success);
            Assert.NotNull(config);
            Assert.Equal("TestRun", config!.RunName);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            var json = "invalid json";
            var success = BenchmarkSuiteConfigurationJsonExtensions.TryFromJson(json, out var config);
            Assert.False(success);
            Assert.Null(config);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            var success = BenchmarkSuiteConfigurationJsonExtensions.TryFromJson("", out var config);
            Assert.False(success);
            Assert.Null(config);
        }
    }
}
