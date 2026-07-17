#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ImageDomainTests"/>.
/// </summary>
public static class ImageDomainTestsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>
    /// Converts an <see cref="ImageDomainTests"/> instance to JSON.
    /// </summary>
    /// <param name="value">The <see cref="ImageDomainTests"/> instance to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON representation of the <see cref="ImageDomainTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ImageDomainTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(JsonOptions) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Creates an <see cref="ImageDomainTests"/> instance from JSON.
    /// </summary>
    /// <param name="json">The JSON to deserialize.</param>
    /// <returns>The deserialized <see cref="ImageDomainTests"/> instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
    public static ImageDomainTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ImageDomainTests>(json, JsonOptions);
    }

    /// <summary>
    /// Attempts to create an <see cref="ImageDomainTests"/> instance from JSON.
    /// </summary>
    /// <param name="json">The JSON to deserialize.</param>
    /// <param name="value">The deserialized <see cref="ImageDomainTests"/> instance, or null if deserialization fails.</param>
    /// <returns>True if the JSON was successfully deserialized; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
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