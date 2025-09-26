#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Extension methods for <see cref="XmlResultFormatter"/> that provide additional XML formatting functionality.
    /// </summary>
    public static class XmlResultFormatterExtensions
    {
        /// <summary>
        /// Formats a collection of processing results to XML with additional metadata.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="results">Collection of processing results.</param>
        /// <param name="includeStatistics">Whether to include summary statistics.</param>
        /// <returns>Formatted XML string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter is null.</exception>
        public static string FormatResultsWithStatistics(
            this XmlResultFormatter formatter,
            IEnumerable<ProcessingResult> results,
            bool includeStatistics = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(results);

            var resultList = results.ToList();
            var root = new XElement("ProcessingResultsWithStats");

            // Add results
            var resultsElement = new XElement("Results");
            foreach (var result in resultList)
            {
                long durationMs = (long)((result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0);
                var resultElement = new XElement("Result",
                    new XAttribute("id", result.Id),
                    new XElement("JobId", result.JobId),
                    new XElement("ImageId", result.ImageId),
                    new XElement("Status", result.Status),
                    new XElement("StartTime", result.StartTime?.ToString("O")),
                    new XElement("DurationMs", durationMs),
                    new XElement("OutputImagePath", result.OutputImagePath)
                );
                resultsElement.Add(resultElement);
            }

            root.Add(resultsElement);

            // Add statistics if requested
            if (includeStatistics)
            {
                var stats = CalculateResultStatistics(resultList);
                root.Add(new XElement("Statistics",
                    new XElement("TotalResults", stats.TotalCount),
                    new XElement("Successful", stats.SuccessfulCount),
                    new XElement("Failed", stats.FailedCount),
                    new XElement("SuccessRate", stats.SuccessRate.ToString("P2", CultureInfo.InvariantCulture)),
                    new XElement("AverageDurationMs", stats.AverageDurationMs.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement("TotalDurationMs", stats.TotalDurationMs)
                ));
            }

            root.Add(new XAttribute("timestamp", DateTime.UtcNow.ToString("O")));
            root.Add(new XAttribute("count", resultList.Count));

            return formatter.FormatResults(resultList);
        }

        /// <summary>
        /// Formats a job with additional processing details and breakdown by status.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="job">The processing job.</param>
        /// <param name="includeBreakdown">Whether to include status breakdown.</param>
        /// <returns>Formatted XML string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or job is null.</exception>
        public static string FormatJobWithDetails(
            this XmlResultFormatter formatter,
            ProcessingJob job,
            bool includeBreakdown = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(job);

            var jobXml = formatter.FormatJob(job);

            if (!includeBreakdown)
            {
                return jobXml;
            }

            // Parse the generated XML to add breakdown
            var xDoc = XDocument.Parse(jobXml);
            var jobElement = xDoc.Root;

            if (jobElement != null)
            {
                var breakdown = new XElement("ProcessingBreakdown");
                breakdown.Add(new XElement("TotalImages", job.TotalImages));
                breakdown.Add(new XElement("ProcessedImages", job.ProcessedImages));
                breakdown.Add(new XElement("FailedImages", job.FailedImages));
                breakdown.Add(new XElement("PendingImages", job.TotalImages - job.ProcessedImages - job.FailedImages));

                if (job.TotalImages > 0)
                {
                    breakdown.Add(new XElement("CompletionRate",
                        (job.ProcessedImages / (double)job.TotalImages).ToString("P2", CultureInfo.InvariantCulture)));
                }

                jobElement.Add(breakdown);
            }

            return jobElement?.ToString() ?? jobXml;
        }

        /// <summary>
        /// Formats a device with extended capabilities information.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="device">The device information.</param>
        /// <param name="includeExtensions">Whether to include extension details.</param>
        /// <returns>Formatted XML string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or device is null.</exception>
        public static string FormatDeviceWithExtensions(
            this XmlResultFormatter formatter,
            DeviceInfo device,
            bool includeExtensions = true)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(device);

            var deviceXml = formatter.FormatDevice(device);

            if (!includeExtensions || device.Extensions == null || device.Extensions.Count == 0)
            {
                return deviceXml;
            }

            // Parse the generated XML to add extensions
            var xDoc = XDocument.Parse(deviceXml);
            var deviceElement = xDoc.Root;

            if (deviceElement != null)
            {
                var extensionsElement = new XElement("Extensions");
                foreach (var extension in device.Extensions)
                {
                    extensionsElement.Add(new XElement("Extension",
                        new XAttribute("name", extension.Key),
                        new XElement("Version", extension.Value)
                    ));
                }
                deviceElement.Add(extensionsElement);
            }

            return deviceElement?.ToString() ?? deviceXml;
        }

        /// <summary>
        /// Creates a standardized XML envelope around formatted content.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="contentType">Type of content being wrapped.</param>
        /// <param name="content">The XML content to wrap.</param>
        /// <param name="metadata">Optional metadata to include.</param>
        /// <returns>Formatted XML string with envelope.</returns>
        /// <exception cref="ArgumentNullException">Thrown when formatter or content is null.</exception>
        public static string WrapInEnvelope(
            this XmlResultFormatter formatter,
            string contentType,
            string content,
            IDictionary<string, string> metadata = null)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(contentType);
            ArgumentNullException.ThrowIfNull(content);

            var envelope = new XElement("Envelope",
                new XAttribute("version", "1.0"),
                new XAttribute("timestamp", DateTime.UtcNow.ToString("O")),
                new XElement("ContentType", contentType),
                new XElement("Content", XElement.Parse(content))
            );

            if (metadata != null && metadata.Count > 0)
            {
                var metadataElement = new XElement("Metadata");
                foreach (var kvp in metadata)
                {
                    metadataElement.Add(new XElement("Item",
                        new XAttribute("key", kvp.Key),
                        kvp.Value
                    ));
                }
                envelope.Add(metadataElement);
            }

            var result = new ProcessingResult
            {
                Id = Guid.NewGuid(),
                JobId = Guid.NewGuid(),
                ImageId = Guid.Empty,
                InputFilePath = string.Empty,
                OutputFilePath = string.Empty,
                IsSuccessful = true,
                Status = GpuImageProcessing.Core.Constants.ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow,
                CompletionTime = DateTime.UtcNow
            };

            return formatter.FormatResult(result);
        }

        #region Private Helper Methods

        private static ResultStatistics CalculateResultStatistics(IReadOnlyList<ProcessingResult> results)
        {
            if (results == null || results.Count == 0)
            {
                return new ResultStatistics
                {
                    TotalCount = 0,
                    SuccessfulCount = 0,
                    FailedCount = 0,
                    TotalDurationMs = 0,
                    AverageDurationMs = 0
                };
            }

            int successful = results.Count(r => r.IsSuccessful);
            int failed = results.Count - successful;
            double totalDuration = results.Sum(r => (r.CompletionTime - r.StartTime)?.TotalMilliseconds ?? 0);

            return new ResultStatistics
            {
                TotalCount = results.Count,
                SuccessfulCount = successful,
                FailedCount = failed,
                TotalDurationMs = (long)totalDuration,
                AverageDurationMs = results.Count > 0 ? totalDuration / results.Count : 0.0
            };
        }

        private sealed class ResultStatistics
        {
            public int TotalCount { get; set; }
            public int SuccessfulCount { get; set; }
            public int FailedCount { get; set; }
            public long TotalDurationMs { get; set; }
            public double AverageDurationMs { get; set; }
            public double SuccessRate => TotalCount > 0 ? (double)SuccessfulCount / TotalCount : 0;
        }
        #endregion
    }
}