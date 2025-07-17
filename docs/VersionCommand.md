# VersionCommand

A command-line handler that outputs the application version and build information. It is typically invoked to report the current version of the `gpu-image-processing` tool.

## API

### `public VersionCommand`

Initializes a new instance of the `VersionCommand` class.

### `public override string GetDescription`

Gets a short, user-friendly description of what the command does.

- **Returns**: A string describing the command’s purpose.
- **Throws**: No exceptions are thrown by this member.

### `public override string GetUsage`

Gets a usage example for the command, including syntax and available options.

- **Returns**: A string showing how to invoke the command from the command line.
- **Throws**: No exceptions are thrown by this member.

### `public override async Task<int> ExecuteAsync`

Asynchronously executes the command, writes the application version to the console, and returns an exit code.

- **Returns**: A `Task<int>` that resolves to `0` on success.
- **Throws**:
  - `InvalidOperationException`: If the version information cannot be retrieved at runtime.

## Usage
