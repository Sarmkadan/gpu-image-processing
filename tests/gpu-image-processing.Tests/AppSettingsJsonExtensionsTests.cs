using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using GpuImageProcessing.Configuration;

namespace GpuImageProcessing.Tests.Configuration
{
    public class AppSettingsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ReturnsCorrectJson()
        {
            var settings = new AppSettings { OperationTimeoutMs = 1500 };
            var result = AppSettingsJsonExtensions.ToJson(settings);
            Assert.Contains("\"OperationTimeoutMs\": 1500", result);
        }

        [Fact]
        public void FromJson_ReturnsCorrectSettings()
        {
            var json = "{ \"OperationTimeoutMs\": 1500 }";
            var result = AppSettingsJsonExtensions.FromJson(json);
            Assert.NotNull(result);
            Assert.Equal(1500, result.OperationTimeoutMs);
        }

        [Fact]
        public void TryFromJson_ReturnsCorrectSettings()
        {
            var json = "{ \"OperationTimeoutMs\": 1500 }";
            Assert.True(AppSettingsJsonExtensions.TryFromJson(json, out var result));
            Assert.NotNull(result);
            Assert.Equal(1500, result.OperationTimeoutMs);
        }
    }
}