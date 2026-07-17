#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Fallback;

/// <summary>
/// Extension methods that provide convenient high-level operations on <see cref="CpuImageProcessor"/>.
/// </summary>
public static class CpuImageProcessorExtensions
{
	/// <summary>
	/// Applies a sequence of filter configurations to an image, returning the final processed image.
	/// </summary>
	/// <param name="processor">The <see cref="CpuImageProcessor"/> instance.</param>
	/// <param name="image">The image to process.</param>
	/// <param name="configs">The filter configurations to apply in order.</param>
	/// <param name="cancellationToken">Optional cancellation token.</param>
	/// <returns>The processed image.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/>, <paramref name="image"/>, or <paramref name="configs"/> is null.</exception>
	/// <exception cref="ProcessingException">Propagated from <see cref="CpuImageProcessor.ApplyFilterAsync"/>.</exception>
	public static async Task<Image> ApplyFiltersAsync(
		this CpuImageProcessor processor,
		Image image,
		IEnumerable<FilterConfiguration> configs,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(processor);
		ArgumentNullException.ThrowIfNull(image);
		ArgumentNullException.ThrowIfNull(configs);

		var current = image;
		foreach (var config in configs)
		{
			current = await processor.ApplyFilterAsync(current, config, cancellationToken).ConfigureAwait(false);
		}

		return current;
	}

	/// <summary>
	/// Resizes an image and then applies a single filter configuration.
	/// </summary>
	/// <param name="processor">The <see cref="CpuImageProcessor"/> instance.</param>
	/// <param name="image">The image to process.</param>
	/// <param name="targetWidth">The target width after resizing.</param>
	/// <param name="targetHeight">The target height after resizing.</param>
	/// <param name="config">The filter configuration to apply.</param>
	/// <returns>The processed image.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/>, <paramref name="image"/>, or <paramref name="config"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="targetWidth"/> or <paramref name="targetHeight"/> is not positive.</exception>
	/// <exception cref="ProcessingException">Propagated from <see cref="CpuImageProcessor.ApplyFilterAsync"/>.</exception>
	public static async Task<Image> ResizeAndApplyFilterAsync(
		this CpuImageProcessor processor,
		Image image,
		int targetWidth,
		int targetHeight,
		FilterConfiguration config,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(processor);
		ArgumentNullException.ThrowIfNull(image);
		ArgumentNullException.ThrowIfNull(config);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth, nameof(targetWidth));
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight, nameof(targetHeight));

		var resized = processor.Resize(image, targetWidth, targetHeight);
		return await processor.ApplyFilterAsync(resized, config, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Creates a thumbnail of the specified size from the given image.
	/// </summary>
	/// <param name="processor">The <see cref="CpuImageProcessor"/> instance.</param>
	/// <param name="image">The source image.</param>
	/// <param name="thumbWidth">The thumbnail width.</param>
	/// <param name="thumbHeight">The thumbnail height.</param>
	/// <returns>The thumbnail image.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> or <paramref name="image"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="thumbWidth"/> or <paramref name="thumbHeight"/> is not positive.</exception>
	public static Image CreateThumbnail(
		this CpuImageProcessor processor,
		Image image,
		int thumbWidth,
		int thumbHeight)
	{
		ArgumentNullException.ThrowIfNull(processor);
		ArgumentNullException.ThrowIfNull(image);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(thumbWidth, nameof(thumbWidth));
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(thumbHeight, nameof(thumbHeight));

		return processor.Resize(image, thumbWidth, thumbHeight);
	}

	/// <summary>
	/// Applies grayscale conversion followed by a box blur to the image.
	/// </summary>
	/// <param name="processor">The <see cref="CpuImageProcessor"/> instance.</param>
	/// <param name="image">The image to process.</param>
	/// <param name="blurRadius">The radius of the blur kernel.</param>
	/// <returns>The processed image.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="processor"/> or <paramref name="image"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="blurRadius"/> is negative.</exception>
	public static Image ApplyGrayscaleAndBlur(
		this CpuImageProcessor processor,
		Image image,
		int blurRadius = 1)
	{
		ArgumentNullException.ThrowIfNull(processor);
		ArgumentNullException.ThrowIfNull(image);
		ArgumentOutOfRangeException.ThrowIfNegative(blurRadius, nameof(blurRadius));

		var gray = processor.ToGrayscale(image);
		return processor.Blur(gray, blurRadius);
	}
}
