# PerformanceMonitoringServiceTests

The `PerformanceMonitoringServiceTests` class serves as the comprehensive test suite for the `PerformanceMonitoringService` component within the `gpu-image-processing` project. It validates the correctness of metric recording, system resource tracking, throughput calculations, and historical data management. The suite specifically ensures data integrity during concurrent operations, verifies exception handling for invalid initialization states, and confirms the accuracy of statistical aggregations such as averages, success rates, and min/max time tracking.

## API

### Constructors

**`public PerformanceMonitoringServiceTests()`**
Initializes a new instance of the test class. This constructor prepares the test context required to instantiate and evaluate the `PerformanceMonitoringService` under various conditions.

### Test Methods

**`public void Constructor_NullLogger_ThrowsArgumentNullException()`**
Verifies that the `PerformanceMonitoringService` constructor throws an `ArgumentNullException` when provided with a null logger instance.
*   **Parameters**: None (test harness supplies null dependency).
*   **Returns**: `void`.
*   **Throws**: Fails the test if `ArgumentNullException` is not thrown.

**`public void RecordOperation_SuccessfulOperation_RecordsMetrics()`**
Confirms that invoking `RecordOperation` with a successful execution status correctly logs the operation duration and increments the success counter.
*   **Parameters**: None (simulates a successful operation internally).
*   **Returns**: `void`.
*   **Throws**: Fails if metrics are not updated as expected.

**`public void RecordOperation_FailedOperation_IncrementsFailed()`**
Ensures that recording an operation marked as failed increments the failure counter without affecting the success count.
*   **Parameters**: None (simulates a failed operation internally).
*   **Returns**: `void`.
*   **Throws**: Fails if the failure count remains unchanged.

**`public void RecordOperation_MultipleOperations_CalculatesAverageCorrectly()`**
Validates the arithmetic mean calculation of operation durations after multiple recordings.
*   **Parameters**: None (executes a sequence of operations with known durations).
*   **Returns**: `void`.
*   **Throws**: Fails if the calculated average deviates from the expected value.

**`public void RecordOperation_SlowOperation_LogsWarning()`**
Checks that operations exceeding a predefined duration threshold trigger a warning log entry.
*   **Parameters**: None (simulates a slow operation).
*   **Returns**: `void`.
*   **Throws**: Fails if no warning is logged.

**`public void RecordOperation_TracksMinAndMaxTimes()`**
Verifies that the service accurately maintains the minimum and maximum execution times observed across recorded operations.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if min/max values are incorrect.

**`public void UpdateSystemMetrics_UpdatesCpuAndMemoryMetrics()`**
Tests the `UpdateSystemMetrics` method to ensure current CPU and memory usage values are correctly captured and stored.
*   **Parameters**: None (mocks system metric sources).
*   **Returns**: `void`.
*   **Throws**: Fails if CPU or memory metrics are not updated.

**`public void UpdateThroughput_UpdatesPixelsAndMegabytes()`**
Confirms that calling `UpdateThroughput` correctly aggregates the total number of pixels processed and megabytes transferred.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if throughput counters are inaccurate.

**`public void GetCurrentMetrics_ReturnsIndependentCopy()`**
Ensures that the object returned by `GetCurrentMetrics` is a deep copy, preventing external modification of the internal service state.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if modifications to the returned object affect internal state.

**`public void SnapshotAndReset_CreatesSnapshotAndClearsMetrics()`**
Validates that `SnapshotAndReset` archives the current metrics to history and immediately resets the active counters to zero.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if history is not updated or current metrics are not cleared.

**`public void GetMetricsHistory_ReturnsAllSnapshots()`**
Checks that `GetMetricsHistory` returns the complete collection of all previously created snapshots.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if the snapshot count mismatches.

**`public void GetMetricsHistory_WithLimit_ReturnsOnlyRequestedCount()`**
Verifies that requesting a limited history returns only the most recent N snapshots as specified.
*   **Parameters**: None (test defines a limit).
*   **Returns**: `void`.
*   **Throws**: Fails if the returned count exceeds the limit.

**`public void GetMetricsHistory_ReturnsNewestFirst()`**
Ensures the order of the history collection is sorted by timestamp in descending order (newest first).
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if the ordering is incorrect.

**`public void GetAverageMetrics_NoRecentMetrics_ReturnsEmptyDictionary()`**
Confirms that requesting average metrics when no recent data exists returns an empty dictionary rather than throwing an exception or returning invalid data.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if the result is not empty.

**`public void GetAverageMetrics_ReturnsAverageOfRecentMetrics()`**
Calculates and verifies the average of specific metrics over a defined recent time window.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if averages are incorrect.

**`public void GetPerformanceReport_ContainsAllRequiredInformation()`**
Validates that the generated performance report includes all mandatory fields, such as totals, averages, and system status.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if any required field is missing or null.

**`public void GetPerformanceReport_CalculatesSuccessRateCorrectly()`**
Verifies the mathematical accuracy of the success rate percentage calculated in the performance report.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Fails if the success rate calculation is erroneous.

**`public void ConcurrentRecordOperations_IsSafeFromRaceConditions()`**
Stress-tests the `RecordOperation` method by simulating simultaneous calls from multiple threads to ensure data consistency and absence of race conditions.
*   **Parameters**: None (spawns multiple threads).
*   **Returns**: `void`.
*   **Throws**: Fails if data corruption or exceptions occur during concurrent access.

## Usage

### Example 1: Validating Metric Recording and Aggregation
This example demonstrates how the test suite verifies that sequential operations are recorded and averaged correctly.

```csharp
[TestFixture]
public class PerformanceMonitoringServiceTests
{
    [Test]
    public void RecordOperation_MultipleOperations_CalculatesAverageCorrectly()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var service = new PerformanceMonitoringService(logger.Object);
        var expectedAverage = 150.0;

        // Act
        service.RecordOperation(100, true);
        service.RecordOperation(200, true);
        var metrics = service.GetCurrentMetrics();

        // Assert
        Assert.That(metrics.AverageDuration, Is.EqualTo(expectedAverage).Within(0.01));
    }
}
```

### Example 2: Verifying Thread Safety Under Load
This example illustrates the test logic used to ensure the service handles concurrent updates without data corruption.

```csharp
[TestFixture]
public class PerformanceMonitoringServiceTests
{
    [Test]
    public void ConcurrentRecordOperations_IsSafeFromRaceConditions()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var service = new PerformanceMonitoringService(logger.Object);
        int threadCount = 10;
        int operationsPerThread = 100;

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(i =>
            Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    service.RecordOperation(50, true);
                }
            })
        );
        Task.WaitAll(tasks.ToArray());

        // Assert
        var finalMetrics = service.GetCurrentMetrics();
        Assert.That(finalMetrics.TotalOperations, Is.EqualTo(threadCount * operationsPerThread));
    }
}
```

## Notes

*   **Thread Safety**: The `ConcurrentRecordOperations_IsSafeFromRaceConditions` test explicitly validates that internal counters and collections are protected against race conditions. Implementations relying on these tests must ensure that `RecordOperation` and `UpdateSystemMetrics` use appropriate synchronization primitives (e.g., locks or concurrent collections) when accessed from multiple threads.
*   **Data Isolation**: The test `GetCurrentMetrics_ReturnsIndependentCopy` dictates that consumers of the metrics object cannot modify the internal state of the service. Any implementation must return a defensive copy of the data structure.
*   **Empty State Handling**: Methods retrieving averages or reports must handle scenarios where no data has been recorded yet. Specifically, `GetAverageMetrics` should return an empty dictionary rather than throwing a division-by-zero exception or returning null when the dataset is empty.
*   **Ordering Guarantees**: The history retrieval methods guarantee a specific sort order (newest first). Consumers relying on `GetMetricsHistory` should not assume insertion order if the underlying implementation changes, but can rely on the behavior verified by `GetMetricsHistory_ReturnsNewestFirst`.
*   **Exception Propagation**: Initialization failures, such as passing a null logger, are treated as critical errors resulting in `ArgumentNullException`. This ensures fail-fast behavior during service construction.
