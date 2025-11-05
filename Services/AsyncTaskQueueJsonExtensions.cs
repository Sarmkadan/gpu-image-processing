#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for AsyncTaskQueue
    /// </summary>
    public static class AsyncTaskQueueJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Converts the AsyncTaskQueue to a JSON string
        /// </summary>
        /// <param name="value">The task queue to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>JSON representation of the task queue</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this AsyncTaskQueue value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = true
                }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes an AsyncTaskQueue from JSON
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>The deserialized task queue, or null if JSON is null or empty</returns>
        /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
        public static AsyncTaskQueue? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AsyncTaskQueue>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize an AsyncTaskQueue from JSON
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="value">Output parameter for the deserialized task queue</param>
        /// <returns>True if deserialization succeeded; false otherwise</returns>
        public static bool TryFromJson(string json, out AsyncTaskQueue? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<AsyncTaskQueue>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}