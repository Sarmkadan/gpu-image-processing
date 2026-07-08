#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Robust command-line argument parser with help text generation and validation.
    /// Handles subcommands, flags, options, and positional arguments.
    /// </summary>
    public class CliParser
    {
        private readonly Dictionary<string, CommandDefinition> _commands;
        private readonly List<OptionDefinition> _globalOptions;

        public CliParser()
        {
            _commands = new Dictionary<string, CommandDefinition>(StringComparer.OrdinalIgnoreCase);
            _globalOptions = new List<OptionDefinition>();
        }

        /// <summary>
        /// Registers a command with its options and description
        /// </summary>
        public void RegisterCommand(string name, string description, Action<CommandBuilder> builder)
        {
            var cmdBuilder = new CommandBuilder(name, description);
            builder(cmdBuilder);
            _commands[name] = cmdBuilder.Build();
        }

        /// <summary>
        /// Registers a global option available to all commands
        /// </summary>
        public void RegisterGlobalOption(string name, string shortForm, string description, bool requiresValue = false)
        {
            _globalOptions.Add(new OptionDefinition
            {
                LongForm = name,
                ShortForm = shortForm,
                Description = description,
                RequiresValue = requiresValue
            });
        }

        /// <summary>
        /// Parses command-line arguments and returns a parsed command context
        /// </summary>
        public ParsedCommand Parse(string[] args)
        {
            if (args == null || args.Length == 0)
                return ParsedCommand.Empty();

            var commandName = args[0];
            if (!_commands.ContainsKey(commandName))
                throw new CliParsingException($"Unknown command: {commandName}");

            var command = _commands[commandName];
            var remainingArgs = args.Skip(1).ToArray();

            return ParseCommandArguments(command, remainingArgs);
        }

        private ParsedCommand ParseCommandArguments(CommandDefinition command, string[] args)
        {
            var parsed = new ParsedCommand { CommandName = command.Name };
            var positionalArgs = new List<string>();
            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("--"))
                {
                    var optionName = arg.Substring(2);
                    var optionDef = command.Options.FirstOrDefault(o => o.LongForm == optionName)
                                    ?? _globalOptions.FirstOrDefault(o => o.LongForm == optionName);

                    if (optionDef == null)
                        throw new CliParsingException($"Unknown option: --{optionName}");

                    if (optionDef.RequiresValue)
                    {
                        if (i + 1 >= args.Length)
                            throw new CliParsingException($"Option --{optionName} requires a value");
                        options[optionName] = args[++i];
                    }
                    else
                    {
                        options[optionName] = "true";
                    }
                }
                else if (arg.StartsWith("-") && arg.Length == 2)
                {
                    var shortForm = arg[1].ToString();
                    var optionDef = command.Options.FirstOrDefault(o => o.ShortForm == shortForm)
                                    ?? _globalOptions.FirstOrDefault(o => o.ShortForm == shortForm);

                    if (optionDef == null)
                        throw new CliParsingException($"Unknown option: -{shortForm}");

                    if (optionDef.RequiresValue)
                    {
                        if (i + 1 >= args.Length)
                            throw new CliParsingException($"Option -{shortForm} requires a value");
                        options[optionDef.LongForm] = args[++i];
                    }
                    else
                    {
                        options[optionDef.LongForm] = "true";
                    }
                }
                else
                {
                    positionalArgs.Add(arg);
                }
            }

            // Validate required options
            foreach (var option in command.Options.Where(o => o.IsRequired))
            {
                if (!options.ContainsKey(option.LongForm))
                    throw new CliParsingException($"Required option --{option.LongForm} is missing");
            }

            parsed.Options = options;
            parsed.PositionalArguments = positionalArgs;
            return parsed;
        }

        /// <summary>
        /// Generates formatted help text for all commands
        /// </summary>
        public string GenerateHelpText()
        {
            var lines = new List<string>
            {
                "GPU Image Processing CLI",
                "Usage: gpu-image-processing <command> [options] [arguments]",
                "",
                "Commands:"
            };

            foreach (var command in _commands.Values)
            {
                lines.Add($"  {command.Name,-20} {command.Description}");
            }

            lines.Add("");
            lines.Add("Global Options:");

            foreach (var option in _globalOptions)
            {
                var shortForm = string.IsNullOrEmpty(option.ShortForm) ? "" : $"-{option.ShortForm}, ";
                lines.Add($"  {shortForm,5}--{option.LongForm,-20} {option.Description}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Generates help text for a specific command
        /// </summary>
        public string GenerateCommandHelp(string commandName)
        {
            if (!_commands.ContainsKey(commandName))
                return $"Unknown command: {commandName}";

            var command = _commands[commandName];
            var lines = new List<string>
            {
                $"Command: {command.Name}",
                $"Description: {command.Description}",
                ""
            };

            if (command.Options.Any())
            {
                lines.Add("Options:");
                foreach (var option in command.Options)
                {
                    var shortForm = string.IsNullOrEmpty(option.ShortForm) ? "" : $"-{option.ShortForm}, ";
                    var required = option.IsRequired ? " (required)" : "";
                    lines.Add($"  {shortForm,5}--{option.LongForm,-20} {option.Description}{required}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        public class CommandBuilder
        {
            private readonly string _name;
            private readonly string _description;
            private readonly List<OptionDefinition> _options;

            public CommandBuilder(string name, string description)
            {
                _name = name;
                _description = description;
                _options = new List<OptionDefinition>();
            }

            public CommandBuilder AddOption(string longForm, string shortForm, string description,
                bool requiresValue = false, bool isRequired = false)
            {
                _options.Add(new OptionDefinition
                {
                    LongForm = longForm,
                    ShortForm = shortForm,
                    Description = description,
                    RequiresValue = requiresValue,
                    IsRequired = isRequired
                });
                return this;
            }

            internal CommandDefinition Build()
            {
                return new CommandDefinition
                {
                    Name = _name,
                    Description = _description,
                    Options = _options
                };
            }
        }

        internal class CommandDefinition
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<OptionDefinition> Options { get; set; } = new();
        }

        internal class OptionDefinition
        {
            public string LongForm { get; set; }
            public string ShortForm { get; set; }
            public string Description { get; set; }
            public bool RequiresValue { get; set; }
            public bool IsRequired { get; set; }
        }
    }

    public class ParsedCommand
    {
        public string CommandName { get; set; }
        public Dictionary<string, string> Options { get; set; } = new();
        public List<string> PositionalArguments { get; set; } = new();

        public static ParsedCommand Empty() => new ParsedCommand { CommandName = "" };

        public string GetOption(string name, string defaultValue = null)
        {
            return Options.TryGetValue(name, out var value) ? value : defaultValue;
        }

        public bool HasOption(string name)
        {
            return Options.ContainsKey(name) && Options[name] == "true";
        }
    }

    public class CliParsingException : Exception
    {
        public CliParsingException(string message) : base(message) { }
    }
}
