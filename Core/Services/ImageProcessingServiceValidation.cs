namespace GpuImageProcessing.Core.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class ImageProcessingServiceValidation
{
    /// <summary>
    /// Validates an instance of <see cref="ImageProcessingService"/> and returns a list of human-readable problems.
    /// </summary>
    public static IReadOnlyList<string> Validate(this ImageProcessingService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // No specific validation rules provided; focus on null/empty checks
        if (value._imageRepository == null) problems.Add("Image repository cannot be null.");
        if (value._filterRepository == null) problems.Add("Filter repository cannot be null.");
        if (value._transformRepository == null) problems.Add("Transform repository cannot be null.");
        if (value._profileRepository == null) problems.Add("Profile repository cannot be null.");
        if (value._deviceService == null) problems.Add("Device service cannot be null.");
        if (value._computeShaderPipeline == null) problems.Add("Compute shader pipeline cannot be null.");
        if (value._logger == null) problems.Add("Logger cannot be null.");
        if (value._filterService == null) problems.Add("Filter service cannot be null.");
        if (value._transformService == null) problems.Add("Transform service cannot be null.");

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
