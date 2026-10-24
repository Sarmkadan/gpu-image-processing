#nullable enable

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Represents configuration settings for concurrency and configuration testing scenarios.
/// </summary>
/// <param name="MaxConcurrentThreads">Maximum number of concurrent threads to use during testing.</param>
/// <param name="TestDurationSeconds">Duration in seconds for each test run.</param>
/// <param name="EnableGpuAcceleration">Whether GPU acceleration should be enabled for tests.</param>
/// <param name="MemoryLimitMb">Maximum memory limit in megabytes for test operations.</param>
/// <param name="ConcurrencyStrategy">The concurrency strategy to apply during testing.</param>
public sealed record ConcurrencyAndConfigurationTestSettings(
    int MaxConcurrentThreads = 8,
    int TestDurationSeconds = 30,
    bool EnableGpuAcceleration = true,
    long MemoryLimitMb = 512,
    ConcurrencyStrategy ConcurrencyStrategy = ConcurrencyStrategy.Balanced)
{
    /// <summary>
    /// Gets a value indicating whether the settings are valid.
    /// </summary>
    public bool IsValid => MaxConcurrentThreads > 0 && TestDurationSeconds > 0 && MemoryLimitMb > 0;
}

/// <summary>
/// Defines the concurrency strategy to apply during testing.
/// </summary>
public enum ConcurrencyStrategy
{
    /// <summary>
    /// Balanced approach with moderate concurrency, suitable for most scenarios.
    /// </summary>
    Balanced,

    /// <summary>
    /// Maximize throughput with high concurrency, prioritizing processing speed over resource usage.
    /// </summary>
    ThroughputMaximized,

    /// <summary>
    /// Minimize latency with low concurrency, prioritizing response time over processing speed.
    /// </summary>
    LatencyMinimized,

    /// <summary>
    /// Optimize for memory efficiency, minimizing memory footprint at the cost of processing speed.
    /// </summary>
    MemoryOptimized
}
