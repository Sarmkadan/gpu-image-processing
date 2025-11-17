#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Provides System.Text.Json serialization helpers for BatchProcessingServiceTestsExtensions.
/// </summary>
public static class BatchProcessingServiceTestsExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Configuration and state for BatchProcessingServiceTestsExtensions extension methods.
    /// </summary>
    public sealed class BatchProcessingServiceTestsExtensions
    {
        /// <summary>
        /// Gets or sets the default number of images to include in test batches.
        /// </summary>
        public int DefaultImageCount { get; init; } = 3;

        /// <summary>
        /// Gets or sets the default number of filters to include in test batches.
        /// </summary>
        public int DefaultFilterCount { get; init; } = 2;

        /// <summary>
        /// Gets or sets a value indicating whether verbose test output is enabled.
        /// </summary>
        public bool EnableVerboseOutput { get; init; } = false;
    }

    /// <summary>
    /// Serializes a BatchProcessingServiceTestsExtensions configuration to a JSON string.
    /// </summary>
    /// <param name="value">The configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static string ToJson(this BatchProcessingServiceTestsExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a BatchProcessingServiceTestsExtensions instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown if json is null.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static BatchProcessingServiceTestsExtensions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BatchProcessingServiceTestsExtensions>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a BatchProcessingServiceTestsExtensions instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if json is null.</exception>
    public static bool TryFromJson(string json, out BatchProcessingServiceTestsExtensions? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<BatchProcessingServiceTestsExtensions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}