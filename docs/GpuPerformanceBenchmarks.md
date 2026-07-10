# GpuPerformanceBenchmarks

Provides a benchmark harness for GPU‑accelerated image‑processing filters in the **gpu-image-processing** project. The class configures image dimensions, allocates GPU resources, exposes asynchronous methods to apply individual filters or filter chains, and reports performance‑related metrics such as pixel data size and estimated memory footprints for common resolutions.

## API

### ImageWidth  
**Purpose:** Gets or sets the width (in pixels) of the test image used by all benchmark operations.  
**Parameters:** None.  
**Return value:** `int` – the current width.  
**Throws:**  
- `ArgumentOutOfRangeException` if a value less than 1 is assigned.

### ImageHeight  
**Purpose:** Gets or sets the height (in pixels) of the test image used by all benchmark operations.  
**Parameters:** None.  
**Return value:** `int` – the current height.  
**Throws:**  
- `ArgumentOutOfRangeException` if a value less than 1 is assigned.

### Setup  
**Purpose:** Initializes internal GPU resources, creates the test image based on `ImageWidth` and `ImageHeight`, and prepares the benchmark environment. Must be called before any filter‑application or measurement methods.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:**  
- `InvalidOperationException` if called after `Cleanup` without a subsequent `Setup`.  
- `ExternalException` if GPU initialization fails (e.g., no compatible device found).

### Cleanup  
**Purpose:** Releases all GPU resources allocated by `Setup` and resets the benchmark state. After calling this method, further invocations of filter methods will throw.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:**  
- `ObjectDisposedException` if called more than once without an intervening `Setup`.

### ApplyGaussianBlurFilter  
**Purpose:** Asynchronously applies a Gaussian blur filter to the current test image and returns the processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>` – contains timing information and the resulting image data.  
**Throws:**  
- `InvalidOperationException` if `Setup` has not been called.  
- `ObjectDisposedException` if `Cleanup` has been called.  
- `ExternalException` if the GPU kernel execution fails.

### ApplyEdgeDetectionFilter  
**Purpose:** Asynchronously applies an edge detection filter (e.g., Sobel) to the current test image and returns the processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>`.  
**Throws:** Same as `ApplyGaussianBlurFilter`.

### ApplySharpenFilter  
**Purpose:** Asynchronously applies a sharpening filter to the current test image and returns the processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>`.  
**Throws:** Same as `ApplyGaussianBlurFilter`.

### ApplyGrayscaleFilter  
**Purpose:** Asynchronously converts the current test image to grayscale and returns the processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>`.  
**Throws:** Same as `ApplyGaussianBlurFilter`.

### ApplyCustomConvolutionFilter  
**Purpose:** Asynchronously applies a user‑defined convolution kernel (provided elsewhere in the test harness) to the current test image and returns the processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>`.  
**Throws:** Same as `ApplyGaussianBlurFilter`.

### ApplyThreeFilterChain  
**Purpose:** Asynchronously applies a predefined chain of three filters (e.g., grayscale → blur → edge detect → sharpen) to the current test image and returns the aggregated processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>`.  
**Throws:** Same as `ApplyGaussianBlurFilter`.

### ApplyFiveFilterChain  
**Purpose:** Asynchronously applies a predefined chain of five filters to the current test image and returns the aggregated processing result.  
**Parameters:** None.  
**Return value:** `Task<ProcessingResult>`.  
**Throws:** Same as `ApplyGaussianBlurFilter`.

### CalculatePixelDataSize  
**Purpose:** Computes the size in bytes of the raw pixel buffer for the currently configured image dimensions (assuming 4 bytes per pixel, RGBA8).  
**Parameters:** None.  
**Return value:** `long` – number of bytes required for the image data.  
**Throws:**  
- `InvalidOperationException` if `ImageWidth` or `ImageHeight` have not been set to positive values.

### MemoryFootprint1080p  
**Purpose:** Gets the estimated GPU memory footprint (in bytes) required to process a 1920×1080 image with the current filter configuration.  
**Parameters:** None.  
**Return value:** `long`.  
**Throws:** None (value is derived from constants and current settings).

### MemoryFootprint4K  
**Purpose:** Gets the estimated GPU memory footprint (in bytes) required to process a 3840×2160 image with the current filter configuration.  
**Parameters:** None.  
**Return value:** `long`.  
**Throws:** None.

### ProcessTenImages  
**Purpose:** Asynchronously processes ten consecutive images using the default benchmark workload (typically a filter chain) and returns when all iterations have completed. Useful for measuring sustained throughput.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:**  
- `InvalidOperationException` if `Setup` has not been called.  
- `ObjectDisposedException` if `Cleanup` has been called.  
- `ExternalException` if any iteration fails on the GPU.

### GetBestDevice  
**Purpose:** Asynchronously queries the system for the GPU device that offers the highest expected performance for the current workload and returns it, or `null` if no suitable device is found.  
**Parameters:** None.  
**Return value:** `Task<GpuDevice?>` – the selected device descriptor or `null`.  
**Throws:**  
- `InvalidOperationException` if called before `Setup`.  
- `ExternalException` if device enumeration fails.

### CreateServiceProvider  
**Purpose:** Static factory method that builds and configures an `IServiceProvider` instance with the dependencies required by `GpuPerformanceBenchmarks` (e.g., GPU context factories, logger).  
**Parameters:** None.  
**Return value:** `IServiceProvider` – ready for use with dependency injection.  
**Throws:**  
- `InvalidOperationException` if required services cannot be resolved.

## Usage

### Example 1: Basic filter benchmark

```csharp
using System.Threading.Tasks;
using GpuImageProcessing.Benchmarks;
using Microsoft.Extensions.DependencyInjection;

// Build the service provider (registers GPU context, logger, etc.)
IServiceProvider services = GpuPerformanceBenchmarks.CreateServiceProvider();

var bench = new GpuPerformanceBenchmarks
{
    ImageWidth = 1920,
    ImageHeight = 1080
};

// Resolve any needed services via the provider (omitted for brevity)
bench.Setup();

try
{
    // Apply a Gaussian blur and capture the result
    ProcessingResult result = await bench.ApplyGaussianBlurFilter();

    Console.WriteLine($"Blur took {result.ElapsedMilliseconds} ms");
    Console.WriteLine($"Output size: {result.OutputByteLength} bytes");
}
finally
{
    bench.Cleanup();
}
```

### Example 2: Selecting the best GPU and running a multi‑image workload

```csharp
using System.Threading.Tasks;
using GpuImageProcessing.Benchmarks;
using Microsoft.Extensions.DependencyInjection;

IServiceProvider services = GpuPerformanceBenchmarks.CreateServiceProvider();

var bench = new GpuPerformanceBenchmarks
{
    ImageWidth = 3840,
    ImageHeight = 2160
};

bench.Setup();

try
{
    // Choose the most suitable GPU for the current configuration
    GpuDevice? best = await bench.GetBestDevice();
    if (best != null)
    {
        Console.WriteLine($"Selected device: {best.Name} (ID: {best.Id})");
    }

    // Process ten images using the default benchmark workload
    await bench.ProcessTenImages();
    Console.WriteLine("Ten-image batch completed.");
}
finally
{
    bench.Cleanup();
}
```

## Notes

- **State dependence:** All filter‑application methods (`Apply*Filter`, `ProcessTenImages`) require a successful call to `Setup` beforehand. Invoking them before `Setup` or after `Cleanup` results in `InvalidOperationException` or `ObjectDisposedException`, respectively.  
- **Image dimensions:** `ImageWidth` and `ImageHeight` must be set to positive values before calling `Setup`. Changing these properties after `Setup` has no effect on the already‑allocated buffers; to use new dimensions, call `Cleanup` then set the properties and invoke `Setup` again.  
- **Memory footprint properties:** `MemoryFootprint1080p` and `MemoryFootprint4K` return estimates based on the current filter chain configuration and assume RGBA8 pixel format. They do not reflect actual allocation after `Setup`; they are intended for quick comparative analysis.  
- **Thread safety:** The class is **not** thread‑safe. Concurrent calls to `Setup`, `Cleanup`, or any filter method from multiple threads may lead to race conditions, double‑initialization, or premature resource disposal. If parallel benchmarks are required, instantiate a separate `GpuPerformanceBenchmarks` object per thread or synchronize access externally.  
- **Error handling:** GPU‑related failures surface as `ExternalException`. Callers should inspect the exception’s `Message` or inner details for diagnostics (e.g., driver incompatibility, out‑of‑memory).  
- **Static factory:** `CreateServiceProvider` does not depend on instance state and can be invoked once per application to share services across multiple benchmark instances. Disposing of the returned `IServiceProvider` is the responsibility of the caller.
