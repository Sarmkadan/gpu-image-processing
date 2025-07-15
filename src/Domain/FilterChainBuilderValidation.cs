#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="FilterChainBuilder"/> instances.
/// </summary>
/// <remarks>
/// This static class contains extension methods that validate the state of a
/// <see cref="FilterChainBuilder"/> instance, ensuring all constraints are met before building.
/// </remarks>
public static class FilterChainBuilderValidation
{
    /// <summary>
    /// Validates the given <see cref="FilterChainBuilder"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FilterChainBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate chain name (set in Create)
        if (string.IsNullOrWhiteSpace(value.GetName()))
        {
            errors.Add("Chain name must not be blank.");
        }

        // Validate description (optional, can be empty)
        // No validation needed for description as it defaults to empty string

        // Validate execution order (optional, defaults to 0)
        // No validation needed for execution order

        // Validate parallel execution settings
        if (value.GetAllowParallel() && value.GetMaxParallelSteps() < 1)
        {
            errors.Add("When parallel execution is enabled, max parallel steps must be at least 1.");
        }

        if (value.GetAllowParallel() && value.GetMaxParallelSteps() > AppConstants.Processing.DefaultThreadCount * 4)
        {
            errors.Add($"When parallel execution is enabled, max parallel steps must not exceed {AppConstants.Processing.DefaultThreadCount * 4}.");
        }

        // Validate cache intermediates flag (boolean, no validation needed)

        // Validate that at least one filter step has been added
        if (value.GetStepCount() == 0)
        {
            errors.Add("A filter chain must contain at least one filter step.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the given <see cref="FilterChainBuilder"/> is in a valid state.
    /// </summary>
    /// <param name="value">The builder instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this FilterChainBuilder value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the given <see cref="FilterChainBuilder"/> is in a valid state.
    /// </summary>
    /// <param name="value">The builder instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing all error messages.</exception>
    public static void EnsureValid(this FilterChainBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "FilterChainBuilder is not in a valid state. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }

    // Private reflection helpers to access private fields of FilterChainBuilder
    // These are used internally by the validation methods
    private static string GetName(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(builder) as string ?? string.Empty;
    }

    private static string GetDescription(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(builder) as string ?? string.Empty;
    }

    private static bool GetAllowParallel(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_allowParallel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(builder) as bool? ?? false;
    }

    private static int GetMaxParallelSteps(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_maxParallelSteps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(builder) as int? ?? 0;
    }

    private static bool GetCacheIntermediates(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_cacheIntermediates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(builder) as bool? ?? false;
    }

    private static int GetExecutionOrder(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_executionOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(builder) as int? ?? 0;
    }

    private static int GetStepCount(this FilterChainBuilder builder)
    {
        var field = typeof(FilterChainBuilder).GetField("_steps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(builder) is List<(Guid FilterId, double EstimatedMs)> steps)
        {
            return steps.Count;
        }
        return 0;
    }
}