using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Integration;

/// <summary>
/// Provides validation helpers for <see cref="RemoteImageResult"/> instances.
/// </summary>
public static class RemoteImageServiceValidation
{
	/// <summary>
	/// Validates the specified <see cref="RemoteImageResult"/> instance.
	/// </summary>
	/// <param name="value">The result instance to validate.</param>
	/// <returns>A list of human-readable validation problems; empty if valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this RemoteImageResult value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = new List<string>();

		// Validate IsSuccess and related fields
		if (!value.IsSuccess)
		{
			if (value.Data is not null)
			{
				errors.Add("Data must be null when IsSuccess is false.");
			}

			if (string.IsNullOrWhiteSpace(value.Error))
			{
				errors.Add("Error must be a non-empty string when IsSuccess is false.");
			}
		}
		else
		{
			if (value.Data is null)
			{
				errors.Add("Data must not be null when IsSuccess is true.");
			}
			else
			{
				errors.AddRange(value.Data.Validate());
			}
		}

		return errors.AsReadOnly();
	}

	/// <summary>
	/// Validates the specified <see cref="RemoteImageData"/> instance.
	/// </summary>
	/// <param name="value">The data instance to validate.</param>
	/// <returns>A list of human-readable validation problems; empty if valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this RemoteImageData value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = new List<string>();

		// Validate ImageData
		if (value.ImageData is null)
		{
			errors.Add("ImageData must not be null.");
		}
		else if (value.ImageData.Length == 0)
		{
			errors.Add("ImageData must not be empty.");
		}

		// Validate ContentType
		if (string.IsNullOrWhiteSpace(value.ContentType))
		{
			errors.Add("ContentType must be a non-empty string.");
		}
		else if (!value.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
		{
			errors.Add("ContentType must start with 'image/' prefix.");
		}

		// Validate SizeBytes
		if (value.SizeBytes < 0)
		{
			errors.Add("SizeBytes must be a non-negative number.");
		}

		// Validate SourceUrl
		if (!string.IsNullOrWhiteSpace(value.SourceUrl) && !Uri.TryCreate(value.SourceUrl, UriKind.Absolute, out _))
		{
			errors.Add("SourceUrl must be a valid absolute URI if specified.");
		}

		// Validate DownloadedAt
		if (value.DownloadedAt != default && value.DownloadedAt > DateTime.UtcNow)
		{
			errors.Add("DownloadedAt cannot be in the future.");
		}

		return errors.AsReadOnly();
	}

	/// <summary>
	/// Determines whether the specified <see cref="RemoteImageResult"/> instance is valid.
	/// </summary>
	/// <param name="value">The result instance to check.</param>
	/// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static bool IsValid(this RemoteImageResult value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return value.Validate().Count == 0;
	}

	/// <summary>
	/// Determines whether the specified <see cref="RemoteImageData"/> instance is valid.
	/// </summary>
	/// <param name="value">The data instance to check.</param>
	/// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static bool IsValid(this RemoteImageData value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return value.Validate().Count == 0;
	}

	/// <summary>
	/// Ensures that the specified <see cref="RemoteImageResult"/> instance is valid.
	/// </summary>
	/// <param name="value">The result instance to validate.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
	public static void EnsureValid(this RemoteImageResult value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = value.Validate();
		if (errors.Count > 0)
		{
			throw new ArgumentException(
				"RemoteImageResult is not valid. " +
				string.Join(" ", errors),
				nameof(value));
		}
	}

	/// <summary>
	/// Ensures that the specified <see cref="RemoteImageData"/> instance is valid.
	/// </summary>
	/// <param name="value">The data instance to validate.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
	public static void EnsureValid(this RemoteImageData value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var errors = value.Validate();
		if (errors.Count > 0)
		{
			throw new ArgumentException(
				"RemoteImageData is not valid. " +
				string.Join(" ", errors),
				nameof(value));
		}
	}
}