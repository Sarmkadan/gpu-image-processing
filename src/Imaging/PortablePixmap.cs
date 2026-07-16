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
/// Minimal reader/writer for the binary Netpbm formats: P6 (24-bit RGB PPM)
/// and P5 (8-bit grayscale PGM).
///
/// These formats are dependency-free, byte-exact and trivial to round-trip,
/// which makes them ideal as a portable on-disk representation for the batch
/// CLI and for golden-image regression fixtures. The pixel buffers produced
/// here are laid out row-major with <c>bpp</c> bytes per pixel, exactly what
/// <see cref="GpuImageProcessing.Fallback.CpuImageProcessor"/> expects.
/// </summary>
public static class PortablePixmap
{
    /// <summary>File extensions recognised as portable pixmaps.</summary>
    public static readonly string[] Extensions = [".ppm", ".pgm"];

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="path"/> has a
    /// recognised portable-pixmap extension.
    /// </summary>
    public static bool IsSupported(string path)
    {
        var ext = System.IO.Path.GetExtension(path);
        foreach (var candidate in Extensions)
        {
            if (string.Equals(ext, candidate, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Loads a P5/P6 image file into a fully-populated <see cref="Image"/>
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
    /// Decodes a P5/P6 stream into an <see cref="Image"/>. Kept separate from
    /// <see cref="Load"/> so callers can decode from any stream (memory, tests).
    /// </summary>
    public static Image Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        string magic = ReadToken(stream);
        int channels = magic switch
        {
            "P6" => 3,
            "P5" => 1,
            _ => throw new NotSupportedException($"Unsupported pixmap magic '{magic}' (only P5/P6 are handled).")
        };

        int width = int.Parse(ReadToken(stream), CultureInfo.InvariantCulture);
        int height = int.Parse(ReadToken(stream), CultureInfo.InvariantCulture);
        int maxVal = int.Parse(ReadToken(stream), CultureInfo.InvariantCulture);
        if (maxVal is <= 0 or > 255)
            throw new NotSupportedException($"Only 8-bit channels are supported (max value {maxVal}).");

        int length = checked(width * height * channels);
        var pixels = new byte[length];
        int read = 0;
        while (read < length)
        {
            int n = stream.Read(pixels, read, length - read);
            if (n <= 0)
                throw new EndOfStreamException($"Truncated pixmap: expected {length} bytes, got {read}.");
            read += n;
        }

        return new Image
        {
            Width = width,
            Height = height,
            Channels = channels,
            BitsPerPixel = channels * 8,
            PixelData = pixels,
            FileSizeBytes = length
        };
    }

    /// <summary>
    /// Writes <paramref name="image"/> to <paramref name="path"/> as a binary
    /// P6 (3+ channels) or P5 (single channel) pixmap.
    /// </summary>
    public static void Save(Image image, string path)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (image.PixelData is null)
            throw new InvalidOperationException("Image has no pixel data to write.");

        var dir = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        // The batch pipeline may run grayscale filters that keep the original
        // channel count; we honour whatever the buffer actually carries.
        string magic = bpp >= 3 ? "P6" : "P5";
        int outChannels = bpp >= 3 ? 3 : 1;

        using var stream = File.Create(path);
        var header = Encoding.ASCII.GetBytes(
            $"{magic}\n{image.Width} {image.Height}\n255\n");
        stream.Write(header, 0, header.Length);

        if (bpp == outChannels)
        {
            stream.Write(image.PixelData, 0, image.Width * image.Height * outChannels);
            return;
        }

        // Repack when the in-memory stride differs from the on-disk channel count.
        var buffer = new byte[image.Width * image.Height * outChannels];
        int pixelCount = image.Width * image.Height;
        for (int p = 0; p < pixelCount; p++)
        {
            for (int c = 0; c < outChannels; c++)
                buffer[p * outChannels + c] = image.PixelData[p * bpp + c];
        }
        stream.Write(buffer, 0, buffer.Length);
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

    private static string ReadToken(Stream stream)
    {
        var sb = new StringBuilder(8);
        int c;

        // Skip leading whitespace and '#'-comment lines.
        while (true)
        {
            c = stream.ReadByte();
            if (c < 0)
                throw new EndOfStreamException("Unexpected end of pixmap header.");
            if (c == '#')
            {
                while (c is not ('\n' or -1))
                    c = stream.ReadByte();
                continue;
            }
            if (!char.IsWhiteSpace((char)c))
                break;
        }

        do
        {
            sb.Append((char)c);
            c = stream.ReadByte();
        }
        while (c >= 0 && !char.IsWhiteSpace((char)c));

        return sb.ToString();
    }
}
