using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="FilterConfigurationRepository"/>.
/// </summary>
public static class FilterConfigurationRepositoryJsonExtensions
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
    /// Serializes a <see cref="FilterConfigurationRepository"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The filter configuration repository to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the filter configuration repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this FilterConfigurationRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? _indentedJsonOptions : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="FilterConfigurationRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/> or empty/whitespace.</param>
    /// <returns>The deserialized filter configuration repository, or <see langword="null"/> if deserialization produces a null result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or consists only of whitespace.</exception>
    public static FilterConfigurationRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        return JsonSerializer.Deserialize<FilterConfigurationRepository>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="FilterConfigurationRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/> or empty/whitespace.</param>
    /// <param name="value">Receives the deserialized filter configuration repository if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or consists only of whitespace.</exception>
    public static bool TryFromJson(string json, out FilterConfigurationRepository? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json, nameof(json));

        try
        {
            value = JsonSerializer.Deserialize<FilterConfigurationRepository>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}