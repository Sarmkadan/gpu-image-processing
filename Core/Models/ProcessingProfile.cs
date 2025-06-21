// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Represents a configuration profile for optimized image processing
    /// </summary>
    public class ProcessingProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UseGPUAcceleration { get; set; } = true;
        public int MaxParallelOperations { get; set; } = 4;
        public int BatchSize { get; set; } = 10;
        public bool CacheIntermediateResults { get; set; } = false;
        public long MaxMemoryUsageBytes { get; set; } = 2_000_000_000; // 2GB
        public bool EnablePrecisionReduction { get; set; } = false;
        public string PrecisionFormat { get; set; } = "float32"; // float32, float16
        public bool EnableTiling { get; set; } = false;
        public int TileSizePixels { get; set; } = 512;
        public Dictionary<string, float> OptimizationSettings { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the ProcessingProfile class
        /// </summary>
        public ProcessingProfile()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a speed-optimized profile for fast processing
        /// </summary>
        public static ProcessingProfile CreateSpeedOptimized()
        {
            return new ProcessingProfile
            {
                Name = "Speed Optimized",
                Description = "Fastest processing with reduced quality",
                UseGPUAcceleration = true,
                MaxParallelOperations = 8,
                BatchSize = 20,
                EnablePrecisionReduction = true,
                PrecisionFormat = "float16",
                IsDefault = false
            };
        }

        /// <summary>
        /// Creates a quality-optimized profile for high-quality results
        /// </summary>
        public static ProcessingProfile CreateQualityOptimized()
        {
            return new ProcessingProfile
            {
                Name = "Quality Optimized",
                Description = "Highest quality processing with reduced speed",
                UseGPUAcceleration = true,
                MaxParallelOperations = 2,
                BatchSize = 5,
                CacheIntermediateResults = true,
                EnablePrecisionReduction = false,
                PrecisionFormat = "float32",
                IsDefault = false
            };
        }

        /// <summary>
        /// Creates a balanced profile for general-purpose use
        /// </summary>
        public static ProcessingProfile CreateBalanced()
        {
            return new ProcessingProfile
            {
                Name = "Balanced",
                Description = "Balanced speed and quality",
                UseGPUAcceleration = true,
                MaxParallelOperations = 4,
                BatchSize = 10,
                EnablePrecisionReduction = false,
                IsDefault = true
            };
        }

        /// <summary>
        /// Gets an optimization setting value
        /// </summary>
        public float GetOptimizationSetting(string key, float defaultValue = 1.0f)
        {
            return OptimizationSettings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Sets an optimization setting
        /// </summary>
        public void SetOptimizationSetting(string key, float value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Setting key cannot be empty", nameof(key));

            OptimizationSettings[key] = value;
        }

        /// <summary>
        /// Validates the profile configuration
        /// </summary>
        public bool IsValid()
        {
            return MaxParallelOperations > 0 &&
                   BatchSize > 0 &&
                   MaxMemoryUsageBytes > 0 &&
                   TileSizePixels > 0 &&
                   !string.IsNullOrEmpty(Name);
        }

        /// <summary>
        /// Gets the effective batch size based on memory constraints
        /// </summary>
        public int GetEffectiveBatchSize(long estimatedMemoryPerImage)
        {
            if (estimatedMemoryPerImage <= 0)
                return BatchSize;

            var maxBatchByMemory = (int)(MaxMemoryUsageBytes / estimatedMemoryPerImage);
            return Math.Min(BatchSize, Math.Max(1, maxBatchByMemory));
        }

        /// <summary>
        /// Gets a summary of the profile configuration
        /// </summary>
        public string GetProfileSummary()
        {
            var precision = EnablePrecisionReduction ? PrecisionFormat : "full";
            return $"{Name}\n" +
                   $"  GPU Acceleration: {UseGPUAcceleration}\n" +
                   $"  Max Parallel Ops: {MaxParallelOperations}\n" +
                   $"  Batch Size: {BatchSize}\n" +
                   $"  Precision: {precision}\n" +
                   $"  Max Memory: {MaxMemoryUsageBytes / (1024.0 * 1024.0):F2} MB";
        }

        /// <summary>
        /// Clones this profile with a new ID
        /// </summary>
        public ProcessingProfile Clone()
        {
            return new ProcessingProfile
            {
                Id = Guid.NewGuid(),
                Name = $"{Name} (Copy)",
                Description = this.Description,
                UseGPUAcceleration = this.UseGPUAcceleration,
                MaxParallelOperations = this.MaxParallelOperations,
                BatchSize = this.BatchSize,
                CacheIntermediateResults = this.CacheIntermediateResults,
                MaxMemoryUsageBytes = this.MaxMemoryUsageBytes,
                EnablePrecisionReduction = this.EnablePrecisionReduction,
                PrecisionFormat = this.PrecisionFormat,
                EnableTiling = this.EnableTiling,
                TileSizePixels = this.TileSizePixels,
                OptimizationSettings = new Dictionary<string, float>(this.OptimizationSettings),
                IsDefault = false
            };
        }
    }
}
