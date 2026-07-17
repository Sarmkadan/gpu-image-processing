#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GpuImageProcessing.BackgroundWorkers;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// JSON serialization helpers for <see cref="JobProcessingWorker"/>.
    /// </summary>
    public static class JobProcessingWorkerJsonExtensions
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Converts a <see cref="JobProcessingWorker"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="JobProcessingWorker"/> to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>The JSON string representation of the <see cref="JobProcessingWorker"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this JobProcessingWorker value, bool indented = false) =>
            JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : s_jsonOptions);

        /// <summary>
        /// Deserializes a JSON string to a <see cref="JobProcessingWorker"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="JobProcessingWorker"/> or <see langword="null"/> if the JSON is invalid.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
        public static JobProcessingWorker? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);
            return JsonSerializer.Deserialize<JobProcessingWorker>(json, s_jsonOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string to a <see cref="JobProcessingWorker"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="JobProcessingWorker"/> or <see langword="null"/> if the JSON is invalid.</param>
        /// <returns><see langword="true"/> if the deserialization was successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static bool TryFromJson(string json, out JobProcessingWorker? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<JobProcessingWorker>(json, s_jsonOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        private static JsonSerializerOptions GetIndentedOptions()
        {
            var options = new JsonSerializerOptions(s_jsonOptions)
            {
                WriteIndented = true
            };
            return options;
        }
    }
}