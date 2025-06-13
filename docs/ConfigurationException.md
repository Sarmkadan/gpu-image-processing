# ConfigurationException

`ConfigurationException` is a custom exception type used within the `gpu-image-processing` project to signal configuration-related errors. It captures the configuration key and value that triggered the exception, providing detailed context for debugging configuration issues.

## API

### Properties

#### `public string? ConfigurationKey`
Gets the configuration key that caused the exception. This property may be `null` if the key is not available or not applicable.

#### `public string? ConfigurationValue`
Gets the configuration value associated with the key that caused the exception. This property may be `null` if the value is not available or not applicable.

### Constructors

#### `public ConfigurationException()`
Initializes a new instance of the `ConfigurationException` class with default values for `ConfigurationKey` and `ConfigurationValue`.

#### `public ConfigurationException(string? message)`
Initializes a new instance of the `ConfigurationException` class with a specified error message. The `ConfigurationKey` and `ConfigurationValue` properties will be `null`.

#### `public ConfigurationException(string? message, Exception? innerException)`
Initializes a new instance of the `ConfigurationException` class with a specified error message and a reference to the inner exception that is the cause of this exception. The `ConfigurationKey` and `ConfigurationValue` properties will be `null`.

### Methods

#### `public override string ToString()`
Returns a string representation of the exception, including the error message, `ConfigurationKey`, and `ConfigurationValue` (if available). The format is implementation-defined and may change between versions.

## Usage

### Example 1: Throwing a ConfigurationException
