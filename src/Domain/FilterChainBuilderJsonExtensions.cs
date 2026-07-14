#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="FilterChainBuilder"/> instances.
/// </summary>
/// <remarks>
/// Serializes the builder's internal state so that a chain can be reconstructed
/// from JSON without losing metadata such as <see cref="FilterChain.AllowParallelExecution"/>,
/// <see cref="FilterChain.MaxParallelSteps"/>, and <see cref="FilterChain.CacheIntermediateResults"/>.
/// </remarks>
public static class FilterChainBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions _indentedJsonOptions = new(_jsonOptions)
    {
        WriteIndented = true
    };

    /// <summary>
    /// Serializes the <see cref="FilterChainBuilder"/> to a JSON string.
    /// </summary>
    /// <param name="value">The builder instance to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for human readability.</param>
    /// <returns>A JSON representation of the builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this FilterChainBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? _indentedJsonOptions : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="FilterChainBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized builder, or <see langword="null"/> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static FilterChainBuilder? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<FilterChainBuilder>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="FilterChainBuilder"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized builder if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out FilterChainBuilder? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<FilterChainBuilder>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}