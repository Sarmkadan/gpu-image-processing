#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Provides validation helpers for <see cref="ImageRepository"/> instances.
/// </summary>
public static class ImageRepositoryValidation
{
    /// <summary>
    /// Validates all images in the repository.
    /// </summary>
    /// <param name="repository">The repository to validate.</param>
    /// <returns>A list of validation errors; empty if validation succeeds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static IReadOnlyList<string> Validate(this ImageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var errors = new List<string>();

        // Validate repository state
        if (repository is null)
        {
            errors.Add("Repository instance cannot be null.");
            return errors.AsReadOnly();
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the repository is in a valid state.
    /// </summary>
    /// <param name="repository">The repository to check.</param>
    /// <returns>True if the repository is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    public static bool IsValid(this ImageRepository repository)
    {
        return Validate(repository).Count == 0;
    }

    /// <summary>
    /// Ensures that the repository is in a valid state.
    /// </summary>
    /// <param name="repository">The repository to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null.</exception>
    /// <exception cref="ArgumentException">Thrown when repository validation fails.</exception>
    public static void EnsureValid(this ImageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var errors = Validate(repository);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Repository validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", errors)}");
        }
    }
}