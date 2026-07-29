using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using GpuImageProcessing.Configuration;

namespace GpuImageProcessing.Tests.Configuration
{
    public class AppSettingsExtensionsTests
    {
        [Fact]
        public void GetOperationTimeout_ReturnsCorrectTimeSpan()
        {
            var settings = new AppSettings { OperationTimeoutMs = 1500 };
            var result = AppSettingsExtensions.GetOperationTimeout(settings);
            Assert.Equal(TimeSpan.FromMilliseconds(1500), result);
        }

        [Fact]
        public void GetOperationTimeout_NullSettings_ThrowsArgumentNullException()
        {
            AppSettings? settings = null;
            Assert.Throws<ArgumentNullException>(() => AppSettingsExtensions.GetOperationTimeout(settings!));
        }

        [Fact]
        public void GetCachePath_ReturnsCombinedPath()
        {
            var settings = new AppSettings
            {
                CacheDirectory = "/tmp/cache",
                ApplicationName = "MyApp"
            };

            var expected = Path.Combine(settings.CacheDirectory, settings.ApplicationName);
            var actual = AppSettingsExtensions.GetCachePath(settings);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "MyApp")]
        [InlineData("", "MyApp")]
        [InlineData("/tmp/cache", null)]
        [InlineData("/tmp/cache", "")]
        public void GetCachePath_InvalidInputs_ThrowsArgumentException(string cacheDir, string appName)
        {
            var settings = new AppSettings
            {
                CacheDirectory = cacheDir,
                ApplicationName = appName
            };

            // Null settings are handled separately; here we focus on empty/invalid strings.
            if (settings.CacheDirectory == null || settings.ApplicationName == null)
            {
                Assert.Throws<ArgumentNullException>(() => AppSettingsExtensions.GetCachePath(settings));
            }
            else
            {
                Assert.Throws<ArgumentException>(() => AppSettingsExtensions.GetCachePath(settings));
            }
        }

        [Fact]
        public void GetSupportedImageFormats_ReturnsReadOnlyList_WithValues()
        {
            var formats = new List<string> { "png", "jpg" };
            var settings = new AppSettings { SupportedImageFormats = formats };

            var result = AppSettingsExtensions.GetSupportedImageFormats(settings);
            Assert.IsAssignableFrom<IReadOnlyList<string>>(result);
            Assert.Equal(formats.Count, result.Count);
            Assert.Equal(formats[0], result[0]);
            Assert.Equal(formats[1], result[1]);
        }

        [Fact]
        public void GetSupportedImageFormats_NullList_ReturnsEmptyArray()
        {
            var settings = new AppSettings { SupportedImageFormats = null };
            var result = AppSettingsExtensions.GetSupportedImageFormats(settings);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            var settings = new AppSettings
            {
                ApplicationName = "TestApp",
                ApplicationVersion = "1.0.0",
                OutputDirectory = "/tmp/out",
                CacheDirectory = "/tmp/cache",
                MaxConcurrentOperations = 4,
                OperationTimeoutMs = 5000,
                MaxBatchSize = 10,
                MaxMemoryPerImage = 256,
                MaxTotalGpuMemory = 4096,
                EnableCaching = true,
                CacheExpirMinutes = 30,
                EnableMetricsCollection = true,
                MetricsCollectionIntervalMs = 1000,
                SupportedImageFormats = new List<string> { "png" }
            };

            // Call the extension method via its static class to avoid ambiguity.
            AppSettingsExtensions.EnsureValid(settings);
        }

        [Fact]
        public void EnsureValid_NullSettings_ThrowsArgumentNullException()
        {
            AppSettings? settings = null;
            Assert.Throws<ArgumentNullException>(() => AppSettingsExtensions.EnsureValid(settings!));
        }

        [Fact]
        public void EnsureValid_MissingApplicationName_ThrowsArgumentException()
        {
            var settings = new AppSettings
            {
                ApplicationName = "",
                ApplicationVersion = "1.0",
                OutputDirectory = "/tmp/out",
                CacheDirectory = "/tmp/cache",
                MaxConcurrentOperations = 1,
                OperationTimeoutMs = 1000,
                MaxBatchSize = 1,
                MaxMemoryPerImage = 1,
                MaxTotalGpuMemory = 1,
                EnableCaching = false,
                SupportedImageFormats = new List<string> { "png" }
            };

            Assert.Throws<ArgumentException>(() => AppSettingsExtensions.EnsureValid(settings));
        }

        [Fact]
        public void EnsureValid_ZeroMaxConcurrentOperations_ThrowsArgumentException()
        {
            var settings = new AppSettings
            {
                ApplicationName = "App",
                ApplicationVersion = "1.0",
                OutputDirectory = "/tmp/out",
                CacheDirectory = "/tmp/cache",
                MaxConcurrentOperations = 0,
                OperationTimeoutMs = 1000,
                MaxBatchSize = 1,
                MaxMemoryPerImage = 1,
                MaxTotalGpuMemory = 1,
                EnableCaching = false,
                SupportedImageFormats = new List<string> { "png" }
            };

            Assert.Throws<ArgumentException>(() => AppSettingsExtensions.EnsureValid(settings));
        }
    }
}
