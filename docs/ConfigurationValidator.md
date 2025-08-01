# ConfigurationValidator

Provides a centralized, strongly-typed mechanism for validating application configuration values retrieved from environment variables, settings files, or other string-based sources. It exposes both individual validation routines for common configuration types (integers, timeouts, URLs, memory sizes, batch sizes) and a composite method that runs all validations and aggregates errors. The type also offers a generic helper to safely parse and retrieve typed configuration values.

## API

### `public static ConfigurationValidationResult ValidateConfiguration(string key, string value, string validationType)`

Validates a single configuration entry against a named validation rule.

- **Parameters:**
  - `key` (`string`): The configuration key name (used for error reporting).
  - `value` (`string`): The raw configuration value to validate.
  - `validationType` (`string`): A string identifying the validation rule to apply. Recognized values correspond to the specialized validators (e.g., `"IntegerRange"`, `"Timeout"`, `"BatchSize"`, `"MemorySize"`, `"Url"`, `"EnvironmentVariable"`).
- **Returns:** A `ConfigurationValidationResult` indicating success or failure with an associated message.
- **Throws:** `ArgumentNullException` if `key` or `validationType` is `null`. `ArgumentException` if `validationType` is not a recognized rule name.

### `public static ConfigurationValidationResult ValidateIntegerRange(string key, string value, int min, int max)`

Validates that a configuration value represents an integer within a specified inclusive range.

- **Parameters:**
  - `key` (`string`): The configuration key name.
  - `value` (`string`): The raw string value to parse and validate.
  - `min` (`int`): The minimum acceptable value (inclusive).
  - `max` (`int`): The maximum acceptable value (inclusive).
- **Returns:** `ConfigurationValidationResult.Success` if the value parses to an integer within `[min, max]`; otherwise a `Failure` result with a descriptive message.
- **Throws:** `ArgumentNullException` if `key` is `null`. `ArgumentOutOfRangeException` if `min` is greater than `max`.

### `public static ConfigurationValidationResult ValidateTimeout(string key, string value)`

Validates that a configuration value represents a valid timeout duration string (e.g., `"00:00:30"`, `"30s"`, `"500ms"`).

- **Parameters:**
  - `key` (`string`): The configuration key name.
  - `value` (`string`): The raw timeout string to validate.
- **Returns:** A `ConfigurationValidationResult` indicating whether the value is a recognized timeout format.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### `public static ConfigurationValidationResult ValidateBatchSize(string key, string value)`

Validates that a configuration value represents a positive batch size integer, typically greater than zero and within system-imposed upper bounds.

- **Parameters:**
  - `key` (`string`): The configuration key name.
  - `value` (`string`): The raw batch size string to validate.
- **Returns:** A `ConfigurationValidationResult` indicating success if the value is a positive integer within allowed batch limits; otherwise a failure with details.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### `public static ConfigurationValidationResult ValidateMemorySize(string key, string value)`

Validates that a configuration value represents a memory size string with a recognized unit suffix (e.g., `"256MB"`, `"1GB"`, `"512KB"`).

- **Parameters:**
  - `key` (`string`): The configuration key name.
  - `value` (`string`): The raw memory size string to validate.
- **Returns:** A `ConfigurationValidationResult` indicating whether the value is a well-formed memory size expression.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### `public static ConfigurationValidationResult ValidateUrl(string key, string value)`

Validates that a configuration value represents a well-formed absolute URL.

- **Parameters:**
  - `key` (`string`): The configuration key name.
  - `value` (`string`): The raw URL string to validate.
- **Returns:** A `ConfigurationValidationResult` indicating success if the value is a valid absolute URI; otherwise failure with the specific URI format error.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### `public static ConfigurationValidationResult ValidateEnvironmentVariable(string key, string value)`

Validates that a configuration value is present and non-empty, enforcing that a required environment variable has been set.

- **Parameters:**
  - `key` (`string`): The configuration key (environment variable name).
  - `value` (`string`): The environment variable value to check.
- **Returns:** `ConfigurationValidationResult.Success` if the value is not `null` and not whitespace; otherwise a failure indicating the variable is missing or empty.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### `public static T GetConfigurationValue<T>(string key, string value, Func<string, T> parser)`

Parses a raw configuration string into a strongly-typed value using a caller-supplied parser function. This method does not perform validation itself; it delegates parsing to the provided function and wraps any exceptions.

- **Parameters:**
  - `key` (`string`): The configuration key name (used in exception messages).
  - `value` (`string`): The raw configuration value to parse.
  - `parser` (`Func<string, T>`): A function that converts the string to type `T`.
- **Returns:** The parsed value of type `T`.
- **Throws:** `ArgumentNullException` if `key` or `parser` is `null`. `ConfigurationException` if the `parser` throws an exception during conversion, with the original exception preserved as the inner exception.

### `public static List<ConfigurationValidationError> ValidateAllConfiguration(IDictionary<string, string> configuration, IDictionary<string, string> validationRules)`

Runs all configured validations against a complete configuration dictionary and returns a list of all errors encountered.

- **Parameters:**
  - `configuration` (`IDictionary<string, string>`): The full set of configuration key-value pairs to validate.
  - `validationRules` (`IDictionary<string, string>`): A mapping of configuration keys to their validation rule names (e.g., `"IntegerRange"`, `"Url"`).
- **Returns:** A `List<ConfigurationValidationError>` containing every validation failure found. An empty list indicates all validations passed.
- **Throws:** `ArgumentNullException` if either dictionary is `null`. Individual validation exceptions are caught and converted to error entries; the method itself does not throw for validation failures.

### `public bool IsValid`

Gets a value indicating whether this validation result represents a successful validation.

- **Value:** `true` if the result is a success; `false` if it is a failure.

### `public string Message`

Gets the human-readable message associated with this validation result. For successes, this may be an empty string or a confirmation message; for failures, it contains a description of the validation error.

### `public string Key`

Gets the configuration key that was validated, as provided to the validation method that produced this result.

### `public string Value`

Gets the raw configuration value that was validated, as provided to the validation method that produced this result.

### `public static ConfigurationValidationResult Success`

Returns a pre-built `ConfigurationValidationResult` instance representing a successful validation with no message.

- **Value:** A static singleton result with `IsValid == true` and an empty `Message`.

### `public static ConfigurationValidationResult Failure(string key, string value, string message)`

Creates a `ConfigurationValidationResult` instance representing a failed validation.

- **Parameters:**
  - `key` (`string`): The configuration key that failed validation.
  - `value` (`string`): The raw value that failed validation.
  - `message` (`string`): The error message describing the failure.
- **Returns:** A new `ConfigurationValidationResult` with `IsValid == false` and the specified key, value, and message populated.
- **Throws:** `ArgumentNullException` if `key` or `message` is `null`.

## Usage

### Example 1: Validating Individual Configuration Values

```csharp
using GpuImageProcessing.Configuration;

// Validate a batch size from an environment variable
string batchSizeEnv = Environment.GetEnvironmentVariable("GPU_BATCH_SIZE") ?? "64";
ConfigurationValidationResult batchResult = ConfigurationValidator.ValidateBatchSize(
    "GPU_BATCH_SIZE",
    batchSizeEnv
);

if (!batchResult.IsValid)
{
    Console.WriteLine($"Batch size invalid: {batchResult.Message}");
    // Fall back to default
    batchSizeEnv = "64";
}

// Validate and parse a timeout
string timeoutEnv = Environment.GetEnvironmentVariable("GPU_TIMEOUT") ?? "00:02:00";
ConfigurationValidationResult timeoutResult = ConfigurationValidator.ValidateTimeout(
    "GPU_TIMEOUT",
    timeoutEnv
);

if (!timeoutResult.IsValid)
{
    throw new InvalidOperationException(
        $"Critical configuration error: {timeoutResult.Message}"
    );
}

// Parse the validated timeout into a usable TimeSpan
TimeSpan timeout = ConfigurationValidator.GetConfigurationValue(
    "GPU_TIMEOUT",
    timeoutEnv,
    v => TimeSpan.Parse(v)
);
```

### Example 2: Bulk Validation of All Configuration

```csharp
using GpuImageProcessing.Configuration;

// Collect all configuration from environment
var config = new Dictionary<string, string>
{
    ["GPU_BATCH_SIZE"] = Environment.GetEnvironmentVariable("GPU_BATCH_SIZE") ?? "128",
    ["GPU_MEMORY_LIMIT"] = Environment.GetEnvironmentVariable("GPU_MEMORY_LIMIT") ?? "4GB",
    ["GPU_SERVICE_URL"] = Environment.GetEnvironmentVariable("GPU_SERVICE_URL") ?? "https://api.example.com/v2",
    ["GPU_TIMEOUT"] = Environment.GetEnvironmentVariable("GPU_TIMEOUT") ?? "00:05:00",
    ["GPU_REQUIRED_TOKEN"] = Environment.GetEnvironmentVariable("GPU_REQUIRED_TOKEN") ?? ""
};

// Define validation rules for each key
var rules = new Dictionary<string, string>
{
    ["GPU_BATCH_SIZE"] = "BatchSize",
    ["GPU_MEMORY_LIMIT"] = "MemorySize",
    ["GPU_SERVICE_URL"] = "Url",
    ["GPU_TIMEOUT"] = "Timeout",
    ["GPU_REQUIRED_TOKEN"] = "EnvironmentVariable"
};

// Run all validations
List<ConfigurationValidationError> errors = ConfigurationValidator.ValidateAllConfiguration(
    config,
    rules
);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"[{error.Key}] {error.Message} (Value: '{error.Value}')");
    }
    throw new InvalidOperationException(
        $"Application startup aborted: {errors.Count} configuration errors detected."
    );
}

// All validations passed — proceed with startup
Console.WriteLine("All configuration validated successfully.");
```

## Notes

- **Validation rule names** passed to `ValidateConfiguration` and `ValidateAllConfiguration` are case-sensitive and must exactly match the recognized names (`"IntegerRange"`, `"Timeout"`, `"BatchSize"`, `"MemorySize"`, `"Url"`, `"EnvironmentVariable"`). Unrecognized names cause an `ArgumentException`.
- **`ValidateIntegerRange`** requires both `min` and `max` to be provided explicitly. It does not infer bounds from context. Passing `min > max` throws `ArgumentOutOfRangeException` before any parsing occurs.
- **`ValidateTimeout`** accepts multiple formats (ISO 8601 duration-like strings, shorthand suffixes). Values that parse to zero or negative durations are treated as failures.
- **`ValidateMemorySize`** expects a numeric portion followed immediately by a recognized unit suffix. Whitespace between the number and unit is not permitted. Decimal values (e.g., `"1.5GB"`) may be rejected depending on the internal parser implementation.
- **`ValidateUrl`** requires an absolute URI. Relative paths, protocol-relative URLs, and malformed strings all produce a failure result. The validator does not check whether the URL is reachable.
- **`ValidateEnvironmentVariable`** treats whitespace-only strings as empty and therefore invalid. A variable set to `" "` will fail validation.
- **`GetConfigurationValue<T>`** does not perform validation. It is the caller's responsibility to validate the value before parsing, or to handle the `ConfigurationException` that wraps parser failures.
- **`ValidateAllConfiguration`** catches exceptions from individual validators and converts them to `ConfigurationValidationError` entries. It never throws due to a validation failure, making it safe for batch validation scenarios where partial results are acceptable.
- **Thread safety:** All public static methods are stateless and operate only on their parameters. The `Success` singleton is immutable. Instance members (`IsValid`, `Message`, `Key`, `Value`) are read-only after construction. The type is safe for concurrent use without external synchronization.
