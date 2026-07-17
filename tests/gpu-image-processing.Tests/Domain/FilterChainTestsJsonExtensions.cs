#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GpuImageProcessing.Tests.Domain;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="FilterChainTests"/>.
/// </summary>
public static class FilterChainTestsJsonExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Converts a <see cref="FilterChainTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="FilterChainTests"/> instance to convert.</param>
    /// <param name="indented">Optional; if <c>true</c>, the JSON will be formatted with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="FilterChainTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this FilterChainTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(s_jsonOptions) { WriteIndented = true }
            : s_jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Creates a <see cref="FilterChainTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="FilterChainTests"/> instance, or <c>null</c> if <paramref name="json"/> is <c>null</c> or empty.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is not a valid JSON string.</exception>
    public static FilterChainTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<FilterChainTests>(json, s_jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON string.", ex);
        }
    }

    /// <summary>
    /// Attempts to create a <see cref="FilterChainTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="FilterChainTests"/> instance if deserialization succeeds; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeds; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out FilterChainTests? value)
    {
        try
        {
            value = FromJson(json);
            return value is not null;
        }
        catch (ArgumentException)
        {
            value = null;
            return false;
        }
    }
}
