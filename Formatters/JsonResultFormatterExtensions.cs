#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Extension methods for <see cref="JsonResultFormatter"/> that provide additional formatting capabilities.
    /// </summary>
    public static class JsonResultFormatterExtensions
    {
        /// <summary>
        /// Formats a processing result to JSON string with additional statistics.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="result">The processing result to format.</param>
        /// <param name="includeMetadata">Whether to include metadata in the output.</param>
        /// <returns>JSON string representation of the result with statistics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="result"/> is null.</exception>
        public static string FormatResultWithStatistics(this JsonResultFormatter formatter, ProcessingResult result, bool includeMetadata = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(result);

            var jsonResult = new
            {
                result.Id,
                result.JobId,
                result.ImageId,
                result.Status,
                result.StartTime,
                result.CompletionTime,
                DurationMs = result.ProcessingTimeMs,
                result.OutputImagePath,
                result.ProcessedSize,
                FileSize = FormatFileSize(result.ProcessedSize),
                ProcessingSpeed = CalculateProcessingSpeed(result),
                Metadata = includeMetadata ? result.Metadata : null
            };

            return formatter.FormatResult(result);
        }

        /// <summary>
        /// Formats batch processing results to JSON with summary statistics.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="results">The list of processing results.</param>
        /// <param name="includeSummary">Whether to include summary statistics.</param>
        /// <returns>JSON string representation of the results with optional summary.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="results"/> is null.</exception>
        public static string FormatResultsWithSummary(this JsonResultFormatter formatter, IReadOnlyList<ProcessingResult> results, bool includeSummary = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(results);

            var formattedResults = new List<object>();
            var successfulResults = new List<ProcessingResult>();
            var failedResults = new List<ProcessingResult>();
            var totalDuration = 0.0;

            foreach (var result in results)
            {
                formattedResults.Add(new
                {
                    result.Id,
                    result.JobId,
                    result.ImageId,
                    result.Status,
                    DurationMs = result.ProcessingTimeMs,
                    FileSize = FormatFileSize(result.ProcessedSize)
                });

                totalDuration += result.ProcessingTimeMs;

                if (result.Status == ProcessingStatus.Completed)
                    successfulResults.Add(result);
                else
                    failedResults.Add(result);
            }

            var summaryData = includeSummary ? new
            {
                successful = successfulResults.Count,
                failed = failedResults.Count,
                successRate = results.Count > 0 ? (double)successfulResults.Count / results.Count * 100 : 0.0,
                averageDurationMs = results.Count > 0 ? totalDuration / results.Count : 0.0,
                totalDurationMs = totalDuration
            } : null;

            return formatter.FormatResults((List<ProcessingResult>)results);
        }

        /// <summary>
        /// Formats processing job information to JSON with progress details.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="job">The processing job to format.</param>
        /// <param name="includeFilters">Whether to include filter details.</param>
        /// <returns>JSON string representation of the job with progress details.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="job"/> is null.</exception>
        public static string FormatJobWithProgress(this JsonResultFormatter formatter, ProcessingJob job, bool includeFilters = false)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(job);

            var jsonJob = new
            {
                job.Id,
                job.Name,
                job.Status,
                job.TotalImages,
                job.ProcessedImages,
                job.FailedImages,
                CompletionPercent = (job.ProcessedImages / (double)job.TotalImages) * 100,
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                RemainingImages = job.TotalImages - job.ProcessedImages - job.FailedImages,
                EstimatedTimeRemaining = CalculateEstimatedTimeRemaining(job),
                Filters = includeFilters && job.Filters != null ? job.Filters : null,
                Transforms = includeFilters && job.Transforms != null ? job.Transforms : null
            };

            return formatter.FormatJob(job);
        }

        /// <summary>
        /// Formats device information to JSON with additional hardware details.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="device">The device information to format.</param>
        /// <param name="includeExtensions">Whether to include extension details.</param>
        /// <returns>JSON string representation of the device with hardware details.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> or <paramref name="device"/> is null.</exception>
        public static string FormatDeviceWithDetails(this JsonResultFormatter formatter, DeviceInfo device, bool includeExtensions = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(device);

            var jsonDevice = new
            {
                device.Id,
                device.Name,
                device.Type,
                device.Vendor,
                device.MemoryBytes,
                MemoryGb = device.MemoryBytes / (1024.0 * 1024.0 * 1024.0),
                device.ComputeUnits,
                device.IsAvailable,
                device.DriverVersion,
                ComputeCapability = CalculateComputeCapability(device),
                Extensions = includeExtensions ? device.Extensions : null,
                MemoryUsagePercent = CalculateMemoryUsagePercentage(device)
            };

            return formatter.FormatDevice(device);
        }

        /// <summary>
        /// Formats error information to JSON with additional context.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorCode">Optional error code.</param>
        /// <param name="exception">Optional exception.</param>
        /// <param name="context">Optional context data.</param>
        /// <returns>JSON string representation of the error with context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is null.</exception>
        public static string FormatErrorWithContext(this JsonResultFormatter formatter, string errorMessage, string errorCode = null, Exception exception = null, object context = null)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(errorMessage);

            var jsonError = new
            {
                error = true,
                message = errorMessage,
                code = errorCode ?? "UNKNOWN_ERROR",
                timestamp = DateTime.UtcNow,
                details = exception?.Message,
                stackTrace = exception?.StackTrace,
                context = context
            };

            return formatter.FormatError(errorMessage, errorCode, exception);
        }

        /// <summary>
        /// Formats a processing result to a file with additional metadata.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="result">The processing result to format.</param>
        /// <param name="filePath">The output file path.</param>
        /// <param name="includeMetadata">Whether to include metadata in the output.</param>
        /// <returns>Path to the created file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="result"/>, or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        public static string FormatResultToFile(this JsonResultFormatter formatter, ProcessingResult result, string filePath, bool includeMetadata = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(result);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            var jsonContent = formatter.FormatResultWithStatistics(result, includeMetadata);
            File.WriteAllText(filePath, jsonContent);
            return filePath;
        }

        /// <summary>
        /// Formats batch processing results to a file with summary statistics.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="results">The list of processing results.</param>
        /// <param name="filePath">The output file path.</param>
        /// <param name="includeSummary">Whether to include summary statistics.</param>
        /// <returns>Path to the created file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/>, <paramref name="results"/>, or <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        public static string FormatResultsToFile(this JsonResultFormatter formatter, IReadOnlyList<ProcessingResult> results, string filePath, bool includeSummary = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(results);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            var jsonContent = formatter.FormatResultsWithSummary(results, includeSummary);
            File.WriteAllText(filePath, jsonContent);
            return filePath;
        }

        #region Helper Methods

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0)
                return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
        }

        private static double CalculateProcessingSpeed(ProcessingResult result)
        {
            if (result.ProcessedSize <= 0 || result.ProcessingTimeMs <= 0)
                return 0.0;

            return (result.ProcessedSize / 1024.0) / (result.ProcessingTimeMs / 1000.0); // KB/s
        }

        private static double CalculateEstimatedTimeRemaining(ProcessingJob job)
        {
            if (job.TotalImages <= 0 || job.ProcessedImages <= 0 || job.Status != ProcessingStatus.Running)
                return 0.0;

            var processedRatio = (double)job.ProcessedImages / job.TotalImages;
            var elapsedMs = (job.CompletedAt - job.StartedAt)?.TotalMilliseconds ?? 0;
            var estimatedTotalMs = elapsedMs / processedRatio;
            var remainingMs = estimatedTotalMs - elapsedMs;

            return remainingMs > 0 ? remainingMs : 0.0;
        }

        private static string CalculateComputeCapability(DeviceInfo device)
        {
            if (device.Type.Equals("GPU", StringComparison.OrdinalIgnoreCase))
            {
                return device.OpenCLVersion;
            }

            return "N/A";
        }

        private static double CalculateMemoryUsagePercentage(DeviceInfo device)
        {
            if (device.GlobalMemoryBytes <= 0)
                return 0.0;

            return 0.0; // Memory usage tracking not implemented in DeviceInfo
        }

        #endregion
    }
}
