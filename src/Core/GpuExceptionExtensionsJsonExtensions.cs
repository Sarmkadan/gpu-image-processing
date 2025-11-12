#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core;

/// <summary>
/// Provides JSON serialization and deserialization helpers for GpuExceptionExtensions functionality.
/// </summary>
public static class GpuExceptionExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serialization marker for GpuExceptionExtensions configuration.
    /// </summary>
    public sealed class GpuExceptionExtensionsConfig
    {
        public string Type { get; } = "GpuExceptionExtensions";
        public bool IsTimeoutDetectionEnabled { get; init; } = true;
        public bool IsMemoryDetectionEnabled { get; init; } = true;
        public bool IsComputePipelineDetectionEnabled { get; init; } = true;
    }

    /// <summary>
    /// Converts extension method configuration to JSON representation.
    /// </summary>
    /// <param name="config">The configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
    public static string ToJson(GpuExceptionExtensionsConfig config, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(config);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Parses JSON to create extension method configuration.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized configuration, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static GpuExceptionExtensionsConfig? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GpuExceptionExtensionsConfig>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to parse JSON to create extension method configuration.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="config">Receives the deserialized configuration if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out GpuExceptionExtensionsConfig? config)
    {
        config = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            config = JsonSerializer.Deserialize<GpuExceptionExtensionsConfig>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
