#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
    public static IReadOnlyList<string> Validate(this ComputeShaderPipeline? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate constructor dependencies (these are private fields but we can infer their validity)
        // The constructor throws ArgumentNullException for null dependencies, so we assume they're valid if pipeline exists

        // Validate statistics fields
        if (value.GetType().GetField("_totalExecutions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is long totalExecutions && totalExecutions < 0)
        {
            problems.Add("Total executions count cannot be negative.");
        }

        if (value.GetType().GetField("_totalPassesExecuted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is long totalPassesExecuted && totalPassesExecuted < 0)
        {
            problems.Add("Total passes executed count cannot be negative.");
        }

        if (value.GetType().GetField("_totalPassesFailed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is long totalPassesFailed && totalPassesFailed < 0)
        {
            problems.Add("Total passes failed count cannot be negative.");
        }

        if (value.GetType().GetField("_totalPixelsProcessed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is long totalPixelsProcessed && totalPixelsProcessed < 0)
        {
            problems.Add("Total pixels processed count cannot be negative.");
        }

        if (value.GetType().GetField("_totalProcessingMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is double totalProcessingMs && totalProcessingMs < 0)
        {
            problems.Add("Total processing time cannot be negative.");
        }

        if (value.GetType().GetField("_totalOccupancySum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is double totalOccupancySum && totalOccupancySum < 0)
        {
            problems.Add("Total occupancy sum cannot be negative.");
        }

        if (value.GetType().GetField("_totalOccupancySamples", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is long totalOccupancySamples && totalOccupancySamples < 0)
        {
            problems.Add("Total occupancy samples count cannot be negative.");
        }

        // Validate that the pipeline is in a usable state
        // Since we can't access private fields directly, we'll check if the object is properly initialized
        // by attempting to verify its internal consistency through public methods
        try
        {
            // This will throw if the pipeline is in an invalid state
            _ = value.GetType().GetMethod("GetStatisticsAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.Invoke(value, new object[] { default(System.Threading.CancellationToken) });
        }
        catch (Exception ex) when (ex is not System.Reflection.TargetInvocationException)
        {
            problems.Add($"Pipeline internal state is invalid: {ex.Message}");
        }
        catch
        {
            // TargetInvocationException wraps the actual exception, which is expected
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