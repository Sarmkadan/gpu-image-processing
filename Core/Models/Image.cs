#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Represents an image asset with metadata and processing history
    /// </summary>
    public class Image
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int Channels { get; set; }
        public ImageFormat Format { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
        public bool IsProcessed { get; set; }
        public Guid? ParentImageId { get; set; }

        /// <summary>
        /// Initializes a new instance of the Image class
        /// </summary>
        public Image()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new image from file path with automatic property initialization
        /// </summary>
        public static Image CreateFromFile(string filePath, string name)
        {
            var image = new Image
            {
                FilePath = filePath,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            return image;
        }

        /// <summary>
        /// Updates the modification timestamp and marks as processed
        /// </summary>
        public void MarkAsProcessed()
        {
            IsProcessed = true;
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Validates image dimensions and properties
        /// </summary>
        public bool IsValid()
        {
            return Width > 0 && Height > 0 && Channels > 0 && FileSizeBytes > 0 && !string.IsNullOrEmpty(FilePath);
        }

        /// <summary>
        /// Calculates the aspect ratio of the image
        /// </summary>
        public double GetAspectRatio()
        {
            return Width > 0 ? (double)Width / Height : 0;
        }

        /// <summary>
        /// Gets the total number of pixels in the image
        /// </summary>
        public long GetPixelCount()
        {
            return (long)Width * Height;
        }

        /// <summary>
        /// Adds or updates metadata key-value pair
        /// </summary>
        public void AddMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metadata key cannot be empty", nameof(key));

            Metadata[key] = value;
        }

        /// <summary>
        /// Retrieves metadata by key with default value
        /// </summary>
        public string GetMetadata(string key, string defaultValue = "")
        {
            return Metadata.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
