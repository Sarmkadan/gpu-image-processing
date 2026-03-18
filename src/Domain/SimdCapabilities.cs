#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.Intrinsics.X86;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Available SIMD instruction-set levels, ordered by increasing capability.
/// </summary>
public enum SimdLevel
{
    /// <summary>No SIMD extensions detected; scalar processing only.</summary>
    None = 0,

    /// <summary>128-bit SSE2 integer and floating-point vectors.</summary>
    Sse2 = 1,

    /// <summary>128-bit SSE 4.1 vectors with enhanced integer operations.</summary>
    Sse41 = 2,

    /// <summary>256-bit AVX single- and double-precision vectors.</summary>
    Avx = 3,

    /// <summary>256-bit AVX2 with full 256-bit integer vector support.</summary>
    Avx2 = 4,

    /// <summary>512-bit AVX-512F foundation vectors.</summary>
    Avx512F = 5
}

/// <summary>
/// Immutable snapshot of the SIMD capabilities available on the current CPU.
/// Instances are produced by <see cref="Detect"/> and are safe to share across threads.
/// </summary>
public sealed class SimdCapabilities
{
    /// <summary>True when the CPU advertises SSE2 support.</summary>
    public bool SupportsSSE2 { get; init; }

    /// <summary>True when the CPU advertises SSE 4.1 support.</summary>
    public bool SupportsSse41 { get; init; }

    /// <summary>True when the CPU advertises 256-bit AVX floating-point support.</summary>
    public bool SupportsAvx { get; init; }

    /// <summary>True when the CPU advertises AVX2 256-bit integer vector support.</summary>
    public bool SupportsAvx2 { get; init; }

    /// <summary>True when the CPU advertises AVX-512 Foundation support.</summary>
    public bool SupportsAvx512F { get; init; }

    /// <summary>Highest SIMD level detected on this processor.</summary>
    public SimdLevel BestAvailableLevel { get; init; }

    /// <summary>
    /// Native hardware vector-register width in bytes (e.g. 16 for SSE, 32 for AVX, 64 for AVX-512).
    /// </summary>
    public int VectorWidthBytes { get; init; }

    /// <summary>
    /// <see langword="true"/> when at least one SIMD extension is available,
    /// allowing faster-than-scalar pixel processing.
    /// </summary>
    public bool IsAnySimdAvailable => BestAvailableLevel > SimdLevel.None;

    /// <summary>
    /// Probes the runtime for CPU SIMD support and returns an immutable
    /// <see cref="SimdCapabilities"/> instance describing what is available.
    /// Safe to call from any thread; the result should be cached per-process.
    /// </summary>
    public static SimdCapabilities Detect()
    {
        var sse2   = Sse2.IsSupported;
        var sse41  = Sse41.IsSupported;
        var avx    = Avx.IsSupported;
        var avx2   = Avx2.IsSupported;
        var avx512 = Avx512F.IsSupported;

        var level = avx512 ? SimdLevel.Avx512F
                  : avx2   ? SimdLevel.Avx2
                  : avx    ? SimdLevel.Avx
                  : sse41  ? SimdLevel.Sse41
                  : sse2   ? SimdLevel.Sse2
                  :          SimdLevel.None;

        int width = avx512        ? 64
                  : avx2 || avx   ? 32
                  : sse2 || sse41 ? 16
                  : System.Numerics.Vector<float>.Count * sizeof(float);

        return new SimdCapabilities
        {
            SupportsSSE2       = sse2,
            SupportsSse41      = sse41,
            SupportsAvx        = avx,
            SupportsAvx2       = avx2,
            SupportsAvx512F    = avx512,
            BestAvailableLevel = level,
            VectorWidthBytes   = width
        };
    }

    /// <summary>Returns a human-readable summary of all detected SIMD capabilities.</summary>
    public override string ToString() =>
        $"SimdLevel={BestAvailableLevel}, VectorWidth={VectorWidthBytes}B " +
        $"[SSE2={SupportsSSE2} SSE4.1={SupportsSse41} AVX={SupportsAvx} " +
        $"AVX2={SupportsAvx2} AVX-512F={SupportsAvx512F}]";
}
