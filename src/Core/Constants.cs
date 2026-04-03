#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class Constants
{
    public const string ApplicationName = "GPU Image Processing";
    public const string ApplicationVersion = "1.0.0";
    public const string Author = "Vladyslav Zaiets";
    public const string WebsiteUrl = "https://sarmkadan.com";

    public static class Processing
    {
        public const int MaxBatchSize = 1000;
        public const int MinImageWidth = 16;
        public const int MaxImageWidth = 16384;
        public const int MinImageHeight = 16;
        public const int MaxImageHeight = 16384;
        public const int DefaultThreadCount = 4;
        public const int MaxConcurrentOperations = 16;
        public const int DefaultTimeout = 300000; // 5 minutes in milliseconds
        public const int ImageBufferAlignment = 256;
    }

    public static class Memory
    {
        public const long MaxMemoryPerImage = 1024L * 1024 * 512; // 512 MB per image
        public const long MaxTotalGpuMemory = 1024L * 1024 * 1024 * 4; // 4 GB total GPU memory
        public const long MemoryWarningThreshold = 1024L * 1024 * 1024 * 2; // 2 GB
        public const int AllocationAlignment = 64;
    }

    public static class Filters
    {
        public const float DefaultBlurRadius = 1.5f;
        public const float DefaultSharpenStrength = 1.0f;
        public const float DefaultBrightnessAdjustment = 0.0f;
        public const float DefaultContrastAdjustment = 1.0f;
        public const float MinKernelRadius = 0.5f;
        public const float MaxKernelRadius = 50.0f;
    }

    public static class Performance
    {
        public const int PerformanceSampleInterval = 100;
        public const int MetricsRetentionPeriodMinutes = 60;
        public const double SlowOperationThresholdMs = 1000.0;
    }

    public static class FileSystem
    {
        public const int MaxFileNameLength = 260;
        public const long MaxFileSizeBytes = 1024L * 1024 * 1024; // 1 GB
        public const string DefaultOutputDirectory = "./output";
        public const string DefaultCacheDirectory = "./cache";
        public const string TempFileSuffix = ".tmp";
    }

    public static class Validation
    {
        public const int MaxRetries = 3;
        public const int RetryDelayMs = 100;
        public const int MinBatchSize = 1;
    }

    public static class ErrorCodes
    {
        public const int GpuInitializationFailed = 1001;
        public const int InsufficientMemory = 1002;
        public const int InvalidImageFormat = 1003;
        public const int FilterNotFound = 1004;
        public const int ProcessingTimeout = 1005;
        public const int InvalidParameters = 1006;
        public const int DeviceNotAvailable = 1007;
        public const int KernelCompilationFailed = 1008;
        public const int MemoryAllocationFailed = 1009;
        public const int BatchProcessingFailed = 1010;
    }
}
