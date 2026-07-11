#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities;

/// <summary>
/// Data transfer object for image processing configuration.
/// </summary>
public sealed record ImageProcessingConfiguration
{
    /// <summary>
    /// Gets or sets the minimum image width for processing.
    /// </summary>
    public int MinImageWidth { get; init; } = 16;

    /// <summary>
    /// Gets or sets the maximum image width for processing.
    /// </summary>
    public int MaxImageWidth { get; init; } = 8192;

    /// <summary>
    /// Gets or sets the minimum image height for processing.
    /// </summary>
    public int MinImageHeight { get; init; } = 16;

    /// <summary>
    /// Gets or sets the maximum image height for processing.
    /// </summary>
    public int MaxImageHeight { get; init; } = 8192;

    /// <summary>
    /// Gets or sets the slow operation threshold in milliseconds.
    /// </summary>
    public int SlowOperationThresholdMs { get; init; } = 5000;
}

/// <summary>
/// Provides System.Text.Json serialization extensions for image processing configuration.
/// </summary>
public static class ImageProcessingExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts image processing configuration to a JSON string.
    /// </summary>
    /// <param name="value">The configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ImageProcessingConfiguration value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes image processing configuration from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized configuration, or null if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ImageProcessingConfiguration? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ImageProcessingConfiguration>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize image processing configuration from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized configuration if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out ImageProcessingConfiguration? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ImageProcessingConfiguration>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}