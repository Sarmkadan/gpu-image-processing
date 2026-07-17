# ImageProcessingControllerExtensions

Provides fluent extension methods for interacting with the GPU image processing HTTP API, encapsulating request/response handling, error mapping, and asynchronous batch operations. All methods return `ApiResponse<T>` to uniformly represent success, validation failures, and server errors without throwing for expected HTTP status codes.

## API

### RegisterImagesAsync
```csharp
public static async Task<ApiResponse<List<ImageMetadata>>> RegisterImagesAsync(
    this IImageProcessingController controller,
    IEnumerable<RemoteImage> images,
    CancellationToken cancellationToken = default)
```
Registers a collection of remote images with the processing service, uploading each source and returning metadata including assigned identifiers and storage locations.

**Parameters**
- `controller`: The controller instance to extend.
- `images`: Remote image descriptors containing source URIs and optional metadata. Must not be null or empty.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<List<ImageMetadata>>` containing the registered image metadata on success; `ApiResponse.Failure` with validation errors if any image descriptor is invalid, or server errors if upload fails.

**Throws**
- `ArgumentNullException` if `controller` or `images` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

---

### ApplyFilterToImagesAsync
```csharp
public static async Task<ApiResponse<List<ProcessingResult>>> ApplyFilterToImagesAsync(
    this IImageProcessingController controller,
    IEnumerable<Guid> imageIds,
    FilterConfiguration filter,
    CancellationToken cancellationToken = default)
```
Submits a filter operation for the specified images, executing the configured filter pipeline on the GPU cluster.

**Parameters**
- `controller`: The controller instance to extend.
- `imageIds`: Identifiers of previously registered images. Must not be null or empty; all IDs must exist.
- `filter`: Filter configuration defining the kernel, parameters, and execution options. Must not be null.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<List<ProcessingResult>>` with per-image results including output locations and performance metrics on success; `ApiResponse.Failure` if any image ID is unknown, the filter configuration is invalid, or the GPU queue rejects the job.

**Throws**
- `ArgumentNullException` if `controller`, `imageIds`, or `filter` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

---

### ApplyTransformToImagesAsync
```csharp
public static async Task<ApiResponse<List<ProcessingResult>>> ApplyTransformToImagesAsync(
    this IImageProcessingController controller,
    IEnumerable<Guid> imageIds,
    TransformDefinition transform,
    CancellationToken cancellationToken = default)
```
Submits a geometric or color-space transform for the specified images.

**Parameters**
- `controller`: The controller instance to extend.
- `imageIds`: Identifiers of previously registered images. Must not be null or empty; all IDs must exist.
- `transform`: Transform definition describing the operation (e.g., resize, rotate, color conversion). Must not be null.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<List<ProcessingResult>>` with per-image results on success; `ApiResponse.Failure` if any image ID is unknown, the transform definition is invalid, or the GPU queue rejects the job.

**Throws**
- `ArgumentNullException` if `controller`, `imageIds`, or `transform` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

---

### GetImagesInfoAsync
```csharp
public static async Task<ApiResponse<List<ImageMetadata>>> GetImagesInfoAsync(
    this IImageProcessingController controller,
    IEnumerable<Guid> imageIds,
    CancellationToken cancellationToken = default)
```
Retrieves current metadata for the specified images, including storage status, dimensions, and processing history summary.

**Parameters**
- `controller`: The controller instance to extend.
- `imageIds`: Identifiers of images to query. Must not be null or empty.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<List<ImageMetadata>>` with metadata for each requested image on success; `ApiResponse.Failure` if any ID is unknown or the query fails.

**Throws**
- `ArgumentNullException` if `controller` or `imageIds` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

---

### GetProcessingResultsByImageAsync
```csharp
public static async Task<ApiResponse<Dictionary<Guid, List<ProcessingResult>>>> GetProcessingResultsByImageAsync(
    this IImageProcessingController controller,
    IEnumerable<Guid> imageIds,
    CancellationToken cancellationToken = default)
```
Retrieves the full processing history for each specified image, grouped by image identifier.

**Parameters**
- `controller`: The controller instance to extend.
- `imageIds`: Identifiers of images to query. Must not be null or empty.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<Dictionary<Guid, List<ProcessingResult>>>` mapping each image ID to its chronological list of processing results on success; `ApiResponse.Failure` if any ID is unknown or the query fails.

**Throws**
- `ArgumentNullException` if `controller` or `imageIds` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

---

### CancelBatchJobsAsync
```csharp
public static async Task<ApiResponse<List<string>>> CancelBatchJobsAsync(
    this IImageProcessingController controller,
    IEnumerable<Guid> jobIds,
    CancellationToken cancellationToken = default)
```
Requests cancellation of the specified batch jobs. Jobs already completed or cancelled are ignored.

**Parameters**
- `controller`: The controller instance to extend.
- `jobIds`: Batch job identifiers to cancel. Must not be null or empty.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<List<string>>` containing the IDs of jobs successfully cancelled on success; `ApiResponse.Failure` if the request is malformed or the server rejects the cancellation.

**Throws**
- `ArgumentNullException` if `controller` or `jobIds` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

---

### GetBatchJobsStatusAsync
```csharp
public static async Task<ApiResponse<Dictionary<Guid, BatchJobStatus>>> GetBatchJobsStatusAsync(
    this IImageProcessingController controller,
    IEnumerable<Guid> jobIds,
    CancellationToken cancellationToken = default)
```
Retrieves the current status of the specified batch jobs.

**Parameters**
- `controller`: The controller instance to extend.
- `jobIds`: Batch job identifiers to query. Must not be null or empty.
- `cancellationToken`: Token to cancel the underlying HTTP request.

**Returns**
`ApiResponse<Dictionary<Guid, BatchJobStatus>>` mapping each job ID to its current status on success; `ApiResponse.Failure` if any ID is unknown or the query fails.

**Throws**
- `ArgumentNullException` if `controller` or `jobIds` is null.
- `OperationCanceledException` if `cancellationToken` is triggered before completion.

## Usage

### Register images and apply a filter pipeline
```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://api.gpu-processing.example") };
var controller = client.CreateImageProcessingController();

var images = new[]
{
    new RemoteImage(new Uri("https://cdn.example.com/input/photo1.raw"), "photo1"),
    new RemoteImage(new Uri("https://cdn.example.com/input/photo2.raw"), "photo2")
};

var registerResponse = await controller.RegisterImagesAsync(images);
if (!registerResponse.IsSuccess)
{
    logger.LogError("Registration failed: {Errors}", registerResponse.Errors);
    return;
}

var imageIds = registerResponse.Value.Select(m => m.Id).ToList();

var filter = new FilterConfiguration
{
    Kernel = "sharpen-3x3",
    Parameters = new Dictionary<string, object> { ["strength"] = 0.8f },
    ExecutionMode = ExecutionMode.GpuPreferred
};

var filterResponse = await controller.ApplyFilterToImagesAsync(imageIds, filter);
if (filterResponse.IsSuccess)
{
    foreach (var result in filterResponse.Value)
    {
        logger.LogInformation("Image {ImageId} processed -> {OutputUri}", result.ImageId, result.OutputUri);
    }
}
else
{
    logger.LogError("Filter application failed: {Errors}", filterResponse.Errors);
}
```

### Monitor batch jobs and cancel long-running work
```csharp
var transform = new TransformDefinition
{
    Operation = TransformOperation.Resize,
    Parameters = new Dictionary<string, object> { ["width"] = 1920, ["height"] = 1080, ["mode"] = "lanczos" }
};

var transformResponse = await controller.ApplyTransformToImagesAsync(imageIds, transform);
if (!transformResponse.IsSuccess)
{
    logger.LogError("Transform submission failed: {Errors}", transformResponse.Errors);
    return;
}

var jobIds = transformResponse.Value.Select(r => r.BatchJobId).Distinct().ToList();

// Poll until all jobs reach terminal state
var terminalStates = new[] { BatchJobStatus.Completed, BatchJobStatus.Failed, BatchJobStatus.Cancelled };
while (true)
{
    var statusResponse = await controller.GetBatchJobsStatusAsync(jobIds);
    if (!statusResponse.IsSuccess)
    {
        logger.LogWarning("Status query failed: {Errors}", statusResponse.Errors);
        break;
    }

    var allTerminal = statusResponse.Value.Values.All(s => terminalStates.Contains(s));
    if (allTerminal) break;

    await Task.Delay(TimeSpan.FromSeconds(5));
}

// Cancel any job still running after timeout
var runningJobs = statusResponse.Value
    .Where(kvp => kvp.Value == BatchJobStatus.Running || kvp.Value == BatchJobStatus.Queued)
    .Select(kvp => kvp.Key)
    .ToList();

if (runningJobs.Any())
{
    var cancelResponse = await controller.CancelBatchJobsAsync(runningJobs);
    logger.LogInformation("Cancelled {Count} jobs", cancelResponse.Value?.Count ?? 0);
}
```

## Notes

- **Idempotency**: `RegisterImagesAsync` is not idempotent; repeated calls with the same `RemoteImage` instances create duplicate entries. Callers should deduplicate before registration.
- **Partial failure**: Batch operations (`ApplyFilterToImagesAsync`, `ApplyTransformToImagesAsync`) succeed or fail as a unit. If any image ID is invalid or the GPU queue rejects the batch, the entire request fails with no partial results.
- **Cancellation**: All methods honor `CancellationToken` for the outbound HTTP request only. Server-side job cancellation requires `CancelBatchJobsAsync`; in-flight GPU kernels may not terminate immediately.
- **Thread safety**: The extension methods are stateless and safe for concurrent use across threads. The underlying `IImageProcessingController` implementation (typically `HttpImageClient`) must be thread-safe; the provided `HttpClient`-based implementation is.
- **Large payloads**: `RegisterImagesAsync` streams each remote image; memory usage scales with concurrent uploads, not total payload size. Prefer `RemoteImage` with pre-signed URLs for large datasets.
- **Result ordering**: `GetProcessingResultsByImageAsync` returns results in chronological order per image. The dictionary keys match the input `imageIds` order only if the server preserves it; do not rely on dictionary iteration order.
- **Error handling**: Always check `ApiResponse.IsSuccess` before accessing `Value`. `Errors` contains structured error codes (`ValidationError`, `NotFound`, `GpuQueueFull`, etc.) suitable for programmatic retry logic.
