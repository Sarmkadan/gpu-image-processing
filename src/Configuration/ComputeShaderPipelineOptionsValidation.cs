#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Validation helpers for <see cref="ComputeShaderPipelineOptions"/>.
/// </summary>
public static class ComputeShaderPipelineOptionsValidation
{
    /// <summary>
    /// Validates the specified <paramref name="value"/> and returns a list of
    /// human-readable problem descriptions. An empty list indicates success.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>A list of problem descriptions; empty if the options are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ComputeShaderPipelineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxWorkgroupDimension < 1 || value.MaxWorkgroupDimension > 1024)
            problems.Add($"MaxWorkgroupDimension must be between 1 and 1024, but was {value.MaxWorkgroupDimension}.");

        if (value.MaxPipelineDepth < 1)
            problems.Add($"MaxPipelineDepth must be at least 1, but was {value.MaxPipelineDepth}.");

        if (value.DefaultLocalMemoryPerThreadBytes < 0)
            problems.Add($"DefaultLocalMemoryPerThreadBytes must be non-negative, but was {value.DefaultLocalMemoryPerThreadBytes}.");

        if (value.OccupancyWarningThreshold < 0.0 || value.OccupancyWarningThreshold > 1.0)
            problems.Add($"OccupancyWarningThreshold must be between 0.0 and 1.0, but was {value.OccupancyWarningThreshold.ToString(CultureInfo.InvariantCulture)}.");

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns><see langword="true"/> if the options are valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ComputeShaderPipelineOptions value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <paramref name="value"/> is valid, throwing an
    /// <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid; the exception message lists the problems.</exception>
    public static void EnsureValid(this ComputeShaderPipelineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
            throw new ArgumentException(
                $"Invalid ComputeShaderPipelineOptions: {string.Join(Environment.NewLine, problems)}",
                nameof(value));
    }
}
