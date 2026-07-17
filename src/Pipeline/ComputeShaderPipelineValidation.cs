#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Provides validation helpers for <see cref="ComputeShaderPipeline"/> instances.
/// </summary>
public static class ComputeShaderPipelineValidation
{
    /// <summary>
    /// Validates the specified <see cref="ComputeShaderPipeline"/> instance.
    /// </summary>
    /// <param name="value">The pipeline instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when reflection fails to access pipeline members.</exception>
    public static IReadOnlyList<string> Validate(this ComputeShaderPipeline? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            // Use public API to validate pipeline state instead of fragile reflection
            PipelineStatistics stats = value.GetStatisticsAsync().GetAwaiter().GetResult();

            // Validate statistics values
            if (stats.TotalExecutions < 0)
            {
                problems.Add("Total executions count cannot be negative.");
            }

            if (stats.TotalPassesExecuted < 0)
            {
                problems.Add("Total passes executed count cannot be negative.");
            }

            if (stats.TotalPassesFailed < 0)
            {
                problems.Add("Total passes failed count cannot be negative.");
            }

            if (stats.TotalPixelsProcessed < 0)
            {
                problems.Add("Total pixels processed count cannot be negative.");
            }

            if (stats.TotalProcessingTime < TimeSpan.Zero)
            {
                problems.Add("Total processing time cannot be negative.");
            }

            if (stats.AveragePassDurationMs < 0)
            {
                problems.Add("Average pass duration cannot be negative.");
            }

            if (stats.AverageOccupancy < 0)
            {
                problems.Add("Average occupancy cannot be negative.");
            }

            if (stats.SuccessRate < 0 || stats.SuccessRate > 1)
            {
                problems.Add("Success rate must be between 0 and 1.");
            }

            if (stats.CollectedAt == default)
            {
                problems.Add("Statistics collection timestamp cannot be default.");
            }
            else if (stats.CollectedAt.Kind != DateTimeKind.Utc)
            {
                problems.Add("Statistics timestamp must be in UTC.");
            }
            else if (stats.CollectedAt > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("Statistics timestamp cannot be in the future.");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            problems.Add($"Pipeline internal state is invalid: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ComputeShaderPipeline"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline instance to check.</param>
    /// <returns><see langword="true"/> if the pipeline is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ComputeShaderPipeline? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ComputeShaderPipeline"/> instance is valid.
    /// </summary>
    /// <param name="value">The pipeline instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the pipeline is invalid, containing a list of validation problems.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the pipeline cannot be validated due to internal errors.</exception>
    public static void EnsureValid(this ComputeShaderPipeline? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"ComputeShaderPipeline is invalid. Problems:\n{string.Join("\n", problems)}",
            nameof(value));
    }
}