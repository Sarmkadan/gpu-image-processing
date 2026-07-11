# FilterParameter

Represents a configurable parameter for image-processing filters, encapsulating a value with metadata such as bounds, type, and unit information. Used to standardize parameter handling across GPU-accelerated image processing operations.

## API

### `public Guid Id`
A unique identifier for the parameter. Used to distinguish parameters in collections and during serialization.

### `public string Name`
The human-readable name of the parameter, typically displayed in UI controls or logs.

### `public float Value`
The current value of the parameter. Must be within the range defined by `Min` and `Max` after clamping.

### `public float Min`
The minimum allowed value for `Value`. Used during clamping and validation.

### `public float Max`
The maximum allowed value for `Value`. Used during clamping and validation.

### `public string Type`
A string identifier describing the semantic type of the parameter (e.g., "Exposure", "Contrast", "Threshold"). Used for categorization and UI grouping.

### `public string? Unit`
An optional string representing the unit of measurement (e.g., "dB", "px", "%"). May be `null` if unitless.

### `public string? Description`
An optional descriptive note about the parameter’s purpose or effect. May be `null`.

### `public bool IsRequired`
Indicates whether the parameter must be provided with a valid value for the filter to function correctly.

### `public FilterParameter`
Constructs a new `FilterParameter` with the specified properties. All parameters except `Unit`, `Description`, and `IsRequired` are required.

**Parameters:**
- `id`: Unique identifier.
- `name`: Display name.
- `value`: Initial value.
- `min`: Minimum allowed value.
- `max`: Maximum allowed value.
- `type`: Semantic type identifier.
- `unit`: Optional unit string.
- `description`: Optional description.
- `isRequired`: Whether the parameter is mandatory.

**Throws:**
- `ArgumentOutOfRangeException`: If `min` > `max`.
- `ArgumentException`: If `name` or `type` is null or whitespace.

### `public bool IsValid`
Gets whether the current `Value` is within the valid range `[Min, Max]`.

**Returns:**
- `true` if `Value` is between `Min` and `Max` (inclusive); otherwise, `false`.

### `public void ClampValue()`
Adjusts `Value` to lie within the range `[Min, Max]` if it currently lies outside.

### `public FilterParameter Clone()`
Creates a deep copy of the current parameter, including all properties.

**Returns:**
- A new `FilterParameter` instance with identical values.

### `public float GetNormalizedValue()`
Returns the current `Value` normalized to the range `[0, 1]` based on `Min` and `Max`.

**Returns:**
- A normalized float in the range `[0, 1]`.

**Throws:**
- `InvalidOperationException`: If `Min` equals `Max`.

### `public void SetFromNormalizedValue(float normalizedValue)`
Sets `Value` from a normalized input in the range `[0, 1]`, converting it back to the original scale using `Min` and `Max`.

**Parameters:**
- `normalizedValue`: A value in the range `[0, 1]`.

**Throws:**
- `ArgumentOutOfRangeException`: If `normalizedValue` is outside `[0, 1]`.
- `InvalidOperationException`: If `Min` equals `Max`.

## Usage
