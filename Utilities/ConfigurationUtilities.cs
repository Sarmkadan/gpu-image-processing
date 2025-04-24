// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utility methods for configuration management and application settings.
    /// Provides configuration parsing, defaults, and environment-specific overrides.
    /// </summary>
    public static class ConfigurationUtilities
    {
        /// <summary>
        /// Loads configuration from environment variable with fallback to default value.
        /// </summary>
        public static string GetConfigValue(string key, string defaultValue = null)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        /// <summary>
        /// Loads integer configuration from environment variable with validation.
        /// </summary>
        public static int GetConfigInteger(string key, int defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
                return defaultValue;

            return result;
        }

        /// <summary>
        /// Loads boolean configuration from environment variable.
        /// Recognizes common boolean values: true, 1, yes, on (case-insensitive).
        /// </summary>
        public static bool GetConfigBoolean(string key, bool defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            value = value.ToLower();
            return value is "true" or "1" or "yes" or "on";
        }

        /// <summary>
        /// Loads double configuration from environment variable with validation.
        /// </summary>
        public static double GetConfigDouble(string key, double defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value) || !double.TryParse(value, out double result))
                return defaultValue;

            return result;
        }

        /// <summary>
        /// Gets application environment (Development, Production, Testing, etc.).
        /// Defaults to "Production" if not specified.
        /// </summary>
        public static string GetEnvironment()
        {
            return GetConfigValue("ENVIRONMENT", "Production");
        }

        /// <summary>
        /// Checks if running in development mode.
        /// </summary>
        public static bool IsDevelopment()
        {
            return GetEnvironment().Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if running in production mode.
        /// </summary>
        public static bool IsProduction()
        {
            return GetEnvironment().Equals("Production", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the application data directory for storing temporary and cached data.
        /// Creates directory if it doesn't exist.
        /// </summary>
        public static string GetDataDirectory()
        {
            string dataDir = GetConfigValue("DATA_DIRECTORY", "./data");

            try
            {
                System.IO.Directory.CreateDirectory(dataDir);
                return dataDir;
            }
            catch
            {
                return "./data";
            }
        }

        /// <summary>
        /// Gets the log output directory.
        /// Creates directory if it doesn't exist.
        /// </summary>
        public static string GetLogDirectory()
        {
            string logDir = GetConfigValue("LOG_DIRECTORY", "./logs");

            try
            {
                System.IO.Directory.CreateDirectory(logDir);
                return logDir;
            }
            catch
            {
                return "./logs";
            }
        }

        /// <summary>
        /// Gets the temporary files directory for processing.
        /// Creates directory if it doesn't exist.
        /// </summary>
        public static string GetTempDirectory()
        {
            string tempDir = GetConfigValue("TEMP_DIRECTORY", System.IO.Path.GetTempPath());

            try
            {
                System.IO.Directory.CreateDirectory(tempDir);
                return tempDir;
            }
            catch
            {
                return System.IO.Path.GetTempPath();
            }
        }

        /// <summary>
        /// Gets maximum concurrent operations allowed.
        /// Defaults to number of processor cores.
        /// </summary>
        public static int GetMaxConcurrentOperations()
        {
            int defaultValue = Environment.ProcessorCount;
            return GetConfigInteger("MAX_CONCURRENT_OPS", defaultValue);
        }

        /// <summary>
        /// Gets operation timeout in seconds.
        /// </summary>
        public static int GetOperationTimeoutSeconds()
        {
            return GetConfigInteger("OPERATION_TIMEOUT", 300); // 5 minutes default
        }

        /// <summary>
        /// Gets the OpenCL device ID to use (-1 for auto-select).
        /// </summary>
        public static int GetPreferredDeviceId()
        {
            return GetConfigInteger("OPENCL_DEVICE_ID", -1);
        }

        /// <summary>
        /// Gets the logging level.
        /// </summary>
        public static string GetLogLevel()
        {
            return GetConfigValue("LOG_LEVEL", IsDevelopment() ? "Debug" : "Information");
        }

        /// <summary>
        /// Gets whether to enable detailed performance logging.
        /// </summary>
        public static bool GetEnablePerformanceLogging()
        {
            return GetConfigBoolean("ENABLE_PERF_LOGGING", IsDevelopment());
        }

        /// <summary>
        /// Gets whether to enable detailed debug logging.
        /// </summary>
        public static bool GetEnableDebugLogging()
        {
            return GetConfigBoolean("ENABLE_DEBUG_LOGGING", IsDevelopment());
        }

        /// <summary>
        /// Gets memory cache size in MB.
        /// </summary>
        public static int GetCacheSizeMb()
        {
            return GetConfigInteger("CACHE_SIZE_MB", 512);
        }

        /// <summary>
        /// Gets whether to use GPU acceleration.
        /// </summary>
        public static bool GetUseGpuAcceleration()
        {
            return GetConfigBoolean("USE_GPU", true);
        }

        /// <summary>
        /// Gets the default image processing profile (fast, balanced, quality).
        /// </summary>
        public static string GetDefaultProfile()
        {
            return GetConfigValue("DEFAULT_PROFILE", "balanced");
        }

        /// <summary>
        /// Builds a configuration dictionary from IConfiguration instance.
        /// </summary>
        public static Dictionary<string, string> BuildConfigurationDictionary(IConfiguration configuration)
        {
            var dict = new Dictionary<string, string>();

            if (configuration == null)
                return dict;

            foreach (var child in configuration.GetChildren())
            {
                if (child.Value != null)
                {
                    dict[child.Key] = child.Value;
                }
                else
                {
                    var childDict = BuildConfigurationDictionary(child);
                    foreach (var kvp in childDict)
                    {
                        dict[$"{child.Key}:{kvp.Key}"] = kvp.Value;
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Validates required configuration values are present.
        /// </summary>
        public static (bool isValid, List<string> missingKeys) ValidateRequiredConfiguration(params string[] requiredKeys)
        {
            var missingKeys = new List<string>();

            foreach (var key in requiredKeys)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    missingKeys.Add(key);
                }
            }

            return (missingKeys.Count == 0, missingKeys);
        }
    }
}
