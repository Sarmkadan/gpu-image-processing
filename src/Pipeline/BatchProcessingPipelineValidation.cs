#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Provides validation helpers for <see cref="BatchProcessingPipeline"/> and related types.
/// </summary>
public static class BatchProcessingPipelineValidation
{
    /// <summary>
    /// Validates the specified <see cref="BatchPipelineResult"/> instance.
    /// </summary>
    /// <param name="value">The batch pipeline result to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this BatchPipelineResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate BatchId
        if (value.BatchId == Guid.Empty)
        {
            problems.Add("Batch ID cannot be empty.");
        }

        // Validate BatchName
        if (string.IsNullOrWhiteSpace(value.BatchName))
        {
            problems.Add("Batch name cannot be null or whitespace.");
        }
        else if (value.BatchName.Length > 255)
        {
            problems.Add("Batch name cannot exceed 255 characters.");
        }

        // Validate TotalImages
        if (value.TotalImages < 0)
        {
            problems.Add("Total images count cannot be negative.");
        }

        // Validate SucceededCount
        if (value.SucceededCount < 0)
        {
            problems.Add("Succeeded count cannot be negative.");
        }

        // Validate FailedCount
        if (value.FailedCount < 0)
        {
            problems.Add("Failed count cannot be negative.");
        }

        // Validate that succeeded + failed equals total
        if (value.TotalImages > 0 && value.SucceededCount + value.FailedCount != value.TotalImages)
        {
            problems.Add("Succeeded count plus failed count must equal total images count.");
        }

        // Validate TotalDuration
        if (value.TotalDuration < TimeSpan.Zero)
        {
            problems.Add("Total duration cannot be negative.");
        }

        // Validate AverageProcessingMs
        if (value.AverageProcessingMs < 0)
        {
            problems.Add("Average processing time cannot be negative.");
        }

        // Validate CompletedAt
        if (value.CompletedAt == default)
        {
            problems.Add("Completed timestamp cannot be default (Unix epoch).");
        }
        else if (value.CompletedAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("Completed timestamp must be in UTC.");
        }
        else if (value.CompletedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Completed timestamp cannot be in the future.");
        }

        // Validate Outcomes collection using pattern matching
        if (value.Outcomes is null)
        {
            problems.Add("Outcomes collection cannot be null.");
        }
        else
        {
            // Validate Outcomes count matches TotalImages
            if (value.TotalImages > 0 && value.Outcomes.Count != value.TotalImages)
            {
                problems.Add("Outcomes collection count must match total images count.");
            }

            // Validate each outcome using pattern matching and expression bodies
            foreach (var outcome in value.Outcomes)
            {
                if (outcome is null)
                {
                    problems.Add("Outcome in collection cannot be null.");
                    continue;
                }

                // Validate ImageId
                if (outcome.ImageId == Guid.Empty)
                {
                    problems.Add("Image ID in outcome cannot be empty.");
                }

                // Validate Stage using pattern matching
                if (outcome.Stage is not (>= PipelineStage.Pending and <= PipelineStage.Failed))
                {
                    problems.Add($"Image outcome has invalid stage: {outcome.Stage}.");
                }

                // Validate Attempts
                if (outcome.Attempts < 1)
                {
                    problems.Add("Attempts count must be at least 1.");
                }

                // Validate ProcessingMs
                if (outcome.ProcessingMs < 0)
                {
                    problems.Add("Processing time cannot be negative.");
                }

                // Validate Error message using pattern matching
                if (outcome.Stage == PipelineStage.Failed)
                {
                    if (string.IsNullOrWhiteSpace(outcome.Error))
                    {
                        problems.Add("Failed outcome must have an error message.");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(outcome.Error))
                {
                    problems.Add("Only failed outcomes should have error messages.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BatchPipelineResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The batch pipeline result to check.</param>
    /// <returns><see langword="true"/> if the result is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this BatchPipelineResult? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="BatchPipelineResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The batch pipeline result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the result is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this BatchPipelineResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"BatchPipelineResult is invalid. Problems:\n{string.Join("\n", problems)}",
            nameof(value));
    }
}