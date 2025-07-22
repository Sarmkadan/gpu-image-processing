# ResultRepository

`ResultRepository` provides query and aggregation access to persisted GPU image processing results. It exposes asynchronous methods that retrieve subsets of `ProcessingResult` records based on job identity, image identity, outcome status, time range, applied filter, or performance thresholds, alongside computed statistics and maintenance operations. The synchronous properties offer immediate snapshot values for common metrics without requiring an explicit query.

## API

### async Task<IEnumerable<ProcessingResult>> GetByJobAsync
Returns all processing results associated with a specific job identifier.  
**Parameters:** job ID (type inferred from context, typically `string` or `Guid`).  
**Returns:** a collection of `ProcessingResult` records for that job.  
**Throws:** `ArgumentNullException` when the job ID is null or empty.

### async Task<IEnumerable<ProcessingResult>> GetByImageAsync
Returns all processing results for a given source image identifier.  
**Parameters:** image ID (type inferred from context, typically `string` or `Guid`).  
**Returns:** a collection of `ProcessingResult` records linked to that image.  
**Throws:** `ArgumentNullException` when the image ID is null or empty.

### async Task<IEnumerable<ProcessingResult>> GetSuccessfulAsync
Returns all processing results whose outcome status indicates success.  
**Returns:** a collection of `ProcessingResult` records with a successful status flag.

### async Task<IEnumerable<ProcessingResult>> GetFailedAsync
Returns all processing results whose outcome status indicates failure.  
**Returns:** a collection of `ProcessingResult` records with a failed status flag.

### async Task<IEnumerable<ProcessingResult>> GetProcessedBetweenAsync
Returns processing results whose completion timestamp falls within a specified inclusive range.  
**Parameters:** `DateTime start` and `DateTime end`.  
**Returns:** a collection of `ProcessingResult` records processed in that window.  
**Throws:** `ArgumentException` when `start` is later than `end`.

### async Task<IEnumerable<ProcessingResult>> GetByAppliedFilterAsync
Returns processing results where a particular filter was applied.  
**Parameters:** filter name (type inferred, typically `string`).  
**Returns:** a collection of `ProcessingResult` records that used the specified filter.  
**Throws:** `ArgumentNullException` when the filter name is null or empty.

### async Task<IEnumerable<ProcessingResult>> GetSlowProcessingAsync
Returns processing results whose processing time exceeded a defined slow threshold.  
**Parameters:** threshold value in milliseconds (type inferred, typically `float` or `double`).  
**Returns:** a collection of `ProcessingResult` records slower than the threshold.  
**Throws:** `ArgumentOutOfRangeException` when the threshold is negative.

### async Task<float> GetAverageCompressionRatioAsync
Computes the average compression ratio across all successful results that have a measurable output size.  
**Returns:** the mean compression ratio as a `float`. Returns `0.0f` when no qualifying results exist.

### async Task<ResultStatistics> GetStatisticsAsync
Aggregates a comprehensive statistics object from all stored results.  
**Returns:** a `ResultStatistics` instance containing counts, timing distributions, size summaries, and success/failure breakdowns.

### async Task<Dictionary<string, int>> GetMostUsedFiltersAsync
Counts filter usage across all results and returns a dictionary mapping filter names to their occurrence counts, ordered by descending frequency.  
**Returns:** a `Dictionary<string, int>` of filter usage counts.

### async Task<int> ClearOldResultsAsync
Removes results whose completion timestamp is older than a specified cutoff.  
**Parameters:** `DateTime cutoff` — results completed before this point are deleted.  
**Returns:** the number of removed records.  
**Throws:** `ArgumentException` when the cutoff is in the future.

### int TotalResults
Gets the total number of `ProcessingResult` records currently stored.

### int SuccessfulResults
Gets the number of results with a successful outcome status.

### int FailedResults
Gets the number of results with a failed outcome status.

### double SuccessRate
Gets the ratio of successful results to total results as a value between `0.0` and `1.0`. Returns `0.0` when there are no results.

### float AverageProcessingTimeMs
Gets the mean processing time in milliseconds across all completed results. Returns `0.0f` when no results exist.

### float FastestProcessingMs
Gets the minimum processing time in milliseconds among all completed results. Returns `0.0f` when no results exist.

### float SlowestProcessingMs
Gets the maximum processing time in milliseconds among all completed results. Returns `0.0f` when no results exist.

### long TotalOutputBytes
Gets the sum of output file sizes in bytes across all results that produced output.

### double AverageOutputFileSize
Gets the mean output file size in bytes across results that produced output. Returns `0.0` when no output-producing results exist.

## Usage

```csharp
// Retrieve statistics for a dashboard and clean up old records
var repo = new ResultRepository(connectionFactory);

ResultStatistics stats = await repo.GetStatisticsAsync();
Console.WriteLine($"Success rate: {repo.SuccessRate:P1}");
Console.WriteLine($"Avg processing time: {repo.AverageProcessingTimeMs:F2} ms");

int removed = await repo.ClearOldResultsAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Cleaned up {removed} results older than 30 days.");
```

```csharp
// Investigate slow jobs that used a specific filter in a time window
var repo = new ResultRepository(connectionFactory);

var slowResults = await repo.GetSlowProcessingAsync(thresholdMs: 5000.0f);
var filteredSlow = slowResults.Where(r => r.AppliedFilter == "BilateralBlur");

var recentFailed = await repo.GetFailedAsync();
var recentFailedInWindow = await repo.GetProcessedBetweenAsync(
    DateTime.UtcNow.AddHours(-6),
    DateTime.UtcNow
);

var failedFilterCounts = recentFailedInWindow
    .GroupBy(r => r.AppliedFilter)
    .Select(g => new { Filter = g.Key, Failures = g.Count() })
    .OrderByDescending(x => x.Failures);

foreach (var item in failedFilterCounts)
    Console.WriteLine($"{item.Filter}: {item.Failures} failures");
```

## Notes

- All asynchronous query methods return empty collections rather than null when no matching records exist.
- The synchronous properties (`TotalResults`, `SuccessRate`, `AverageProcessingTimeMs`, etc.) reflect the state at the moment of access and are not atomically consistent with one another unless the underlying store guarantees snapshot isolation.
- `GetAverageCompressionRatioAsync` and `AverageOutputFileSize` exclude results where output size is zero or unavailable; division-by-zero is guarded internally.
- `ClearOldResultsAsync` uses the provided cutoff exclusively; records exactly at the cutoff timestamp are retained.
- This type is designed for use in multi-threaded environments. Individual reads are safe, but callers should not assume that a sequence of property accesses represents a point-in-time snapshot. External synchronisation is required if a consistent multi-value read is needed.
- Methods accepting identifier or filter name parameters treat null, empty, or whitespace-only strings as invalid and will throw `ArgumentNullException`.
