#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Integration
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for EndToEndProcessingTestsExtensions.
    /// </summary>
    public static class EndToEndProcessingTestsExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Represents the configuration and state of EndToEndProcessingTestsExtensions extension methods.
        /// </summary>
        public sealed class EndToEndProcessingTestsExtensions
        {
            /// <summary>
            /// Gets or sets whether all test methods should be executed.
            /// </summary>
            public bool IsRunAllTestsEnabled { get; init; } = true;

            /// <summary>
            /// Gets or sets whether test method names should be retrieved.
            /// </summary>
            public bool IsGetTestMethodNamesEnabled { get; init; } = true;

            /// <summary>
            /// Gets or sets whether test success checking should be enabled.
            /// </summary>
            public bool IsAllTestsPassEnabled { get; init; } = true;
        }

        /// <summary>
        /// Serializes an EndToEndProcessingTestsExtensions configuration to a JSON string.
        /// </summary>
        /// <param name="value">The configuration to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representing the configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this EndToEndProcessingTestsExtensions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an EndToEndProcessingTestsExtensions instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
        /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
        public static EndToEndProcessingTestsExtensions? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<EndToEndProcessingTestsExtensions>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                throw;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an EndToEndProcessingTestsExtensions instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out EndToEndProcessingTestsExtensions? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<EndToEndProcessingTestsExtensions>(json, _jsonOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}