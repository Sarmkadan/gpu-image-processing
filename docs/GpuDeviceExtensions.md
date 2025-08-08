# GpuDeviceExtensions

`GpuDeviceExtensions` provides a set of static convenience methods for querying properties and capabilities of GPU devices in the `gpu-image-processing` library. These extension methods allow callers to retrieve memory statistics, check color space support, and obtain human-readable device type names without needing to interact with lower-level device inspection APIs.

## API

### GetTotalMemoryMb

```csharp
public static long GetTotalMemoryMb(this GpuDevice device)
```

Returns the total physical memory available on the GPU device, expressed in megabytes. This value represents the full dedicated video memory capacity, not accounting for current allocations or system-shared memory pools.

**Parameters:**
- `device` — The `GpuDevice` instance to query.

**Return Value:**
A `long` representing total GPU memory in megabytes. Returns `0` if the device does not expose memory information or is a software rasterizer.

**Exceptions:**
- `ArgumentNullException` — Thrown when `device` is `null`.
- `GpuException` — Thrown when the underlying device handle is invalid or has been disposed.

---

### GetAvailableMemoryMb

```csharp
public static long GetAvailableMemoryMb(this GpuDevice device)
```

Returns an estimate of currently available (free) GPU memory in megabytes. This value is a snapshot at the time of the call and may change rapidly under concurrent workloads.

**Parameters:**
- `device` — The `GpuDevice` instance to query.

**Return Value:**
A `long` representing available GPU memory in megabytes. Returns `0` if memory querying is unsupported or the device is a software rasterizer.

**Exceptions:**
- `ArgumentNullException` — Thrown when `device` is `null`.
- `GpuException` — Thrown when the underlying device handle is invalid or has been disposed.

---

### SupportsColorSpace

```csharp
public static bool SupportsColorSpace(this GpuDevice device, ColorSpace colorSpace)
```

Determines whether the specified color space is natively supported by the GPU device for image processing operations. Support implies that textures and shader operations can use the color space without emulation or software fallback.

**Parameters:**
- `device` — The `GpuDevice` instance to query.
- `colorSpace` — A `ColorSpace` enumeration value to test (e.g., `Linear`, `SRgb`, `Hdr10`).

**Return Value:**
`true` if the device reports hardware or driver-level support for the color space; `false` otherwise, including when the device is a software rasterizer.

**Exceptions:**
- `ArgumentNullException` — Thrown when `device` is `null`.
- `GpuException` — Thrown when the underlying device handle is invalid or has been disposed.

---

### GetDeviceTypeDisplayName

```csharp
public static string GetDeviceTypeDisplayName(this GpuDevice device)
```

Returns a human-readable string describing the type of GPU device, suitable for logging, diagnostics, or user-facing UI elements. The returned name is localized based on the current thread's culture.

**Parameters:**
- `device` — The `GpuDevice` instance to query.

**Return Value:**
A non-null, non-empty `string` such as `"Discrete GPU"`, `"Integrated GPU"`, `"Software Rasterizer"`, or `"External GPU"`.

**Exceptions:**
- `ArgumentNullException` — Thrown when `device` is `null`.
- `GpuException` — Thrown when the underlying device handle is invalid or has been disposed.

## Usage

### Example 1: Logging Device Capabilities on Startup

```csharp
using GpuImageProcessing;

void LogDeviceInfo(GpuDevice device)
{
    string deviceType = device.GetDeviceTypeDisplayName();
    long totalMem = device.GetTotalMemoryMb();
    long availMem = device.GetAvailableMemoryMb();
    bool supportsLinear = device.SupportsColorSpace(ColorSpace.Linear);

    Console.WriteLine($"Device: {deviceType}");
    Console.WriteLine($"Memory: {availMem} MB available / {totalMem} MB total");
    Console.WriteLine($"Linear color space: {(supportsLinear ? "Supported" : "Not supported")}");
}
```

### Example 2: Selecting a Processing Pipeline Based on Color Space Support

```csharp
using GpuImageProcessing;

IProcessingPass SelectColorConversionPass(GpuDevice device, ColorSpace sourceSpace, ColorSpace targetSpace)
{
    bool sourceSupported = device.SupportsColorSpace(sourceSpace);
    bool targetSupported = device.SupportsColorSpace(targetSpace);

    if (sourceSupported && targetSupported)
    {
        return new GpuColorConversionPass(device, sourceSpace, targetSpace);
    }

    // Fall back to a CPU-based pass when GPU lacks native support
    return new CpuColorConversionPass(sourceSpace, targetSpace);
}
```

## Notes

- **Memory values are snapshots:** `GetAvailableMemoryMb` reflects the state at call time. Under concurrent GPU workloads, the available memory may decrease between the query and a subsequent allocation attempt. Always check for allocation failures rather than relying solely on this value as a guarantee.
- **Zero return values:** Both memory methods return `0` for software rasterizers and devices whose drivers do not expose memory querying interfaces. Callers should treat `0` as "unknown" rather than assuming the device has no memory.
- **Color space granularity:** `SupportsColorSpace` reports driver-level support. Some color spaces may be partially supported (e.g., via emulation on older hardware). A `false` return indicates no native path exists; a `true` return does not guarantee optimal performance.
- **Thread safety:** All methods are static and thread-safe. They do not mutate device state. However, calling these methods on a `GpuDevice` that is concurrently being disposed by another thread may result in a `GpuException`.
- **Localization:** `GetDeviceTypeDisplayName` respects `CultureInfo.CurrentUICulture`. If consistent output is required across locales (e.g., for log parsing), consider using the invariant culture or storing the raw device type enum separately.
