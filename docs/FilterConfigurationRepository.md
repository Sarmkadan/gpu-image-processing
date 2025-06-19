# FilterConfigurationRepository

`FilterConfigurationRepository` provides asynchronous persistence and query operations for `FilterConfiguration` entities within the GPU image processing pipeline. It abstracts the underlying data store, offering methods to retrieve, create, update, delete, and query filter configurations by various criteria such as type, name, parameters, active status, and kernel presence. All operations return `Task`-based results, making the repository suitable for non-blocking I/O contexts.

## API

### GetByIdAsync

```csharp
public Task<FilterConfiguration?> GetByIdAsync(/* identifier */)
```

Retrieves a single `FilterConfiguration` by its unique identifier. Returns `null` if no matching entity exists. The identifier type and parameter name are determined by the implementation.

### GetAllAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetAllAsync()
```

Returns all `FilterConfiguration` entities in the store without filtering. The result is an in-memory collection; callers should be mindful of potential size when the dataset is large.

### GetByCriteriaAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetByCriteriaAsync(/* criteria */)
```

Queries the store using a criteria object or predicate. The exact parameter shape is implementation-specific. Returns zero or more matching configurations. Throws if the criteria argument is malformed or null (implementation-dependent).

### CreateAsync

```csharp
public Task<FilterConfiguration> CreateAsync(FilterConfiguration configuration)
```

Persists a new `FilterConfiguration`. The returned entity reflects the post-persistence state, including any store-generated fields (e.g., identifier, timestamps). Throws if the configuration is null, violates a uniqueness constraint, or fails validation.

### UpdateAsync

```csharp
public Task<FilterConfiguration> UpdateAsync(FilterConfiguration configuration)
```

Updates an existing `FilterConfiguration`. The entity must already exist in the store; the returned object contains the updated state. Throws if the configuration is null, references a non-existent entity, or violates constraints.

### DeleteAsync

```csharp
public Task<bool> DeleteAsync(/* identifier */)
```

Removes a `FilterConfiguration` by its identifier. Returns `true` if the entity was found and deleted; returns `false` if no matching entity existed. Throws if the underlying store encounters an unrecoverable error.

### ExistsAsync

```csharp
public Task<bool> ExistsAsync(/* identifier */)
```

Checks whether a `FilterConfiguration` with the given identifier exists. Returns `true` if present, `false` otherwise. Does not throw for missing entities.

### CountAsync

```csharp
public Task<int> CountAsync()
```

Returns the total number of `FilterConfiguration` entities in the store. May accept optional filtering criteria in the implementation.

### GetPagedAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetPagedAsync(/* offset, limit */)
```

Retrieves a paginated subset of `FilterConfiguration` entities. Parameters typically include an offset (or page number) and a page size. Returns the requested page of results. Throws if pagination parameters are negative or out of range.

### GetByTypeAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetByTypeAsync(/* filterType */)
```

Returns all `FilterConfiguration` entities that match a specific filter type (e.g., convolution, morphological, color). The type parameter is implementation-defined. Returns an empty collection if no matches exist.

### GetActiveFiltersAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetActiveFiltersAsync()
```

Returns only those `FilterConfiguration` entities marked as active. The definition of "active" is determined by a property on the entity (e.g., an `IsActive` flag or status field). Returns an empty collection if no active filters exist.

### GetByNameAsync

```csharp
public Task<FilterConfiguration?> GetByNameAsync(string name)
```

Looks up a single `FilterConfiguration` by its unique name. Returns `null` if no match is found. Throws if the name argument is null or empty, depending on implementation. Names are assumed to be case-sensitive unless documented otherwise by the implementation.

### GetByParameterAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetByParameterAsync(/* parameterKey, parameterValue */)
```

Queries for `FilterConfiguration` entities that contain a specific parameter key-value pair. The parameter signature is implementation-specific (e.g., a dictionary entry, a key and value pair). Returns an empty collection if no configurations match.

### GetFiltersWithKernelAsync

```csharp
public Task<IEnumerable<FilterConfiguration>> GetFiltersWithKernelAsync()
```

Returns all `FilterConfiguration` entities that have a defined convolution kernel or matrix. Filters without a kernel are excluded. Returns an empty collection if no such configurations exist.

## Usage

### Example 1: Creating and retrieving an active filter by name

```csharp
var repo = new FilterConfigurationRepository(/* dependencies */);

var newFilter = new FilterConfiguration
{
    Name = "GaussianBlur3x3",
    Type = FilterType.Convolution,
    IsActive = true,
    Kernel = new float[,] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } }
};

FilterConfiguration created = await repo.CreateAsync(newFilter);

FilterConfiguration? retrieved = await repo.GetByNameAsync("GaussianBlur3x3");
if (retrieved is not null)
{
    Console.WriteLine($"Filter '{retrieved.Name}' is active: {retrieved.IsActive}");
}
```

### Example 2: Paginated listing of convolution filters with kernels

```csharp
var repo = new FilterConfigurationRepository(/* dependencies */);

IEnumerable<FilterConfiguration> convolutionFilters = await repo.GetByTypeAsync(FilterType.Convolution);
IEnumerable<FilterConfiguration> kernelFilters = await repo.GetFiltersWithKernelAsync();

// Intersect and paginate manually, or use a combined criteria method if available
int pageSize = 10;
int page = 0;
IEnumerable<FilterConfiguration> pageOfFilters = await repo.GetPagedAsync(page * pageSize, pageSize);

foreach (var filter in pageOfFilters)
{
    bool exists = await repo.ExistsAsync(filter.Id);
    Console.WriteLine($"Filter {filter.Name} exists in store: {exists}");
}

int totalActive = (await repo.GetActiveFiltersAsync()).Count();
Console.WriteLine($"Total active filters: {totalActive}");
```

## Notes

- **Null returns**: `GetByIdAsync` and `GetByNameAsync` return `null` when no match is found. Callers must null-check before accessing members of the returned entity.
- **Empty collections**: Methods returning `IEnumerable<FilterConfiguration>` (e.g., `GetByTypeAsync`, `GetActiveFiltersAsync`, `GetByParameterAsync`, `GetFiltersWithKernelAsync`) return empty collections, not `null`, when no entities satisfy the criteria.
- **DeleteAsync return value**: A `false` return indicates the entity was not found; it does not necessarily indicate a store failure. Callers should distinguish between not-found and exceptions.
- **Uniqueness constraints**: `CreateAsync` and `UpdateAsync` may throw if a name or identifier conflict is detected. The exact exception type depends on the implementation and underlying store.
- **Thread safety**: The repository itself is not guaranteed to be thread-safe. Concurrent calls to `CreateAsync`, `UpdateAsync`, or `DeleteAsync` targeting the same entity may result in race conditions or optimistic concurrency failures. External synchronization is recommended when multiple threads operate on overlapping data.
- **Pagination boundaries**: `GetPagedAsync` may throw if the offset exceeds the total count or if limit is non-positive. Callers should validate pagination parameters or use `CountAsync` to compute valid ranges.
- **Parameter matching semantics**: `GetByParameterAsync` matching logic (exact match, partial match, case sensitivity) is implementation-defined. Consult the concrete implementation for details.
