#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Provides validation helpers for <see cref="EndToEndProcessingTests"/> instances.
/// </summary>
public static class EndToEndProcessingTestsValidation
{
    /// <summary>
    /// Validates that an <see cref="EndToEndProcessingTests"/> instance is properly initialized.
    /// Since the constructor initializes all dependencies, this simply verifies the instance exists.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null check is present")]
    public static IReadOnlyList<string> Validate(this EndToEndProcessingTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EndToEndProcessingTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid([NotNullWhen(true)] this EndToEndProcessingTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var problems = Validate(value);
            return problems.Count == 0;
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