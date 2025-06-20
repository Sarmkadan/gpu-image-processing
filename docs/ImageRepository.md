# ImageRepository
The `ImageRepository` provides asynchronous access to image records stored in the GPU image processing system, enabling creation, retrieval, updating, deletion, and querying of images based on various criteria.

## API
### `Task<Image?> GetByIdAsync(object id, CancellationToken cancellationToken = default)`
Retrieves the image with the specified identifier.  
- **Parameters**  
  - `id`: The unique identifier of the image (type depends on the underlying store, commonly `Guid` or `string`).  
  - `cancellationToken`: Optional token to cancel the operation.  
- **Return Value**  
  - A task that resolves to the `Image` instance if found, otherwise `null`.  
- **Exceptions**  
  - `OperationCanceledException` if the token is triggered.  
  - `ArgumentException` if `id` is invalid.  
  - `IOException` for underlying storage failures.

### `Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default)`
Returns all images in the repository.  
- **Parameters**  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable collection of `Image` objects.  
- **Exceptions**  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage errors.

### `Task<IEnumerable<Image>> GetByCriteriaAsync(ImageCriteria criteria, CancellationToken cancellationToken = default)`
Fetches images that match the supplied criteria object.  
- **Parameters**  
  - `criteria`: An instance defining filter conditions (e.g., status, format, date range). Must not be `null`.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects matching the criteria.  
- **Exceptions**  
  - `ArgumentNullException` if `criteria` is `null`.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage issues.

### `Task<Image> CreateAsync(Image image, CancellationToken cancellationToken = default)`
Inserts a new image record.  
- **Parameters**  
  - `image`: The `Image` to add; must not be `null` and must have a valid, non‑persisted identifier.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to the created `Image`, typically with its identifier populated.  
- **Exceptions**  
  - `ArgumentNullException` if `image` is `null`.  
  - `InvalidOperationException` if an image with the same identifier already exists.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage failure.

### `Task<Image> UpdateAsync(Image image, CancellationToken cancellationToken = default)`
Updates an existing image record.  
- **Parameters**  
  - `image`: The `Image` with modified values; must not be `null` and must represent an existing record.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to the updated `Image` instance.  
- **Exceptions**  
  - `ArgumentNullException` if `image` is `null`.  
  - `KeyNotFoundException` if no image with the given identifier exists.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage errors.

### `Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default)`
Removes the image with the specified identifier.  
- **Parameters**  
  - `id`: The identifier of the image to delete.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to `true` if the image was deleted, `false` if it was not found.  
- **Exceptions**  
  - `ArgumentException` if `id` is invalid.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage failure.

### `Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default)`
Checks whether an image with the given identifier exists.  
- **Parameters**  
  - `id`: The identifier to test.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to `true` if a matching image exists, otherwise `false`.  
- **Exceptions**  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage errors.

### `Task<int> CountAsync(CancellationToken cancellationToken = default)`
Returns the total number of images in the repository.  
- **Parameters**  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to the count of images as an `int`.  
- **Exceptions**  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage failure.

### `Task<IEnumerable<Image>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)`
Retrieves a slice of images for paging scenarios.  
- **Parameters**  
  - `pageIndex`: Zero‑based index of the page to return. Must be ≥ 0.  
  - `pageSize`: Maximum number of items per page. Must be > 0.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects for the requested page.  
- **Exceptions**  
  - `ArgumentOutOfRangeException` if `pageIndex` < 0 or `pageSize` ≤ 0.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage errors.

### `Task<IEnumerable<Image>> GetByStatusAsync(ImageStatus status, CancellationToken cancellationToken = default)`
Returns all images whose processing status matches the supplied value.  
- **Parameters**  
  - `status`: The `ImageStatus` to filter by.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects with the given status.  
- **Exceptions**  
  - `ArgumentException` if `status` is undefined.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage failure.

### `Task<IEnumerable<Image>> GetByFormatAsync(ImageFormat format, CancellationToken cancellationToken = default)`
Returns images stored in the specified file format.  
- **Parameters**  
  - `format`: The `ImageFormat` (e.g., PNG, JPEG) to filter by.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects matching the format.  
- **Exceptions**  
  - `ArgumentException` if `format` is not supported.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage errors.

### `Task<IEnumerable<Image>> GetBySizeRangeAsync(int minWidth, int maxWidth, int minHeight, int maxHeight, CancellationToken cancellationToken = default)`
Returns images whose dimensions fall within the supplied width and height ranges.  
- **Parameters**  
  - `minWidth`: Inclusive minimum width.  
  - `maxWidth`: Inclusive maximum width; must be ≥ `minWidth`.  
  - `minHeight`: Inclusive minimum height.  
  - `maxHeight`: Inclusive maximum height; must be ≥ `minHeight`.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects satisfying the size constraints.  
- **Exceptions**  
  - `ArgumentOutOfRangeException` if any minimum exceeds its corresponding maximum.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage failure.

### `Task<IEnumerable<Image>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)`
Returns images whose creation or last‑modified timestamp lies within the supplied date interval.  
- **Parameters**  
  - `startDate`: Inclusive start of the range.  
  - `endDate`: Inclusive end of the range; must be ≥ `startDate`.  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects within the date range.  
- **Exceptions**  
  - `ArgumentException` if `startDate` is after `endDate`.  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage errors.

### `Task<IEnumerable<Image>> GetFailedImagesAsync(CancellationToken cancellationToken = default)`
Returns all images that have entered a failed processing state.  
- **Parameters**  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects marked as failed.  
- **Exceptions**  
  - `OperationCanceledException` on cancellation.  
  - `IOException` on storage failure.

### `Task<IEnumerable<Image>> GetPendingImagesAsync(CancellationToken cancellationToken = default)`
Returns all images awaiting processing.  
- **Parameters**  
  - `cancellationToken`: Optional cancellation token.  
- **Return Value**  
  - A task that resolves to an enumerable of `Image` objects with a pending status.  
- **Exceptions**  
  - `OperationCanceledException` on cancellation.  
  - `IOException` for storage errors.

## Usage
```csharp
// Example 1: Process pending images
var pendingImages = await imageRepository.GetPendingImagesAsync();
foreach (var img in pendingImages)
{
    try
    {
        await ProcessImageAsync(img);
        // Mark as succeeded (assuming UpdateAsync flips status)
        await imageRepository.UpdateAsync(img with { Status = ImageStatus.Processed });
    }
    catch (Exception ex)
    {
        // Log and mark as failed
        img = img with { Status = ImageStatus.Failed, ErrorMessage = ex.Message };
        await imageRepository.UpdateAsync(img);
    }
}
```

```csharp
// Example 2: Add a new thumbnail and retrieve it by ID
var newImage = new Image
{
    Format = ImageFormat.Png,
    Width = 256,
    Height = 256,
    Status = ImageStatus.Pending
};

var created = await imageRepository.CreateAsync(newImage);
// created.Id now contains the generated identifier

var fetched = await imageRepository.GetByIdAsync(created.Id);
if (fetched is not null)
{
    Console.WriteLine($"Image {fetched.Id} loaded with status {fetched.Status}");
}
```

## Notes
- All methods are asynchronous and accept an optional `CancellationToken`; callers should propagate cancellation tokens to ensure responsive behavior.  
- The repository does not enforce locking; concurrent calls are safe only if the underlying storage provider is thread‑safe. If the provider is not, external synchronization is required.  
- Enumerables returned by methods such as `GetAllAsync` or `GetByCriteriaAsync` represent a snapshot at the time of query; modifications to the repository after enumeration begins are not reflected in the already‑returned sequence.  
- Methods that may return `null` (`GetByIdAsync`) or `false` (`DeleteAsync`, `ExistsAsync`) should be checked by callers to avoid assuming success.  
- Invalid arguments (e.g., null identifiers, out‑of‑range page sizes) result in `ArgumentException` or derived exceptions before any I/O operation is attempted.  
- Storage‑related failures (disk errors, database connectivity) surface as `IOException`; callers may wish to treat these as transient and retry where appropriate.  
- The repository does not automatically dispose of resources; any `IDisposable` dependencies (e.g., database connections) should be managed by the containing scope or DI container.
