using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Events;

namespace GpuImageProcessing.BackgroundWorkers;

/// <summary>
/// Provides validation helpers for <see cref="JobProcessingWorker"/> instances.
/// </summary>
public static class JobProcessingWorkerValidation
{
    /// <summary>
    /// Validates the specified <see cref="JobProcessingWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this JobProcessingWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.GetName()))
        {
            errors.Add("Worker name cannot be null or whitespace.");
        }

        if (value.IsRunning)
        {
            errors.Add("Worker cannot be running during validation.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the constructor dependencies of a <see cref="JobProcessingWorker"/> instance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="batchProcessingService">The batch processing service.</param>
    /// <param name="imageProcessingService">The image processing service.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public static IReadOnlyList<string> ValidateDependencies(
        ILogger<JobProcessingWorker> logger,
        BatchProcessingService batchProcessingService,
        ImageProcessingService imageProcessingService,
        IEventPublisher eventPublisher)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(batchProcessingService);
        ArgumentNullException.ThrowIfNull(imageProcessingService);
        ArgumentNullException.ThrowIfNull(eventPublisher);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="JobProcessingWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this JobProcessingWorker value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="JobProcessingWorker"/> instance is valid.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this JobProcessingWorker value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"JobProcessingWorker is invalid. {string.Join(" ", errors)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures that the constructor dependencies are valid.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="batchProcessingService">The batch processing service.</param>
    /// <param name="imageProcessingService">The image processing service.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public static void EnsureDependenciesValid(
        ILogger<JobProcessingWorker> logger,
        BatchProcessingService batchProcessingService,
        ImageProcessingService imageProcessingService,
        IEventPublisher eventPublisher)
    {
        _ = ValidateDependencies(logger, batchProcessingService, imageProcessingService, eventPublisher);
    }
}