#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="Image"/> entities.
/// </summary>
public static class ImageValidation
{
    /// <summary>
    /// Validates an image and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The image to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the image is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Image value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.FilePath))
        {
            problems.Add("FilePath is required and cannot be empty or whitespace.");
        }
        else
        {
            // Validate file path length
            if (value.FilePath.Length > AppConstants.FileSystem.MaxFileNameLength)
            {
                problems.Add($"FilePath exceeds maximum length of {AppConstants.FileSystem.MaxFileNameLength} characters.");
            }

            // Validate file path characters (basic check)
            if (value.FilePath.Any(c => Path.GetInvalidPathChars().Contains(c)))
            {
                problems.Add("FilePath contains invalid path characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(value.FileName))
        {
            problems.Add("FileName is required and cannot be empty or whitespace.");
        }
        else
        {
            // Validate file name length
            if (value.FileName.Length > AppConstants.FileSystem.MaxFileNameLength)
            {
                problems.Add($"FileName exceeds maximum length of {AppConstants.FileSystem.MaxFileNameLength} characters.");
            }

            // Validate file name characters
            if (value.FileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            {
                problems.Add("FileName contains invalid file name characters.");
            }
        }

        // Validate image format
        if (value.Format == ImageFormat.Unknown)
        {
            problems.Add("Format must be a valid ImageFormat value other than Unknown.");
        }

        // Validate color space
        if (value.ColorSpace == ColorSpace.Unknown)
        {
            problems.Add("ColorSpace must be a valid ColorSpace value other than Unknown.");
        }

        // Validate dimensions
        if (value.Width < AppConstants.Processing.MinImageWidth)
        {
            problems.Add($"Width must be at least {AppConstants.Processing.MinImageWidth} pixels, but was {value.Width}.");
        }
        else if (value.Width > AppConstants.Processing.MaxImageWidth)
        {
            problems.Add($"Width must not exceed {AppConstants.Processing.MaxImageWidth} pixels, but was {value.Width}.");
        }

        if (value.Height < AppConstants.Processing.MinImageHeight)
        {
            problems.Add($"Height must be at least {AppConstants.Processing.MinImageHeight} pixels, but was {value.Height}.");
        }
        else if (value.Height > AppConstants.Processing.MaxImageHeight)
        {
            problems.Add($"Height must not exceed {AppConstants.Processing.MaxImageHeight} pixels, but was {value.Height}.");
        }

        // Validate channels
        if (value.Channels < 1)
        {
            problems.Add($"Channels must be at least 1, but was {value.Channels}.");
        }

        // Validate bits per pixel
        if (value.BitsPerPixel is not (8 or 16 or 24 or 32))
        {
            problems.Add("BitsPerPixel must be one of: 8, 16, 24, or 32.");
        }

        // Validate file size
        if (value.FileSizeBytes > AppConstants.Memory.MaxMemoryPerImage)
        {
            problems.Add($"FileSizeBytes must not exceed {AppConstants.Memory.MaxMemoryPerImage} bytes, but was {value.FileSizeBytes}.");
        }
        else if (value.FileSizeBytes < 0)
        {
            problems.Add("FileSizeBytes cannot be negative.");
        }

        // Validate pixel data size consistency
        if (value.PixelData is not null)
        {
            var calculatedSize = value.CalculatePixelDataSize();
            if (value.FileSizeBytes > 0 && value.FileSizeBytes < calculatedSize)
            {
                problems.Add("FileSizeBytes is smaller than calculated pixel data size.");
            }
        }

        // Validate timestamps
        var now = DateTime.UtcNow;
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > now.AddMinutes(5))
        {
            problems.Add("CreatedAt cannot be in the future.");
        }

        if (value.ModifiedAt == default)
        {
            problems.Add("ModifiedAt must be set to a valid DateTime.");
        }
        else if (value.ModifiedAt > now.AddMinutes(5))
        {
            problems.Add("ModifiedAt cannot be in the future.");
        }
        else if (value.ModifiedAt < value.CreatedAt)
        {
            problems.Add("ModifiedAt cannot be earlier than CreatedAt.");
        }

        // Validate metadata dictionary
        if (value.Metadata is null)
        {
            problems.Add("Metadata dictionary cannot be null.");
        }

        // Validate status is within valid range (0-5 based on the enum values)
        if (!Enum.IsDefined(typeof(ProcessingStatus), value.Status))
        {
            problems.Add("Status must be a valid ProcessingStatus value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified image is valid.
    /// </summary>
    /// <param name="value">The image to check.</param>
    /// <returns><see langword="true"/> if the image is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this Image value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified image is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The image to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the image is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this Image value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Image validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)
                }");
        }
    }
}