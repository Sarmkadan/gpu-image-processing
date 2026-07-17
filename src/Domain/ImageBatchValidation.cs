#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="ImageBatch"/> instances.
/// </summary>
public static class ImageBatchValidation
{
    /// <summary>
    /// Validates an <see cref="ImageBatch"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The image batch to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the batch is valid.</returns>
/// <exception cref="ArgumentNullException">Thrown if <see cref="AppConstants"/> configuration is null.</exception>
    public static IReadOnlyList<string> Validate(this ImageBatch value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name is required and cannot be empty or whitespace.");
        }
        else if (value.Name.Length > AppConstants.FileSystem.MaxFileNameLength)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Name exceeds maximum length of {0} characters.",
                AppConstants.FileSystem.MaxFileNameLength));
        }

        if (string.IsNullOrWhiteSpace(value.OutputDirectory))
        {
            problems.Add("OutputDirectory is required and cannot be empty or whitespace.");
        }
        else if (value.OutputDirectory.Length > AppConstants.FileSystem.MaxFileNameLength)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "OutputDirectory exceeds maximum length of {0} characters.",
                AppConstants.FileSystem.MaxFileNameLength));
        }

        // Validate description (optional but has length constraint if provided)
        if (!string.IsNullOrEmpty(value.Description) && value.Description.Length > 10000)
        {
            problems.Add("Description exceeds maximum length of 10000 characters.");
        }

        // Validate GUID collections
        if (value.ImageIds is null)
        {
            problems.Add("ImageIds collection cannot be null.");
        }
        else if (value.ImageIds.Count > AppConstants.Processing.MaxBatchSize)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "ImageIds contains {0} items, exceeding maximum batch size of {1}.",
                value.ImageIds.Count,
                AppConstants.Processing.MaxBatchSize));
        }
        else if (value.ImageIds.Count > 0)
        {
            // Check for duplicate or default GUIDs in ImageIds
            var seenImageIds = new HashSet<Guid>(value.ImageIds.Count);
            foreach (var imageId in value.ImageIds)
            {
                if (imageId == Guid.Empty)
                {
                    problems.Add("ImageIds contains a default (empty) GUID.");
                    break;
                }

                if (!seenImageIds.Add(imageId))
                {
                    problems.Add("ImageIds contains duplicate GUID values.");
                    break;
                }
            }
        }

        if (value.FilterIds is null)
        {
            problems.Add("FilterIds collection cannot be null.");
        }
        else if (value.FilterIds.Count > AppConstants.Processing.MaxBatchSize)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "FilterIds contains {0} items, exceeding maximum batch size of {1}.",
                value.FilterIds.Count,
                AppConstants.Processing.MaxBatchSize));
        }
        else if (value.FilterIds.Count > 0)
        {
            // Check for duplicate or default GUIDs in FilterIds
            var seenFilterIds = new HashSet<Guid>(value.FilterIds.Count);
            foreach (var filterId in value.FilterIds)
            {
                if (filterId == Guid.Empty)
                {
                    problems.Add("FilterIds contains a default (empty) GUID.");
                    break;
                }

                if (!seenFilterIds.Add(filterId))
                {
                    problems.Add("FilterIds contains duplicate GUID values.");
                    break;
                }
            }
        }

        // Validate status
        if (!Enum.IsDefined(typeof(Core.ProcessingStatus), value.Status))
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Status has invalid value: {0}.",
                value.Status));
        }

        // Validate dates
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt.Kind != DateTimeKind.Utc)
        {
            problems.Add("CreatedAt must be in UTC format.");
        }

        if (value.StartedAt != default)
        {
            if (value.StartedAt.Kind != DateTimeKind.Utc)
            {
                problems.Add("StartedAt must be in UTC format.");
            }
            else if (value.StartedAt < value.CreatedAt)
            {
                problems.Add("StartedAt cannot be earlier than CreatedAt.");
            }
        }

        if (value.CompletedAt != default)
        {
            if (value.CompletedAt.Kind != DateTimeKind.Utc)
            {
                problems.Add("CompletedAt must be in UTC format.");
            }
            else if (value.CompletedAt < value.CreatedAt)
            {
                problems.Add("CompletedAt cannot be earlier than CreatedAt.");
            }
            else if (value.StartedAt != default && value.CompletedAt < value.StartedAt)
            {
                problems.Add("CompletedAt cannot be earlier than StartedAt.");
            }
        }

        // Validate counts
        if (value.TotalImages < 0)
        {
            problems.Add("TotalImages cannot be negative.");
        }
        else if (value.TotalImages > AppConstants.Processing.MaxBatchSize)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "TotalImages ({0}) exceeds maximum batch size of {1}.",
                value.TotalImages,
                AppConstants.Processing.MaxBatchSize));
        }

        if (value.ProcessedImages < 0)
        {
            problems.Add("ProcessedImages cannot be negative.");
        }

        if (value.FailedImages < 0)
        {
            problems.Add("FailedImages cannot be negative.");
        }

        if (value.ProcessedImages + value.FailedImages > value.TotalImages)
        {
            problems.Add(
                "Sum of ProcessedImages and FailedImages cannot exceed TotalImages.");
        }

        // Validate BatchOptions dictionary
        if (value.BatchOptions is null)
        {
            problems.Add("BatchOptions dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.BatchOptions)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    problems.Add("BatchOptions contains an entry with null or empty key.");
                    break;
                }

                if (kvp.Key.Length > 256)
                {
                    problems.Add("BatchOptions key exceeds maximum length of 256 characters.");
                    break;
                }
            }
        }

        // Validate OutputDirectory is a valid path (basic check)
        if (!string.IsNullOrEmpty(value.OutputDirectory))
        {
            try
            {
                // Simple path validation - check for invalid characters
                var invalidChars = Path.GetInvalidPathChars();
                if (value.OutputDirectory.IndexOfAny(invalidChars) >= 0)
                {
                    problems.Add("OutputDirectory contains invalid path characters.");
                }
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
            {
                problems.Add("OutputDirectory format is invalid: " + ex.Message);
            }
        }

        // Validate PerformanceMetrics
        if (value.Metrics is null)
        {
            problems.Add("Metrics cannot be null.");
        }
        else
        {
            if (value.Metrics.RecordedAt == default)
            {
                problems.Add("Metrics.RecordedAt must be set to a valid DateTime.");
            }
            else if (value.Metrics.RecordedAt.Kind != DateTimeKind.Utc)
            {
                problems.Add("Metrics.RecordedAt must be in UTC format.");
            }

            if (value.Metrics.CpuUsagePercent < 0 || value.Metrics.CpuUsagePercent > 100)
            {
                problems.Add(
                    "Metrics.CpuUsagePercent must be between 0 and 100 inclusive.");
            }

            if (value.Metrics.GpuUtilizationPercent < 0 || value.Metrics.GpuUtilizationPercent > 100)
            {
                problems.Add(
                    "Metrics.GpuUtilizationPercent must be between 0 and 100 inclusive.");
            }

            if (value.Metrics.MemoryUsedBytes < 0)
            {
                problems.Add("Metrics.MemoryUsedBytes cannot be negative.");
            }
            else if (value.Metrics.MemoryUsedBytes > AppConstants.Memory.MaxTotalGpuMemory)
            {
                problems.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "Metrics.MemoryUsedBytes ({0} bytes) exceeds maximum GPU memory ({1} bytes).",
                    value.Metrics.MemoryUsedBytes,
                    AppConstants.Memory.MaxTotalGpuMemory));
            }

            if (value.Metrics.GpuMemoryUsedBytes < 0)
            {
                problems.Add("Metrics.GpuMemoryUsedBytes cannot be negative.");
            }
            else if (value.Metrics.GpuMemoryUsedBytes > AppConstants.Memory.MaxTotalGpuMemory)
            {
                problems.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "Metrics.GpuMemoryUsedBytes ({0} bytes) exceeds maximum GPU memory ({1} bytes).",
                    value.Metrics.GpuMemoryUsedBytes,
                    AppConstants.Memory.MaxTotalGpuMemory));
            }

            if (value.Metrics.AverageExecutionTimeMs < 0)
            {
                problems.Add("Metrics.AverageExecutionTimeMs cannot be negative.");
            }

            if (value.Metrics.MaxExecutionTimeMs < 0)
            {
                problems.Add("Metrics.MaxExecutionTimeMs cannot be negative.");
            }

            if (value.Metrics.MinExecutionTimeMs < 0)
            {
                problems.Add("Metrics.MinExecutionTimeMs cannot be negative.");
            }

            if (value.Metrics.ImagePixelsProcessedPerSecond < 0)
            {
                problems.Add("Metrics.ImagePixelsProcessedPerSecond cannot be negative.");
            }

            if (value.Metrics.TotalOperationsCount < 0)
            {
                problems.Add("Metrics.TotalOperationsCount cannot be negative.");
            }

            if (value.Metrics.FailedOperationsCount < 0)
            {
                problems.Add("Metrics.FailedOperationsCount cannot be negative.");
            }

            if (value.Metrics.FailedOperationsCount > value.Metrics.TotalOperationsCount)
            {
                problems.Add(
                    "Metrics.FailedOperationsCount cannot exceed Metrics.TotalOperationsCount.");
            }

            if (value.Metrics.ThroughputMegabytesPerSecond < 0)
            {
                problems.Add("Metrics.ThroughputMegabytesPerSecond cannot be negative.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an <see cref="ImageBatch"/> instance is valid.
    /// </summary>
    /// <param name="value">The image batch to check.</param>
    /// <returns>True if the batch is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ImageBatch value)
    {
    ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ImageBatch"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The image batch to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown if the batch is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ImageBatch value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException("ImageBatch validation failed:\n" + string.Join("\n", problems), nameof(value));
        }
        }
    }
