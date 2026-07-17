#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides extension methods for <see cref="FilterConfiguration"/> to simplify common operations
/// like parameter management, configuration copying, and filter-specific helpers.
/// </summary>
public static class FilterConfigurationExtensions
{
    /// <summary>
    /// Gets a parameter value by key with type safety and default value fallback.
    /// </summary>
    /// <typeparam name="T">The expected parameter type.</typeparam>
    /// <param name="configuration">The filter configuration.</param>
    /// <param name="key">The parameter key.</param>
    /// <param name="defaultValue">The default value to return if the parameter is not found or cannot be cast.</param>
    /// <returns>The parameter value if found and of correct type, otherwise the default value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static T GetParameter<T>(this FilterConfiguration configuration, string key, T defaultValue) where T : class
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return configuration.GetParameter<T>(key) ?? defaultValue;
    }

    /// <summary>
    /// Sets a parameter with automatic type inference from the provided value.
    /// </summary>
    /// <param name="configuration">The filter configuration.</param>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static void SetParameter<T>(this FilterConfiguration configuration, string key, T value) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);

        var typeName = typeof(T).FullName ?? typeof(T).Name;
        configuration.SetParameter(key, value, typeName);
    }

    /// <summary>
    /// Creates a deep copy of the filter configuration with a new unique identifier.
    /// </summary>
    /// <param name="configuration">The filter configuration to clone.</param>
    /// <returns>A new instance with the same properties but a new <see cref="Guid"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static FilterConfiguration WithNewId(this FilterConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var clone = configuration.Clone();
        clone.Id = Guid.NewGuid();
        clone.CreatedAt = DateTime.UtcNow;
        clone.ModifiedAt = DateTime.UtcNow;
        return clone;
    }

    /// <summary>
    /// Gets the convolution kernel size (side length) if the filter type is <see cref="FilterType.CustomConvolution"/>.
    /// </summary>
    /// <param name="configuration">The filter configuration.</param>
    /// <returns>The kernel size (3, 5, 7, etc.) if available, otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static int? GetConvolutionKernelSize(this FilterConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (configuration.FilterType != FilterType.CustomConvolution)
            return null;

        if (configuration.ConvolutionKernel is null || configuration.ConvolutionKernel.Length == 0)
            return null;

        int side = (int)Math.Round(Math.Sqrt(configuration.ConvolutionKernel.Length));
        return side % 2 == 1 ? side : null; // Only odd sizes are valid
    }

    /// <summary>
    /// Determines whether this filter configuration is a convolution-based filter.
    /// </summary>
    /// <param name="configuration">The filter configuration.</param>
    /// <returns><see langword="true"/> if the filter is convolution-based; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static bool IsConvolutionFilter(this FilterConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.FilterType is FilterType.Blur
            or FilterType.GaussianBlur
            or FilterType.Median
            or FilterType.Bilateral
            or FilterType.Emboss
            or FilterType.Sobel
            or FilterType.EdgeDetection
            or FilterType.CustomConvolution;
    }

    /// <summary>
    /// Gets the normalized parameter value as a float between 0 and 1.
    /// </summary>
    /// <param name="configuration">The filter configuration.</param>
    /// <param name="key">The parameter key.</param>
    /// <param name="defaultValue">The default value to return if the parameter is not found.</param>
    /// <returns>A normalized float value between 0 and 1.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static float GetNormalizedParameter(this FilterConfiguration configuration, string key, float defaultValue = 0.5f)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (!configuration.Parameters.TryGetValue(key, out var value))
            return defaultValue;

        return value switch
        {
            float f => Math.Clamp(f, 0f, 1f),
            int i => Math.Clamp(i / 100f, 0f, 1f),
            double d => Math.Clamp((float)d, 0f, 1f),
            string s when float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) => Math.Clamp(parsed, 0f, 1f),
            _ => defaultValue
        };
    }
}