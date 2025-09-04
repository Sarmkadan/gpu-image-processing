using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides extension methods for <see cref="MetricsPublisher"/>.
    /// </summary>
    public static class MetricsPublisherExtensions
    {
        /// <summary>
        /// Records a metric event with no tags.
        /// </summary>
        /// <param name="publisher">The publisher instance.</param>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value of the metric.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
        public static void RecordMetric(this MetricsPublisher publisher, string name, double value)
        {
            ArgumentNullException.ThrowIfNull(publisher);
            ArgumentException.ThrowIfNullOrEmpty(name);
            publisher.RecordMetric(name, value, null);
        }

        /// <summary>
        /// Records an error event.
        /// </summary>
        /// <param name="publisher">The publisher instance.</param>
        /// <param name="operation">The operation name.</param>
        /// <param name="message">The error message.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="operation"/> is null or empty.</exception>
        public static void RecordError(this MetricsPublisher publisher, string operation, string message)
        {
            ArgumentNullException.ThrowIfNull(publisher);
            ArgumentException.ThrowIfNullOrEmpty(operation);
            var tags = new Dictionary<string, string> { { "operation", operation }, { "status", "error" } };
            if (message != null)
            {
                tags["message"] = message;
            }
            publisher.RecordMetric($"{operation}_error_count", 1, tags);
        }

        /// <summary>
        /// Records a success event.
        /// </summary>
        /// <param name="publisher">The publisher instance.</param>
        /// <param name="operation">The operation name.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="operation"/> is null or empty.</exception>
        public static void RecordSuccess(this MetricsPublisher publisher, string operation)
        {
            ArgumentNullException.ThrowIfNull(publisher);
            ArgumentException.ThrowIfNullOrEmpty(operation);
            var tags = new Dictionary<string, string> { { "operation", operation }, { "status", "success" } };
            publisher.RecordMetric($"{operation}_success_count", 1, tags);
        }
    }
}
