# CommandDispatcher

The `CommandDispatcher` serves as the central routing mechanism within the `gpu-image-processing` project, managing the registration and asynchronous execution of GPU-accelerated image processing commands. It maintains an internal registry of available operations, allowing consumers to dynamically discover supported tasks, retrieve metadata, and dispatch execution requests without needing direct references to underlying command implementations.

## API

### `public CommandDispatcher`
Initializes a new instance of the `CommandDispatcher` class. The constructor sets up the internal command registry and prepares the dispatcher for immediate use. No parameters are required.

### `public void RegisterCommand`
Registers a specific command implementation with the dispatcher, making it available for subsequent execution.
*   **Parameters**: Accepts the necessary arguments to define the command key and its associated handler logic (specific signature details depend on the handler delegate type used internally).
*   **Return Value**: None (`void`).
*   **Exceptions**: Throws an exception if a command with the same identifier is already registered or if the provided handler is null.

### `public async Task<int> DispatchAsync`
Asynchronously resolves a command by its identifier and executes the associated logic.
*   **Parameters**: Takes the command identifier string and any required execution context or payload arguments.
*   **Return Value**: Returns a `Task<int>` representing the asynchronous operation. The resulting integer typically indicates the execution status code or the number of processed items.
*   **Exceptions**: Throws if the specified command identifier is not found in the registry, if the command execution fails, or if the underlying GPU context is invalid.

### `public IEnumerable<string> GetAvailableCommands`
Retrieves a collection of all command identifiers currently registered with the dispatcher.
*   **Parameters**: None.
*   **Return Value**: Returns an `IEnumerable<string>` containing the unique keys of registered commands.
*   **Exceptions**: Does not throw under normal conditions; returns an empty enumerable if no commands are registered.

### `public string GetCommandDescription`
Fetches the human-readable description or metadata associated with a specific registered command.
*   **Parameters**: Accepts the command identifier string.
*   **Return Value**: Returns a `string` containing the description.
*   **Exceptions**: Throws if the provided command identifier does not exist in the registry.

## Usage

### Example 1: Registration and Discovery
This example demonstrates initializing the dispatcher, registering a custom image filter, and listing available operations.

```csharp
using GpuImageProcessing;

// Initialize the dispatcher
var dispatcher = new CommandDispatcher();

// Register a hypothetical 'GaussianBlur' command
dispatcher.RegisterCommand("GaussianBlur", (context) => 
{
    // Implementation of GPU blur logic
    return Task.FromResult(1); 
});

// Retrieve and display all available commands
var commands = dispatcher.GetAvailableCommands();
foreach (var command in commands)
{
    var description = dispatcher.GetCommandDescription(command);
    Console.WriteLine($"Command: {command} - {description}");
}
```

### Example 2: Asynchronous Execution
This example illustrates dispatching a command asynchronously and handling the result.

```csharp
using GpuImageProcessing;

public async Task ProcessImageAsync(CommandDispatcher dispatcher, string imagePath)
{
    try 
    {
        // Dispatch the 'EdgeDetect' command with the image path as payload
        int resultCode = await dispatcher.DispatchAsync("EdgeDetect", imagePath);
        
        if (resultCode == 0)
        {
            Console.WriteLine("Processing completed successfully.");
        }
        else
        {
            Console.WriteLine($"Processing finished with status code: {resultCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to execute command: {ex.Message}");
    }
}
```

## Notes

*   **Thread Safety**: The `RegisterCommand` method is not thread-safe; registration should occur during the application initialization phase before concurrent dispatch operations begin. `DispatchAsync`, `GetAvailableCommands`, and `GetCommandDescription` are safe for concurrent read access.
*   **Command Lifecycle**: Attempting to dispatch a command that has not been explicitly registered via `RegisterCommand` will result in a runtime exception. Ensure command availability via `GetAvailableCommands` if dynamic execution is required.
*   **Asynchronous Nature**: As `DispatchAsync` returns a `Task<int>`, callers must await the operation to ensure GPU resources are properly synchronized and to capture any exceptions thrown during the asynchronous execution flow.
*   **Identifier Uniqueness**: Command identifiers must be unique within a single `CommandDispatcher` instance. Duplicate registration attempts will fail.
