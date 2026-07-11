# SimdCapabilitiesExtensions
The `SimdCapabilitiesExtensions` type provides a set of static methods for working with SIMD (Single Instruction, Multiple Data) capabilities, allowing developers to query and utilize the optimal SIMD level for their specific use case, as well as determine the presence of SIMD support.

## API
* `public static bool SupportsVectorWidth`: This method determines whether the current system supports a specific vector width. 
* `public static SimdLevel GetOptimalSimdLevel`: This method returns the optimal SIMD level for the current system, taking into account the available hardware capabilities. 
* `public static bool HasSIMD`: This method checks if the current system has SIMD support. 
* `public static string ToFriendlyString`: This method converts the SIMD capabilities to a human-readable string representation.

## Usage
The following examples demonstrate how to use the `SimdCapabilitiesExtensions` type:
```csharp
// Example 1: Checking SIMD support
if (SimdCapabilitiesExtensions.HasSIMD)
{
    Console.WriteLine("SIMD support is available.");
}
else
{
    Console.WriteLine("SIMD support is not available.");
}

// Example 2: Getting the optimal SIMD level
SimdLevel optimalLevel = SimdCapabilitiesExtensions.GetOptimalSimdLevel;
Console.WriteLine($"Optimal SIMD level: {optimalLevel}");
```

## Notes
When using the `SimdCapabilitiesExtensions` type, consider the following edge cases and thread-safety remarks:
* The `GetOptimalSimdLevel` method may return different results depending on the system's hardware configuration.
* The `HasSIMD` method is a simple boolean check and does not provide detailed information about the available SIMD capabilities.
* The `ToFriendlyString` method is primarily intended for debugging and logging purposes.
* The `SimdCapabilitiesExtensions` type is thread-safe, as all methods are static and do not rely on instance state. However, the underlying system's SIMD capabilities may change if the hardware configuration is modified, which could affect the results of these methods.
