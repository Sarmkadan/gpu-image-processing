#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using GpuImageProcessing.Benchmarking;

namespace GpuImageProcessing.Tests.Benchmarking;

/// <summary>
/// Tests for the <see cref="BenchmarkSuiteConfiguration"/> class.
/// </summary>
public class BenchmarkSuiteConfigurationTests
{
    /// <summary>
    /// Validates that a valid default configuration returns no errors.
    /// </summary>
    [Fact]
    public void Validate_ValidDefaultConfig_ReturnsNoErrors()
    {
        var config = new BenchmarkSuiteConfiguration { RunName = "CI Run" };

        var errors = config.Validate();

        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Validates that a configuration with a blank run name returns a name error.
    /// </summary>
    [Fact]
    public void Validate_BlankRunName_ReturnsNameError()
    {
        var config = new BenchmarkSuiteConfiguration { RunName = "   " };

        var errors = config.Validate();

        errors.Should().ContainSingle(e => e.Contains(nameof(BenchmarkSuiteConfiguration.RunName)));
    }

    /// <summary>
    /// Validates that a configuration with all benchmark categories disabled returns at least one error.
    /// </summary>
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

    /// <summary>
    /// Gets the enabled benchmark categories for a default configuration.
    /// </summary>
    /// <returns>A collection of enabled benchmark categories.</returns>
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

    /// <summary>
    /// Gets the enabled benchmark categories for a configuration with all categories enabled.
    /// </summary>
    /// <returns>A collection of enabled benchmark categories.</returns>
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

    /// <summary>
    /// Creates a CI preset configuration with quick accuracy and three categories.
    /// </summary>
    /// <param name="runName">The run name for the configuration.</param>
    /// <returns>A CI preset configuration.</returns>
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

    /// <summary>
    /// Creates a release preset configuration with thorough accuracy and all categories.
    /// </summary>
    /// <param name="runName">The run name for the configuration.</param>
    /// <returns>A release preset configuration.</returns>
    [Fact]
    public void ForRelease_Preset_HasThoroughAccuracyAndAllCategories()
    {
        var config = BenchmarkSuiteConfiguration.ForRelease("Release-2026");

        config.AccuracyLevel.Should().Be(BenchmarkAccuracyLevel.Thorough);
        config.EnableHardwareCounters.Should().BeTrue();
        config.GetEnabledCategories().Should().HaveCount(5);
    }

    /// <summary>
    /// Validates a release preset configuration.
    /// </summary>
    [Fact]
    public void ForRelease_Preset_IsValid()
    {
        var config = BenchmarkSuiteConfiguration.ForRelease("Release-2026");
        config.Validate().Should().BeEmpty();
    }

    /// <summary>
    /// Validates a CI preset configuration.
    /// </summary>
    [Fact]
    public void ForCi_Preset_IsValid()
    {
        var config = BenchmarkSuiteConfiguration.ForCi("CI-2026");
        config.Validate().Should().BeEmpty();
    }
}
