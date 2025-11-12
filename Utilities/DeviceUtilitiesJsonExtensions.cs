#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for DeviceUtilities configuration.
    /// </summary>
    public static class DeviceUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a device configuration to a JSON string.
        /// </summary>
        /// <param name="value">The device configuration to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the device configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this DeviceConfiguration value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a device configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized DeviceConfiguration instance, or null if the JSON is invalid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static DeviceConfiguration? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<DeviceConfiguration>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a device configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized configuration if successful, otherwise null.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out DeviceConfiguration? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<DeviceConfiguration>(json, _jsonSerializerOptions);
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Represents a serializable device configuration for GPU devices.
    /// </summary>
    public sealed class DeviceConfiguration
    {
        /// <summary>Gets or sets the device name.</summary>
        public string? DeviceName { get; set; }

        /// <summary>Gets or sets the total global memory in bytes.</summary>
        public long GlobalMemoryBytes { get; set; }

        /// <summary>Gets or sets the number of compute units.</summary>
        public int ComputeUnits { get; set; }

        /// <summary>Gets or sets the maximum clock frequency in MHz.</summary>
        public int MaxClockFrequency { get; set; }

        /// <summary>Gets or sets the compute capability version (e.g., "3.0").</summary>
        public string? ComputeCapability { get; set; }

        /// <summary>Gets or sets the memory pressure level.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MemoryPressureLevel MemoryPressureLevel { get; set; }

        /// <summary>Gets or sets the recommended batch size.</summary>
        public int RecommendedBatchSize { get; set; }
    }
}