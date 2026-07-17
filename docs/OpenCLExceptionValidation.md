# OpenCLExceptionValidation

Utility class providing validation for OpenCL-related exceptions in GPU image processing scenarios. It offers methods to check whether OpenCL operations completed successfully and to enforce validation with descriptive error messages.

## API

### `Validate()`
Returns a list of validation messages indicating any OpenCL-related issues detected. Each message describes a specific problem encountered during OpenCL operations.

- **Returns**: `IReadOnlyList<string>` – A read-only collection of error messages. Empty if no issues are found.
- **Throws**: Never throws exceptions; returns an empty list on success.

### `IsValid`
Indicates whether OpenCL operations completed without detectable issues.

- **Returns**: `bool` – `true` if no validation messages are present; otherwise, `false`.
- **Throws**: Never throws exceptions.

### `EnsureValid()`
Validates OpenCL operations and throws an `InvalidOperationException` if issues are detected, including a detailed message listing all validation failures.

- **Throws**:
  - `InvalidOperationException` – If `Validate()` returns any non-empty list of messages. The exception message includes all validation failures.

## Usage
