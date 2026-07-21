#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Static factory class providing preconfigured filter chain presets for common image processing workflows.
/// </summary>
/// <remarks>
/// <para>
/// This class offers ready-to-use filter chain configurations for typical scenarios:
/// - Sharpen: Enhances image details and edges
/// - Vintage: Applies a retro/vintage film effect
/// - Dramatic: Creates high-contrast dramatic visuals
/// </para>
/// <para>
/// Each preset is built using the <see cref="FilterChainBuilder"/> fluent API and returns a fully configured,
/// validated <see cref="FilterChain"/> instance that can be used directly or as a starting point for customization.
/// </para>
/// <example>
/// <code>
/// // Use a preset directly
/// var sharpenChain = FilterPresets.Sharpen.Build();
///
/// // Or customize a preset
/// var customChain = FilterPresets.Vintage
///     .WithDescription("Custom vintage with extra contrast")
///     .AllowParallelExecution(2)
///     .Build();
/// </code>
/// </example>
/// </remarks>
public static class FilterPresets
{
    /// <summary>
    /// Gets a sharpening filter chain that enhances image details and edges.
    /// </summary>
    /// <remarks>
    /// This preset applies:
    /// 1. Color correction (brightness adjustment)
    /// 2. Sharpening with moderate strength
    /// 3. Optional parallel execution for performance
    /// </remarks>
    public static FilterChainBuilder Sharpen => FilterChainBuilder
        .Create("Sharpen")
        .WithDescription("Enhances image details and edges with sharpening filter")
        .AddColorCorrection(brightness: 0.05f)
        .AddSharpen(strength: 1.2f)
        .AllowParallelExecution(maxParallelSteps: 2);

    /// <summary>
    /// Gets a vintage film effect filter chain that creates a retro aesthetic.
    /// </summary>
    /// <remarks>
    /// This preset applies:
    /// 1. Grayscale conversion
    /// 2. Color correction with reduced brightness
    /// 3. Slight blur for film grain effect
    /// 4. Sharpening to restore some detail
    /// </remarks>
    public static FilterChainBuilder Vintage => FilterChainBuilder
        .Create("Vintage")
        .WithDescription("Creates a retro vintage film effect")
        .AddGrayscale()
        .AddColorCorrection(brightness: -0.15f)
        .AddBlur(radius: 1.2f)
        .AddSharpen(strength: 0.7f)
        .CacheIntermediates();

    /// <summary>
    /// Gets a dramatic high-contrast filter chain for striking visual effects.
    /// </summary>
    /// <remarks>
    /// This preset applies:
    /// 1. Grayscale conversion
    /// 2. Color correction with increased contrast
    /// 3. Sharpening to enhance edges
    /// 4. Threshold for high-contrast binarization effect
    /// </remarks>
    public static FilterChainBuilder Dramatic => FilterChainBuilder
        .Create("Dramatic")
        .WithDescription("Creates high-contrast dramatic visual effects")
        .AddGrayscale()
        .AddColorCorrection(brightness: 0.1f)
        .AddSharpen(strength: 1.5f)
        .AddThreshold(thresholdValue: 0.6f);
}
