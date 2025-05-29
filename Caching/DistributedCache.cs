#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// Distributed cache abstraction supporting both in-memory and external backends.
    /// Provides key-value storage with TTL, statistics, and eviction policies.
    /// </summary>
    public class DistributedCache
    {
        private readonly Dictionary<string, CacheEntry> _memoryStore;
        private readonly CacheEvictionPolicy _evictionPolicy;
        private readonly long _maxMemoryBytes;
        private long _currentMemoryBytes;
        private readonly object _lockObject = new();

        public event EventHandler<CacheEventArgs> ItemEvicted;
        public event EventHandler<CacheEventArgs> ItemExpired;

        public DistributedCache(
            long maxMemoryBytes = 500 * 1024 * 1024,
            CacheEvictionPolicy evictionPolicy = CacheEvictionPolicy.LRU)
        {
            _memoryStore = new Dictionary<string, CacheEntry>();
            _maxMemoryBytes = maxMemoryBytes;
            _currentMemoryBytes = 0;
            _evictionPolicy = evictionPolicy;
        }

        /// <summary>
        /// Stores a value in cache with optional TTL
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be empty", nameof(key));

            lock (_lockObject)
            {
                var serialized = System.Text.Json.JsonSerializer.Serialize(value);
                var sizeBytes = System.Text.Encoding.UTF8.GetByteCount(serialized);

                // Remove existing entry if present
                if (_memoryStore.TryGetValue(key, out var existing))
                {
                    _currentMemoryBytes -= existing.SizeBytes;
                }

                // Ensure space available
                while (_currentMemoryBytes + sizeBytes > _maxMemoryBytes && _memoryStore.Count > 0)
                {
                    EvictEntry();
                }

                var expiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;

                _memoryStore[key] = new CacheEntry
                {
                    Key = key,
                    Value = serialized,
                    SizeBytes = sizeBytes,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    AccessCount = 0
                };

                _currentMemoryBytes += sizeBytes;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves a value from cache
        /// </summary>
        public async Task<(bool Found, T Value)> TryGetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return (false, default);

            lock (_lockObject)
            {
                if (!_memoryStore.TryGetValue(key, out var entry))
                    return (false, default);

                // Check expiration
                if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt.Value)
                {
                    _memoryStore.Remove(key);
                    _currentMemoryBytes -= entry.SizeBytes;
                    ItemExpired?.Invoke(this, new CacheEventArgs { Key = key });
                    return (false, default);
                }

                // Update access metadata
                entry.LastAccessedAt = DateTime.UtcNow;
                entry.AccessCount++;

                try
                {
                    var value = System.Text.Json.JsonSerializer.Deserialize<T>(entry.Value);
                    return (true, value);
                }
                catch
                {
                    return (false, default);
                }
            }
        }

        /// <summary>
        /// Removes an item from cache
        /// </summary>
        public async Task<bool> RemoveAsync(string key)
        {
            lock (_lockObject)
            {
                if (_memoryStore.TryGetValue(key, out var entry))
                {
                    _memoryStore.Remove(key);
                    _currentMemoryBytes -= entry.SizeBytes;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Clears all cache entries
        /// </summary>
        public async Task ClearAsync()
        {
            lock (_lockObject)
            {
                _memoryStore.Clear();
                _currentMemoryBytes = 0;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        public CacheStats GetStats()
        {
            lock (_lockObject)
            {
                var entries = _memoryStore.Values.ToList();
                var totalAccess = entries.Sum(e => e.AccessCount);

                return new CacheStats
                {
                    ItemCount = _memoryStore.Count,
                    UsedMemoryBytes = _currentMemoryBytes,
                    MaxMemoryBytes = _maxMemoryBytes,
                    MemoryUsagePercent = (_currentMemoryBytes / (float)_maxMemoryBytes) * 100,
                    AverageItemSize = entries.Count > 0 ? entries.Average(e => e.SizeBytes) : 0,
                    TotalAccesses = totalAccess,
                    HotItems = entries.OrderByDescending(e => e.AccessCount).Take(10)
                        .Select(e => (dynamic)new { e.Key, e.AccessCount, e.SizeBytes })
                        .ToList()
                };
            }
        }

        /// <summary>
        /// Removes expired entries
        /// </summary>
        public async Task CleanupExpiredAsync()
        {
            var now = DateTime.UtcNow;

            lock (_lockObject)
            {
                var expiredKeys = _memoryStore
                    .Where(kvp => kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt.Value <= now)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    if (_memoryStore.TryGetValue(key, out var entry))
                    {
                        _memoryStore.Remove(key);
                        _currentMemoryBytes -= entry.SizeBytes;
                        ItemExpired?.Invoke(this, new CacheEventArgs { Key = key });
                    }
                }
            }

            await Task.CompletedTask;
        }

        private void EvictEntry()
        {
            CacheEntry entryToEvict = _evictionPolicy switch
            {
                CacheEvictionPolicy.LRU => _memoryStore.Values.OrderBy(e => e.LastAccessedAt).First(),
                CacheEvictionPolicy.LFU => _memoryStore.Values.OrderBy(e => e.AccessCount).First(),
                CacheEvictionPolicy.FIFO => _memoryStore.Values.OrderBy(e => e.CreatedAt).First(),
                _ => _memoryStore.Values.First()
            };

            _memoryStore.Remove(entryToEvict.Key);
            _currentMemoryBytes -= entryToEvict.SizeBytes;
            ItemEvicted?.Invoke(this, new CacheEventArgs { Key = entryToEvict.Key });
        }

        private class CacheEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public int SizeBytes { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccessedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public long AccessCount { get; set; }
        }
    }

    public class CacheStats
    {
        public int ItemCount { get; set; }
        public long UsedMemoryBytes { get; set; }
        public long MaxMemoryBytes { get; set; }
        public float MemoryUsagePercent { get; set; }
        public double AverageItemSize { get; set; }
        public long TotalAccesses { get; set; }
        public List<dynamic> HotItems { get; set; }
    }

    public class CacheEventArgs : EventArgs
    {
        public string Key { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    public enum CacheEvictionPolicy
    {
        LRU,  // Least Recently Used
        LFU,  // Least Frequently Used
        FIFO  // First In First Out
    }
}
