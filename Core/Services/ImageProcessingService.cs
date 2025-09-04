#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Exceptions;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;
using GpuImageProcessing.Pipeline; // Added for IComputeShaderPipeline
using Microsoft.Extensions.Logging; // Added for ILogger
using GpuImageProcessing.Domain; // Added for ComputeShaderPass GPU-side image buffers
using Image = GpuImageProcessing.Core.Models.Image;
using ProcessingResult = GpuImageProcessing.Core.Models.ProcessingResult;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Main service for GPU-accelerated image processing operations
    /// </summary>
    public class ImageProcessingService
    {
        private readonly ImageRepository _imageRepository;
        private readonly GenericRepository<Filter> _filterRepository;
        private readonly GenericRepository<Transform> _transformRepository;
        private readonly GenericRepository<ProcessingProfile> _profileRepository;
        private readonly DeviceService _deviceService;
        private readonly IComputeShaderPipeline _computeShaderPipeline;
        private readonly ILogger<ImageProcessingService> _logger;
        private readonly FilterService _filterService; // Added
        private readonly TransformService _transformService; // Added
        private readonly Dictionary<Guid, List<ProcessingResult>> _resultsHistory = new();
        private readonly object _resultsHistoryLock = new();

        public ImageProcessingService(
            ImageRepository imageRepository,
            GenericRepository<Filter> filterRepository,
            GenericRepository<Transform> transformRepository,
            GenericRepository<ProcessingProfile> profileRepository,
            DeviceService deviceService,
            IComputeShaderPipeline computeShaderPipeline,
            ILogger<ImageProcessingService> logger,
            FilterService filterService, // Added
            TransformService transformService) // Added
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _filterRepository = filterRepository ?? throw new ArgumentNullException(nameof(filterRepository));
            _transformRepository = transformRepository ?? throw new ArgumentNullException(nameof(transformRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _computeShaderPipeline = computeShaderPipeline ?? throw new ArgumentNullException(nameof(computeShaderPipeline));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService)); // Added
            _transformService = transformService ?? throw new ArgumentNullException(nameof(transformService)); // Added
        }

        /// <summary>
        /// Registers a new image for processing
        /// </summary>
        public async Task<Image> RegisterImageAsync(string filePath, string name)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            var image = Image.CreateFromFile(filePath, name);
            return await _imageRepository.AddAsync(image);
        }

        /// <summary>
        /// Gets an image by ID
        /// </summary>
        public async Task<Image?> GetImageAsync(Guid imageId)
        {
            return await _imageRepository.GetByIdAsync(imageId);
        }

        /// <summary>
        /// Gets all registered images
        /// </summary>
        public async Task<IEnumerable<Image>> GetAllImagesAsync()
        {
            return await _imageRepository.GetAllAsync();
        }

        /// <summary>
        /// Processes a single image with specified filters and transforms
        /// </summary>
        /// <param name="imageId">The identifier of the image to process.</param>
        /// <param name="filterIds">The list of filter identifiers to apply.</param>
        /// <param name="transformIds">The list of transform identifiers to apply.</param>
        /// <param name="profileId">The optional identifier of the processing profile to use.</param>
        /// <returns>The result of the processing operation.</returns>
        public async Task<ProcessingResult> ProcessImageAsync(Guid imageId, List<Guid> filterIds, List<Guid> transformIds, Guid? profileId)
        {
            filterIds ??= new List<Guid>();
            transformIds ??= new List<Guid>();

            var stopwatch = Stopwatch.StartNew();
            var sourceImage = await _imageRepository.GetByIdAsync(imageId);

            if (sourceImage == null)
            {
                _logger.LogWarning("Image with ID {ImageId} not found.", imageId);
                return ProcessingResult.CreateFailure(Guid.Empty, imageId, "", "Image not found");
            }

            if (!sourceImage.IsValid())
            {
                _logger.LogError("Image {ImageName} (ID: {ImageId}) is invalid for processing.", sourceImage.Name, imageId);
                return ProcessingResult.CreateFailure(Guid.Empty, imageId, sourceImage.FilePath, "Image validation failed");
            }

            try
            {
                ProcessingProfile? profile = null;
                if (profileId.HasValue)
                {
                    profile = await _profileRepository.GetByIdAsync(profileId.Value);
                }
                
                if (profile == null)
                {
                    profile = ProcessingProfile.CreateBalanced();
                    _logger.LogInformation("No profile specified or found for image {ImageId}, using default balanced profile.", imageId);
                }

                // Get selected device
                var selectedDevice = _deviceService.GetSelectedDevice();
                if (selectedDevice == null)
                {
                    _logger.LogError("No compute device selected or available for processing image {ImageId}.", imageId);
                    throw new GpuException("No compute device available.");
                }
                _logger.LogDebug("Processing image {ImageId} on device {DeviceName}.", imageId, selectedDevice.Name);

                // Prepare pipeline passes
                var passes = new List<ComputeShaderPass>();
                var currentImage = new GpuImageProcessing.Domain.Image(sourceImage.Width, sourceImage.Height, sourceImage.Channels); // Represents image in GPU memory
                // In a real scenario, image data would be loaded into currentImage

                // Add filter passes
                foreach (var filterId in filterIds)
                {
                    var filter = await _filterRepository.GetByIdAsync(filterId);
                    if (filter != null && filter.IsActive)
                    {
                        var kernelCode = await _filterService.GetKernelCodeAsync(filter.Type);
                        if (!string.IsNullOrEmpty(kernelCode))
                        {
                           var pass = new ComputeShaderPass(filter.Name, kernelCode) // Now uses kernelCode from FilterService
                            {
                                OutputImage = new GpuImageProcessing.Domain.Image(currentImage.Width, currentImage.Height, currentImage.Channels)
                            };
                            pass.InputImages.Add(currentImage);
                            foreach (var p in filter.Parameters.ToDictionary(p => p.Name, p => (object)p.Value))
                                pass.Parameters[p.Key] = p.Value;
                            passes.Add(pass);
                            _logger.LogDebug("Added filter pass: {FilterName} for image {ImageId}.", filter.Name, imageId);
                        }
                    }
                }

                // Add transform passes
                foreach (var transformId in transformIds)
                {
                    var transform = await _transformRepository.GetByIdAsync(transformId);
                    if (transform != null && transform.IsActive)
                    {
                        var kernelCode = await _transformService.GetKernelCodeAsync(transform.Type);
                        if (!string.IsNullOrEmpty(kernelCode))
                        {
                            var pass = new ComputeShaderPass(transform.Name, kernelCode) // Now uses kernelCode from TransformService
                            {
                                OutputImage = new GpuImageProcessing.Domain.Image(currentImage.Width, currentImage.Height, currentImage.Channels)
                            };
                            pass.InputImages.Add(currentImage);
                            foreach (var p in transform.Parameters)
                                pass.Parameters[p.Key] = p.Value;
                            passes.Add(pass);
                            _logger.LogDebug("Added transform pass: {TransformName} for image {ImageId}.", transform.Name, imageId);
                        }
                    }
                }

                if (passes.Count == 0)
                {
                    _logger.LogInformation("No filters or transforms applied to image {ImageId}.", imageId);
                    // If no operations, just return success with original image details
                    stopwatch.Stop();
                     return ProcessingResult.CreateSuccess(Guid.NewGuid(), imageId, sourceImage.FilePath,
                        GenerateOutputPath(sourceImage)) with { ProcessingTimeMs = (float)stopwatch.ElapsedMilliseconds };
                }

                // Execute the pipeline
                var pipelineResult = await _computeShaderPipeline.ExecuteAsync(passes, selectedDevice.Id);

                // Assuming the last output image in the pipeline is the final result
                var finalOutputImage = passes.Last().OutputImage;

                // For now, simulate saving the output file and its size
                var outputPath = GenerateOutputPath(sourceImage);
                // In a real scenario, finalOutputImage data would be written to outputPath
                long outputFileSize = (long)(sourceImage.FileSizeBytes * 0.95); // Simulated reduction

                var processingResult = ProcessingResult.CreateSuccess(Guid.NewGuid(), imageId, sourceImage.FilePath, outputPath) with
                {
                    ProcessingTimeMs = (float)stopwatch.ElapsedMilliseconds,
                    OutputFileSizeBytes = outputFileSize,
                    DeviceUsed = selectedDevice.Name,
                    ProfileUsed = profile.Name
                };

                foreach (var passRecord in pipelineResult.PassRecords)
                {
                    if (passRecord.Succeeded)
                        processingResult.RecordAppliedOperation(passRecord.KernelName);
                    else
                        processingResult.RecordFailedOperation(passRecord.KernelName, passRecord.ErrorMessage ?? "Unknown error");
                }

                sourceImage.MarkAsProcessed();
                await _imageRepository.UpdateAsync(sourceImage);

                stopwatch.Stop();
                _logger.LogInformation("Successfully processed image {ImageId} in {ElapsedMs:F1} ms.", imageId, stopwatch.ElapsedMilliseconds);
                RecordResult(imageId, processingResult);
                return processingResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Processing failed for image {ImageId}.", imageId);
                var failureResult = ProcessingResult.CreateFailure(Guid.Empty, imageId, sourceImage.FilePath,
                    $"Processing failed: {ex.Message}");
                RecordResult(imageId, failureResult);
                return failureResult;
            }
        }

        /// <summary>
        /// Applies a single filter to an image and returns the processing result.
        /// </summary>
        public Task<ProcessingResult> ApplyFilterAsync(Guid imageId, Guid filterId)
        {
            return ProcessImageAsync(imageId, new List<Guid> { filterId }, new List<Guid>(), null);
        }

        /// <summary>
        /// Applies a single transform to an image and returns the processing result.
        /// </summary>
        public Task<ProcessingResult> ApplyTransformAsync(Guid imageId, Guid transformId)
        {
            return ProcessImageAsync(imageId, new List<Guid>(), new List<Guid> { transformId }, null);
        }

        /// <summary>
        /// Gets the history of processing results recorded for an image.
        /// </summary>
        public Task<List<ProcessingResult>> GetProcessingResultsAsync(Guid imageId)
        {
            lock (_resultsHistoryLock)
            {
                var results = _resultsHistory.TryGetValue(imageId, out var history)
                    ? new List<ProcessingResult>(history)
                    : new List<ProcessingResult>();
                return Task.FromResult(results);
            }
        }

        private void RecordResult(Guid imageId, ProcessingResult result)
        {
            lock (_resultsHistoryLock)
            {
                if (!_resultsHistory.TryGetValue(imageId, out var history))
                {
                    history = new List<ProcessingResult>();
                    _resultsHistory[imageId] = history;
                }

                history.Add(result);
            }
        }

        /// <summary>
        /// Processes multiple images in batch
        /// </summary>
        public async Task<List<ProcessingResult>> ProcessBatchAsync(List<Guid> imageIds, List<Guid> filterIds,
            List<Guid> transformIds, Guid profileId)
        {
            if (imageIds == null)
                throw new ArgumentNullException(nameof(imageIds), "List of image IDs cannot be null for batch processing.");

            filterIds ??= new List<Guid>();
            transformIds ??= new List<Guid>();

            var results = new List<ProcessingResult>();
            var profile = await _profileRepository.GetByIdAsync(profileId);

            if (profile == null)
            {
                profile = ProcessingProfile.CreateBalanced();
                _logger.LogInformation("No profile specified or found for batch processing, using default balanced profile.");
            }

            if (!imageIds.Any())
            {
                _logger.LogWarning("No image IDs provided for batch processing.");
                return results;
            }

            var batchSize = profile.BatchSize > 0 ? profile.BatchSize : 1; 
            
            var batches = imageIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                _logger.LogDebug("Processing batch of {Count} images.", batch.Count);
                var batchResults = await Task.WhenAll(
                    batch.Select(id => ProcessImageAsync(id, filterIds, transformIds, profile?.Id))
                );
                results.AddRange(batchResults);
            }

            _logger.LogInformation("Batch processing completed for {TotalImages} images.", imageIds.Count);
            return results;
        }

        /// <summary>
        /// Gets processing statistics for an image
        /// </summary>
        public async Task<ImageProcessingStats> GetImageStatsAsync(Guid imageId)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
            {
                _logger.LogWarning("Image with ID {ImageId} not found for stats.", imageId);
                throw new InvalidImageException("Image not found");
            }

            var stats = new ImageProcessingStats
            {
                ImageId = imageId,
                ImageName = image.Name,
                FilePath = image.FilePath,
                FileSizeBytes = image.FileSizeBytes,
                Width = image.Width,
                Height = image.Height,
                PixelCount = image.GetPixelCount(),
                AspectRatio = image.GetAspectRatio(),
                IsProcessed = image.IsProcessed,
                CreatedAt = image.CreatedAt,
                ModifiedAt = image.ModifiedAt
            };

            return await Task.FromResult(stats);
        }

        /// <summary>
        /// Validates if processing is possible with given resources
        /// </summary>
        public async Task<bool> CanProcessAsync(List<Guid> imageIds, Guid profileId)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId) ?? ProcessingProfile.CreateBalanced();
            var device = _deviceService.GetSelectedDevice();

            if (device == null)
            {
                _logger.LogWarning("Cannot process: No compute device selected.");
                return false;
            }

            long totalMemoryNeeded = 0;
            foreach (var imageId in imageIds)
            {
                var image = await _imageRepository.GetByIdAsync(imageId);
                if (image != null)
                    totalMemoryNeeded += image.FileSizeBytes * 3; // Input, output, working buffer
            }
            _logger.LogDebug("Total estimated memory needed: {MemoryNeeded} bytes.", totalMemoryNeeded);
            return device.HasSufficientMemory(totalMemoryNeeded);
        }

        /// <summary>
        /// Gets processing profile for optimization
        /// </summary>
        public async Task<ProcessingProfile?> GetProfileAsync(Guid profileId)
        {
            return await _profileRepository.GetByIdAsync(profileId);
        }

        /// <summary>
        /// Gets all available profiles
        /// </summary>
        public async Task<IEnumerable<ProcessingProfile>> GetAllProfilesAsync()
        {
            return await _profileRepository.GetAllAsync();
        }

        /// <summary>
        /// Creates a new processing profile
        /// </summary>
        public async Task<ProcessingProfile> CreateProfileAsync(string name, string description)
        {
            var profile = new ProcessingProfile
            {
                Name = name,
                Description = description,
                UseGPUAcceleration = true,
                MaxParallelOperations = 4,
                BatchSize = 10
            };

            return await _profileRepository.AddAsync(profile);
        }

        /// <summary>
        /// Generates output file path for processed image
        /// </summary>
        private string GenerateOutputPath(Image image)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            // Ensure output directory exists if necessary
            return $"./output/{image.Name}_{timestamp}.processed.png";
        }
    }

    /// <summary>
    /// Image processing statistics
    /// </summary>
    public class ImageProcessingStats
    {
        public Guid ImageId { get; set; }
        public string ImageName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long PixelCount { get; set; }
        public double AspectRatio { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
