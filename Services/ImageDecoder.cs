#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Decodes image files from various formats into raw RGBA pixel buffers
    /// suitable for GPU upload. Supports BMP natively; extensible via format registration.
    /// </summary>
    public class ImageDecoder
    {
        private readonly ILogger<ImageDecoder> _logger;

        /// <summary>
        /// Initializes the image decoder.
        /// </summary>
        /// <param name="logger">Logger for decode diagnostics.</param>
        public ImageDecoder(ILogger<ImageDecoder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Decodes a BMP file from a stream into a raw RGBA pixel buffer.
        /// </summary>
        /// <param name="stream">Input stream positioned at the beginning of a BMP file.</param>
        /// <param name="width">Output image width in pixels.</param>
        /// <param name="height">Output image height in pixels.</param>
        /// <returns>Raw RGBA pixel data, 4 bytes per pixel, row-major top-to-bottom.</returns>
        public byte[] DecodeBmp(Stream stream, out int width, out int height)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);

            // BMP header
            ushort magic = reader.ReadUInt16();
            if (magic != 0x4D42) // 'BM'
                throw new InvalidDataException("Not a valid BMP file.");

            reader.ReadInt32(); // file size
            reader.ReadInt32(); // reserved
            int dataOffset = reader.ReadInt32();

            // DIB header (BITMAPINFOHEADER)
            int headerSize = reader.ReadInt32();
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            reader.ReadInt16(); // color planes
            short bpp = reader.ReadInt16();

            if (bpp != 24 && bpp != 32)
                throw new NotSupportedException($"Only 24-bit and 32-bit BMP are supported, got {bpp}-bit.");

            bool bottomUp = height > 0;
            height = Math.Abs(height);

            _logger.LogDebug("Decoding BMP: {W}x{H}, {Bpp}bpp", width, height, bpp);

            stream.Position = dataOffset;
            int bytesPerPixel = bpp / 8;
            int rowStride = (width * bytesPerPixel + 3) & ~3; // rows padded to 4 bytes
            var rowBuffer = new byte[rowStride];
            var pixels = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                int actualRow = bottomUp ? height - 1 - y : y;
                stream.ReadExactly(rowBuffer, 0, rowStride);

                for (int x = 0; x < width; x++)
                {
                    int srcIdx = x * bytesPerPixel;
                    int dstIdx = (actualRow * width + x) * 4;

                    pixels[dstIdx + 0] = rowBuffer[srcIdx + 2]; // R (BMP is BGR)
                    pixels[dstIdx + 1] = rowBuffer[srcIdx + 1]; // G
                    pixels[dstIdx + 2] = rowBuffer[srcIdx + 0]; // B
                    pixels[dstIdx + 3] = bpp == 32 ? rowBuffer[srcIdx + 3] : (byte)255; // A
                }
            }

            _logger.LogInformation("Decoded BMP image {W}x{H}, {Len} bytes RGBA", width, height, pixels.Length);
            return pixels;
        }

        /// <summary>
        /// Reads all bytes from a file asynchronously and detects the image format
        /// based on magic bytes in the header.
        /// </summary>
        /// <param name="filePath">Path to the image file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The detected image format string (e.g. "BMP", "PNG", "JPEG", "UNKNOWN").</returns>
        public async Task<string> DetectFormatAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            var header = new byte[8];
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            int read = await fs.ReadAsync(header.AsMemory(0, 8), cancellationToken).ConfigureAwait(false);

            if (read < 4)
                return "UNKNOWN";

            // Check magic bytes
            if (header[0] == 0x42 && header[1] == 0x4D)
                return "BMP";
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return "PNG";
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return "JPEG";
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46)
                return "GIF";
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
                return "WEBP";

            _logger.LogWarning("Unknown image format in {Path}", filePath);
            return "UNKNOWN";
        }

        /// <summary>
        /// Estimates the uncompressed RGBA memory footprint for an image of given dimensions.
        /// </summary>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <returns>Required memory in bytes (4 bytes per pixel).</returns>
        public static long EstimateMemoryUsage(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            return (long)width * height * 4;
        }
    }
}
