// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Main application configuration settings
    /// </summary>
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; } = "GPU Image Processing";
        public string Version { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Development";

        public OpenCLSettings OpenCL { get; set; } = new();
        public ProcessingSettings Processing { get; set; } = new();
        public StorageSettings Storage { get; set; } = new();
        public PerformanceSettings Performance { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    /// <summary>
    /// OpenCL-specific configuration
    /// </summary>
    public class OpenCLSettings
    {
        public bool Enabled { get; set; } = true;
        public string? PreferredPlatformName { get; set; }
        public string? PreferredDeviceName { get; set; }
        public int MaxKernels { get; set; } = 100;
        public bool CompileKernels { get; set; } = true;
        public string KernelCachePath { get; set; } = "./cache/kernels/";
    }

    /// <summary>
    /// Image processing configuration
    /// </summary>
    public class ProcessingSettings
    {
        public int DefaultBatchSize { get; set; } = 10;
        public int MaxParallelOperations { get; set; } = 4;
        public bool UseGPUAcceleration { get; set; } = true;
        public int MaxImageDimension { get; set; } = 8192;
        public long MaxImageFileSizeBytes { get; set; } = 500_000_000; // 500MB
        public int TileSizePixels { get; set; } = 512;
        public bool EnableTiling { get; set; } = false;
        public string[] SupportedFormats { get; set; } = { "jpg", "png", "bmp", "tiff", "webp" };
        public int CompressionQuality { get; set; } = 90;
        public int MaxJobRetries { get; set; } = 3;
    }

    /// <summary>
    /// Storage and output configuration
    /// </summary>
    public class StorageSettings
    {
        public string InputDirectory { get; set; } = "./input/";
        public string OutputDirectory { get; set; } = "./output/";
        public string TempDirectory { get; set; } = "./temp/";
        public string CacheDirectory { get; set; } = "./cache/";
        public bool CacheIntermediateResults { get; set; } = false;
        public long MaxCacheSizeBytes { get; set; } = 5_000_000_000; // 5GB
        public int CacheRetentionDays { get; set; } = 7;
        public bool AutoCreateDirectories { get; set; } = true;
        public bool OverwriteExistingOutput { get; set; } = false;
    }

    /// <summary>
    /// Performance tuning configuration
    /// </summary>
    public class PerformanceSettings
    {
        public bool EnableOptimizations { get; set; } = true;
        public bool EnableMemoryPooling { get; set; } = true;
        public long MaxMemoryUsageBytes { get; set; } = 2_000_000_000; // 2GB
        public bool EnablePrecisionReduction { get; set; } = false;
        public string PrecisionFormat { get; set; } = "float32"; // float32, float16
        public int MaxConcurrentJobs { get; set; } = 4;
        public int MonitoringIntervalMs { get; set; } = 1000;
        public bool EnableProfiling { get; set; } = false;
    }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingSettings
    {
        public string LogLevel { get; set; } = "Information"; // Trace, Debug, Information, Warning, Error, Critical
        public string LogPath { get; set; } = "./logs/";
        public int MaxLogFileSizeBytes { get; set; } = 10_000_000; // 10MB
        public int MaxLogFiles { get; set; } = 10;
        public bool LogToConsole { get; set; } = true;
        public bool LogToFile { get; set; } = true;
        public bool LogPerformanceMetrics { get; set; } = true;
        public bool LogDeviceInfo { get; set; } = true;
        public Dictionary<string, string> CustomLoggers { get; set; } = new();
    }

    /// <summary>
    /// Configuration validation helper
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Validates the entire configuration
        /// </summary>
        public static List<string> ValidateSettings(ApplicationSettings settings)
        {
            var errors = new List<string>();

            if (settings == null)
            {
                errors.Add("Settings cannot be null");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(settings.ApplicationName))
                errors.Add("ApplicationName cannot be empty");

            if (settings.Processing.DefaultBatchSize <= 0)
                errors.Add("DefaultBatchSize must be greater than 0");

            if (settings.Processing.MaxParallelOperations <= 0)
                errors.Add("MaxParallelOperations must be greater than 0");

            if (settings.Processing.MaxImageDimension <= 0)
                errors.Add("MaxImageDimension must be greater than 0");

            if (settings.Processing.MaxImageFileSizeBytes <= 0)
                errors.Add("MaxImageFileSizeBytes must be greater than 0");

            if (settings.Performance.MaxMemoryUsageBytes <= 0)
                errors.Add("MaxMemoryUsageBytes must be greater than 0");

            if (string.IsNullOrWhiteSpace(settings.Storage.InputDirectory))
                errors.Add("InputDirectory cannot be empty");

            if (string.IsNullOrWhiteSpace(settings.Storage.OutputDirectory))
                errors.Add("OutputDirectory cannot be empty");

            return errors;
        }

        /// <summary>
        /// Creates default settings with safe values
        /// </summary>
        public static ApplicationSettings CreateDefaultSettings()
        {
            return new ApplicationSettings
            {
                ApplicationName = "GPU Image Processing",
                Version = "1.0.0",
                Environment = "Development",
                OpenCL = new OpenCLSettings
                {
                    Enabled = true,
                    MaxKernels = 100,
                    CompileKernels = true
                },
                Processing = new ProcessingSettings
                {
                    DefaultBatchSize = 10,
                    MaxParallelOperations = 4,
                    UseGPUAcceleration = true,
                    MaxImageDimension = 8192
                },
                Storage = new StorageSettings
                {
                    InputDirectory = "./input/",
                    OutputDirectory = "./output/",
                    CacheDirectory = "./cache/",
                    AutoCreateDirectories = true
                },
                Performance = new PerformanceSettings
                {
                    EnableOptimizations = true,
                    MaxConcurrentJobs = 4
                },
                Logging = new LoggingSettings
                {
                    LogLevel = "Information",
                    LogToConsole = true,
                    LogToFile = true
                }
            };
        }
    }
}
