#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="ComputeShaderPipelineOptions"/>.
/// </summary>
public static class ComputeShaderPipelineOptionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts the <see cref="ComputeShaderPipelineOptions"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The options instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ComputeShaderPipelineOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="ComputeShaderPipelineOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized options instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ComputeShaderPipelineOptions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ComputeShaderPipelineOptions>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="ComputeShaderPipelineOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized options if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ComputeShaderPipelineOptions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ComputeShaderPipelineOptions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}