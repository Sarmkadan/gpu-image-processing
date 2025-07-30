# ProcessingCache

A thread-safe, time-limited cache implementation designed for GPU-accelerated image processing pipelines. It stores processed image results with configurable time-to-live (TTL) semantics, allowing efficient reuse of expensive computations while preventing unbounded memory growth. The cache automatically evicts stale entries based on access time and enforces a maximum entry count to maintain memory constraints.

## API

### `ProcessingCache(int maxEntries, TimeSpan ttl)`

Initializes a new cache with the specified maximum entry count and time-to-live duration for cached items.

- **Parameters**
  - `maxEntries`: Maximum number of entries the cache can hold before eviction occurs.
  - `ttl`: Duration after which an entry is considered stale and eligible for eviction.
- **Throws**
  - `ArgumentOutOfRangeException`: If `maxEntries` is less than or equal to zero, or if `ttl` is negative.

---

### `void Set<T>(T value)`

Stores a value in the cache with the default TTL. The value is associated with its type `T` as the cache key.

- **Parameters**
  - `value`: The value to cache. Must not be `null`.
- **Throws**
  - `ArgumentNullException`: If `value` is `null`.
  - `InvalidOperationException`: If the cache has reached its maximum capacity and no entries can be evicted to make room.

---

### `void Set<T>(T value, TimeSpan customTtl)`

Stores a value in the cache with a custom time-to-live duration. The value is associated with its type `T` as the cache key.

- **Parameters**
  - `value`: The value to cache. Must not be `null`.
  - `customTtl`: Custom TTL for this specific entry. Must not be negative.
- **Throws**
  - `ArgumentNullException`: If `value` is `null`.
  - `ArgumentOutOfRangeException`: If `customTtl` is negative.
  - `InvalidOperationException`: If the cache has reached its maximum capacity and no entries can be evicted to make room.

---

### `bool TryGet<T>(out T value)`

Attempts to retrieve a cached value of type `T`. Updates the last-accessed timestamp of the entry if found.

- **Parameters**
  - `value`: Output parameter receiving the cached value if found.
- **Returns**
  - `true` if the value was found and is not stale; otherwise, `false`.
- **Remarks**
  - The TTL is checked against the current time using the entry's `LastAccessedAt` timestamp plus its TTL.

---
### `bool ContainsKey<T>()`

Checks whether a cached value of type `T` exists and is not stale.

- **Returns**
  - `true` if a non-stale entry for type `T` exists; otherwise, `false`.

---
### `bool Remove<T>()`

Removes the cached entry associated with type `T` if it exists.

- **Returns**
  - `true` if an entry was found and removed; otherwise, `false`.

---
### `void Clear()`

Removes all entries from the cache, resetting statistics and freeing resources.

---
### `CacheStatistics GetStatistics()`

Retrieves current cache statistics including entry count, utilization, and memory pressure indicators.

- **Returns**
  - A `CacheStatistics` struct containing:
    - `EntryCount`: Current number of entries.
    - `MaxEntries`: Maximum allowed entries.
    - `UtilizationPercent`: Percentage of capacity currently used.
    - `OldestEntryAge`: Age of the oldest entry (if any).
    - `MemoryPressure`: Qualitative indicator of memory usage.

---
### `void Dispose()`

Releases all resources held by the cache, including clearing all entries and releasing synchronization primitives.

---
### `object Value`

Gets the cached value associated with the current generic type parameter. Only valid when accessed through a type-safe wrapper or after a successful `TryGet<T>` call.

- **Remarks**
  - Throws `InvalidOperationException` if the entry does not exist or is stale.
  - The value is returned as `object` to support non-generic access patterns.

---
### `DateTime CreatedAt`

Gets the timestamp when the current entry was created. Only valid when accessed through a type-safe wrapper or after a successful `TryGet<T>` call.

---
### `DateTime LastAccessedAt`

Gets the timestamp when the current entry was last accessed. Only valid when accessed through a type-safe wrapper or after a successful `TryGet<T>` call.

---
### `TimeSpan Ttl`

Gets the time-to-live duration for the current entry. Only valid when accessed through a type-safe wrapper or after a successful `TryGet<T>` call.

---
### `int EntryCount`

Gets the current number of entries in the cache.

---
### `int MaxEntries`

Gets the maximum number of entries the cache is configured to hold.

---
### `double UtilizationPercent`

Gets the current utilization percentage of the cache (i.e., `EntryCount / MaxEntries * 100`).

## Usage
