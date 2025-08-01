# MetricsUtilities
The `MetricsUtilities` type provides a set of methods and properties for calculating and analyzing statistical metrics, such as mean, median, and standard deviation, as well as detecting anomalies and calculating rates of change. It is designed to be used in the context of image processing and other data-intensive applications, where understanding the characteristics of large datasets is crucial.

## API
* `public static StatisticalMetrics CalculateStatistics`: Calculates a range of statistical metrics, including mean, median, and standard deviation, from a dataset. Returns a `StatisticalMetrics` object containing the calculated metrics.
* `public static double GetPercentile`: Returns the value at a specified percentile in a dataset. Parameters: `double[] data`, `double percentile`. Returns the value at the specified percentile. Throws `ArgumentException` if `percentile` is outside the range [0, 100].
* `public static Histogram CreateHistogram`: Creates a histogram from a dataset. Parameters: `double[] data`, `int bucketCount`. Returns a `Histogram` object representing the dataset.
* `public static List<double> DetectAnomalies`: Detects anomalies in a dataset using a statistical method. Parameters: `double[] data`, `double threshold`. Returns a list of anomalous values.
* `public static List<double> CalculateRateOfChange`: Calculates the rate of change between consecutive values in a dataset. Parameters: `double[] data`. Returns a list of rates of change.
* `public static List<double> CalculateMovingAverage`: Calculates the moving average of a dataset. Parameters: `double[] data`, `int windowSize`. Returns a list of moving averages.
* `public static double CalculateThroughput`: Calculates the throughput of a system. Parameters: `double[] data`, `double timeInterval`. Returns the throughput.
* `public static float CalculateEfficiency`: Calculates the efficiency of a system. Parameters: `double[] data`, `double capacity`. Returns the efficiency.
* `public int Count`: Gets the number of values in the dataset.
* `public double Min`: Gets the minimum value in the dataset.
* `public double Max`: Gets the maximum value in the dataset.
* `public double Mean`: Gets the mean value of the dataset.
* `public double Median`: Gets the median value of the dataset.
* `public double P95`: Gets the 95th percentile value of the dataset.
* `public double P99`: Gets the 99th percentile value of the dataset.
* `public double StdDev`: Gets the standard deviation of the dataset.
* `public double Sum`: Gets the sum of all values in the dataset.
* `public List<HistogramBucket> Buckets`: Gets the histogram buckets of the dataset.
* `public int TotalCount`: Gets the total count of values in the dataset.
* `public int BucketNumber`: Gets the number of buckets in the histogram.

## Usage
```csharp
// Example 1: Calculating statistical metrics
double[] data = { 1, 2, 3, 4, 5 };
StatisticalMetrics metrics = MetricsUtilities.CalculateStatistics(data);
Console.WriteLine($"Mean: {metrics.Mean}, Median: {metrics.Median}, StdDev: {metrics.StdDev}");

// Example 2: Detecting anomalies
double[] data2 = { 1, 2, 3, 4, 5, 100 };
List<double> anomalies = MetricsUtilities.DetectAnomalies(data2, 2);
Console.WriteLine($"Anomalies: {string.Join(", ", anomalies)}");
```

## Notes
* The `MetricsUtilities` type is not thread-safe, and its methods should not be called concurrently from multiple threads.
* The `CalculateStatistics` method assumes that the input dataset is not empty, and will throw an `ArgumentException` if it is.
* The `GetPercentile` method will return `double.NaN` if the input dataset is empty, or if the specified percentile is outside the range [0, 100].
* The `DetectAnomalies` method uses a statistical method to detect anomalies, and may not detect all anomalies in the dataset. The `threshold` parameter can be adjusted to trade off between false positives and false negatives.
