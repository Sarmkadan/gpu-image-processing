#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// In-memory caching service for processing results and intermediate data.
    /// Implements LRU (Least Recently Used) eviction policy with TTL support.
    /// </summary>
    public class ProcessingCache : IDisposable
    {
        private readonly ILogger<ProcessingCache> _logger;
        private readonly Dictionary<string, CacheEntry> _cache;
        private readonly int _maxEntries;
        private readonly TimeSpan _defaultTtl;
        private readonly object _lockObject = new object();

        public ProcessingCache(ILogger<ProcessingCache> logger, int maxEntries = 1000, TimeSpan? defaultTtl = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxEntries = maxEntries;
            _defaultTtl = defaultTtl ?? TimeSpan.FromHours(1);
            _cache = new Dictionary<string, CacheEntry>();
        }

        /// <summary>
        /// Gets the number of entries currently in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _cache.Count;
                }
            }
        }

        /// <summary>
        /// Adds or updates a cache entry with default TTL.
        /// </summary>
        public void Set(string key, object value)
        {
            Set(key, value, _defaultTtl);
        }

        /// <summary>
        /// Adds or updates a cache entry with specific TTL.
        /// Evicts least recently used entry if cache is full.
        /// </summary>
        public void Set(string key, object value, TimeSpan? ttl)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be empty", nameof(key));

            lock (_lockObject)
            {
                // Remove expired entries first
                RemoveExpiredEntries();

                // Check if we need to evict
                if (_cache.Count >= _maxEntries && !_cache.ContainsKey(key))
                {
                    EvictLruEntry();
                }

                var entry = new CacheEntry
                {
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow,
                    Ttl = ttl ?? _defaultTtl
                };

                _cache[key] = entry;

                _logger.LogDebug(
                    "Cache entry set - Key: {CacheKey}, TTL: {Ttl}s, Size: {CacheSize}",
                    key,
                    entry.Ttl.TotalSeconds,
                    _cache.Count);
            }
        }

        /// <summary>
        /// Retrieves a value from the cache if it exists and hasn't expired.
        /// Updates last access time for LRU tracking.
        /// </summary>
        public bool TryGet<T>(string key, out T value) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            lock (_lockObject)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    // Check if expired
                    if (DateTime.UtcNow - entry.CreatedAt > entry.Ttl)
                    {
                        _cache.Remove(key);
                        value = null;
                        _logger.LogDebug("Cache entry expired - Key: {CacheKey}", key);
                        return false;
                    }

                    // Update last accessed time
                    entry.LastAccessedAt = DateTime.UtcNow;
                    value = entry.Value as T;

                    _logger.LogDebug("Cache hit - Key: {CacheKey}", key);
                    return true;
                }

                value = null;
                _logger.LogDebug("Cache miss - Key: {CacheKey}", key);
                return false;
            }
        }

        /// <summary>
        /// Checks if a cache entry exists and is valid.
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            lock (_lockObject)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    // Check if expired
                    if (DateTime.UtcNow - entry.CreatedAt > entry.Ttl)
                    {
                        _cache.Remove(key);
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Removes a cache entry by key.
        /// </summary>
        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            lock (_lockObject)
            {
                bool removed = _cache.Remove(key);
                if (removed)
                {
                    _logger.LogDebug("Cache entry removed - Key: {CacheKey}", key);
                }

                return removed;
            }
        }

        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _cache.Clear();
                _logger.LogInformation("Cache cleared");
            }
        }

        /// <summary>
        /// Gets cache statistics including size and hit rate.
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                return new CacheStatistics
                {
                    EntryCount = _cache.Count,
                    MaxEntries = _maxEntries,
                    UtilizationPercent = (_cache.Count / (double)_maxEntries) * 100
                };
            }
        }

        /// <summary>
        /// Removes all expired entries from the cache.
        /// </summary>
        private void RemoveExpiredEntries()
        {
            var expiredKeys = _cache
                .Where(kvp => DateTime.UtcNow - kvp.Value.CreatedAt > kvp.Value.Ttl)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Removed {ExpiredCount} expired cache entries", expiredKeys.Count);
            }
        }

        /// <summary>
        /// Evicts the least recently used entry from the cache.
        /// </summary>
        private void EvictLruEntry()
        {
            var lruEntry = _cache
                .OrderBy(kvp => kvp.Value.LastAccessedAt)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lruEntry.Key))
            {
                _cache.Remove(lruEntry.Key);
                _logger.LogDebug(
                    "Evicted LRU cache entry - Key: {CacheKey}, LastAccess: {LastAccess}",
                    lruEntry.Key,
                    lruEntry.Value.LastAccessedAt);
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>
    /// Internal cache entry structure.
    /// </summary>
    internal class CacheEntry
    {
        public object Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public TimeSpan Ttl { get; set; }
    }

    /// <summary>
    /// Cache statistics information.
    /// </summary>
    public class CacheStatistics
    {
        public int EntryCount { get; set; }
        public int MaxEntries { get; set; }
        public double UtilizationPercent { get; set; }
    }
}
