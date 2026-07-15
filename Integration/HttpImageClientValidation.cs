#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides validation helpers for <see cref="HttpImageClient"/> instances.
    /// </summary>
    public static class HttpImageClientValidation
    {
        /// <summary>
        /// Validates an <see cref="HttpImageClient"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this HttpImageClient value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // HttpImageClient doesn't expose internal state for validation
            // All validation is done through method parameters at call time

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether an <see cref="HttpImageClient"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this HttpImageClient value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that an <see cref="HttpImageClient"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing validation errors.</exception>
        public static void EnsureValid(this HttpImageClient value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    "HttpImageClient instance is not valid. Validation errors: " + string.Join(" ", errors),
                    nameof(value));
            }
        }

        /// <summary>
        /// Validates a URL string for HTTP image operations.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateUrl(string? url)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(url))
            {
                errors.Add("URL cannot be null, empty, or whitespace.");
                return errors.AsReadOnly();
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            {
                errors.Add($"URL '{url}' is not a valid absolute URI.");
            }
            else if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                errors.Add($"URL scheme must be http or https, but was '{uriResult.Scheme}'.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates a file path for image operations.
        /// </summary>
        /// <param name="filePath">The file path to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateFilePath(string? filePath)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errors.Add("File path cannot be null, empty, or whitespace.");
                return errors.AsReadOnly();
            }

            if (filePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
            {
                errors.Add($"File path '{filePath}' contains invalid characters.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates that a file exists at the specified path.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>A list of validation errors; empty if file exists.</returns>
        public static IReadOnlyList<string> ValidateFileExists(string filePath)
        {
            var errors = new List<string>();

            if (!System.IO.File.Exists(filePath))
            {
                errors.Add($"File does not exist at path: '{filePath}'.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates an output directory path.
        /// </summary>
        /// <param name="outputPath">The output path to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateOutputDirectory(string? outputPath)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                errors.Add("Output path cannot be null, empty, or whitespace.");
                return errors.AsReadOnly();
            }

            var directoryName = System.IO.Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directoryName) && !System.IO.Directory.Exists(directoryName))
            {
                errors.Add($"Output directory does not exist and will need to be created: '{directoryName}'.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates HTTP status code values.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateHttpStatusCode(int? httpStatusCode)
        {
            var errors = new List<string>();

            if (httpStatusCode.HasValue)
            {
                const int minValidStatusCode = 100;
                const int maxValidStatusCode = 599;

                if (httpStatusCode < minValidStatusCode || httpStatusCode > maxValidStatusCode)
                {
                    errors.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "HTTP status code must be between {0} and {1}, but was {2}.",
                            minValidStatusCode,
                            maxValidStatusCode,
                            httpStatusCode.Value));
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates timeout values.
        /// </summary>
        /// <param name="timeout">The timeout to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateTimeout(TimeSpan? timeout)
        {
            var errors = new List<string>();

            if (timeout.HasValue)
            {
                const int minTimeoutSeconds = 1;
                const int maxTimeoutSeconds = 300;

                if (timeout.Value.TotalSeconds < minTimeoutSeconds)
                {
                    errors.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Timeout must be at least {0} second(s), but was {1:F2} seconds.",
                            minTimeoutSeconds,
                            timeout.Value.TotalSeconds));
                }

                if (timeout.Value.TotalSeconds > maxTimeoutSeconds)
                {
                    errors.Add(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Timeout must be at most {0} seconds, but was {1:F2} seconds.",
                            maxTimeoutSeconds,
                            timeout.Value.TotalSeconds));
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates retry count values.
        /// </summary>
        /// <param name="maxRetries">The maximum retry count to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> ValidateMaxRetries(int maxRetries)
        {
            var errors = new List<string>();

            const int minRetries = 0;
            const int maxRetriesLimit = 10;

            if (maxRetries < minRetries)
            {
                errors.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Maximum retries must be at least {0}, but was {1}.",
                        minRetries,
                        maxRetries));
            }

            if (maxRetries > maxRetriesLimit)
            {
                errors.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Maximum retries must be at most {0}, but was {1}.",
                        maxRetriesLimit,
                        maxRetries));
            }

            return errors.AsReadOnly();
        }
    }
}