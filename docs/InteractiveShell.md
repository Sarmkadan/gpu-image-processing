# InteractiveShell

The `InteractiveShell` class provides a read-evaluate-print loop for commands configured through `CliParser`. It reads commands from the console, records command history, handles built-in help and exit commands, parses other input, and asynchronously invokes registered handlers. The same source file also defines `CompletionProvider`, which supplies prefix-based command suggestions.

## API

### `InteractiveShell(CliParser parser)`

Creates an interactive shell that uses `parser` to parse commands and generate help text. Throws `ArgumentNullException` when `parser` is null.

### `RegisterHandler(string commandName, Func<ParsedCommand, Task> handler)`

Registers the asynchronous handler invoked for `commandName`. Command names are matched without regard to case, and registering the same name again replaces the existing handler.

- `commandName`: The command name to associate with the handler. It cannot be null or empty.
- `handler`: The asynchronous callback that receives the parsed command. It cannot be null.

### `RunAsync()`

Starts the console input loop and returns when the user enters `exit`. The shell recognizes these built-in inputs:

- `help`: Displays help for all commands and global options registered with the parser.
- `help <command>`: Displays help for one command.
- `history`: Displays up to the 100 most recently entered non-empty inputs.
- `exit`: Stops the loop.

Other input is split on spaces and passed to `CliParser.Parse`. If a handler is registered for the parsed command name, the shell awaits it. Parsing and handler exceptions are reported to the console, and the shell then continues reading input.

### `CompletionProvider(CliParser parser)`

Creates a completion provider associated with a parser.

### `CompletionProvider.RegisterCommand(string name)`

Adds a command name to the completion list if it is not already present. Throws `ArgumentException` when `name` is null or empty.

### `CompletionProvider.GetSuggestions(string partialInput)`

Returns registered command names that start with a single-word input prefix. The current implementation does not provide option suggestions.

## Usage

```csharp
using GpuImageProcessing.Cli;

var parser = new CliParser();
parser.RegisterCommand("resize", "Resize an image", command =>
{
    command.AddOption("width", "w", "Output width", requiresValue: true);
});

var shell = new InteractiveShell(parser);
shell.RegisterHandler("resize", parsed =>
{
    Console.WriteLine($"Requested width: {parsed.GetOption("width", "unchanged")}");
    return Task.CompletedTask;
});

await shell.RunAsync();
```

A session can then enter `resize --width 800`, use `history` to review prior input, or enter `exit` to stop the shell.

## Notes

- Configure the parser and register handlers before calling `RunAsync`.
- The shell performs simple space-delimited tokenization; quoted arguments and escaped spaces are not interpreted specially.
- `CompletionProvider` is independent of the console loop. `InteractiveShell` does not call it automatically.
