#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Formats processing results as Markdown for documentation and reports.
    /// Generates tables, sections, and summary statistics in Markdown format.
    /// </summary>
    public class MarkdownResultFormatter : IResultFormatter
    {
        public string GetFileExtension() => ".md";

        public string GetMimeType() => "text/markdown";

        public string FormatResult(ProcessingResult result) => Format(new List<ProcessingResult> { result });

        public string FormatResults(List<ProcessingResult> results) => Format(results);

        public string FormatJob(ProcessingJob job)
        {
            var md = new StringBuilder();
            md.AppendLine($"## Job: {job.Name}");
            md.AppendLine();
            md.AppendLine($"- **Status:** {job.Status}");
            md.AppendLine($"- **Progress:** {job.ProcessedImages}/{job.TotalImages} ({job.ProgressPercentage:F1}%)");
            if (!string.IsNullOrEmpty(job.ErrorMessage))
                md.AppendLine($"- **Error:** {EscapeMarkdown(job.ErrorMessage)}");
            return md.ToString();
        }

        public string FormatDevice(DeviceInfo device)
        {
            var md = new StringBuilder();
            md.AppendLine($"## Device: {device.Name}");
            md.AppendLine();
            md.AppendLine($"- **Vendor:** {device.Vendor}");
            md.AppendLine($"- **Type:** {device.DeviceType}");
            md.AppendLine($"- **Available:** {device.IsAvailable}");
            md.AppendLine($"- **Global Memory:** {FormatBytes(device.GlobalMemoryBytes)}");
            return md.ToString();
        }

        public string FormatError(string errorMessage, string? errorCode = null, Exception? exception = null)
        {
            var md = new StringBuilder();
            md.AppendLine("## Error");
            md.AppendLine();
            md.AppendLine($"- **Message:** {EscapeMarkdown(errorMessage)}");
            if (!string.IsNullOrEmpty(errorCode))
                md.AppendLine($"- **Code:** {errorCode}");
            if (exception != null)
                md.AppendLine($"- **Exception:** {EscapeMarkdown(exception.ToString())}");
            return md.ToString();
        }

        public string Format(List<ProcessingResult> results)
        {
            var md = new StringBuilder();

            md.AppendLine("# GPU Image Processing Results Report");
            md.AppendLine();
            md.AppendLine($"**Generated:** {DateTime.UtcNow:O}");
            md.AppendLine();

            if (results == null || results.Count == 0)
            {
                md.AppendLine("No processing results found.");
                return md.ToString();
            }

            // Summary section
            md.AppendLine("## Summary");
            md.AppendLine();
            md.AppendLine($"- **Total Operations:** {results.Count}");

            var successCount = results.Count(r => r.IsSuccessful);
            var failureCount = results.Count - successCount;
            var successRate = (successCount / (double)results.Count) * 100;

            md.AppendLine($"- **Successful:** {successCount} ({successRate:F1}%)");
            md.AppendLine($"- **Failed:** {failureCount}");
            md.AppendLine();

            // Statistics section
            var totalDuration = results.Sum(r => r.ProcessingTimeMs);
            var avgDuration = results.Average(r => r.ProcessingTimeMs);
            var totalOutputSize = results.Sum(r => r.OutputFileSizeBytes);

            md.AppendLine("## Performance Metrics");
            md.AppendLine();
            md.AppendLine($"| Metric | Value |");
            md.AppendLine("|--------|-------|");
            md.AppendLine($"| Total Duration | {totalDuration:F2}ms |");
            md.AppendLine($"| Average Duration | {avgDuration:F2}ms |");
            md.AppendLine($"| Min Duration | {results.Min(r => r.ProcessingTimeMs):F2}ms |");
            md.AppendLine($"| Max Duration | {results.Max(r => r.ProcessingTimeMs):F2}ms |");
            md.AppendLine($"| Total Output Size | {FormatBytes(totalOutputSize)} |");
            md.AppendLine();

            // Results table
            md.AppendLine("## Detailed Results");
            md.AppendLine();
            md.AppendLine("| ID | Image ID | Operation | Status | Duration (ms) | Output Size |");
            md.AppendLine("|----|-----------|-----------|----|------|------|");

            foreach (var result in results)
            {
                var status = result.IsSuccessful ? "✓ Success" : "✗ Failed";
                md.AppendLine($"| {result.Id} " +
                            $"| {result.ImageId} " +
                            $"| {EscapeMarkdown(GetOperationLabel(result))} " +
                            $"| {status} " +
                            $"| {result.ProcessingTimeMs:F2} " +
                            $"| {FormatBytes(result.OutputFileSizeBytes)} |");
            }

            md.AppendLine();

            // Errors section
            var failedResults = results.Where(r => !r.IsSuccessful).ToList();
            if (failedResults.Any())
            {
                md.AppendLine("## Errors");
                md.AppendLine();

                foreach (var result in failedResults)
                {
                    md.AppendLine($"### Operation: {EscapeMarkdown(GetOperationLabel(result))}");
                    md.AppendLine($"- **ID:** {result.Id}");
                    md.AppendLine($"- **Image ID:** {result.ImageId}");
                    md.AppendLine($"- **Error:** {EscapeMarkdown(result.ErrorMessage)}");
                    md.AppendLine();
                }
            }

            // Operations breakdown
            md.AppendLine("## Operations Breakdown");
            md.AppendLine();

            var operationGroups = results.GroupBy(r => GetOperationLabel(r));
            foreach (var group in operationGroups)
            {
                var count = group.Count();
                var successes = group.Count(r => r.IsSuccessful);
                var avgDur = group.Average(r => r.ProcessingTimeMs);

                md.AppendLine($"- **{EscapeMarkdown(group.Key)}**");
                md.AppendLine($"  - Count: {count}");
                md.AppendLine($"  - Success Rate: {(successes / (double)count * 100):F1}%");
                md.AppendLine($"  - Average Duration: {avgDur:F2}ms");
            }

            md.AppendLine();

            // Footer
            md.AppendLine("---");
            md.AppendLine("*Report generated by GPU Image Processing System*");

            return md.ToString();
        }

        private string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text
                .Replace("|", "\\|")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("*", "\\*")
                .Replace("_", "\\_")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private static string GetOperationLabel(ProcessingResult result)
        {
            var operations = result.AppliedFilters.Concat(result.AppliedTransforms).ToList();
            return operations.Count > 0 ? string.Join(", ", operations) : "N/A";
        }

        private string FormatBytes(long bytes)
        {
            var size = (double)bytes;
            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            var index = 0;

            while (size >= 1024 && index < units.Length - 1)
            {
                size /= 1024;
                index++;
            }

            return $"{size:F2} {units[index]}";
        }
    }
}
