#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for ProcessingProfile
    /// </summary>
    public static class ProcessingProfileJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Converts a ProcessingProfile instance to its JSON representation
        /// </summary>
        /// <param name="value">The ProcessingProfile instance to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability</param>
        /// <returns>A JSON string representation of the ProcessingProfile</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this ProcessingProfile value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
        }

        /// <summary>
        /// Parses a JSON string and creates a ProcessingProfile instance
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>A ProcessingProfile instance, or null if the JSON is invalid</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static ProcessingProfile? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ProcessingProfile>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to parse a JSON string and create a ProcessingProfile instance
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized ProcessingProfile if successful</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static bool TryFromJson(string json, out ProcessingProfile? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ProcessingProfile>(json, _jsonOptions);
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