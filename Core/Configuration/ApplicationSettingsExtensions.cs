using System;
using GpuImageProcessing.Core.Configuration;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Provides extension methods for <see cref="ApplicationSettings"/>.
    /// </summary>
    public static class ApplicationSettingsExtensions
    {
        /// <summary>
        /// Determines if GPU acceleration is effectively enabled based on OpenCL and processing configuration.
        /// </summary>
        /// <param name="settings">The application settings instance.</param>
        /// <returns>True if GPU acceleration is enabled and supported; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        public static bool IsGpuAccelerationEnabled(this ApplicationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            return settings.OpenCL.Enabled && settings.Processing.UseGPUAcceleration;
        }

        /// <summary>
        /// Checks if the application environment is set to Production.
        /// </summary>
        /// <param name="settings">The application settings instance.</param>
        /// <returns>True if the environment is Production; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        public static bool IsProductionEnvironment(this ApplicationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            return string.Equals(settings.Environment, "Production", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates if the given file size is within the allowed limits for processing.
        /// </summary>
        /// <param name="settings">The application settings instance.</param>
        /// <param name="fileSizeBytes">The size of the file in bytes.</param>
        /// <returns>True if the file size is within limits; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileSizeBytes"/> is negative.</exception>
        public static bool IsFileWithinLimits(this ApplicationSettings settings, long fileSizeBytes)
        {
            ArgumentNullException.ThrowIfNull(settings);
            if (fileSizeBytes < 0)
            {
                throw new ArgumentException("File size cannot be negative", nameof(fileSizeBytes));
            }
            return fileSizeBytes <= settings.Processing.MaxImageFileSizeBytes;
        }
    }
}
