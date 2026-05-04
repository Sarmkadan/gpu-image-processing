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
    /// Formatter for converting processing results to human-readable text format.
    /// Designed for console output and log files.
    /// </summary>
    public class TextResultFormatter : IResultFormatter
    {
        private const string Separator = "==================================================";
        private const int IndentSize = 2;

        /// <summary>
        /// Gets the file extension for this format.
        /// </summary>
        public string GetFileExtension()
        {
            return ".txt";
        }

        /// <summary>
        /// Gets the MIME type for this format.
        /// </summary>
        public string GetMimeType()
        {
            return "text/plain";
        }

        /// <summary>
        /// Formats a single processing result to readable text.
        /// </summary>
        public string FormatResult(ProcessingResult result)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Separator);
            sb.AppendLine("PROCESSING RESULT");
            sb.AppendLine(Separator);

            if (result != null)
            {
                sb.AppendLine($"ID:                  {result.Id}");
                sb.AppendLine($"Job ID:              {result.JobId}");
                sb.AppendLine($"Image ID:            {result.ImageId}");
                sb.AppendLine($"Status:              {result.Status}");
                sb.AppendLine($"Start Time:          {result.StartTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Completion Time:     {result.CompletionTime:yyyy-MM-dd HH:mm:ss}");

                if (result.CompletionTime.HasValue && result.StartTime.HasValue)
                {
                    long durationMs = (long)(result.CompletionTime.Value - result.StartTime.Value).TotalMilliseconds;
                    sb.AppendLine($"Duration:            {FormatDuration(durationMs)}");
                }

                sb.AppendLine($"Output Path:         {result.OutputImagePath}");
                sb.AppendLine($"Processed Size:      {result.ProcessedSize} bytes");

                if (result.Metadata != null && result.Metadata.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Metadata:");
                    foreach (var kvp in result.Metadata)
                    {
                        sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }
            }
            else
            {
                sb.AppendLine("No result data available");
            }

            sb.AppendLine(Separator);

            return sb.ToString();
        }

        /// <summary>
        /// Formats multiple processing results to text summary.
        /// </summary>
        public string FormatResults(List<ProcessingResult> results)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Separator);
            sb.AppendLine("PROCESSING RESULTS SUMMARY");
            sb.AppendLine(Separator);

            if (results == null || results.Count == 0)
            {
                sb.AppendLine("No results available");
                sb.AppendLine(Separator);
                return sb.ToString();
            }

            sb.AppendLine($"Total Results: {results.Count}");
            sb.AppendLine();

            foreach (var result in results)
            {
                sb.AppendLine($"[{result.Id}]");
                sb.AppendLine($"  Status:    {result.Status}");
                sb.AppendLine($"  Image ID:  {result.ImageId}");
                sb.AppendLine($"  Output:    {result.OutputImagePath}");

                if (result.CompletionTime.HasValue && result.StartTime.HasValue)
                {
                    long durationMs = (long)(result.CompletionTime.Value - result.StartTime.Value).TotalMilliseconds;
                    sb.AppendLine($"  Duration:  {FormatDuration(durationMs)}");
                }

                sb.AppendLine();
            }

            sb.AppendLine(Separator);

            return sb.ToString();
        }

        /// <summary>
        /// Formats job information to readable text.
        /// </summary>
        public string FormatJob(ProcessingJob job)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Separator);
            sb.AppendLine("PROCESSING JOB INFORMATION");
            sb.AppendLine(Separator);

            if (job != null)
            {
                sb.AppendLine($"Job ID:              {job.Id}");
                sb.AppendLine($"Name:                {job.Name}");
                sb.AppendLine($"Status:              {job.Status}");
                sb.AppendLine($"Total Images:        {job.TotalImages}");
                sb.AppendLine($"Processed Images:    {job.ProcessedImages}");
                sb.AppendLine($"Failed Images:       {job.FailedImages}");

                double completionPercent = (job.ProcessedImages / (double)job.TotalImages) * 100;
                sb.AppendLine($"Completion:          {completionPercent:F1}%");
                sb.AppendLine($"Created At:          {job.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Started At:          {job.StartedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Completed At:        {job.CompletedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Filters Applied:     {job.Filters?.Count ?? 0}");
                sb.AppendLine($"Transforms Applied:  {job.Transforms?.Count ?? 0}");
            }
            else
            {
                sb.AppendLine("No job data available");
            }

            sb.AppendLine(Separator);

            return sb.ToString();
        }

        /// <summary>
        /// Formats device information to readable text.
        /// </summary>
        public string FormatDevice(DeviceInfo device)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Separator);
            sb.AppendLine("DEVICE INFORMATION");
            sb.AppendLine(Separator);

            if (device != null)
            {
                sb.AppendLine($"Device ID:           {device.Id}");
                sb.AppendLine($"Name:                {device.Name}");
                sb.AppendLine($"Type:                {device.Type}");
                sb.AppendLine($"Vendor:              {device.Vendor}");
                sb.AppendLine($"Available:           {(device.IsAvailable ? "Yes" : "No")}");

                double memoryGb = device.MemoryBytes / (1024.0 * 1024.0 * 1024.0);
                sb.AppendLine($"Memory:              {memoryGb:F2} GB ({device.MemoryBytes} bytes)");
                sb.AppendLine($"Compute Units:       {device.ComputeUnits}");
                sb.AppendLine($"Driver Version:      {device.DriverVersion}");

                if (device.Extensions != null && device.Extensions.Count > 0)
                {
                    sb.AppendLine($"Extensions:          {string.Join(", ", device.Extensions)}");
                }
            }
            else
            {
                sb.AppendLine("No device data available");
            }

            sb.AppendLine(Separator);

            return sb.ToString();
        }

        /// <summary>
        /// Formats error information to readable text.
        /// </summary>
        public string FormatError(string errorMessage, string errorCode = null, Exception exception = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Separator);
            sb.AppendLine("ERROR REPORT");
            sb.AppendLine(Separator);

            sb.AppendLine($"Code:       {errorCode ?? "UNKNOWN_ERROR"}");
            sb.AppendLine($"Message:    {errorMessage}");
            sb.AppendLine($"Timestamp:  {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            if (exception != null)
            {
                sb.AppendLine();
                sb.AppendLine("Exception Details:");
                sb.AppendLine($"  Type:    {exception.GetType().Name}");
                sb.AppendLine($"  Message: {exception.Message}");

                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    sb.AppendLine();
                    sb.AppendLine("Stack Trace:");
                    foreach (var line in exception.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        sb.AppendLine($"  {line}");
                    }
                }
            }

            sb.AppendLine(Separator);

            return sb.ToString();
        }

        /// <summary>
        /// Formats duration from milliseconds to human-readable string.
        /// </summary>
        private string FormatDuration(long milliseconds)
        {
            if (milliseconds < 1000)
                return $"{milliseconds}ms";
            else if (milliseconds < 60000)
                return $"{milliseconds / 1000.0:F2}s";
            else if (milliseconds < 3600000)
                return $"{milliseconds / 60000.0:F2}m";
            else
                return $"{milliseconds / 3600000.0:F2}h";
        }
    }
}
