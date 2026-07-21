#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Contains unit tests for the <see cref="FilterPresets"/> static class, which provides preconfigured filter chain presets.
/// </summary>
/// <remarks>
/// These tests validate that each preset produces a valid, well-formed filter chain
/// with the expected number of steps and configuration.
/// </remarks>
public class FilterPresetsTests
{
    /// <summary>
    /// Tests that the Sharpen preset produces a valid filter chain with the expected steps.
    /// </summary>
    /// <remarks>
    /// Validates that the Sharpen preset contains the expected filter sequence:
    /// 1. Color correction
    /// 2. Sharpen
    /// </remarks>
    [Fact]
    public void Sharpen_Preset_ProducesValidChainWithExpectedSteps()
    {
        // Act
        var chain = FilterPresets.Sharpen.Build();

        // Assert
        chain.Should().NotBeNull();
        chain.Name.Should().Be("Sharpen");
        chain.Description.Should().Be("Enhances image details and edges with sharpening filter");
        chain.Steps.Should().HaveCount(2);
        chain.IsEnabled.Should().BeTrue();
        chain.Validate().Should().BeTrue();
        chain.AllowParallelExecution.Should().BeTrue();
        chain.MaxParallelSteps.Should().Be(2);

        // Verify step order and types
        chain.Steps[0].FilterId.Should().NotBe(Guid.Empty);
        chain.Steps[0].Order.Should().Be(0);
        chain.Steps[0].IsEnabled.Should().BeTrue();

        chain.Steps[1].FilterId.Should().NotBe(Guid.Empty);
        chain.Steps[1].Order.Should().Be(1);
        chain.Steps[1].IsEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the Vintage preset produces a valid filter chain with the expected steps.
    /// </summary>
    /// <remarks>
    /// Validates that the Vintage preset contains the expected filter sequence:
    /// 1. Grayscale
    /// 2. Color correction (reduced brightness)
    /// 3. Blur
    /// 4. Sharpen
    /// </remarks>
    [Fact]
    public void Vintage_Preset_ProducesValidChainWithExpectedSteps()
    {
        // Act
        var chain = FilterPresets.Vintage.Build();

        // Assert
        chain.Should().NotBeNull();
        chain.Name.Should().Be("Vintage");
        chain.Description.Should().Be("Creates a retro vintage film effect");
        chain.Steps.Should().HaveCount(4);
        chain.IsEnabled.Should().BeTrue();
        chain.Validate().Should().BeTrue();
        chain.CacheIntermediateResults.Should().BeTrue();

        // Verify step order
        chain.Steps.Should().HaveCount(4);
        chain.Steps[0].Order.Should().Be(0);
        chain.Steps[1].Order.Should().Be(1);
        chain.Steps[2].Order.Should().Be(2);
        chain.Steps[3].Order.Should().Be(3);
    }

    /// <summary>
    /// Tests that the Dramatic preset produces a valid filter chain with the expected steps.
    /// </summary>
    /// <remarks>
    /// Validates that the Dramatic preset contains the expected filter sequence:
    /// 1. Grayscale
    /// 2. Color correction (increased brightness)
    /// 3. Sharpen
    /// 4. Threshold
    /// </remarks>
    [Fact]
    public void Dramatic_Preset_ProducesValidChainWithExpectedSteps()
    {
        // Act
        var chain = FilterPresets.Dramatic.Build();

        // Assert
        chain.Should().NotBeNull();
        chain.Name.Should().Be("Dramatic");
        chain.Description.Should().Be("Creates high-contrast dramatic visual effects");
        chain.Steps.Should().HaveCount(4);
        chain.IsEnabled.Should().BeTrue();
        chain.Validate().Should().BeTrue();

        // Verify step order
        chain.Steps.Should().HaveCount(4);
        chain.Steps[0].Order.Should().Be(0);
        chain.Steps[1].Order.Should().Be(1);
        chain.Steps[2].Order.Should().Be(2);
        chain.Steps[3].Order.Should().Be(3);
    }

    /// <summary>
    /// Tests that all presets produce chains that can be customized after retrieval.
    /// </summary>
    /// <remarks>
    /// Validates that presets return builders that support fluent API operations,
    /// allowing further customization before building the final chain.
    /// </remarks>
    [Fact]
    public void Presets_ReturnBuildersThatSupportCustomization()
    {
        // Act - customize each preset
        var customizedSharpen = FilterPresets.Sharpen
            .WithDescription("Custom sharpened version")
            .WithExecutionOrder(5)
            .Build();

        var customizedVintage = FilterPresets.Vintage
            .AllowParallelExecution(3)
            .Build();

        var customizedDramatic = FilterPresets.Dramatic
            .WithDescription("High contrast dramatic")
            .CacheIntermediates()
            .Build();

        // Assert
        customizedSharpen.Description.Should().Be("Custom sharpened version");
        customizedSharpen.ExecutionOrder.Should().Be(5);

        customizedVintage.AllowParallelExecution.Should().BeTrue();
        customizedVintage.MaxParallelSteps.Should().Be(3);

        customizedDramatic.Description.Should().Be("High contrast dramatic");
        customizedDramatic.CacheIntermediateResults.Should().BeTrue();
    }

    /// <summary>
    /// Tests that each preset produces a different chain configuration.
    /// </summary>
    /// <remarks>
    /// Validates that the three presets are distinct and not returning the same chain.
    /// </remarks>
    [Fact]
    public void AllPresets_ProduceDistinctChains()
    {
        // Act
        var sharpenChain = FilterPresets.Sharpen.Build();
        var vintageChain = FilterPresets.Vintage.Build();
        var dramaticChain = FilterPresets.Dramatic.Build();

        // Assert
        sharpenChain.Name.Should().NotBe(vintageChain.Name);
        vintageChain.Name.Should().NotBe(dramaticChain.Name);
        dramaticChain.Name.Should().NotBe(sharpenChain.Name);

        // Verify each preset has the expected number of steps
        sharpenChain.Steps.Should().HaveCount(2);
        vintageChain.Steps.Should().HaveCount(4);
        dramaticChain.Steps.Should().HaveCount(4);
    }

    /// <summary>
    /// Tests that all preset chains have reasonable estimated processing times.
    /// </summary>
    /// <remarks>
    /// Validates that each preset produces a chain with a positive, reasonable processing time estimate.
    /// </remarks>
    [Fact]
    public void AllPresets_ProduceChainsWithValidProcessingTimes()
    {
        // Act
        var sharpenChain = FilterPresets.Sharpen.Build();
        var vintageChain = FilterPresets.Vintage.Build();
        var dramaticChain = FilterPresets.Dramatic.Build();

        // Assert
        var sharpenTime = sharpenChain.EstimateTotalProcessingTime();
        var vintageTime = vintageChain.EstimateTotalProcessingTime();
        var dramaticTime = dramaticChain.EstimateTotalProcessingTime();

        sharpenTime.Should().BeGreaterThan(0);
        vintageTime.Should().BeGreaterThan(0);
        dramaticTime.Should().BeGreaterThan(0);

        // Reasonable upper bounds (should complete in under 1 second on typical hardware)
        sharpenTime.Should().BeLessThan(1000);
        vintageTime.Should().BeLessThan(1000);
        dramaticTime.Should().BeLessThan(1000);
    }
}
