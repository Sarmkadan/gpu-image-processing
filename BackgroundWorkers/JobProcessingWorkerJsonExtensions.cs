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
        private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Converts a <see cref="JobProcessingWorker"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="JobProcessingWorker"/> to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>The JSON string representation of the <see cref="JobProcessingWorker"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this JobProcessingWorker value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (indented)
            {
                s_jsonOptions.WriteIndented = true;
            }

            return JsonSerializer.Serialize(value, s_jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="JobProcessingWorker"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="JobProcessingWorker"/> or null if the JSON is invalid.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static JobProcessingWorker? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<JobProcessingWorker>(json, s_jsonOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string to a <see cref="JobProcessingWorker"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="JobProcessingWorker"/> or null if the JSON is invalid.</param>
        /// <returns>true if the deserialization was successful; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out JobProcessingWorker? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<JobProcessingWorker>(json, s_jsonOptions);
                return value != null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
