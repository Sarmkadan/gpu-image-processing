# CommandHandler

Base class for command handlers that process GPU image processing operations. Provides a standardized interface for describing, validating, and executing commands with argument parsing capabilities.

## API

### `public abstract string GetDescription()`
Returns a human-readable description of the command's purpose and behavior.

- **Returns**: A string containing the command description.
- **Throws**: Not specified; assumes implementation handles error cases internally.

### `public abstract string GetUsage()`
Returns a usage string that shows how to invoke the command, including required and optional arguments.

- **Returns**: A string formatted as a usage example (e.g., `"--command [--option value]"`).
- **Throws**: Not specified; assumes implementation handles error cases internally.

### `public abstract Task<int> ExecuteAsync()`
Executes the command asynchronously and returns an exit code indicating success or failure.

- **Returns**: A `Task<int>` resolving to an exit code (typically `0` for success, non-zero for errors).
- **Throws**: Implementation-specific exceptions may be thrown during execution.

### `public string GetArgument(string name)`
Retrieves the value of a named argument from the parsed input.

- **Parameters**:
  - `name`: The name of the argument to retrieve.
- **Returns**: The value of the argument if present; `null` otherwise.
- **Throws**: `ArgumentNullException` if `name` is `null`.

### `public bool HasFlag(string name)`
Checks whether a named flag (boolean argument) was provided in the input.

- **Parameters**:
  - `name`: The name of the flag to check.
- **Returns**: `true` if the flag was provided; `false` otherwise.
- **Throws**: `ArgumentNullException` if `name` is `null`.

### `public void SetArguments(string[] args)`
Configures the handler with command-line arguments for parsing.

- **Parameters**:
  - `args`: An array of strings representing the command-line arguments.
- **Throws**: `ArgumentNullException` if `args` is `null`.

## Usage
