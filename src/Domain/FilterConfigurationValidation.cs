#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="FilterConfiguration"/> instances.
/// </summary>
public static class FilterConfigurationValidation
{
    /// <summary>
    /// Validates the filter configuration and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The filter configuration to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FilterConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required properties
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name is required and cannot be empty or whitespace.");
        }

        if (value.FilterType == FilterType.None)
        {
            problems.Add("FilterType must be specified and cannot be None.");
        }

        if (value.MaxThreadsPerBlock is < 32 or > 1024)
        {
            problems.Add("MaxThreadsPerBlock must be between 32 and 1024 (inclusive).");
        }

        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid DateTime.");
        }

        if (value.ModifiedAt == default)
        {
            problems.Add("ModifiedAt must be set to a valid DateTime.");
        }

        if (value.Priority < 0)
        {
            problems.Add("Priority must be a non-negative integer.");
        }

        // Validate Parameters dictionary consistency
        foreach (var param in value.Parameters)
        {
            if (!value.ParameterTypes.ContainsKey(param.Key))
            {
                problems.Add($"Parameter '{param.Key}' has a value but no type definition.");
            }
        }

        // Validate filter-specific parameters
        problems.AddRange(ValidateFilterSpecificParameters(value));

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the filter configuration is valid.
    /// </summary>
    /// <param name="value">The filter configuration to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this FilterConfiguration value)
    {
        var problems = FilterConfigurationValidation.Validate(value);
        return problems.Count == 0;
    }

    /// <summary>
    /// Ensures that the filter configuration is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The filter configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this FilterConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = FilterConfigurationValidation.Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FilterConfiguration is invalid. Problems:\n  - {
                    string.Join("\n  - ", problems)
                }",
                nameof(value));
        }
    }

    private static IReadOnlyList<string> ValidateFilterSpecificParameters(FilterConfiguration config)
    {
        var problems = new List<string>();

        switch (config.FilterType)
        {
            case FilterType.Blur or FilterType.GaussianBlur:
                if (config.Parameters.TryGetValue("radius", out var radiusObj))
                {
                    if (radiusObj is float radius)
                    {
                        if (radius < AppConstants.Filters.MinKernelRadius || radius > AppConstants.Filters.MaxKernelRadius)
                        {
                            problems.Add(
                                $"For {config.FilterType} filter, radius must be between {AppConstants.Filters.MinKernelRadius} and {AppConstants.Filters.MaxKernelRadius} (inclusive). Provided: {radius.ToString(CultureInfo.InvariantCulture)}.");
                        }
                    }
                    else
                    {
                        problems.Add("For Blur/GaussianBlur filter, 'radius' parameter must be a float.");
                    }
                }
                else
                {
                    problems.Add("Blur/GaussianBlur filter requires a 'radius' parameter of type float.");
                }
                break;

            case FilterType.Sharpen:
                if (config.Parameters.TryGetValue("strength", out var strengthObj))
                {
                    if (strengthObj is float strength)
                    {
                        if (strength < 0.0f || strength > 10.0f)
                        {
                            problems.Add(
                                $"For Sharpen filter, strength must be between 0.0 and 10.0 (inclusive). Provided: {strength.ToString(CultureInfo.InvariantCulture)}.");
                        }
                    }
                    else
                    {
                        problems.Add("For Sharpen filter, 'strength' parameter must be a float.");
                    }
                }
                else
                {
                    problems.Add("Sharpen filter requires a 'strength' parameter of type float.");
                }
                break;

            case FilterType.Rotation:
                if (config.Parameters.TryGetValue("angle", out var angleObj))
                {
                    if (angleObj is float angle)
                    {
                        if (angle < -360f || angle > 360f)
                        {
                            problems.Add(
                                $"For Rotation filter, angle must be between -360 and 360 degrees (inclusive). Provided: {angle.ToString(CultureInfo.InvariantCulture)}.");
                        }
                    }
                    else
                    {
                        problems.Add("For Rotation filter, 'angle' parameter must be a float.");
                    }
                }
                else
                {
                    problems.Add("Rotation filter requires an 'angle' parameter of type float.");
                }
                break;

            case FilterType.Scaling:
                if (config.Parameters.TryGetValue("scaleX", out var scaleXObj) &&
                    config.Parameters.TryGetValue("scaleY", out var scaleYObj))
                {
                    if (scaleXObj is float scaleX && scaleYObj is float scaleY)
                    {
                        if (scaleX <= 0 || scaleY <= 0)
                        {
                            problems.Add(
                                $"For Scaling filter, scaleX and scaleY must be positive values. Provided: scaleX={scaleX.ToString(CultureInfo.InvariantCulture)}, scaleY={scaleY.ToString(CultureInfo.InvariantCulture)}.");
                        }
                    }
                    else
                    {
                        problems.Add("For Scaling filter, 'scaleX' and 'scaleY' parameters must be floats.");
                    }
                }
                else
                {
                    problems.Add("Scaling filter requires both 'scaleX' and 'scaleY' parameters of type float.");
                }
                break;

            case FilterType.ColorCorrection:
                if (config.Parameters.TryGetValue("brightness", out var brightnessObj))
                {
                    if (brightnessObj is float brightness)
                    {
                        if (brightness < -1.0f || brightness > 1.0f)
                        {
                            problems.Add(
                                $"For ColorCorrection filter, brightness must be between -1.0 and 1.0 (inclusive). Provided: {brightness.ToString(CultureInfo.InvariantCulture)}.");
                        }
                    }
                    else
                    {
                        problems.Add("For ColorCorrection filter, 'brightness' parameter must be a float.");
                    }
                }
                break;

            case FilterType.Threshold:
                if (config.Parameters.TryGetValue("thresholdValue", out var thresholdObj))
                {
                    if (thresholdObj is float threshold)
                    {
                        if (threshold < 0.0f || threshold > 1.0f)
                        {
                            problems.Add(
                                $"For Threshold filter, thresholdValue must be between 0.0 and 1.0 (inclusive). Provided: {threshold.ToString(CultureInfo.InvariantCulture)}.");
                        }
                    }
                    else
                    {
                        problems.Add("For Threshold filter, 'thresholdValue' parameter must be a float.");
                    }
                }
                else
                {
                    problems.Add("Threshold filter requires a 'thresholdValue' parameter of type float.");
                }
                break;

            case FilterType.CustomConvolution:
                problems.AddRange(ValidateConvolutionKernel(config));
                break;
        }

        return problems.AsReadOnly();
    }

    private static IReadOnlyList<string> ValidateConvolutionKernel(FilterConfiguration config)
    {
        var problems = new List<string>();

        if (config.ConvolutionKernel is null)
        {
            problems.Add("CustomConvolution filter requires a ConvolutionKernel to be set.");
            return problems.AsReadOnly();
        }

        if (config.ConvolutionKernel.Length == 0)
        {
            problems.Add("ConvolutionKernel cannot be empty.");
            return problems.AsReadOnly();
        }

        int len = config.ConvolutionKernel.Length;
        int side = (int)Math.Round(Math.Sqrt(len));

        if (side * side != len)
        {
            problems.Add(
                $"ConvolutionKernel must be a perfect square (length must be a square number). " +
                $"Provided length: {len}, which is not a perfect square.");
        }

        if (side < 3 || side > 15)
        {
            problems.Add(
                $"ConvolutionKernel side length must be between 3 and 15 (inclusive) and odd. " +
                $"Provided side length: {side}.");
        }

        if (side % 2 == 0)
        {
            problems.Add(
                $"ConvolutionKernel side length must be odd. Provided side length: {side}.");
        }

        return problems.AsReadOnly();
    }
}