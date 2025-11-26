#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Services;

/// <summary>
/// Provides validation helpers for <see cref="FilterService"/> service.
/// </summary>
public static class FilterServiceValidation
{
    /// <summary>
    /// Validates a FilterService instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The FilterService to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FilterService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate repository dependency via reflection
        var repositoryField = value.GetType().GetField(
            "_repository",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (repositoryField?.GetValue(value) is not FilterConfigurationRepository)
        {
            problems.Add("Repository dependency is null.");
        }

        // Validate logger dependency via reflection
        var loggerField = value.GetType().GetField(
            "_logger",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (loggerField?.GetValue(value) is not ILogger<FilterService>)
        {
            problems.Add("Logger dependency is null.");
        }

        // Validate filter handlers dictionary via reflection
        var handlersField = value.GetType().GetField(
            "_filterHandlers",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (handlersField?.GetValue(value) is not FrozenDictionary<FilterType, Func<Image, FilterConfiguration, ValueTask>>)
        {
            problems.Add("Filter handlers dictionary is null or not properly initialized.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified FilterService is valid.
    /// </summary>
    /// <param name="value">The FilterService to check.</param>
    /// <returns><see langword="true"/> if the service is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this FilterService value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified FilterService is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The FilterService to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the service is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this FilterService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FilterService validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}