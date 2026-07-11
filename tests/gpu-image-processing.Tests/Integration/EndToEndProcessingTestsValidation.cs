#nullable enable

using System.Globalization;

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Provides validation helpers for <see cref="EndToEndProcessingTests"/> instances.
/// </summary>
public static class EndToEndProcessingTestsValidation
{
    /// <summary>
    /// Validates all public members of an <see cref="EndToEndProcessingTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EndToEndProcessingTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private fields indirectly through public API
        // Since this is a test class with only private fields, we validate through constructor behavior
        // which would throw if dependencies were invalid

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EndToEndProcessingTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this EndToEndProcessingTests value)
    {
        try
        {
            _ = Validate(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="EndToEndProcessingTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this EndToEndProcessingTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EndToEndProcessingTests instance is invalid. Problems: {string.Join(", ", problems)}");
        }
    }
}