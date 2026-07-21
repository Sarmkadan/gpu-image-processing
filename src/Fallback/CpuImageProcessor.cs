#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Exceptions;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Fallback;

/// <summary>
/// CPU-only image processor used when no OpenCL-capable device is available.
/// Implements resize, grayscale, and blur using raw pixel manipulation so the
/// library remains functional on machines without a compatible GPU.
/// </summary>
public sealed class CpuImageProcessor : IImageProcessor
{
    private static readonly HashSet<FilterType> _supported =
    [
        FilterType.Grayscale,
        FilterType.Blur,
        FilterType.GaussianBlur,
        FilterType.Sharpen,
        FilterType.EdgeDetection,
        FilterType.Threshold
    ];

    private readonly ILogger<CpuImageProcessor> _logger;

    public CpuImageProcessor(ILogger<CpuImageProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("CPU image processor initialised (no OpenCL device available).");
    }

    /// <inheritdoc />
    public bool CanProcess(FilterType filterType) => _supported.Contains(filterType);

    /// <inheritdoc />
    public async Task<Image> ApplyFilterAsync(
        Image image,
        FilterConfiguration config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(config);

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("CPU applying {FilterType} to image {ImageId} ({Width}x{Height})",
            config.FilterType, image.Id, image.Width, image.Height);

        try
        {
            var result = await Task.Run(() => ApplyCore(image, config, cancellationToken), cancellationToken);
            sw.Stop();
            _logger.LogDebug("CPU {FilterType} completed in {Elapsed}ms for image {ImageId}",
                config.FilterType, sw.ElapsedMilliseconds, image.Id);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CPU filter {FilterType} cancelled for image {ImageId}",
                config.FilterType, image.Id);
            throw;
        }
        catch (Exception ex) when (ex is not ProcessingException)
        {
            _logger.LogError(ex, "CPU fallback failed applying {FilterType} to image {ImageId}",
                config.FilterType, image.Id);
            throw new ProcessingException(
                $"CPU filter application failed: {config.Name}", ex,
                image.FilePath, config.Name);
        }
    }

    /// <inheritdoc />
    public Image Resize(Image image, int targetWidth, int targetHeight)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (targetWidth <= 0) throw new ArgumentOutOfRangeException(nameof(targetWidth));
        if (targetHeight <= 0) throw new ArgumentOutOfRangeException(nameof(targetHeight));

        int bpp  = Math.Max(1, image.BitsPerPixel / 8);
        var src  = image.PixelData ?? new byte[image.Width * image.Height * bpp];
        var dst  = new byte[targetWidth * targetHeight * bpp];

        float xRatio = (float)image.Width  / targetWidth;
        float yRatio = (float)image.Height / targetHeight;

        Span<byte> dstSpan = dst.AsSpan();
        for (int y = 0; y < targetHeight; y++)
        for (int x = 0; x < targetWidth;  x++)
        {
            // Bilinear interpolation
            float srcX = x * xRatio;
            float srcY = y * yRatio;
            int   x0   = (int)srcX;
            int   y0   = (int)srcY;
            int   x1   = Math.Min(x0 + 1, image.Width  - 1);
            int   y1   = Math.Min(y0 + 1, image.Height - 1);
            float dx   = srcX - x0;
            float dy   = srcY - y0;

            int dstIdx = (y * targetWidth + x) * bpp;
            for (int c = 0; c < bpp; c++)
            {
                float v = src[(y0 * image.Width + x0) * bpp + c] * (1 - dx) * (1 - dy)
                        + src[(y0 * image.Width + x1) * bpp + c] * dx       * (1 - dy)
                        + src[(y1 * image.Width + x0) * bpp + c] * (1 - dx) * dy
                        + src[(y1 * image.Width + x1) * bpp + c] * dx       * dy;
                dstSpan[dstIdx + c] = (byte)Math.Clamp((int)v, 0, 255);
            }
        }

        image.PixelData  = dst;
        image.Width      = targetWidth;
        image.Height     = targetHeight;
        image.ModifiedAt = DateTime.UtcNow;
        return image;
    }

    /// <inheritdoc />
    public Image ToGrayscale(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);
        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        var src = image.PixelData ?? new byte[image.Width * image.Height * bpp];
        image.PixelData  = CpuFilters.Grayscale(src, bpp);
        image.ModifiedAt = DateTime.UtcNow;
        return image;
    }

    /// <inheritdoc />
    public Image Blur(Image image, int radius = 1)
    {
        ArgumentNullException.ThrowIfNull(image);
        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        var src = image.PixelData ?? new byte[image.Width * image.Height * bpp];
        image.PixelData  = CpuFilters.BoxBlur(src, image.Width, image.Height, bpp, radius);
        image.ModifiedAt = DateTime.UtcNow;
        return image;
    }

/// <inheritdoc />
public Image Crop(Image image, int x, int y, int width, int height)
{
    ArgumentNullException.ThrowIfNull(image);

    if (x < 0) throw new ValidationException("Crop x coordinate cannot be negative", nameof(x));
    if (y < 0) throw new ValidationException("Crop y coordinate cannot be negative", nameof(y));
    if (width <= 0) throw new ValidationException("Crop width must be positive", nameof(width));
    if (height <= 0) throw new ValidationException("Crop height must be positive", nameof(height));

    if (x >= image.Width) throw new ValidationException("Crop x coordinate is beyond image width", nameof(x), new Dictionary<string, string> {
        {"ImageWidth", image.Width.ToString()},
        {"CropX", x.ToString()}
    });
    if (y >= image.Height) throw new ValidationException("Crop y coordinate is beyond image height", nameof(y), new Dictionary<string, string> {
        {"ImageHeight", image.Height.ToString()},
        {"CropY", y.ToString()}
    });

    int maxWidth = image.Width - x;
    int maxHeight = image.Height - y;

    if (width > maxWidth) throw new ValidationException("Crop width exceeds available width from x coordinate", nameof(width), new Dictionary<string, string> {
        {"ImageWidth", image.Width.ToString()},
        {"CropX", x.ToString()},
        {"RequestedWidth", width.ToString()},
        {"AvailableWidth", maxWidth.ToString()}
    });

    if (height > maxHeight) throw new ValidationException("Crop height exceeds available height from y coordinate", nameof(height), new Dictionary<string, string> {
        {"ImageHeight", image.Height.ToString()},
        {"CropY", y.ToString()},
        {"RequestedHeight", height.ToString()},
        {"AvailableHeight", maxHeight.ToString()}
    });

    int bpp = Math.Max(1, image.BitsPerPixel / 8);
    var src = image.PixelData ?? new byte[image.Width * image.Height * bpp];

    var dst = new byte[width * height * bpp];
        Span<byte> dstSpan = dst.AsSpan();

    for (int srcY = y, dstY = 0; dstY < height; srcY++, dstY++)
    {
        int srcRowStart = srcY * image.Width * bpp;
        int dstRowStart = dstY * width * bpp;

        for (int srcX = x * bpp, dstX = 0; dstX < width * bpp; srcX += bpp, dstX += bpp)
        {
            int srcIdx = srcRowStart + srcX;
            int dstIdx = dstRowStart + dstX;

            for (int c = 0; c < bpp; c++)
            {
                dstSpan[dstIdx + c] = src[srcIdx + c];
            }
        }
    }

    image.PixelData = dst;
    image.Width = width;
    image.Height = height;
    image.ModifiedAt = DateTime.UtcNow;
    return image;
}

    // ── Dispatch ──────────────────────────────────────────────────────────────

    private Image ApplyCore(Image image, FilterConfiguration config, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        var src = image.PixelData ?? new byte[image.Width * image.Height * bpp];

        image.PixelData = config.FilterType switch
        {
            FilterType.Grayscale                       => CpuFilters.Grayscale(src, bpp),
            FilterType.Blur or FilterType.GaussianBlur => CpuFilters.BoxBlur(src, image.Width, image.Height, bpp),
            FilterType.Sharpen                         => CpuFilters.Convolve(src, image.Width, image.Height, bpp, CpuFilters.SharpenKernel),
            FilterType.EdgeDetection                   => CpuFilters.Sobel(src, image.Width, image.Height, bpp),
            FilterType.Threshold                       => CpuFilters.Threshold(src, FloatParam(config, "thresholdValue", 0.5f)),
            _                                          => src
        };

        image.ModifiedAt = DateTime.UtcNow;
        image.Metadata["processor"] = "cpu_fallback";
        return image;
    }

    private static float FloatParam(FilterConfiguration config, string key, float fallback) =>
        config.Parameters.TryGetValue(key, out var raw) && raw is float f ? f : fallback;
}

/// <summary>
/// Pure-static CPU filter kernel implementations shared by
/// <see cref="CpuImageProcessor"/> and related utilities.
/// </summary>
internal static class CpuFilters
{
    // BT.601 luma weights
    public static byte[] Grayscale(ReadOnlySpan<byte> src, int bpp)
    {
        var dst = new byte[src.Length];
        int rgb = Math.Min(bpp, 3);
        for (int p = 0; p <= src.Length - bpp; p += bpp)
        {
            byte y = (byte)((77 * src[p] + 150 * src[p + 1] + 29 * src[p + 2]) >> 8);
            for (int c = 0; c < rgb; c++) dst[p + c] = y;
            if (bpp > 3) dst[p + 3] = src[p + 3];
        }
        return dst;
    }

    public static byte[] Threshold(ReadOnlySpan<byte> src, float threshold)
    {
        byte limit = (byte)Math.Clamp(threshold * 255f, 0f, 255f);
        var dst    = new byte[src.Length];
        for (int i = 0; i < src.Length; i++)
            dst[i] = src[i] >= limit ? (byte)255 : (byte)0;
        return dst;
    }

    public static byte[] BoxBlur(ReadOnlySpan<byte> src, int w, int h, int bpp, int radius = 1)
    {
        var dst      = new byte[src.Length];
        Span<byte> dstSpan = dst.AsSpan();
        int channels = Math.Min(bpp, 3);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int px = (y * w + x) * bpp;
            for (int c = 0; c < channels; c++)
            {
                int sum = 0, n = 0;
                for (int dy = -radius; dy <= radius; dy++)
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int ny = y + dy, nx = x + dx;
                    if ((uint)ny < (uint)h && (uint)nx < (uint)w)
                    {
                        sum += src[(ny * w + nx) * bpp + c];
                        n++;
                    }
                }
                dstSpan[px + c] = (byte)(sum / n);
            }
            if (bpp > 3) dstSpan[px + 3] = src[px + 3];
        }
        return dst;
    }

    public static readonly int[] SharpenKernel = [-1, -1, -1, -1, 9, -1, -1, -1, -1];

    public static byte[] Convolve(ReadOnlySpan<byte> src, int w, int h, int bpp, ReadOnlySpan<int> k)
    {
        var dst      = new byte[src.Length];
        Span<byte> dstSpan = dst.AsSpan();
        int channels = Math.Min(bpp, 3);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int px = (y * w + x) * bpp;
            for (int c = 0; c < channels; c++)
            {
                int acc = 0;
                for (int ky = -1; ky <= 1; ky++)
                for (int kx = -1; kx <= 1; kx++)
                    acc += Sample(src, w, h, bpp, c, x + kx, y + ky) * k[(ky + 1) * 3 + (kx + 1)];
                dstSpan[px + c] = (byte)Math.Clamp(acc, 0, 255);
            }
            if (bpp > 3) dstSpan[px + 3] = src[px + 3];
        }
        return dst;
    }

    public static byte[] Sobel(ReadOnlySpan<byte> src, int w, int h, int bpp)
    {
        var dst      = new byte[src.Length];
        Span<byte> dstSpan = dst.AsSpan();
        int channels = Math.Min(bpp, 3);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int px = (y * w + x) * bpp;
            for (int c = 0; c < channels; c++)
            {
                int gx = -Sample(src, w, h, bpp, c, x - 1, y - 1)
                       +  Sample(src, w, h, bpp, c, x + 1, y - 1)
                       - 2 * Sample(src, w, h, bpp, c, x - 1, y)
                       + 2 * Sample(src, w, h, bpp, c, x + 1, y)
                       - Sample(src, w, h, bpp, c, x - 1, y + 1)
                       +  Sample(src, w, h, bpp, c, x + 1, y + 1);

                int gy = -Sample(src, w, h, bpp, c, x - 1, y - 1)
                       - 2 * Sample(src, w, h, bpp, c, x, y - 1)
                       - Sample(src, w, h, bpp, c, x + 1, y - 1)
                       +  Sample(src, w, h, bpp, c, x - 1, y + 1)
                       + 2 * Sample(src, w, h, bpp, c, x, y + 1)
                       +  Sample(src, w, h, bpp, c, x + 1, y + 1);

                dstSpan[px + c] = (byte)Math.Clamp((int)Math.Sqrt(gx * gx + gy * gy), 0, 255);
            }
            if (bpp > 3) dstSpan[px + 3] = src[px + 3];
        }
        return dst;
    }

    private static int Sample(ReadOnlySpan<byte> src, int w, int h, int bpp, int ch, int x, int y)
    {
        x = Math.Clamp(x, 0, w - 1);
        y = Math.Clamp(y, 0, h - 1);
        return src[(y * w + x) * bpp + ch];
    }
}
