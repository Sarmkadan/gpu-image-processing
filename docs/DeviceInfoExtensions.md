# DeviceInfoExtensions

Provides utility methods for inspecting and formatting GPU device capability information. This static class exposes concise summaries of hardware resources, clock frequencies, memory details, and capability scores, enabling callers to quickly assess whether a device meets processing requirements and to present device specifications in human-readable form.

## API

### HasSufficientResources
```csharp
public static bool HasSufficientResources(this DeviceInfo deviceInfo)
```
Determines whether the device possesses adequate compute and memory resources for GPU image processing workloads. Returns `true` if the device meets or exceeds predefined thresholds; otherwise `false`. Throws `ArgumentNullException` if `deviceInfo` is `null`.

### GetCapabilitySummary
```csharp
public static IReadOnlyDictionary<string, object> GetCapabilitySummary(this DeviceInfo deviceInfo)
```
Extracts a dictionary of key capability metrics from the device, including compute unit count, memory size, and clock frequency values. Returns a read-only dictionary mapping metric names to their corresponding values. Throws `ArgumentNullException` if `deviceInfo` is `null`.

### GetFormattedClockFrequency
```csharp
public static string GetFormattedClockFrequency(this DeviceInfo deviceInfo)
```
Formats the device's maximum clock frequency into a human-readable string with appropriate units (MHz or GHz). Returns a formatted string such as `"1.2 GHz"`. Throws `ArgumentNullException` if `deviceInfo` is `null`.

### GetFormattedMemory
```csharp
public static string GetFormattedMemory(this DeviceInfo deviceInfo)
```
Formats the device's total memory capacity into a human-readable string with appropriate units (MB or GB). Returns a formatted string such as `"8.0 GB"`. Throws `ArgumentNullException` if `deviceInfo` is `null`.

### GetCapabilitiesSummary
```csharp
public static string GetCapabilitiesSummary(this DeviceInfo deviceInfo)
```
Produces a multi-line formatted string summarizing all major device capabilities, including compute units, memory, clock frequency, and driver version. Returns a string suitable for display in logs or diagnostic output. Throws `ArgumentNullException` if `deviceInfo` is `null`.

### GetFormattedCapabilityScore
```csharp
public static string GetFormattedCapabilityScore(this DeviceInfo deviceInfo)
```
Computes a numerical capability score based on weighted hardware metrics and returns it as a formatted string with a descriptive label. Returns a string such as `"Score: 85/100"`. Throws `ArgumentNullException` if `deviceInfo` is `null`.

## Usage

### Example 1: Selecting a suitable device for processing
```csharp
DeviceInfo[] devices = context.GetAvailableDevices();
DeviceInfo? selectedDevice = null;

foreach (var device in devices)
{
    if (device.HasSufficientResources())
    {
        Console.WriteLine($"Selected: {device.GetCapabilitiesSummary()}");
        Console.WriteLine($"Score: {device.GetFormattedCapabilityScore()}");
        selectedDevice = device;
        break;
    }
}

if (selectedDevice is null)
{
    throw new InvalidOperationException("No device with sufficient resources found.");
}
```

### Example 2: Logging device specifications for diagnostics
```csharp
DeviceInfo primaryDevice = context.GetPrimaryDevice();

var summary = primaryDevice.GetCapabilitySummary();
foreach (var kvp in summary)
{
    logger.Info($"{kvp.Key}: {kvp.Value}");
}

logger.Info($"Clock: {primaryDevice.GetFormattedClockFrequency()}");
logger.Info($"Memory: {primaryDevice.GetFormattedMemory()}");
logger.Info(primaryDevice.GetCapabilitiesSummary());
```

## Notes

- All methods are extension methods on `DeviceInfo` and require a non-null instance; passing `null` will consistently throw `ArgumentNullException`.
- The `HasSufficientResources` threshold is determined by internal constants and may not reflect requirements of all workloads—callers with specialized needs should perform additional validation.
- `GetFormattedClockFrequency` and `GetFormattedMemory` assume the underlying device properties are populated; if the device reports zero or negative values, the formatted output will reflect those values literally (e.g., `"0 MHz"`).
- `GetFormattedCapabilityScore` uses a fixed weighting scheme; the score is relative and intended for coarse comparison between devices, not absolute performance measurement.
- All methods are thread-safe as they operate on immutable snapshots of device information and do not mutate shared state.
