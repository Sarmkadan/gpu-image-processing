using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Configuration;
using Xunit;

namespace GpuImageProcessing.Tests
{
    public class ComputeShaderPipelineOptionsExtensionsTests
    {
        private ComputeShaderPipelineOptions CreateSampleOptions()
        {
            return new ComputeShaderPipelineOptions
            {
                MaxWorkgroupDimension = 16,
                BenchmarkGuidedOptimization = true,
                EnableProfiling = true,
                MaxPipelineDepth = 5,
                DefaultLocalMemoryPerThreadBytes = 256,
                OccupancyWarningThreshold = 0.2
            };
        }

        [Fact]
        public void Clone_WithValidOptions_ReturnsDeepCopy()
        {
            // Arrange
            var original = CreateSampleOptions();

            // Act
            var clone = original.Clone();

            // Assert
            Assert.NotSame(original, clone);
            Assert.Equal(original.MaxWorkgroupDimension, clone.MaxWorkgroupDimension);
            Assert.Equal(original.BenchmarkGuidedOptimization, clone.BenchmarkGuidedOptimization);
            Assert.Equal(original.EnableProfiling, clone.EnableProfiling);
            Assert.Equal(original.MaxPipelineDepth, clone.MaxPipelineDepth);
            Assert.Equal(original.DefaultLocalMemoryPerThreadBytes, clone.DefaultLocalMemoryPerThreadBytes);
            Assert.Equal(original.OccupancyWarningThreshold, clone.OccupancyWarningThreshold);
        }

        [Fact]
        public void Clone_NullOptions_ThrowsArgumentNullException()
        {
            ComputeShaderPipelineOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options!.Clone());
        }

        [Fact]
        public void WithDevelopmentSettings_SetsExpectedValues()
        {
            var options = CreateSampleOptions();

            var result = options.WithDevelopmentSettings(enableBenchmarking: false);

            Assert.Same(options, result);
            Assert.False(result.BenchmarkGuidedOptimization);
            Assert.True(result.EnableProfiling);
            Assert.Equal(0.15, result.OccupancyWarningThreshold);
        }

        [Fact]
        public void WithDevelopmentSettings_NullOptions_ThrowsArgumentNullException()
        {
            ComputeShaderPipelineOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options!.WithDevelopmentSettings());
        }

        [Fact]
        public void WithProductionSettings_ValidDimension_SetsExpectedValues()
        {
            var options = CreateSampleOptions();

            var result = options.WithProductionSettings(maxWorkgroupDimension: 64);

            Assert.Same(options, result);
            Assert.Equal(64, result.MaxWorkgroupDimension);
            Assert.False(result.BenchmarkGuidedOptimization);
            Assert.False(result.EnableProfiling);
            Assert.Equal(0.5, result.OccupancyWarningThreshold);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1025)]
        public void WithProductionSettings_InvalidDimension_ThrowsArgumentOutOfRangeException(int invalidDimension)
        {
            var options = CreateSampleOptions();
            Assert.Throws<ArgumentOutOfRangeException>(() => options.WithProductionSettings(invalidDimension));
        }

        [Fact]
        public void WithProductionSettings_NullOptions_ThrowsArgumentNullException()
        {
            ComputeShaderPipelineOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options!.WithProductionSettings());
        }

        [Fact]
        public void GetClampedLocalMemoryPerThreadBytes_ClampsTo512()
        {
            var options = CreateSampleOptions();
            options.DefaultLocalMemoryPerThreadBytes = 1024;

            int clamped = options.GetClampedLocalMemoryPerThreadBytes();

            Assert.Equal(512, clamped);
        }

        [Fact]
        public void GetClampedLocalMemoryPerThreadBytes_ValueBelowCap_ReturnsOriginal()
        {
            var options = CreateSampleOptions();
            options.DefaultLocalMemoryPerThreadBytes = 256;

            int clamped = options.GetClampedLocalMemoryPerThreadBytes();

            Assert.Equal(256, clamped);
        }

        [Fact]
        public void GetClampedLocalMemoryPerThreadBytes_NullOptions_ThrowsArgumentNullException()
        {
            ComputeShaderPipelineOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options!.GetClampedLocalMemoryPerThreadBytes());
        }

        [Fact]
        public void ToDictionary_ReturnsAllPropertiesWithCorrectStringValues()
        {
            var options = new ComputeShaderPipelineOptions
            {
                DefaultStrategy = default,
                MaxWorkgroupDimension = 32,
                BenchmarkGuidedOptimization = true,
                EnableProfiling = false,
                MaxPipelineDepth = 10,
                DefaultLocalMemoryPerThreadBytes = 128,
                OccupancyWarningThreshold = 0.25
            };

            Dictionary<string, string> dict = options.ToDictionary();

            Assert.Equal(7, dict.Count);
            Assert.Equal(options.DefaultStrategy.ToString(), dict[nameof(ComputeShaderPipelineOptions.DefaultStrategy)]);
            Assert.Equal(options.MaxWorkgroupDimension.ToString(CultureInfo.InvariantCulture), dict[nameof(ComputeShaderPipelineOptions.MaxWorkgroupDimension)]);
            Assert.Equal(options.BenchmarkGuidedOptimization.ToString(), dict[nameof(ComputeShaderPipelineOptions.BenchmarkGuidedOptimization)]);
            Assert.Equal(options.EnableProfiling.ToString(), dict[nameof(ComputeShaderPipelineOptions.EnableProfiling)]);
            Assert.Equal(options.MaxPipelineDepth.ToString(CultureInfo.InvariantCulture), dict[nameof(ComputeShaderPipelineOptions.MaxPipelineDepth)]);
            Assert.Equal(options.DefaultLocalMemoryPerThreadBytes.ToString(CultureInfo.InvariantCulture), dict[nameof(ComputeShaderPipelineOptions.DefaultLocalMemoryPerThreadBytes)]);
            Assert.Equal(options.OccupancyWarningThreshold.ToString(CultureInfo.InvariantCulture), dict[nameof(ComputeShaderPipelineOptions.OccupancyWarningThreshold)]);
        }

        [Fact]
        public void ToDictionary_NullOptions_ThrowsArgumentNullException()
        {
            ComputeShaderPipelineOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options!.ToDictionary());
        }
    }
}
