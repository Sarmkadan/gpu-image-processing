# GpuException

The `GpuException` class represents errors that occur during GPU-accelerated image processing operations within the `gpu-image-processing` library. It extends standard exception handling by capturing specific hardware context, including the identifier of the failing GPU device, a vendor-specific or internal error code, and the precise timestamp of the failure, facilitating diagnostics in multi-GPU or distributed rendering environments.

## API

### `DeviceName`
```csharp
public string? DeviceName { get; }
```
Retrieves the human-readable name of the GPU device where the exception originated. This property returns `null` if the device identifier could not be resolved at the time of the error or if the failure occurred before device initialization.

### `ErrorCode`
```csharp
public int? ErrorCode { get; }
```
Gets the numeric error code associated with the GPU failure. This value typically corresponds to vendor-specific status codes (e.g., CUDA error codes, DirectX HRESULTs) or internal library state codes. It returns `null` if no specific error code was captured.

### `OccurredAt`
```csharp
public DateTime OccurredAt { get; }
```
Returns the exact date and time when the exception was instantiated. This timestamp is useful for correlating GPU failures with system logs or performance metrics collected during image processing pipelines.

### `GpuException` (Constructor)
```csharp
public GpuException()
```
Initializes a new instance of the `GpuException` class with default values. No specific message, inner exception, device context, or error code is set by this overload.

### `GpuException` (Constructor)
```csharp
public GpuException(string? message, Exception? innerException = null, string? deviceName = null, int? errorCode = null)
```
Initializes a new instance of the `GpuException` class with a specified error message, optional inner exception, and GPU-specific context.
*   **Parameters**:
    *   `message`: A descriptive message explaining the error.
    *   `innerException`: The exception that is the cause of the current exception, or `null` if no inner exception is specified.
    *   `deviceName`: The name of the GPU device involved in the error.
    *   `errorCode`: The specific error code returned by the GPU runtime.
*   **Returns**: A new `GpuException` instance.
*   **Throws**: No specific exceptions are thrown by the constructor itself beyond standard runtime constraints.

### `ToString`
```csharp
public override string ToString()
```
Returns a string representation of the current exception, including the standard exception message, stack trace, and the GPU-specific details (`DeviceName`, `ErrorCode`, and `OccurredAt`).
*   **Returns**: A formatted string containing the exception details.
*   **Throws**: Does not throw exceptions under normal circumstances.

## Usage

### Example 1: Catching and Diagnosing a GPU Failure
This example demonstrates how to catch a `GpuException` during an image filtering operation and log the specific device and error code for troubleshooting.

```csharp
using System;
using GpuImageProcessing;

public class ImageProcessor
{
    public void ProcessImage(ImageBuffer input)
    {
        try
        {
            // Hypothetical method that triggers GPU execution
            input.ApplyGaussianBlur();
        }
        catch (GpuException ex)
        {
            Console.WriteLine($"GPU Error on device '{ex.DeviceName ?? "Unknown"}' at {ex.OccurredAt}");
            Console.WriteLine($"Error Code: {ex.ErrorCode?.ToString() ?? "N/A"}");
            Console.WriteLine($"Details: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}
```

### Example 2: Throwing a Custom GpuException
This example shows how to manually instantiate and throw a `GpuException` when a specific hardware constraint is violated within a custom kernel wrapper.

```csharp
using System;
using GpuImageProcessing;

public class KernelWrapper
{
    public void ExecuteKernel(string deviceName, int status)
    {
        if (status != 0)
        {
            throw new GpuException(
                message: "Failed to execute convolution kernel due to memory overflow.",
                innerException: null,
                deviceName: deviceName,
                errorCode: status
            );
        }
    }
}
```

## Notes

*   **Null Values**: Consumers of this API must handle `null` values for `DeviceName` and `ErrorCode`. These properties are nullable because certain initialization failures may occur before the runtime can successfully query the device name or assign a specific hardware error code.
*   **Timestamp Precision**: The `OccurredAt` property relies on `DateTime.Now` (or equivalent) at the moment of construction. In high-frequency trading or real-time processing loops, ensure system clock synchronization if correlating events across multiple nodes.
*   **Thread Safety**: The `GpuException` class is immutable after construction; all public members are getters or read-only fields. Therefore, instances of `GpuException` are thread-safe for reading across multiple threads once instantiated. However, the act of throwing and catching exceptions should follow standard .NET threading practices.
*   **String Representation**: The `ToString()` override is designed for diagnostic logging. It may include sensitive environment details depending on the `DeviceName` format; avoid logging raw `ToString()` output to unsecured channels in production environments without sanitization.
