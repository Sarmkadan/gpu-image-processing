#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="ComputeShaderPass"/> instances to ensure
/// all required properties are properly initialized before execution.
/// </summary>
public static class ComputeShaderPassValidation
{
    /// <summary>
    /// Validates a <see cref="ComputeShaderPass"/> instance and returns a list of human-readable
    /// problems found. Returns an empty list when the pass is valid.
    /// </summary>
    /// <param name="value">The compute shader pass to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ComputeShaderPass value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id must not be empty.");
        }

        // Validate KernelName
        if (string.IsNullOrWhiteSpace(value.KernelName))
        {
            errors.Add("KernelName must not be null or whitespace.");
        }

        // Validate KernelSource (can be empty string for pre-compiled kernels)
        if (value.KernelSource is null)
        {
            errors.Add("KernelSource must not be null.");
        }

        // Validate PassType
        // ShaderPassType is an enum, so it will always have a valid default value
        // No validation needed beyond null check on the class itself

        // Validate Priority (should be reasonable, not negative)
        if (value.Priority < 0)
        {
            errors.Add("Priority must not be negative.");
        }

        // Validate WorkgroupConfiguration
        if (value.WorkgroupConfiguration is null)
        {
            errors.Add("WorkgroupConfiguration must be set for execution.");
        }
        else
        {
            errors.AddRange(Validate(value.WorkgroupConfiguration));
        }

        // Validate Parameters
        if (value.Parameters is null)
        {
            errors.Add("Parameters dictionary must not be null.");
        }
        else if (value.Parameters.Count == 0)
        {
            errors.Add("Parameters dictionary should not be empty for most use cases.");
        }

        // Validate InputImages
        if (value.InputImages is null)
        {
            errors.Add("InputImages list must not be null.");
        }
        else if (value.InputImages.Count == 0)
        {
            errors.Add("InputImages must contain at least one image.");
        }

        // Validate OutputImage
        if (value.OutputImage is null)
        {
            errors.Add("OutputImage must be set before execution.");
        }

        // Validate CreatedAt (should not be default DateTime)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid UTC timestamp.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt appears to be in the future.");
        }

        return errors;
    }

    /// <summary>
    /// Validates a <see cref="WorkgroupConfiguration"/> instance and returns a list of
    /// human-readable problems found.
    /// </summary>
    /// <param name="configuration">The workgroup configuration to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WorkgroupConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors = new List<string>();

        if (configuration.WorkgroupSizeX <= 0)
        {
            errors.Add("WorkgroupSizeX must be positive.");
        }

        if (configuration.WorkgroupSizeY <= 0)
        {
            errors.Add("WorkgroupSizeY must be positive.");
        }

        if (configuration.WorkgroupSizeZ <= 0)
        {
            errors.Add("WorkgroupSizeZ must be positive.");
        }

        if (configuration.GlobalWorkSizeX <= 0)
        {
            errors.Add("GlobalWorkSizeX must be positive.");
        }

        if (configuration.GlobalWorkSizeY <= 0)
        {
            errors.Add("GlobalWorkSizeY must be positive.");
        }

        if (configuration.GlobalWorkSizeZ <= 0)
        {
            errors.Add("GlobalWorkSizeZ must be positive.");
        }

        if (configuration.LocalMemoryRequiredBytes < 0)
        {
            errors.Add("LocalMemoryRequiredBytes must not be negative.");
        }

        if (configuration.EstimatedOccupancy < 0 || configuration.EstimatedOccupancy > 1)
        {
            errors.Add("EstimatedOccupancy must be in the range [0, 1].");
        }

        if (configuration.OptimizationScore < 0 || configuration.OptimizationScore > 100)
        {
            errors.Add("OptimizationScore must be in the range [0, 100].");
        }

        if (configuration.ComputedAt == default)
        {
            errors.Add("ComputedAt must be set to a valid UTC timestamp.");
        }
        else if (configuration.ComputedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("ComputedAt appears to be in the future.");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ComputeShaderPass"/> is valid.
    /// </summary>
    /// <param name="value">The compute shader pass to check.</param>
    /// <returns><see langword="true"/> if the pass is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ComputeShaderPass value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ComputeShaderPass"/> is valid.
    /// </summary>
    /// <param name="value">The compute shader pass to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the pass is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ComputeShaderPass value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ComputeShaderPass is invalid. Problems:\n{string.Join("\n", errors)}",
            nameof(value));
    }

    /// <summary>
    /// Ensures that the specified <see cref="WorkgroupConfiguration"/> is valid.
    /// </summary>
    /// <param name="configuration">The workgroup configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this WorkgroupConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var errors = Validate(configuration);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"WorkgroupConfiguration is invalid. Problems:\n{string.Join("\n", errors)}",
            nameof(configuration));
    }
}