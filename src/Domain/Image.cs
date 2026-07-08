#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Represents an image entity with metadata and processing information.
/// </summary>
public class Image
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public ImageFormat Format { get; set; }
    public ColorSpace ColorSpace { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Channels { get; set; }
    public int BitsPerPixel { get; set; }
    public long FileSizeBytes { get; set; }
    public byte[]? PixelData { get; set; }
    public ProcessingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ProcessedOutputPath { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public Image()
    {
        Id = Guid.NewGuid();
        Status = ProcessingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates an in-memory GPU-side image buffer with the given dimensions.
    /// </summary>
    public Image(int width, int height, int channels) : this()
    {
        Width = width;
        Height = height;
        Channels = channels;
    }

    /// <summary>
    /// Validates image properties before processing.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            return false;

        if (Width < AppConstants.Processing.MinImageWidth || Width > AppConstants.Processing.MaxImageWidth)
            return false;

        if (Height < AppConstants.Processing.MinImageHeight || Height > AppConstants.Processing.MaxImageHeight)
            return false;

        if (FileSizeBytes > AppConstants.Memory.MaxMemoryPerImage)
            return false;

        if (BitsPerPixel is not (8 or 16 or 24 or 32))
            return false;

        return true;
    }

    /// <summary>
    /// Calculates the memory footprint of pixel data in bytes.
    /// </summary>
    public long CalculatePixelDataSize()
    {
        var bytesPerPixel = BitsPerPixel / 8;
        return Width * Height * bytesPerPixel;
    }

    /// <summary>
    /// Updates the modification timestamp and status.
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = ProcessingStatus.Processing;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes processing and sets output path.
    /// </summary>
    public void MarkAsCompleted(string outputPath)
    {
        Status = ProcessingStatus.Completed;
        ProcessedOutputPath = outputPath;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks processing as failed with error message.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = ProcessingStatus.Failed;
        Metadata["error"] = errorMessage;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets aspect ratio of the image.
    /// </summary>
    public double GetAspectRatio() => (double)Width / Height;

    /// <summary>
    /// Gets total pixel count.
    /// </summary>
    public long GetPixelCount() => (long)Width * Height;
}
