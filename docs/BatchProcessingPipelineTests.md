# BatchProcessingPipelineTests

Unit tests for the `BatchProcessingPipeline` class, verifying correct behavior under success, failure, and edge-case scenarios including null inputs, retries, progress reporting, and output directory creation.

## API

### `BatchProcessingPipelineTests`
Public test class that contains all unit tests for the `BatchProcessingPipeline` class. This class uses xUnit and Moq to mock dependencies and verify pipeline behavior under various conditions.

### `RunAsync_NullBatch_ThrowsArgumentNullException`
Verifies that passing a `null` batch to `BatchProcessingPipeline.RunAsync` results in an `ArgumentNullException`. This ensures the pipeline validates input parameters before processing.

### `RunAsync_InvalidBatch_ThrowsProcessingException`
Ensures that when an invalid batch (e.g., with malformed image references or invalid metadata) is processed, the pipeline throws a `ProcessingException` rather than continuing with invalid data.

### `RunAsync_AllImagesSucceed_ReturnsFullSuccessResult`
Validates that when all images in a batch are processed successfully, the pipeline returns a result indicating full success, including correct counts of processed, succeeded, and failed images.

### `RunAsync_AllImagesFail_ReturnsFullFailureResult`
Confirms that when every image in a batch fails processing, the pipeline returns a result indicating full failure, with accurate counts and no successful images.

### `RunAsync_PartialFailure_ReturnsCorrectCounts`
Checks that when some images succeed and others fail, the pipeline returns a result with correct partial success/failure counts, reflecting the actual outcome of each image.

### `RunAsync_RaisesProgressChangedForEachImage`
Ensures that the pipeline raises a `ProgressChanged` event for each image processed, allowing consumers to track progress in real time. The event should fire in the correct order and include accurate status updates.

### `RunAsync_CreatesOutputDirectory`
Verifies that the pipeline creates the specified output directory if it does not exist, ensuring that processed images can be saved without runtime errors due to missing directories.

### `RunAsync_CompletedBatchHasCorrectStatus`
Ensures that after processing completes, the returned `BatchProcessingResult` reflects the correct overall status (e.g., `Completed`, `CompletedWithErrors`) based on the success or failure of individual images.

### `RunAsync_RetriesFailedImageUpToMaxRetries`
Validates that the pipeline retries failed images up to the configured maximum retry count before marking them as failed, ensuring transient errors are handled appropriately.

### `Constructor_NullProcessingService_ThrowsArgumentNullException`
Ensures that the `BatchProcessingPipeline` constructor throws an `ArgumentNullException` when a `null` `IProcessingService` is provided, enforcing dependency injection safety.

### `Constructor_NullOptions_ThrowsArgumentNullException`
Ensures that the `BatchProcessingPipeline` constructor throws an `ArgumentNullException` when `null` options are provided, preventing misconfiguration and runtime failures.

## Usage
