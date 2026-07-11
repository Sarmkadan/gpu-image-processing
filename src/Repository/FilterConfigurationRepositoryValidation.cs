#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Provides validation helpers for <see cref="FilterConfigurationRepository"/> instances.
/// </summary>
public static class FilterConfigurationRepositoryValidation
{
    /// <summary>
    /// Validates the repository and all its filter configurations.
    /// </summary>
    /// <param name="value">The repository to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FilterConfigurationRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate repository state
        lock (value.GetLockObject())
        {
            var allFilters = value.GetStorage();

            if (allFilters == null)
            {
                problems.Add("Repository storage is null.");
                return problems.AsReadOnly();
            }

            // Validate each filter configuration
            foreach (var filter in allFilters)
            {
                if (filter == null)
                {
                    problems.Add("Repository contains a null filter configuration.");
                    continue;
                }

                ValidateFilterConfiguration(filter, problems);
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the repository and all its filter configurations are valid.
    /// </summary>
    /// <param name="value">The repository to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this FilterConfigurationRepository value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the repository and all its filter configurations are valid.
    /// </summary>
    /// <param name="value">The repository to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid(this FilterConfigurationRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"FilterConfigurationRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    private static void ValidateFilterConfiguration(FilterConfiguration filter, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(problems);

        // Validate filter ID
        if (filter.Id == Guid.Empty)
        {
            problems.Add($"Filter '{filter.Name}' has an empty Guid ID.");
        }

        // Validate filter name
        if (string.IsNullOrWhiteSpace(filter.Name))
        {
            problems.Add($"Filter with ID '{filter.Id}' has a null, empty, or whitespace name.");
        }
        else if (filter.Name.Length > 256)
        {
            problems.Add($"Filter '{filter.Name}' has a name longer than 256 characters (length: {filter.Name.Length}).");
        }

        // Validate filter type
        if (filter.FilterType == FilterType.None)
        {
            problems.Add($"Filter '{filter.Name}' has FilterType.None, which is not a valid filter type.");
        }

        // Validate description
        if (filter.Description != null && filter.Description.Length > 1024)
        {
            problems.Add($"Filter '{filter.Name}' has a description longer than 1024 characters (length: {filter.Description.Length}).");
        }

        // Validate parameters
        if (filter.Parameters == null)
        {
            problems.Add($"Filter '{filter.Name}' has null Parameters dictionary.");
        }
        else if (filter.Parameters.Count > 100)
        {
            problems.Add($"Filter '{filter.Name}' has more than 100 parameters (count: {filter.Parameters.Count}).");
        }

        // Validate parameter types
        if (filter.ParameterTypes == null)
        {
            problems.Add($"Filter '{filter.Name}' has null ParameterTypes dictionary.");
        }
        else if (filter.ParameterTypes.Count > 100)
        {
            problems.Add($"Filter '{filter.Name}' has more than 100 parameter types (count: {filter.ParameterTypes.Count}).");
        }
        else if (filter.Parameters != null)
        {
            // Check that all parameters have corresponding types
            foreach (var paramKey in filter.Parameters.Keys)
            {
                if (!filter.ParameterTypes.ContainsKey(paramKey))
                {
                    problems.Add($"Filter '{filter.Name}' has parameter '{paramKey}' without a corresponding type in ParameterTypes.");
                }
            }
        }

        // Validate IsActive
        // No specific validation needed beyond null check

        // Validate Priority
        if (filter.Priority < 0 || filter.Priority > 1000)
        {
            problems.Add($"Filter '{filter.Name}' has a priority outside the valid range [0-1000] (value: {filter.Priority}).");
        }

        // Validate timestamps
        if (filter.CreatedAt == default)
        {
            problems.Add($"Filter '{filter.Name}' has a default CreatedAt date.");
        }
        else if (filter.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add($"Filter '{filter.Name}' has a CreatedAt date in the future (value: {filter.CreatedAt:O}).");
        }

        if (filter.ModifiedAt == default)
        {
            problems.Add($"Filter '{filter.Name}' has a default ModifiedAt date.");
        }
        else if (filter.ModifiedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add($"Filter '{filter.Name}' has a ModifiedAt date in the future (value: {filter.ModifiedAt:O}).");
        }

        // Validate MaxThreadsPerBlock
        if (filter.MaxThreadsPerBlock < 32 || filter.MaxThreadsPerBlock > 1024)
        {
            problems.Add($"Filter '{filter.Name}' has MaxThreadsPerBlock outside the valid range [32-1024] (value: {filter.MaxThreadsPerBlock}).");
        }

        // Validate KernelCode
        if (filter.KernelCode != null && filter.KernelCode.Length > 10240)
        {
            problems.Add($"Filter '{filter.Name}' has a KernelCode longer than 10240 characters (length: {filter.KernelCode.Length}).");
        }

        // Validate ConvolutionKernel if present
        if (filter.ConvolutionKernel != null)
        {
            if (filter.ConvolutionKernel.Length == 0)
            {
                problems.Add($"Filter '{filter.Name}' has an empty ConvolutionKernel array.");
            }
            else if (filter.ConvolutionKernel.Length > 225) // 15x15 max
            {
                problems.Add($"Filter '{filter.Name}' has a ConvolutionKernel with more than 225 elements (count: {filter.ConvolutionKernel.Length}).");
            }
            else
            {
                int len = filter.ConvolutionKernel.Length;
                int side = (int)Math.Round(Math.Sqrt(len));
                if (side * side != len)
                {
                    problems.Add($"Filter '{filter.Name}' has a ConvolutionKernel that is not a perfect square (length: {len}).");
                }
                else if (side < 3 || side > 15)
                {
                    problems.Add($"Filter '{filter.Name}' has a ConvolutionKernel with invalid side length {side} (must be 3-15 and odd).");
                }
                else if (side % 2 == 0)
                {
                    problems.Add($"Filter '{filter.Name}' has a ConvolutionKernel with even side length {side} (must be odd).");
                }
            }
        }

        // Validate filter-specific parameters using the domain validation
        if (!filter.Validate())
        {
            problems.Add($"Filter '{filter.Name}' failed domain-level validation.");
        }
    }

    private static object GetLockObject(this FilterConfigurationRepository repository)
    {
        var lockField = typeof(FilterConfigurationRepository).GetField(
            "_lockObject",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return lockField?.GetValue(repository) ?? throw new InvalidOperationException("Repository _lockObject field not found.");
    }

    private static List<FilterConfiguration> GetStorage(this FilterConfigurationRepository repository)
    {
        var storageField = typeof(FilterConfigurationRepository).GetField(
            "_storage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return storageField?.GetValue(repository) as List<FilterConfiguration> ?? throw new InvalidOperationException("Repository _storage field not found.");
    }
}
