#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Validation helpers for <see cref="ProcessingException"/>.
/// </summary>
public static class ProcessingExceptionValidation
{
    /// <summary>
    /// Validates the provided <see cref="ProcessingException"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="ProcessingException"/> instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ProcessingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.ImagePath))
        {
            problems.Add("ImagePath is null or empty.");
        }

        if (string.IsNullOrEmpty(value.FilterName))
        {
            problems.Add("FilterName is null or empty.");
        }

        if (value.AttemptNumber.HasValue && value.AttemptNumber.Value < 1)
        {
            problems.Add("AttemptNumber must be greater than 0.");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the provided <see cref="ProcessingException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ProcessingException"/> instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ProcessingException value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures the provided <see cref="ProcessingException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ProcessingException"/> instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this ProcessingException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException($"The ProcessingException instance is invalid: {string.Join(", ", problems)}", nameof(value));
        }
    }
}