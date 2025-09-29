#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Extension methods for <see cref="CsvResultFormatter"/> that provide additional formatting capabilities
    /// and convenience methods for working with CSV output.
    /// </summary>
    public static class CsvResultFormatterExtensions
    {
        /// <summary>
        /// Escapes CSV field values by wrapping in quotes and escaping internal quotes.
        /// </summary>
        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape quotes by doubling them
            return value.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Formats multiple processing results to CSV with additional metadata columns.
        /// Includes processing time statistics and success rate.
        /// </summary>
        /// <param name="formatter">The CSV formatter instance</param>
        /// <param name="results">List of processing results to format</param>
        /// <param name="includeStatistics">Whether to include processing statistics in the output</param>
        /// <returns>CSV formatted string with results and optional statistics</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or results is null</exception>
        public static string FormatResultsWithStatistics(
            this CsvResultFormatter formatter,
            List<ProcessingResult> results,
            bool includeStatistics = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(results);

            var sb = new System.Text.StringBuilder();

            // Header with statistics columns
            sb.AppendLine("ID,JobID,ImageID,Status,StartTime,CompletionTime,DurationMs,OutputPath,ProcessedSize,ProcessingRateMs");

            // Data rows
            foreach (var result in results)
            {
                long durationMs = (long)(result.ProcessingTimeMs);
                double processingRateMs = durationMs > 0 && result.OutputFileSizeBytes > 0
                    ? (double)result.OutputFileSizeBytes / durationMs
                    : 0;

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4:O}\",\"{5:O}\",{6},\"{7}\",{8},{9:F2}",
                    result.Id,
                    result.JobId,
                    result.ImageId,
                    result.IsSuccessful ? "Completed" : "Failed",
                    result.ProcessedAt,
                    result.ProcessedAt,
                    durationMs,
                    EscapeCsvValue(result.OutputFilePath),
                    result.OutputFileSizeBytes,
                    processingRateMs));
            }

            // Add statistics section if requested
            if (includeStatistics && results.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("# Statistics");
                sb.AppendLine("Metric,Value");

                int successCount = results.Count(r => r.IsSuccessful);
                int failedCount = results.Count - successCount;
                double successRate = results.Count > 0 ? (double)successCount / results.Count * 100 : 0;

                long totalDuration = 0;
                long totalSize = 0;
                foreach (var result in results)
                {
                    totalDuration += (long)result.ProcessingTimeMs;
                    totalSize += result.OutputFileSizeBytes;
                }

                double avgDurationMs = results.Count > 0 ? (double)totalDuration / results.Count : 0;
                double totalProcessingRate = totalDuration > 0 ? (double)totalSize / totalDuration : 0;

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "TotalResults,{0}\nSuccessful,{1}\nFailed,{2}\nSuccessRate,{3:F2}%\nTotalDurationMs,{4}\nAverageDurationMs,{5:F2}\nTotalProcessedSize,{6}\nTotalProcessingRate,{7:F2} bytes/ms",
                    results.Count,
                    successCount,
                    failedCount,
                    successRate,
                    totalDuration,
                    avgDurationMs,
                    totalSize,
                    totalProcessingRate));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a collection of jobs to CSV with summary statistics.
        /// </summary>
        /// <param name="formatter">The CSV formatter instance</param>
        /// <param name="jobs">Collection of processing jobs to format</param>
        /// <returns>CSV formatted string with jobs and summary statistics</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or jobs is null</exception>
        public static string FormatJobsWithSummary(
            this CsvResultFormatter formatter,
            IEnumerable<ProcessingJob> jobs)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(jobs);

            var jobList = new List<ProcessingJob>(jobs);
            var sb = new System.Text.StringBuilder();

            // Header
            sb.AppendLine("JobID,Name,Status,TotalImages,ProcessedImages,FailedImages,CompletionPercent,CreatedAt,StartedAt,CompletedAt,EstimatedDurationMs");

            // Data rows
            foreach (var job in jobList)
            {
                double completionPercent = job.TotalImages > 0
                    ? (job.ProcessedImages / (double)job.TotalImages) * 100
                    : 0;

                long estimatedDurationMs = job.TotalImages > 0 && job.ProcessedImages > 0
                    ? (long)(((double)(job.ProcessedImages * 1000) / job.TotalImages) * (job.TotalImages - job.ProcessedImages))
                    : 0;

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "\"{0}\",\"{1}\",\"{2}\",{3},{4},{5},{6:F2},\"{7:O}\",\"{8:O}\",\"{9:O}\",{10}",
                    job.Id,
                    EscapeCsvValue(job.Name),
                    job.Status,
                    job.TotalImages,
                    job.ProcessedImages,
                    job.FailedImages,
                    completionPercent,
                    job.CreatedAt,
                    job.StartedAt,
                    job.CompletedAt,
                    estimatedDurationMs));
            }

            // Summary statistics
            if (jobList.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("# Summary");
                sb.AppendLine("Metric,Value");

                int totalJobs = jobList.Count;
                int completedJobs = jobList.Count(j => j.Status == ProcessingStatus.Completed);
                int pendingJobs = jobList.Count(j => j.Status == ProcessingStatus.Pending);
                int failedJobs = jobList.Count - completedJobs - pendingJobs;

                double avgCompletionPercent = jobList.Sum(j => j.TotalImages > 0
                    ? (j.ProcessedImages / (double)j.TotalImages) * 100
                    : 0) / totalJobs;

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "TotalJobs,{0}\nCompletedJobs,{1}\nPendingJobs,{2}\nFailedJobs,{3}\nAverageCompletionPercent,{4:F2}%",
                    totalJobs,
                    completedJobs,
                    pendingJobs,
                    failedJobs,
                    avgCompletionPercent));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats device information to CSV with extended device capabilities.
        /// </summary>
        /// <param name="formatter">The CSV formatter instance</param>
        /// <param name="devices">Collection of device information to format</param>
        /// <returns>CSV formatted string with device information and capabilities</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or devices is null</exception>
        public static string FormatDevicesWithCapabilities(
            this CsvResultFormatter formatter,
            IEnumerable<DeviceInfo> devices)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(devices);

            var deviceList = new List<DeviceInfo>(devices);
            var sb = new System.Text.StringBuilder();

            // Header with additional capability columns
            sb.AppendLine("DeviceID,Name,Type,Vendor,MemoryBytes,MemoryGB,ComputeUnits,Available,DriverVersion,MaxWorkGroupSize,ClockFrequencyMHz,ComputeCapability");

            // Data rows
            foreach (var device in deviceList)
            {
                double memoryGb = device.GlobalMemoryBytes / (1024.0 * 1024.0 * 1024.0);

                // Parse compute capability from driver version if available
                string computeCapability = "Unknown";
                if (!string.IsNullOrEmpty(device.DriverVersion) && device.DriverVersion.Contains("."))
                {
                    var parts = device.DriverVersion.Split('.');
                    if (parts.Length >= 2 && int.TryParse(parts[0], out int major) && int.TryParse(parts[1], out int minor))
                    {
                        computeCapability = $"{major}.{minor}";
                    }
                }

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},\"{1}\",\"{2}\",\"{3}\",{4},{5:F2},{6},{7},{8},\"{9}\",{10},{11}",
                    device.Id,
                    EscapeCsvValue(device.Name),
                    device.Type,
                    EscapeCsvValue(device.Vendor),
                    device.GlobalMemoryBytes,
                    memoryGb,
                    device.ComputeUnits,
                    device.IsAvailable,
                    EscapeCsvValue(device.DriverVersion),
                    device.MaxWorkGroupSize,
                    device.ClockFrequencyMHz,
                    computeCapability));
            }

            // Summary statistics
            if (deviceList.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("# Summary");
                sb.AppendLine("Metric,Value");

                int availableCount = deviceList.Count(d => d.IsAvailable);
                int unavailableCount = deviceList.Count - availableCount;
                long totalMemory = deviceList.Sum(d => d.GlobalMemoryBytes);
                double totalMemoryGb = totalMemory / (1024.0 * 1024.0 * 1024.0);
                int totalComputeUnits = deviceList.Sum(d => d.ComputeUnits);

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "TotalDevices,{0}\nAvailableDevices,{1}\nUnavailableDevices,{2}\nTotalMemoryGB,{3:F2}\nTotalComputeUnits,{4}\nAverageMemoryGB,{5:F2}",
                    deviceList.Count,
                    availableCount,
                    unavailableCount,
                    totalMemoryGb,
                    totalComputeUnits,
                    totalMemoryGb / deviceList.Count));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats error information to CSV with additional context columns.
        /// </summary>
        /// <param name="formatter">The CSV formatter instance</param>
        /// <param name="errorMessages">Collection of error messages to format</param>
        /// <param name="includeStackTrace">Whether to include stack trace information</param>
        /// <returns>CSV formatted string with errors and optional stack traces</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or errorMessages is null</exception>
        public static string FormatErrorsWithContext(
            this CsvResultFormatter formatter,
            IEnumerable<(string Message, string Code, Exception Exception)> errorMessages,
            bool includeStackTrace = false)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(errorMessages);

            var sb = new System.Text.StringBuilder();

            // Header with optional stack trace column
            sb.AppendLine(includeStackTrace
                ? "Timestamp,ErrorCode,Message,Details,StackTrace"
                : "Timestamp,ErrorCode,Message,Details");

            // Data rows
            foreach (var (message, code, exception) in errorMessages)
            {
                string details = exception?.Message ?? string.Empty;
                string stackTrace = includeStackTrace && exception != null
                    ? exception.StackTrace?.Replace("\r\n", " | ") ?? string.Empty
                    : string.Empty;

                if (includeStackTrace)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                        DateTime.UtcNow,
                        code ?? "UNKNOWN_ERROR",
                        EscapeCsvValue(message),
                        EscapeCsvValue(details),
                        EscapeCsvValue(stackTrace)));
                }
                else
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "\"{0}\",\"{1}\",\"{2}\",\"{3}\"",
                        DateTime.UtcNow,
                        code ?? "UNKNOWN_ERROR",
                        EscapeCsvValue(message),
                        EscapeCsvValue(details)));
                }
            }

            // Summary statistics
            var errorList = new List<(string Message, string Code, Exception Exception)>(errorMessages);
            if (errorList.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("# Summary");
                sb.AppendLine("Metric,Value");

                var errorCodes = new Dictionary<string, int>();
                foreach (var error in errorList)
                {
                    string code = error.Code ?? "UNKNOWN_ERROR";
                    if (errorCodes.ContainsKey(code))
                        errorCodes[code]++;
                    else
                        errorCodes[code] = 1;
                }

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "TotalErrors,{0}", errorList.Count));

                foreach (var codeGroup in errorCodes.OrderByDescending(kv => kv.Value))
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "Error_{0},{1}", codeGroup.Key, codeGroup.Value));
                }
            }

            return sb.ToString();
        }
    }
}
