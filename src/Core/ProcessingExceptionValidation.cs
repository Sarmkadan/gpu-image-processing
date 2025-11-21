#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Validation helpers for ProcessingException.
/// </summary>
public static class ProcessingExceptionValidation
{
    /// <summary>
    /// Validates the provided ProcessingException instance.
    /// </summary>
    /// <param name="value">The ProcessingException instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this ProcessingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.ImagePath == null)
        {
            problems.Add("ImagePath is null.");
        }

        if (value.FilterName == null)
        {
            problems.Add("FilterName is null.");
        }

        if (value.AttemptNumber.HasValue && (value.AttemptNumber.Value < 1 || value.AttemptNumber.Value > int.MaxValue))
        {
            problems.Add("AttemptNumber is out of range.");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the provided ProcessingException instance is valid.
    /// </summary>
    /// <param name="value">The ProcessingException instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    public static bool IsValid(this ProcessingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures the provided ProcessingException instance is valid.
    /// </summary>
    /// <param name="value">The ProcessingException instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this ProcessingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Any())
        {
            throw new ArgumentException($"The following problems were found: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
