# DeviceUtilitiesJsonExtensions
The `DeviceUtilitiesJsonExtensions` type provides a set of methods and properties for working with device utilities in JSON format, allowing for easy serialization and deserialization of device configuration data. This type is part of the `gpu-image-processing` project and is designed to simplify the process of working with device utilities in a JSON-based workflow.

## API
* `public static string ToJson`: This method takes no parameters and returns a JSON string representation of the device utilities. It does not throw any exceptions.
* `public static DeviceConfiguration? FromJson(string json)`: This method takes a JSON string as a parameter and returns a `DeviceConfiguration` object if the JSON string is valid, or `null` if it is not. It throws an exception if the JSON string is malformed.
* `public static bool TryFromJson(string json, out DeviceConfiguration? configuration)`: This method takes a JSON string and an output parameter for a `DeviceConfiguration` object. It returns `true` if the JSON string is valid and the `DeviceConfiguration` object is successfully populated, and `false` otherwise. It does not throw any exceptions.
* `public string? DeviceName`: This property gets the name of the device. It does not throw any exceptions.
* `public long GlobalMemoryBytes`: This property gets the global memory bytes of the device. It does not throw any exceptions.
* `public int ComputeUnits`: This property gets the number of compute units of the device. It does not throw any exceptions.
* `public int MaxClockFrequency`: This property gets the maximum clock frequency of the device. It does not throw any exceptions.
* `public string? ComputeCapability`: This property gets the compute capability of the device. It does not throw any exceptions.
* `public MemoryPressureLevel MemoryPressureLevel`: This property gets the memory pressure level of the device. It does not throw any exceptions.
* `public int RecommendedBatchSize`: This property gets the recommended batch size of the device. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `DeviceUtilitiesJsonExtensions` type:
```csharp
// Example 1: Serializing device utilities to JSON
var deviceUtilities = new DeviceUtilitiesJsonExtensions();
var json = DeviceUtilitiesJsonExtensions.ToJson();
Console.WriteLine(json);

// Example 2: Deserializing JSON to device utilities
var json = "{\"DeviceName\":\"MyDevice\",\"GlobalMemoryBytes\":1024,\"ComputeUnits\":10,\"MaxClockFrequency\":1000,\"ComputeCapability\":\"1.0\",\"MemoryPressureLevel\":\"Low\",\"RecommendedBatchSize\":10}";
if (DeviceUtilitiesJsonExtensions.TryFromJson(json, out var deviceConfiguration))
{
    Console.WriteLine($"Device Name: {deviceConfiguration.DeviceName}");
    Console.WriteLine($"Global Memory Bytes: {deviceConfiguration.GlobalMemoryBytes}");
    Console.WriteLine($"Compute Units: {deviceConfiguration.ComputeUnits}");
    Console.WriteLine($"Max Clock Frequency: {deviceConfiguration.MaxClockFrequency}");
    Console.WriteLine($"Compute Capability: {deviceConfiguration.ComputeCapability}");
    Console.WriteLine($"Memory Pressure Level: {deviceConfiguration.MemoryPressureLevel}");
    Console.WriteLine($"Recommended Batch Size: {deviceConfiguration.RecommendedBatchSize}");
}
```

## Notes
When working with the `DeviceUtilitiesJsonExtensions` type, note that the `FromJson` method will throw an exception if the JSON string is malformed. The `TryFromJson` method, on the other hand, will return `false` if the JSON string is invalid, allowing for more robust error handling. Additionally, the properties of the `DeviceUtilitiesJsonExtensions` type do not throw any exceptions, but may return `null` or default values if the underlying data is not available. The `DeviceUtilitiesJsonExtensions` type is not thread-safe, and should be used in a single-threaded context to avoid potential concurrency issues.
