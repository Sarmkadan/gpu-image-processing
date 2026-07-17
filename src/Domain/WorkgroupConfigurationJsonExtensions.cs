#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides JSON serialization helpers for <see cref="WorkgroupConfiguration"/>.
/// </summary>
public static class WorkgroupConfigurationJsonExtensions
{
	/// <summary>
	/// Gets the JSON serialization options configured with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

	/// <summary>
	/// Serializes a <see cref="WorkgroupConfiguration"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="WorkgroupConfiguration"/> to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the object.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
public static string ToJson(this WorkgroupConfiguration value, bool indented = false)
{
	ArgumentNullException.ThrowIfNull(value);

	JsonOptions.WriteIndented = indented;
	return JsonSerializer.Serialize(value, JsonOptions);
}

	/// <summary>
	/// Deserializes a <see cref="WorkgroupConfiguration"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="WorkgroupConfiguration"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty, whitespace, or whitespace-only.</exception>
	/// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
	public static WorkgroupConfiguration? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json.Trim());

		return JsonSerializer.Deserialize<WorkgroupConfiguration?>(json, JsonOptions)
			?? throw new JsonException("Failed to deserialize WorkgroupConfiguration");
	}

	/// <summary>
	/// Tries to deserialize a <see cref="WorkgroupConfiguration"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized <see cref="WorkgroupConfiguration"/> if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty, whitespace, or whitespace-only.</exception>
	public static bool TryFromJson(string json, out WorkgroupConfiguration? value)
	{
		try
		{
			ArgumentNullException.ThrowIfNull(json);
			ArgumentException.ThrowIfNullOrEmpty(json.Trim());

			value = JsonSerializer.Deserialize<WorkgroupConfiguration?>(json, JsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}