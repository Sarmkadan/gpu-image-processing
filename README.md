## WorkgroupOptimizationCache

The `WorkgroupOptimizationCache` class caches workgroup optimization results per (device, shader, image-size bucket) to avoid re-running expensive autotuning computations on repeated dispatches. The cache key is composed of device identity (ID + name), a shader hash computed from the kernel source, and an image size bucket (power-of-two ranges). Entries are persisted to disk in JSON format under `AppConstants.FileSystem.DefaultCacheDirectory` to survive application restarts and provide cold-start performance benefits.

### Key Features

- Memory-first lookup with optional disk persistence
- Cache key derived from device identity, shader hash, and image size bucket
- Disk entries persisted as JSON (`*.wgcache`) and loaded on startup
- Automatic cleanup of stale cache files older than 90 days
- Statistics reporting for cache state and disk usage

### Usage Examples

```csharp
using GpuImageProcessing.Pipeline;
using Microsoft.Extensions.Logging;

// Create the cache (disk persistence enabled by default)
var logger = LoggerFactory.Create(b => b.AddConsole())
    .CreateLogger<WorkgroupOptimizationCache>();
var cache = new WorkgroupOptimizationCache(logger, enableDiskCache: true);

// Get or compute a workgroup configuration synchronously
var config = cache.GetOrAdd(
    device,
    kernelSource,
    imageWidth,
    imageHeight,
    localMemoryPerThreadBytes,
    WorkgroupOptimizationStrategy.Default,
    () => ComputeWorkgroupConfiguration());

// Get or compute asynchronously
var asyncConfig = await cache.GetOrAddAsync(
    device,
    kernelSource,
    imageWidth,
    imageHeight,
    localMemoryPerThreadBytes,
    WorkgroupOptimizationStrategy.Default,
    () => ComputeWorkgroupConfigurationAsync(),
    cancellationToken);

// Inspect cache statistics
var stats = cache.GetStatistics();
Console.WriteLine($"Memory entries: {stats.MemoryCacheCount}");
Console.WriteLine($"Disk files: {stats.DiskCacheFileCount}");
Console.WriteLine($"Cache size: {stats.CacheSizeBytes} bytes");

// Clear the cache (optionally including disk entries)
cache.Clear(clearDiskCache: true);

// Dispose when done
cache.Dispose();
```
## FilterPresets

The `FilterPresets` static class provides preconfigured filter chain presets for common image processing workflows. Each preset is built using the `FilterChainBuilder` fluent API and returns a fully configured, validated `FilterChain` instance that can be used directly or as a starting point for customization.

### Key Features

- **Sharpen**: Enhances image details and edges with moderate sharpening
- **Vintage**: Applies a retro/vintage film effect with grayscale, blur, and sharpening
- **Dramatic**: Creates high-contrast dramatic visuals with grayscale, sharpening, and threshold
- Each preset supports further customization via the builder pattern (e.g., modifying descriptions, enabling parallel execution, caching intermediates)
- All presets are validated and ready for immediate use in image processing pipelines

### Usage Examples

```csharp
// Use a preset directly
var sharpenChain = FilterPresets.Sharpen.Build();

// Or customize a preset
var customChain = FilterPresets.Vintage
    .WithDescription("Custom vintage with extra contrast")
    .AllowParallelExecution(2)
    .Build();

// Access preset properties
Console.WriteLine(FilterPresets.Dramatic.Description);
// Output: Creates high-contrast dramatic visual effects
```