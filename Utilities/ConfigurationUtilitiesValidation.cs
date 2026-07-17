#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="ConfigurationUtilities"/> configuration values.
    /// </summary>
    public sealed class ConfigurationUtilitiesValidation
    {
        /// <summary>
        /// Validates all configuration values from ConfigurationUtilities.
        /// Returns a list of human-readable problems found in the configuration.
        /// </summary>
        /// <returns>List of validation problems; empty if all valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if problems is null</exception>
        public static IReadOnlyList<string> Validate(List<string>? problems = null)
        {
            problems ??= new List<string>();

            ArgumentNullException.ThrowIfNull(problems);

            ValidateEnvironment(problems);
            ValidateDirectoryPaths(problems);
            ValidateNumericRanges(problems);
            ValidateBooleanFlags(problems);
            ValidateProfile(problems);
            ValidateLogLevel(problems);

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if all configuration values are valid.
        /// </summary>
        /// <returns>True if all configuration values are valid; false otherwise</returns>
        public static bool IsValid()
        {
            return Validate().Count == 0;
        }

        /// <summary>
        /// Ensures all configuration values are valid, throwing an exception with detailed problems if not.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if any configuration value is invalid with detailed error message</exception>
        public static void EnsureValid()
        {
            var problems = new List<string>();
            Validate(problems);

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }

        private static void ValidateEnvironment(List<string> problems)
        {
            string environment = ConfigurationUtilities.GetEnvironment();

            if (string.IsNullOrWhiteSpace(environment))
            {
                problems.Add("Environment variable 'ENVIRONMENT' is null, empty, or whitespace. Expected values: Development, Production, Testing, etc.");
            }
            else if (environment.ToLowerInvariant() is not ("development" or "production" or "testing"))
            {
                problems.Add($"Environment '{environment}' is not a recognized value. Expected: Development, Production, Testing.");
            }
        }

        private static void ValidateDirectoryPaths(List<string> problems)
        {
            ValidateDirectoryPath(
                ConfigurationUtilities.GetDataDirectory(),
                nameof(ConfigurationUtilities.GetDataDirectory),
                problems);

            ValidateDirectoryPath(
                ConfigurationUtilities.GetLogDirectory(),
                nameof(ConfigurationUtilities.GetLogDirectory),
                problems);

            ValidateDirectoryPath(
                ConfigurationUtilities.GetTempDirectory(),
                nameof(ConfigurationUtilities.GetTempDirectory),
                problems);
        }

        private static void ValidateDirectoryPath(string path, string methodName, List<string> problems)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                problems.Add($"{methodName} returned null, empty, or whitespace directory path.");
                return;
            }

            try
            {
                // Test if we can access the directory
                bool exists = System.IO.Directory.Exists(path);
                if (!exists)
                {
                    problems.Add($"Directory '{path}' does not exist and could not be created by {methodName}.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"{methodName} returned path '{path}' but directory access failed: {ex.Message}");
            }
        }

        private static void ValidateNumericRanges(List<string> problems)
        {
            int maxConcurrent = ConfigurationUtilities.GetMaxConcurrentOperations();
            if (maxConcurrent <= 0)
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetMaxConcurrentOperations)} returned {maxConcurrent}, but must be greater than 0.");
            }

            int timeoutSeconds = ConfigurationUtilities.GetOperationTimeoutSeconds();
            if (timeoutSeconds <= 0)
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetOperationTimeoutSeconds)} returned {timeoutSeconds}, but must be greater than 0 seconds.");
            }
            else if (timeoutSeconds > 86400) // 24 hours
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetOperationTimeoutSeconds)} returned {timeoutSeconds} seconds, which exceeds reasonable maximum of 86400 seconds (24 hours).");
            }

            int preferredDeviceId = ConfigurationUtilities.GetPreferredDeviceId();
            if (preferredDeviceId < -1)
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetPreferredDeviceId)} returned {preferredDeviceId}, but must be -1 (auto-select) or >= 0.");
            }

            int cacheSizeMb = ConfigurationUtilities.GetCacheSizeMb();
            if (cacheSizeMb <= 0)
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetCacheSizeMb)} returned {cacheSizeMb}, but must be greater than 0 MB.");
            }
            else if (cacheSizeMb > 16384) // 16 GB
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetCacheSizeMb)} returned {cacheSizeMb} MB, which exceeds reasonable maximum of 16384 MB (16 GB).");
            }
        }

        private static void ValidateBooleanFlags(List<string> problems)
        {
            // These methods return boolean values that are always valid by design
            // No validation needed beyond null checks which are handled by the methods themselves
            _ = ConfigurationUtilities.GetEnablePerformanceLogging();
            _ = ConfigurationUtilities.GetEnableDebugLogging();
            _ = ConfigurationUtilities.GetUseGpuAcceleration();
        }

        private static void ValidateProfile(List<string> problems)
        {
            string profile = ConfigurationUtilities.GetDefaultProfile();

            if (string.IsNullOrWhiteSpace(profile))
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetDefaultProfile)} returned null, empty, or whitespace profile name.");
                return;
            }

            string lowerProfile = profile.ToLowerInvariant();
            if (lowerProfile is not ("fast" or "balanced" or "quality"))
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetDefaultProfile)} returned '{profile}'. Expected: 'fast', 'balanced', or 'quality'.");
            }
        }

        private static void ValidateLogLevel(List<string> problems)
        {
            string logLevel = ConfigurationUtilities.GetLogLevel();

            if (string.IsNullOrWhiteSpace(logLevel))
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetLogLevel)} returned null, empty, or whitespace log level.");
                return;
            }

            bool isValid = logLevel.ToLowerInvariant() switch
            {
                "trace" or "debug" or "information" or "warning" or "error" or "critical" or "none" => true,
                _ => false
            };

            if (!isValid)
            {
                problems.Add($"{nameof(ConfigurationUtilities.GetLogLevel)} returned '{logLevel}'. Expected one of: Trace, Debug, Information, Warning, Error, Critical, None.");
            }
        }
    }
}