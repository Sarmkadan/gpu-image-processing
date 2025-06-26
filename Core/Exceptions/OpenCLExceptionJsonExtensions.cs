#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Provides System.Text.Json serialization and deserialization extensions for OpenCLException
    /// </summary>
    public static class OpenCLExceptionJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes the OpenCLException to a JSON string
        /// </summary>
        /// <param name="value">The exception to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>A JSON string representation of the exception</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this OpenCLException value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes an OpenCLException from a JSON string
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized OpenCLException, or null if the JSON is invalid</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static OpenCLException? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<OpenCLException>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize an OpenCLException from a JSON string
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized exception if successful</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static bool TryFromJson(string json, out OpenCLException? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<OpenCLException>(json, _jsonOptions);
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
