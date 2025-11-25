#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="FilterChain"/> instances.
/// </summary>
public static class FilterChainValidation
{
    /// <summary>
    /// Validates the specified filter chain.
    /// </summary>
    /// <param name="value">The filter chain to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FilterChain? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            problems.Add("FilterChain.Id cannot be empty (Guid.Empty).");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("FilterChain.Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > AppConstants.FileSystem.MaxFileNameLength)
        {
            problems.Add($"FilterChain.Name exceeds maximum length of {AppConstants.FileSystem.MaxFileNameLength} characters.");
        }

        // Validate Description
        if (value.Description.Length > AppConstants.FileSystem.MaxFileNameLength * 2)
        {
            problems.Add($"FilterChain.Description exceeds maximum length of {AppConstants.FileSystem.MaxFileNameLength * 2} characters.");
        }

        // Validate Steps
        if (value.Steps is null)
        {
            problems.Add("FilterChain.Steps cannot be null.");
        }
        else
        {
            if (value.Steps.Count == 0)
            {
                problems.Add("FilterChain.Steps cannot be empty.");
            }

            for (int i = 0; i < value.Steps.Count; i++)
            {
                var step = value.Steps[i];
                if (step is null)
                {
                    problems.Add($"FilterChain.Steps[{i}] cannot be null.");
                    continue;
                }

                if (step.FilterId == Guid.Empty)
                {
                    problems.Add($"FilterChain.Steps[{i}].FilterId cannot be empty (Guid.Empty).");
                }

                if (step.Order < 0)
                {
                    problems.Add($"FilterChain.Steps[{i}].Order cannot be negative.");
                }

                if (step.EstimatedExecutionTimeMs < 0)
                {
                    problems.Add($"FilterChain.Steps[{i}].EstimatedExecutionTimeMs cannot be negative.");
                }

                if (step.StepId == Guid.Empty)
                {
                    problems.Add($"FilterChain.Steps[{i}].StepId cannot be empty (Guid.Empty).");
                }
            }
        }

        // Validate IsEnabled
        // No specific validation needed beyond being a boolean

        // Validate ExecutionOrder
        if (value.ExecutionOrder < 0)
        {
            problems.Add("FilterChain.ExecutionOrder cannot be negative.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("FilterChain.CreatedAt cannot be default (DateTime.MinValue).");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("FilterChain.CreatedAt cannot be in the future.");
        }

        // Validate ModifiedAt
        if (value.ModifiedAt == default)
        {
            problems.Add("FilterChain.ModifiedAt cannot be default (DateTime.MinValue).");
        }
        else if (value.ModifiedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("FilterChain.ModifiedAt cannot be in the future.");
        }
        else if (value.ModifiedAt < value.CreatedAt)
        {
            problems.Add("FilterChain.ModifiedAt cannot be earlier than FilterChain.CreatedAt.");
        }

        // Validate ChainOptions
        if (value.ChainOptions is null)
        {
            problems.Add("FilterChain.ChainOptions cannot be null.");
        }
        else
        {
            foreach (var key in value.ChainOptions.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    problems.Add("FilterChain.ChainOptions contains a null or whitespace key.");
                    break;
                }

                if (key.Length > AppConstants.FileSystem.MaxFileNameLength)
                {
                    problems.Add($"FilterChain.ChainOptions key '{key}' exceeds maximum length of {AppConstants.FileSystem.MaxFileNameLength} characters.");
                    break;
                }
            }
        }

        // Validate AllowParallelExecution
        // No specific validation needed beyond being a boolean

        // Validate MaxParallelSteps
        if (value.AllowParallelExecution)
        {
            if (value.MaxParallelSteps < 1)
            {
                problems.Add("FilterChain.MaxParallelSteps cannot be less than 1 when AllowParallelExecution is true.");
            }
            else if (value.MaxParallelSteps > AppConstants.Processing.MaxConcurrentOperations)
            {
                problems.Add($"FilterChain.MaxParallelSteps cannot exceed {AppConstants.Processing.MaxConcurrentOperations} when AllowParallelExecution is true.");
            }
        }
        else if (value.MaxParallelSteps != 0)
        {
            problems.Add("FilterChain.MaxParallelSteps should be 0 when AllowParallelExecution is false.");
        }

        // Validate CacheIntermediateResults
        // No specific validation needed beyond being a boolean

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified filter chain is valid.
    /// </summary>
    /// <param name="value">The filter chain to check.</param>
    /// <returns><see langword="true"/> if the filter chain is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this FilterChain? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified filter chain is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The filter chain to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the filter chain is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this FilterChain? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"FilterChain is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}