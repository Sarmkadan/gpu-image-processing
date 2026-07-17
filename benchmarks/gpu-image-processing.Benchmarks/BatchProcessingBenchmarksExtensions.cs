namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Extension methods for <see cref="BatchProcessingBenchmarks"/>.
/// </summary>
public static class BatchProcessingBenchmarksExtensions
{
    /// <summary>
    /// Calculates the estimated total processing time based on the current progress and estimated remaining time.
    /// </summary>
    /// <param name="benchmarks">The <see cref="BatchProcessingBenchmarks"/> instance.</param>
    /// <returns>The estimated total processing time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if estimated remaining time is not available or progress percentage is 0.</exception>
    public static TimeSpan GetEstimatedTotalProcessingTime(this BatchProcessingBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        var estimatedRemainingTime = benchmarks.GetEstimatedRemainingTime();
        if (estimatedRemainingTime == null)
        {
            throw new InvalidOperationException("Estimated remaining time is not available.");
        }

        var progressPercentage = benchmarks.GetProgressPercentage();
        if (progressPercentage <= 0)
        {
            throw new InvalidOperationException("Progress percentage must be greater than 0 to calculate estimated total time.");
        }

        return TimeSpan.FromTicks((long)(estimatedRemainingTime.Value.Ticks / (progressPercentage / 100.0)));
    }

    /// <summary>
    /// Determines if the benchmark has completed processing all images.
    /// </summary>
    /// <param name="benchmarks">The <see cref="BatchProcessingBenchmarks"/> instance.</param>
    /// <returns>true if all images have been processed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    public static bool HasCompletedProcessing(this BatchProcessingBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        return benchmarks.GetProgressPercentage() == 100;
    }

    /// <summary>
    /// Creates a new <see cref="BatchProcessingBenchmarks"/> instance with a priority queue.
    /// </summary>
    /// <param name="benchmarks">The <see cref="BatchProcessingBenchmarks"/> instance.</param>
    /// <returns>A new <see cref="BatchProcessingBenchmarks"/> instance with a priority queue.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="benchmarks"/>.ImageCount is less than 1.</exception>
    public static BatchProcessingBenchmarks WithPriorityQueue(this BatchProcessingBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);

        if (benchmarks.ImageCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(benchmarks), "ImageCount must be at least 1.");
        }

        return new BatchProcessingBenchmarks { ImageCount = benchmarks.ImageCount };
    }
}
