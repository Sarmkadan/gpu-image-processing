#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="ApiResponse{T}"/>.
    /// </summary>
    public static class ApiResponseJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Serializes the <see cref="ApiResponse{T}"/> instance to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of data contained in the response.</typeparam>
        /// <param name="value">The API response to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the API response.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson<T>(this ApiResponse<T> value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an <see cref="ApiResponse{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of data contained in the response.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized API response, or null if the JSON is null or empty.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static ApiResponse<T>? FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an <see cref="ApiResponse{T}"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of data contained in the response.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized API response if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJson<T>(string json, out ApiResponse<T>? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}