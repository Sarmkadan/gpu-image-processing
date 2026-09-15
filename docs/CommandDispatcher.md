# CommandDispatcher

The `CommandDispatcher` routes command-line arguments to `CommandHandler` implementations. It registers the built-in commands when constructed, supports additional command registrations, caches handler instances after their first execution, and converts unknown commands or execution failures into a non-zero exit code.

Built-in command names are `process`, `device`, `filter`, `batch`, `batch-dir`, `help`, and `version`. Command-name matching is case-insensitive.

## API

### `CommandDispatcher(IServiceProvider serviceProvider)`

Creates a dispatcher, using `serviceProvider` when constructing command handlers, and registers all built-in commands.

- `serviceProvider`: The service provider passed to each handler constructor.
- Throws `ArgumentNullException` when `serviceProvider` is null.

### `RegisterCommand(string commandName, Type commandType)`

Registers `commandType` under `commandName`. Registering an existing name replaces its type and clears any cached handler instance for that name.

- `commandName`: The command name used during dispatch. Names are matched without regard to case.
- `commandType`: A type derived from `CommandHandler`. To be dispatched successfully, it must be constructible with the configured `IServiceProvider` as its constructor argument.
- Throws `ArgumentException` when `commandType` does not derive from `CommandHandler`.

### `Task<int> DispatchAsync(string[] args)`

Dispatches the supplied command-line arguments. An empty argument array or a first argument of `help`, `-h`, or `--help` executes the help command. Otherwise, the first argument selects the registered handler and the complete array is passed to that handler through `SetArguments`.

Returns the handler's exit code. Unknown commands and exceptions raised while creating or executing a handler are reported to the console and return `1`.

### `IEnumerable<string> GetAvailableCommands()`

Returns the names of all currently registered commands.

### `string GetCommandDescription(string commandName)`

Creates a handler for the registered command and returns its `GetDescription()` result. Returns `null` when the command is not registered or the handler cannot be created.

## Usage

```csharp
using GpuImageProcessing.Cli;

IServiceProvider services = BuildServiceProvider();
var dispatcher = new CommandDispatcher(services);

foreach (string command in dispatcher.GetAvailableCommands())
{
    Console.WriteLine($"{command}: {dispatcher.GetCommandDescription(command)}");
}

int exitCode = await dispatcher.DispatchAsync(new[] { "version" });
return exitCode;
```

Custom commands can be added with `RegisterCommand`; their types must derive from `CommandHandler` and provide a constructor that accepts an `IServiceProvider`.
