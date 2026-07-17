#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Utilities;

/// <summary>
/// Provides validation extension methods for image processing types.
/// </summary>
public static class ImageProcessingExtensionsValidation
{
    /// <summary>
    /// Validates an <see cref="ImageFormat"/> value.
    /// </summary>
    /// <param name="format">The image format to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown when format is <see cref="ImageFormat.Unknown"/>.</exception>
    public static IReadOnlyList<string> Validate(this ImageFormat format)
    {
        var problems = new List<string>();

        if (format == ImageFormat.Unknown)
        {
            problems.Add("Image format cannot be Unknown");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an <see cref="ImageFormat"/> is valid.
    /// </summary>
    /// <param name="format">The image format to check.</param>
    /// <returns>True if the format is valid; otherwise, false.</returns>
    public static bool IsValid(this ImageFormat format) => format.Validate().Count == 0;

    /// <summary>
    /// Ensures an <see cref="ImageFormat"/> is valid, throwing if not.
    /// </summary>
    /// <param name="format">The image format to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the format is invalid.</exception>
    public static void EnsureValid(this ImageFormat format)
    {
        var problems = format.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", problems), nameof(format));
        }
    }

    /// <summary>
    /// Validates a <see cref="FilterType"/> value.
    /// </summary>
    /// <param name="filterType">The filter type to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown when filterType is <see cref="FilterType.None"/>.</exception>
    public static IReadOnlyList<string> Validate(this FilterType filterType)
    {
        var problems = new List<string>();

        if (filterType == FilterType.None)
        {
            problems.Add("Filter type cannot be None");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="FilterType"/> is valid.
    /// </summary>
    /// <param name="filterType">The filter type to check.</param>
    /// <returns>True if the filter type is valid; otherwise, false.</returns>
    public static bool IsValid(this FilterType filterType) => filterType.Validate().Count == 0;

    /// <summary>
    /// Ensures a <see cref="FilterType"/> is valid, throwing if not.
    /// </summary>
    /// <param name="filterType">The filter type to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the filter type is invalid.</exception>
    public static void EnsureValid(this FilterType filterType)
    {
        var problems = filterType.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", problems), nameof(filterType));
        }
    }

    /// <summary>
    /// Validates a <see cref="Image"/> instance.
    /// </summary>
    /// <param name="image">The image to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when image is null.</exception>
    public static IReadOnlyList<string> Validate(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var problems = new List<string>();

        if (image.Width <= 0)
        {
            problems.Add("Image width must be positive");
        }

        if (image.Height <= 0)
        {
            problems.Add("Image height must be positive");
        }

        if (image.Channels <= 0)
        {
            problems.Add("Image channels must be positive");
        }

        if (image.FileSizeBytes <= 0)
        {
            problems.Add("Image file size must be positive");
        }

        if (string.IsNullOrWhiteSpace(image.FilePath))
        {
            problems.Add("Image file path cannot be null or whitespace");
        }

        if (image.Format == ImageFormat.Unknown)
        {
            problems.Add("Image format cannot be Unknown");
        }

        if (image.CreatedAt == default)
        {
            problems.Add("Image created date must be set");
        }

        if (image.ModifiedAt == default)
        {
            problems.Add("Image modified date must be set");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an <see cref="Image"/> instance is valid.
    /// </summary>
    /// <param name="image">The image to check.</param>
    /// <returns>True if the image is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when image is null.</exception>
    public static bool IsValid(this Image image) => Validate(image).Count == 0;

    /// <summary>
    /// Ensures an <see cref="Image"/> instance is valid, throwing if not.
    /// </summary>
    /// <param name="image">The image to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the image is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when image is null.</exception>
    public static void EnsureValid(this Image image)
    {
        var problems = Validate(image);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", problems), nameof(image));
        }
    }

    /// <summary>
    /// Validates a file extension string.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when extension is null.</exception>
    public static IReadOnlyList<string> Validate(this string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(extension))
        {
            problems.Add("File extension cannot be null or whitespace");
        }
        else if (!extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("File extension must start with a dot (.)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a file extension is valid.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <returns>True if the extension is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when extension is null.</exception>
    public static bool IsValid(this string extension) => Validate(extension).Count == 0;

    /// <summary>
    /// Ensures a file extension is valid, throwing if not.
    /// </summary>
    /// <param name="extension">The file extension to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the extension is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when extension is null.</exception>
    public static void EnsureValid(this string extension)
    {
        var problems = Validate(extension);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", problems), nameof(extension));
        }
    }
}