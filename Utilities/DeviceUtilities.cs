#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utilities for GPU device discovery, capability detection, and resource management.
    /// Provides helpers for querying device information and making optimal device selections.
    /// </summary>
    public static class DeviceUtilities
    {
        /// <summary>
        /// Scores a GPU device based on capabilities and availability
        /// </summary>
        public static float ScoreGpuDevice(
            long globalMemoryBytes,
            int computeUnits,
            int maxClockFrequency,
            float utilizationPercent = 0)
        {
            var memoryScore = Math.Min(globalMemoryBytes / (8L * 1024 * 1024 * 1024), 100);
            var computeScore = Math.Min(computeUnits / 64f, 100);
            var clockScore = Math.Min(maxClockFrequency / 3000f, 100);
            var utilizationScore = 100 - utilizationPercent;

            // Weighted average: memory 40%, compute units 30%, clock 20%, utilization 10%
            return (float)((memoryScore * 0.4) + (computeScore * 0.3) + (clockScore * 0.2) + (utilizationScore * 0.1));
        }

        /// <summary>
        /// Calculates theoretical peak performance in GFLOPS
        /// </summary>
        public static float CalculatePeakPerformance(int computeUnits, int clockFrequencyMhz, int computePerCore = 64)
        {
            // Simplification: 64 FLOPs per core per clock
            return (computeUnits * clockFrequencyMhz * computePerCore) / 1000f;
        }

        /// <summary>
        /// Estimates bandwidth utilization for a given data transfer
        /// </summary>
        public static float EstimateBandwidthUtilization(long dataSizeBytes, float bandwidthGbps, float operationTimeMs)
        {
            var dataTransferTimeMs = (dataSizeBytes / (bandwidthGbps * 1024 * 1024 * 1024)) * 1000;
            return (float)(dataTransferTimeMs / operationTimeMs);
        }

        /// <summary>
        /// Checks if device supports required compute capability
        /// </summary>
        public static bool SupportsComputeCapability(string deviceComputeCapability, string requiredCapability)
        {
            // Parse semantic versioning (e.g., "3.0" for CC 3.0)
            if (!Version.TryParse(deviceComputeCapability, out var deviceCc) ||
                !Version.TryParse(requiredCapability, out var requiredCc))
                return false;

            return deviceCc >= requiredCc;
        }

        /// <summary>
        /// Validates device memory is sufficient for operation
        /// </summary>
        public static bool ValidateMemorySufficiency(
            long availableMemoryBytes,
            long requiredMemoryBytes,
            float safetyMargin = 0.2f)
        {
            var safeAvailableMemory = (long)(availableMemoryBytes * (1 - safetyMargin));
            return safeAvailableMemory >= requiredMemoryBytes;
        }

        /// <summary>
        /// Detects memory pressure on device and recommends actions
        /// </summary>
        public static MemoryPressureAnalysis AnalyzeMemoryPressure(
            long totalMemoryBytes,
            long usedMemoryBytes,
            long minFreeMemoryBytes = 100 * 1024 * 1024)
        {
            var freeMemory = totalMemoryBytes - usedMemoryBytes;
            var usagePercent = (usedMemoryBytes / (float)totalMemoryBytes) * 100;

            var recommendation = usagePercent switch
            {
                < 50 => MemoryPressureLevel.Low,
                < 75 => MemoryPressureLevel.Moderate,
                < 90 => MemoryPressureLevel.High,
                _ => MemoryPressureLevel.Critical
            };

            return new MemoryPressureAnalysis
            {
                TotalMemoryBytes = totalMemoryBytes,
                UsedMemoryBytes = usedMemoryBytes,
                FreeMemoryBytes = freeMemory,
                UsagePercent = usagePercent,
                PressureLevel = recommendation,
                IsMemoryConstrained = freeMemory < minFreeMemoryBytes,
                RecommendedBatchSize = CalculateRecommendedBatchSize(freeMemory)
            };
        }

        /// <summary>
        /// Calculates safe batch size based on available memory
        /// </summary>
        public static int CalculateRecommendedBatchSize(long availableMemoryBytes)
        {
            // Assume 4MB per item as a conservative estimate
            var bytesPerItem = 4 * 1024 * 1024;
            var maxBatchSize = (int)(availableMemoryBytes / bytesPerItem);

            // Cap between 1 and 256 for practical reasons
            return Math.Max(1, Math.Min(maxBatchSize, 256));
        }

        /// <summary>
        /// Identifies device bottlenecks based on performance metrics
        /// </summary>
        public static List<string> IdentifyBottlenecks(
            float computeUtilization,
            float memoryBandwidthUtilization,
            float memoryUtilization)
        {
            var bottlenecks = new List<string>();

            if (computeUtilization < 40 && memoryBandwidthUtilization > 70)
                bottlenecks.Add("Memory bandwidth limited");

            if (computeUtilization > 90)
                bottlenecks.Add("Compute limited");

            if (memoryUtilization > 95)
                bottlenecks.Add("Memory capacity limited");

            if (bottlenecks.Count == 0)
                bottlenecks.Add("Well balanced");

            return bottlenecks;
        }

        /// <summary>
        /// Generates device capability summary string
        /// </summary>
        public static string GenerateCapabilitySummary(
            string deviceName,
            long globalMemoryBytes,
            int computeUnits,
            int maxClockFrequency,
            string computeCapability)
        {
            var peakPerf = CalculatePeakPerformance(computeUnits, maxClockFrequency);
            var memorySize = DataConversionUtilities.FormatFileSize(globalMemoryBytes);

            return $"{deviceName} " +
                   $"({memorySize} VRAM, " +
                   $"{computeUnits} CUs, " +
                   $"{maxClockFrequency} MHz, " +
                   $"CC {computeCapability}, " +
                   $"Peak: {peakPerf:F1} GFLOPS)";
        }
    }

    public enum MemoryPressureLevel
    {
        Low,
        Moderate,
        High,
        Critical
    }

    public class MemoryPressureAnalysis
    {
        public long TotalMemoryBytes { get; set; }
        public long UsedMemoryBytes { get; set; }
        public long FreeMemoryBytes { get; set; }
        public float UsagePercent { get; set; }
        public MemoryPressureLevel PressureLevel { get; set; }
        public bool IsMemoryConstrained { get; set; }
        public int RecommendedBatchSize { get; set; }
    }
}
