#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides useful extension methods for <see cref="FilterChainBuilder"/> to simplify fluent chain construction.
/// </summary>
/// <remarks>
/// These extensions add convenience methods for common filter chain patterns and validations.
/// </remarks>
public static class FilterChainBuilderExtensions
{
    /// <summary>
    /// Adds multiple filter steps in a single call using a collection of filter types.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="filterTypes">Collection of filter types to add.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="filterTypes"/> is null.</exception>
    public static FilterChainBuilder AddFilters(this FilterChainBuilder builder, IEnumerable<FilterType> filterTypes)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(filterTypes);

        foreach (var filterType in filterTypes)
        {
            builder = filterType switch
            {
                FilterType.Grayscale => builder.AddGrayscale(),
                FilterType.Blur => builder.AddBlur(),
                FilterType.Sharpen => builder.AddSharpen(),
                FilterType.EdgeDetection => builder.AddEdgeDetection(),
                FilterType.ColorCorrection => builder.AddColorCorrection(),
                FilterType.Threshold => builder.AddThreshold(),
                FilterType.Rotation => builder.AddRotation(0f),
                FilterType.Scaling => builder.AddScaling(1f, 1f),
                FilterType.Bilateral => builder.AddBilateral(),
                FilterType.Median => builder.AddMedian(),
                FilterType.Emboss => builder.AddEmboss(),
                FilterType.Sobel => builder.AddSobel(),
                _ => builder // Skip unsupported types gracefully
            };
        }

        return builder;
    }

    /// <summary>
    /// Adds a sequence of common image enhancement filters (grayscale, sharpen, contrast).
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="sharpenStrength">Sharpen strength (0.0 - 10.0). Defaults to 1.0.</param>
    /// <param name="brightness">Brightness adjustment (-1.0 - 1.0). Defaults to 0.0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    public static FilterChainBuilder AddEnhancementSequence(
        this FilterChainBuilder builder,
        float sharpenStrength = 1.0f,
        float brightness = 0.0f)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .AddGrayscale()
            .AddSharpen(strength: sharpenStrength)
            .AddColorCorrection(brightness: brightness);
    }

    /// <summary>
    /// Adds a standard edge detection pipeline (grayscale, edge detection, threshold).
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="threshold">Threshold value (0.0 - 1.0). Defaults to 0.5.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    public static FilterChainBuilder AddEdgeDetectionPipeline(
        this FilterChainBuilder builder,
        float threshold = 0.5f)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .AddGrayscale()
            .AddEdgeDetection()
            .AddThreshold(threshold);
    }

    /// <summary>
    /// Adds a batch of common filters for noise reduction and smoothing.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="blurRadius">Blur radius in pixels. Defaults to 2.0.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="blurRadius"/> is out of valid range.</exception>
    public static FilterChainBuilder AddNoiseReductionPipeline(
        this FilterChainBuilder builder,
        float blurRadius = 2.0f)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (blurRadius < AppConstants.Filters.MinKernelRadius || blurRadius > AppConstants.Filters.MaxKernelRadius)
        {
            throw new ArgumentOutOfRangeException(
                nameof(blurRadius),
                FormattableString.Invariant($"Blur radius must be between {AppConstants.Filters.MinKernelRadius} and {AppConstants.Filters.MaxKernelRadius}."));
        }

        return builder
            .AddBilateral()
            .AddMedian()
            .AddBlur(radius: blurRadius);
    }
}