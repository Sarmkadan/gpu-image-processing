#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Interface for output formatters that convert processing results to various formats.
    /// Supports JSON, CSV, XML, and other output formats.
    /// </summary>
    public interface IResultFormatter
    {
        /// <summary>
        /// Gets the file extension associated with this format.
        /// </summary>
        string GetFileExtension();

        /// <summary>
        /// Gets the MIME type for this format.
        /// </summary>
        string GetMimeType();

        /// <summary>
        /// Formats a single processing result.
        /// </summary>
        string FormatResult(ProcessingResult result);

        /// <summary>
        /// Formats multiple processing results.
        /// </summary>
        string FormatResults(List<ProcessingResult> results);

        /// <summary>
        /// Formats job information.
        /// </summary>
        string FormatJob(ProcessingJob job);

        /// <summary>
        /// Formats device information.
        /// </summary>
        string FormatDevice(DeviceInfo device);

        /// <summary>
        /// Formats error information.
        /// </summary>
        string FormatError(string errorMessage, string errorCode = null, Exception exception = null);
    }

    /// <summary>
    /// Factory for creating result formatters by format type.
    /// </summary>
    public class ResultFormatterFactory
    {
        /// <summary>
        /// Creates a formatter for the specified format type.
        /// </summary>
        public static IResultFormatter CreateFormatter(string format, bool prettyPrint = true)
        {
            return format.ToLower() switch
            {
                "json" => new JsonResultFormatter(prettyPrint),
                "csv" => new CsvResultFormatter(),
                "xml" => new XmlResultFormatter(prettyPrint),
                "text" or "txt" => new TextResultFormatter(),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
        }

        /// <summary>
        /// Gets all supported format names.
        /// </summary>
        public static List<string> GetSupportedFormats()
        {
            return new List<string> { "json", "csv", "xml", "text" };
        }

        /// <summary>
        /// Determines if a format is supported.
        /// </summary>
        public static bool IsFormatSupported(string format)
        {
            return GetSupportedFormats().Contains(format.ToLower());
        }

        /// <summary>
        /// Gets formatter from file extension.
        /// </summary>
        public static IResultFormatter CreateFromExtension(string fileExtension, bool prettyPrint = true)
        {
            string format = fileExtension.ToLower().TrimStart('.');

            return format switch
            {
                "json" => new JsonResultFormatter(prettyPrint),
                "csv" => new CsvResultFormatter(),
                "xml" => new XmlResultFormatter(prettyPrint),
                "txt" or "text" => new TextResultFormatter(),
                _ => throw new ArgumentException($"Unknown file extension: {fileExtension}")
            };
        }
    }
}
