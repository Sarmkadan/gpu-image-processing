# BatchProcessingBenchmarks

A utility class designed to benchmark and validate batch processing of images, particularly for GPU-accelerated pipelines. It tracks progress, success rates, and estimated completion times while managing prioritized image batches for processing.

## API

### `public int ImageCount`
Gets the total number of images in the current batch.
- **Returns**: The count of images in the batch.
- **Throws**: `InvalidOperationException` if no batch has been created or populated.

---

### `public void Setup()`
Initializes the benchmarking environment and resets internal state.
- **Throws**: `InvalidOperationException` if called after batch creation without resetting.

---

### `public ImageBatch CreateAndPopulateBatch(int count)`
Creates and populates a new batch of images with the specified count.
- **Parameters**:
  - `count`: The number of images to include in the batch.
- **Returns**: An `ImageBatch` instance representing the newly created batch.
- **Throws**:
  - `ArgumentOutOfRangeException` if `count` is less than or equal to zero.
  - `InvalidOperationException` if a batch already exists.

---
### `public bool ValidateBatch(ImageBatch batch)`
Validates the integrity and correctness of the provided batch.
- **Parameters**:
  - `batch`: The batch to validate.
- **Returns**: `true` if the batch is valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `batch` is `null`.

---
### `public double GetProgressPercentage()`
Calculates the current progress as a percentage of completed images.
- **Returns**: A value between `0.0` and `100.0` representing completion progress.
- **Throws**: `InvalidOperationException` if no batch has been created or populated.

---
### `public double GetSuccessRate()`
Computes the success rate of processed images in the current batch.
- **Returns**: A value between `0.0` and `1.0` representing the success rate.
- **Throws**: `InvalidOperationException` if no batch has been created or populated.

---
### `public TimeSpan? GetEstimatedRemainingTime()`
Estimates the time remaining to complete processing the current batch.
- **Returns**: The estimated remaining time as a `TimeSpan`, or `null` if insufficient data is available.
- **Throws**: `InvalidOperationException` if no batch has been created or populated.

---
### `public PriorityQueue<Guid, int> BuildPriorityQueue()`
Constructs a priority queue of image identifiers based on processing priority.
- **Returns**: A `PriorityQueue<Guid, int>` where the priority is derived from internal processing metrics.
- **Throws**: `InvalidOperationException` if no batch has been created or populated.

---
### `public void MarkImageProcessed_TenSuccesses(Guid imageId)`
Records the successful processing of an image and updates internal metrics.
- **Parameters**:
  - `imageId`: The unique identifier of the processed image.
- **Throws**: `ArgumentException` if `imageId` is an empty GUID.

## Usage

### Example 1: Basic Batch Processing Benchmark
