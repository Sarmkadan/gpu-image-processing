#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// REST API controller for image processing operations.
    /// Handles HTTP requests for image registration, processing, and result retrieval.
    /// </summary>
    public class ImageProcessingController
    {
        private readonly ImageProcessingService _imageProcessingService;
        private readonly FilterService _filterService;
        private readonly TransformService _transformService;
        private readonly BatchProcessingService _batchProcessingService;

        public ImageProcessingController(
            ImageProcessingService imageProcessingService,
            FilterService filterService,
            TransformService transformService,
            BatchProcessingService batchProcessingService)
        {
            _imageProcessingService = imageProcessingService;
            _filterService = filterService;
            _transformService = transformService;
            _batchProcessingService = batchProcessingService;
        }

        /// <summary>
        /// Registers a new image for processing
        /// </summary>
        public async Task<ApiResponse<ImageMetadata>> RegisterImageAsync(string filePath, string description)
        {
            try
            {
                // Fix: Add input validation for filePath and description.
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentNullException(nameof(filePath), "Image file path cannot be null or empty.");
                }

                // Ensure description is not null, default to empty string if it is.
                description ??= string.Empty;

                var image = await _imageProcessingService.RegisterImageAsync(filePath, description);
                return ApiResponse<ImageMetadata>.Success(new ImageMetadata
                {
                    Id = image.Id,
                    Path = image.Path,
                    Width = image.Width,
                    Height = image.Height,
                    Channels = image.Channels,
                    FileSizeBytes = image.FileSizeBytes,
                    Description = image.Description,
                    RegisteredAt = image.RegisteredAt
                });
            }
            catch (ArgumentNullException ex)
            {
                return ApiResponse<ImageMetadata>.Failure($"Validation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ApiResponse<ImageMetadata>.Failure($"Failed to register image: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies a filter to an image
        /// </summary>
        public async Task<ApiResponse<ProcessingResult>> ApplyFilterAsync(Guid imageId, Guid filterId)
        {
            try
            {
                if (imageId == Guid.Empty || filterId == Guid.Empty)
                    return ApiResponse<ProcessingResult>.Failure("Invalid image or filter ID");

                var result = await _imageProcessingService.ApplyFilterAsync(imageId, filterId);
                return ApiResponse<ProcessingResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProcessingResult>.Failure($"Filter application failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies a transformation to an image
        /// </summary>
        public async Task<ApiResponse<ProcessingResult>> ApplyTransformAsync(Guid imageId, Guid transformId)
        {
            try
            {
                if (imageId == Guid.Empty || transformId == Guid.Empty)
                    return ApiResponse<ProcessingResult>.Failure("Invalid image or transform ID");

                var result = await _imageProcessingService.ApplyTransformAsync(imageId, transformId);
                return ApiResponse<ProcessingResult>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<ProcessingResult>.Failure($"Transform application failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets detailed information about a registered image
        /// </summary>
        public async Task<ApiResponse<ImageMetadata>> GetImageInfoAsync(Guid imageId)
        {
            try
            {
                var image = await _imageProcessingService.GetImageAsync(imageId);
                if (image == null)
                    return ApiResponse<ImageMetadata>.Failure("Image not found");

                return ApiResponse<ImageMetadata>.Success(new ImageMetadata
                {
                    Id = image.Id,
                    Path = image.Path,
                    Width = image.Width,
                    Height = image.Height,
                    Channels = image.Channels,
                    FileSizeBytes = image.FileSizeBytes,
                    Description = image.Description,
                    RegisteredAt = image.RegisteredAt
                });
            }
            catch (Exception ex)
            {
                return ApiResponse<ImageMetadata>.Failure($"Failed to retrieve image: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a batch processing job
        /// </summary>
        public async Task<ApiResponse<BatchJobMetadata>> CreateBatchJobAsync(
            string jobName,
            List<Guid> imageIds,
            List<Guid> filterIds,
            List<Guid> transformIds,
            Guid? profileId = null)
        {
            try
            {
                var job = await _batchProcessingService.CreateJobAsync(
                    jobName,
                    imageIds,
                    filterIds,
                    transformIds,
                    profileId ?? Guid.Empty
                );

                return ApiResponse<BatchJobMetadata>.Success(new BatchJobMetadata
                {
                    JobId = job.Id,
                    JobName = job.Name,
                    TotalImages = job.TotalImages,
                    Status = job.Status.ToString(),
                    CreatedAt = job.CreatedAt,
                    EstimatedCompletionTime = job.EstimatedCompletionTime
                });
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchJobMetadata>.Failure($"Failed to create batch job: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the status of a batch processing job
        /// </summary>
        public async Task<ApiResponse<BatchJobStatus>> GetBatchJobStatusAsync(Guid jobId)
        {
            try
            {
                var job = await _batchProcessingService.GetJobAsync(jobId);
                if (job == null)
                    return ApiResponse<BatchJobStatus>.Failure("Job not found");

                var progress = await _batchProcessingService.GetJobProgressAsync(jobId);

                return ApiResponse<BatchJobStatus>.Success(new BatchJobStatus
                {
                    JobId = job.Id,
                    Status = job.Status.ToString(),
                    ProcessedImages = progress.ProcessedCount,
                    TotalImages = progress.TotalCount,
                    PercentComplete = progress.PercentComplete,
                    ElapsedTime = progress.ElapsedTime,
                    EstimatedTimeRemaining = progress.EstimatedTimeRemaining,
                    Errors = progress.Errors
                });
            }
            catch (Exception ex)
            {
                return ApiResponse<BatchJobStatus>.Failure($"Failed to retrieve job status: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels an ongoing batch job
        /// </summary>
        public async Task<ApiResponse<string>> CancelBatchJobAsync(Guid jobId)
        {
            try
            {
                await _batchProcessingService.CancelJobAsync(jobId);
                return ApiResponse<string>.Success("Job cancelled successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure($"Failed to cancel job: {ex.Message}");
            }
        }

        /// <summary>
        /// Lists all registered filters
        /// </summary>
        public async Task<ApiResponse<List<FilterInfo>>> ListFiltersAsync()
        {
            try
            {
                var filters = await _filterService.ListFiltersAsync();
                var result = new List<FilterInfo>();

                foreach (var filter in filters)
                {
                    result.Add(new FilterInfo
                    {
                        Id = filter.Id,
                        Name = filter.Name,
                        Type = filter.Type.ToString(),
                        Description = filter.Description
                    });
                }

                return ApiResponse<List<FilterInfo>>.Success(result);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<FilterInfo>>.Failure($"Failed to list filters: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets processing results for an image
        /// </summary>
        public async Task<ApiResponse<List<ProcessingResult>>> GetProcessingResultsAsync(Guid imageId)
        {
            try
            {
                var results = await _imageProcessingService.GetProcessingResultsAsync(imageId);
                return ApiResponse<List<ProcessingResult>>.Success(results ?? new List<ProcessingResult>());
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProcessingResult>>.Failure($"Failed to retrieve results: {ex.Message}");
            }
        }
    }

    public class ImageMetadata
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Channels { get; set; }
        public long FileSizeBytes { get; set; }
        public string Description { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class BatchJobMetadata
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public int TotalImages { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public TimeSpan? EstimatedCompletionTime { get; set; }
    }

    public class BatchJobStatus
    {
        public Guid JobId { get; set; }
        public string Status { get; set; }
        public int ProcessedImages { get; set; }
        public int TotalImages { get; set; }
        public double PercentComplete { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class FilterInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
