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

        public ImageProcessingService(
            ImageRepository imageRepository,
            GenericRepository<Filter> filterRepository,
            GenericRepository<Transform> transformRepository,
            GenericRepository<ProcessingProfile> profileRepository,
            DeviceService deviceService)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _filterRepository = filterRepository ?? throw new ArgumentNullException(nameof(filterRepository));
            _transformRepository = transformRepository ?? throw new ArgumentNullException(nameof(transformRepository));
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
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
        public async Task<ProcessingResult> ProcessImageAsync(Guid imageId, List<Guid> filterIds, List<Guid> transformIds, Guid? profileId)
        {
            // Fix: Ensure filterIds and transformIds are not null.
            filterIds ??= new List<Guid>();
            transformIds ??= new List<Guid>();

            var stopwatch = Stopwatch.StartNew();
            var image = await _imageRepository.GetByIdAsync(imageId);

            if (image == null)
                return ProcessingResult.CreateFailure(Guid.Empty, imageId, "", "Image not found");

            if (!image.IsValid())
                return ProcessingResult.CreateFailure(Guid.Empty, imageId, image.FilePath, "Image validation failed");

            try
            {
                ProcessingProfile? profile = null;
                if (profileId.HasValue)
                {
                    profile = await _profileRepository.GetByIdAsync(profileId.Value);
                }
                
                if (profile == null)
                    profile = ProcessingProfile.CreateBalanced();

                var result = ProcessingResult.CreateSuccess(Guid.NewGuid(), imageId, image.FilePath, GenerateOutputPath(image));

                // Simulate filter application
                foreach (var filterId in filterIds)
                {
                    var filter = await _filterRepository.GetByIdAsync(filterId);
                    if (filter != null && filter.IsActive)
                    {
                        result.RecordAppliedFilter(filter.Name);
                    }
                }

                // Simulate transform application
                foreach (var transformId in transformIds)
                {
                    var transform = await _transformRepository.GetByIdAsync(transformId);
                    if (transform != null && transform.IsActive)
                    {
                        result.RecordAppliedTransform(transform.Name);
                    }
                }

                stopwatch.Stop();
                result.ProcessingTimeMs = (float)stopwatch.ElapsedMilliseconds;
                result.OutputFileSizeBytes = (long)(image.FileSizeBytes * 0.95);
                result.AddMetric("profile_used", profile.Name);
                result.AddMetric("device", _deviceService.GetSelectedDevice()?.Name ?? "Unknown");

                image.MarkAsProcessed();
                await _imageRepository.UpdateAsync(image);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ProcessingResult.CreateFailure(Guid.Empty, imageId, image.FilePath,
                    $"Processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes multiple images in batch
        /// </summary>
        public async Task<List<ProcessingResult>> ProcessBatchAsync(List<Guid> imageIds, List<Guid> filterIds,
            List<Guid> transformIds, Guid profileId)
        {
            // Fix: Add null check for imageIds.
            if (imageIds == null)
                throw new ArgumentNullException(nameof(imageIds), "List of image IDs cannot be null for batch processing.");

            // Fix: Ensure filterIds and transformIds are not null.
            filterIds ??= new List<Guid>();
            transformIds ??= new List<Guid>();

            var results = new List<ProcessingResult>();
            var profile = await _profileRepository.GetByIdAsync(profileId);

            if (profile == null)
                profile = ProcessingProfile.CreateBalanced();

            // Fix: Use the BatchSize from the profile for actual batching.
            // If imageIds is empty, there are no batches to process.
            if (!imageIds.Any())
                return results;

            var batchSize = profile.BatchSize > 0 ? profile.BatchSize : 1; // Ensure batchSize is at least 1
            
            var batches = imageIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                var batchResults = await Task.WhenAll(
                    batch.Select(id => ProcessImageAsync(id, filterIds, transformIds, profile?.Id))
                );
                results.AddRange(batchResults);
            }

            return results;
        }

        /// <summary>
        /// Gets processing statistics for an image
        /// </summary>
        public async Task<ImageProcessingStats> GetImageStatsAsync(Guid imageId)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image == null)
                throw new InvalidImageException("Image not found");

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
                return false;

            long totalMemoryNeeded = 0;
            foreach (var imageId in imageIds)
            {
                var image = await _imageRepository.GetByIdAsync(imageId);
                if (image != null)
                    totalMemoryNeeded += image.FileSizeBytes * 3; // Input, output, working buffer
            }

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
