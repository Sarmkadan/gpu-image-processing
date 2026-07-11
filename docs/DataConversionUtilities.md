# DataConversionUtilities

`DataConversionUtilities` is a static utility class providing conversion, formatting, and parsing operations for binary data, numeric arrays, file sizes, time spans, and floating-point normalization. It serves as a centralized helper for translating between human-readable representations and machine-oriented formats within the GPU image processing pipeline.

## API

### `public static string BytesToHex(byte[] bytes)`
Converts an array of bytes to its hexadecimal string representation. Each byte is rendered as two uppercase hexadecimal characters without separators.

**Parameters:**
- `bytes` — The byte array to convert. Must not be null.

**Returns:** A string of hexadecimal digits (e.g., `"A1F2"`).

**Throws:** `ArgumentNullException` if `bytes` is null.

---

### `public static byte[] HexToBytes(string hex)`
Parses a hexadecimal string into a byte array. The string must contain an even number of hexadecimal characters; whitespace and separator characters are not permitted.

**Parameters:**
- `hex` — The hexadecimal string to parse. Must not be null.

**Returns:** A byte array corresponding to the hex string.

**Throws:**
- `ArgumentNullException` if `hex` is null.
- `FormatException` if the string contains non-hex characters or has an odd length.

---

### `public static byte[] FloatsToBytes(float[] floats)`
Reinterprets an array of single-precision floating-point values as a byte array using the IEEE 754 binary representation of each float. The resulting byte array length is four times the float array length.

**Parameters:**
- `floats` — The float array to convert. Must not be null.

**Returns:** A byte array containing the raw binary representation of the floats.

**Throws:** `ArgumentNullException` if `floats` is null.

---

### `public static float[] BytesToFloats(byte[] bytes)`
Reinterprets a byte array as an array of single-precision floating-point values. The byte array length must be a multiple of four.

**Parameters:**
- `bytes` — The byte array to convert. Must not be null.

**Returns:** A float array reconstructed from the byte data.

**Throws:**
- `ArgumentNullException` if `bytes` is null.
- `ArgumentException` if the length of `bytes` is not divisible by four.

---

### `public static string FormatFileSize(long bytes)`
Formats a file size in bytes as a human-readable string with an appropriate unit suffix (B, KB, MB, GB, TB). Uses binary prefixes (1 KB = 1024 B). The value is rounded to two decimal places when the magnitude exceeds 1023 bytes.

**Parameters:**
- `bytes` — The file size in bytes.

**Returns:** A formatted string such as `"512 B"`, `"1.50 KB"`, or `"3.00 GB"`.

**Throws:** Nothing. Negative values are formatted with a minus sign.

---

### `public static long ParseFileSize(string size)`
Parses a human-readable file size string back into a byte count. Accepts suffixes B, KB, MB, GB, TB (case-insensitive) with optional whitespace between the numeric value and the suffix. Binary interpretation is used (1 KB = 1024 B).

**Parameters:**
- `size` — The size string to parse. Must not be null or empty.

**Returns:** The size in bytes as a `long`.

**Throws:**
- `ArgumentNullException` if `size` is null.
- `FormatException` if the string cannot be parsed or contains an unrecognized suffix.

---

### `public static string FormatTimeSpan(TimeSpan duration)`
Formats a `TimeSpan` into a compact human-readable string showing days, hours, minutes, seconds, and milliseconds as applicable. Leading zero-valued components are omitted unless the entire duration is zero, in which case `"0ms"` is returned.

**Parameters:**
- `duration` — The `TimeSpan` to format.

**Returns:** A string such as `"2h 30m 15s"` or `"500ms"`.

**Throws:** Nothing.

---

### `public static TimeSpan ParseDuration(string duration)`
Parses a duration string into a `TimeSpan`. Supports formats like `"1d 2h 30m 15s 500ms"` with any subset of components present. Component order is flexible, and whitespace is permitted between tokens.

**Parameters:**
- `duration` — The duration string to parse. Must not be null or empty.

**Returns:** A `TimeSpan` representing the parsed duration.

**Throws:**
- `ArgumentNullException` if `duration` is null.
- `FormatException` if the string contains unrecognized tokens or malformed numeric values.

---

### `public static string ToBinaryString(byte[] bytes, string separator = " ")`
Converts a byte array to a string of binary representations, with each byte shown as eight bits. An optional separator is inserted between bytes.

**Parameters:**
- `bytes` — The byte array to convert. Must not be null.
- `separator` — A string placed between each byte's binary representation. Defaults to a single space.

**Returns:** A binary string such as `"01000001 11110010"`.

**Throws:** `ArgumentNullException` if `bytes` is null.

---

### `public static bool IsWithinTolerance(float actual, float expected, float tolerance)`
Determines whether a floating-point value is within a specified absolute tolerance of an expected value. Returns `true` if the absolute difference is less than or equal to the tolerance.

**Parameters:**
- `actual` — The measured or computed value.
- `expected` — The reference value.
- `tolerance` — The maximum permitted absolute difference. Must be non-negative.

**Returns:** `true` if `|actual - expected| <= tolerance`; otherwise `false`.

**Throws:** `ArgumentOutOfRangeException` if `tolerance` is negative.

---

### `public static float Normalize(float value, float min, float max)`
Normalizes a value from a specified range `[min, max]` to the range `[0.0, 1.0]`. The input is clamped to the range before normalization.

**Parameters:**
- `value` — The value to normalize.
- `min` — The lower bound of the source range.
- `max` — The upper bound of the source range. Must be greater than `min`.

**Returns:** A float in the range `[0.0, 1.0]`.

**Throws:** `ArgumentException` if `min >= max`.

---

### `public static float Denormalize(float normalized, float min, float max)`
Maps a normalized value from `[0.0, 1.0]` back to the original range `[min, max]`. The input is clamped to `[0.0, 1.0]` before denormalization.

**Parameters:**
- `normalized` — The normalized value, typically in `[0.0, 1.0]`.
- `min` — The lower bound of the target range.
- `max` — The upper bound of the target range. Must be greater than `min`.

**Returns:** A float in the range `[min, max]`.

**Throws:** `ArgumentException` if `min >= max`.

## Usage

### Example 1: Converting GPU Buffer Data
```csharp
// Raw float data from a GPU compute buffer
float[] gpuOutput = new float[] { 0.5f, 1.0f, -0.25f, 3.14159f };

// Convert to bytes for network transmission or disk storage
byte[] rawBytes = DataConversionUtilities.FloatsToBytes(gpuOutput);

// Transmit or store rawBytes...

// Reconstruct on the receiving end
float[] reconstructed = DataConversionUtilities.BytesToFloats(rawBytes);

// Verify round-trip fidelity
for (int i = 0; i < gpuOutput.Length; i++)
{
    bool match = DataConversionUtilities.IsWithinTolerance(
        reconstructed[i], gpuOutput[i], tolerance: 1e-6f);
    Console.WriteLine($"Index {i}: {(match ? "PASS" : "FAIL")}");
}
```

### Example 2: Formatting Processing Metrics
```csharp
// After processing a batch of images, report metrics
long bytesProcessed = 2_147_483_648; // 2 GB
TimeSpan elapsed = new TimeSpan(0, 3, 45, 30, 200); // 3h 45m 30s 200ms

string sizeStr = DataConversionUtilities.FormatFileSize(bytesProcessed);
string timeStr = DataConversionUtilities.FormatTimeSpan(elapsed);

Console.WriteLine($"Processed {sizeStr} in {timeStr}");
// Output: "Processed 2.00 GB in 3h 45m 30s 200ms"

// Parse a user-provided duration threshold
string userThreshold = "2h 30m";
TimeSpan threshold = DataConversionUtilities.ParseDuration(userThreshold);

if (elapsed > threshold)
{
    Console.WriteLine("Processing exceeded the configured time threshold.");
}
```

## Notes

- **Thread Safety:** All methods are static and operate on immutable inputs or produce new output instances without shared mutable state. They are safe for concurrent invocation from multiple threads without external synchronization.
- **Endianness:** `FloatsToBytes` and `BytesToFloats` use the system's native endianness for float-to-byte reinterpretation. When exchanging data across systems with different endianness, an additional byte-order reversal step is required outside this class.
- **Hex Parsing:** `HexToBytes` expects a contiguous hex string with no delimiters or whitespace. Strings with dashes, spaces, or `0x` prefixes will throw a `FormatException`.
- **File Size Formatting:** `FormatFileSize` uses binary units (powers of 1024). This differs from SI decimal units (powers of 1000) sometimes used by storage manufacturers. `ParseFileSize` applies the same binary interpretation.
- **Duration Parsing:** `ParseDuration` accepts components in any order and is case-insensitive for unit suffixes. Duplicate components (e.g., `"5m 3m"`) are summed. Negative values are not supported and will cause a `FormatException`.
- **Normalization Edge Cases:** `Normalize` and `Denormalize` clamp inputs to their respective valid ranges before computation. If `min` and `max` are equal, an `ArgumentException` is thrown. For `Normalize`, if the value is outside `[min, max]`, it is clamped to the nearest bound before mapping.
- **Tolerance:** `IsWithinTolerance` uses absolute tolerance. For relative or ULPS-based comparisons, callers must implement those separately. A negative tolerance always throws.
