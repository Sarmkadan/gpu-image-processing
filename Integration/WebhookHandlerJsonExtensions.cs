#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
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
        /// <returns>A JSON string representation of the webhook handler state.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this WebhookHandler value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var state = new WebhookHandlerState
            {
                ActiveWebhookCount = value.GetActiveWebhookCount(),
                TotalWebhookCount = value.GetWebhooks().Count,
                Subscriptions = value.GetWebhooks()
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
        /// <param name="logger">The logger to inject into the deserialized handler.</param>
        /// <returns>A deserialized <see cref="WebhookHandler"/> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="json"/> is null or empty.
        /// Thrown when <paramref name="logger"/> is null.
        /// </exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static WebhookHandler? FromJson(string json, ILogger<WebhookHandler> logger)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            ArgumentNullException.ThrowIfNull(logger);

            var state = JsonSerializer.Deserialize<WebhookHandlerState>(json, _jsonOptions)
                       ?? throw new JsonException("Failed to deserialize webhook handler state");

            var handler = new WebhookHandler(logger);

            // Note: Webhook subscriptions are not restored as they represent runtime state
            // This allows serialization to capture the current state without persisting subscriptions
            return handler;
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="WebhookHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="logger">The logger to inject into the deserialized handler.</param>
        /// <param name="value">The deserialized <see cref="WebhookHandler"/> instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="json"/> is null or empty.
        /// Thrown when <paramref name="logger"/> is null.
        /// </exception>
        public static bool TryFromJson(string json, ILogger<WebhookHandler> logger, out WebhookHandler? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            ArgumentNullException.ThrowIfNull(logger);

            try
            {
                value = FromJson(json, logger);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private sealed class WebhookHandlerState
        {
            public int ActiveWebhookCount { get; set; }
            public int TotalWebhookCount { get; set; }
            public List<WebhookSubscription>? Subscriptions { get; set; }
        }
    }
}