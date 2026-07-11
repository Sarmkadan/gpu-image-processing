using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="AppSettings"/> configuration.
/// </summary>
public static class AppSettingsValidation
{
    /// <summary>
    /// Validates the application settings and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AppSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ApplicationName))
        {
            problems.Add("ApplicationName cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.ApplicationVersion))
        {
            problems.Add("ApplicationVersion cannot be null or whitespace.");
        }

        if (value.MaxConcurrentOperations < 1)
        {
            problems.Add("MaxConcurrentOperations must be at least 1.");
        }

        if (value.OperationTimeoutMs < 100)
        {
            problems.Add("OperationTimeoutMs must be at least 100 milliseconds.");
        }

        if (string.IsNullOrWhiteSpace(value.OutputDirectory))
        {
            problems.Add("OutputDirectory cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.CacheDirectory))
        {
            problems.Add("CacheDirectory cannot be null or whitespace.");
        }

        if (value.MetricsCollectionIntervalMs < 100)
        {
            problems.Add("MetricsCollectionIntervalMs must be at least 100 milliseconds.");
        }

        if (value.MaxBatchSize < 1)
        {
            problems.Add("MaxBatchSize must be at least 1.");
        }

        if (value.MaxMemoryPerImage <= 0)
        {
            problems.Add("MaxMemoryPerImage must be greater than 0.");
        }

        if (value.MaxTotalGpuMemory <= 0)
        {
            problems.Add("MaxTotalGpuMemory must be greater than 0.");
        }

        if (value.CacheExpirMinutes < 1)
        {
            problems.Add("CacheExpirMinutes must be at least 1 minute.");
        }

        if (value.SupportedImageFormats is null || value.SupportedImageFormats.Count == 0)
        {
            problems.Add("SupportedImageFormats must contain at least one format.");
        }
        else
        {
            var invalidFormats = value.SupportedImageFormats
                .Where(f => string.IsNullOrWhiteSpace(f))
                .ToList();

            if (invalidFormats.Count > 0)
            {
                problems.Add($"SupportedImageFormats contains {invalidFormats.Count} null or whitespace entries.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the application settings are valid.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AppSettings value)
    {
        return AppSettingsValidation.Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the application settings are valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if any are found.
    /// </summary>
    /// <param name="value">The settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all problems.</exception>
    public static void EnsureValid(this AppSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = AppSettingsValidation.Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AppSettings validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}