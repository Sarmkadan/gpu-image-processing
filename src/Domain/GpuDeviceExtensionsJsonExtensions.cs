#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides System.Text.Json serialization helpers for GpuDeviceExtensions configuration.
/// </summary>
public static class GpuDeviceExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Configuration DTO for GpuDeviceExtensions extension methods.
    /// </summary>
    public sealed class GpuDeviceExtensionsConfig
    {
        /// <summary>
        /// Gets the type identifier for this configuration.
        /// </summary>
        public string Type { get; } = "GpuDeviceExtensions";

        /// <summary>
        /// Gets or sets whether memory extension methods are enabled.
        /// </summary>
        public bool IsMemoryExtensionsEnabled { get; init; } = true;

        /// <summary>
        /// Gets or sets whether color space detection is enabled.
        /// </summary>
        public bool IsColorSpaceDetectionEnabled { get; init; } = true;

        /// <summary>
        /// Gets or sets whether device type display name formatting is enabled.
        /// </summary>
        public bool IsDeviceTypeDisplayEnabled { get; init; } = true;

        /// <summary>
        /// Gets or sets the default memory unit for extension method outputs.
        /// </summary>
        public string DefaultMemoryUnit { get; init; } = "MB";
    }

    /// <summary>
    /// Serializes a <see cref="GpuDeviceExtensionsConfig"/> to a JSON string.
    /// </summary>
    /// <param name="value">The configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this GpuDeviceExtensionsConfig value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GpuDeviceExtensionsConfig"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized configuration, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static GpuDeviceExtensionsConfig? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GpuDeviceExtensionsConfig>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="GpuDeviceExtensionsConfig"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized configuration if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out GpuDeviceExtensionsConfig? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<GpuDeviceExtensionsConfig>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}