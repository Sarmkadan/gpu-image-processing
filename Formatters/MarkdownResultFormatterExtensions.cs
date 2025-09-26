using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides extension methods for <see cref="MarkdownResultFormatter"/> to enhance markdown formatting capabilities.
    /// </summary>
    public static class MarkdownResultFormatterExtensions
    {
        /// <summary>
        /// Formats a collection of results as a markdown table with automatic column detection.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="results">The results to format.</param>
        /// <returns>A markdown table representation of the results.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="results"/> is null.</exception>
        public static string FormatResultsAsTable(this MarkdownResultFormatter formatter, IReadOnlyList<object> results)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(results);

            if (results.Count == 0)
            {
                return "No results to display.";
            }

            // Get property names from first item
            var properties = new List<string>();
            foreach (var prop in results[0].GetType().GetProperties())
            {
                properties.Add(prop.Name);
            }

            var sb = new StringBuilder();

            // Header row
            sb.Append("| ");
            foreach (var prop in properties)
            {
                sb.Append(prop).Append(" | ");
            }
            sb.Length -= 3;
            sb.AppendLine();

            // Separator row
            sb.Append("| ");
            foreach (var prop in properties)
            {
                sb.Append("---").Append(" | ");
            }
            sb.Length -= 3;
            sb.AppendLine();

            // Data rows
            foreach (var result in results)
            {
                sb.Append("| ");
                foreach (var prop in properties)
                {
                    var value = result.GetType().GetProperty(prop)?.GetValue(result)?.ToString() ?? "";
                    sb.Append(value).Append(" | ");
                }
                sb.Length -= 3;
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a file path as a markdown link with appropriate icon based on file extension.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="filePath">The file path to format.</param>
        /// <param name="text">Optional display text. If null, uses the file name.</param>
        /// <returns>A markdown link with an appropriate icon.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is null or empty.</exception>
        public static string FormatFileLink(this MarkdownResultFormatter formatter, string filePath, string? text = null)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var displayText = text ?? fileName;

            var icon = extension switch
            {
                ".cs" => "📄",
                ".md" => "📝",
                ".json" => "📊",
                ".xml" => "📑",
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => "🖼️",
                ".zip" or ".tar" or ".gz" => "🗜️",
                ".exe" or ".dll" => "🔧",
                ".sql" => "🗄️",
                _ => "📁"
            };

            return $"[{icon} {displayText}]({filePath})";
        }

        /// <summary>
        /// Formats a collection of files as a markdown list with file icons and sizes.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="filePaths">The file paths to format.</param>
        /// <returns>A markdown list of files with metadata.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="formatter"/> or <paramref name="filePaths"/> is null.</exception>
        public static string FormatFileList(this MarkdownResultFormatter formatter, IReadOnlyList<string> filePaths)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(filePaths);

            if (filePaths.Count == 0)
            {
                return "No files to display.";
            }

            var sb = new StringBuilder();

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var fileInfo = new FileInfo(filePath);
                var size = fileInfo.Length;
                var sizeText = FormatFileSize(size);

                var icon = extension switch
                {
                    ".cs" => "📄",
                    ".md" => "📝",
                    ".json" => "📊",
                    ".xml" => "📑",
                    ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => "🖼️",
                    ".zip" or ".tar" or ".gz" => "🗜️",
                    ".exe" or ".dll" => "🔧",
                    ".sql" => "🗄️",
                    _ => "📁"
                };

                sb.Append($"- {icon} **[{fileName}]({filePath})** ({sizeText})\n");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a numeric value with units and appropriate precision based on magnitude.
        /// </summary>
        /// <param name="formatter">The formatter instance.</param>
        /// <param name="value">The numeric value to format.</param>
        /// <param name="unit">The unit of measurement.</param>
        /// <returns>A formatted string with value and unit.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is null.</exception>
        public static string FormatWithUnit(this MarkdownResultFormatter formatter, double value, string unit)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentException.ThrowIfNullOrEmpty(unit);

            return value switch
            {
                >= 1_000_000_000 => $"{value / 1_000_000_000:N2} {unit}",
                >= 1_000_000 => $"{value / 1_000_000:N2} {unit}",
                >= 1_000 => $"{value / 1_000:N2} {unit}",
                _ => $"{value:N2} {unit}"
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            double len = bytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:N2} {sizes[order]}";
        }
    }
}
