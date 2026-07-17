# FilterConfigurationExtensions

Provides extension methods for `FilterConfiguration` instances to simplify parameter access, filter identification, and configuration management. These methods abstract common operations such as retrieving typed parameters, setting values, and determining filter characteristics (e.g., convolution filters) to reduce boilerplate code when working with GPU-based image processing filters.

## API

### `GetParameter<T>`
Retrieves a strongly-typed parameter value from the filter configuration.

**Parameters:**
- None.

**Returns:**
- The parameter value of type `T`.

**Throws:**
- `KeyNotFoundException` – If the parameter does not exist in the configuration.
- `InvalidCastException` – If the parameter value cannot be cast to type `T`.

---

### `SetParameter<T>`
Sets a strongly-typed parameter value in the filter configuration.

**Parameters:**
- `value` – The value of type `T` to set.

**Returns:**
- Void.

**Throws:**
- `ArgumentNullException` – If `value` is `null` (for reference types).
- `InvalidOperationException` – If the parameter cannot be set due to configuration constraints.

---

### `WithNewId`
Creates a new `FilterConfiguration` instance with a new unique identifier, preserving all existing parameters.

**Parameters:**
- None.

**Returns:**
- A new `FilterConfiguration` instance with a new `Id`.

**Throws:**
- None.

---

### `GetConvolutionKernelSize`
Retrieves the kernel size of a convolution filter, if the filter is a convolution type.

**Parameters:**
- None.

**Returns:**
- `int?` – The kernel size if the filter is a convolution filter; otherwise, `null`.

**Throws:**
- None.

---

### `IsConvolutionFilter`
Determines whether the filter is a convolution filter.

**Parameters:**
- None.

**Returns:**
- `bool` – `true` if the filter is a convolution filter; otherwise, `false`.

**Throws:**
- None.

---

### `GetNormalizedParameter`
Retrieves a normalized parameter value (typically in the range [0, 1]) from the filter configuration.

**Parameters:**
- None.

**Returns:**
- `float` – The normalized parameter value.

**Throws:**
- `KeyNotFoundException` – If the parameter does not exist.
- `InvalidCastException` – If the parameter value cannot be normalized to a `float`.

## Usage

### Example 1: Accessing and Modifying Filter Parameters
