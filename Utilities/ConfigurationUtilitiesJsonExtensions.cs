#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for ConfigurationUtilities state.
    /// </summary>
    public static class ConfigurationUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the current configuration state to a JSON string.
        /// </summary>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the current configuration state.</returns>
        public static string ToJson(bool indented = false)
        {
            var configState = new ConfigurationState
            {
                Environment = ConfigurationUtilities.GetEnvironment(),
                DataDirectory = ConfigurationUtilities.GetDataDirectory(),
                LogDirectory = ConfigurationUtilities.GetLogDirectory(),
                TempDirectory = ConfigurationUtilities.GetTempDirectory(),
                MaxConcurrentOperations = ConfigurationUtilities.GetMaxConcurrentOperations(),
                OperationTimeoutSeconds = ConfigurationUtilities.GetOperationTimeoutSeconds(),
                PreferredDeviceId = ConfigurationUtilities.GetPreferredDeviceId(),
                LogLevel = ConfigurationUtilities.GetLogLevel(),
                EnablePerformanceLogging = ConfigurationUtilities.GetEnablePerformanceLogging(),
                EnableDebugLogging = ConfigurationUtilities.GetEnableDebugLogging(),
                CacheSizeMb = ConfigurationUtilities.GetCacheSizeMb(),
                UseGpuAcceleration = ConfigurationUtilities.GetUseGpuAcceleration(),
                DefaultProfile = ConfigurationUtilities.GetDefaultProfile()
            };

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(configState, options);
        }

        /// <summary>
        /// Deserializes a JSON string to update the current configuration state.
        /// </summary>
        /// <param name="json">The JSON string containing configuration to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static void FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            var configState = JsonSerializer.Deserialize<ConfigurationState>(json, _jsonSerializerOptions)
                ?? throw new JsonException("Configuration state cannot be null");

            // Apply the configuration state by setting environment variables
            if (configState.Environment is not null)
                Environment.SetEnvironmentVariable("ENVIRONMENT", configState.Environment);

            if (configState.DataDirectory is not null)
                Environment.SetEnvironmentVariable("DATA_DIRECTORY", configState.DataDirectory);

            if (configState.LogDirectory is not null)
                Environment.SetEnvironmentVariable("LOG_DIRECTORY", configState.LogDirectory);

            if (configState.TempDirectory is not null)
                Environment.SetEnvironmentVariable("TEMP_DIRECTORY", configState.TempDirectory);

            if (configState.MaxConcurrentOperations >= 0)
                Environment.SetEnvironmentVariable("MAX_CONCURRENT_OPS", configState.MaxConcurrentOperations.ToString());

            if (configState.OperationTimeoutSeconds >= 0)
                Environment.SetEnvironmentVariable("OPERATION_TIMEOUT", configState.OperationTimeoutSeconds.ToString());

            if (configState.PreferredDeviceId >= -1)
                Environment.SetEnvironmentVariable("OPENCL_DEVICE_ID", configState.PreferredDeviceId.ToString());

            if (configState.LogLevel is not null)
                Environment.SetEnvironmentVariable("LOG_LEVEL", configState.LogLevel);

            Environment.SetEnvironmentVariable("ENABLE_PERF_LOGGING", configState.EnablePerformanceLogging ? "true" : "false");
            Environment.SetEnvironmentVariable("ENABLE_DEBUG_LOGGING", configState.EnableDebugLogging ? "true" : "false");

            if (configState.CacheSizeMb >= 0)
                Environment.SetEnvironmentVariable("CACHE_SIZE_MB", configState.CacheSizeMb.ToString());

            Environment.SetEnvironmentVariable("USE_GPU", configState.UseGpuAcceleration ? "true" : "false");

            if (configState.DefaultProfile is not null)
                Environment.SetEnvironmentVariable("DEFAULT_PROFILE", configState.DefaultProfile);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to update the current configuration state.
        /// </summary>
        /// <param name="json">The JSON string containing configuration to apply.</param>
        /// <param name="success">Receives true if deserialization succeeded; otherwise, false.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="success"/> is null.</exception>
        public static bool TryFromJson(string json, out bool success)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                var configState = JsonSerializer.Deserialize<ConfigurationState>(json, _jsonSerializerOptions);
                if (configState == null)
                {
                    success = false;
                    return false;
                }

                // Apply the configuration state
                if (configState.Environment is not null)
                    Environment.SetEnvironmentVariable("ENVIRONMENT", configState.Environment);

                if (configState.DataDirectory is not null)
                    Environment.SetEnvironmentVariable("DATA_DIRECTORY", configState.DataDirectory);

                if (configState.LogDirectory is not null)
                    Environment.SetEnvironmentVariable("LOG_DIRECTORY", configState.LogDirectory);

                if (configState.TempDirectory is not null)
                    Environment.SetEnvironmentVariable("TEMP_DIRECTORY", configState.TempDirectory);

                if (configState.MaxConcurrentOperations >= 0)
                    Environment.SetEnvironmentVariable("MAX_CONCURRENT_OPS", configState.MaxConcurrentOperations.ToString());

                if (configState.OperationTimeoutSeconds >= 0)
                    Environment.SetEnvironmentVariable("OPERATION_TIMEOUT", configState.OperationTimeoutSeconds.ToString());

                if (configState.PreferredDeviceId >= -1)
                    Environment.SetEnvironmentVariable("OPENCL_DEVICE_ID", configState.PreferredDeviceId.ToString());

                if (configState.LogLevel is not null)
                    Environment.SetEnvironmentVariable("LOG_LEVEL", configState.LogLevel);

                Environment.SetEnvironmentVariable("ENABLE_PERF_LOGGING", configState.EnablePerformanceLogging ? "true" : "false");
                Environment.SetEnvironmentVariable("ENABLE_DEBUG_LOGGING", configState.EnableDebugLogging ? "true" : "false");

                if (configState.CacheSizeMb >= 0)
                    Environment.SetEnvironmentVariable("CACHE_SIZE_MB", configState.CacheSizeMb.ToString());

                Environment.SetEnvironmentVariable("USE_GPU", configState.UseGpuAcceleration ? "true" : "false");

                if (configState.DefaultProfile is not null)
                    Environment.SetEnvironmentVariable("DEFAULT_PROFILE", configState.DefaultProfile);

                success = true;
                return true;
            }
            catch (JsonException)
            {
                success = false;
                return false;
            }
        }

        /// <summary>
        /// Represents the serializable configuration state.
        /// </summary>
        private sealed class ConfigurationState
        {
            /// <summary>Gets or sets the environment name.</summary>
            public string? Environment { get; set; }

            /// <summary>Gets or sets the data directory path.</summary>
            public string? DataDirectory { get; set; }

            /// <summary>Gets or sets the log directory path.</summary>
            public string? LogDirectory { get; set; }

            /// <summary>Gets or sets the temporary directory path.</summary>
            public string? TempDirectory { get; set; }

            /// <summary>Gets or sets the maximum concurrent operations count. Defaults to -1.</summary>
            public int MaxConcurrentOperations { get; set; } = -1;

            /// <summary>Gets or sets the operation timeout in seconds. Defaults to -1.</summary>
            public int OperationTimeoutSeconds { get; set; } = -1;

            /// <summary>Gets or sets the preferred device ID. Defaults to -2.</summary>
            public int PreferredDeviceId { get; set; } = -2;

            /// <summary>Gets or sets the log level.</summary>
            public string? LogLevel { get; set; }

            /// <summary>Gets or sets whether performance logging is enabled.</summary>
            public bool EnablePerformanceLogging { get; set; }

            /// <summary>Gets or sets whether debug logging is enabled.</summary>
            public bool EnableDebugLogging { get; set; }

            /// <summary>Gets or sets the cache size in MB. Defaults to -1.</summary>
            public int CacheSizeMb { get; set; } = -1;

            /// <summary>Gets or sets whether GPU acceleration is enabled.</summary>
            public bool UseGpuAcceleration { get; set; }

            /// <summary>Gets or sets the default profile name.</summary>
            public string? DefaultProfile { get; set; }
        }
    }
}