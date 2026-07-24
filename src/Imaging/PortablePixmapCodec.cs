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
/// Portable Pixmap (Netpbm) codec implementation.
/// Supports P6 (binary RGB PPM), P5 (binary grayscale PGM), and P3 (ASCII RGB PPM) formats.
/// These formats are dependency-free, byte-exact and trivial to round-trip, making them
/// ideal as a portable on-disk representation for the batch CLI and for golden-image
/// regression fixtures.
/// </summary>
public sealed class PortablePixmapCodec : IImageCodec
{
    /// <summary>
    /// Singleton instance of the PortablePixmapCodec.
    /// </summary>
    public static readonly PortablePixmapCodec Instance = new();

    /// <summary>File extensions recognised as portable pixmaps.</summary>
    private static readonly string[] _extensions = [".ppm", ".pgm"];

    /// <summary>
    /// PPM/PGM format variants supported by this codec.
    /// </summary>
    private enum PpmFormat
    {
        /// <summary>Binary P6 format (24-bit RGB PPM).</summary>
        P6,

        /// <summary>ASCII P3 format (768-column plain text PPM).</summary>
        P3
    }

    /// <inheritdoc />
    public string Format => "Portable Pixmap (Netpbm)";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedExtensions => _extensions;

    /// <inheritdoc />
    public byte[]? MagicBytes => null; // Magic bytes vary by format variant

    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private PortablePixmapCodec() { }

    /// <inheritdoc />
    public bool CanRead(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var ext = Path.GetExtension(path);
        return CanReadByExtension(ext);
    }

    /// <inheritdoc />
    public bool CanReadByExtension(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        return _extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool CanReadFromStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // Check magic bytes at the start of the stream
        // P6 starts with "P6\n", P5 starts with "P5\n", P3 starts with "P3\n"
        Span<byte> header = stackalloc byte[2];
        int bytesRead = stream.Read(header);

        if (bytesRead < 2)
        {
            return false;
        }

        // Restore stream position
        stream.Position -= bytesRead;

        return header[0] == 'P' && (header[1] == '3' || header[1] == '5' || header[1] == '6');
    }

    /// <inheritdoc />
    public Image Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        string magic = ReadToken(stream);
        int channels = magic switch
        {
            "P6" => 3,
            "P5" => 1,
            "P3" => 3,
            _ => throw new NotSupportedException($"Unsupported pixmap magic '{magic}' (only P3/P5/P6 are handled).")
        };

        int width = int.Parse(ReadToken(stream), CultureInfo.InvariantCulture);
        int height = int.Parse(ReadToken(stream), CultureInfo.InvariantCulture);
        int maxVal = int.Parse(ReadToken(stream), CultureInfo.InvariantCulture);
        if (maxVal is <= 0 or > 255)
        {
            throw new NotSupportedException($"Only 8-bit channels are supported (max value {maxVal}).");
        }

        int length = checked(width * height * channels);
        var pixels = new byte[length];
        int read = 0;
        while (read < length)
        {
            int n = stream.Read(pixels, read, length - read);
            if (n <= 0)
            {
                throw new EndOfStreamException($"Truncated pixmap: expected {length} bytes, got {read}.");
            }
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
    /// Writes an image to the specified stream.
    /// </summary>
    /// <inheritdoc />
    public void Write(Image image, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(stream);
        if (image.PixelData is null)
        {
            throw new InvalidOperationException("Image has no pixel data to write.");
        }

        // Determine format based on channels
        string magic = image.Channels switch
        {
            1 => "P5",
            3 => "P6",
            _ => throw new InvalidOperationException("Only 1 or 3 channel images are supported by PortablePixmapCodec.")
        };

        var header = Encoding.ASCII.GetBytes($"{magic}\n{image.Width} {image.Height}\n255\n");
        stream.Write(header, 0, header.Length);

        // Write pixel data
        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        if (bpp == image.Channels)
        {
            stream.Write(image.PixelData, 0, image.Width * image.Height * image.Channels);
            return;
        }

        // Repack when the in-memory stride differs from the on-disk channel count
        var buffer = new byte[image.Width * image.Height * image.Channels];
        int pixelCount = image.Width * image.Height;
        for (int p = 0; p < pixelCount; p++)
        {
            for (int c = 0; c < image.Channels; c++)
            {
                buffer[p * image.Channels + c] = image.PixelData[p * bpp + c];
            }
        }
        stream.Write(buffer, 0, buffer.Length);
    }

    /// <summary>
    /// Reads a token from the stream, skipping whitespace and comments.
    /// </summary>
    private static string ReadToken(Stream stream)
    {
        var sb = new StringBuilder(8);
        int c;

        // Skip leading whitespace and '#'-comment lines
        while (true)
        {
            c = stream.ReadByte();
            if (c < 0)
            {
                throw new EndOfStreamException("Unexpected end of pixmap header.");
            }
            if (c == '#')
            {
                while (c is not ('\n' or -1))
                {
                    c = stream.ReadByte();
                }
                continue;
            }
            if (!char.IsWhiteSpace((char)c))
            {
                break;
            }
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