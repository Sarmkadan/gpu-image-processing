#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.IO;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Imaging;

/// <summary>
/// Defines a codec for reading and writing image formats.
/// Implementations should be stateless and thread-safe.
/// </summary>
public interface IImageCodec
{
    /// <summary>
    /// Gets the format identifier for this codec (e.g., "PPM", "PGM", "PNG").
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Gets the file extensions supported by this codec (e.g.,[".ppm", ".pgm"]).
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Gets the magic bytes that identify this format in a file header.
    /// Returns null if the format doesn't use magic bytes.
    /// </summary>
    byte[]? MagicBytes { get; }

    /// <summary>
    /// Determines if this codec can read the specified file path.
    /// </summary>
    /// <param name="path">File path to check.</param>
    /// <returns>True if this codec can read the file.</returns>
    bool CanRead(string path);

    /// <summary>
    /// Determines if this codec can read the specified file extension.
    /// </summary>
    /// <param name="extension">File extension to check (including leading dot).</param>
    /// <returns>True if this codec can read the extension.</returns>
    bool CanReadByExtension(string extension);

    /// <summary>
    /// Determines if this codec can read from the specified stream by checking magic bytes.
    /// </summary>
    /// <param name="stream">Stream to check.</param>
    /// <returns>True if the stream appears to be in this format.</returns>
    bool CanReadFromStream(Stream stream);

    /// <summary>
    /// Reads an image from the specified stream.
    /// </summary>
    /// <param name="stream">Stream containing image data.</param>
    /// <returns>Image with pixel data and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown if stream is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if the stream contains invalid data.</exception>
    /// <exception cref="NotSupportedException">Thrown if the stream format is not supported.</exception>
    Image Read(Stream stream);

    /// <summary>
    /// Writes an image to the specified stream.
    /// </summary>
    /// <param name="image">Image to write.</param>
    /// <param name="stream">Stream to write to.</param>
    /// <exception cref="ArgumentNullException">Thrown if image or stream is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if image has no pixel data.</exception>
    void Write(Image image, Stream stream);
}