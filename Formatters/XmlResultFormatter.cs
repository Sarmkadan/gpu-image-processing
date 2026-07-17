#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Formatter for converting processing results to XML format.
    /// Provides structured XML output with proper schema and formatting.
    /// </summary>
    public class XmlResultFormatter : IResultFormatter
    {
        private readonly bool _prettyPrint;

        public XmlResultFormatter(bool prettyPrint = true)
        {
            _prettyPrint = prettyPrint;
        }

        /// <summary>
        /// Gets the file extension for this format.
        /// </summary>
        public string GetFileExtension()
        {
            return ".xml";
        }

        /// <summary>
        /// Gets the MIME type for this format.
        /// </summary>
        public string GetMimeType()
        {
            return "application/xml";
        }

        /// <summary>
        /// Formats a single processing result to XML.
        /// </summary>
        public string FormatResult(ProcessingResult result)
        {
            if (result == null)
                return CreateErrorElement("No result provided").ToString();

            var root = new XElement("ProcessingResult",
                new XAttribute("id", result.Id),
                new XElement("JobId", result.JobId),
                new XElement("ImageId", result.ImageId),
                new XElement("Status", result.Status),
                new XElement("StartTime", result.StartTime?.ToString("O")),
                new XElement("CompletionTime", result.CompletionTime?.ToString("O")),
                new XElement("DurationMs", (long)((result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0)),
                new XElement("OutputImagePath", result.OutputImagePath),
                new XElement("ProcessedSize", result.ProcessedSize)
            );

            return FormatXml(root);
        }

        /// <summary>
        /// Formats multiple processing results to XML.
        /// </summary>
        public string FormatResults(List<ProcessingResult> results)
        {
            var root = new XElement("ProcessingResults");

            if (results != null)
            {
                foreach (var result in results)
                {
                    long durationMs = (long)((result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0);
                    var resultElement = new XElement("Result",
                        new XAttribute("id", result.Id),
                        new XElement("JobId", result.JobId),
                        new XElement("ImageId", result.ImageId),
                        new XElement("Status", result.Status),
                        new XElement("StartTime", result.StartTime?.ToString("O")),
                        new XElement("CompletionTime", result.CompletionTime?.ToString("O")),
                        new XElement("DurationMs", durationMs),
                        new XElement("OutputImagePath", result.OutputImagePath)
                    );

                    root.Add(resultElement);
                }
            }

            root.Add(new XAttribute("count", results?.Count ?? 0));
            return FormatXml(root);
        }

        /// <summary>
        /// Formats job information to XML.
        /// </summary>
        public string FormatJob(ProcessingJob job)
        {
            if (job == null)
                return CreateErrorElement("No job provided").ToString();

            double completionPercent = (job.ProcessedImages / (double)job.TotalImages) * 100;

            var root = new XElement("ProcessingJob",
                new XAttribute("id", job.Id),
                new XElement("Name", job.Name),
                new XElement("Status", job.Status),
                new XElement("TotalImages", job.TotalImages),
                new XElement("ProcessedImages", job.ProcessedImages),
                new XElement("FailedImages", job.FailedImages),
                new XElement("CompletionPercent", completionPercent.ToString("F2")),
                new XElement("CreatedAt", job.CreatedAt.ToString("O")),
                new XElement("StartedAt", job.StartedAt?.ToString("O")),
                new XElement("CompletedAt", job.CompletedAt?.ToString("O"))
            );

            return FormatXml(root);
        }

        /// <summary>
        /// Formats device information to XML.
        /// </summary>
        public string FormatDevice(DeviceInfo device)
        {
            if (device == null)
                return CreateErrorElement("No device provided").ToString();

            double memoryGb = device.MemoryBytes / (1024.0 * 1024.0 * 1024.0);

            var root = new XElement("Device",
                new XAttribute("id", device.Id),
                new XElement("Name", device.Name),
                new XElement("Type", device.Type),
                new XElement("Vendor", device.Vendor),
                new XElement("MemoryBytes", device.MemoryBytes),
                new XElement("MemoryGB", memoryGb.ToString("F2")),
                new XElement("ComputeUnits", device.ComputeUnits),
                new XElement("Available", device.IsAvailable),
                new XElement("DriverVersion", device.DriverVersion)
            );

            if (device.Extensions != null && device.Extensions.Count > 0)
            {
                var extensionsElement = new XElement("Extensions");
                foreach (var ext in device.Extensions)
                {
                    extensionsElement.Add(new XElement("Extension", ext));
                }
                root.Add(extensionsElement);
            }

            return FormatXml(root);
        }

        /// <summary>
        /// Formats error information to XML.
        /// </summary>
        public string FormatError(string errorMessage, string errorCode = null, Exception exception = null)
        {
            var root = new XElement("Error",
                new XElement("Code", errorCode ?? "UNKNOWN_ERROR"),
                new XElement("Message", errorMessage),
                new XElement("Timestamp", DateTime.UtcNow.ToString("O"))
            );

            if (exception != null)
            {
                root.Add(new XElement("ExceptionMessage", exception.Message));
                root.Add(new XElement("StackTrace", exception.StackTrace));
            }

            return FormatXml(root);
        }

        /// <summary>
        /// Formats an XElement with proper indentation if enabled.
        /// </summary>
        internal string FormatXml(XElement element)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = _prettyPrint,
                    ConformanceLevel = ConformanceLevel.Document,
                    OmitXmlDeclaration = false,
                    Encoding = System.Text.Encoding.UTF8
                };

                using (var writer = XmlWriter.Create(new System.IO.StringWriter(), settings))
                {
                    element.WriteTo(writer);
                    writer.Flush();
                    return writer.ToString();
                }
            }
            catch
            {
                // Fallback to non-pretty format if formatting fails
                return element.ToString();
            }
        }

        /// <summary>
        /// Creates an error element for error responses.
        /// </summary>
        private XElement CreateErrorElement(string message)
        {
            return new XElement("Error",
                new XElement("Message", message),
                new XElement("Timestamp", DateTime.UtcNow.ToString("O"))
            );
        }
    }
}
