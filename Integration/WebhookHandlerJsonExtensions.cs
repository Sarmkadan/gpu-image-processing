#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="WebhookHandler"/>.
    /// </summary>
    public static class WebhookHandlerJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes the <see cref="WebhookHandler"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The webhook handler instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the webhook handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this WebhookHandler value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var state = new WebhookHandlerState
            {
                ActiveWebhookCount = value.GetActiveWebhookCount()
            };

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(state, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="WebhookHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized <see cref="WebhookHandler"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static WebhookHandler? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            // WebhookHandler is a service class with no public state to deserialize,
            // so we create a new instance with default values
            return new WebhookHandler(null!);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="WebhookHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="WebhookHandler"/> instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out WebhookHandler? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        private sealed class WebhookHandlerState
        {
            public int ActiveWebhookCount { get; set; }
        }
    }
}
