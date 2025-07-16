# FilterCommand

Provides a base command for applying image filters in GPU-accelerated pipelines. Derived classes implement specific filter operations while reusing common command infrastructure.

## API

### `public FilterCommand`

Base constructor for filter commands. Initializes the command with default or inherited behavior for GPU-based image processing.

### `public override string GetDescription`

Returns a human-readable description of the filter’s purpose and behavior. The description is typically used in help text or command listings.

- **Return value**: A non-null string containing the filter’s description.
- **Exceptions**: None.

### `public override string GetUsage`

Provides a usage string showing how to invoke the filter from the command line, including expected arguments and options.

- **Return value**: A non-null string formatted as a command-line usage example.
- **Exceptions**: None.

### `public override async Task<int> ExecuteAsync`

Executes the filter operation asynchronously on the GPU. Processes the input image, applies the filter logic, and produces the output image.

- **Return value**: A `Task<int>` that completes with an exit code indicating success (0) or failure (non-zero).
- **Exceptions**:
  - `InvalidOperationException`: Thrown if the command is not properly initialized or required resources are unavailable.
  - `ArgumentException`: Thrown if input parameters are invalid or inconsistent.
  - `OperationCanceledException`: Thrown if the operation is canceled during execution.

## Usage
