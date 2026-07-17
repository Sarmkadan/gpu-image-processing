#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="TextResultFormatter"/> instances.
    /// </summary>
    public static class TextResultFormatterValidation
    {
        /// <summary>
        /// Validates the specified <see cref="TextResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this TextResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate GetFileExtension()
            try
            {
                var extension = value.GetFileExtension();
                if (string.IsNullOrWhiteSpace(extension))
                {
                    errors.Add("GetFileExtension() returned null or whitespace.");
                }
                else if (!extension.StartsWith(".", StringComparison.Ordinal))
                {
                    errors.Add("GetFileExtension() did not return a valid file extension (must start with '.').");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"GetFileExtension() threw an exception: {ex.Message}");
            }

            // Validate GetMimeType()
            try
            {
                var mimeType = value.GetMimeType();
                if (string.IsNullOrWhiteSpace(mimeType))
                {
                    errors.Add("GetMimeType() returned null or whitespace.");
                }
                else if (!mimeType.Contains("/"))
                {
                    errors.Add("GetMimeType() did not return a valid MIME type (must contain '/').");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"GetMimeType() threw an exception: {ex.Message}");
            }

            // Validate FormatResult() with null input
            try
            {
                value.FormatResult(null!);
            }
            catch (Exception ex)
            {
                errors.Add($"FormatResult(null) threw an exception: {ex.Message}");
            }

            // Validate FormatResults() with null and empty input
            try
            {
                value.FormatResults(null!);
            }
            catch (Exception ex)
            {
                errors.Add($"FormatResults(null) threw an exception: {ex.Message}");
            }

            try
            {
                var emptyResults = new List<ProcessingResult>();
                var result = value.FormatResults(emptyResults);
                if (string.IsNullOrWhiteSpace(result))
                {
                    errors.Add("FormatResults(empty) returned null or whitespace.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatResults(empty) threw an exception: {ex.Message}");
            }

            // Validate FormatJob() with null and valid input
            try
            {
                value.FormatJob(null!);
            }
            catch (Exception ex)
            {
                errors.Add($"FormatJob(null) threw an exception: {ex.Message}");
            }

            try
            {
                var validJob = new ProcessingJob
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Job",
                    Status = ProcessingStatus.Pending,
                    TotalImages = 10,
                    ProcessedImages = 0,
                    FailedImages = 0,
                    CreatedAt = DateTime.UtcNow,
                    StartedAt = null,
                    CompletedAt = null
                };
                var jobResult = value.FormatJob(validJob);
                if (string.IsNullOrWhiteSpace(jobResult))
                {
                    errors.Add("FormatJob(valid) returned null or whitespace.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatJob(valid) threw an exception: {ex.Message}");
            }

            // Validate FormatDevice() with null and valid input
            try
            {
                value.FormatDevice(null!);
            }
            catch (Exception ex)
            {
                errors.Add($"FormatDevice(null) threw an exception: {ex.Message}");
            }

            try
            {
                var validDevice = new DeviceInfo
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Device",
                    Type = "GPU",
                    Vendor = "NVIDIA",
                    IsAvailable = true,
                    MemoryBytes = 8L * 1024 * 1024 * 1024,
                    ComputeUnits = 5120,
                    DriverVersion = "535.86.0"
                };
                var deviceResult = value.FormatDevice(validDevice);
                if (string.IsNullOrWhiteSpace(deviceResult))
                {
                    errors.Add("FormatDevice(valid) returned null or whitespace.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatDevice(valid) threw an exception: {ex.Message}");
            }

            // Validate FormatError() with null and valid input
            try
            {
                value.FormatError(null!);
            }
            catch (Exception ex)
            {
                errors.Add($"FormatError(null) threw an exception: {ex.Message}");
            }

            try
            {
                var errorResult = value.FormatError("Test error message", "TEST_ERROR");
                if (string.IsNullOrWhiteSpace(errorResult))
                {
                    errors.Add("FormatError(valid) returned null or whitespace.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatError(valid) threw an exception: {ex.Message}");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="TextResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this TextResultFormatter value) => value?.Validate().Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="TextResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the validation errors.</exception>
        public static void EnsureValid(this TextResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"TextResultFormatter is not valid. Validation errors:\n - {
                    string.Join("\n - ", errors)
                    }",
                    nameof(value));
            }
        }
    }
}