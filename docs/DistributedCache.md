# DistributedCache

A thread-safe, memory-bounded distributed cache implementation that provides asynchronous access to cached items with automatic expiry tracking and detailed usage statistics. It supports generic typed storage, manual and time-based eviction, and exposes real-time memory consumption metrics to enable proactive cache management in GPU image processing pipelines.

## API

### DistributedCache

Creates a new cache instance with the specified maximum memory limit. The cache will reject insertions that would exceed this bound and supports background expiry cleanup.

```csharp
public DistributedCache(long maxMemoryBytes)
```

**Parameters:**
- `maxMemoryBytes` — The maximum total size in bytes the cache is allowed to occupy. Must be greater than zero.

**Throws:**
- `ArgumentOutOfRangeException` when `maxMemoryBytes` is zero or negative.

---

### SetAsync\<T\>

Stores a value of type `T` under the given key, associating it with a size in bytes and an optional absolute expiration time. If the key already exists, its value, size, and expiry are overwritten. The operation fails when the new item would cause the cache to exceed its configured memory limit.

```csharp
public async Task SetAsync<T>(string key, T value, int sizeBytes, DateTime? expiresAt = null)
```

**Parameters:**
- `key` — Non-null, non-empty unique identifier for the cached entry.
- `value` — The object to cache. Must not be null.
- `sizeBytes` — The memory footprint of the value in bytes. Must be positive.
- `expiresAt` — Optional absolute UTC time after which the entry is considered stale and eligible for removal.

**Returns:** A `Task` that completes when the value has been stored.

**Throws:**
- `ArgumentNullException` when `key` is null or `value` is null.
- `ArgumentException` when `key` is empty or `sizeBytes` is zero or negative.
- `InvalidOperationException` when inserting the item would exceed `MaxMemoryBytes` and no expired entries can be evicted to make room.

---

### TryGetAsync\<T\>

Attempts to retrieve a cached value by key. Returns both a found indicator and the value if present and not expired. Automatically updates the entry’s last-accessed timestamp and increments its access counter.

```csharp
public async Task<(bool Found, T Value)> TryGetAsync<T>(string key)
```

**Parameters:**
- `key` — The key to look up. Must not be null or empty.

**Returns:** A tuple where `Found` is `true` and `Value` contains the cached object if the key exists and has not expired; otherwise `Found` is `false` and `Value` is `default(T)`.

**Throws:**
- `ArgumentNullException` when `key` is null.
- `ArgumentException` when `key` is empty.

---

### RemoveAsync

Removes the entry associated with the specified key and frees its tracked memory. Returns whether the key existed and was removed.

```csharp
public async Task<bool> RemoveAsync(string key)
```

**Parameters:**
- `key` — The key to remove. Must not be null or empty.

**Returns:** `true` if the key was found and removed; `false` if the key did not exist.

**Throws:**
- `ArgumentNullException` when `key` is null.
- `ArgumentException` when `key` is empty.

---

### ClearAsync

Removes all entries from the cache immediately, resetting memory usage to zero and discarding all access statistics for the cleared items.

```csharp
public async Task ClearAsync()
```

**Returns:** A `Task` that completes when the cache is empty.

---

### GetStats

Returns a snapshot of the cache’s current operational statistics, including item count, memory usage, and aggregate access metrics.

```csharp
public CacheStats GetStats()
```

**Returns:** A `CacheStats` object populated with the latest values. The returned instance is a point-in-time copy and does not reflect subsequent cache mutations.

---

### CleanupExpiredAsync

Scans the entire cache for entries whose `ExpiresAt` timestamp has passed and removes them. This can be called proactively to free memory before inserting large items.

```csharp
public async Task CleanupExpiredAsync()
```

**Returns:** A `Task` that completes when all expired entries have been evicted.

---

### CacheStats

A read-only data transfer object that exposes the cache’s current state and aggregate metrics.

#### Key

The cache key of the most recently accessed or modified entry at the time stats were captured. May be null if the cache is empty.

```csharp
public string Key { get; }
```

#### Value

The string representation of the value associated with `Key`. May be null.

```csharp
public string Value { get; }
```

#### SizeBytes

The size in bytes of the entry identified by `Key`.

```csharp
public int SizeBytes { get; }
```

#### CreatedAt

The UTC creation timestamp of the entry identified by `Key`.

```csharp
public DateTime CreatedAt { get; }
```

#### LastAccessedAt

The UTC timestamp of the last read or write access to the entry identified by `Key`.

```csharp
public DateTime LastAccessedAt { get; }
```

#### ExpiresAt

The absolute expiration time of the entry identified by `Key`, or null if no expiration was set.

```csharp
public DateTime? ExpiresAt { get; }
```

#### AccessCount

The number of times the entry identified by `Key` has been successfully retrieved.

```csharp
public long AccessCount { get; }
```

#### ItemCount

The total number of non-expired entries currently in the cache.

```csharp
public int ItemCount { get; }
```

#### UsedMemoryBytes

The sum of `SizeBytes` for all non-expired entries currently in the cache.

```csharp
public long UsedMemoryBytes { get; }
```

#### MaxMemoryBytes

The configured maximum memory capacity of the cache.

```csharp
public long MaxMemoryBytes { get; }
```

#### MemoryUsagePercent

The ratio of `UsedMemoryBytes` to `MaxMemoryBytes`, expressed as a percentage between 0 and 100.

```csharp
public float MemoryUsagePercent { get; }
```

#### AverageItemSize

The arithmetic mean of `SizeBytes` across all non-expired entries. Returns zero when the cache is empty.

```csharp
public double AverageItemSize { get; }
```

#### TotalAccesses

The cumulative number of successful `TryGetAsync` calls across all entries since the cache was created or last cleared.

```csharp
public long TotalAccesses { get; }
```

## Usage

### Example 1: Caching GPU-processed image tiles with expiration

```csharp
var cache = new DistributedCache(maxMemoryBytes: 512 * 1024 * 1024); // 512 MB

async Task<ProcessedTile?> GetTileAsync(string tileId)
{
    var (found, tile) = await cache.TryGetAsync<ProcessedTile>(tileId);
    if (found)
        return tile;

    tile = await RenderTileFromSourceAsync(tileId);
    int tileSize = EstimateTileMemory(tile);

    // Evict stale entries if needed to make room
    await cache.CleanupExpiredAsync();

    await cache.SetAsync(tileId, tile, tileSize, expiresAt: DateTime.UtcNow.AddMinutes(10));
    return tile;
}
```

### Example 2: Monitoring cache pressure before batch operations

```csharp
var cache = new DistributedCache(maxMemoryBytes: 1024 * 1024 * 1024); // 1 GB

async Task InsertBatchAsync(IEnumerable<ImagePatch> patches)
{
    var stats = cache.GetStats();

    if (stats.MemoryUsagePercent > 85.0f)
    {
        await cache.CleanupExpiredAsync();
        stats = cache.GetStats();

        if (stats.MemoryUsagePercent > 90.0f)
            throw new InvalidOperationException("Cache memory pressure too high for batch insert.");
    }

    foreach (var patch in patches)
    {
        await cache.SetAsync(
            key: patch.Id,
            value: patch,
            sizeBytes: patch.Data.Length,
            expiresAt: DateTime.UtcNow.AddHours(1));
    }
}
```

## Notes

- **Thread safety:** All public instance methods (`SetAsync`, `TryGetAsync`, `RemoveAsync`, `ClearAsync`, `CleanupExpiredAsync`, `GetStats`) are safe to call concurrently from multiple threads. Internal locking ensures consistent memory accounting and prevents corruption of the underlying key-value store.
- **Expiry behavior:** Expired entries are not automatically removed on access unless `CleanupExpiredAsync` is called or a write triggers eviction. `TryGetAsync` will treat an expired entry as a cache miss (`Found = false`) but will not physically remove it from storage or free its tracked memory until explicit cleanup occurs.
- **Memory enforcement:** `SetAsync` checks the total `UsedMemoryBytes` against `MaxMemoryBytes` *before* inserting. If the new item would push usage over the limit, the operation throws `InvalidOperationException` rather than silently evicting arbitrary entries. Callers should invoke `CleanupExpiredAsync` or manually remove lower-priority items to free capacity.
- **Size tracking:** The `sizeBytes` argument is caller-provided; the cache does not measure object sizes internally. Inaccurate size reporting will cause `MemoryUsagePercent` and `UsedMemoryBytes` to diverge from actual memory consumption.
- **Stats snapshot:** `GetStats` returns a copy of the current metrics. The `Key`, `Value`, `SizeBytes`, `CreatedAt`, `LastAccessedAt`, `ExpiresAt`, and `AccessCount` fields reflect the single entry that was most recently accessed or modified at the moment of the call, not an aggregate or a user-specified entry.
- **Null values:** `SetAsync<T>` rejects null values to prevent ambiguity with `TryGetAsync<T>` returning `default(T)` on a miss. Use a sentinel wrapper type if you need to cache the absence of data.
