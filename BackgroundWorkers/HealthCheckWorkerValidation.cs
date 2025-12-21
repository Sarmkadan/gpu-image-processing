using System;
using System.Collections.Generic;

namespace GpuImageProcessing.BackgroundWorkers;

/// <summary>
/// Provides validation helpers for <see cref="HealthCheckWorker"/> instances.
/// </summary>
public static class HealthCheckWorkerValidation
{
    /// <summary>
    /// Validates the specified <see cref="HealthCheckWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this HealthCheckWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(value.GetName()))
        {
            errors.Add("Worker name cannot be null or empty.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="HealthCheckWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this HealthCheckWorker value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="HealthCheckWorker"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance has validation problems, listing all issues.</exception>
    public static void EnsureValid(this HealthCheckWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                string.Join(" ", errors),
                nameof(value));
        }
    }
}