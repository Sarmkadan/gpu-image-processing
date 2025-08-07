# GpuExceptionExtensions

Provides a set of extension methods for diagnosing and formatting GPU‑related exceptions thrown by the GPU image processing library. These helpers allow callers to quickly determine the category of a failure and produce a consistent log‑friendly representation without exposing internal exception details.

## API

### IsTimeoutError
```csharp
public static bool IsTimeoutError(this GpuException ex)
```
**Purpose** – Returns `true` if the supplied exception indicates that a GPU operation exceeded its allotted time limit.  
**Parameters**  
- `ex`: The `GpuException` instance to inspect. Must not be `null`.  
**Return value** – `true` when the error originates from a timeout condition; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### IsMemoryError
```csharp
public static bool IsMemoryError(this GpuException ex)
```
**Purpose** – Returns `true` if the supplied exception signals that the GPU ran out of memory or encountered an allocation failure.  
**Parameters**  
- `ex`: The `GpuException` instance to inspect. Must not be `null`.  
**Return value** – `true` when the error is memory‑related; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### IsComputePipelineError
```csharp
public static bool IsComputePipelineError(this GpuException ex)
```
**Purpose** – Returns `true` if the supplied exception stems from a problem in the GPU compute pipeline (e.g., shader compilation failure, dispatch error, or invalid pipeline state).  
**Parameters**  
- `ex`: The `GpuException` instance to inspect. Must not be `null`.  
**Return value** – `true` when the error is compute‑pipeline related; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

### FormatForLogging
```csharp
public static string FormatForLogging(this GpuException ex)
```
**Purpose** – Produces a single‑line, machine‑readable string suitable for inclusion in logs. The format includes the exception type, message, and any relevant error codes without exposing stack traces or sensitive data.  
**Parameters**  
- `ex`: The `GpuException` instance to format. Must not be `null`.  
**Return value** – A formatted string ready for logging.  
**Exceptions** – Throws `ArgumentNullException` if `ex` is `null`.

## Usage

### Checking error categories
```csharp
try
{
    processor.Run(image);
}
catch (GpuException gpuEx)
{
    if (gpuEx.IsTimeoutError())
    {
        // Handle timeout – maybe retry with a longer deadline.
        Log.Warn("GPU operation timed out: {Msg}", gpuEx.Message);
    }
    else if (gpuEx.IsMemoryError())
    {
        // Handle out‑of‑memory – reduce workload or allocate more resources.
        Log.Error("GPU out of memory: {Msg}", gpuEx.Message);
    }
    else if (gpuEx.IsComputePipelineError())
    {
        // Handle pipeline issues – possibly recompile shaders.
        Log.Error("GPU compute pipeline failure: {Msg}", gpuEx.Message);
    }
    else
    {
        Log.Error("Unexpected GPU error: {Msg}", gpuEx.Message);
    }
}
```

### Logging a GPU exception
```csharp
try
{
    filter.Apply(frame);
}
catch (GpuException gpuEx)
{
    string logEntry = gpuEx.FormatForLogging();
    // logEntry might look like: "[GpuException] Code=0x8007000E Msg=Out of memory"
    Logger.Info(logEntry);
}
```

## Notes
- All extension methods are **pure** with respect to the supplied exception; they do not modify the exception instance.  
- The methods are **thread‑safe** because they only read immutable data from the exception object and perform no internal state changes. Concurrent calls on different `GpuException` instances are safe.  
- Passing `null` to any of these members results in an `ArgumentNullException`; callers should ensure the exception reference is valid before invoking the helpers.  
- The string returned by `FormatForLogging` is intended for textual logs only; it is not guaranteed to be parseable by automated tools and may evolve across library versions.  
- These helpers do not swallow exceptions; they merely inspect or format the provided instance. Proper exception handling (e.g., `try/catch`) remains the responsibility of the caller.
