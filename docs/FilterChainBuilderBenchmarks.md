# FilterChainBuilderBenchmarks

A benchmarking utility for evaluating the performance and correctness of `FilterChain` construction in GPU-accelerated image processing pipelines. It measures the time and memory characteristics of building filter chains of varying complexity, enabling comparison of different optimization strategies or hardware configurations.

## API

### `public void Setup()`
Initializes the benchmark environment. Must be called before any benchmark method to ensure a clean state. This includes allocating GPU resources, resetting internal timers, and preparing any required test data.

- **Parameters**: None
- **Return value**: None
- **Throws**: May throw if GPU initialization fails or required resources cannot be allocated.

---

### `public FilterChain Build_ThreeStep()`
Constructs and returns a `FilterChain` composed of exactly three enabled filter steps. Used to measure the baseline cost of constructing small, representative pipelines.

- **Parameters**: None
- **Return value**: A newly constructed `FilterChain` instance with three enabled steps.
- **Throws**: May throw if any filter step fails to initialize or if the chain cannot be assembled.

---

### `public FilterChain Build_TenStep()`
Constructs and returns a `FilterChain` composed of exactly ten enabled filter steps. Used to evaluate the scalability of chain construction under moderate load.

- **Parameters**: None
- **Return value**: A newly constructed `FilterChain` instance with ten enabled steps.
- **Throws**: May throw if any filter step fails to initialize or if the chain cannot be assembled.

---
### `public bool Validate_TenStep()`
Validates the correctness of a ten-step filter chain by executing a predefined test image through the pipeline and comparing the output against an expected result. Ensures functional correctness in addition to performance.

- **Parameters**: None
- **Return value**: `true` if the output matches the expected result; otherwise, `false`.
- **Throws**: May throw if the chain has not been built or if GPU execution fails.

---
### `public FilterChain Clone_TenStep()`
Creates a deep copy of a ten-step filter chain. Used to evaluate the cost and correctness of chain duplication, which is common in image processing workflows involving branching or reuse.

- **Parameters**: None
- **Return value**: A new `FilterChain` instance that is a deep copy of the original.
- **Throws**: May throw if memory allocation fails or if any step cannot be cloned.

---
### `public double EstimateTotalProcessingTime()`
Estimates the total processing time (in milliseconds) for the most recently built chain by summing the estimated execution times of all enabled steps. Provides a quick performance indicator without full execution.

- **Parameters**: None
- **Return value**: The estimated total processing time in milliseconds.
- **Throws**: May throw if the chain has not been built or if any step lacks timing data.

---
### `public System.Collections.Generic.List<FilterStep> GetEnabledSteps()`
Retrieves a list of all enabled filter steps in the most recently built chain. Useful for inspection and validation of chain composition.

- **Parameters**: None
- **Return value**: A `List<FilterStep>` containing all enabled steps in the order they appear in the chain.
- **Throws**: May throw if the chain has not been built.

## Usage

### Example 1: Benchmarking chain construction
