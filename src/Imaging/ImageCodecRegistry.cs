#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.IO;

namespace GpuImageProcessing.Imaging;

/// <summary>
/// Central registry for discovering and selecting image codecs by file extension or magic bytes.
/// Codecs are registered once at startup and queried by the registry.
/// </summary>
public static class ImageCodecRegistry
{
    private static readonly List<IImageCodec> _codecs = [];
    private static readonly object _lock = new();

    /// <summary>
    /// Gets all registered codecs in registration order.
    /// </summary>
    public static IReadOnlyList<IImageCodec> Codecs => _codecs.AsReadOnly();

    static ImageCodecRegistry()
    {
        // Register built-in codecs in order of preference/fallback
        Register(PortablePixmapCodec.Instance);
    }

    /// <summary>
    /// Registers a codec with the registry.
    /// </summary>
    /// <param name="codec">Codec to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if codec is null.</exception>
    public static void Register(IImageCodec codec)
    {
        ArgumentNullException.ThrowIfNull(codec);

        lock (_lock)
        {
            if (!_codecs.Contains(codec))
            {
                _codecs.Add(codec);
            }
        }
    }

    /// <summary>
    /// Finds the first codec that can read the specified file path.
    /// </summary>
    /// <param name="path">File path to check.</param>
    /// <returns>Matching codec or null if none found.</returns>
    public static IImageCodec? FindCodec(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var extension = Path.GetExtension(path);
        return FindCodecByExtension(extension);
    }

    /// <summary>
    /// Finds the first codec that can read the specified file extension.
    /// </summary>
    /// <param name="extension">File extension to check (including leading dot).</param>
    /// <returns>Matching codec or null if none found.</returns>
    public static IImageCodec? FindCodecByExtension(string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);

        lock (_lock)
        {
            foreach (var codec in _codecs)
            {
                if (codec.CanReadByExtension(extension))
                {
                    return codec;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first codec whose magic bytes match the beginning of the stream.
    /// </summary>
    /// <param name="stream">Stream to check.</param>
    /// <returns>Matching codec or null if none found.</returns>
    public static IImageCodec? FindCodecByMagicBytes(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // Save position to restore later
        var originalPosition = stream.Position;

        try
        {
            lock (_lock)
            {
                foreach (var codec in _codecs)
                {
                    if (codec.MagicBytes != null)
                    {
                        // Read magic bytes from stream
                        var magicBytes = codec.MagicBytes;
                        var buffer = ArrayPool<byte>.Shared.Rent(magicBytes.Length);

                        try
                        {
                            int bytesRead = stream.Read(buffer, 0, magicBytes.Length);
                            if (bytesRead == magicBytes.Length)
                            {
                                // Compare magic bytes
                                bool matches = true;
                                for (int i = 0; i < magicBytes.Length; i++)
                                {
                                    if (buffer[i] != magicBytes[i])
                                    {
                                        matches = false;
                                        break;
                                    }
                                }

                                if (matches)
                                {
                                    return codec;
                                }
                            }
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                }
            }
        }
        finally
        {
            // Restore stream position
            if (stream.Position != originalPosition)
            {
                stream.Position = originalPosition;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the first codec that can read from the specified stream.
    /// </summary>
    /// <param name="stream">Stream to check.</param>
    /// <returns>Matching codec or null if none found.</returns>
    public static IImageCodec? FindCodecForStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        // Try magic bytes first
        var byMagic = FindCodecByMagicBytes(stream);
        if (byMagic != null)
        {
            return byMagic;
        }

        // Fall back to extension-based lookup (requires stream to be seekable)
        if (stream.CanSeek)
        {
            var originalPosition = stream.Position;
            try
            {
                // Try to get extension from path if available
                if (stream is FileStream fileStream && !string.IsNullOrEmpty(fileStream.Name))
                {
                    var extension = Path.GetExtension(fileStream.Name);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        return FindCodecByExtension(extension);
                    }
                }
            }
            finally
            {
                if (stream.Position != originalPosition)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        return null;
    }
}