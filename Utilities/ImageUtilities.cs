#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Frozen;
using System.IO;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utility methods for image file and format operations.
    /// Provides validation, format detection, and file operations.
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        /// Supported image file extensions for processing.
        /// </summary>
        public static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".webp", ".gif" };

        // FrozenSet gives O(1) lookups with a case-insensitive comparer, avoiding
        // the O(n) linear scan of SupportedExtensions on every ingestion call.
        private static readonly FrozenSet<string> _supportedExtensionLookup =
            new HashSet<string>(SupportedExtensions, StringComparer.OrdinalIgnoreCase).ToFrozenSet();

        /// <summary>
        /// Determines if a file is a supported image format.
        /// </summary>
        public static bool IsSupportedImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var extension = Path.GetExtension(filePath.AsSpan());
            return _supportedExtensionLookup.Contains(extension.ToString());
        }

        /// <summary>
        /// Gets the image format from file extension.
        /// Returns null if extension is not recognized.
        /// </summary>
        public static string GetImageFormat(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            string extension = Path.GetExtension(filePath).ToLower().TrimStart('.');

            return extension switch
            {
                "jpg" or "jpeg" => "JPEG",
                "png" => "PNG",
                "bmp" => "BMP",
                "tiff" or "tif" => "TIFF",
                "webp" => "WEBP",
                "gif" => "GIF",
                _ => null
            };
        }

        /// <summary>
        /// Validates that an image file exists and is accessible.
        /// </summary>
        public static bool ValidateImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            if (!File.Exists(filePath))
                return false;

            try
            {
                // Attempt to open file to verify accessibility
                using (var stream = File.OpenRead(filePath))
                {
                    return stream.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the file size of an image in bytes.
        /// Returns -1 if file doesn't exist.
        /// </summary>
        public static long GetImageFileSize(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return -1;

            try
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Converts file size in bytes to human-readable format.
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            if (bytes < 0)
                return "Unknown";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Generates a unique output filename based on input and filter applied.
        /// Preserves original filename and adds timestamp for uniqueness.
        /// </summary>
        public static string GenerateOutputFilename(string inputPath, string filterName, string outputDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                throw new ArgumentException("Input path cannot be empty", nameof(inputPath));

            outputDirectory = outputDirectory ?? Path.GetDirectoryName(inputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
                outputDirectory = ".";

            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string suffix = string.IsNullOrEmpty(filterName) ? timestamp : $"{filterName}_{timestamp}";

            return Path.Combine(outputDirectory, $"{nameWithoutExt}_{suffix}{extension}");
        }

        /// <summary>
        /// Validates that output directory exists and is writable.
        /// Creates directory if it doesn't exist.
        /// </summary>
        public static bool EnsureOutputDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return false;

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Verify write access by attempting to create a temp file
                string testFile = Path.Combine(directoryPath, ".write_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Calculates aspect ratio of image dimensions.
        /// </summary>
        public static double CalculateAspectRatio(int width, int height)
        {
            if (height <= 0)
                throw new ArgumentException("Height must be greater than zero", nameof(height));

            return (double)width / height;
        }

        /// <summary>
        /// Calculates new dimensions for image resizing while maintaining aspect ratio.
        /// </summary>
        public static (int width, int height) CalculateProportionalSize(int originalWidth, int originalHeight, double scaleFactor)
        {
            if (scaleFactor <= 0)
                throw new ArgumentException("Scale factor must be greater than zero", nameof(scaleFactor));

            int newWidth = (int)(originalWidth * scaleFactor);
            int newHeight = (int)(originalHeight * scaleFactor);

            return (newWidth, newHeight);
        }

        /// <summary>
        /// Gets the total size of all images in a directory.
        /// </summary>
        public static long GetDirectoryImageSize(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return 0;

            try
            {
                var directory = new DirectoryInfo(directoryPath);
                return directory
                    .GetFiles()
                    .Where(f => IsSupportedImageFile(f.FullPath))
                    .Sum(f => f.Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Safely deletes an image file with error handling.
        /// </summary>
        public static bool SafeDeleteImage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets MIME type for an image file.
        /// </summary>
        public static string GetMimeType(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            string extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                ".tiff" or ".tif" => "image/tiff",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
