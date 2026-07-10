# SimdCapabilities

`SimdCapabilities` provides a runtime detection mechanism for SIMD (Single Instruction, Multiple Data) instruction set support on the current CPU. It exposes boolean flags for specific instruction sets, a convenience enumeration representing the best available level, and the corresponding native vector width in bytes. The type is designed to allow GPU-accelerated image processing pipelines to dynamically select optimal code paths without requiring compile-time configuration.

## API

### `public static SimdCapabilities Detect`

Static factory method that probes the host CPU and returns a fully populated `SimdCapabilities` instance reflecting the actual hardware capabilities.

- **Parameters:** None.
- **Returns:** A new `SimdCapabilities` instance with all support flags, `BestAvailableLevel`, and `VectorWidthBytes` set according to the detected CPU features.
- **Exceptions:** Throws `PlatformNotSupportedException` if the detection intrinsics are unavailable on the current runtime platform (e.g., when running on an architecture without CPUID-like probing).

### `public bool SupportsSSE2`

Indicates whether the Streaming SIMD Extensions 2 instruction set is available.

- **Type:** Read-only instance property.
- **Value:** `true` if SSE2 is supported; otherwise `false`.

### `public bool SupportsSse41`

Indicates whether the Streaming SIMD Extensions 4.1 instruction set is available.

- **Type:** Read-only instance property.
- **Value:** `true` if SSE4.1 is supported; otherwise `false`.

### `public bool SupportsAvx`

Indicates whether the Advanced Vector Extensions instruction set is available.

- **Type:** Read-only instance property.
- **Value:** `true` if AVX is supported; otherwise `false`.

### `public bool SupportsAvx2`

Indicates whether the Advanced Vector Extensions 2 instruction set is available.

- **Type:** Read-only instance property.
- **Value:** `true` if AVX2 is supported; otherwise `false`.

### `public bool SupportsAvx512F`

Indicates whether the AVX-512 Foundation instruction set is available.

- **Type:** Read-only instance property.
- **Value:** `true` if AVX-512F is supported; otherwise `false`.

### `public SimdLevel BestAvailableLevel`

Returns the highest SIMD capability tier detected, represented as a `SimdLevel` enumeration value.

- **Type:** Read-only instance property.
- **Value:** The `SimdLevel` member corresponding to the most advanced supported instruction set (e.g., `Avx2`, `Avx512F`). Falls back to a baseline scalar level when no SIMD support is detected.

### `public int VectorWidthBytes`

The native SIMD vector register width in bytes for the best available instruction set.

- **Type:** Read-only instance property.
- **Value:** For example, `16` for SSE/SSE4.1, `32` for AVX/AVX2, and `64` for AVX-512F. Returns a minimal width (typically `16` or a scalar-equivalent value) when no SIMD support is present.

### `public override string ToString()`

Returns a human-readable summary of the detected SIMD capabilities.

- **Parameters:** None.
- **Returns:** A `string` that typically includes the `BestAvailableLevel` label and the vector width, suitable for diagnostics and logging.
- **Exceptions:** None.

## Usage

### Example 1: Selecting a Processing Kernel at Startup

```csharp
using gpu_image_processing;

public class KernelSelector
{
    private readonly SimdCapabilities _capabilities;

    public KernelSelector()
    {
        _capabilities = SimdCapabilities.Detect;
        Console.WriteLine($"Detected SIMD level: {_capabilities}");
    }

    public void ProcessImage(ImageBuffer buffer)
    {
        if (_capabilities.SupportsAvx512F)
        {
            ApplyAvx512Filter(buffer);
        }
        else if (_capabilities.SupportsAvx2)
        {
            ApplyAvx2Filter(buffer);
        }
        else if (_capabilities.SupportsSse41)
        {
            ApplySse41Filter(buffer);
        }
        else
        {
            ApplyScalarFilter(buffer);
        }
    }

    private void ApplyAvx512Filter(ImageBuffer buffer) { /* AVX-512 implementation */ }
    private void ApplyAvx2Filter(ImageBuffer buffer)   { /* AVX2 implementation */ }
    private void ApplySse41Filter(ImageBuffer buffer)  { /* SSE4.1 implementation */ }
    private void ApplyScalarFilter(ImageBuffer buffer) { /* Scalar fallback */ }
}
```

### Example 2: Batch Processing with Vector-Width-Aware Buffering

```csharp
using gpu_image_processing;

public class BatchProcessor
{
    public void RunBatch(IReadOnlyList<ImageBuffer> images)
    {
        SimdCapabilities caps = SimdCapabilities.Detect;

        // Allocate a processing buffer sized to a multiple of the vector width.
        int vectorWidth = caps.VectorWidthBytes;
        int pixelCount = (images.Count * images[0].PixelStride) / sizeof(float);
        int paddedCount = (pixelCount + vectorWidth - 1) / vectorWidth * vectorWidth;
        float[] workBuffer = new float[paddedCount];

        // Dispatch using the best available level directly.
        switch (caps.BestAvailableLevel)
        {
            case SimdLevel.Avx512F:
                ProcessVectorized(workBuffer, vectorWidth);
                break;
            case SimdLevel.Avx2:
            case SimdLevel.Avx:
                ProcessVectorized(workBuffer, vectorWidth);
                break;
            default:
                ProcessScalar(workBuffer);
                break;
        }
    }

    private void ProcessVectorized(float[] buffer, int width) { /* SIMD loop */ }
    private void ProcessScalar(float[] buffer) { /* scalar loop */ }
}
```

## Notes

- **Detection cost:** `SimdCapabilities.Detect` executes CPUID instructions and caches nothing internally. Callers should invoke it once and store the result for the application lifetime rather than calling it repeatedly in hot paths.
- **Thread safety:** The `Detect` method is static and does not mutate shared state; it is safe to call from multiple threads concurrently. The returned instance is immutable and can be safely shared across threads without synchronization.
- **Instruction set hierarchy:** The boolean properties are independent and may be simultaneously `true` for multiple levels (e.g., a CPU supporting AVX2 will also report `SupportsSSE2` and `SupportsAvx` as `true`). `BestAvailableLevel` reflects the highest tier, but callers may still query lower tiers directly when a specific kernel requires only that level.
- **Vector width fallback:** When no SIMD support is detected, `VectorWidthBytes` returns a minimal value (typically `16` on x64, corresponding to the scalar floating-point width or SSE baseline). Code that divides buffers by this value should handle the degenerate case where the width equals the scalar element size.
- **Platform limitations:** On non-x86/x64 platforms or when running under certain virtualization layers that hide CPUID features, `Detect` may throw `PlatformNotSupportedException`. Portable libraries should wrap the call in a try-catch and fall back to a scalar-only `SimdCapabilities` instance with all flags set to `false`.
- **ToString format:** The output of `ToString()` is intended for diagnostics and logging. Its exact format is not contractual and may change across versions; do not parse it programmatically.
