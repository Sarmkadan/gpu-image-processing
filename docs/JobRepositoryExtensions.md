# JobRepositoryExtensions

`JobRepositoryExtensions` is a static class that provides a set of convenience query methods for working with `ProcessingJob` entities through a job repository. The extension methods encapsulate common filtering and aggregation scenarios, allowing callers to retrieve jobs based on status, activity, progress thresholds, date ranges, recency, text search, longevity, and overall counts without repeating query logic.

## API

### GetByStatusAsync
- **Purpose:** Retrieves all jobs that match a specified processing status.
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` containing the jobs with the requested status.
- **Exceptions:** May propagate exceptions from the underlying repository (e.g., if the repository cannot be accessed or an invalid state is encountered).

### GetActiveJobsAsync
- **Purpose:** Returns jobs that are currently active (e.g., queued, running, or paused).
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` of active jobs.
- **Exceptions:** May throw exceptions originating from the data store access layer.

### GetJobsAboveProgressAsync
- **Purpose:** Gets jobs whose progress percentage exceeds a given threshold.
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` for jobs with progress above the threshold.
- **Exceptions:** May throw exceptions if the repository encounters an error while evaluating progress values.

### GetJobsCreatedBetweenAsync
- **Purpose:** Returns jobs created within a supplied date‑time interval.
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` of jobs whose creation timestamp falls between the bounds.
- **Exceptions:** May throw exceptions related to invalid date handling or repository access failures.

### GetMostRecentCompletedAsync
- **Purpose:** Retrieves the most recently completed jobs, limited by an internal count or configuration.
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` of the latest completed jobs.
- **Exceptions:** May propagate any repository‑level exceptions (e.g., connection failures).

### SearchByNameOrDescriptionAsync
- **Purpose:** Performs a case‑insensitive search for jobs whose name or description contains the supplied text.
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` matching the search criteria.
- **Exceptions:** May throw exceptions from the underlying query execution.

### GetStatisticsByStatusAsync
- **Purpose:** Calculates aggregate statistics (e.g., counts, averages) grouped by job status.
- **Parameters:** (none)
- **Return Value:** `Task<JobStatistics>` containing the computed statistics.
- **Exceptions:** May throw exceptions if the repository cannot compute the aggregates.

### GetLongRunningJobsAsync
- **Purpose:** Identifies jobs that have exceeded a configured duration threshold.
- **Parameters:** (none)
- **Return Value:** `Task<IReadOnlyList<ProcessingJob>>` of long‑running jobs.
- **Exceptions:** May throw exceptions arising from time‑based calculations or repository access.

### GetTotalJobsCountAsync
- **Purpose:** Returns the total number of jobs stored in the repository.
- **Parameters:** (none)
- **Return Value:** `Task<int>` representing the overall job count.
- **Exceptions:** May throw exceptions if the count operation fails.

## Usage

```csharp
// Example 1: Obtain all jobs that are currently active and display their IDs.
IReadOnlyList<ProcessingJob> activeJobs = await jobRepository.GetActiveJobsAsync();
foreach (var job in activeJobs)
{
    Console.WriteLine($"Active Job ID: {job.Id}");
}

// Example 2: Retrieve statistics for monitoring dashboard purposes.
JobStatistics stats = await jobRepository.GetStatisticsByStatusAsync();
Console.WriteLine($"Completed: {stats.CompletedCount}, Failed: {stats.FailedCount}");
```

## Notes

- The extension methods are stateless; they do not maintain any internal data and rely entirely on the repository instance they are invoked upon.
- Thread safety depends on the underlying repository implementation. If the repository is thread‑safe, concurrent calls to these extension methods are safe; otherwise, external synchronization is required.
- When a method returns an empty list, it indicates that no matching records were found rather than an error condition.
- Callers should consider supplying a `CancellationToken` (if the repository overloads support it) to allow cooperative cancellation of long‑running queries.
- Exceptions thrown by these methods are not wrapped; they reflect the exact errors reported by the data access layer, enabling callers to handle specific failure scenarios as needed.
