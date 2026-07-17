#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="BatchProcessingUtilities"/>
    /// </summary>
    public static class BatchProcessingUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes batch processing configuration to a JSON string.
        /// </summary>
        /// <param name="batchSize">The batch size to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the batch processing configuration.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="batchSize"/> is negative.</exception>
        public static string ToJson(int batchSize, bool indented = false)
        {
            if (batchSize < 0)
            {
                throw new ArgumentException("Batch size cannot be negative", nameof(batchSize));
            }

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            var config = new BatchProcessingConfiguration { BatchSize = batchSize };
            return JsonSerializer.Serialize(config, options);
        }

        /// <summary>
        /// Deserializes a batch processing configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized batch processing configuration, or null if JSON is null, empty, or whitespace.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static BatchProcessingConfiguration? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<BatchProcessingConfiguration>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a batch processing configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized configuration if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out BatchProcessingConfiguration? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                value = null;
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<BatchProcessingConfiguration>(json, _jsonSerializerOptions);
                return value is not null && value.BatchSize >= 0;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Represents a serializable batch processing configuration.
        /// </summary>
        public sealed class BatchProcessingConfiguration
        {
            /// <summary>
            /// Gets or sets the batch size.
            /// </summary>
            /// <value>The batch size must be non-negative.</value>
            public int BatchSize { get; set; }
        }
    }
}