# ComputeShaderPipelineOptions

Configuration options for compute shader pipeline execution, including workgroup sizing, memory limits, validation, and profiling behavior.

## API

### `public WorkgroupOptimizationStrategy DefaultStrategy`
Determines the default strategy used to optimize workgroup sizes when not explicitly specified. Defaults to `WorkgroupOptimizationStrategy.Balanced`.

### `public int MaxWorkgroupDimension`
Maximum allowed dimension for a workgroup in any pipeline stage. Must be a positive integer. Defaults to `1024`.

### `public bool BenchmarkGuidedOptimization`
Enables runtime benchmarking to guide workgroup size selection. When `true`, the pipeline may override `DefaultStrategy` based on empirical performance data. Defaults to `false`.

### `public bool EnableProfiling`
Includes detailed timing and resource usage metrics in pipeline execution reports when `true`. Defaults to `false`.

### `public int MaxPipelineDepth`
Maximum number of nested compute shader dispatches allowed in a single pipeline execution. Must be a non-negative integer. Defaults to `8`.

### `public int DefaultLocalMemoryPerThreadBytes`
Default amount of local (shared) memory allocated per thread, in bytes. Must be a non-negative integer. Defaults to `32768`.

### `public double OccupancyWarningThreshold`
Fractional threshold (0.0 to 1.0) below which the pipeline logs a warning if compute shader occupancy falls below this value. Defaults to `0.75`.

### `public bool Validate`
Enables input validation and consistency checks during pipeline setup and execution. Defaults to `true`.

### `public static IServiceCollection AddComputeShaderPipeline(IServiceCollection services)`
Registers compute shader pipeline services with the dependency injection container. Returns the input `services` for method chaining.

**Parameters:**
- `services`: The `IServiceCollection` to configure.

**Returns:**
- The configured `IServiceCollection`.

**Throws:**
- `ArgumentNullException`: If `services` is `null`.

### `public static IServiceCollection AddComputeShaderPipeline(IServiceCollection services, Action<ComputeShaderPipelineOptions> configureOptions)`
Registers compute shader pipeline services and applies custom configuration.

**Parameters:**
- `services`: The `IServiceCollection` to configure.
- `configureOptions`: Action delegate to configure pipeline options.

**Returns:**
- The configured `IServiceCollection`.

**Throws:**
- `ArgumentNullException`: If `services` or `configureOptions` is `null`.

### `public static IServiceProvider LogComputeShaderPipelineSettings(IServiceProvider provider)`
Logs the current compute shader pipeline configuration to the configured logger. Returns the input `provider` for method chaining.

**Parameters:**
- `provider`: The `IServiceProvider` containing configured services.

**Returns:**
- The input `IServiceProvider`.

**Throws:**
- `ArgumentNullException`: If `provider` is `null`.

## Usage

### Example 1: Basic Configuration
