#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Imaging;

/// <summary>
/// Minimal reader/writer for the Netpbm formats: P6 (binary RGB PPM),
/// P5 (binary grayscale PGM), and P3 (ASCII RGB PPM).
///
/// These formats are dependency-free, byte-exact and trivial to round-trip,
/// which makes them ideal as a portable on-disk representation for the batch
/// CLI and for golden-image regression fixtures. The pixel buffers produced
/// here are laid out row-major with <c>bpp</c> bytes per pixel, exactly what
/// <see cref="GpuImageProcessing.Fallback.CpuImageProcessor"/> expects.
///
/// This class now delegates to <see cref="PortablePixmapCodec"/> for actual implementation.
/// </summary>
public static class PortablePixmap
{
    /// <summary>File extensions recognised as portable pixmaps.</summary>
    public static readonly string[] Extensions = [".ppm", ".pgm"];

    /// <summary>
    /// PPM/PGM format variants supported by this class.
    /// </summary>
    public enum PpmFormat
    {
        /// <summary>Binary P6 format (24-bit RGB PPM).</summary>
        P6,

        /// <summary>ASCII P3 format (768-column plain text PPM).</summary>
        P3
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="path"/> has a
    /// recognised portable-pixmap extension.
    /// </summary>
    public static bool IsSupported(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var ext = System.IO.Path.GetExtension(path);
        foreach (var candidate in Extensions)
        {
            if (string.Equals(ext, candidate, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Loads a P3/P5/P6 image file into a fully-populated <see cref="Image"/>
    /// (dimensions, channels, bits-per-pixel and raw pixel data).
    /// </summary>
    public static Image Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        var image = Decode(stream);
        image.FilePath = path;
        image.FileName = System.IO.Path.GetFileName(path);
        image.FileSizeBytes = new FileInfo(path).Length;
        return image;
    }

    /// <summary>
    /// Decodes a P3/P5/P6 stream into an <see cref="Image"/>. Kept separate from
    /// <see cref="Load"/> so callers can decode from any stream (memory, tests).
    /// </summary>
    public static Image Decode(Stream stream)
    {
        return PortablePixmapCodec.Instance.Read(stream);
    }

    /// <summary>
    /// Writes <paramref name="image"/> to <paramref name="path"/> as a binary
    /// P6 (3+ channels) or P5 (single channel) pixmap, or as ASCII P3 (RGB only).
    /// </summary>
    /// <param name="image">Image to save.</param>
    /// <param name="path">Output file path.</param>
    /// <param name="format">Output format (P6 binary by default).</param>
    public static void Save(Image image, string path, PpmFormat format = PpmFormat.P6)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (image.PixelData is null)
            throw new InvalidOperationException("Image has no pixel data to write.");

        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        // Convert to codec format
        var codec = PortablePixmapCodec.Instance;
        var codecImage = new Image
        {
            Width = image.Width,
            Height = image.Height,
            Channels = image.Channels,
            BitsPerPixel = image.BitsPerPixel,
            PixelData = image.PixelData,
            FileSizeBytes = image.FileSizeBytes
        };

        using var stream = File.Create(path);
        codec.Write(codecImage, stream);
    }

    /// <summary>
    /// Computes a stable lowercase hex SHA-256 digest over the raw pixel data.
    /// Golden-image tests compare against these digests to detect any change in
    /// filter output down to a single byte.
    /// </summary>
    public static string PixelHash(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return PixelHash(image.PixelData ?? []);
    }

    /// <summary>
    /// Computes a stable lowercase hex SHA-256 digest over a pixel buffer.
    /// </summary>
    public static string PixelHash(ReadOnlySpan<byte> pixels)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(pixels, digest);
        return Convert.ToHexStringLower(digest);
    }

    /// <summary>
    /// Writes an image in ASCII P3 format (plain text PPM).
    /// This is kept for backward compatibility but PortablePixmapCodec handles the actual writing.
    /// </summary>
    private static void WriteAsciiP3(Image image, Stream stream)
    {
        // ASCII P3 format: each pixel component is written as a decimal number
        // with values from 0 to maxval (255), separated by whitespace.
        // Lines should be no longer than 768 characters for readability.

        var header = Encoding.ASCII.GetBytes(
            $"P3\n{image.Width} {image.Height}\n255\n");
        stream.Write(header, 0, header.Length);

        int pixelCount = image.Width * image.Height;
        int componentsPerPixel = 3; // RGB

        for (int p = 0; p < pixelCount; p++)
        {
            for (int c = 0; c < componentsPerPixel; c++)
            {
                // Get the byte value (0-255)
                byte value = image.PixelData[p * componentsPerPixel + c];

                // Write as ASCII decimal number
                string valueStr = value.ToString(CultureInfo.InvariantCulture);
                var bytes = Encoding.ASCII.GetBytes(valueStr);
                stream.Write(bytes, 0, bytes.Length);

                // Add whitespace separator (space or newline for line wrapping)
                // Try to keep lines under 768 characters by adding newlines
                if ((p * componentsPerPixel + c + 1) % 16 == 0)
                    stream.WriteByte((byte)'\n');
                else
                    stream.WriteByte((byte)' ');
            }
        }

        // Ensure file ends with newline
        stream.WriteByte((byte)'\n');
    }
}