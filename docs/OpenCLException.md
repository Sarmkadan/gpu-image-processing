# OpenCLException

Exception type thrown when an error occurs during OpenCL operations in the `gpu-image-processing` project. This exception encapsulates OpenCL-specific error codes and device context to aid in debugging GPU computation failures.

## API

### `public string? DeviceName`
Gets the name of the OpenCL device associated with the exception, if available. This may be `null` if the device could not be determined at the time of exception creation.

### `public int? OpenCLErrorCode`
Gets the OpenCL error code associated with the exception, if available. This corresponds to values defined in the OpenCL specification (e.g., `CL_INVALID_VALUE`). May be `null` if no specific error code was captured.

### `public override string ToString()`
Returns a string representation of the exception, including the error message, OpenCL error code (if present), and device name (if present). The format is:
`"OpenCLException: [Message] (ErrorCode: [Code], Device: [Name])"`

### `public DeviceInitializationException`
Exception type thrown when OpenCL device initialization fails. Inherits from `OpenCLException` and includes additional context about initialization failures.

### `public KernelCompilationException`
Exception type thrown when OpenCL kernel compilation fails. Inherits from `OpenCLException` and includes kernel source and compilation log details for debugging.

### `public string? KernelSource`
Gets the OpenCL kernel source code that failed to compile, if available. This may be `null` if the source was not captured or not applicable to the exception.

### `public string? CompilationLog`
Gets the OpenCL kernel compilation log output, if available. This contains compiler warnings or errors generated during kernel compilation and may be `null` if no log was produced.

## Usage
