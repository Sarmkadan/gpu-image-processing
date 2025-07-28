#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GpuImageProcessing.Fallback;

/// <summary>
/// Provides validation helpers for <see cref="CpuImageProcessor"/> instances.
/// </summary>
public static class CpuImageProcessorValidation
{
    /// <summary>
    /// Validates the specified <see cref="CpuImageProcessor"/> instance.
    /// </summary>
    /// <param name="value">The processor instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CpuImageProcessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // CpuImageProcessor has no public state to validate beyond null check
        // All validation is handled by argument validation in its methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CpuImageProcessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The processor instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CpuImageProcessor? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="CpuImageProcessor"/> instance is valid.
    /// </summary>
    /// <param name="value">The processor instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the processor is invalid.</exception>
    public static void EnsureValid(this CpuImageProcessor? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"CpuImageProcessor is invalid. Problems: {string.Join(", ", problems)}");
    }
}