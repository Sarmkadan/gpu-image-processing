#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Provides JSON (de)serialization helpers for <see cref="HealthCheckWorker"/>.
    /// </summary>
    public static class HealthCheckWorkerJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the <see cref="HealthCheckWorker"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The worker instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be indented.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this HealthCheckWorker value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var opts = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
            return JsonSerializer.Serialize(value, opts);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="HealthCheckWorker"/> instance.
        /// </summary>
        /// <param name="json">The JSON representation of a <see cref="HealthCheckWorker"/>.</param>
        /// <returns>The deserialized <see cref="HealthCheckWorker"/>, or <c>null</c> if the JSON does not contain a valid object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
        public static HealthCheckWorker? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            return JsonSerializer.Deserialize<HealthCheckWorker>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="HealthCheckWorker"/> instance.
        /// </summary>
        /// <param name="json">The JSON representation of a <see cref="HealthCheckWorker"/>.</param>
        /// <param name="value">When this method returns, contains the deserialized instance if the operation succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        public static bool TryFromJson(string json, out HealthCheckWorker? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                value = JsonSerializer.Deserialize<HealthCheckWorker>(json, _options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
