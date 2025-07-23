# ApplicationSettings

Central configuration container for GPU-accelerated image-processing pipeline settings. Aggregates environment descriptors, OpenCL device preferences, processing parameters, storage options, performance knobs, and logging directives into a single, serializable root object. Used at startup to bootstrap the pipeline and to enforce consistent behavior across components.

## API

### `public string ApplicationName`
Identifier of the application (e.g., “GpuImageProcessing”). Used in log headers and UI captions. Must be non-null and non-empty; an `ArgumentException` is thrown on construction if the value is null or whitespace.

### `public string Version`
Semantic version string of the current build (e.g., “1.2.3”). Consumed by telemetry and update checks. Must be non-null and non-empty; an `ArgumentException` is thrown on construction if the value is null or whitespace.

### `public string Environment`
Deployment context label (“Development”, “Staging”, “Production”). Influences log verbosity and feature toggles. Must be non-null and non-empty; an `ArgumentException` is thrown on construction if the value is null or whitespace.

### `public OpenCLSettings OpenCL`
Sub-configuration for OpenCL platform/device selection and kernel compilation. Exposes `PreferredPlatformName`, `PreferredDeviceName`, `CompileKernels`, `KernelCachePath`, and related tuning values. Never null; an `ArgumentNullException` is thrown on construction if the instance is null.

### `public ProcessingSettings Processing`
Sub-configuration for image-processing algorithms (e.g., interpolation, color-space conversion). Exposes `DefaultBatchSize`, `MaxParallelOperations`, `MaxImageDimension`, `MaxImageFileSizeBytes`, and `TileSizePixels`. Never null; an `ArgumentNullException` is thrown on construction if the instance is null.

### `public StorageSettings Storage`
Sub-configuration for temporary file storage and caching. Exposes paths and cleanup policies. Never null; an `ArgumentNullException` is thrown on construction if the instance is null.

### `public PerformanceSettings Performance`
Sub-configuration for performance-sensitive parameters (e.g., GPU memory limits, CPU fallback thresholds). Never null; an `ArgumentNullException` is thrown on construction if the instance is null.

### `public LoggingSettings Logging`
Sub-configuration for logging providers, levels, and output paths. Never null; an `ArgumentNullException` is thrown on construction if the instance is null.

### `public bool Enabled`
Global feature flag that gates all GPU-accelerated processing. When `false`, the pipeline falls back to CPU implementations. Defaults to `true`.

### `public string? PreferredPlatformName`
Name of the preferred OpenCL platform (e.g., “NVIDIA CUDA”). If `null`, the runtime selects the first available platform. Must be non-empty if non-null; an `ArgumentException` is thrown on construction if the string is empty.

### `public string? PreferredDeviceName`
Name of the preferred OpenCL device (e.g., “GeForce RTX 3090”). If `null`, the runtime selects the first device on the preferred platform. Must be non-empty if non-null; an `ArgumentException` is thrown on construction if the string is empty.

### `public int MaxKernels`
Upper bound on the number of simultaneously compiled or cached kernels. Must be ≥ 0; an `ArgumentOutOfRangeException` is thrown on construction if the value is negative.

### `public bool CompileKernels`
Flag indicating whether kernels should be compiled at startup (`true`) or loaded from the kernel cache (`false`). Defaults to `true`.

### `public string KernelCachePath`
Directory path where compiled kernel binaries are stored and loaded from. Must be a valid absolute or relative directory path; an `ArgumentException` is thrown on construction if the path is invalid or inaccessible.

### `public int DefaultBatchSize`
Default number of images processed in a single GPU dispatch. Must be ≥ 1; an `ArgumentOutOfRangeException` is thrown on construction if the value is less than 1.

### `public int MaxParallelOperations`
Maximum number of concurrent image-processing operations allowed across all threads. Must be ≥ 1; an `ArgumentOutOfRangeException` is thrown on construction if the value is less than 1.

### `public bool UseGPUAcceleration`
Global switch to enable or disable GPU acceleration. When `false`, CPU fallbacks are used for all image operations. Defaults to `true`.

### `public int MaxImageDimension`
Maximum allowed pixel dimension (width or height) for any input image. Must be ≥ 1; an `ArgumentOutOfRangeException` is thrown on construction if the value is less than 1.

### `public long MaxImageFileSizeBytes`
Maximum allowed file size for any input image. Must be ≥ 0; an `ArgumentOutOfRangeException` is thrown on construction if the value is negative.

### `public int TileSizePixels`
Preferred tile size (in pixels) used when tiling large images for GPU processing. Must be ≥ 1; an `ArgumentOutOfRangeException` is thrown on construction if the value is less than 1.

## Usage
