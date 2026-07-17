#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Api
{
	/// <summary>
	/// Extension methods for <see cref="ImageProcessingController"/> that provide
	/// convenient utility operations for image processing workflows.
	/// </summary>
	public static class ImageProcessingControllerExtensions
	{
		private const string EmptyGuidError = "Invalid ID (empty GUID)";
		private const string NoItemsError = "No items provided";

		/// <summary>
		/// Registers multiple images for processing in a single batch operation.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="filePaths">List of image file paths to register.</param>
		/// <param name="description">Optional description for the batch.</param>
		/// <returns>ApiResponse containing list of registered image metadata.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="filePaths"/> is null.</exception>
		public static async Task<ApiResponse<List<ImageMetadata>>> RegisterImagesAsync(
			this ImageProcessingController controller,
			IReadOnlyList<string> filePaths,
			string? description = null)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(filePaths);

			if (filePaths.Count == 0)
			{
				return ApiResponse<List<ImageMetadata>>.Failure(NoItemsError);
			}

			var results = new List<ImageMetadata>();
			var errors = new List<string>();

			foreach (var filePath in filePaths)
			{
				if (string.IsNullOrWhiteSpace(filePath))
				{
					errors.Add("File path cannot be null or empty");
					continue;
				}

				var result = await controller.RegisterImageAsync(filePath, description ?? string.Empty);

				if (result.IsSuccess)
				{
					_ = result.Data ?? throw new InvalidOperationException("Registered image metadata cannot be null");
					results.Add(result.Data);
				}
				else
				{
					errors.Add($"Failed to register {filePath}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<List<ImageMetadata>>.Failure(
					$"Batch registration completed with {errors.Count} errors. Success: {results.Count}/{filePaths.Count}",
					results)
				: ApiResponse<List<ImageMetadata>>.Success(results);
		}

		/// <summary>
		/// Applies the same filter to multiple images in parallel.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="imageIds">List of image IDs to process.</param>
		/// <param name="filterId">Filter ID to apply.</param>
		/// <returns>ApiResponse containing list of processing results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="imageIds"/> is null.</exception>
		public static async Task<ApiResponse<List<ProcessingResult>>> ApplyFilterToImagesAsync(
			this ImageProcessingController controller,
			IReadOnlyList<Guid> imageIds,
			Guid filterId)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(imageIds);

			if (imageIds.Count == 0)
			{
				return ApiResponse<List<ProcessingResult>>.Failure(NoItemsError);
			}

			if (filterId == Guid.Empty)
			{
				return ApiResponse<List<ProcessingResult>>.Failure(EmptyGuidError);
			}

			var results = new List<ProcessingResult>();
			var errors = new List<string>();

			foreach (var imageId in imageIds)
			{
				if (imageId == Guid.Empty)
				{
					errors.Add(EmptyGuidError);
					continue;
				}

				var result = await controller.ApplyFilterAsync(imageId, filterId);

				if (result.IsSuccess)
				{
					_ = result.Data ?? throw new InvalidOperationException("Processing result cannot be null");
					results.Add(result.Data);
				}
				else
				{
					errors.Add($"Failed to apply filter to image {imageId}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<List<ProcessingResult>>.Failure(
					$"Batch filter application completed with {errors.Count} errors. Success: {results.Count}/{imageIds.Count}",
					results)
				: ApiResponse<List<ProcessingResult>>.Success(results);
		}

		/// <summary>
		/// Applies the same transform to multiple images in parallel.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="imageIds">List of image IDs to process.</param>
		/// <param name="transformId">Transform ID to apply.</param>
		/// <returns>ApiResponse containing list of processing results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="imageIds"/> is null.</exception>
		public static async Task<ApiResponse<List<ProcessingResult>>> ApplyTransformToImagesAsync(
			this ImageProcessingController controller,
			IReadOnlyList<Guid> imageIds,
			Guid transformId)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(imageIds);

			if (imageIds.Count == 0)
			{
				return ApiResponse<List<ProcessingResult>>.Failure(NoItemsError);
			}

			if (transformId == Guid.Empty)
			{
				return ApiResponse<List<ProcessingResult>>.Failure(EmptyGuidError);
			}

			var results = new List<ProcessingResult>();
			var errors = new List<string>();

			foreach (var imageId in imageIds)
			{
				if (imageId == Guid.Empty)
				{
					errors.Add(EmptyGuidError);
					continue;
				}

				var result = await controller.ApplyTransformAsync(imageId, transformId);

				if (result.IsSuccess)
				{
					_ = result.Data ?? throw new InvalidOperationException("Processing result cannot be null");
					results.Add(result.Data);
				}
				else
				{
					errors.Add($"Failed to apply transform to image {imageId}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<List<ProcessingResult>>.Failure(
					$"Batch transform application completed with {errors.Count} errors. Success: {results.Count}/{imageIds.Count}",
					results)
				: ApiResponse<List<ProcessingResult>>.Success(results);
		}

		/// <summary>
		/// Gets image information for multiple images in a single call.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="imageIds">List of image IDs to query.</param>
		/// <returns>ApiResponse containing list of image metadata.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="imageIds"/> is null.</exception>
		public static async Task<ApiResponse<List<ImageMetadata>>> GetImagesInfoAsync(
			this ImageProcessingController controller,
			IReadOnlyList<Guid> imageIds)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(imageIds);

			if (imageIds.Count == 0)
			{
				return ApiResponse<List<ImageMetadata>>.Failure(NoItemsError);
			}

			var results = new List<ImageMetadata>();
			var errors = new List<string>();

			foreach (var imageId in imageIds)
			{
				if (imageId == Guid.Empty)
				{
					errors.Add(EmptyGuidError);
					continue;
				}

				var result = await controller.GetImageInfoAsync(imageId);

				if (result.IsSuccess)
				{
					_ = result.Data ?? throw new InvalidOperationException("Image metadata cannot be null");
					results.Add(result.Data);
				}
				else
				{
					errors.Add($"Failed to get info for image {imageId}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<List<ImageMetadata>>.Failure(
					$"Batch image info retrieval completed with {errors.Count} errors. Success: {results.Count}/{imageIds.Count}",
					results)
				: ApiResponse<List<ImageMetadata>>.Success(results);
		}

		/// <summary>
		/// Gets processing results for multiple images in a single call.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="imageIds">List of image IDs to query.</param>
		/// <returns>ApiResponse containing dictionary mapping image IDs to their processing results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="imageIds"/> is null.</exception>
		public static async Task<ApiResponse<Dictionary<Guid, List<ProcessingResult>>>> GetProcessingResultsByImageAsync(
			this ImageProcessingController controller,
			IReadOnlyList<Guid> imageIds)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(imageIds);

			if (imageIds.Count == 0)
			{
				return ApiResponse<Dictionary<Guid, List<ProcessingResult>>>.Failure(NoItemsError);
			}

			var results = new Dictionary<Guid, List<ProcessingResult>>();
			var errors = new List<string>();

			foreach (var imageId in imageIds)
			{
				if (imageId == Guid.Empty)
				{
					errors.Add(EmptyGuidError);
					continue;
				}

				var result = await controller.GetProcessingResultsAsync(imageId);

				if (result.IsSuccess)
				{
					results[imageId] = result.Data ?? new List<ProcessingResult>();
				}
				else
				{
					errors.Add($"Failed to get results for image {imageId}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<Dictionary<Guid, List<ProcessingResult>>>.Failure(
					$"Batch results retrieval completed with {errors.Count} errors. Success: {results.Count}/{imageIds.Count}",
					results)
				: ApiResponse<Dictionary<Guid, List<ProcessingResult>>>.Success(results);
		}

		/// <summary>
		/// Cancels multiple batch jobs in a single operation.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="jobIds">List of job IDs to cancel.</param>
		/// <returns>ApiResponse containing list of cancellation results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="jobIds"/> is null.</exception>
		public static async Task<ApiResponse<List<string>>> CancelBatchJobsAsync(
			this ImageProcessingController controller,
			IReadOnlyList<Guid> jobIds)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(jobIds);

			if (jobIds.Count == 0)
			{
				return ApiResponse<List<string>>.Failure(NoItemsError);
			}

			var results = new List<string>();
			var errors = new List<string>();

			foreach (var jobId in jobIds)
			{
				if (jobId == Guid.Empty)
				{
					errors.Add(EmptyGuidError);
					continue;
				}

				var result = await controller.CancelBatchJobAsync(jobId);

				if (result.IsSuccess)
				{
					results.Add(jobId.ToString("D", CultureInfo.InvariantCulture));
				}
				else
				{
					errors.Add($"Failed to cancel job {jobId}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<List<string>>.Failure(
					$"Batch cancellation completed with {errors.Count} errors. Success: {results.Count}/{jobIds.Count}",
					results)
				: ApiResponse<List<string>>.Success(results);
		}

		/// <summary>
		/// Gets status for multiple batch jobs in a single call.
		/// </summary>
		/// <param name="controller">The controller instance.</param>
		/// <param name="jobIds">List of job IDs to query.</param>
		/// <returns>ApiResponse containing dictionary mapping job IDs to their statuses.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="jobIds"/> is null.</exception>
		public static async Task<ApiResponse<Dictionary<Guid, BatchJobStatus>>> GetBatchJobsStatusAsync(
			this ImageProcessingController controller,
			IReadOnlyList<Guid> jobIds)
		{
			ArgumentNullException.ThrowIfNull(controller);
			ArgumentNullException.ThrowIfNull(jobIds);

			if (jobIds.Count == 0)
			{
				return ApiResponse<Dictionary<Guid, BatchJobStatus>>.Failure(NoItemsError);
			}

			var results = new Dictionary<Guid, BatchJobStatus>();
			var errors = new List<string>();

			foreach (var jobId in jobIds)
			{
				if (jobId == Guid.Empty)
				{
					errors.Add(EmptyGuidError);
					continue;
				}

				var result = await controller.GetBatchJobStatusAsync(jobId);

				if (result.IsSuccess)
				{
					_ = result.Data ?? throw new InvalidOperationException("Batch job status cannot be null");
					results[jobId] = result.Data;
				}
				else
				{
					errors.Add($"Failed to get status for job {jobId}: {result.Message}");
				}
			}

			return errors.Count > 0
				? ApiResponse<Dictionary<Guid, BatchJobStatus>>.Failure(
					$"Batch status retrieval completed with {errors.Count} errors. Success: {results.Count}/{jobIds.Count}",
					results)
				: ApiResponse<Dictionary<Guid, BatchJobStatus>>.Success(results);
		}
	}
}