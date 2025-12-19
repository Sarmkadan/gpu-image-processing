#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Provides validation helpers for <see cref="ApplicationSettings"/>.
    /// </summary>
    public static class ApplicationSettingsValidation
    {
        /// <summary>
        /// Validates the specified application settings.
        /// </summary>
        /// <param name="value">The settings to validate.</param>
        /// <returns>A list of validation errors, if any.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ApplicationSettings value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value.ApplicationName))
                errors.Add("ApplicationName cannot be null or whitespace.");

            if (string.IsNullOrWhiteSpace(value.Version))
                errors.Add("Version cannot be null or whitespace.");

            if (string.IsNullOrWhiteSpace(value.Environment))
                errors.Add("Environment cannot be null or whitespace.");

            errors.AddRange(ValidateOpenCLSettings(value.OpenCL));
            errors.AddRange(ValidateProcessingSettings(value.Processing));
            errors.AddRange(ValidateStorageSettings(value.Storage));
            errors.AddRange(ValidatePerformanceSettings(value.Performance));
            errors.AddRange(ValidateLoggingSettings(value.Logging));

            return errors;
        }

        /// <summary>
        /// Determines whether the specified application settings are valid.
        /// </summary>
        /// <param name="value">The settings to validate.</param>
        /// <returns><c>true</c> if the settings are valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ApplicationSettings value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified application settings are valid.
        /// </summary>
        /// <param name="value">The settings to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails with all validation errors.</exception>
        public static void EnsureValid(this ApplicationSettings value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors), nameof(value));
            }
        }

        private static IEnumerable<string> ValidateOpenCLSettings(OpenCLSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var errors = new List<string>();

            if (settings.MaxKernels <= 0)
                errors.Add("OpenCLSettings.MaxKernels must be greater than 0.");

            if (string.IsNullOrWhiteSpace(settings.KernelCachePath))
                errors.Add("OpenCLSettings.KernelCachePath cannot be null or whitespace.");

            return errors;
        }

        private static IEnumerable<string> ValidateProcessingSettings(ProcessingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var errors = new List<string>();

            if (settings.DefaultBatchSize <= 0)
                errors.Add("ProcessingSettings.DefaultBatchSize must be greater than 0.");

            if (settings.MaxParallelOperations <= 0)
                errors.Add("ProcessingSettings.MaxParallelOperations must be greater than 0.");

            if (settings.MaxImageDimension <= 0)
                errors.Add("ProcessingSettings.MaxImageDimension must be greater than 0.");

            if (settings.MaxImageFileSizeBytes < 0)
                errors.Add("ProcessingSettings.MaxImageFileSizeBytes must be non-negative.");

            if (settings.TileSizePixels <= 0)
                errors.Add("ProcessingSettings.TileSizePixels must be greater than 0.");

            return errors;
        }

        private static IEnumerable<string> ValidateStorageSettings(StorageSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(settings.InputDirectory))
                errors.Add("StorageSettings.InputDirectory cannot be null or whitespace.");

            if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
                errors.Add("StorageSettings.OutputDirectory cannot be null or whitespace.");

            if (string.IsNullOrWhiteSpace(settings.CacheDirectory))
                errors.Add("StorageSettings.CacheDirectory cannot be null or whitespace.");

            if (settings.MaxCacheSizeBytes < 0)
                errors.Add("StorageSettings.MaxCacheSizeBytes must be non-negative.");

            if (settings.CacheRetentionDays <= 0)
                errors.Add("StorageSettings.CacheRetentionDays must be greater than 0.");

            return errors;
        }

        private static IEnumerable<string> ValidatePerformanceSettings(PerformanceSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var errors = new List<string>();

            if (settings.MaxMemoryUsageBytes < 0)
                errors.Add("PerformanceSettings.MaxMemoryUsageBytes must be non-negative.");

            if (settings.MaxConcurrentJobs <= 0)
                errors.Add("PerformanceSettings.MaxConcurrentJobs must be greater than 0.");

            if (settings.MonitoringIntervalMs <= 0)
                errors.Add("PerformanceSettings.MonitoringIntervalMs must be greater than 0.");

            return errors;
        }

        private static IEnumerable<string> ValidateLoggingSettings(LoggingSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(settings.LogPath))
                errors.Add("LoggingSettings.LogPath cannot be null or whitespace.");

            if (settings.MaxLogFileSizeBytes <= 0)
                errors.Add("LoggingSettings.MaxLogFileSizeBytes must be greater than 0.");

            if (settings.MaxLogFiles <= 0)
                errors.Add("LoggingSettings.MaxLogFiles must be greater than 0.");

            return errors;
        }
    }
}
