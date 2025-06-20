# ProcessingResultRepository

`ProcessingResultRepository` provides asynchronous data-access operations for `ProcessingResult` entities within the GPU image processing pipeline. It abstracts persistence concerns, offering methods to retrieve, create, update, delete, and query processing results by various criteria such as image identifier, processing status, time ranges, and performance metrics. All operations return `Task`-based results, making the repository suitable for non-blocking I/O contexts.

## API

### GetByIdAsync

```csharp
public Task<ProcessingResult?> GetByIdAsync(string id)
```

Retrieves a single `ProcessingResult` by its unique identifier.

**Parameters:**
- `id` (`string`): The unique identifier of the processing result.

**Returns:**
- A `Task` that resolves to the matching `ProcessingResult` instance, or `null` if no result with the given identifier exists.

**Exceptions:**
- `ArgumentNullException` when `id` is `null`.
- `ArgumentException` when `id` is empty or consists only of whitespace.

---

### GetAllAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetAllAsync()
```

Returns all `ProcessingResult` entities stored in the repository without filtering.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` containing every persisted result. The collection may be empty if no results exist.

---

### GetByCriteriaAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetByCriteriaAsync(Expression<Func<ProcessingResult, bool>> predicate)
```

Retrieves all `ProcessingResult` entities that satisfy a caller-supplied predicate.

**Parameters:**
- `predicate` (`Expression<Func<ProcessingResult, bool>>`): A LINQ-compatible expression defining the filter criteria.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` of matching entities. Returns an empty collection when no entities match.

**Exceptions:**
- `ArgumentNullException` when `predicate` is `null`.

---

### CreateAsync

```csharp
public Task<ProcessingResult> CreateAsync(ProcessingResult entity)
```

Persists a new `ProcessingResult` and returns the stored representation.

**Parameters:**
- `entity` (`ProcessingResult`): The processing result to create. The identifier may be assigned by the repository if not already set.

**Returns:**
- A `Task` that resolves to the created `ProcessingResult`, reflecting any server-generated values (e.g., timestamps, assigned identifier).

**Exceptions:**
- `ArgumentNullException` when `entity` is `null`.
- `InvalidOperationException` when an entity with the same identifier already exists.

---

### UpdateAsync

```csharp
public Task<ProcessingResult> UpdateAsync(ProcessingResult entity)
```

Updates an existing `ProcessingResult` with the supplied data.

**Parameters:**
- `entity` (`ProcessingResult`): The processing result containing updated field values. The identifier must match an existing entity.

**Returns:**
- A `Task` that resolves to the updated `ProcessingResult` as persisted.

**Exceptions:**
- `ArgumentNullException` when `entity` is `null`.
- `KeyNotFoundException` when no entity with the given identifier exists.

---

### DeleteAsync

```csharp
public Task<bool> DeleteAsync(string id)
```

Removes a `ProcessingResult` from the repository by its identifier.

**Parameters:**
- `id` (`string`): The unique identifier of the processing result to delete.

**Returns:**
- A `Task` that resolves to `true` if the entity was found and deleted; `false` if no matching entity existed.

**Exceptions:**
- `ArgumentNullException` when `id` is `null`.
- `ArgumentException` when `id` is empty or consists only of whitespace.

---

### ExistsAsync

```csharp
public Task<bool> ExistsAsync(string id)
```

Checks whether a `ProcessingResult` with the specified identifier exists.

**Parameters:**
- `id` (`string`): The unique identifier to test.

**Returns:**
- A `Task` that resolves to `true` if an entity with the given identifier is present; otherwise `false`.

**Exceptions:**
- `ArgumentNullException` when `id` is `null`.
- `ArgumentException` when `id` is empty or consists only of whitespace.

---

### CountAsync

```csharp
public Task<int> CountAsync()
```

Returns the total number of `ProcessingResult` entities in the repository.

**Returns:**
- A `Task` that resolves to the count as an `int`. Returns `0` when the repository is empty.

---

### GetPagedAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetPagedAsync(int page, int pageSize)
```

Retrieves a paginated subset of `ProcessingResult` entities.

**Parameters:**
- `page` (`int`): The one-based page index to retrieve.
- `pageSize` (`int`): The maximum number of entities per page.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` for the requested page. Returns an empty collection when the page exceeds available data.

**Exceptions:**
- `ArgumentOutOfRangeException` when `page` is less than 1 or `pageSize` is less than 1.

---

### GetByImageIdAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetByImageIdAsync(string imageId)
```

Retrieves all `ProcessingResult` entities associated with a specific source image.

**Parameters:**
- `imageId` (`string`): The identifier of the image whose processing results are requested.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` for the given image. Returns an empty collection when no results are associated with the image.

**Exceptions:**
- `ArgumentNullException` when `imageId` is `null`.
- `ArgumentException` when `imageId` is empty or consists only of whitespace.

---

### GetByStatusAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetByStatusAsync(ProcessingStatus status)
```

Retrieves all `ProcessingResult` entities with a specific processing status.

**Parameters:**
- `status` (`ProcessingStatus`): The status enumeration value to filter by.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` of entities matching the status. Returns an empty collection when none match.

---

### GetSuccessfulResultsAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetSuccessfulResultsAsync()
```

Retrieves all `ProcessingResult` entities that completed successfully.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` of successful results. Returns an empty collection when no successful results exist.

---

### GetFailedResultsAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetFailedResultsAsync()
```

Retrieves all `ProcessingResult` entities that ended in failure.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` of failed results. Returns an empty collection when no failed results exist.

---

### GetCompletedBetweenAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetCompletedBetweenAsync(DateTime start, DateTime end)
```

Retrieves all `ProcessingResult` entities whose completion timestamp falls within a specified inclusive range.

**Parameters:**
- `start` (`DateTime`): The inclusive lower bound of the completion time range.
- `end` (`DateTime`): The inclusive upper bound of the completion time range.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` of entities completed within the range. Returns an empty collection when none fall within the bounds.

**Exceptions:**
- `ArgumentException` when `start` is later than `end`.

---

### GetSlowestResultsAsync

```csharp
public Task<IEnumerable<ProcessingResult>> GetSlowestResultsAsync(int count)
```

Retrieves the top N `ProcessingResult` entities ordered by longest processing duration.

**Parameters:**
- `count` (`int`): The maximum number of slowest results to return.

**Returns:**
- A `Task` that resolves to an `IEnumerable<ProcessingResult>` containing up to `count` entities, sorted descending by processing time. Returns fewer than `count` when the repository contains insufficient entities.

**Exceptions:**
- `ArgumentOutOfRangeException` when `count` is less than 1.

---

### GetAverageProcessingTimeAsync

```csharp
public Task<double> GetAverageProcessingTimeAsync()
```

Computes the arithmetic mean of processing durations across all completed `ProcessingResult` entities.

**Returns:**
- A `Task` that resolves to the average processing time as a `double`. Returns `0.0` when no completed results with measurable durations exist.

## Usage

### Example 1: Creating and querying results for a specific image

```csharp
var repository = serviceProvider.GetRequiredService<ProcessingResultRepository>();

// Create a new processing result
var newResult = new ProcessingResult
{
    ImageId = "img-42",
    Status = ProcessingStatus.Completed,
    ProcessingTimeMs = 1250.5,
    CompletedAt = DateTime.UtcNow
};
var created = await repository.CreateAsync(newResult);

// Later, retrieve all results for that image
var imageResults = await repository.GetByImageIdAsync("img-42");
foreach (var result in imageResults)
{
    Console.WriteLine($"Result {result.Id}: {result.Status}, {result.ProcessingTimeMs} ms");
}
```

### Example 2: Performance monitoring with pagination and averages

```csharp
var repository = serviceProvider.GetRequiredService<ProcessingResultRepository>();

// Get the slowest 5 processing results for investigation
var slowest = await repository.GetSlowestResultsAsync(5);
foreach (var result in slowest)
{
    Console.WriteLine($"Slow result: {result.Id} took {result.ProcessingTimeMs} ms");
}

// Compute overall average processing time
var average = await repository.GetAverageProcessingTimeAsync();
Console.WriteLine($"Average processing time: {average:F2} ms");

// Paginate through all results for a dashboard
int page = 1;
IEnumerable<ProcessingResult> pageResults;
do
{
    pageResults = await repository.GetPagedAsync(page, 20);
    foreach (var result in pageResults)
    {
        // Render result row
    }
    page++;
} while (pageResults.Any());
```

## Notes

- All methods are asynchronous and should be awaited. Calling them without awaiting will result in unobserved `Task` instances and may lead to incomplete operations or resource leaks.
- Methods returning `IEnumerable<ProcessingResult>` may stream results or materialize them into an in-memory collection depending on the underlying implementation. Callers should not assume deferred execution semantics across an `await` boundary unless explicitly documented by the concrete implementation.
- `GetByIdAsync`, `DeleteAsync`, and `ExistsAsync` validate the `id` parameter for null and whitespace. Passing a string that is technically non-empty but contains only whitespace characters will trigger an `ArgumentException`.
- `GetCompletedBetweenAsync` treats the `start` and `end` parameters as inclusive bounds. Results with a `CompletedAt` value exactly equal to `start` or `end` are included in the returned set.
- `GetAverageProcessingTimeAsync` considers only results that have a measurable processing time. Results with a status that does not imply completion (e.g., pending or cancelled) are excluded from the calculation. When no qualifying results exist, the method returns `0.0` rather than throwing.
- Thread safety is not guaranteed at the repository level. Concurrent calls to `CreateAsync`, `UpdateAsync`, or `DeleteAsync` targeting the same entity may result in race conditions (e.g., duplicate key violations, lost updates, or stale state). Callers must coordinate such operations externally if concurrent access is expected.
- The repository does not expose transactional boundaries in its public API. Multi-step workflows that require atomicity across several method calls should be wrapped in an ambient transaction or unit-of-work mechanism provided by the underlying data layer.
