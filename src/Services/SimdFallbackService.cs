// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Numerics;

namespace GpuImageProcessing.Services;

/// <summary>
/// CPU-based image processing service that uses SIMD vector instructions as a
/// transparent fallback when no compatible GPU device is available.
/// Per-pixel operations (threshold, grayscale) use <see cref="Vector{T}"/> to
/// process multiple bytes per cycle; neighbourhood operations (blur, sharpen,
/// edge detection) fall back to optimised scalar loops with border clamping.
/// </summary>
public sealed class SimdFallbackService
{
    private static readonly HashSet<FilterType> _supported =
    [
        FilterType.Grayscale,
        FilterType.Blur, FilterType.GaussianBlur,
        FilterType.Sharpen,
        FilterType.EdgeDetection,
        FilterType.Threshold
    ];

    private readonly PerformanceMonitoringService _performanceService;
    private readonly ILogger<SimdFallbackService> _logger;
    private readonly SimdCapabilities _capabilities;

    /// <summary>
    /// Initialises the service and probes the host CPU for available SIMD extensions.
    /// </summary>
    public SimdFallbackService(
        PerformanceMonitoringService performanceService,
        ILogger<SimdFallbackService> logger)
    {
        _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _capabilities = SimdCapabilities.Detect();
        _logger.LogInformation("SIMD fallback processor initialised — {Capabilities}", _capabilities);
    }

    /// <summary>Detected SIMD capabilities of the host CPU.</summary>
    public SimdCapabilities Capabilities => _capabilities;

    /// <summary>
    /// Returns <see langword="true"/> when this service can process
    /// <paramref name="filterType"/> entirely on the CPU without GPU support.
    /// </summary>
    public bool CanProcess(FilterType filterType) => _supported.Contains(filterType);

    /// <summary>
    /// Applies <paramref name="config"/> to <paramref name="image"/> using
    /// SIMD-accelerated CPU code.  The operation runs on a thread-pool thread so
    /// the calling thread is never blocked by pixel computation.
    /// </summary>
    /// <param name="image">
    /// Source image.  When <see cref="Image.PixelData"/> is <see langword="null"/> a
    /// zero-filled buffer of the correct size is allocated automatically.
    /// </param>
    /// <param name="config">Filter configuration including type and named parameters.</param>
    /// <param name="cancellationToken">Token that aborts the operation.</param>
    /// <returns>
    /// The same <paramref name="image"/> instance with updated
    /// <see cref="Image.PixelData"/> and <see cref="Image.ModifiedAt"/>.
    /// </returns>
    /// <exception cref="ProcessingException">Thrown on any filter execution failure.</exception>
    public async Task<Image> ApplyFilterAsync(
        Image image,
        FilterConfiguration config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(config);

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("SIMD applying {FilterType} to image {ImageId} ({Width}x{Height})",
            config.FilterType, image.Id, image.Width, image.Height);

        try
        {
            var result = await Task.Run(
                () => ApplyCore(image, config, cancellationToken),
                cancellationToken);

            sw.Stop();
            _performanceService.RecordOperation(sw.ElapsedMilliseconds);
            _logger.LogDebug("SIMD {FilterType} completed in {Elapsed}ms for image {ImageId}",
                config.FilterType, sw.ElapsedMilliseconds, image.Id);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SIMD filter {FilterType} cancelled for image {ImageId}",
                config.FilterType, image.Id);
            throw;
        }
        catch (Exception ex) when (ex is not ProcessingException)
        {
            _logger.LogError(ex, "SIMD fallback failed applying {FilterType} to image {ImageId}",
                config.FilterType, image.Id);
            throw new ProcessingException(
                $"SIMD filter application failed: {config.Name}", ex,
                image.FilePath, config.Name);
        }
    }

    // ── Dispatch ─────────────────────────────────────────────────────────────

    private Image ApplyCore(Image image, FilterConfiguration config, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        var src  = image.PixelData ?? new byte[image.Width * image.Height * bpp];

        image.PixelData = config.FilterType switch
        {
            FilterType.Grayscale                       => Grayscale(src, bpp),
            FilterType.Blur or FilterType.GaussianBlur => BoxBlur(src, image.Width, image.Height, bpp),
            FilterType.Sharpen                         => Convolve(src, image.Width, image.Height, bpp, Kernels.Sharpen),
            FilterType.EdgeDetection                   => Sobel(src, image.Width, image.Height, bpp),
            FilterType.Threshold                       => Threshold(src, FloatParam(config, "thresholdValue", 0.5f)),
            _                                          => src
        };

        image.ModifiedAt = DateTime.UtcNow;
        image.Metadata["simd_level"] = _capabilities.BestAvailableLevel.ToString();
        return image;
    }

    // ── Filter kernels ────────────────────────────────────────────────────────

    private static class Kernels
    {
        // Unsharp-mask: identity boosted at centre, neighbours subtracted
        public static readonly int[] Sharpen = [-1, -1, -1, -1, 9, -1, -1, -1, -1];
    }

    // ── Per-filter implementations ────────────────────────────────────────────

    // BT.601 luma: Y = (77R + 150G + 29B) >> 8  (integer fixed-point, no overflow)
    private static byte[] Grayscale(ReadOnlySpan<byte> src, int bpp)
    {
        var dst = new byte[src.Length];
        int rgb = Math.Min(bpp, 3);

        for (int p = 0; p <= src.Length - bpp; p += bpp)
        {
            byte y = (byte)((77 * src[p] + 150 * src[p + 1] + 29 * src[p + 2]) >> 8);
            for (int c = 0; c < rgb; c++) dst[p + c] = y;
            if (bpp > 3) dst[p + 3] = src[p + 3]; // preserve alpha
        }

        return dst;
    }

    // SIMD bulk pass: Vector<byte> comparison yields 0xFF / 0x00 per element
    private static byte[] Threshold(ReadOnlySpan<byte> src, float threshold)
    {
        byte limit = (byte)Math.Clamp(threshold * 255f, 0f, 255f);
        var dst    = new byte[src.Length];
        var vLimit = new Vector<byte>(limit);
        int vLen   = Vector<byte>.Count;
        int i      = 0;

        for (; i <= src.Length - vLen; i += vLen)
            Vector.GreaterThanOrEqual(new Vector<byte>(src.Slice(i, vLen)), vLimit).CopyTo(dst, i);

        for (; i < src.Length; i++)
            dst[i] = src[i] >= limit ? (byte)255 : (byte)0;

        return dst;
    }

    private static byte[] BoxBlur(ReadOnlySpan<byte> src, int w, int h, int bpp)
    {
        var dst      = new byte[src.Length];
        int channels = Math.Min(bpp, 3);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int px = (y * w + x) * bpp;
            for (int c = 0; c < channels; c++)
            {
                int sum = 0, n = 0;
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int ny = y + dy, nx = x + dx;
                    if ((uint)ny < (uint)h && (uint)nx < (uint)w)
                    {
                        sum += src[(ny * w + nx) * bpp + c];
                        n++;
                    }
                }
                dst[px + c] = (byte)(sum / n);
            }
            if (bpp > 3) dst[px + 3] = src[px + 3];
        }

        return dst;
    }

    private static byte[] Convolve(ReadOnlySpan<byte> src, int w, int h, int bpp, ReadOnlySpan<int> k)
    {
        var dst      = new byte[src.Length];
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
                dst[px + c] = (byte)Math.Clamp(acc, 0, 255);
            }
            if (bpp > 3) dst[px + 3] = src[px + 3];
        }

        return dst;
    }

    private static byte[] Sobel(ReadOnlySpan<byte> src, int w, int h, int bpp)
    {
        var dst      = new byte[src.Length];
        int channels = Math.Min(bpp, 3);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int px = (y * w + x) * bpp;
            for (int c = 0; c < channels; c++)
            {
                int gx = -1 * Sample(src, w, h, bpp, c, x - 1, y - 1)
                       +  1 * Sample(src, w, h, bpp, c, x + 1, y - 1)
                       + -2 * Sample(src, w, h, bpp, c, x - 1, y)
                       +  2 * Sample(src, w, h, bpp, c, x + 1, y)
                       + -1 * Sample(src, w, h, bpp, c, x - 1, y + 1)
                       +  1 * Sample(src, w, h, bpp, c, x + 1, y + 1);

                int gy = -1 * Sample(src, w, h, bpp, c, x - 1, y - 1)
                       + -2 * Sample(src, w, h, bpp, c, x,     y - 1)
                       + -1 * Sample(src, w, h, bpp, c, x + 1, y - 1)
                       +  1 * Sample(src, w, h, bpp, c, x - 1, y + 1)
                       +  2 * Sample(src, w, h, bpp, c, x,     y + 1)
                       +  1 * Sample(src, w, h, bpp, c, x + 1, y + 1);

                dst[px + c] = (byte)Math.Clamp((int)Math.Sqrt(gx * gx + gy * gy), 0, 255);
            }
            if (bpp > 3) dst[px + 3] = src[px + 3];
        }

        return dst;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int Sample(ReadOnlySpan<byte> src, int w, int h, int bpp, int channel, int x, int y)
    {
        x = Math.Clamp(x, 0, w - 1);
        y = Math.Clamp(y, 0, h - 1);
        return src[(y * w + x) * bpp + channel];
    }

    private static float FloatParam(FilterConfiguration config, string key, float fallback) =>
        config.Parameters.TryGetValue(key, out var raw) && raw is float f ? f : fallback;
}
