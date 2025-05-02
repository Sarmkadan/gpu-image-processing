#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Repository for image data access operations
    /// </summary>
    public class ImageRepository : GenericRepository<Image>
    {
        /// <summary>
        /// Gets all images in a specific format
        /// </summary>
        public async Task<IEnumerable<Image>> GetByFormatAsync(ImageFormat format)
        {
            return await Task.FromResult(
                _entities.Where(i => i.Format == format).ToList()
            );
        }

        /// <summary>
        /// Gets all processed images
        /// </summary>
        public async Task<IEnumerable<Image>> GetProcessedAsync()
        {
            return await Task.FromResult(
                _entities.Where(i => i.IsProcessed).ToList()
            );
        }

        /// <summary>
        /// Gets all unprocessed images
        /// </summary>
        public async Task<IEnumerable<Image>> GetUnprocessedAsync()
        {
            return await Task.FromResult(
                _entities.Where(i => !i.IsProcessed).ToList()
            );
        }

        /// <summary>
        /// Gets images larger than a specified size
        /// </summary>
        public async Task<IEnumerable<Image>> GetByMinFileSizeAsync(long minBytes)
        {
            return await Task.FromResult(
                _entities.Where(i => i.FileSizeBytes >= minBytes).ToList()
            );
        }

        /// <summary>
        /// Gets images by name pattern
        /// </summary>
        public async Task<IEnumerable<Image>> GetByNamePatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return new List<Image>();

            var lowerPattern = pattern.ToLowerInvariant();
            return await Task.FromResult(
                _entities.Where(i => i.Name.ToLowerInvariant().Contains(lowerPattern)).ToList()
            );
        }

        /// <summary>
        /// Gets images created after a specific date
        /// </summary>
        public async Task<IEnumerable<Image>> GetCreatedAfterAsync(DateTime date)
        {
            return await Task.FromResult(
                _entities.Where(i => i.CreatedAt >= date).OrderByDescending(i => i.CreatedAt).ToList()
            );
        }

        /// <summary>
        /// Gets derived images for a parent image
        /// </summary>
        public async Task<IEnumerable<Image>> GetDerivedImagesAsync(Guid parentImageId)
        {
            return await Task.FromResult(
                _entities.Where(i => i.ParentImageId == parentImageId).ToList()
            );
        }

        /// <summary>
        /// Gets images with specific aspect ratio tolerance
        /// </summary>
        public async Task<IEnumerable<Image>> GetByAspectRatioAsync(double targetRatio, double tolerance = 0.1)
        {
            return await Task.FromResult(
                _entities.Where(i =>
                {
                    var ratio = i.GetAspectRatio();
                    var diff = Math.Abs(ratio - targetRatio);
                    return diff <= tolerance;
                }).ToList()
            );
        }

        /// <summary>
        /// Gets the total storage used by all images
        /// </summary>
        public async Task<long> GetTotalStorageUsageAsync()
        {
            return await Task.FromResult(_entities.Sum(i => i.FileSizeBytes)).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets statistics about stored images
        /// </summary>
        public async Task<ImageStatistics> GetStatisticsAsync()
        {
            var stats = new ImageStatistics
            {
                TotalImages = _entities.Count,
                ProcessedImages = _entities.Count(i => i.IsProcessed),
                UnprocessedImages = _entities.Count(i => !i.IsProcessed),
                TotalStorageBytes = _entities.Sum(i => i.FileSizeBytes),
                AverageFileSize = _entities.Count > 0 ? _entities.Average(i => i.FileSizeBytes) : 0,
                AverageWidth = _entities.Count > 0 ? _entities.Average(i => i.Width) : 0,
                AverageHeight = _entities.Count > 0 ? _entities.Average(i => i.Height) : 0,
                AverageChannels = _entities.Count > 0 ? _entities.Average(i => i.Channels) : 0,
                OldestImage = _entities.OrderBy(i => i.CreatedAt).FirstOrDefault(),
                NewestImage = _entities.OrderByDescending(i => i.CreatedAt).FirstOrDefault()
            };

            return await Task.FromResult(stats).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Statistics about images in the repository
    /// </summary>
    public class ImageStatistics
    {
        public int TotalImages { get; set; }
        public int ProcessedImages { get; set; }
        public int UnprocessedImages { get; set; }
        public long TotalStorageBytes { get; set; }
        public double AverageFileSize { get; set; }
        public double AverageWidth { get; set; }
        public double AverageHeight { get; set; }
        public double AverageChannels { get; set; }
        public Image? OldestImage { get; set; }
        public Image? NewestImage { get; set; }
    }
}
