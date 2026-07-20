#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Fallback;

/// <summary>
/// Defines the contract for processing images with a specific backend
/// (GPU via OpenCL or CPU fallback).
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Returns <see langword="true"/> when this processor supports the given filter type.
    /// </summary>
    bool CanProcess(FilterType filterType);

    /// <summary>
    /// Applies <paramref name="config"/> to <paramref name="image"/> and returns
    /// the modified image.
    /// </summary>
    Task<Image> ApplyFilterAsync(
        Image image,
        FilterConfiguration config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resizes <paramref name="image"/> to <paramref name="targetWidth"/> ×
    /// <paramref name="targetHeight"/> using bilinear interpolation.
    /// </summary>
    Image Resize(Image image, int targetWidth, int targetHeight);

    /// <summary>
    /// Converts <paramref name="image"/> to grayscale in-place.
    /// </summary>
    Image ToGrayscale(Image image);

    /// <summary>
    /// Applies a box-blur with the given <paramref name="radius"/> to
    /// <paramref name="image"/> in-place.
    /// </summary>
    Image Blur(Image image, int radius = 1);

/// <summary>
/// Crops <paramref name="image"/> to the specified rectangle defined by
/// <paramref name="x"/>, <paramref name="y"/>, <paramref name="width"/>, and <paramref name="height"/>.
/// Throws <see cref="ValidationException"/> if coordinates are out of bounds.
/// </summary>
Image Crop(Image image, int x, int y, int width, int height);
}
