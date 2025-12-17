namespace GpuImageProcessing.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ImageProcessingServiceValidation
{
    /// <summary>
    /// Validates an instance of <see cref="ImageProcessingService"/> and returns a list of human-readable problems.
    /// </summary>
    public static IReadOnlyList<string> Validate(this ImageProcessingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        var requiredFieldNames = new[] { "_imageRepository", "_filterRepository", "_transformRepository", 
                                         "_profileRepository", "_deviceService", "_computeShaderPipeline", 
                                         "_logger", "_filterService", "_transformService" };

        foreach (var fieldName in requiredFieldNames)
        {
            var fieldInfo = typeof(ImageProcessingService).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo?.GetValue(value) == null)
            {
                problems.Add($"{fieldName} cannot be null.");
            }
        }

        // Add checks for numeric properties if they exist and have valid ranges
        // For example:
        // var propertyInfo = typeof(ImageProcessingService).GetProperty("SomeProperty");
        // if (propertyInfo != null && (int)propertyInfo.GetValue(value) < 0)
        // {
        //     problems.Add("SomeProperty must be non-negative.");
        // }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an instance of <see cref="ImageProcessingService"/> is valid.
    /// </summary>
    public static bool IsValid(this ImageProcessingService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an instance of <see cref="ImageProcessingService"/> is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    public static void EnsureValid(this ImageProcessingService value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid ImageProcessingService instance: {string.Join(", ", problems)}");
        }
    }
}
