#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Analyses a <see cref="GpuDevice"/>'s hardware capabilities to derive the most
/// efficient workgroup layout for 2-D image processing kernels dispatched via OpenCL.
/// </summary>
/// <remarks>
/// <para>
/// The optimiser enumerates power-of-two tile sizes along each axis, evaluates every
/// (X, Y) candidate against three hardware metrics, and returns the layout with the
/// highest composite score for the requested <see cref="WorkgroupOptimizationStrategy"/>.
/// </para>
/// <para>
/// Metrics modelled:
/// <list type="bullet">
///   <item><description><b>Occupancy</b> — fraction of wavefronts simultaneously active on the device.</description></item>
///   <item><description><b>Memory fraction</b> — proportion of local memory left unused per workgroup.</description></item>
///   <item><description><b>Wavefront alignment</b> — whether the workgroup size is a multiple of the warp/wavefront width.</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class WorkgroupOptimizer(ILogger<WorkgroupOptimizer> logger) : IWorkgroupOptimizer
{
    private readonly ILogger<WorkgroupOptimizer> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    // Power-of-two tile dimensions probed during layout search.
    private static readonly int[] TileSizes = [4, 8, 16, 32];

    /// <inheritdoc />
    public WorkgroupConfiguration Compute(
        GpuDevice device,
        int imageWidth,
        int imageHeight,
        int localMemoryPerThreadBytes = 0,
        WorkgroupOptimizationStrategy strategy = WorkgroupOptimizationStrategy.Balanced)
    {
        ArgumentNullException.ThrowIfNull(device);
        if (imageWidth  <= 0) throw new ArgumentOutOfRangeException(nameof(imageWidth),  "Image width must be positive.");
        if (imageHeight <= 0) throw new ArgumentOutOfRangeException(nameof(imageHeight), "Image height must be positive.");

        int  wavefront   = DetectWavefrontSize(device);
        int  maxThreads  = device.MaxWorkGroupSize;
        long localMemCap = device.LocalMemoryBytes;

        // For very large images (> 8192 on either axis) convolution kernels
        // allocate a per-workgroup halo tile in local memory.  If the caller
        // did not supply an explicit estimate we apply a conservative default
        // (4 float channels × 4 bytes each) so that EnumerateCandidates can
        // filter out workgroup sizes that would exceed device limits.
        bool isLargeImage = imageWidth > 8192 || imageHeight > 8192;
        int effectiveLocalMemPerThread = localMemoryPerThreadBytes > 0
            ? localMemoryPerThreadBytes
            : isLargeImage ? 16 : 0;

        var candidates = EnumerateCandidates(maxThreads, localMemCap, effectiveLocalMemPerThread, strategy, isLargeImage);
        var best       = SelectBest(candidates, device, imageWidth, imageHeight,
                                    effectiveLocalMemPerThread, localMemCap, wavefront, strategy);

        _logger.LogDebug("Optimised workgroup for '{Device}': {Config}", device.Name, best);
        return best;
    }

    /// <inheritdoc />
    public async Task<WorkgroupConfiguration> BenchmarkAsync(
        GpuDevice device,
        int imageWidth,
        int imageHeight,
        int localMemoryPerThreadBytes = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);

        var results = new List<(WorkgroupConfiguration Config, double EstimatedMs)>();

        foreach (int tx in TileSizes)
        {
            foreach (int ty in TileSizes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (tx * ty > device.MaxWorkGroupSize) continue;
                if ((long)tx * ty * localMemoryPerThreadBytes > device.LocalMemoryBytes) continue;

                double ms  = EstimateDispatchMs(device, imageWidth, imageHeight, tx, ty);
                var    cfg = BuildConfiguration(device, imageWidth, imageHeight,
                                                tx, ty, localMemoryPerThreadBytes,
                                                WorkgroupOptimizationStrategy.ThroughputMaximized);
                results.Add((cfg, ms));
            }

            // Yield after each outer tile to avoid monopolising the thread pool.
            await Task.Yield();
        }

        if (results.Count == 0)
            return Compute(device, imageWidth, imageHeight, localMemoryPerThreadBytes);

        var winner = results.MinBy(r => r.EstimatedMs).Config;

        _logger.LogInformation(
            "Benchmark selected [{X}×{Y}] for '{Device}' on a {W}×{H} image",
            winner.WorkgroupSizeX, winner.WorkgroupSizeY,
            device.Name, imageWidth, imageHeight);

        return winner;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static List<(int X, int Y)> EnumerateCandidates(
        int   maxThreads,
        long  localMemCap,
        int   localMemPerThread,
        WorkgroupOptimizationStrategy strategy,
        bool  isLargeImage = false)
    {
        int[] pool = strategy switch
        {
            WorkgroupOptimizationStrategy.LatencyMinimized => [4, 8],
            WorkgroupOptimizationStrategy.MemoryOptimized  => [4, 8, 16],
            _                                              => TileSizes
        };

        // Images larger than 8192 pixels on any axis require smaller tiles to keep
        // the per-workgroup local memory halo within device limits and prevent
        // CL_OUT_OF_RESOURCES errors on both NVIDIA and AMD hardware.
        if (isLargeImage)
            pool = pool.Where(s => s <= 8).DefaultIfEmpty(4).ToArray();

        var result = new List<(int, int)>();
        foreach (int x in pool)
        {
            foreach (int y in pool)
            {
                if (x * y > maxThreads)                              continue;
                if ((long)x * y * localMemPerThread > localMemCap)   continue;
                result.Add((x, y));
            }
        }

        // Guarantee at least one viable candidate.
        return result.Count > 0 ? result : [(1, 1)];
    }

    private static WorkgroupConfiguration SelectBest(
        List<(int X, int Y)>           candidates,
        GpuDevice                      device,
        int                            imageWidth,
        int                            imageHeight,
        int                            localMemPerThread,
        long                           localMemCap,
        int                            wavefront,
        WorkgroupOptimizationStrategy  strategy)
    {
        WorkgroupConfiguration? best      = null;
        double                  bestScore = double.MinValue;

        foreach (var (x, y) in candidates)
        {
            var    cfg   = BuildConfiguration(device, imageWidth, imageHeight, x, y, localMemPerThread, strategy);
            double score = ComputeScore(cfg, device, localMemCap, wavefront, strategy);

            if (score > bestScore)
            {
                bestScore = score;
                best = cfg with { OptimizationScore = Math.Round(score * 100.0, 2) };
            }
        }

        return best!;
    }

    private static WorkgroupConfiguration BuildConfiguration(
        GpuDevice                     device,
        int                           imageWidth,
        int                           imageHeight,
        int                           sizeX,
        int                           sizeY,
        int                           localMemPerThread,
        WorkgroupOptimizationStrategy strategy)
    {
        int    globalX   = AlignUp(imageWidth,  sizeX);
        int    globalY   = AlignUp(imageHeight, sizeY);
        long   localMem  = (long)sizeX * sizeY * localMemPerThread;
        int    wavefront = DetectWavefrontSize(device);
        double occupancy = Math.Clamp(
            (double)(sizeX * sizeY) / (wavefront * Math.Max(1, device.MaxComputeUnits / 4.0)),
            0.0, 1.0);

        return new WorkgroupConfiguration
        {
            WorkgroupSizeX           = sizeX,
            WorkgroupSizeY           = sizeY,
            WorkgroupSizeZ           = 1,
            GlobalWorkSizeX          = globalX,
            GlobalWorkSizeY          = globalY,
            GlobalWorkSizeZ          = 1,
            LocalMemoryRequiredBytes = localMem,
            EstimatedOccupancy       = occupancy,
            Strategy                 = strategy,
            DeviceId                 = device.Id,
            OptimizationScore        = 0   // scored by SelectBest after construction
        };
    }

    private static double ComputeScore(
        WorkgroupConfiguration        cfg,
        GpuDevice                     device,
        long                          localMemCap,
        int                           wavefront,
        WorkgroupOptimizationStrategy strategy)
    {
        double memFraction = localMemCap > 0
            ? 1.0 - (double)cfg.LocalMemoryRequiredBytes / localMemCap
            : 1.0;

        double alignment = cfg.GetTotalWorkgroupSize() % wavefront == 0 ? 1.0 : 0.6;

        // Favour larger workgroups when the device has many compute units.
        double scaledOccupancy = cfg.EstimatedOccupancy *
            Math.Min(1.0, device.MaxComputeUnits / 64.0 + 0.5);

        return strategy switch
        {
            WorkgroupOptimizationStrategy.ThroughputMaximized =>
                scaledOccupancy * 0.55 + alignment * 0.25 + memFraction * 0.20,

            WorkgroupOptimizationStrategy.LatencyMinimized =>
                alignment * 0.50 + memFraction * 0.30 + scaledOccupancy * 0.20,

            WorkgroupOptimizationStrategy.MemoryOptimized =>
                memFraction * 0.60 + scaledOccupancy * 0.25 + alignment * 0.15,

            _ /* Balanced */ =>
                scaledOccupancy * 0.40 + memFraction * 0.35 + alignment * 0.25
        };
    }

    private static double EstimateDispatchMs(
        GpuDevice device, int width, int height, int tileX, int tileY)
    {
        long   dispatches = (long)(AlignUp(width,  tileX) / tileX) *
                                   (AlignUp(height, tileY) / tileY);
        double overhead   = dispatches * 0.002;   // ~2 µs per dispatch call
        double compute    = (double)width * height /
                            Math.Max(1, device.MaxComputeUnits) /
                            Math.Max(1.0, device.MaxClockFrequencyMhz * 1_000.0);
        return overhead + compute * 1_000.0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AlignUp(int value, int alignment) =>
        ((value + alignment - 1) / alignment) * alignment;

    /// <summary>
    /// Returns the device wavefront (warp) size. Prefers the value reported by the
    /// device itself; falls back to vendor heuristics only when unavailable.
    /// RDNA 2+ uses 32-wide wavefronts, older AMD GCN uses 64.
    /// </summary>
    private static int DetectWavefrontSize(GpuDevice device)
    {
        if (device.WavefrontSize > 0)
            return device.WavefrontSize;

        if (device.Vendor?.Contains("Intel", StringComparison.OrdinalIgnoreCase) == true)
            return 16;

        if (device.Name?.Contains("AMD", StringComparison.OrdinalIgnoreCase) != true)
            return 32; // NVIDIA default

        // RDNA 2/3 devices operate on wave32 by default
        bool isRdna = device.Name.Contains("RX 6", StringComparison.OrdinalIgnoreCase) ||
                      device.Name.Contains("RX 7", StringComparison.OrdinalIgnoreCase) ||
                      device.Name.Contains("RDNA", StringComparison.OrdinalIgnoreCase);
        return isRdna ? 32 : 64;
    }
}
