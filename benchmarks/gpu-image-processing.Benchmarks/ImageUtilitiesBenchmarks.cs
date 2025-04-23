#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Utilities;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Benchmarks for ImageUtilities hot paths: extension validation, MIME resolution,
/// and file-size formatting. These are called per-image in the ingestion pipeline.
/// </summary>
[MemoryDiagnoser]
public class ImageUtilitiesBenchmarks
{
    // --- IsSupportedImageFile -------------------------------------------------

    [Benchmark]
    public bool IsSupportedImageFile_Jpeg()
        => ImageUtilities.IsSupportedImageFile("render_2024_final_v3.jpg");

    [Benchmark]
    public bool IsSupportedImageFile_WebP()
        => ImageUtilities.IsSupportedImageFile("banner.webp");

    [Benchmark]
    public bool IsSupportedImageFile_Unsupported()
        => ImageUtilities.IsSupportedImageFile("report.pdf");

    // --- FormatFileSize -------------------------------------------------------

    [Benchmark]
    public string FormatFileSize_Kilobytes()
        => ImageUtilities.FormatFileSize(512_000);

    [Benchmark]
    public string FormatFileSize_Megabytes()
        => ImageUtilities.FormatFileSize(4_718_592);      // 4.5 MB JPEG

    [Benchmark]
    public string FormatFileSize_Gigabytes()
        => ImageUtilities.FormatFileSize(10_737_418_240L); // 10 GB RAW batch

    // --- GetMimeType / GetImageFormat ----------------------------------------

    [Benchmark]
    public string? GetMimeType_Jpeg()
        => ImageUtilities.GetMimeType("photo.jpg");

    [Benchmark]
    public string? GetMimeType_Png()
        => ImageUtilities.GetMimeType("screenshot.png");

    [Benchmark]
    public string? GetImageFormat_Tiff()
        => ImageUtilities.GetImageFormat("scan.tiff");

    // --- CalculateProportionalSize --------------------------------------------

    [Benchmark]
    public (int, int) CalculateProportionalSize_2x()
        => ImageUtilities.CalculateProportionalSize(1920, 1080, 2.0);
}
