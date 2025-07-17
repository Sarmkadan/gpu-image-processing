# CliParser

The `CliParser` class provides a fluent interface for defining, registering, and parsing command-line arguments within the `gpu-image-processing` application. It supports the registration of global options and specific subcommands, each with their own set of named options and positional arguments. The parser generates standardized help text and returns a structured `ParsedCommand` object containing the resolved command name, options, and arguments after processing the input string array.

## API

### Constructors

#### `public CliParser()`
Initializes a new instance of the `CliParser` class. This constructor creates an empty parser ready to accept command and option registrations.

### Registration Methods

#### `public void RegisterCommand(CommandBuilder command)`
Registers a subcommand definition with the parser.
*   **Parameters**:
    *   `command`: An instance of `CommandBuilder` containing the command name, description, options, and positional argument definitions.
*   **Returns**: `void`.
*   **Throws**: May throw an exception if a command with the same `CommandName` has already been registered.

#### `public void RegisterGlobalOption(OptionDefinition option)`
Registers an option that applies globally to the parser, regardless of the specific subcommand invoked.
*   **Parameters**:
    *   `option`: An `OptionDefinition` specifying the long form, short form, description, value requirements, and mandatory status of the option.
*   **Returns**: `void`.
*   **Throws**: May throw an exception if an option with the same `LongForm` or `ShortForm` is already registered.

### Parsing and Help Generation

#### `public ParsedCommand Parse(string[] args)`
Processes the provided command-line arguments and returns the resulting command structure.
*   **Parameters**:
    *   `args`: The array of command-line arguments (typically `Environment.GetCommandLineArgs()` excluding the executable path).
*   **Returns**: A `ParsedCommand` object containing the matched `CommandName`, a dictionary of parsed `Options`, and a list of `PositionalArguments`. If no command is matched or args are empty, it may return `ParsedCommand.Empty`.
*   **Throws**: Throws an exception if the input arguments do not match any registered command schema, if a required option is missing, or if an option requiring a value does not receive one.

#### `public string GenerateHelpText()`
Generates a comprehensive help message displaying all registered commands and global options.
*   **Parameters**: None.
*   **Returns**: A formatted string containing usage instructions, command lists, and global option details.
*   **Throws**: None.

#### `public string GenerateCommandHelp(string commandName)`
Generates a detailed help message for a specific subcommand.
*   **Parameters**:
    *   `commandName`: The name of the command for which to generate help.
*   **Returns**: A formatted string detailing the specific command's usage, options, and positional arguments.
*   **Throws**: Throws an exception if `commandName` does not match any registered command.

### Nested Types and Properties

#### `public class CommandBuilder`
A helper class used to construct command definitions before registration.

*   **`public string Name`**: Gets or sets the unique identifier for the command.
*   **`public string Description`**: Gets or sets the human-readable description of the command's purpose.
*   **`public List<OptionDefinition> Options`**: Gets the list of options specific to this command.
*   **`public CommandBuilder AddOption(OptionDefinition option)`**: Adds an option to this command's definition. Returns the current `CommandBuilder` instance to allow fluent chaining.

#### `public class OptionDefinition`
Defines the schema for a single command-line option.

*   **`public string LongForm`**: The long-form name of the option (e.g., `--input`).
*   **`public string ShortForm`**: The short-form alias of the option (e.g., `-i`).
*   **`public string Description`**: The description displayed in help text.
*   **`public bool RequiresValue`**: Indicates whether the option expects an accompanying value.
*   **`public bool IsRequired`**: Indicates whether the option must be present for the command to execute.

#### `public class ParsedCommand`
Represents the result of a successful parse operation.

*   **`public string CommandName`**: The name of the matched command.
*   **`public Dictionary<string, string> Options`**: A dictionary mapping option names to their provided values.
*   **`public List<string> PositionalArguments`**: A list of arguments provided that were not matched to named options.
*   **`public static ParsedCommand Empty`**: A static readonly instance representing a null or empty parse result, used when no command is specified.

## Usage

### Example 1: Defining and Parsing a Subcommand
This example demonstrates how to register a command with specific options and parse user input.

```csharp
var parser = new CliParser();

// Define a 'convert' command
var convertCommand = new CommandBuilder
{
    Name = "convert",
    Description = "Converts an image from one format to another."
};

convertCommand.AddOption(new OptionDefinition
{
    LongForm = "--input",
    ShortForm = "-i",
    Description = "Path to the source image file.",
    RequiresValue = true,
    IsRequired = true
});

convertCommand.AddOption(new OptionDefinition
{
    LongForm = "--output",
    ShortForm = "-o",
    Description = "Path to the destination file.",
    RequiresValue = true,
    IsRequired = false
});

parser.RegisterCommand(convertCommand);

// Register a global verbose flag
parser.RegisterGlobalOption(new OptionDefinition
{
    LongForm = "--verbose",
    ShortForm = "-v",
    Description = "Enable detailed logging.",
    RequiresValue = false,
    IsRequired = false
});

string[] args = { "convert", "-i", "source.png", "-o", "dest.jpg", "-v" };

try
{
    ParsedCommand result = parser.Parse(args);
    
    if (result.CommandName == "convert")
    {
        string inputPath = result.Options["--input"];
        bool isVerbose = result.Options.ContainsKey("--verbose");
        
        // Proceed with processing logic
        Console.WriteLine($"Processing {inputPath}...");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Parse error: {ex.Message}");
    Console.WriteLine(parser.GenerateCommandHelp("convert"));
}
```

### Example 2: Generating Help Text
This example shows how to trigger help output when invalid arguments are provided or when explicitly requested.

```csharp
var parser = new CliParser();

// Register commands...
// (Assume 'filter' and 'resize' commands are registered here)

string[] args = { "--help" };

// Simple heuristic to detect help request
if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine(parser.GenerateHelpText());
    return;
}

try
{
    var result = parser.Parse(args);
    if (result == ParsedCommand.Empty)
    {
        Console.WriteLine("No valid command specified.");
        Console.WriteLine(parser.GenerateHelpText());
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    // Attempt to show specific command help if a command name was partially recognized
    // Otherwise show global help
    Console.WriteLine(parser.GenerateHelpText());
}
```

## Notes

*   **Thread Safety**: The `CliParser` instance is not thread-safe during the configuration phase. Calls to `RegisterCommand` and `RegisterGlobalOption` should be completed on a single thread before invoking `Parse`. Once configuration is complete, `Parse`, `GenerateHelpText`, and `GenerateCommandHelp` are generally safe for concurrent read-only access, provided the underlying collections are not modified.
*   **Option Precedence**: If an option is defined both as a global option and within a specific `CommandBuilder`, the behavior depends on the internal implementation order. Typically, command-specific options override or shadow global options of the same name within the scope of that command.
*   **Empty Results**: The `ParsedCommand.Empty` static member should be checked explicitly when `Parse` is called with an empty argument array or when the input does not match any registered command pattern, rather than relying solely on exception handling.
*   **Value Parsing**: The `Options` dictionary in `ParsedCommand` stores values as strings. Consumers are responsible for converting these strings to appropriate types (e.g., `int`, `bool`, `float`) and validating formats (e.g., file paths, numeric ranges).
*   **Positional Arguments**: Positional arguments are collected in the order they appear after all named options have been processed. Care should be taken when mixing optional named arguments with positional arguments to avoid ambiguity.
