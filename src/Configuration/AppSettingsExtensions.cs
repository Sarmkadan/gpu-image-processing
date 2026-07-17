using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GpuImageProcessing.Configuration
{
    /// <summary>
    /// Extension methods that provide convenient helpers for <see cref="AppSettings"/>.
    /// </summary>
    public static class AppSettingsExtensions
    {
        /// <summary>
        /// Gets the operation timeout as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <returns>A <see cref="TimeSpan"/> representing <see cref="AppSettings.OperationTimeoutMs"/> milliseconds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
        public static TimeSpan GetOperationTimeout(this AppSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            return TimeSpan.FromMilliseconds(settings.OperationTimeoutMs);
        }

        /// <summary>
        /// Builds an absolute cache directory path that incorporates the application name.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <returns>The combined cache directory path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="AppSettings.CacheDirectory"/> or <see cref="AppSettings.ApplicationName"/> is <c>null</c> or empty.
        /// </exception>
        public static string GetCachePath(this AppSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentException.ThrowIfNullOrEmpty(settings.CacheDirectory, nameof(settings.CacheDirectory));
            ArgumentException.ThrowIfNullOrEmpty(settings.ApplicationName, nameof(settings.ApplicationName));
            return Path.Combine(settings.CacheDirectory, settings.ApplicationName);
        }

        /// <summary>
        /// Returns a read‑only view of the supported image formats.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of supported image format strings.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetSupportedImageFormats(this AppSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
                    return settings.SupportedImageFormats?.AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
        }

        /// <summary>
        /// Validates the configuration and throws an <see cref="ArgumentException"/> if any required
        /// property is missing or out of range.
        /// </summary>
        /// <param name="settings">The application settings to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more required settings are invalid.
        /// </exception>
        public static void EnsureValid(this AppSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            // Application name and version are essential for logging and diagnostics.
            ArgumentException.ThrowIfNullOrEmpty(settings.ApplicationName, nameof(settings.ApplicationName));
            ArgumentException.ThrowIfNullOrEmpty(settings.ApplicationVersion, nameof(settings.ApplicationVersion));

            // Directories must be provided.
            ArgumentException.ThrowIfNullOrEmpty(settings.OutputDirectory, nameof(settings.OutputDirectory));
            ArgumentException.ThrowIfNullOrEmpty(settings.CacheDirectory, nameof(settings.CacheDirectory));

            // Numeric ranges.
            if (settings.MaxConcurrentOperations <= 0)
                throw new ArgumentException("MaxConcurrentOperations must be greater than zero.", nameof(settings.MaxConcurrentOperations));

            if (settings.OperationTimeoutMs <= 0)
                throw new ArgumentException("OperationTimeoutMs must be greater than zero.", nameof(settings.OperationTimeoutMs));

            if (settings.MaxBatchSize <= 0)
                throw new ArgumentException("MaxBatchSize must be greater than zero.", nameof(settings.MaxBatchSize));

            if (settings.MaxMemoryPerImage <= 0)
                throw new ArgumentException("MaxMemoryPerImage must be greater than zero.", nameof(settings.MaxMemoryPerImage));

            if (settings.MaxTotalGpuMemory <= 0)
                throw new ArgumentException("MaxTotalGpuMemory must be greater than zero.", nameof(settings.MaxTotalGpuMemory));

            if (settings.EnableCaching && settings.CacheExpirMinutes <= 0)
                throw new ArgumentException("CacheExpirMinutes must be greater than zero when caching is enabled.", nameof(settings.CacheExpirMinutes));

            if (settings.EnableMetricsCollection && settings.MetricsCollectionIntervalMs <= 0)
                throw new ArgumentException("MetricsCollectionIntervalMs must be greater than zero when metrics collection is enabled.", nameof(settings.MetricsCollectionIntervalMs));

            // Supported formats list should contain at least one entry.
            var formats = settings.SupportedImageFormats;
            if (formats == null || formats.Count == 0)
                throw new ArgumentException("At least one supported image format must be specified.", nameof(settings.SupportedImageFormats));
        }
    }
}
