# FilterChainBenchmarks

A benchmarking utility for evaluating the performance and correctness of filter chains in GPU-accelerated image processing pipelines. This class measures throughput, latency, and functional validation of filter sequences to identify bottlenecks and ensure consistency across different configurations.

## API

### `void Setup()`
Initializes the benchmarking environment, including GPU resources, filter chain configurations, and measurement instrumentation. This method must be called before any benchmarking operations.

- **Parameters**: None
- **Return value**: None
- **Throws**: `InvalidOperationException` if called after benchmarking has started or if GPU resources cannot be initialized.

---

### `FilterChain AddStep_TenFilters()`
Constructs a filter chain consisting of ten preconfigured filter steps and returns the chain for benchmarking. The chain is populated with default filter parameters optimized for benchmarking scenarios.

- **Parameters**: None
- **Return value**: A `FilterChain` instance containing ten enabled filter steps.
- **Throws**: None

---

### `List<FilterStep> GetEnabledSteps_TenSteps()`
Retrieves the list of enabled filter steps from a preconfigured ten-step filter chain. Useful for validating the state of the chain before or after benchmarking.

- **Parameters**: None
- **Return value**: A `List<FilterStep>` containing the enabled steps in execution order.
- **Throws**: `NullReferenceException` if the internal chain reference is null (e.g., if `AddStep_TenFilters` was not called).

---
### `bool Validate_TenSteps()`
Performs a functional validation of the ten-step filter chain by processing a known test image and comparing the output against an expected reference. Returns `true` if the output matches the reference within a configurable tolerance.

- **Parameters**: None
- **Return value**: `true` if validation passes; otherwise, `false`.
- **Throws**: `InvalidOperationException` if the chain has not been initialized or if the test image cannot be loaded.

---
### `int GetEnabledFilterCount()`
Returns the number of enabled filter steps in the current chain configuration. This count reflects the active filters that will be executed during benchmarking.

- **Parameters**: None
- **Return value**: An `int` representing the number of enabled filters.
- **Throws**: `NullReferenceException` if the internal chain reference is null.

---
### `FilterChain Clone_TenStepChain()`
Creates a deep copy of the ten-step filter chain, including all filter parameters and enabled states. The clone is independent of the original and can be safely modified without affecting benchmarking results.

- **Parameters**: None
- **Return value**: A new `FilterChain` instance with identical configuration.
- **Throws**: `NullReferenceException` if the internal chain reference is null.

## Usage

### Example 1: Basic Benchmarking Workflow
