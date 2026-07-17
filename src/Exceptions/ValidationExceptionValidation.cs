#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ValidationException"/> instances.
/// </summary>
public static class ValidationExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="ValidationException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if the exception is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EntityName
        if (string.IsNullOrWhiteSpace(value.EntityName))
        {
            problems.Add("EntityName must not be null, empty, or whitespace.");
        }

        // Validate ValidationErrors
        if (value.ValidationErrors == null)
        {
            problems.Add("ValidationErrors must not be null.");
        }
        else if (value.ValidationErrors.Count == 0)
        {
            problems.Add("ValidationErrors must not be empty.");
        }
        else
        {
            // Check for null keys or values in the dictionary
            foreach (var kvp in value.ValidationErrors)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("ValidationErrors contains an entry with null, empty, or whitespace key.");
                }

                if (string.IsNullOrWhiteSpace(kvp.Value))
                {
                    problems.Add("ValidationErrors contains an entry with null, empty, or whitespace error message.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ValidationException"/> instance is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if the exception is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ValidationException? value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ValidationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid with details about validation failures.</exception>
    public static void EnsureValid(this ValidationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationException is not valid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}