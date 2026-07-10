# FilterChainBuilder

The `FilterChainBuilder` class implements the builder pattern to construct a `FilterChain` instance for GPU-accelerated image processing. It provides a fluent interface for configuring chain metadata, execution behavior, and adding a sequence of image filters. After all desired filters and settings are specified, the `Build` method produces an immutable `FilterChain` ready for execution.

## API

### `public static FilterChainBuilder Create()`
Creates a new `FilterChainBuilder` with default settings.  
**Returns:** A new `FilterChainBuilder` instance.

### `public FilterChainBuilder WithDescription(string description)`
Sets a human-readable description for the filter chain.  
**Parameters:**  
- `description` – A string describing the chain’s purpose.  
**Returns:** The same builder instance for chaining.  
**Throws:** `ArgumentNullException` if `description` is `null`.

### `public FilterChainBuilder WithExecutionOrder(ExecutionOrder order)`
Specifies the order in which filters are applied relative to one another.  
**Parameters:**  
- `order` – An `ExecutionOrder` value (e.g., `Sequential`, `Parallel`).  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AllowParallelExecution(bool allow)`
Controls whether the chain may execute filters in parallel when possible.  
**Parameters:**  
- `allow` – `true` to permit parallel execution; `false` to force sequential execution.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder CacheIntermediates(bool cache)`
Enables or disables caching of intermediate results between filters.  
**Parameters:**  
- `cache` – `true` to cache intermediate images; `false` to discard them.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddGrayscale()`
Appends a grayscale conversion filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddBlur()`
Appends a Gaussian blur filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddSharpen()`
Appends a sharpening filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddEdgeDetection()`
Appends an edge detection filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddColorCorrection()`
Appends a color correction filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddThreshold()`
Appends a thresholding filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddRotation()`
Appends a rotation filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddScaling()`
Appends a scaling filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddBilateral()`
Appends a bilateral filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddMedian()`
Appends a median filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddEmboss()`
Appends an emboss filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddSobel()`
Appends a Sobel edge detection filter to the chain.  
**Returns:** The same builder instance for chaining.

### `public FilterChainBuilder AddCustomFilter(IFilter filter)`
Appends a user-defined filter to the chain.  
**Parameters:**  
- `filter` – An object implementing the `IFilter` interface.  
**Returns:** The same builder instance for chaining.  
**Throws:** `ArgumentNullException` if `filter` is `null`.

### `public FilterChain Build()`
Constructs and returns the final `FilterChain` based on the current builder configuration.  
**Returns:** A new `FilterChain` instance.  
**Throws:** `InvalidOperationException` if the configuration is invalid (e.g., no filters added, conflicting execution settings).

## Usage

### Example 1: Basic chain with grayscale and blur

```csharp
using GpuImageProcessing;

FilterChain chain = FilterChainBuilder
    .Create()
    .WithDescription("Basic grayscale + blur")
    .AddGrayscale()
    .AddBlur()
    .Build();
```

### Example 2: Advanced chain with parallel execution, caching, and custom filter

```csharp
using GpuImageProcessing;

var customFilter = new MyCustomFilter();

FilterChain chain = FilterChainBuilder
    .Create()
    .WithDescription("Advanced pipeline")
    .WithExecutionOrder(ExecutionOrder.Sequential)
    .AllowParallelExecution(true)
    .CacheIntermediates(true)
    .AddColorCorrection()
    .AddSharpen()
    .AddEdgeDetection()
    .AddCustomFilter(customFilter)
    .Build();
```

## Notes

- The builder is **not thread-safe**. All method calls should be made from a single thread.
- Calling `Build()` multiple times on the same builder instance will produce separate `FilterChain` objects, each reflecting the current configuration at the time of the call.
- If no filters are added before `Build()` is called, an `InvalidOperationException` is thrown.
- Adding the same filter type multiple times is allowed; the chain will apply them in the order they were added.
- The `ExecutionOrder` setting only affects filters that can be reordered; some filters may have implicit dependencies that override this setting.
