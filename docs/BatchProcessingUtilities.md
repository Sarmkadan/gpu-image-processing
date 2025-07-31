# BatchProcessingUtilities

Utility class providing methods for partitioning, scheduling, monitoring, and retrying batch processing tasks on GPU-accelerated image processing pipelines. Designed to optimize throughput by balancing workload distribution across GPU resources while maintaining processing priority and error resilience.

## API

### `public static List<List<T>> PartitionForGpu<T>(IEnumerable<T> items, int maxBatchSize)`

Partitions an input sequence into sublists suitable for GPU processing, ensuring each sublist does not exceed the specified maximum batch size.

- **Parameters**
  - `items`: The input sequence of items to partition.
  - `maxBatchSize`: The maximum number of items allowed in a single batch.
- **Return Value**: A list of lists, where each inner list represents a GPU-compatible batch.
- **Throws**: `ArgumentNullException` if `items` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `maxBatchSize` is less than 1.

---

### `public static int CalculateOptimalBatchSize(int totalItems, int maxGpuMemoryMb)`

Calculates the recommended batch size based on total item count and available GPU memory.

- **Parameters**
  - `totalItems`: Total number of items to process.
  - `maxGpuMemoryMb`: Maximum available GPU memory in megabytes.
- **Return Value**: The optimal batch size as an integer.
- **Throws**: `ArgumentOutOfRangeException` if `totalItems` is negative or `maxGpuMemoryMb` is less than 1.

---

### `public static List<BatchItem<T>> ScheduleWithPriority<T>(IEnumerable<T> items, int priority)`

Creates a prioritized schedule of batch items for processing, assigning a uniform priority to all items.

- **Parameters**
  - `items`: The sequence of items to schedule.
  - `priority`: The priority level to assign to each item (higher values indicate higher priority).
- **Return Value**: A list of `BatchItem<T>` objects, each initialized with the given priority and sequence number.
- **Throws**: `ArgumentNullException` if `items` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `priority` is outside the valid range for `int`.

---

### `public static TimeSpan EstimateProcessingTime(int totalItems, double itemsPerSecond)`

Estimates the total processing time required for a given number of items at a measured throughput rate.

- **Parameters**
  - `totalItems`: The total number of items to process.
  - `itemsPerSecond`: The observed processing rate in items per second.
- **Return Value**: A `TimeSpan` representing the estimated duration.
- **Throws**: `ArgumentOutOfRangeException` if `totalItems` is negative or `itemsPerSecond` is zero or negative.

---

### `public static BatchProgress CalculateProgress(int processedCount, int totalCount, DateTime? startTime)`

Computes the current progress metrics for a batch processing operation.

- **Parameters**
  - `processedCount`: Number of items successfully processed so far.
  - `totalCount`: Total number of items in the batch.
  - `startTime`: The start time of processing (optional; if `null`, elapsed time is zero).
- **Return Value**: A `BatchProgress` struct containing `PercentComplete`, `ItemsPerSecond`, and `ElapsedTime`.
- **Throws**: `ArgumentOutOfRangeException` if `processedCount` is negative, greater than `totalCount`, or if `totalCount` is zero.

---

### `public static ThrottleRecommendation EvaluateThrottleNeeded(double currentItemsPerSecond, double targetItemsPerSecond)`

Determines whether throttling is required based on current throughput compared to a target rate.

- **Parameters**
  - `currentItemsPerSecond`: The current processing rate.
  - `targetItemsPerSecond`: The desired processing rate.
- **Return Value**: A `ThrottleRecommendation` indicating whether to increase, decrease, or maintain the current processing rate.
- **Throws**: No exceptions.

---

### `public static async Task RetryFailedItemsAsync<T>(List<BatchItem<T>> batch, Func<BatchItem<T>, Task> processItemAsync, int maxRetries = 3, int delayMs = 1000)`

Asynchronously retries processing of failed items in a batch up to a maximum number of attempts.

- **Parameters**
  - `batch`: The list of batch items to retry.
  - `processItemAsync`: Async function to process a single item.
  - `maxRetries`: Maximum number of retry attempts per item (default: 3).
  - `delayMs`: Delay in milliseconds between retry attempts (default: 1000).
- **Return Value**: A `Task` representing the asynchronous operation.
- **Throws**: `ArgumentNullException` if `batch` or `processItemAsync` is `null`.
- **Throws**: `ArgumentOutOfRangeException` if `maxRetries` is negative or `delayMs` is negative.

---

### `public T Item { get; }`

Gets the payload or data associated with a batch item.

- **Type**: `T`
- **Access**: Read-only
- **Throws**: No exceptions.

---

### `public int SequenceNumber { get; }`

Gets the zero-based index of the item within its batch.

- **Type**: `int`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public int Priority { get; }`

Gets the assigned priority level of the batch item (higher values indicate higher priority).

- **Type**: `int`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public DateTime ScheduledAt { get; }`

Gets the timestamp when the item was scheduled for processing.

- **Type**: `DateTime`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public DateTime? ProcessedAt { get; }`

Gets the timestamp when the item was successfully processed, or `null` if not yet processed.

- **Type**: `DateTime?`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public int RetryCount { get; }`

Gets the number of times this item has been retried after failure.

- **Type**: `int`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public string LastError { get; }`

Gets the error message from the most recent processing failure, or `null` if no failure occurred.

- **Type**: `string`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public int ProcessedCount { get; }`

Gets the number of items successfully processed in the current batch.

- **Type**: `int`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public int TotalCount { get; }`

Gets the total number of items in the current batch.

- **Type**: `int`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public int ErrorCount { get; }`

Gets the number of items in the batch that have failed processing.

- **Type**: `int`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public double PercentComplete { get; }`

Gets the percentage of the batch that has been successfully processed.

- **Type**: `double`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public double ItemsPerSecond { get; }`

Gets the current processing rate in items per second.

- **Type**: `double`
- **Access**: Read-only
- **Throws**: No exceptions.

---
### `public TimeSpan ElapsedTime { get; }`

Gets the total time elapsed since the batch processing began.

- **Type**: `TimeSpan`
- **Access**: Read-only
- **Throws**: No exceptions.

## Usage

### Example 1: Partitioning and Scheduling a Batch
