#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// JSON serialization helpers for ImageDomainTests.
/// </summary>
public static class ImageDomainTestsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts an ImageDomainTests instance to JSON.
    /// </summary>
    /// <param name="value">The ImageDomainTests instance to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON representation of the ImageDomainTests instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ImageDomainTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? JsonOptions : JsonSerializerOptions.Default;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Creates an ImageDomainTests instance from JSON.
    /// </summary>
    /// <param name="json">The JSON to deserialize.</param>
    /// <returns>The deserialized ImageDomainTests instance, or null if the JSON is empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static ImageDomainTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ImageDomainTests>(json, JsonOptions);
    }

    /// <summary>
    /// Tries to create an ImageDomainTests instance from JSON.
    /// </summary>
    /// <param name="json">The JSON to deserialize.</param>
    /// <param name="value">The deserialized ImageDomainTests instance, or null if the JSON is invalid.</param>
    /// <returns>True if the JSON was successfully deserialized; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out ImageDomainTests? value)
    {
        try
        {
            value = FromJson(json);
            return value != null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
