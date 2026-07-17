#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ComputeShaderPass"/>.
/// </summary>
public static class ComputeShaderPassExtensionsJsonExtensions
{
    /// <summary>
    /// Gets the shared JSON serialization options configured for <see cref="ComputeShaderPass"/>.
    /// Uses camelCase property naming and includes <see cref="JsonStringEnumConverter"/>.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes a <see cref="ComputeShaderPass"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ComputeShaderPass"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ComputeShaderPass value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonOptions.WriteIndented = indented;
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    /// <summary>
    /// Deserializes a <see cref="ComputeShaderPass"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ComputeShaderPass"/> instance, or <see langword="null"/> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of whitespace.</exception>
    /// <exception cref="JsonException">JSON parsing fails or the JSON does not represent a valid <see cref="ComputeShaderPass"/>.</exception>
    public static ComputeShaderPass? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        return JsonSerializer.Deserialize<ComputeShaderPass>(json, JsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ComputeShaderPass"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="ComputeShaderPass"/> if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out ComputeShaderPass? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ComputeShaderPass>(json.Trim(), JsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}