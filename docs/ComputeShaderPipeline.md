# ComputeShaderPipeline

A high-level abstraction for executing GPU compute shaders with built-in workgroup optimization, execution tracking, and statistics collection. It manages shader resource binding, dispatch configuration, and asynchronous execution while providing feedback on performance characteristics and execution results.

## API

### `ComputeShaderPipeline`
Initializes a new compute shader pipeline instance for a given compute shader asset. The shader must be pre-compiled and compatible with the runtime environment.

### `async Task<PipelineExecutionResult> ExecuteAsync`
Executes the compute shader pipeline asynchronously with the current workgroup configuration. Returns a `PipelineExecutionResult` containing execution status, timing metrics, and optional error information.

- **Parameters**: None
- **Return Value**: `Task<PipelineExecutionResult>` – A task that resolves to a `PipelineExecutionResult` describing the outcome of the execution.
- **Exceptions**: Throws `InvalidOperationException` if the shader is not loaded or the pipeline is in an invalid state. Throws `ArgumentNullException` if required resources are missing.

### `async Task<WorkgroupConfiguration> OptimizeWorkgroupAsync`
Analyzes shader resource usage and hardware capabilities to compute an optimal workgroup size. This may involve multiple test dispatches and heuristic evaluation.

- **Parameters**: None
- **Return Value**: `Task<WorkgroupConfiguration>` – A task that resolves to the optimized `WorkgroupConfiguration` for the current shader.
- **Exceptions**: Throws `InvalidOperationException` if the shader is not loaded or optimization cannot proceed.

### `Task<PipelineStatistics> GetStatisticsAsync`
Retrieves accumulated performance statistics for all prior executions of the pipeline. Includes dispatch counts, execution times, and memory usage.

- **Parameters**: None
- **Return Value**: `Task<PipelineStatistics>` – A task that resolves to a `PipelineStatistics` object with cumulative metrics.
- **Exceptions**: Throws `InvalidOperationException` if statistics tracking is disabled or unavailable.

### `Task ResetStatisticsAsync`
Resets all accumulated statistics to zero, clearing historical performance data. Useful for isolating measurements across test runs.

- **Parameters**: None
- **Return Value**: `Task` – A task that completes when statistics have been reset.
- **Exceptions**: Throws `InvalidOperationException` if statistics tracking is not supported.

## Usage
