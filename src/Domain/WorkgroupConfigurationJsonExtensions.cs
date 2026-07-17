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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
	public static string ToJson(this WorkgroupConfiguration value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		// Clone the static options to avoid mutating shared state (thread‑safety).
		var options = new JsonSerializerOptions(JsonOptions)
		{
			WriteIndented = indented
		};

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a <see cref="WorkgroupConfiguration"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="WorkgroupConfiguration"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
	/// <exception cref="JsonException">Thrown when JSON parsing fails or the JSON does not represent a valid <see cref="WorkgroupConfiguration"/>.</exception>
	public static WorkgroupConfiguration FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);
		if (string.IsNullOrWhiteSpace(json))
			throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));

		return JsonSerializer.Deserialize<WorkgroupConfiguration>(json, JsonOptions)
			?? throw new JsonException("Failed to deserialize WorkgroupConfiguration");
	}

	/// <summary>
	/// Tries to deserialize a <see cref="WorkgroupConfiguration"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">
	/// When this method returns, contains the deserialized <see cref="WorkgroupConfiguration"/> if the operation succeeded,
	/// or <c>null</c> if it failed.
	/// </param>
	/// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
	public static bool TryFromJson(string json, out WorkgroupConfiguration? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		if (string.IsNullOrWhiteSpace(json))
			throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));

		try
		{
			value = JsonSerializer.Deserialize<WorkgroupConfiguration>(json, JsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
