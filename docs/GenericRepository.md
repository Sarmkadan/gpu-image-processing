# GenericRepository

`GenericRepository` is an abstract base class providing a reusable, entity-agnostic data access layer for the `gpu-image-processing` project. It encapsulates common CRUD operations, existence checks, pagination, and persistence concerns against an underlying data store, allowing derived repositories to inherit standard implementations while overriding methods for entity-specific behavior.

## API

### `public virtual Task<T?> GetByIdAsync(object id)`
Retrieves a single entity by its primary key.
- **Parameters**: `id` — the primary key value. The concrete implementation determines the expected type (e.g., `int`, `Guid`, `string`).
- **Returns**: The entity of type `T` if found; `null` otherwise.
- **Throws**: May throw `ArgumentNullException` if `id` is `null`. Derived implementations may throw store-specific exceptions (e.g., timeout, connection failure).

### `public virtual Task<IEnumerable<T>> GetAllAsync()`
Returns all entities of type `T` from the store.
- **Returns**: An `IEnumerable<T>` containing all records. May be empty if no records exist.
- **Throws**: Store-specific exceptions on connection or query failures.

### `public virtual Task<T> AddAsync(T entity)`
Persists a new entity to the store.
- **Parameters**: `entity` — the entity to add. Must not be `null`.
- **Returns**: The added entity, typically with any store-generated values (e.g., auto-incremented ID) applied.
- **Throws**: `ArgumentNullException` if `entity` is `null`. `InvalidOperationException` or store-specific exceptions on constraint violations or connection failures.

### `public virtual Task<T> UpdateAsync(T entity)`
Persists modifications to an existing entity.
- **Parameters**: `entity` — the entity with updated values. Must not be `null` and must represent an existing record.
- **Returns**: The updated entity as it exists in the store after the operation.
- **Throws**: `ArgumentNullException` if `entity` is `null`. `InvalidOperationException` or store-specific exceptions if the entity does not exist, on concurrency conflicts, or on connection failures.

### `public virtual Task<bool> DeleteAsync(object id)`
Removes an entity identified by its primary key.
- **Parameters**: `id` — the primary key of the entity to delete.
- **Returns**: `true` if the entity was found and deleted; `false` if no matching entity exists.
- **Throws**: May throw `ArgumentNullException` if `id` is `null`. Store-specific exceptions on connection failures.

### `public virtual Task<bool> DeleteAsync(T entity)`
Removes the given entity from the store directly.
- **Parameters**: `entity` — the entity instance to delete. Must not be `null`.
- **Returns**: `true` if the entity was successfully deleted; `false` if the entity was not tracked by the store.
- **Throws**: `ArgumentNullException` if `entity` is `null`. Store-specific exceptions on connection failures or if the entity is not in a valid state for deletion.

### `public virtual Task<bool> ExistsAsync(object id)`
Checks whether an entity with the given primary key exists.
- **Parameters**: `id` — the primary key to test.
- **Returns**: `true` if a matching record exists; `false` otherwise.
- **Throws**: May throw `ArgumentNullException` if `id` is `null`. Store-specific exceptions on connection failures.

### `public virtual Task<int> CountAsync()`
Returns the total number of entities of type `T` in the store.
- **Returns**: A non-negative integer count.
- **Throws**: Store-specific exceptions on connection or query failures.

### `public virtual Task<IEnumerable<T>> GetPageAsync(int page, int pageSize)`
Retrieves a paginated subset of entities.
- **Parameters**:
  - `page` — the 1-based page index. Must be >= 1.
  - `pageSize` — the number of items per page. Must be >= 1.
- **Returns**: An `IEnumerable<T>` for the requested page. May be empty if the page exceeds available data.
- **Throws**: `ArgumentOutOfRangeException` if `page` or `pageSize` is less than 1. Store-specific exceptions on connection or query failures.

### `public virtual Task<int> SaveChangesAsync()`
Flushes all pending changes tracked by the underlying unit of work or context to the store.
- **Returns**: The number of state entries written to the store.
- **Throws**: Store-specific exceptions on connection failures, concurrency conflicts, or constraint violations.

### `public virtual void Clear()`
Resets the internal change tracker, detaching all tracked entities without persisting any pending changes.
- **Side effects**: All unsaved modifications are discarded. The underlying store is not affected.

## Usage

### Example 1: Basic CRUD with a derived repository
```csharp
public class ImageRepository : GenericRepository<ProcessedImage>
{
    public ImageRepository(ImageDbContext context) : base(context) { }
}

// Usage
var repo = new ImageRepository(dbContext);

// Add
var image = await repo.AddAsync(new ProcessedImage { FileName = "result.png" });

// Read
var fetched = await repo.GetByIdAsync(image.Id);
var allImages = await repo.GetAllAsync();

// Update
fetched.FileName = "final.png";
await repo.UpdateAsync(fetched);
await repo.SaveChangesAsync();

// Delete
bool deleted = await repo.DeleteAsync(fetched.Id);
```

### Example 2: Paginated listing with existence check
```csharp
var repo = new ImageRepository(dbContext);

bool hasImages = await repo.ExistsAsync(someId);
int total = await repo.CountAsync();

const int pageSize = 20;
for (int page = 1; ; page++)
{
    var batch = await repo.GetPageAsync(page, pageSize);
    var list = batch.ToList();

    if (list.Count == 0) break;

    foreach (var img in list)
    {
        Console.WriteLine($"Processing {img.FileName}");
    }
}

// Discard accidental changes without persisting
repo.Clear();
```

## Notes

- **Thread safety**: This class is not guaranteed to be thread-safe. The underlying data context or session is typically scoped to a single unit of work and should not be shared concurrently across threads without external synchronization.
- **Virtual methods**: All members are `virtual`, allowing derived classes to override behavior for entity-specific validation, soft deletes, or custom query logic. The default implementations assume a standard ORM-backed store.
- **`DeleteAsync` overloads**: The two overloads serve different scenarios. The key-based overload avoids materializing an entity before deletion. The entity-based overload is useful when the instance is already tracked. Both return `false` when the target does not exist rather than throwing.
- **`Clear` vs. `SaveChangesAsync`**: `Clear` discards in-memory changes only; it does not roll back committed transactions. Calling `Clear` after `SaveChangesAsync` is redundant for persistence but can free memory by detaching tracked entities.
- **Pagination**: `GetPageAsync` uses 1-based indexing. Passing `page = 0` or `pageSize = 0` should throw `ArgumentOutOfRangeException` in conforming implementations.
- **`GetByIdAsync` returning `null`**: Callers must guard against `null` returns; the method does not throw when an entity is simply not found.
