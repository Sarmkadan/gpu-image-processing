#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Domain;

/// <summary>
/// Represents a GPU device and its capabilities.
/// </summary>
public class GpuDevice
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GpuDeviceType DeviceType { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public long GlobalMemoryBytes { get; set; }
    public long LocalMemoryBytes { get; set; }
    public long MaxAllocatableMemoryBytes { get; set; }
    public int MaxComputeUnits { get; set; }
    public int MaxWorkGroupSize { get; set; }
    public int MaxWorkItemDimensions { get; set; }
    public int[] MaxWorkItemSizes { get; set; } = [];
    public double MaxClockFrequencyMhz { get; set; }
    public bool IsAvailable { get; set; }
    public bool SupportsDoublePrecision { get; set; }
    public bool SupportsHalfPrecision { get; set; }
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, string> Extensions { get; set; } = new();
    public List<string> SupportedFormats { get; set; } = [];
    public int ComputeCapabilityMajor { get; set; }
    public int ComputeCapabilityMinor { get; set; }

    /// <summary>
    /// The native wavefront (warp) size reported by the device via
    /// CL_KERNEL_PREFERRED_WORK_GROUP_SIZE_MULTIPLE or equivalent query.
    /// When zero, the optimizer falls back to vendor-based heuristics.
    /// </summary>
    public int WavefrontSize { get; set; }

    public GpuDevice()
    {
        Id = Guid.NewGuid();
        DetectedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets available global memory in bytes.
    /// </summary>
    public long GetAvailableMemory()
    {
        return Math.Max(0, MaxAllocatableMemoryBytes);
    }

    /// <summary>
    /// Checks if device has sufficient memory for operation.
    /// </summary>
    public bool HasSufficientMemory(long requiredBytes)
    {
        return GetAvailableMemory() >= requiredBytes;
    }

    /// <summary>
    /// Gets memory usage percentage.
    /// </summary>
    public double GetMemoryUsagePercent()
    {
        if (GlobalMemoryBytes == 0)
            return 0.0;

        var used = GlobalMemoryBytes - MaxAllocatableMemoryBytes;
        return (used / (double)GlobalMemoryBytes) * 100.0;
    }

    /// <summary>
    /// Validates device capabilities.
    /// </summary>
    public bool Validate()
    {
        if (!IsAvailable)
            return false;

        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (DeviceType == GpuDeviceType.Unknown)
            return false;

        if (GlobalMemoryBytes <= 0)
            return false;

        if (MaxComputeUnits <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the compute capability as a version string.
    /// </summary>
    public string GetComputeCapability()
    {
        return $"{ComputeCapabilityMajor}.{ComputeCapabilityMinor}";
    }

    /// <summary>
    /// Checks if device supports a specific extension.
    /// </summary>
    public bool SupportsExtension(string extensionName)
    {
        return Extensions.ContainsKey(extensionName);
    }

    /// <summary>
    /// Gets device information summary.
    /// </summary>
    public override string ToString()
    {
        return $"{Name} [{DeviceType}] - {GlobalMemoryBytes / (1024 * 1024)} MB, " +
               $"Compute Units: {MaxComputeUnits}, Max Clock: {MaxClockFrequencyMhz} MHz";
    }

    /// <summary>
    /// Estimates performance score based on device capabilities.
    /// </summary>
    public double CalculatePerformanceScore()
    {
        var score = MaxComputeUnits * (MaxClockFrequencyMhz / 1000.0);

        if (SupportsDoublePrecision)
            score *= 1.2;

        if (GlobalMemoryBytes > 1024L * 1024 * 1024 * 8) // > 8 GB
            score *= 1.3;

        return score;
    }

    /// <summary>
    /// Checks if GPU memory usage is above warning threshold.
    /// </summary>
    public bool IsMemoryWarningRequired()
    {
        return GetMemoryUsagePercent() >= 80.0;
    }
}
