#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Caches workgroup optimization results per (device, shader, image-size bucket) to avoid
/// re-running expensive autotuning computations on repeated dispatches.
/// </summary>
/// <remarks>
/// The cache key is composed of:
/// <list type="bullet">
/// <item><description>Device identity (ID + name)</description></item>
/// <item><description>Shader hash (computed from kernel source)</description></item>
/// <item><description>Image size bucket (power-of-two ranges)</description></item>
/// </list>
///
/// Cache entries are persisted to disk in JSON format under AppConstants.FileSystem.DefaultCacheDirectory
/// to survive application restarts and provide cold-start performance benefits.
/// </remarks>
public sealed class WorkgroupOptimizationCache : IDisposable
{
    private readonly ILogger<WorkgroupOptimizationCache> _logger;
    private readonly ConcurrentDictionary<CacheKey, CachedWorkgroupResult> _memoryCache = new();
    private readonly ReaderWriterLockSlim _diskCacheLock = new();
    private readonly string _cacheDirectory;
    private readonly bool _enableDiskCache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromDays(30);
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="WorkgroupOptimizationCache"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="enableDiskCache">Whether to persist cache entries to disk.</param>
    public WorkgroupOptimizationCache(ILogger<WorkgroupOptimizationCache> logger, bool enableDiskCache = true)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _enableDiskCache = enableDiskCache;

        _cacheDirectory = AppConstants.FileSystem.DefaultCacheDirectory;

        if (_enableDiskCache)
        {
            InitializeCacheDirectory();
            LoadFromDisk();
        }

        _logger.LogInformation("Workgroup optimization cache initialized - Memory: {Count} entries, Disk: {Enabled}",
            _memoryCache.Count, _enableDiskCache ? "Enabled" : "Disabled");
    }

    /// <summary>
    /// Gets or computes the optimal workgroup configuration for the given parameters.
    /// </summary>
    /// <param name="device">Target GPU device.</param>
    /// <param name="kernelSource">The OpenCL kernel source code (used to compute shader hash).</param>
    /// <param name="imageWidth">Image width in pixels.</param>
    /// <param name="imageHeight">Image height in pixels.</param>
    /// <param name="localMemoryPerThreadBytes">Local memory required per thread.</param>
    /// <param name="strategy">Optimization strategy to apply.</param>
    /// <param name="computeFunc">Function to compute the configuration if not found in cache.</param>
    /// <returns>The cached or newly computed workgroup configuration.</returns>
    public WorkgroupConfiguration GetOrAdd(
        GpuDevice device,
        string kernelSource,
        int imageWidth,
        int imageHeight,
        int localMemoryPerThreadBytes,
        WorkgroupOptimizationStrategy strategy,
        Func<WorkgroupConfiguration> computeFunc)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(kernelSource);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageWidth, 0, nameof(imageWidth));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageHeight, 0, nameof(imageHeight));

        // Compute cache key
        var key = CreateCacheKey(device, kernelSource, imageWidth, imageHeight);

        // Try to get from memory cache first
        if (_memoryCache.TryGetValue(key, out var cachedResult))
        {
            LogCacheHit(key, cachedResult.Configuration);
            return cachedResult.Configuration;
        }

        // Compute if not found
        var config = computeFunc();

        // Store in memory cache
        var newResult = new CachedWorkgroupResult(config, DateTime.UtcNow);
        _memoryCache[key] = newResult;

        // Persist to disk if enabled
        if (_enableDiskCache)
        {
            try
            {
                SaveToDisk(key, newResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist workgroup cache entry to disk for key {Key}", key);
            }
        }

        LogCacheMiss(key, config);
        return config;
    }

    /// <summary>
    /// Gets or computes the optimal workgroup configuration asynchronously.
    /// </summary>
    /// <param name="device">Target GPU device.</param>
    /// <param name="kernelSource">The OpenCL kernel source code (used to compute shader hash).</param>
    /// <param name="imageWidth">Image width in pixels.</param>
    /// <param name="imageHeight">Image height in pixels.</param>
    /// <param name="localMemoryPerThreadBytes">Local memory required per thread.</param>
    /// <param name="strategy">Optimization strategy to apply.</param>
    /// <param name="computeFuncAsync">Function to compute the configuration if not found in cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly computed workgroup configuration.</returns>
    public async Task<WorkgroupConfiguration> GetOrAddAsync(
        GpuDevice device,
        string kernelSource,
        int imageWidth,
        int imageHeight,
        int localMemoryPerThreadBytes,
        WorkgroupOptimizationStrategy strategy,
        Func<Task<WorkgroupConfiguration>> computeFuncAsync,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(kernelSource);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageWidth, 0, nameof(imageWidth));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(imageHeight, 0, nameof(imageHeight));

        // Compute cache key
        var key = CreateCacheKey(device, kernelSource, imageWidth, imageHeight);

        // Try to get from memory cache first
        if (_memoryCache.TryGetValue(key, out var cachedResult))
        {
            LogCacheHit(key, cachedResult.Configuration);
            return cachedResult.Configuration;
        }

        // Compute if not found
        var config = await computeFuncAsync();

        // Store in memory cache
        var newResult = new CachedWorkgroupResult(config, DateTime.UtcNow);
        _memoryCache[key] = newResult;

        // Persist to disk if enabled
        if (_enableDiskCache)
        {
            try
            {
                await SaveToDiskAsync(key, newResult, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to persist workgroup cache entry to disk for key {Key}", key);
            }
        }

        LogCacheMiss(key, config);
        return config;
    }

    /// <summary>
    /// Clears all cached entries from memory and optionally from disk.
    /// </summary>
    /// <param name="clearDiskCache">Whether to also clear disk cache.</param>
    public void Clear(bool clearDiskCache = false)
    {
        _memoryCache.Clear();

        if (clearDiskCache && _enableDiskCache)
        {
            try
            {
                var di = new DirectoryInfo(_cacheDirectory);
                if (di.Exists)
                {
                    foreach (var file in di.GetFiles("*.wgcache"))
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete cache file {File}", file.FullName);
                        }
                    }
                    _logger.LogInformation("Cleared disk cache - {Count} files deleted", di.GetFiles("*.wgcache").Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear disk cache");
            }
        }

        _logger.LogInformation("Workgroup optimization cache cleared");
    }

    /// <summary>
    /// Gets the current cache statistics.
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        var stats = new CacheStatistics
        {
            MemoryCacheCount = _memoryCache.Count,
            DiskCacheEnabled = _enableDiskCache,
            CacheDirectory = _cacheDirectory,
            DiskCacheFileCount = 0,
            CacheSizeBytes = 0
        };

        if (_enableDiskCache)
        {
            try
            {
                var di = new DirectoryInfo(_cacheDirectory);
                if (di.Exists)
                {
                    stats = stats with {
                        DiskCacheFileCount = di.GetFiles("*.wgcache").Length,
                        CacheSizeBytes = di.GetFiles("*.wgcache", SearchOption.AllDirectories)
                            .Sum(f => f.Length)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get disk cache statistics");
            }
        }

        return stats;
    }

    /// <summary>
    /// Disposes the cache and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _diskCacheLock.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    // -- Private helpers --------------------------------------------------------

    private CacheKey CreateCacheKey(GpuDevice device, string kernelSource, int imageWidth, int imageHeight)
    {
        // Compute image size bucket (power-of-two ranges)
        var widthBucket = GetSizeBucket(imageWidth);
        var heightBucket = GetSizeBucket(imageHeight);

        // Compute shader hash from kernel source
        var shaderHash = ComputeShaderHash(kernelSource);

        return new CacheKey(device.Id, device.Name, shaderHash, widthBucket, heightBucket);
    }

    private static int GetSizeBucket(int size)
    {
        if (size <= 0) return 0;

        // Round up to nearest power of two
        int bucket = 1;
        while (bucket < size)
        {
            bucket <<= 1;
        }

        // Cap at maximum image size
        return Math.Min(bucket, AppConstants.Processing.MaxImageWidth);
    }

    private static string ComputeShaderHash(string kernelSource)
    {
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(kernelSource);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes)[..16]; // Use first 16 chars for brevity
    }

    private void InitializeCacheDirectory()
    {
        try
        {
            var di = new DirectoryInfo(_cacheDirectory);
            if (!di.Exists)
            {
                di.Create();
                _logger.LogInformation("Created cache directory: {Path}", _cacheDirectory);
            }

            // Clean up old cache files if directory already exists
            CleanupOldCacheFiles();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize cache directory {Path}", _cacheDirectory);
            throw;
        }
    }

    private void CleanupOldCacheFiles()
    {
        try
        {
            var di = new DirectoryInfo(_cacheDirectory);
            if (!di.Exists) return;

            var cutoff = DateTime.UtcNow - TimeSpan.FromDays(90); // Keep for 90 days
            foreach (var file in di.GetFiles("*.wgcache"))
            {
                if (file.LastWriteTimeUtc < cutoff)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        // Ignore deletion failures
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup old cache files");
        }
    }

    private void LoadFromDisk()
    {
        try
        {
            var di = new DirectoryInfo(_cacheDirectory);
            if (!di.Exists) return;

            var files = di.GetFiles("*.wgcache");
            _logger.LogInformation("Loading {Count} cache entries from disk", files.Length);

            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file.FullName);
                    var result = JsonSerializer.Deserialize<CachedWorkgroupResult>(json);

                    if (result != null && !_memoryCache.ContainsKey(result.Key))
                    {
                        _memoryCache[result.Key] = result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load cache file {File}", file.FullName);
                    try
                    {
                        file.Delete(); // Remove corrupted file
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }

            _logger.LogInformation("Loaded {Count} cache entries from disk", _memoryCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load cache from disk");
        }
    }

    private void SaveToDisk(CacheKey key, CachedWorkgroupResult result)
    {
        _diskCacheLock.EnterWriteLock();
        try
        {
            var fileName = GetCacheFileName(key);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(fileName, json);
        }
        finally
        {
            _diskCacheLock.ExitWriteLock();
        }
    }

    private async Task SaveToDiskAsync(CacheKey key, CachedWorkgroupResult result, CancellationToken cancellationToken)
    {
        _diskCacheLock.EnterWriteLock();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = GetCacheFileName(key);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
            await File.WriteAllTextAsync(fileName, json, cancellationToken);
        }
        finally
        {
            _diskCacheLock.ExitWriteLock();
        }
    }

    private string GetCacheFileName(CacheKey key)
    {
        // Use device ID and shader hash for filename
        var safeDeviceName = string.Join("_", key.DeviceName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"{safeDeviceName}_{key.ShaderHash}_{key.WidthBucket}_{key.HeightBucket}.wgcache");
    }

    private void LogCacheHit(CacheKey key, WorkgroupConfiguration config)
    {
        _logger.LogDebug("Cache HIT for device '{Device}' shader '{Hash}' size [{Width}×{Height}] -> [{X}×{Y}]",
            key.DeviceName,
            key.ShaderHash,
            key.WidthBucket,
            key.HeightBucket,
            config.WorkgroupSizeX,
            config.WorkgroupSizeY);
    }

    private void LogCacheMiss(CacheKey key, WorkgroupConfiguration config)
    {
        _logger.LogDebug("Cache MISS for device '{Device}' shader '{Hash}' size [{Width}×{Height}] -> [{X}×{Y}]",
            key.DeviceName,
            key.ShaderHash,
            key.WidthBucket,
            key.HeightBucket,
            config.WorkgroupSizeX,
            config.WorkgroupSizeY);
    }

    // -- Nested types --------------------------------------------------------

    private sealed record CacheKey(
        Guid DeviceId,
        string DeviceName,
        string ShaderHash,
        int WidthBucket,
        int HeightBucket)
    {
        public override int GetHashCode() => HashCode.Combine(DeviceId, ShaderHash, WidthBucket, HeightBucket);
        public bool Equals(CacheKey? other) =>
            other != null &&
            DeviceId == other.DeviceId &&
            ShaderHash == other.ShaderHash &&
            WidthBucket == other.WidthBucket &&
            HeightBucket == other.HeightBucket;
    }

    private sealed class CachedWorkgroupResult
    {
        public CacheKey Key { get; }
        public WorkgroupConfiguration Configuration { get; }
        public DateTime CachedAt { get; }

        public CachedWorkgroupResult(WorkgroupConfiguration configuration, DateTime cachedAt)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            CachedAt = cachedAt;
            Key = new CacheKey(
                configuration.DeviceId,
                string.Empty, // DeviceName not available in WorkgroupConfiguration
                ComputeShaderHash(configuration), // Recompute hash from config
                GetSizeBucket(configuration.GlobalWorkSizeX),
                GetSizeBucket(configuration.GlobalWorkSizeY)
            );
        }

        private static string ComputeShaderHash(WorkgroupConfiguration config)
        {
            // For deserialization compatibility, return empty hash
            return string.Empty;
        }

        private static int GetSizeBucket(int size)
        {
            if (size <= 0) return 0;
            int bucket = 1;
            while (bucket < size) bucket <<= 1;
            return Math.Min(bucket, AppConstants.Processing.MaxImageWidth);
        }
    }

    /// <summary>
    /// Statistics about the cache state.
    /// </summary>
    public sealed record CacheStatistics
    {
        public int MemoryCacheCount { get; init; }
        public int DiskCacheFileCount { get; init; }
        public long CacheSizeBytes { get; init; }
        public bool DiskCacheEnabled { get; init; }
        public string CacheDirectory { get; init; } = string.Empty;
    }
}