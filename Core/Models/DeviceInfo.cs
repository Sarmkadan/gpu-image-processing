// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Represents information about a compute device (GPU/CPU) available for processing
    /// </summary>
    public class DeviceInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // GPU, CPU, Accelerator
        public long GlobalMemoryBytes { get; set; }
        public long LocalMemoryBytes { get; set; }
        public int ComputeUnits { get; set; }
        public int MaxWorkGroupSize { get; set; }
        public int MaxWorkItemDimensions { get; set; }
        public string OpenCLVersion { get; set; } = string.Empty;
        public string DriverVersion { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public DateTime DetectedAt { get; set; }
        public Dictionary<string, string> Extensions { get; set; } = new();
        public float ClockFrequencyMHz { get; set; }
        public float EstimatedFlops { get; set; }
        public bool SupportsDoublePrecision { get; set; }
        public string? Extensions_String { get; set; }

        /// <summary>
        /// Initializes a new instance of the DeviceInfo class
        /// </summary>
        public DeviceInfo()
        {
            Id = Guid.NewGuid();
            DetectedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the total available memory in a human-readable format
        /// </summary>
        public string GetFormattedMemory()
        {
            return FormatBytes(GlobalMemoryBytes);
        }

        /// <summary>
        /// Formats bytes into human-readable size
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:F2} {sizes[order]}";
        }

        /// <summary>
        /// Gets the memory usage ratio (0.0 to 1.0)
        /// </summary>
        public float GetMemoryUsageRatio(long usedBytes)
        {
            if (GlobalMemoryBytes <= 0)
                return 0f;

            return Math.Min(1f, (float)usedBytes / GlobalMemoryBytes);
        }

        /// <summary>
        /// Checks if the device has sufficient memory for the given requirement
        /// </summary>
        public bool HasSufficientMemory(long requiredBytes)
        {
            return GlobalMemoryBytes >= requiredBytes;
        }

        /// <summary>
        /// Gets a capability score for this device (0-100)
        /// </summary>
        public int GetCapabilityScore()
        {
            int score = 0;

            // Memory score (max 30)
            if (GlobalMemoryBytes > 0)
            {
                var memoryGb = GlobalMemoryBytes / (1024.0 * 1024.0 * 1024.0);
                score += Math.Min(30, (int)(memoryGb * 3));
            }

            // Compute units score (max 30)
            score += Math.Min(30, ComputeUnits * 2);

            // Clock frequency score (max 20)
            if (ClockFrequencyMHz > 0)
            {
                score += Math.Min(20, (int)(ClockFrequencyMHz / 50));
            }

            // Device type bonus (max 20)
            if (DeviceType == "GPU")
                score += 20;
            else if (DeviceType == "Accelerator")
                score += 10;

            // Double precision bonus (max 10)
            if (SupportsDoublePrecision)
                score += 10;

            return Math.Min(100, score);
        }

        /// <summary>
        /// Gets a summary of device capabilities
        /// </summary>
        public string GetCapabilitiesSummary()
        {
            return $"{Name} ({DeviceType})\n" +
                   $"  Memory: {GetFormattedMemory()}\n" +
                   $"  Compute Units: {ComputeUnits}\n" +
                   $"  Clock: {ClockFrequencyMHz} MHz\n" +
                   $"  OpenCL: {OpenCLVersion}\n" +
                   $"  Capability Score: {GetCapabilityScore()}/100";
        }

        /// <summary>
        /// Checks if device supports a specific extension
        /// </summary>
        public bool SupportsExtension(string extensionName)
        {
            return Extensions.ContainsKey(extensionName) ||
                   (Extensions_String?.Contains(extensionName) ?? false);
        }

        /// <summary>
        /// Adds an extension to the device capabilities
        /// </summary>
        public void AddExtension(string extensionName, string version = "1.0")
        {
            Extensions[extensionName] = version;
        }
    }
}
