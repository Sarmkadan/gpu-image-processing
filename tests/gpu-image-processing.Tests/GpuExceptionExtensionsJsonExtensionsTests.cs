using System;
using System.Text.Json;
using Xunit;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests.Core;

public class GpuExceptionExtensionsJsonExtensionsTests
{
    [Fact]
    public void Config_DefaultValues_AreCorrect()
    {
        var config = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig();

        Assert.Equal("GpuExceptionExtensions", config.Type);
        Assert.True(config.IsTimeoutDetectionEnabled);
        Assert.True(config.IsMemoryDetectionEnabled);
        Assert.True(config.IsComputePipelineDetectionEnabled);
    }

    [Fact]
    public void ToJson_NullConfig_ThrowsArgumentNullException()
    {
        GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig? nullConfig = null;
        Assert.Throws<ArgumentNullException>(() => GpuExceptionExtensionsJsonExtensions.ToJson(nullConfig!));
    }

    [Fact]
    public void ToJson_DefaultConfig_NoIndent_ProducesCompactJson()
    {
        var config = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig
        {
            IsTimeoutDetectionEnabled = true,
            IsMemoryDetectionEnabled = true,
            IsComputePipelineDetectionEnabled = true
        };

        var json = GpuExceptionExtensionsJsonExtensions.ToJson(config, indented: false);

        Assert.NotNull(json);
        Assert.DoesNotContain(Environment.NewLine, json);
        Assert.Contains("\"type\":\"GpuExceptionExtensions\"", json);
        Assert.Contains("\"isTimeoutDetectionEnabled\":true", json);
        Assert.Contains("\"isMemoryDetectionEnabled\":true", json);
        Assert.Contains("\"isComputePipelineDetectionEnabled\":true", json);
    }

    [Fact]
    public void ToJson_DefaultConfig_Indented_ProducesFormattedJson()
    {
        var config = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig
        {
            IsTimeoutDetectionEnabled = false,
            IsMemoryDetectionEnabled = false,
            IsComputePipelineDetectionEnabled = false
        };

        var json = GpuExceptionExtensionsJsonExtensions.ToJson(config, indented: true);

        Assert.NotNull(json);
        Assert.Contains(Environment.NewLine, json); // formatted JSON contains line breaks
        Assert.Contains("\"type\":\"GpuExceptionExtensions\"", json);
        Assert.Contains("\"isTimeoutDetectionEnabled\":false", json);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsConfig()
    {
        var original = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig
        {
            IsTimeoutDetectionEnabled = false,
            IsMemoryDetectionEnabled = true,
            IsComputePipelineDetectionEnabled = false
        };
        var json = GpuExceptionExtensionsJsonExtensions.ToJson(original, indented: false);

        var deserialized = GpuExceptionExtensionsJsonExtensions.FromJson(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Type, deserialized!.Type);
        Assert.Equal(original.IsTimeoutDetectionEnabled, deserialized.IsTimeoutDetectionEnabled);
        Assert.Equal(original.IsMemoryDetectionEnabled, deserialized.IsMemoryDetectionEnabled);
        Assert.Equal(original.IsComputePipelineDetectionEnabled, deserialized.IsComputePipelineDetectionEnabled);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(GpuExceptionExtensionsJsonExtensions.FromJson(null));
        Assert.Null(GpuExceptionExtensionsJsonExtensions.FromJson(string.Empty));
        Assert.Null(GpuExceptionExtensionsJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndConfig()
    {
        var config = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig
        {
            IsTimeoutDetectionEnabled = true,
            IsMemoryDetectionEnabled = false,
            IsComputePipelineDetectionEnabled = true
        };
        var json = GpuExceptionExtensionsJsonExtensions.ToJson(config, indented: false);

        var success = GpuExceptionExtensionsJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(config.IsTimeoutDetectionEnabled, result!.IsTimeoutDetectionEnabled);
        Assert.Equal(config.IsMemoryDetectionEnabled, result.IsMemoryDetectionEnabled);
        Assert.Equal(config.IsComputePipelineDetectionEnabled, result.IsComputePipelineDetectionEnabled);
    }

    [Fact]
    public void TryFromJson_NullOrEmpty_ReturnsTrueWithNullResult()
    {
        var successNull = GpuExceptionExtensionsJsonExtensions.TryFromJson(null, out var resultNull);
        var successEmpty = GpuExceptionExtensionsJsonExtensions.TryFromJson(string.Empty, out var resultEmpty);
        var successWhitespace = GpuExceptionExtensionsJsonExtensions.TryFromJson("   ", out var resultWhitespace);

        Assert.True(successNull);
        Assert.Null(resultNull);

        Assert.True(successEmpty);
        Assert.Null(resultEmpty);

        Assert.True(successWhitespace);
        Assert.Null(resultWhitespace);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var invalidJson = "{ invalid json {{{";

        var success = GpuExceptionExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
