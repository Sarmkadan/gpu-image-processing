#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="HtmlResultFormatter"/> instances.
    /// </summary>
    public static class HtmlResultFormatterValidation
    {
        /// <summary>
        /// Validates the specified <see cref="HtmlResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <returns>A list of validation messages. Empty list indicates the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this HtmlResultFormatter? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate GetFileExtension()
            try
            {
                var extension = value.GetFileExtension();
                if (string.IsNullOrWhiteSpace(extension))
                {
                    errors.Add("GetFileExtension() returned null or whitespace");
                }
                else if (!extension.StartsWith(".", StringComparison.Ordinal))
                {
                    errors.Add("GetFileExtension() should return a value starting with '.'");
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
                    errors.Add("GetMimeType() returned null or whitespace");
                }
                else if (!mimeType.Equals("text/html", StringComparison.Ordinal))
                {
                    errors.Add("GetMimeType() should return 'text/html'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"GetMimeType() threw an exception: {ex.Message}");
            }

            // Validate FormatResult()
            try
            {
                var result = new ProcessingResult
                {
                    JobId = Guid.NewGuid(),
                    ImageId = Guid.NewGuid(),
                    InputFilePath = "/test/input.jpg",
                    OutputFilePath = "/test/output.jpg",
                    IsSuccessful = true,
                    ProcessingTimeMs = 100.5f,
                    OutputFileSizeBytes = 1024L
                };
                var formatted = value.FormatResult(result);
                if (string.IsNullOrWhiteSpace(formatted))
                {
                    errors.Add("FormatResult() returned null or whitespace");
                }
                else if (!formatted.Contains("<!DOCTYPE html>", StringComparison.Ordinal) && !formatted.Contains("<html", StringComparison.Ordinal))
                {
                    errors.Add("FormatResult() should return HTML content containing <!DOCTYPE html> or <html");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatResult() threw an exception: {ex.Message}");
            }

            // Validate FormatResults()
            try
            {
                var results = new List<ProcessingResult>
                {
                    new ProcessingResult
                    {
                        JobId = Guid.NewGuid(),
                        ImageId = Guid.NewGuid(),
                        InputFilePath = "/test/input.jpg",
                        OutputFilePath = "/test/output.jpg",
                        IsSuccessful = true,
                        ProcessingTimeMs = 100.5f,
                        OutputFileSizeBytes = 1024L
                    }
                };
                var formatted = value.FormatResults(results);
                if (string.IsNullOrWhiteSpace(formatted))
                {
                    errors.Add("FormatResults() returned null or whitespace");
                }
                else if (!formatted.Contains("<!DOCTYPE html>", StringComparison.Ordinal) && !formatted.Contains("<html", StringComparison.Ordinal))
                {
                    errors.Add("FormatResults() should return HTML content containing <!DOCTYPE html> or <html");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatResults() threw an exception: {ex.Message}");
            }

            // Validate FormatJob()
            try
            {
                var job = new ProcessingJob
                {
                    Name = "Test Job",
                    Status = Core.Constants.ProcessingStatus.Completed,
                    ProcessedImages = 10,
                    TotalImages = 10,
                    ProgressPercentage = 100.0f
                };
                var formatted = value.FormatJob(job);
                if (string.IsNullOrWhiteSpace(formatted))
                {
                    errors.Add("FormatJob() returned null or whitespace");
                }
                else if (!formatted.Contains("<div class=\"job\">", StringComparison.Ordinal))
                {
                    errors.Add("FormatJob() should return HTML content containing '<div class=\"job\">'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatJob() threw an exception: {ex.Message}");
            }

            // Validate FormatDevice()
            try
            {
                var device = new DeviceInfo
                {
                    Name = "Test Device",
                    Vendor = "NVIDIA",
                    DeviceType = "GPU",
                    IsAvailable = true,
                    GlobalMemoryBytes = 8L * 1024 * 1024 * 1024,
                    ClockFrequencyMHz = 1500.0f,
                    ComputeUnits = 3840,
                    OpenCLVersion = "3.0"
                };
                var formatted = value.FormatDevice(device);
                if (string.IsNullOrWhiteSpace(formatted))
                {
                    errors.Add("FormatDevice() returned null or whitespace");
                }
                else if (!formatted.Contains("<div class=\"device\">", StringComparison.Ordinal))
                {
                    errors.Add("FormatDevice() should return HTML content containing '<div class=\"device\">'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatDevice() threw an exception: {ex.Message}");
            }

            // Validate FormatError()
            try
            {
                var formatted = value.FormatError("Test error message", "TEST001");
                if (string.IsNullOrWhiteSpace(formatted))
                {
                    errors.Add("FormatError() returned null or whitespace");
                }
                else if (!formatted.Contains("<div class=\"error-report\">", StringComparison.Ordinal))
                {
                    errors.Add("FormatError() should return HTML content containing '<div class=\"error-report\">'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"FormatError() threw an exception: {ex.Message}");
            }

            // Validate Format()
            try
            {
                var results = new List<ProcessingResult>
                {
                    new ProcessingResult
                    {
                        JobId = Guid.NewGuid(),
                        ImageId = Guid.NewGuid(),
                        InputFilePath = "/test/input.jpg",
                        OutputFilePath = "/test/output.jpg",
                        IsSuccessful = true,
                        ProcessingTimeMs = 100.5f,
                        OutputFileSizeBytes = 1024L
                    }
                };
                var formatted = value.Format(results);
                if (string.IsNullOrWhiteSpace(formatted))
                {
                    errors.Add("Format() returned null or whitespace");
                }
                else if (!formatted.Contains("<!DOCTYPE html>", StringComparison.Ordinal) && !formatted.Contains("<html", StringComparison.Ordinal))
                {
                    errors.Add("Format() should return HTML content containing <!DOCTYPE html> or <html");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Format() threw an exception: {ex.Message}");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="HtmlResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this HtmlResultFormatter? value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="HtmlResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation errors.</exception>
        public static void EnsureValid(this HtmlResultFormatter? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"HtmlResultFormatter is not valid. Validation errors: {string.Join("; ", errors)}");
            }
        }
    }
}