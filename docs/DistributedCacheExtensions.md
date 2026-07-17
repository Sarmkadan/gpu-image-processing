# DistributedCacheExtensions

Extension methods for `IDistributedCache` that provide typed, asynchronous access to a distributed cache with metadata tracking and bulk operations. These methods simplify common cache patterns such as retrieving or creating values with expiration policies, managing multiple keys atomically, and inspecting cache metadata without manual serialization.

## API

### `GetOrCreateAsync<T>`
Retrieves a value from the cache by key, or creates and stores a new value if the key does not exist. The creation function is invoked only when the key is missing.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `key`: The cache key to look up.
  - `createItem`: A delegate that creates the value if the key is missing.
  - `options`: Optional cache entry options (e.g., expiration, size).
- **Return value**: A `Task<T>` resolving to the cached or newly created value.
- **Exceptions**: Throws `ArgumentNullException` if `cache`, `key`, or `createItem` is `null`. Throws `InvalidOperationException` if deserialization fails.

---

### `GetManyAsync<T>`
Retrieves multiple values from the cache in a single atomic operation. Returns only keys that exist; missing keys are omitted from the result.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `keys`: A collection of keys to retrieve.
- **Return value**: A `Task<IReadOnlyDictionary<string, T>>` mapping existing keys to their deserialized values.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `keys` is `null`.

---

### `SetManyAsync<T>`
Stores multiple values in the cache in a single atomic operation. Existing values for the same keys are overwritten.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `items`: A dictionary mapping keys to values to store.
  - `options`: Optional cache entry options applied to all entries.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `items` is `null`.

---
### `RemoveManyAsync`
Removes multiple keys from the cache in a single atomic operation.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `keys`: A collection of keys to remove.
- **Return value**: A `Task<int>` indicating the number of keys successfully removed.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `keys` is `null`.

---
### `GetStatsString`
Generates a human-readable summary of cache usage, including total size in bytes and key count.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
- **Return value**: A `string` containing the stats summary.
- **Exceptions**: Throws `ArgumentNullException` if `cache` is `null`.

---
### `ContainsKeyAsync`
Checks whether a key exists in the cache.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `key`: The key to check.
- **Return value**: A `Task<bool>` indicating whether the key exists.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `key` is `null`.

---
### `GetExpirationAsync`
Retrieves the expiration timestamp for a cached entry.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `key`: The key whose expiration is queried.
- **Return value**: A `Task<DateTime?>` representing the expiration time, or `null` if the entry never expires.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `key` is `null`. Throws `KeyNotFoundException` if the key does not exist.

---
### `GetMetadataAsync`
Retrieves metadata associated with a cached entry, including creation time, last access time, access count, and expiration.

- **Parameters**
  - `cache`: The `IDistributedCache` instance.
  - `key`: The key whose metadata is queried.
- **Return value**: A `Task<CacheEntryMetadata?>` containing metadata if the key exists, otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `key` is `null`.

---
### `CacheEntryMetadata`
A record containing metadata for a cache entry.

- **Properties**
  - `Key`: The cache key.
  - `Value`: The serialized value (base64-encoded).
  - `SizeBytes`: The size of the value in bytes.
  - `CreatedAt`: The timestamp when the entry was created.
  - `LastAccessedAt`: The timestamp when the entry was last accessed.
  - `AccessCount`: The number of times the entry has been accessed.
  - `ExpiresAt`: The expiration timestamp, or `null` if the entry does not expire.

## Usage

```csharp
// Example 1: Retrieve or create a processed image with expiration
var cache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
var imageKey = "processed:image:12345";

var imageData = await cache.GetOrCreateAsync(
    imageKey,
    async entry =>
    {
        var raw = await DownloadImageAsync("https://example.com/image.png");
        return ProcessImage(raw);
    },
    new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });

// Example 2: Bulk operations for batch processing
var batchKeys = new[] { "batch:1:result", "batch:2:result", "batch:3:result" };
var results = await cache.GetManyAsync<string>(batchKeys);

var newResults = new Dictionary<string, string>
{
    { "batch:4:result", "processed4" },
    { "batch:5:result", "processed5" }
};
await cache.SetManyAsync(newResults);

var removedCount = await cache.RemoveManyAsync(batchKeys.Take(2).ToList());
```

## Notes

- **Atomicity**: Bulk operations (`GetManyAsync`, `SetManyAsync`, `RemoveManyAsync`) are atomic with respect to the cache backend; either all keys are processed or none are, depending on backend support.
- **Thread safety**: All methods are thread-safe with respect to the `IDistributedCache` instance; concurrent calls to the same key may result in redundant work but will not corrupt data.
- **Serialization**: Values are serialized using the default `System.Text.Json` serializer; ensure types are serializable or provide a custom serializer if needed.
- **Metadata accuracy**: `LastAccessedAt` and `AccessCount` are updated only when `GetOrCreateAsync` or `GetManyAsync` successfully retrieves and deserializes a value; manual `Set` operations do not update these fields unless followed by a retrieval.
- **Expiration semantics**: Absolute expiration is enforced by the cache backend; relative expiration is calculated at insertion time.
