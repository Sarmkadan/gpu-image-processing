#nullable enable
// =============================================================================
// Author: 
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utility for exporting <see cref="PerformanceMetrics"/> instances to CSV.
    /// Produces a header row followed by one row per metrics instance.
    /// All numeric values are formatted using <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public static class PerformanceMetricsCsvExporter
    {
        /// <summary>
        /// Exports the provided metrics collection to a CSV string.
        /// </summary>
        /// <param name="metrics">The metrics to export.</param>
        /// <returns>A CSV string containing a header row and one row per metric.</returns>
        public static string Export(IEnumerable<PerformanceMetrics> metrics)
        {
            if (metrics == null)
                throw new ArgumentNullException(nameof(metrics));

            var sb = new StringBuilder();
            var type = typeof(PerformanceMetrics);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Header row
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Data rows
            foreach (var metric in metrics)
            {
                var values = properties.Select(p => FormatValue(p.GetValue(metric)));
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a property value using invariant culture when applicable.
        /// </summary>
        private static string FormatValue(object? value)
        {
            if (value == null)
                return string.Empty;

            if (value is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return value.ToString()!;
        }
    }
}
