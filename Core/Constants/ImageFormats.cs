#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core.Constants
{
    /// <summary>
    /// Enumeration of supported image file formats
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>JPEG format</summary>
        Jpeg = 0,

        /// <summary>PNG format</summary>
        Png = 1,

        /// <summary>BMP format</summary>
        Bmp = 2,

        /// <summary>TIFF format</summary>
        Tiff = 3,

        /// <summary>WebP format</summary>
        WebP = 4,

        /// <summary>GIF format</summary>
        Gif = 5,

        /// <summary>PGM format</summary>
        Pgm = 6,

        /// <summary>PPM format</summary>
        Ppm = 7,

        /// <summary>AVIF format</summary>
        Avif = 8,

        /// <summary>Raw binary format</summary>
        Raw = 9,

        /// <summary>Unknown format</summary>
        Unknown = 10
    }

    /// <summary>
    /// Helper class for image format operations
    /// </summary>
    public static class ImageFormatHelper
    {
        /// <summary>
        /// Gets the file extension for the given format
        /// </summary>
        public static string GetExtension(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.Jpeg => ".jpg",
                ImageFormat.Png => ".png",
                ImageFormat.Bmp => ".bmp",
                ImageFormat.Tiff => ".tiff",
                ImageFormat.WebP => ".webp",
                ImageFormat.Gif => ".gif",
                ImageFormat.Pgm => ".pgm",
                ImageFormat.Ppm => ".ppm",
                ImageFormat.Avif => ".avif",
                ImageFormat.Raw => ".raw",
                _ => ".bin"
            };
        }

        /// <summary>
        /// Determines the format from file extension
        /// </summary>
        public static ImageFormat FromExtension(string extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".bmp" => ImageFormat.Bmp,
                ".tiff" or ".tif" => ImageFormat.Tiff,
                ".webp" => ImageFormat.WebP,
                ".gif" => ImageFormat.Gif,
                ".pgm" => ImageFormat.Pgm,
                ".ppm" => ImageFormat.Ppm,
                ".avif" => ImageFormat.Avif,
                ".raw" => ImageFormat.Raw,
                _ => ImageFormat.Unknown
            };
        }

        /// <summary>
        /// Checks if format supports transparency
        /// </summary>
        public static bool SupportsTransparency(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.Png or ImageFormat.Gif or ImageFormat.WebP or ImageFormat.Avif => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if format is compressed (lossy or lossless)
        /// </summary>
        public static bool IsCompressed(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.Jpeg or ImageFormat.Png or ImageFormat.WebP or ImageFormat.Avif or ImageFormat.Gif => true,
                _ => false
            };
        }
    }
}
