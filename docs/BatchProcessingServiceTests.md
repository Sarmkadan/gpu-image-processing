# BatchProcessingServiceTests

Unit test suite for the `BatchProcessingService` class, validating batch creation, processing, status tracking, cancellation, and progress reporting functionality. Tests cover both success and failure scenarios including null/invalid inputs, partial failures, cancellation, and edge cases around batch lifecycle management.

## API

### `BatchProcessingServiceTests`
Constructor for the test fixture. Initializes the in-memory batch store and service under test with default configuration.

### `ProcessBatchAsync_NullBatch_ThrowsArgumentNullException`
Validates that passing a null batch to `ProcessBatchAsync` results in an `ArgumentNullException`. Ensures the service rejects invalid inputs early.

### `ProcessBatchAsync_InvalidBatch_ThrowsProcessingException`
Verifies that processing a batch containing invalid image or filter references throws a `ProcessingException`. Confirms the service fails fast on invalid data.

### `ProcessBatchAsync_ValidBatch_ReturnsCompletedBatch`
Tests successful batch processing with valid inputs. Ensures the batch transitions to a completed state with all images processed and no failures recorded.

### `ProcessBatchAsync_WithCancellation_ReturnsCancelledBatch`
Validates that cancelling a batch during processing results in a batch marked as cancelled with no further progress. Ensures cancellation is respected and resources are cleaned up.

### `ProcessBatchAsync_PartialFailure_RecordsFailedCount`
Tests processing with some invalid images. Confirms the batch completes with a non-zero failure count and all valid images are processed.

### `GetBatchStatus_ActiveBatch_ReturnsBatch`
Ensures retrieving the status of an active batch returns the batch object with correct state. Validates the service returns expected data for active batches.

### `GetBatchStatus_NonExistentBatch_ReturnsNull`
Confirms that querying a non-existent batch returns `null` rather than throwing. Tests graceful handling of missing resources.

### `CancelBatch_ActiveBatch_ReturnsTrueAndMarksAsCancel`
Validates that cancelling an active batch returns `true` and updates the batch state to cancelled. Ensures the operation is idempotent and state is consistent.

### `CancelBatch_NonExistentBatch_ReturnsFalse`
Tests cancellation of a non-existent batch. Ensures the method returns `false` without throwing, maintaining consistent behavior for invalid inputs.

### `CreateBatchAsync_EmptyImageIds_ThrowsArgumentException`
Verifies that creating a batch with no image IDs throws an `ArgumentException`. Ensures the service enforces minimum input requirements.

### `CreateBatchAsync_EmptyFilterIds_ThrowsArgumentException`
Confirms that creating a batch with no filter IDs throws an `ArgumentException`. Validates that both image and filter lists must be non-empty.

### `CreateBatchAsync_ValidInputs_ReturnsBatchWithImages`
Tests successful batch creation with valid image and filter IDs. Ensures the returned batch contains the correct image references and initial state.

### `GetBatchProgress_ActiveBatch_ReturnsProgressDictionary`
Validates that progress tracking for an active batch returns a dictionary mapping image IDs to completion status. Ensures real-time monitoring is accurate.

### `GetBatchProgress_NonExistentBatch_ReturnsEmptyDictionary`
Confirms that querying progress for a non-existent batch returns an empty dictionary rather than throwing. Tests graceful handling of missing resources.

### `GetActiveBatchCount_MultipleActiveBatches_ReturnsCorrectCount`
Validates that counting active batches returns the correct number of batches in progress. Ensures the service maintains accurate state across multiple batches.

### `GetActiveBatches_MultipleActiveBatches_ReturnsAllBatches`
Tests retrieval of all active batches. Ensures the service returns a complete and accurate list of in-progress batches.

### `ProcessBatchAsync_CreatesOutputDirectory`
Validates that processing a batch creates the required output directory on disk. Ensures the service handles file system operations correctly during batch execution.

## Usage
