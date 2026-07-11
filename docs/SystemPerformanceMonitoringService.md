# SystemPerformanceMonitoringService
The `SystemPerformanceMonitoringService` class is designed to monitor and record the performance of various operations within the `gpu-image-processing` project. It provides a centralized mechanism for tracking and analyzing the execution times of different operations, allowing for the identification of performance bottlenecks and optimization opportunities.

## API
* `public SystemPerformanceMonitoringService`: The constructor for the `SystemPerformanceMonitoringService` class, which initializes a new instance of the service.
* `public void RecordMetric`: Records a performance metric for a specific operation. Parameters: none. Return value: none. Throws: none.
* `public OperationStatistics GetStatistics`: Retrieves the statistics for a specific operation. Parameters: none. Return value: an `OperationStatistics` object containing the operation's performance metrics. Throws: none.
* `public Dictionary<string, OperationStatistics> GetAllStatistics`: Retrieves the statistics for all monitored operations. Parameters: none. Return value: a dictionary of `OperationStatistics` objects, keyed by operation name. Throws: none.
* `public bool ResetStatistics`: Resets the statistics for a specific operation. Parameters: none. Return value: a boolean indicating whether the reset was successful. Throws: none.
* `public void ResetAll`: Resets the statistics for all monitored operations. Parameters: none. Return value: none. Throws: none.
* `public int GetMonitoredOperationCount`: Retrieves the number of operations being monitored. Parameters: none. Return value: an integer representing the number of monitored operations. Throws: none.
* `public List<string> GetSlowOperations`: Retrieves a list of operations that are currently performing slowly. Parameters: none. Return value: a list of operation names. Throws: none.
* `public string OperationName`: Gets or sets the name of the operation being monitored. Parameters: none. Return value: a string representing the operation name. Throws: none.
* `public List<long> Measurements`: Gets or sets the list of measurements for the operation. Parameters: none. Return value: a list of long values representing the measurements. Throws: none.
* `public PerformanceMetric`: Gets or sets the performance metric for the operation. Parameters: none. Return value: a `PerformanceMetric` object. Throws: none.
* `public void RecordMeasurement`: Records a measurement for the operation. Parameters: none. Return value: none. Throws: none.
* `public OperationStatistics GetStatistics`: Retrieves the statistics for the operation. Parameters: none. Return value: an `OperationStatistics` object containing the operation's performance metrics. Throws: none.
* `public void Reset`: Resets the statistics for the operation. Parameters: none. Return value: none. Throws: none.
* `public string OperationName`: Gets or sets the name of the operation. Parameters: none. Return value: a string representing the operation name. Throws: none.
* `public int TotalMeasurements`: Gets the total number of measurements for the operation. Parameters: none. Return value: an integer representing the total number of measurements. Throws: none.
* `public long MinMs`: Gets the minimum execution time for the operation. Parameters: none. Return value: a long value representing the minimum execution time in milliseconds. Throws: none.
* `public long MaxMs`: Gets the maximum execution time for the operation. Parameters: none. Return value: a long value representing the maximum execution time in milliseconds. Throws: none.
* `public long AverageMs`: Gets the average execution time for the operation. Parameters: none. Return value: a long value representing the average execution time in milliseconds. Throws: none.
* `public long MedianMs`: Gets the median execution time for the operation. Parameters: none. Return value: a long value representing the median execution time in milliseconds. Throws: none.

## Usage
The following examples demonstrate how to use the `SystemPerformanceMonitoringService` class:
```csharp
// Example 1: Monitoring a single operation
var monitoringService = new SystemPerformanceMonitoringService();
monitoringService.OperationName = "ImageProcessing";
monitoringService.RecordMeasurement(100); // Record a measurement of 100ms
var statistics = monitoringService.GetStatistics();
Console.WriteLine($"Average execution time: {statistics.AverageMs}ms");

// Example 2: Monitoring multiple operations
var monitoringService = new SystemPerformanceMonitoringService();
monitoringService.OperationName = "ImageLoading";
monitoringService.RecordMeasurement(50); // Record a measurement of 50ms
monitoringService.OperationName = "ImageProcessing";
monitoringService.RecordMeasurement(200); // Record a measurement of 200ms
var allStatistics = monitoringService.GetAllStatistics();
foreach (var operation in allStatistics)
{
    Console.WriteLine($"Operation: {operation.Key}, Average execution time: {operation.Value.AverageMs}ms");
}
```

## Notes
* The `SystemPerformanceMonitoringService` class is not thread-safe by default. If you plan to use it in a multi-threaded environment, you should implement appropriate synchronization mechanisms to ensure data integrity.
* The `ResetStatistics` and `ResetAll` methods can be used to reset the statistics for specific operations or all operations, respectively. However, this will also reset the `TotalMeasurements` counter, which may not be desirable in all cases.
* The `GetSlowOperations` method returns a list of operations that are currently performing slowly, based on their average execution times. The threshold for determining slow operations is not configurable and is based on the average execution time of all monitored operations.
* The `PerformanceMetric` property returns a `PerformanceMetric` object, which contains additional information about the operation's performance, such as the minimum, maximum, and median execution times.
