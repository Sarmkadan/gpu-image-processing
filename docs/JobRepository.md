# JobRepository

Central repository for managing and querying image-processing jobs. Provides methods to retrieve jobs by status, filter criteria, or image association, as well as statistics and cleanup operations.

## API

### `public async Task<IEnumerable<ProcessingJob>> GetByStatusAsync(JobStatus status)`

Retrieves all jobs matching the specified status.

- **Parameters**
  - `status`: The job status to filter by (e.g., `Pending`, `Running`, `Completed`).
- **Returns**
  - An enumerable of `ProcessingJob` instances with the given status.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `status` is not a valid `JobStatus` value.

---

### `public async Task<IEnumerable<ProcessingJob>> GetPendingAsync()`

Retrieves all jobs currently in the `Pending` state.

- **Returns**
  - An enumerable of `ProcessingJob` instances with `JobStatus.Pending`.

---

### `public async Task<IEnumerable<ProcessingJob>> GetRunningAsync()`

Retrieves all jobs currently in the `Running` state.

- **Returns**
  - An enumerable of `ProcessingJob` instances with `JobStatus.Running`.

---
### `public async Task<IEnumerable<ProcessingJob>> GetFailedAsync()`

Retrieves all jobs currently in the `Failed` state.

- **Returns**
  - An enumerable of `ProcessingJob` instances with `JobStatus.Failed`.

---
### `public async Task<IEnumerable<ProcessingJob>> GetCompletedBetweenAsync(DateTime start, DateTime end)`

Retrieves all jobs completed within the specified time range (inclusive).

- **Parameters**
  - `start`: The start of the time range.
  - `end`: The end of the time range.
- **Returns**
  - An enumerable of `ProcessingJob` instances with `CompletedAt` within `[start, end]`.
- **Exceptions**
  - Throws `ArgumentException` if `start > end`.

---
### `public async Task<IEnumerable<ProcessingJob>> GetByFilterAsync(JobFilter filter)`

Retrieves jobs matching the provided filter criteria.

- **Parameters**
  - `filter`: A `JobFilter` object specifying status, time range, and other constraints.
- **Returns**
  - An enumerable of `ProcessingJob` instances matching the filter.
- **Exceptions**
  - Throws `ArgumentException` if `filter` contains invalid combinations (e.g., conflicting statuses).

---
### `public async Task<IEnumerable<ProcessingJob>> GetByImageAsync(Guid imageId)`

Retrieves all jobs associated with the specified image.

- **Parameters**
  - `imageId`: The unique identifier of the image.
- **Returns**
  - An enumerable of `ProcessingJob` instances linked to the image.

---
### `public async Task<IEnumerable<ProcessingJob>> GetHighProgressAsync(double threshold)`

Retrieves jobs with progress exceeding the specified threshold.

- **Parameters**
  - `threshold`: The minimum progress percentage (0.0 to 1.0).
- **Returns**
  - An enumerable of `ProcessingJob` instances with `Progress > threshold`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `threshold` is outside [0.0, 1.0].

---
### `public async Task<ProcessingJob?> GetOldestIncompleteAsync()`

Retrieves the oldest job that is not in a terminal state (`Completed`, `Failed`, `Cancelled`).

- **Returns**
  - The oldest `ProcessingJob` with `Status` other than terminal states, or `null` if none exist.

---
### `public async Task<JobStatistics> GetStatisticsAsync()`

Aggregates statistics about all jobs in the repository.

- **Returns**
  - A `JobStatistics` object containing counts for each status, average completion time, success rate, and total images processed.

---
### `public async Task<int> ClearOldFailedJobsAsync(TimeSpan ageThreshold)`

Removes failed jobs older than the specified age threshold.

- **Parameters**
  - `ageThreshold`: The minimum age for a job to be eligible for deletion.
- **Returns**
  - The number of jobs removed.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `ageThreshold` is negative.

---
### `public int TotalJobs`

Gets the total number of jobs in the repository.

- **Returns**
  - The count of all jobs, regardless of status.

---
### `public int PendingJobs`

Gets the number of jobs in the `Pending` state.

- **Returns**
  - The count of jobs with `JobStatus.Pending`.

---
### `public int RunningJobs`

Gets the number of jobs in the `Running` state.

- **Returns**
  - The count of jobs with `JobStatus.Running`.

---
### `public int CompletedJobs`

Gets the number of jobs in the `Completed` state.

- **Returns**
  - The count of jobs with `JobStatus.Completed`.

---
### `public int FailedJobs`

Gets the number of jobs in the `Failed` state.

- **Returns**
  - The count of jobs with `JobStatus.Failed`.

---
### `public int CancelledJobs`

Gets the number of jobs in the `Cancelled` state.

- **Returns**
  - The count of jobs with `JobStatus.Cancelled`.

---
### `public double AverageCompletionTime`

Gets the average time (in seconds) taken to complete jobs.

- **Returns**
  - The average completion time for all completed jobs, or `0.0` if no jobs are completed.

---
### `public double SuccessRate`

Gets the ratio of successful jobs to total jobs.

- **Returns**
  - A value between `0.0` and `1.0` representing the success rate, or `0.0` if no jobs exist.

---
### `public long TotalImagesProcessed`

Gets the total number of images processed across all jobs.

- **Returns**
  - The cumulative count of images processed by all completed jobs.

## Usage

### Example 1: Retrieving and processing pending jobs
