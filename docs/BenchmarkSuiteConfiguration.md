# BenchmarkSuiteConfiguration

A configuration class used to control the execution and reporting behavior of benchmark suites in the GPU image processing project. It defines which benchmark categories are enabled, accuracy requirements, output handling, and hardware-level instrumentation options.

## API

### `RunName`
Gets or sets the name assigned to this benchmark run. This value is used in output files, logs, and reports to identify the execution context.

### `IncludeFilterChainBenchmarks`
Gets or sets a boolean indicating whether filter chain benchmarks should be included in the suite. When `true`, benchmarks related to image filtering pipelines are executed.

### `IncludeBatchProcessingBenchmarks`
Gets or sets a boolean indicating whether batch processing benchmarks should be included in the suite. When `true`, benchmarks related to processing multiple images in bulk are executed.

### `IncludeFilterChainBuilderBenchmarks`
Gets or sets a boolean indicating whether filter chain builder benchmarks should be included in the suite. When `true`, benchmarks related to constructing and validating filter pipelines are executed.

### `IncludeImageUtilitiesBenchmarks`
Gets or sets a boolean indicating whether image utilities benchmarks should be included in the suite. When `true`, benchmarks related to image loading, saving, and metadata handling are executed.

### `IncludeEnumerableExtensionsBenchmarks`
Gets or sets a boolean indicating whether enumerable extension method benchmarks should be included in the suite. When `true`, benchmarks related to LINQ-like operations on image data structures are executed.

### `AccuracyLevel`
Gets or sets the benchmark accuracy level, which controls the trade-off between execution time and measurement precision. Higher levels may increase runtime but provide more stable and detailed results.

### `OutputDirectory`
Gets or sets the directory where benchmark results and logs will be written. If `null`, results may be written to a default location or not persisted at all.

### `EnableHardwareCounters`
Gets or sets a boolean indicating whether low-level hardware performance counters (e.g., CPU cycles, cache misses) should be collected during benchmark execution. Enabling this may increase overhead and require elevated permissions.

### `Validate`
Gets a read-only list of validation rules or checks to apply before running benchmarks. These may include preconditions on system state, environment variables, or data availability.

### `GetEnabledCategories`
Returns a read-only list of the enabled benchmark categories based on the current configuration flags. The list contains identifiers for each active benchmark group (e.g., "FilterChain", "BatchProcessing").

### `ForCi`
A static factory method that returns a `BenchmarkSuiteConfiguration` optimized for Continuous Integration environments. It disables non-essential benchmarks, sets a strict accuracy level, and configures output for automated analysis.

### `ForRelease`
A static factory method that returns a `BenchmarkSuiteConfiguration` suitable for performance profiling and release validation. It enables all benchmark categories, uses a high accuracy level, and configures output for detailed reporting.

## Usage
