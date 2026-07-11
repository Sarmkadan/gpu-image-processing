# ImageProcessingController

The `ImageProcessingController` is an ASP.NET Core controller designed to handle image processing requests, including registration, filtering, transformation, batch job management, and metadata retrieval. It provides endpoints for uploading images, applying GPU-accelerated filters and transforms, querying processing results, and managing asynchronous batch jobs.

## API

### `ImageProcessingController`
Initializes a new instance of the `ImageProcessingController` class.

### `async Task<ApiResponse<ImageMetadata>> RegisterImageAsync`
Registers an image for processing.

- **Parameters**: None (image data is provided via request body).
- **Return Value**: An `ApiResponse<ImageMetadata>` containing metadata about the registered image (e.g., ID, dimensions, file size).
- **Exceptions**: Throws if the image data is invalid or processing fails.

### `async Task<ApiResponse<ProcessingResult>> ApplyFilterAsync`
Applies a specified filter to the registered image.

- **Parameters**:
  - `filterName` (string): The name of the filter to apply.
  - `imageId` (Guid): The unique identifier of the registered image.
- **Return Value**: An `ApiResponse<ProcessingResult>` containing the processed image data and metadata.
- **Exceptions**: Throws if the filter is unsupported, the image ID is invalid, or processing fails.

### `async Task<ApiResponse<ProcessingResult>> ApplyTransformAsync`
Applies a specified geometric transformation to the registered image.

- **Parameters**:
  - `transformName` (string): The name of the transformation to apply.
  - `imageId` (Guid): The unique identifier of the registered image.
- **Return Value**: An `ApiResponse<ProcessingResult>` containing the transformed image data and metadata.
- **Exceptions**: Throws if the transform is unsupported, the image ID is invalid, or processing fails.

### `async Task<ApiResponse<ImageMetadata>> GetImageInfoAsync`
Retrieves metadata for a registered image.

- **Parameters**:
  - `imageId` (Guid): The unique identifier of the image.
- **Return Value**: An `ApiResponse<ImageMetadata>` containing metadata such as dimensions, file size, and registration timestamp.
- **Exceptions**: Throws if the image ID is invalid.

### `async Task<ApiResponse<BatchJobMetadata>> CreateBatchJobAsync`
Creates a new batch job for processing multiple images.

- **Parameters**:
  - `jobName` (string): A human-readable name for the batch job.
  - `imageIds` (IEnumerable<Guid>): The unique identifiers of the images to process.
  - `filterNames` (IEnumerable<string>): The names of the filters to apply.
- **Return Value**: An `ApiResponse<BatchJobMetadata>` containing metadata about the created batch job (e.g., ID, status, creation timestamp).
- **Exceptions**: Throws if the job name is invalid or the image IDs are invalid.

### `async Task<ApiResponse<BatchJobStatus>> GetBatchJobStatusAsync`
Retrieves the current status of a batch job.

- **Parameters**:
  - `jobId` (Guid): The unique identifier of the batch job.
- **Return Value**: An `ApiResponse<BatchJobStatus>` containing the job's status (e.g., pending, completed, failed) and progress.
- **Exceptions**: Throws if the job ID is invalid.

### `async Task<ApiResponse<string>> CancelBatchJobAsync`
Cancels a pending or in-progress batch job.

- **Parameters**:
  - `jobId` (Guid): The unique identifier of the batch job.
- **Return Value**: An `ApiResponse<string>` containing a confirmation message upon successful cancellation.
- **Exceptions**: Throws if the job ID is invalid or the job cannot be canceled.

### `async Task<ApiResponse<List<FilterInfo>>> ListFiltersAsync`
Retrieves a list of available filters that can be applied to images.

- **Parameters**: None.
- **Return Value**: An `ApiResponse<List<FilterInfo>>` containing metadata about each available filter (e.g., name, description, parameters).
- **Exceptions**: Throws if the filter list cannot be retrieved.

### `async Task<ApiResponse<List<ProcessingResult>>> GetProcessingResultsAsync`
Retrieves the results of previously processed images or batch jobs.

- **Parameters**:
  - `imageId` (Guid, optional): The unique identifier of a specific image. If omitted, returns all results.
- **Return Value**: An `ApiResponse<List<ProcessingResult>>` containing the processed image data and metadata.
- **Exceptions**: Throws if the image ID is invalid (when provided).

## Usage

### Example 1: Register and Apply a Filter
