# DeviceUtilities

Utility class for inspecting and evaluating GPU device capabilities, memory usage, and performance characteristics in GPU-accelerated image processing pipelines. Provides methods to assess device suitability, estimate throughput, validate resource availability, and recommend optimal batch sizes for processing workloads.

## API

### `public static float ScoreGpuDevice()`

Evaluates the overall suitability of the current GPU device for image processing workloads by combining compute capability, memory bandwidth, and memory availability into a normalized score between 0 and 1. Higher scores indicate better suitability.

**Returns**
`float` — A normalized score (0.0 to 1.0) representing device suitability. Returns 0.0 if the device is not supported or if evaluation fails.

**Throws**
`InvalidOperationException` — If GPU device information cannot be retrieved or if the device is not recognized.

---

### `public static float CalculatePeakPerformance(int width, int height, int channels, int batchSize)`

Estimates the maximum achievable throughput (in pixels per second) for a given image configuration and batch size, assuming optimal kernel execution and memory transfer efficiency.

**Parameters**
- `width` (`int`) — Image width in pixels.
- `height` (`int`) — Image height in pixels.
- `channels` (`int`) — Number of color channels per pixel.
- `batchSize` (`int`) — Number of images processed in a single batch.

**Returns**
`float` — Estimated peak performance in pixels per second. Returns 0.0 if estimation cannot be performed.

**Throws**
`ArgumentOutOfRangeException` — If any parameter is less than or equal to zero.

---

### `public static float EstimateBandwidthUtilization(int dataSizeBytes)`

Estimates the percentage of available memory bandwidth currently being utilized based on a data transfer of the specified size, assuming a full memory copy operation.

**Parameters**
- `dataSizeBytes` (`int`) — Total size of data to be transferred in bytes.

**Returns**
`float` — Estimated bandwidth utilization as a percentage (0.0 to 100.0). Returns 0.0 if estimation fails or if data size is zero.

**Throws**
`ArgumentOutOfRangeException` — If `dataSizeBytes` is negative.

---

### `public static bool SupportsComputeCapability(int major, int minor)`

Determines whether the current GPU device supports the specified CUDA compute capability.

**Parameters**
- `major` (`int`) — Major compute capability version.
- `minor` (`int`) — Minor compute capability version.

**Returns**
`bool` — `true` if the device supports the specified compute capability; otherwise, `false`.

**Throws**
`ArgumentOutOfRangeException` — If `major` or `minor` is negative.

---

### `public static bool ValidateMemorySufficiency(long requiredBytes)`

Checks whether the GPU has sufficient free memory to allocate a buffer of the specified size.

**Parameters**
- `requiredBytes` (`long`) — Minimum required memory in bytes.

**Returns**
`bool` — `true` if the GPU has enough free memory; otherwise, `false`.

**Throws**
`ArgumentOutOfRangeException` — If `requiredBytes` is negative.

---
### `public static MemoryPressureAnalysis AnalyzeMemoryPressure()`

Performs a comprehensive analysis of current GPU memory pressure, including total, used, and free memory, usage percentage, and pressure level classification.

**Returns**
`MemoryPressureAnalysis` — A struct containing detailed memory pressure metrics and classification.

---
### `public static int CalculateRecommendedBatchSize(int imageSizeBytes, float targetUtilization = 0.75f)`

Calculates the optimal batch size for image processing based on image size and target memory utilization, aiming to maximize throughput without exceeding available resources.

**Parameters**
- `imageSizeBytes` (`int`) — Size of a single image in bytes.
- `targetUtilization` (`float`) — Desired memory usage percentage (0.0 to 1.0). Defaults to 0.75.

**Returns**
`int` — Recommended batch size. Returns 0 if calculation fails or if inputs are invalid.

**Throws**
`ArgumentOutOfRangeException` — If `imageSizeBytes` is less than or equal to zero, or if `targetUtilization` is outside [0.0, 1.0].

---
### `public static List<string> IdentifyBottlenecks()`

Analyzes current GPU utilization and identifies potential performance bottlenecks such as memory pressure, compute saturation, or bandwidth limitations.

**Returns**
`List<string>` — A list of bottleneck descriptions. Returns an empty list if no bottlenecks are detected or if analysis fails.

---
### `public static string GenerateCapabilitySummary()`

Generates a human-readable summary of the GPU device's compute capability, memory capacity, and feature support.

**Returns**
`string` — A formatted summary of GPU capabilities. Returns an empty string if device information is unavailable.

---
### `public long TotalMemoryBytes`

Gets the total physical memory available on the GPU in bytes.

---
### `public long UsedMemoryBytes`

Gets the currently used GPU memory in bytes.

---
### `public long FreeMemoryBytes`

Gets the currently free GPU memory in bytes.

---
### `public float UsagePercent`

Gets the current GPU memory usage as a percentage (0.0 to 100.0).

---
### `public MemoryPressureLevel PressureLevel`

Gets the current memory pressure classification based on usage and available resources.

---
### `public bool IsMemoryConstrained`

Indicates whether the GPU is currently under memory pressure, defined as usage exceeding 85% of total memory.

---
### `public int RecommendedBatchSize`

Gets the last computed recommended batch size for optimal processing throughput.

## Usage
