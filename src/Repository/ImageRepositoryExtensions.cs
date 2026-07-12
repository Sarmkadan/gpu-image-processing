#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Extension methods for <see cref="ImageRepository"/> providing additional query capabilities
/// for image-specific operations beyond the generic repository interface.
/// </summary>
public static class ImageRepositoryExtensions
{
    /// <summary>
    /// Gets images by file extension filter.
    /// </summary>
    /// <param name="repository">The image repository instance.</param>
    /// <param name="extension">The file extension to filter by (e.g., ".jpg", ".png").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of images matching the file extension.</returns>
    /// <exception cref="ArgumentNullException">Thrown when extension is null.</exception>
    public static Task<IReadOnlyList<Image>> GetByFileExtensionAsync(
        this ImageRepository repository,
        string extension,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(extension);

        return repository.GetByCriteriaAsync(
            i => i.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase),
            cancellationToken
        ).ContinueWith(t => (IReadOnlyList<Image>)t.Result.ToList());
    }

    /// <summary>
    /// Gets images by color space.
    /// </summary>
    /// <param name="repository">The image repository instance.</param>
    /// <param name="colorSpace">The color space to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of images with the specified color space.</returns>
    public static Task<IReadOnlyList<Image>> GetByColorSpaceAsync(
        this ImageRepository repository,
        ColorSpace colorSpace,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByCriteriaAsync(
            i => i.ColorSpace == colorSpace,
            cancellationToken
        ).ContinueWith(t => (IReadOnlyList<Image>)t.Result.ToList());
    }

    /// <summary>
    /// Gets images that exceed a specified file size threshold.
    /// </summary>
    /// <param name="repository">The image repository instance.</param>
    /// <param name="minFileSizeBytes">Minimum file size in bytes to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of images larger than the specified size.</returns>
    public static Task<IReadOnlyList<Image>> GetLargeImagesAsync(
        this ImageRepository repository,
        long minFileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByCriteriaAsync(
            i => i.FileSizeBytes >= minFileSizeBytes,
            cancellationToken
        ).ContinueWith(t => (IReadOnlyList<Image>)t.Result.ToList());
    }

    /// <summary>
    /// Gets images that match a specific aspect ratio within a tolerance.
    /// </summary>
    /// <param name="repository">The image repository instance.</param>
    /// <param name="targetAspectRatio">The target aspect ratio to match.</param>
    /// <param name="tolerance">Allowed deviation from target aspect ratio.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of images matching the aspect ratio criteria.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when tolerance is negative.</exception>
    public static Task<IReadOnlyList<Image>> GetByAspectRatioAsync(
        this ImageRepository repository,
        double targetAspectRatio,
        double tolerance = 0.01,
        CancellationToken cancellationToken = default)
    {
        if (tolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance cannot be negative");

        return repository.GetByCriteriaAsync(
            i => Math.Abs(i.GetAspectRatio() - targetAspectRatio) <= tolerance,
            cancellationToken
        ).ContinueWith(t => (IReadOnlyList<Image>)t.Result.ToList());
    }
}