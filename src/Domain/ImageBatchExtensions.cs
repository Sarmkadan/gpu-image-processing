#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides extension methods for <see cref="ImageBatch"/> to simplify common batch operations.
/// </summary>
public static class ImageBatchExtensions
{
	/// <summary>
	/// Adds multiple images to the batch in a single operation.
	/// </summary>
	/// <param name="batch">The image batch to add images to.</param>
	/// <param name="imageIds">The collection of image IDs to add.</param>
	/// <returns>The number of images successfully added to the batch.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="batch"/> or <paramref name="imageIds"/> is null.</exception>
	public static int AddImages(this ImageBatch batch, IEnumerable<Guid> imageIds)
	{
		ArgumentNullException.ThrowIfNull(batch);
		ArgumentNullException.ThrowIfNull(imageIds);

		var addedCount = 0;
		var maxBatchSize = AppConstants.Processing.MaxBatchSize;
		var currentCount = batch.ImageIds.Count;

		foreach (var imageId in imageIds)
		{
			if (batch.ImageIds.Contains(imageId))
				continue;

			if (currentCount + addedCount >= maxBatchSize)
				break;

			batch.ImageIds.Add(imageId);
			addedCount++;
		}

		batch.TotalImages = batch.ImageIds.Count;
		return addedCount;
	}

	/// <summary>
	/// Removes multiple images from the batch in a single operation.
	/// </summary>
	/// <param name="batch">The image batch to remove images from.</param>
	/// <param name="imageIds">The collection of image IDs to remove.</param>
	/// <returns>The number of images successfully removed from the batch.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="batch"/> or <paramref name="imageIds"/> is null.</exception>
	public static int RemoveImages(this ImageBatch batch, IEnumerable<Guid> imageIds)
	{
		ArgumentNullException.ThrowIfNull(batch);
		ArgumentNullException.ThrowIfNull(imageIds);

		var removedCount = 0;
		foreach (var imageId in imageIds)
		{
			if (batch.ImageIds.Remove(imageId))
				removedCount++;
		}

		batch.TotalImages = batch.ImageIds.Count;
		return removedCount;
	}

	/// <summary>
	/// Adds multiple filters to the batch processing pipeline in a single operation.
	/// </summary>
	/// <param name="batch">The image batch to add filters to.</param>
	/// <param name="filterIds">The collection of filter IDs to add.</param>
	/// <returns>The number of filters successfully added to the batch.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="batch"/> or <paramref name="filterIds"/> is null.</exception>
	public static int AddFilters(this ImageBatch batch, IEnumerable<Guid> filterIds)
	{
		ArgumentNullException.ThrowIfNull(batch);
		ArgumentNullException.ThrowIfNull(filterIds);

		var addedCount = 0;
		var maxBatchSize = AppConstants.Processing.MaxBatchSize;
		var currentCount = batch.FilterIds.Count;

		foreach (var filterId in filterIds)
		{
			if (batch.FilterIds.Contains(filterId))
				continue;

			if (currentCount + addedCount >= maxBatchSize)
				break;

			batch.FilterIds.Add(filterId);
			addedCount++;
		}

		return addedCount;
	}

	/// <summary>
	/// Gets the batch completion status based on processing metrics.
	/// </summary>
	/// <param name="batch">The image batch to check.</param>
	/// <returns>A <see cref="ProcessingStatus"/> value indicating the batch completion state.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="batch"/> is null.</exception>
	public static ProcessingStatus GetCompletionStatus(this ImageBatch batch)
	{
		ArgumentNullException.ThrowIfNull(batch);

		switch (batch.Status)
		{
			case ProcessingStatus.Completed or ProcessingStatus.Failed:
				return batch.Status;
			case ProcessingStatus.Cancelled:
				return ProcessingStatus.Cancelled;
			case ProcessingStatus.Queued:
				return ProcessingStatus.Queued;
		}

		if (batch.TotalImages == 0)
			return ProcessingStatus.Pending;

		var progress = batch.GetProgressPercentage();
		var successRate = batch.GetSuccessRate();

		return progress >= 100.0
			? ProcessingStatus.Completed
			: batch.Status;
	}
}
