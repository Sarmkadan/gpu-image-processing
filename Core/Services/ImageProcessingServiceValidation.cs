namespace GpuImageProcessing.Core.Services;

using System;
using System.Collections.Generic;
using System.Reflection;

public static class ImageProcessingServiceValidation
{
    /// <summary>
    /// Validates an instance of <see cref="ImageProcessingService"/> to ensure all required dependencies are properly initialized.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A list of validation problems; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ImageProcessingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required dependencies using reflection
        var requiredFields = new Dictionary<string, string>
        {
            { "_imageRepository", "Image repository" },
            { "_filterRepository", "Filter repository" },
            { "_transformRepository", "Transform repository" },
            { "_profileRepository", "Processing profile repository" },
            { "_deviceService", "Device service" },
            { "_computeShaderPipeline", "Compute shader pipeline" },
            { "_logger", "Logger" },
            { "_filterService", "Filter service" },
            { "_transformService", "Transform service" }
        };

        foreach (var (fieldName, displayName) in requiredFields)
        {
            var fieldInfo = typeof(ImageProcessingService).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo is null)
            {
                problems.Add($"Required field {fieldName} not found in ImageProcessingService.");
                continue;
            }

            if (fieldInfo.GetValue(value) is null)
            {
                problems.Add($"{displayName} cannot be null.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an instance of <see cref="ImageProcessingService"/> is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ImageProcessingService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an instance of <see cref="ImageProcessingService"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the service instance is invalid.</exception>
    public static void EnsureValid(this ImageProcessingService value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid ImageProcessingService instance: {string.Join(", ", problems)}");
        }
    }
}
