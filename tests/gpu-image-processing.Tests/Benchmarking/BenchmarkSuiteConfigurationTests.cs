#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpuImageProcessing.Benchmarking;

namespace GpuImageProcessing.Tests.Benchmarking;

public class BenchmarkSuiteConfigurationTests
{
    [Fact]
    public void Validate_ValidDefaultConfig_ReturnsNoErrors()
    {
        var config = new BenchmarkSuiteConfiguration { RunName = "CI Run" };

        var errors = config.Validate();

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_BlankRunName_ReturnsNameError()
    {
        var config = new BenchmarkSuiteConfiguration { RunName = "   " };

        var errors = config.Validate();

        errors.Should().ContainSingle(e => e.Contains(nameof(BenchmarkSuiteConfiguration.RunName)));
    }

    [Fact]
    public void Validate_AllCategoriesDisabled_ReturnsAtLeastOneError()
    {
        var config = new BenchmarkSuiteConfiguration
        {
            RunName = "Empty",
            IncludeFilterChainBenchmarks = false,
            IncludeBatchProcessingBenchmarks = false,
            IncludeFilterChainBuilderBenchmarks = false,
            IncludeImageUtilitiesBenchmarks = false,
            IncludeEnumerableExtensionsBenchmarks = false
        };

        var errors = config.Validate();

        errors.Should().ContainSingle(e => e.Contains("at least one benchmark category"));
    }

    [Fact]
    public void GetEnabledCategories_DefaultConfig_ContainsFourCategories()
    {
        // Default: all enabled except EnumerableExtensions
        var config = new BenchmarkSuiteConfiguration { RunName = "Default" };

        var categories = config.GetEnabledCategories();

        categories.Should().HaveCount(4);
        categories.Should().Contain("FilterChain");
        categories.Should().Contain("BatchProcessing");
        categories.Should().Contain("FilterChainBuilder");
        categories.Should().Contain("ImageUtilities");
        categories.Should().NotContain("EnumerableExtensions");
    }

    [Fact]
    public void GetEnabledCategories_AllEnabled_ContainsFiveCategories()
    {
        var config = new BenchmarkSuiteConfiguration
        {
            RunName = "Full",
            IncludeEnumerableExtensionsBenchmarks = true
        };

        var categories = config.GetEnabledCategories();

        categories.Should().HaveCount(5);
        categories.Should().Contain("EnumerableExtensions");
    }

    [Fact]
    public void ForCi_Preset_HasQuickAccuracyAndThreeCategories()
    {
        var config = BenchmarkSuiteConfiguration.ForCi("CI-2026");

        config.AccuracyLevel.Should().Be(BenchmarkAccuracyLevel.Quick);
        config.RunName.Should().Be("CI-2026");
        config.GetEnabledCategories().Should().HaveCount(3);
        config.IncludeImageUtilitiesBenchmarks.Should().BeFalse();
        config.IncludeEnumerableExtensionsBenchmarks.Should().BeFalse();
    }

    [Fact]
    public void ForRelease_Preset_HasThoroughAccuracyAndAllCategories()
    {
        var config = BenchmarkSuiteConfiguration.ForRelease("Release-2026");

        config.AccuracyLevel.Should().Be(BenchmarkAccuracyLevel.Thorough);
        config.EnableHardwareCounters.Should().BeTrue();
        config.GetEnabledCategories().Should().HaveCount(5);
    }

    [Fact]
    public void ForRelease_Preset_IsValid()
    {
        var config = BenchmarkSuiteConfiguration.ForRelease("Release-2026");
        config.Validate().Should().BeEmpty();
    }

    [Fact]
    public void ForCi_Preset_IsValid()
    {
        var config = BenchmarkSuiteConfiguration.ForCi("CI-2026");
        config.Validate().Should().BeEmpty();
    }
}
