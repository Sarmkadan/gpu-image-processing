#nullable enable
using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// Extension methods that add convenient functionality to <see cref="ProcessingCache"/>.
    /// </summary>
    public static class ProcessingCacheExtensions
    {
        /// <summary>
        /// Retrieves a cached value if it exists; otherwise creates the value using the supplied factory,
        /// stores it in the cache and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The cache key.</param>
        /// <param name="valueFactory">A factory that creates the value when it is missing.</param>
        /// <param name="ttl">Optional time‑to‑live for the newly created entry. If <c>null</c>, the cache's default TTL is used.</param>
        /// <returns>The cached or newly created value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="valueFactory"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or whitespace.</exception>
        public static T GetOrAdd<T>(this ProcessingCache cache, string key, Func<T> valueFactory, TimeSpan? ttl = null) where T : class
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(valueFactory);

            return cache.TryGet<T>(key, out var existing) ? existing : CreateAndCache();

            T CreateAndCache()
            {
                var value = valueFactory();
                cache.Set(key, value, ttl);
                return value;
            }
        }

        /// <summary>
        /// Attempts to retrieve a cached value; if the key is missing, returns the supplied default value.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The cache key.</param>
        /// <param name="defaultValue">The value to return when the key is not present.</param>
        /// <returns>The cached value if present; otherwise <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cache"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or whitespace.</exception>
        public static T TryGetOrDefault<T>(this ProcessingCache cache, string key, T defaultValue = default) where T : class
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            return cache.TryGet<T>(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Adds a cache entry only if the key does not already exist.
        /// </summary>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to store.</param>
        /// <param name="ttl">Optional time‑to‑live for the entry. If <c>null</c>, the cache's default TTL is used.</param>
        /// <returns><c>true</c> if the entry was added; <c>false</c> if the key already existed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cache"/> or <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is <c>null</c> or whitespace.</exception>
        public static bool SetIfNotExists(this ProcessingCache cache, string key, object value, TimeSpan? ttl = null)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(value);

            if (cache.ContainsKey(key))
                return false;

            cache.Set(key, value, ttl);
            return true;
        }

        /// <summary>
        /// Returns a human‑readable summary of the cache statistics.
        /// </summary>
        /// <param name="cache">The cache instance.</param>
        /// <returns>A formatted string containing entry count, capacity and utilization percent.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cache"/> is <c>null</c>.</exception>
        public static string GetStatisticsString(this ProcessingCache cache)
        {
            ArgumentNullException.ThrowIfNull(cache);

            var stats = cache.GetStatistics();
            return $"Cache entries: {stats.EntryCount}/{stats.MaxEntries} ({stats.UtilizationPercent:F2}% utilized)";
        }
    }
}