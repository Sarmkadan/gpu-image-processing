#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="ProcessingProfile"/> to enhance functionality
    /// </summary>
    public static class ProcessingProfileExtensions
    {
        /// <summary>
        /// Determines whether GPU acceleration is enabled and supported for this profile
        /// </summary>
        /// <param name="profile">The processing profile</param>
        /// <returns>True if GPU acceleration is enabled and should be used; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when profile is null</exception>
        public static bool ShouldUseGPUAcceleration(this ProcessingProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            return profile.UseGPUAcceleration && Environment.GetEnvironmentVariable("DISABLE_GPU") != "true";
        }

        /// <summary>
        /// Gets the effective precision format string based on profile configuration
        /// </summary>
        /// <param name="profile">The processing profile</param>
        /// <returns>The precision format to use ("float32", "float16", or "full")</returns>
        /// <exception cref="ArgumentNullException">Thrown when profile is null</exception>
        public static string GetEffectivePrecisionFormat(this ProcessingProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            return profile.EnablePrecisionReduction ? profile.PrecisionFormat : "full";
        }

        /// <summary>
        /// Calculates the estimated memory usage for processing a batch of images
        /// </summary>
        /// <param name="profile">The processing profile</param>
        /// <param name="imageCount">Number of images in the batch</param>
        /// <param name="imageWidth">Width of each image in pixels</param>
        /// <param name="imageHeight">Height of each image in pixels</param>
        /// <returns>Estimated memory usage in bytes</returns>
        /// <exception cref="ArgumentNullException">Thrown when profile is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when image dimensions are invalid</exception>
        public static long EstimateMemoryUsage(
            this ProcessingProfile profile,
            int imageCount,
            int imageWidth,
            int imageHeight)
        {
            ArgumentNullException.ThrowIfNull(profile);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageCount, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageWidth, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageHeight, 0);

            // Base memory per image: 4 bytes per pixel (RGBA) * width * height
            var bytesPerImage = 4L * imageWidth * imageHeight;

            // Apply precision reduction factor
            var precisionFactor = profile.GetEffectivePrecisionFormat() switch
            {
                "float16" => 0.5f,
                "float32" => 1.0f,
                _ => 1.0f // full precision
            };

            // Apply tiling overhead if enabled
            var tilingFactor = profile.EnableTiling ? 1.2f : 1.0f;

            // Calculate total memory with safety margin
            var totalMemory = (long)(bytesPerImage * precisionFactor * tilingFactor * imageCount * 1.1f);

            return Math.Min(totalMemory, profile.MaxMemoryUsageBytes);
        }

        /// <summary>
        /// Creates a derived profile with adjusted settings for a specific use case
        /// </summary>
        /// <param name="profile">The source processing profile</param>
        /// <param name="settings">Dictionary of settings to override</param>
        /// <returns>A new profile with the specified settings applied</returns>
        /// <exception cref="ArgumentNullException">Thrown when profile or settings is null</exception>
        public static ProcessingProfile WithSettings(
            this ProcessingProfile profile,
            Dictionary<string, float> settings)
        {
            ArgumentNullException.ThrowIfNull(profile);
            ArgumentNullException.ThrowIfNull(settings);

            var result = profile.Clone();

            foreach (var setting in settings)
            {
                result.SetOptimizationSetting(setting.Key, setting.Value);
            }

            result.ModifiedAt = DateTime.UtcNow;
            return result;
        }
    }
}