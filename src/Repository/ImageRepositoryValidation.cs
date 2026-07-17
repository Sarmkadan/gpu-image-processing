#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Provides validation helpers for <see cref="ImageRepository"/> instances.
/// </summary>
public static class ImageRepositoryValidation
{
    /// <summary>
    /// Validates the repository and all its images.
    /// </summary>
    /// <param name="repository">The repository to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ImageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var problems = new List<string>();

        // Validate repository state
        lock (repository.GetLockObject())
        {
            var allImages = repository.GetStorage();

            if (allImages == null)
            {
                problems.Add("Repository storage is null.");
                return problems.AsReadOnly();
            }

            // Validate each image
            foreach (var image in allImages)
            {
                if (image == null)
                {
                    problems.Add("Repository contains a null image.");
                    continue;
                }

                ValidateImage(image, problems);
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the repository and all its images are valid.
    /// </summary>
    /// <param name="repository">The repository to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> is null.</exception>
    public static bool IsValid(this ImageRepository repository)
    {
        return Validate(repository).Count == 0;
    }

    /// <summary>
    /// Ensures the repository and all its images are valid.
    /// </summary>
    /// <param name="repository">The repository to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid(this ImageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var problems = Validate(repository);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ImageRepository validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    private static void ValidateImage(Image image, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(problems);

        // Validate image ID
        if (image.Id == Guid.Empty)
        {
            problems.Add($"Image has an empty Guid ID.");
        }

        // Validate file path
        if (string.IsNullOrWhiteSpace(image.FilePath))
        {
            problems.Add($"Image with ID '{image.Id}' has a null, empty, or whitespace file path.");
        }
        else if (image.FilePath.Length > AppConstants.FileSystem.MaxFileNameLength)
        {
            problems.Add($"Image '{image.FileName}' has a file path longer than {AppConstants.FileSystem.MaxFileNameLength} characters (length: {image.FilePath.Length}).");
        }

        // Validate file name
        if (string.IsNullOrWhiteSpace(image.FileName))
        {
            problems.Add($"Image with ID '{image.Id}' has a null, empty, or whitespace file name.");
        }
        else if (image.FileName.Length > AppConstants.FileSystem.MaxFileNameLength)
        {
            problems.Add($"Image '{image.FileName}' has a file name longer than {AppConstants.FileSystem.MaxFileNameLength} characters (length: {image.FileName.Length}).");
        }

        // Validate image format
        if (image.Format == ImageFormat.Unknown)
        {
            problems.Add($"Image '{image.FileName}' has ImageFormat.Unknown, which is not a valid format.");
        }

        // Validate dimensions
        if (image.Width < AppConstants.Processing.MinImageWidth || image.Width > AppConstants.Processing.MaxImageWidth)
        {
            problems.Add($"Image '{image.FileName}' has invalid width {image.Width} (must be between {AppConstants.Processing.MinImageWidth} and {AppConstants.Processing.MaxImageWidth}).");
        }

        if (image.Height < AppConstants.Processing.MinImageHeight || image.Height > AppConstants.Processing.MaxImageHeight)
        {
            problems.Add($"Image '{image.FileName}' has invalid height {image.Height} (must be between {AppConstants.Processing.MinImageHeight} and {AppConstants.Processing.MaxImageHeight}).");
        }

        // Validate channels
        if (image.Channels < 1 || image.Channels > 4)
        {
            problems.Add($"Image '{image.FileName}' has invalid channel count {image.Channels} (must be between 1 and 4).");
        }

        // Validate bits per pixel
        if (image.BitsPerPixel is not (8 or 16 or 24 or 32))
        {
            problems.Add($"Image '{image.FileName}' has invalid bits per pixel {image.BitsPerPixel} (must be 8, 16, 24, or 32).");
        }

        // Validate file size
        if (image.FileSizeBytes > AppConstants.FileSystem.MaxFileSizeBytes)
        {
            problems.Add($"Image '{image.FileName}' has file size {image.FileSizeBytes:N0} bytes which exceeds the maximum allowed {AppConstants.FileSystem.MaxFileSizeBytes:N0} bytes.");
        }

        // Validate color space
        if (image.ColorSpace == ColorSpace.Unknown)
        {
            problems.Add($"Image '{image.FileName}' has ColorSpace.Unknown, which is not a valid color space.");
        }

        // Status validation is handled by the domain Image.Validate() method

        // Validate timestamps
        if (image.CreatedAt == default)
        {
            problems.Add($"Image '{image.FileName}' has a default CreatedAt date.");
        }
        else if (image.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add($"Image '{image.FileName}' has a CreatedAt date in the future (value: {image.CreatedAt:O}).");
        }

        if (image.ModifiedAt == default)
        {
            problems.Add($"Image '{image.FileName}' has a default ModifiedAt date.");
        }
        else if (image.ModifiedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add($"Image '{image.FileName}' has a ModifiedAt date in the future (value: {image.ModifiedAt:O}).");
        }
        else if (image.ModifiedAt < image.CreatedAt)
        {
            problems.Add($"Image '{image.FileName}' has a ModifiedAt date ({image.ModifiedAt:O}) that is earlier than CreatedAt ({image.CreatedAt:O}).");
        }

        // Validate metadata
        if (image.Metadata == null)
        {
            problems.Add($"Image '{image.FileName}' has null Metadata dictionary.");
        }
        else if (image.Metadata.Count > 100)
        {
            problems.Add($"Image '{image.FileName}' has more than 100 metadata entries (count: {image.Metadata.Count}).");
        }

        // Validate domain-level constraints using Image.Validate()
        if (!image.Validate())
        {
            problems.Add($"Image '{image.FileName}' failed domain-level validation.");
        }
    }

    private static object GetLockObject(this ImageRepository repository)
    {
        var lockField = typeof(ImageRepository).GetField(
            "_lockObject",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return lockField?.GetValue(repository) ?? throw new InvalidOperationException("Repository _lockObject field not found.");
    }

    private static List<Image> GetStorage(this ImageRepository repository)
    {
        var storageField = typeof(ImageRepository).GetField(
            "_storage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return storageField?.GetValue(repository) as List<Image> ?? throw new InvalidOperationException("Repository _storage field not found.");
    }
}