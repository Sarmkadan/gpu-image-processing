#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Provides System.Text.Json serialization and deserialization extensions for <see cref="OpenCLException"/>
    /// and its derived exception types (<see cref="DeviceInitializationException"/> and <see cref="KernelCompilationException"/>).
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
        /// Serializes the <see cref="OpenCLException"/> to a JSON string using camelCase property naming and camelCase enum values.
        /// </summary>
        /// <param name="value">The exception to serialize. Must not be <see langword="null"/>.</param>
        /// <param name="indented">Whether to format the JSON with indentation for improved readability.</param>
        /// <returns>A JSON string representation of the exception.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this OpenCLException value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes an <see cref="OpenCLException"/> from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/> or empty.</param>
        /// <returns>The deserialized <see cref="OpenCLException"/>, or <see langword="null"/> if the JSON is invalid.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
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
        /// Attempts to deserialize an <see cref="OpenCLException"/> from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/> or empty.</param>
        /// <param name="value">Receives the deserialized exception if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
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
