# ProcessImageCommand

`ProcessImageCommand` is a command-line executable command that processes an input image through a GPU-accelerated pipeline and writes the result to a specified output file. It supports common image processing operations such as filtering, color adjustment, and format conversion, leveraging the GPU for performance.

## API

### `public ProcessImageCommand`

Constructs a new instance of the `ProcessImageCommand` with default configuration. The command must be configured with input/output paths and processing options before execution via its public properties or fluent methods.

### `public override string GetDescription`

Returns a human-readable description of the command's purpose and behavior, including supported operations and usage context.

- **Return value**: A non-null string describing the command.
- **Exceptions**: Never throws.

### `public override string GetUsage`

Returns a usage string that shows how to invoke the command from the command line, including positional and optional arguments.

- **Return value**: A non-null string formatted as a command-line usage example.
- **Exceptions**: Never throws.

### `public override async Task<int> ExecuteAsync`

Asynchronously executes the image processing pipeline defined by the command's configuration. The operation reads the input image, applies the configured GPU-accelerated transformations, and writes the result to the output path.

- **Return value**: A `Task<int>` that resolves to an exit code indicating success (0) or failure (non-zero).
- **Exceptions**:
  - `InvalidOperationException`: Thrown if required properties (e.g., input/output paths) are not set or invalid.
  - `FileNotFoundException`: Thrown if the input file does not exist.
  - `IOException`: Thrown if the output path is inaccessible or unwritable.
  - `ImageProcessingException`: Thrown if the image cannot be decoded or processed.

## Usage
