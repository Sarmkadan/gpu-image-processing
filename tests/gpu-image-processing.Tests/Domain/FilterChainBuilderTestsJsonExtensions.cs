#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="FilterChainBuilderTests"/>.
/// </summary>
public static class FilterChainBuilderTestsJsonExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Converts a <see cref="FilterChainBuilderTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="FilterChainBuilderTests"/> instance to convert.</param>
    /// <param name="indented">Optional; if <c>true</c>, the JSON is formatted with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="FilterChainBuilderTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this FilterChainBuilderTests value, bool indented = false) =>
        JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true } : JsonOptions);

    /// <summary>
    /// Creates a <see cref="FilterChainBuilderTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="FilterChainBuilderTests"/> instance deserialized from the JSON string, or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
    public static FilterChainBuilderTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<FilterChainBuilderTests>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to create a <see cref="FilterChainBuilderTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="FilterChainBuilderTests"/> instance, or <c>null</c> if deserialization fails.</param>
    /// <returns><c>true</c> if deserialization succeeds; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
    public static bool TryFromJson(string json, out FilterChainBuilderTests? value)
    {
        value = null;
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<FilterChainBuilderTests>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
