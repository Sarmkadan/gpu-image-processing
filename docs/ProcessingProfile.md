# ProcessingProfile

Defines a complete configuration profile for the GPU-accelerated image processing pipeline. This type encapsulates all tunable parameters—from hardware acceleration flags and memory budgets to precision trade-offs and tiling strategies—allowing callers to save, restore, and switch between named processing presets. Factory methods provide ready-made profiles optimized for speed, quality, or a balanced compromise.

## API

### Properties

- **`public Guid Id`**  
  Unique identifier for the profile instance. Immutable after construction; used to reference profiles in storage or logging.

- **`public string Name`**  
  Human-readable label for the profile. May be empty but never null. Intended for display in user interfaces and configuration selectors.

- **`public string Description`**  
  Free-text explanation of the profile’s intended use and trade-offs. May be empty but never null.

- **`public bool UseGPUAcceleration`**  
  When `true`, eligible operations are dispatched to available GPU devices. When `false`, all processing falls back to CPU implementations regardless of hardware capability.

- **`public int MaxParallelOperations`**  
  Upper bound on concurrently executing processing tasks. Must be ≥ 1. Values exceeding the system’s logical processor count are clamped internally at execution time but stored as set.

- **`public int BatchSize`**  
  Number of image tiles or scanlines grouped into a single GPU submission. Larger values reduce kernel-launch overhead but increase memory pressure. Must be ≥ 1.

- **`public bool CacheIntermediateResults`**  
  When `true`, intermediate processing stages are retained in a result cache for potential reuse by subsequent operations. When `false`, intermediates are discarded immediately after consumption.

- **`public long MaxMemoryUsageBytes`**  
  Soft ceiling on total working memory (in bytes) the pipeline may allocate before triggering garbage collection or spilling to disk. Must be ≥ 0. A value of 0 disables the limit.

- **`public bool EnablePrecisionReduction`**  
  When `true`, the pipeline may substitute reduced-precision numeric formats (e.g., half-precision floats) where the associated `PrecisionFormat` permits, trading accuracy for throughput and memory savings.

- **`public string PrecisionFormat`**  
  Specifies the target reduced-precision format when `EnablePrecisionReduction` is `true`. Typical values include `"FP16"`, `"BF16"`, or `"INT8"`. Ignored if precision reduction is disabled. Never null.

- **`public bool EnableTiling`**  
  When `true`, large images are subdivided into tiles of size `TileSizePixels` before processing, reducing peak memory and enabling overlap of compute and I/O.

- **`public int TileSizePixels`**  
  Edge length in pixels for square tiles when tiling is enabled. Must be ≥ 1. Ignored if `EnableTiling` is `false`.

- **`public Dictionary<string, float> OptimizationSettings`**  
  Extensible key-value store for algorithm-specific tuning parameters (e.g., `"DenoiseSigma"`, `"SharpenRadius"`). Keys are case-sensitive. Never null; may be empty.

- **`public DateTime CreatedAt`**  
  UTC timestamp of profile creation. Set once during construction.

- **`public DateTime ModifiedAt`**  
  UTC timestamp of the last mutation to any property. Updated automatically on every property change.

- **`public bool IsDefault`**  
  `true` if this profile is designated as the application-wide default. Only one profile in a given scope should have this flag set.

### Constructors

- **`public ProcessingProfile()`**  
  Initializes a new profile with safe defaults: `UseGPUAcceleration = true`, `MaxParallelOperations = Environment.ProcessorCount`, `BatchSize = 64`, `CacheIntermediateResults = false`, `MaxMemoryUsageBytes = 0`, `EnablePrecisionReduction = false`, `PrecisionFormat = "FP16"`, `EnableTiling = false`, `TileSizePixels = 256`, an empty `OptimizationSettings` dictionary, and `IsDefault = false`. The `Id` is assigned a new random GUID, and both `CreatedAt` and `ModifiedAt` are set to the current UTC time.

### Static Factory Methods

- **`public static ProcessingProfile CreateSpeedOptimized()`**  
  Returns a profile configured for maximum throughput: `UseGPUAcceleration = true`, `MaxParallelOperations` set to the logical processor count, large `BatchSize`, `CacheIntermediateResults = false`, `EnablePrecisionReduction = true` with `PrecisionFormat = "FP16"`, `EnableTiling = true` with moderate tile size, and `MaxMemoryUsageBytes` set to a high default.  
  *Return value:* A new `ProcessingProfile` instance with `IsDefault = false`.

- **`public static ProcessingProfile CreateQualityOptimized()`**  
  Returns a profile configured for maximum fidelity: `UseGPUAcceleration = true`, conservative `MaxParallelOperations`, small `BatchSize`, `CacheIntermediateResults = true`, `EnablePrecisionReduction = false`, `EnableTiling = false`, and `MaxMemoryUsageBytes = 0` (unlimited).  
  *Return value:* A new `ProcessingProfile` instance with `IsDefault = false`.

- **`public static ProcessingProfile CreateBalanced()`**  
  Returns a profile that compromises between speed and quality: `UseGPUAcceleration = true`, moderate `MaxParallelOperations` and `BatchSize`, `CacheIntermediateResults = true`, `EnablePrecisionReduction = true` with `PrecisionFormat = "BF16"`, `EnableTiling = true` with a moderate tile size, and a reasonable `MaxMemoryUsageBytes` cap.  
  *Return value:* A new `ProcessingProfile` instance with `IsDefault = false`.

## Usage

### Example 1: Selecting and applying a speed-optimized profile

```csharp
// Obtain a pre-built speed profile and override a specific tuning parameter
ProcessingProfile profile = ProcessingProfile.CreateSpeedOptimized();
profile.OptimizationSettings["DenoiseSigma"] = 1.2f;

// Configure the pipeline with this profile
var pipeline = new ImageProcessingPipeline();
pipeline.ApplyProfile(profile);

// Process a batch of images; tiling and FP16 precision reduction are active
pipeline.ProcessBatch(inputPaths, outputDirectory);
```

### Example 2: Creating a custom profile and persisting it

```csharp
var customProfile = new ProcessingProfile
{
    Name = "Nightly Batch",
    Description = "High-throughput overnight processing with disk spilling allowed",
    UseGPUAcceleration = true,
    MaxParallelOperations = 8,
    BatchSize = 128,
    CacheIntermediateResults = false,
    MaxMemoryUsageBytes = 4L * 1024 * 1024 * 1024, // 4 GiB
    EnablePrecisionReduction = true,
    PrecisionFormat = "INT8",
    EnableTiling = true,
    TileSizePixels = 512
};

// Persist for reuse
ProfileStore.Save(customProfile);

// Later retrieval
ProcessingProfile restored = ProfileStore.Load(customProfile.Id);
```

## Notes

- **Thread safety:** Instance properties are not synchronized. Concurrent reads and writes from multiple threads without external locking may produce inconsistent state and cause `ModifiedAt` to reflect only the last write. Callers must serialize access when sharing a mutable profile across threads.
- **Validation:** Property setters perform basic range validation (e.g., `BatchSize ≥ 1`, `TileSizePixels ≥ 1`, `MaxMemoryUsageBytes ≥ 0`). Violations throw `ArgumentOutOfRangeException` immediately. The `OptimizationSettings` dictionary accepts any key-value pair; semantic validation is deferred to the pipeline at execution time.
- **Precision reduction interaction:** Setting `EnablePrecisionReduction = true` without a recognized `PrecisionFormat` causes the pipeline to fall back to full precision and log a warning. The property itself is not validated against a format whitelist at set time.
- **Tiling and batch size:** When tiling is enabled, `BatchSize` applies per tile submission, not per whole image. A large `BatchSize` combined with a large `TileSizePixels` can quickly exhaust `MaxMemoryUsageBytes`.
- **Default profile semantics:** The `IsDefault` flag is metadata only; the pipeline does not automatically select a profile based on this flag. Application-level logic is responsible for honoring it.
- **Timestamps:** `CreatedAt` is immutable. `ModifiedAt` updates on every property assignment, including assignments that do not change the value. This ensures accurate tracking of any configuration touch.
