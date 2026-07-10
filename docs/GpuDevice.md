# GpuDevice
The `GpuDevice` type represents a graphics processing unit (GPU) device, providing information about its capabilities, properties, and supported features. It serves as a central class for accessing and utilizing GPU resources in the `gpu-image-processing` project, enabling developers to leverage the power of GPU acceleration for image processing tasks.

## API
The `GpuDevice` type exposes the following public members:
* `Id`: A unique identifier for the GPU device, represented as a `Guid`.
* `Name`: The name of the GPU device, provided as a `string`.
* `DeviceType`: The type of GPU device, specified as a `GpuDeviceType`.
* `Vendor`: The vendor of the GPU device, given as a `string`.
* `Version`: The version of the GPU device, represented as a `string`.
* `Driver`: The driver version of the GPU device, provided as a `string`.
* `GlobalMemoryBytes`: The total global memory available on the GPU device, measured in bytes, returned as a `long`.
* `LocalMemoryBytes`: The total local memory available on the GPU device, measured in bytes, returned as a `long`.
* `MaxAllocatableMemoryBytes`: The maximum amount of memory that can be allocated on the GPU device, measured in bytes, returned as a `long`.
* `MaxComputeUnits`: The maximum number of compute units available on the GPU device, returned as an `int`.
* `MaxWorkGroupSize`: The maximum size of a work group that can be executed on the GPU device, returned as an `int`.
* `MaxWorkItemDimensions`: The maximum number of dimensions for a work item that can be executed on the GPU device, returned as an `int`.
* `MaxWorkItemSizes`: An array of integers representing the maximum sizes for each dimension of a work item that can be executed on the GPU device.
* `MaxClockFrequencyMhz`: The maximum clock frequency of the GPU device, measured in megahertz, returned as a `double`.
* `IsAvailable`: A boolean indicating whether the GPU device is available for use.
* `SupportsDoublePrecision`: A boolean indicating whether the GPU device supports double precision floating-point operations.
* `SupportsHalfPrecision`: A boolean indicating whether the GPU device supports half precision floating-point operations.
* `DetectedAt`: The date and time when the GPU device was detected, represented as a `DateTime`.
* `Extensions`: A dictionary of strings representing the extensions supported by the GPU device.
* `SupportedFormats`: A list of strings representing the image formats supported by the GPU device.

## Usage
Here are two examples of using the `GpuDevice` type in C#:
```csharp
// Example 1: Accessing GPU device properties
GpuDevice device = GetGpuDevice(); // Assume GetGpuDevice() returns a GpuDevice instance
Console.WriteLine($"Device Name: {device.Name}, Device Type: {device.DeviceType}, Vendor: {device.Vendor}");
Console.WriteLine($"Global Memory: {device.GlobalMemoryBytes} bytes, Local Memory: {device.LocalMemoryBytes} bytes");
Console.WriteLine($"Max Compute Units: {device.MaxComputeUnits}, Max Work Group Size: {device.MaxWorkGroupSize}");

// Example 2: Checking GPU device capabilities
GpuDevice device2 = GetGpuDevice(); // Assume GetGpuDevice() returns a GpuDevice instance
if (device2.SupportsDoublePrecision)
{
    Console.WriteLine("Double precision floating-point operations are supported.");
}
if (device2.SupportsHalfPrecision)
{
    Console.WriteLine("Half precision floating-point operations are supported.");
}
```

## Notes
When working with the `GpuDevice` type, consider the following edge cases and thread-safety remarks:
* The `Id` property is unique for each GPU device, but its value may change across different system boots or device driver updates.
* The `IsAvailable` property may return `false` if the GPU device is currently in use by another process or if there is a hardware issue.
* The `DetectedAt` property represents the date and time when the GPU device was detected, which may not necessarily be the same as when the device was first installed or configured.
* The `Extensions` and `SupportedFormats` properties may return empty collections if the GPU device does not support any extensions or formats.
* The `GpuDevice` type is not thread-safe by default; if multiple threads need to access the same `GpuDevice` instance, proper synchronization mechanisms should be employed to avoid data corruption or other concurrency issues.
