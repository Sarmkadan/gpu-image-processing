#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Application settings configuration.
/// </summary>
public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string ApplicationName { get; set; } = AppConstants.ApplicationName;
    public string ApplicationVersion { get; set; } = AppConstants.ApplicationVersion;
    public bool EnableGpuAcceleration { get; set; } = true;
    public int MaxConcurrentOperations { get; set; } = AppConstants.Processing.MaxConcurrentOperations;
    public int OperationTimeoutMs { get; set; } = AppConstants.Processing.DefaultTimeout;
    public string OutputDirectory { get; set; } = AppConstants.FileSystem.DefaultOutputDirectory;
    public string CacheDirectory { get; set; } = AppConstants.FileSystem.DefaultCacheDirectory;
    public bool EnableMetricsCollection { get; set; } = true;
    public int MetricsCollectionIntervalMs { get; set; } = 1000;
    public bool EnablePerformanceLogging { get; set; } = true;
    public int MaxBatchSize { get; set; } = AppConstants.Processing.MaxBatchSize;
    public long MaxMemoryPerImage { get; set; } = AppConstants.Memory.MaxMemoryPerImage;
    public long MaxTotalGpuMemory { get; set; } = AppConstants.Memory.MaxTotalGpuMemory;
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirMinutes { get; set; } = 60;
    public List<string> SupportedImageFormats { get; set; } =
    [
        "jpg", "jpeg", "png", "bmp", "tiff", "webp"
    ];

    /// <summary>
    /// Validates application settings.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(ApplicationName))
            return false;

        if (MaxConcurrentOperations < 1 || MaxConcurrentOperations > 128)
            return false;

        if (OperationTimeoutMs < 100)
            return false;

        if (MaxBatchSize < 1 || MaxBatchSize > AppConstants.Processing.MaxBatchSize)
            return false;

        if (MaxMemoryPerImage <= 0)
            return false;

        if (MaxTotalGpuMemory <= 0)
            return false;

        if (CacheExpirMinutes < 1)
            return false;

        if (!SupportedImageFormats.Any())
            return false;

        return true;
    }

    /// <summary>
    /// Gets a formatted settings summary.
    /// </summary>
    public override string ToString()
    {
        return $@"
=== Application Settings ===
Application: {ApplicationName} {ApplicationVersion}
GPU Acceleration: {EnableGpuAcceleration}
Max Concurrent Operations: {MaxConcurrentOperations}
Operation Timeout: {OperationTimeoutMs}ms
Output Directory: {OutputDirectory}
Cache Directory: {CacheDirectory}
Metrics Collection: {EnableMetricsCollection}
Batch Size Limit: {MaxBatchSize}
Max Memory per Image: {MaxMemoryPerImage / (1024 * 1024)} MB
Supported Formats: {string.Join(", ", SupportedImageFormats)}
";
    }
}
