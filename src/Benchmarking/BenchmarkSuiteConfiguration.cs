#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Benchmarking;

/// <summary>
/// Describes the level of precision used during a benchmark run.
/// Higher accuracy requires more warmup and measurement iterations.
/// </summary>
public enum BenchmarkAccuracyLevel
{
    /// <summary>Fast, low-precision — suitable for development feedback loops.</summary>
    Quick = 0,
    /// <summary>Balanced precision suitable for CI regression guards.</summary>
    Standard = 1,
    /// <summary>High-precision run for formal performance reports.</summary>
    Thorough = 2
}

/// <summary>
/// Configures which benchmark categories are active and how they are executed.
/// </summary>
/// <remarks>
/// Create an instance, flip the category flags you want, then call
/// <see cref="Validate"/> before passing the object to a benchmark runner.
/// </remarks>
public sealed class BenchmarkSuiteConfiguration
{
    /// <summary>Gets or sets the display name for this benchmark run. Required.</summary>
    public string RunName { get; set; } = string.Empty;

    /// <summary>Include <c>FilterChain</c> operation benchmarks.</summary>
    public bool IncludeFilterChainBenchmarks { get; set; } = true;

    /// <summary>Include <c>ImageBatch</c> and pipeline benchmarks.</summary>
    public bool IncludeBatchProcessingBenchmarks { get; set; } = true;

    /// <summary>Include <c>FilterChainBuilder</c> fluent-construction benchmarks.</summary>
    public bool IncludeFilterChainBuilderBenchmarks { get; set; } = true;

    /// <summary>Include <c>ImageUtilities</c> hot-path benchmarks.</summary>
    public bool IncludeImageUtilitiesBenchmarks { get; set; } = true;

    /// <summary>Include <c>EnumerableExtensions</c> LINQ-alternative benchmarks.</summary>
    public bool IncludeEnumerableExtensionsBenchmarks { get; set; } = false;

    /// <summary>Accuracy / duration profile for the run. Default: <see cref="BenchmarkAccuracyLevel.Standard"/>.</summary>
    public BenchmarkAccuracyLevel AccuracyLevel { get; set; } = BenchmarkAccuracyLevel.Standard;

    /// <summary>
    /// Optional output directory where BenchmarkDotNet artefacts are written.
    /// When null the current working directory is used.
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// When <c>true</c>, hardware performance counters are collected in addition
    /// to wall-clock timings. Requires elevated privileges on some platforms.
    /// </summary>
    public bool EnableHardwareCounters { get; set; }

    /// <summary>
    /// Validates the configuration and returns a list of validation errors.
    /// An empty list means the configuration is ready to use.
    /// </summary>
    /// <returns>
    /// A read-only list of human-readable error strings.
    /// Empty when the configuration is valid.
    /// </returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(RunName))
            errors.Add($"{nameof(RunName)} must not be blank.");

        if (!AnyBenchmarkEnabled())
            errors.Add("At least one benchmark category must be enabled.");

        if (OutputDirectory is not null && OutputDirectory.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            errors.Add($"{nameof(OutputDirectory)} contains invalid path characters.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Returns the names of all benchmark categories enabled by this configuration.
    /// </summary>
    public IReadOnlyList<string> GetEnabledCategories()
    {
        var categories = new List<string>(5);

        if (IncludeFilterChainBenchmarks)
            categories.Add("FilterChain");
        if (IncludeBatchProcessingBenchmarks)
            categories.Add("BatchProcessing");
        if (IncludeFilterChainBuilderBenchmarks)
            categories.Add("FilterChainBuilder");
        if (IncludeImageUtilitiesBenchmarks)
            categories.Add("ImageUtilities");
        if (IncludeEnumerableExtensionsBenchmarks)
            categories.Add("EnumerableExtensions");

        return categories.AsReadOnly();
    }

    /// <summary>
    /// Creates a configuration preset suitable for quick CI feedback:
    /// <see cref="BenchmarkAccuracyLevel.Quick"/> accuracy, core categories only.
    /// </summary>
    public static BenchmarkSuiteConfiguration ForCi(string runName) =>
        new()
        {
            RunName = runName,
            AccuracyLevel = BenchmarkAccuracyLevel.Quick,
            IncludeFilterChainBenchmarks = true,
            IncludeBatchProcessingBenchmarks = true,
            IncludeFilterChainBuilderBenchmarks = true,
            IncludeImageUtilitiesBenchmarks = false,
            IncludeEnumerableExtensionsBenchmarks = false
        };

    /// <summary>
    /// Creates a full configuration preset for formal performance reports:
    /// <see cref="BenchmarkAccuracyLevel.Thorough"/> accuracy, all categories enabled.
    /// </summary>
    public static BenchmarkSuiteConfiguration ForRelease(string runName) =>
        new()
        {
            RunName = runName,
            AccuracyLevel = BenchmarkAccuracyLevel.Thorough,
            IncludeFilterChainBenchmarks = true,
            IncludeBatchProcessingBenchmarks = true,
            IncludeFilterChainBuilderBenchmarks = true,
            IncludeImageUtilitiesBenchmarks = true,
            IncludeEnumerableExtensionsBenchmarks = true,
            EnableHardwareCounters = true
        };

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool AnyBenchmarkEnabled() =>
        IncludeFilterChainBenchmarks
        || IncludeBatchProcessingBenchmarks
        || IncludeFilterChainBuilderBenchmarks
        || IncludeImageUtilitiesBenchmarks
        || IncludeEnumerableExtensionsBenchmarks;
}
