# FilterServiceExtensions

Provides extension methods for configuring and applying image‑processing filters on the GPU. The methods encapsulate common filter creation, activation, and retrieval operations, allowing callers to work with `FilterConfiguration` objects asynchronously without managing low‑level GPU resources directly.

## API

### CreateGrayscaleFilterAsync
- **Purpose:** Creates a filter configuration that converts an image to grayscale.
- **Parameters:** None.
- **Return Value:** `Task<FilterConfiguration>` representing the grayscale filter configuration.
- **When it throws:** May throw a `GpuException` if the GPU device is unavailable or if there is insufficient memory to allocate the filter resources.

### CreateBlurFilterAsync
- **Purpose:** Creates a filter configuration that applies a blur effect.
- **Parameters:** None.
- **Return Value:** `Task<FilterConfiguration>` representing the blur filter configuration.
- **When it throws:** May throw a `GpuException` for GPU allocation failures or an `InvalidOperationException` if the underlying filter pipeline is not initialized.

### CreateSharpenFilterAsync
- **Purpose:** Creates a filter configuration that sharpens an image.
- **Parameters:** None.
- **Return Value:** `Task<FilterConfiguration>` representing the sharpen filter configuration.
- **When it throws:** May throw a `GpuException` when the GPU cannot compile the sharpen kernel, or an `ArgumentException` if any required internal parameters are out of range.

### GetActiveFiltersAsync
- **Purpose:** Retrieves the list of filter configurations that are currently active in the processing pipeline.
- **Parameters:** None.
- **Return Value:** `Task<IReadOnlyList<FilterConfiguration>>` containing the active filters.
- **When it throws:** May throw a `GpuException` if the pipeline state cannot be read, or an `ObjectDisposedException` if the filter service has been disposed.

### ApplyFiltersAsync
- **Purpose:** Applies all active filter configurations to a supplied image asynchronously.
- **Parameters:** None (the method operates on the image bound to the filter service instance).
- **Return Value:** `Task` that completes when the filters have been applied.
- **When it throws:** May throw a `GpuException` for execution errors on the GPU, or a `NullReferenceException` if no image has been associated with the service.

### CreateConvolutionFilterAsync
- **Purpose:** Creates a filter configuration based on a user‑defined convolution kernel.
- **Parameters:** None.
- **Return Value:** `Task<FilterConfiguration>` representing the convolution filter.
- **When it throws:** May throw a `GpuException` if the kernel size is unsupported or if GPU memory allocation fails, or an `ArgumentException` if the kernel data is malformed.

### GetFiltersByTypeAsync
- **Purpose:** Returns all filter configurations of a specific type (e.g., blur, sharpen) that have been created.
- **Parameters:** None.
- **Return Value:** `Task<IReadOnlyList<FilterConfiguration>>` matching the requested filter type.
- **When it throws:** May throw a `GpuException` if the internal filter catalog cannot be accessed, or a `KeyNotFoundException` if the requested type is unknown.

### FindFilterByNameAsync
- **Purpose:** Looks up a filter configuration by its user‑assigned name.
- **Parameters:** None.
- **Return Value:** `Task<FilterConfiguration?>` containing the filter if found, otherwise `null`.
- **When it throws:** May throw a `GpuException` for internal lookup failures, or an `ArgumentException` if the supplied name is invalid (e.g., null or empty).

### ActivateFilterAsync
- **Purpose:** Activates a previously created filter configuration so that it participates in subsequent image processing.
- **Parameters:** None.
- **Return Value:** `Task<FilterConfiguration>` representing the activated filter.
- **When it throws:** May throw a `GpuException` if activation would exceed GPU resource limits, or an `InvalidOperationException` if the filter is already active.

### DeactivateFilterAsync
- **Purpose:** Deactivates a filter, removing it from the active processing pipeline.
- **Parameters:** None.
- **Return Value:** `Task<bool>` indicating whether the filter was successfully deactivated (`true`) or was not found (`false`).
- **When it throws:** May throw a `GpuException` if the GPU state cannot be updated, or an `ObjectDisposedException` if the filter service has been disposed.

## Usage

```csharp
// Example 1: Create and activate a grayscale filter, then apply it.
var grayConfig = await FilterServiceExtensions.CreateGrayscaleFilterAsync();
await FilterServiceExtensions.ActivateFilterAsync();
await FilterServiceExtensions.ApplyFiltersAsync(); // processes the bound image with grayscale
```

```csharp
// Example 2: Retrieve all blur filters, deactivate a specific one by name.
var blurFilters = await FilterServiceExtensions.GetFiltersByTypeAsync();
// Assume blurFilters contains at least one filter with a known name "MyBlur"
var blurToRemove = await FilterServiceExtensions.FindFilterByNameAsync();
if (blurToRemove != null)
{
    var success = await FilterServiceExtensions.DeactivateFilterAsync();
    // success will be true if the filter was found and deactivated
}
```

## Notes

- All methods are asynchronous and should be awaited; calling them without `await` may lead to unobserved exceptions.
- The extension methods assume that a valid GPU context and an associated source image have been set up elsewhere in the `FilterService` instance; attempting to invoke any method before this initialization will typically result in a `GpuException` or `InvalidOperationException`.
- The service is not thread‑safe for concurrent modification of the filter pipeline (e.g., calling `ActivateFilterAsync` and `DeactivateFilterAsync` from multiple threads simultaneously). External synchronization is required if the same `FilterService` instance is accessed from multiple threads.
- Returned `FilterConfiguration` instances are immutable after creation; however, the underlying GPU resources they reference are released only when the filter is deactivated or the service is disposed.
- Passing invalid data (e.g., null names, malformed kernels) will result in argument‑validation exceptions before any GPU work is attempted.
