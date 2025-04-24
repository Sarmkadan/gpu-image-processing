// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Formatter for converting processing results to CSV (comma-separated values) format.
    /// Useful for spreadsheet applications and data analysis tools.
    /// </summary>
    public class CsvResultFormatter : IResultFormatter
    {
        /// <summary>
        /// Gets the file extension for this format.
        /// </summary>
        public string GetFileExtension()
        {
            return ".csv";
        }

        /// <summary>
        /// Gets the MIME type for this format.
        /// </summary>
        public string GetMimeType()
        {
            return "text/csv";
        }

        /// <summary>
        /// Formats a single processing result to CSV.
        /// </summary>
        public string FormatResult(ProcessingResult result)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("ID,JobID,ImageID,Status,StartTime,CompletionTime,DurationMs,OutputPath,ProcessedSize");

            // Data
            if (result != null)
            {
                long durationMs = (long)((result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0);
                sb.AppendLine($"\"{result.Id}\",\"{result.JobId}\",\"{result.ImageId}\",\"{result.Status}\",\"{result.StartTime:O}\",\"{result.CompletionTime:O}\",{durationMs},\"{EscapeCsvValue(result.OutputImagePath)}\",{result.ProcessedSize}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats multiple processing results to CSV table.
        /// </summary>
        public string FormatResults(List<ProcessingResult> results)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("ID,JobID,ImageID,Status,StartTime,CompletionTime,DurationMs,OutputPath");

            // Data rows
            if (results != null)
            {
                foreach (var result in results)
                {
                    long durationMs = (long)((result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0);
                    sb.AppendLine($"\"{result.Id}\",\"{result.JobId}\",\"{result.ImageId}\",\"{result.Status}\",\"{result.StartTime:O}\",\"{result.CompletionTime:O}\",{durationMs},\"{EscapeCsvValue(result.OutputImagePath)}\"");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats job information to CSV.
        /// </summary>
        public string FormatJob(ProcessingJob job)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("JobID,Name,Status,TotalImages,ProcessedImages,FailedImages,CompletionPercent,CreatedAt,StartedAt,CompletedAt");

            // Data
            if (job != null)
            {
                double completionPercent = (job.ProcessedImages / (double)job.TotalImages) * 100;
                sb.AppendLine($"\"{job.Id}\",\"{EscapeCsvValue(job.Name)}\",\"{job.Status}\",{job.TotalImages},{job.ProcessedImages},{job.FailedImages},{completionPercent:F2},\"{job.CreatedAt:O}\",\"{job.StartedAt:O}\",\"{job.CompletedAt:O}\"");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats device information to CSV.
        /// </summary>
        public string FormatDevice(DeviceInfo device)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("DeviceID,Name,Type,Vendor,MemoryBytes,MemoryGB,ComputeUnits,Available,DriverVersion");

            // Data
            if (device != null)
            {
                double memoryGb = device.MemoryBytes / (1024.0 * 1024.0 * 1024.0);
                sb.AppendLine($"{device.Id},\"{EscapeCsvValue(device.Name)}\",\"{device.Type}\",\"{EscapeCsvValue(device.Vendor)}\",{device.MemoryBytes},{memoryGb:F2},{device.ComputeUnits},{device.IsAvailable},\"{EscapeCsvValue(device.DriverVersion)}\"");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats error information to CSV.
        /// </summary>
        public string FormatError(string errorMessage, string errorCode = null, Exception exception = null)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("ErrorCode,Message,Details,Timestamp");

            // Data
            string code = errorCode ?? "UNKNOWN_ERROR";
            string details = exception?.Message ?? string.Empty;
            sb.AppendLine($"\"{code}\",\"{EscapeCsvValue(errorMessage)}\",\"{EscapeCsvValue(details)}\",\"{DateTime.UtcNow:O}\"");

            return sb.ToString();
        }

        /// <summary>
        /// Escapes CSV field values by wrapping in quotes and escaping internal quotes.
        /// </summary>
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Escape quotes by doubling them
            return value.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Formats statistics summary to CSV.
        /// </summary>
        public string FormatStatistics(Dictionary<string, object> statistics)
        {
            var sb = new StringBuilder();

            if (statistics == null || statistics.Count == 0)
                return sb.ToString();

            // Header
            sb.AppendLine("Metric,Value");

            // Data
            foreach (var stat in statistics)
            {
                sb.AppendLine($"\"{EscapeCsvValue(stat.Key)}\",\"{stat.Value}\"");
            }

            return sb.ToString();
        }
    }
}
