# CliParser

The `CliParser` class defines and parses a command-line interface made up of subcommands, command-specific options, global options, and positional arguments. It validates known commands and options, enforces required options, and can generate help text for the complete CLI or an individual command.

## API

### `CliParser()`

Creates an empty parser to which commands and global options can be registered.

### `RegisterCommand(string? name, string description, Action<CommandBuilder> builder)`

Registers a command. The `builder` callback configures the options accepted by that command. Registering the same command name again replaces its previous definition; command names are matched without regard to case.

- `name`: The command name. It cannot be null or empty.
- `description`: The description displayed in help text. It cannot be null or empty.
- `builder`: A callback that receives the command's `CommandBuilder`. It cannot be null.

### `RegisterGlobalOption(string name, string shortForm, string description, bool requiresValue = false)`

Registers an option that is available to every command.

- `name`: The long option name without the leading `--`.
- `shortForm`: The short option name without the leading `-`.
- `description`: The text displayed in generated help.
- `requiresValue`: Whether the next argument is consumed as the option's value. Flags store the string `"true"`.

### `Parse(string[] args)`

Parses a command name followed by options and positional arguments. Long options use `--name`; short options use a single character such as `-n`. The returned `ParsedCommand` contains the command name, an option dictionary keyed by long option name, and positional arguments in input order. An empty array returns `ParsedCommand.Empty()`.

`Parse` throws `CliParsingException` for an unknown command or option, a missing option value, or a missing required command option. It throws `ArgumentNullException` when `args` is null.

### `GenerateHelpText()`

Returns formatted help text listing every registered command and global option.

### `GenerateCommandHelp(string commandName)`

Returns formatted help for a registered command, including its options and required markers. For an unknown command, it returns `Unknown command: <name>`.

### `CommandBuilder`

The public nested builder used by `RegisterCommand` exposes:

- `CommandBuilder(string name, string description)`: Creates a builder for a command.
- `AddOption(string longForm, string shortForm, string description, bool requiresValue = false, bool isRequired = false)`: Adds a command-specific option and returns the same builder for chaining. Names are supplied without `--` or `-` prefixes.

### `ParsedCommand`

Represents parsed input through these public members:

- `CommandName`: The matched command name, or an empty string for `Empty()`.
- `Options`: Long option names mapped to their values. Flag options have the value `"true"`.
- `PositionalArguments`: Unnamed arguments in their original order.
- `Empty()`: Creates an empty parsed command.
- `GetOption(string name, string defaultValue = null)`: Gets an option value or returns the supplied default.
- `HasOption(string name)`: Returns `true` only when the option exists with the value `"true"`.

### `CliParsingException`

Represents invalid command-line input. Its public constructor accepts the error message.

## Usage

```csharp
using GpuImageProcessing.Cli;

var parser = new CliParser();

parser.RegisterGlobalOption("verbose", "v", "Enable verbose output");
parser.RegisterCommand("resize", "Resize an image", command =>
{
    command
        .AddOption("input", "i", "Source image", requiresValue: true, isRequired: true)
        .AddOption("width", "w", "Output width", requiresValue: true);
});

var parsed = parser.Parse(
    new[] { "resize", "--input", "photo.png", "-w", "800", "--verbose", "thumbnail" });

Console.WriteLine(parsed.CommandName);                  // resize
Console.WriteLine(parsed.GetOption("input"));          // photo.png
Console.WriteLine(parsed.GetOption("width"));          // 800
Console.WriteLine(parsed.HasOption("verbose"));        // True
Console.WriteLine(parsed.PositionalArguments[0]);       // thumbnail
```

## Notes

- Configure the parser before parsing. Its mutable command and option collections are not designed for concurrent registration.
- Command lookup and parsed option lookup are case-insensitive. Long-option matching against registered definitions is case-sensitive.
- A value-taking option consumes the next argument even if that argument begins with `-`.
- Required validation applies to command-specific options marked with `isRequired`; global options cannot be marked as required through the public API.
