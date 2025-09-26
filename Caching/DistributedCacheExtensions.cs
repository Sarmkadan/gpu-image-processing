#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// Extension methods for <see cref="DistributedCache"/> providing higher-level cache operations.
    /// </summary>
    public static class DistributedCacheExtensions
    {
        /// <summary>
        /// Gets a value from cache or computes it if not found
        /// </summary>
        /// <typeparam name="T">Type of value to retrieve</typeparam>
        /// <param name="cache">Cache instance</param>
        /// <param name="key">Cache key</param>
        /// <param name="valueFactory">Function to compute value if not in cache</param>
        /// <param name="ttl">Optional time-to-live for the cached value</param>
        /// <returns>The cached or computed value</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache or valueFactory is null</exception>
        public static async Task<T> GetOrCreateAsync<T>(
            this DistributedCache cache,
            string key,
            Func<Task<T>> valueFactory,
            TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(valueFactory);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var (found, value) = await cache.TryGetAsync<T>(key);
            if (found)
            {
                return value;
            }

            var computedValue = await valueFactory();
            await cache.SetAsync(key, computedValue, ttl);
            return computedValue;
        }

        /// <summary>
        /// Gets multiple values from cache in a single batch operation
        /// </summary>
        /// <typeparam name="T">Type of values to retrieve</typeparam>
        /// <param name="cache">Cache instance</param>
        /// <param name="keys">Collection of cache keys</param>
        /// <returns>Dictionary mapping keys to found values (missing keys are not included)</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache or keys is null</exception>
        public static async Task<IReadOnlyDictionary<string, T>> GetManyAsync<T>(
            this DistributedCache cache,
            IEnumerable<string> keys)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(keys);

            var results = new Dictionary<string, T>();
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                var (found, value) = await cache.TryGetAsync<T>(key);
                if (found)
                {
                    results[key] = value;
                }
            }

            return results.AsReadOnly();
        }

        /// <summary>
        /// Sets multiple values in cache in a single batch operation
        /// </summary>
        /// <typeparam name="T">Type of values to store</typeparam>
        /// <param name="cache">Cache instance</param>
        /// <param name="values">Dictionary mapping keys to values</param>
        /// <param name="ttl">Optional time-to-live for all values</param>
        /// <returns>Task representing the async operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache or values is null</exception>
        public static async Task SetManyAsync<T>(
            this DistributedCache cache,
            IReadOnlyDictionary<string, T> values,
            TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(values);

            foreach (var kvp in values)
            {
                await cache.SetAsync(kvp.Key, kvp.Value, ttl);
            }
        }

        /// <summary>
        /// Removes multiple keys from cache in a single batch operation
        /// </summary>
        /// <param name="cache">Cache instance</param>
        /// <param name="keys">Collection of cache keys to remove</param>
        /// <returns>Number of keys successfully removed</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache or keys is null</exception>
        public static async Task<int> RemoveManyAsync(
            this DistributedCache cache,
            IEnumerable<string> keys)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(keys);

            var removedCount = 0;
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (await cache.RemoveAsync(key))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Gets cache statistics as a formatted string for logging/debugging
        /// </summary>
        /// <param name="cache">Cache instance</param>
        /// <returns>Formatted statistics string</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache is null</exception>
        public static string GetStatsString(this DistributedCache cache)
        {
            ArgumentNullException.ThrowIfNull(cache);

            var stats = cache.GetStats();
            return string.Create(
                CultureInfo.InvariantCulture,
                $@"Cache Stats:
  Items: {stats.ItemCount:N0}
  Memory: {stats.UsedMemoryBytes:N0} / {stats.MaxMemoryBytes:N0} bytes ({stats.MemoryUsagePercent:F2}%)
  Avg Size: {stats.AverageItemSize:N2} bytes
  Total Accesses: {stats.TotalAccesses:N0}
  Hot Items: {stats.HotItems.Count}");
        }

        /// <summary>
        /// Checks if a key exists in cache without retrieving its value
        /// </summary>
        /// <param name="cache">Cache instance</param>
        /// <param name="key">Cache key to check</param>
        /// <returns>True if key exists, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache is null</exception>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        public static async Task<bool> ContainsKeyAsync(this DistributedCache cache, string key)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var (found, _) = await cache.TryGetAsync<object>(key);
            return found;
        }

        /// <summary>
        /// Gets the expiration time for a specific key
        /// </summary>
        /// <param name="cache">Cache instance</param>
        /// <param name="key">Cache key</param>
        /// <returns>Expiration time if set, null if key doesn't exist or has no expiration</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache is null</exception>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        public static async Task<DateTime?> GetExpirationAsync(this DistributedCache cache, string key)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var (found, entry) = await cache.TryGetAsync<CacheEntryInternal>(key);
            return found ? entry?.ExpiresAt : null;
        }

        /// <summary>
        /// Gets cache entry metadata without retrieving the actual value
        /// </summary>
        /// <param name="cache">Cache instance</param>
        /// <param name="key">Cache key</param>
        /// <returns>Cache entry metadata or null if key doesn't exist</returns>
        /// <exception cref="ArgumentNullException">Thrown when cache is null</exception>
        /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
        public static async Task<CacheEntryMetadata?> GetMetadataAsync(this DistributedCache cache, string key)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var (found, _) = await cache.TryGetAsync<object>(key);
            if (!found)
            {
                return null;
            }

            var stats = cache.GetStats();
            var entryStats = stats.HotItems
                .OfType<dynamic>()
                .FirstOrDefault(e => e.Key == key);

            if (entryStats != null)
            {
                return new CacheEntryMetadata
                {
                    Key = key,
                    SizeBytes = entryStats.SizeBytes,
                    CreatedAt = entryStats.CreatedAt,
                    LastAccessedAt = entryStats.LastAccessedAt,
                    AccessCount = entryStats.AccessCount,
                    ExpiresAt = entryStats.ExpiresAt
                };
            }

            return null;
        }
    }

    /// <summary>
    /// Represents metadata about a cache entry
    /// </summary>
    public class CacheEntryMetadata
    {
        public string Key { get; set; } = string.Empty;
        public int SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public long AccessCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Internal cache entry structure for metadata operations
    /// </summary>
    sealed class CacheEntryInternal
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public long AccessCount { get; set; }
    }
}
