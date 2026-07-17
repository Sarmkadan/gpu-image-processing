#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for enumerable operations.
    /// </summary>
    public static class EnumerableExtensionsJsonExtensions
    {
        /// <summary>
        /// JSON serializer options with camelCase naming policy and default settings.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Serializes enumerable batching configuration to a JSON string.
        /// </summary>
        /// <param name="batchSize">The maximum number of items in each batch. Must be positive.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the batching configuration.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is not positive.</exception>
        public static string ToJson(int batchSize, bool indented = false)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

            var config = new EnumerableBatchingConfiguration
            {
                BatchSize = batchSize
            };

            var options = indented
                ? new JsonSerializerOptions(JsonOptions)
                { WriteIndented = true }
                : JsonOptions;

            return JsonSerializer.Serialize(config, options);
        }

        /// <summary>
        /// Deserializes a JSON string to enumerable batching configuration.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
        /// <returns>An enumerable batching configuration, or null if the JSON represents a null value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="json"/> is empty.</exception>
        /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
        public static EnumerableBatchingConfiguration? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<EnumerableBatchingConfiguration>(json, JsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to enumerable batching configuration.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
        /// <param name="value">Receives the deserialized configuration if successful.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out EnumerableBatchingConfiguration? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<EnumerableBatchingConfiguration>(json, JsonOptions);
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Represents a serializable configuration for enumerable batching operations.
        /// </summary>
        public sealed class EnumerableBatchingConfiguration
        {
            /// <summary>Gets or sets the maximum number of items in each batch.</summary>
            public int BatchSize { get; set; } = 10;
        }
    }
}
