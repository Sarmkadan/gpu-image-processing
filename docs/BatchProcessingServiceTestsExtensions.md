# BatchProcessingServiceTestsExtensions

Utility class providing extension methods for testing the `BatchProcessingService` behavior. These methods encapsulate common assertions and batch creation patterns used in unit tests, ensuring consistent validation of batch processing outcomes.

## API

### `public static ImageBatch CreateValidBatch()`

Creates a valid `ImageBatch` instance suitable for testing batch processing scenarios. The batch contains a predefined set of test images with consistent metadata and processing states.

- **Parameters**: None
- **Return Value**: `ImageBatch` – A new batch instance initialized with test data.
- **Throws**: Does not throw under normal conditions.

### `public static void ShouldHaveProcessingStats(ImageBatch batch, int expectedCount, ProcessingStats expectedStats)`

Validates that the batch's processing statistics match the expected values.

- **Parameters**:
  - `batch` – The `ImageBatch` instance to validate.
  - `expectedCount` – The expected number of images in the batch.
  - `expectedStats` – The expected `ProcessingStats` object representing the aggregate processing results.
- **Return Value**: None
- **Throws**:
  - `ArgumentNullException` – If `batch` or `expectedStats` is `null`.
  - `ArgumentException` – If the batch's image count does not match `expectedCount`.

### `public static void ShouldBeCompleted(ImageBatch batch)`

Asserts that the batch has reached a completed state, meaning all images have either succeeded or failed processing.

- **Parameters**:
  - `batch` – The `ImageBatch` instance to validate.
- **Return Value**: None
- **Throws**:
  - `ArgumentNullException` – If `batch` is `null`.
  - `InvalidOperationException` – If any image in the batch is still in a pending or processing state.

### `public static void ShouldHaveProcessedAndFailedCount(ImageBatch batch, int expectedProcessed, int expectedFailed)`

Validates that the batch contains the expected number of successfully processed and failed images.

- **Parameters**:
  - `batch` – The `ImageBatch` instance to validate.
  - `expectedProcessed` – The expected count of successfully processed images.
  - `expectedFailed` – The expected count of failed images.
- **Return Value**: None
- **Throws**:
  - `ArgumentNullException` – If `batch` is `null`.
  - `ArgumentException` – If the sum of processed and failed images does not match the total image count in the batch.
  - `InvalidOperationException` – If any image is not in a terminal state (processed or failed).

## Usage
