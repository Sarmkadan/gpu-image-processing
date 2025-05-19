#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Publishes performance metrics to external monitoring systems.
    /// Supports Prometheus, InfluxDB, and custom HTTP endpoints.
    /// </summary>
    public class MetricsPublisher
    {
        private readonly List<MetricsEndpoint> _endpoints;
        private readonly HttpClient _httpClient;
        private readonly Queue<MetricEvent> _metricsBuffer;
        private readonly int _bufferSize;

        public MetricsPublisher(int bufferSize = 100)
        {
            _endpoints = new List<MetricsEndpoint>();
            _httpClient = new HttpClient();
            _metricsBuffer = new Queue<MetricEvent>();
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Registers a metrics endpoint for publishing
        /// </summary>
        public void RegisterEndpoint(string url, MetricsFormat format, string apiKey = null)
        {
            _endpoints.Add(new MetricsEndpoint
            {
                Url = url,
                Format = format,
                ApiKey = apiKey,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Records a metric event to be published
        /// </summary>
        public void RecordMetric(string name, double value, Dictionary<string, string> tags = null)
        {
            var metricEvent = new MetricEvent
            {
                Name = name,
                Value = value,
                Tags = tags ?? new Dictionary<string, string>(),
                Timestamp = DateTime.UtcNow
            };

            _metricsBuffer.Enqueue(metricEvent);

            // Auto-flush if buffer is full
            if (_metricsBuffer.Count >= _bufferSize)
                FlushAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Records timing metrics for operations
        /// </summary>
        public void RecordTiming(string operationName, TimeSpan duration, Dictionary<string, string> tags = null)
        {
            if (tags == null)
                tags = new Dictionary<string, string>();

            tags["operation"] = operationName;
            RecordMetric($"{operationName}_duration_ms", duration.TotalMilliseconds, tags);
        }

        /// <summary>
        /// Flushes all buffered metrics to endpoints
        /// </summary>
        public async Task FlushAsync()
        {
            var metrics = new List<MetricEvent>();
            while (_metricsBuffer.Count > 0)
                metrics.Add(_metricsBuffer.Dequeue());

            if (metrics.Count == 0)
                return;

            var publishTasks = new List<Task>();

            foreach (var endpoint in _endpoints)
            {
                if (!endpoint.IsActive)
                    continue;

                publishTasks.Add(PublishToEndpointAsync(endpoint, metrics));
            }

            await Task.WhenAll(publishTasks);
        }

        private async Task PublishToEndpointAsync(MetricsEndpoint endpoint, List<MetricEvent> metrics)
        {
            try
            {
                var content = FormatMetrics(metrics, endpoint.Format);

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };

                if (!string.IsNullOrEmpty(endpoint.ApiKey))
                    request.Headers.Add("Authorization", $"Bearer {endpoint.ApiKey}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log error but don't throw - metrics publishing should not crash the application
                Console.Error.WriteLine($"Failed to publish metrics to {endpoint.Url}: {ex.Message}");
            }
        }

        private string FormatMetrics(List<MetricEvent> metrics, MetricsFormat format)
        {
            return format switch
            {
                MetricsFormat.Json => FormatAsJson(metrics),
                MetricsFormat.Prometheus => FormatAsPrometheus(metrics),
                MetricsFormat.InfluxDb => FormatAsInfluxDb(metrics),
                _ => throw new InvalidOperationException($"Unknown format: {format}")
            };
        }

        private string FormatAsJson(List<MetricEvent> metrics)
        {
            var json = new StringBuilder();
            json.Append("[");

            for (int i = 0; i < metrics.Count; i++)
            {
                var metric = metrics[i];
                json.Append("{");
                json.Append($"\"name\":\"{metric.Name}\",");
                json.Append($"\"value\":{metric.Value},");
                json.Append($"\"timestamp\":\"{metric.Timestamp:O}\"");

                if (metric.Tags.Count > 0)
                {
                    json.Append(",\"tags\":{");
                    var tagsList = new List<string>();
                    foreach (var tag in metric.Tags)
                        tagsList.Add($"\"{tag.Key}\":\"{tag.Value}\"");

                    json.Append(string.Join(",", tagsList));
                    json.Append("}");
                }

                json.Append("}");
                if (i < metrics.Count - 1)
                    json.Append(",");
            }

            json.Append("]");
            return json.ToString();
        }

        private string FormatAsPrometheus(List<MetricEvent> metrics)
        {
            var lines = new List<string>();

            foreach (var metric in metrics)
            {
                var tagString = "";
                if (metric.Tags.Count > 0)
                {
                    var tags = new List<string>();
                    foreach (var tag in metric.Tags)
                        tags.Add($"{tag.Key}=\"{tag.Value}\"");

                    tagString = "{" + string.Join(",", tags) + "}";
                }

                lines.Add($"{metric.Name}{tagString} {metric.Value}");
            }

            return string.Join("\n", lines);
        }

        private string FormatAsInfluxDb(List<MetricEvent> metrics)
        {
            var lines = new List<string>();

            foreach (var metric in metrics)
            {
                var tags = "";
                if (metric.Tags.Count > 0)
                {
                    var tagPairs = new List<string>();
                    foreach (var tag in metric.Tags)
                        tagPairs.Add($"{tag.Key}={tag.Value}");

                    tags = "," + string.Join(",", tagPairs);
                }

                var timestamp = (long)metric.Timestamp.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
                lines.Add($"{metric.Name}{tags} value={metric.Value} {timestamp}");
            }

            return string.Join("\n", lines);
        }

        public async ValueTask DisposeAsync()
        {
            await FlushAsync();
            _httpClient.Dispose();
        }

        private class MetricsEndpoint
        {
            public string Url { get; set; }
            public MetricsFormat Format { get; set; }
            public string ApiKey { get; set; }
            public bool IsActive { get; set; }
            public DateTime RegisteredAt { get; set; }
        }

        private class MetricEvent
        {
            public string Name { get; set; }
            public double Value { get; set; }
            public Dictionary<string, string> Tags { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }

    public enum MetricsFormat
    {
        Json,
        Prometheus,
        InfluxDb
    }
}
