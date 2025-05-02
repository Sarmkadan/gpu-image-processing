#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Routes CLI commands to appropriate handlers and manages command lifecycle.
    /// Supports command registration, discovery, and execution with error handling.
    /// </summary>
    public class CommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _commands;
        private readonly Dictionary<string, CommandHandler> _instances;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _instances = new Dictionary<string, CommandHandler>(StringComparer.OrdinalIgnoreCase);

            RegisterDefaultCommands();
        }

        /// <summary>
        /// Registers all built-in commands into the dispatcher.
        /// </summary>
        private void RegisterDefaultCommands()
        {
            RegisterCommand("process", typeof(ProcessImageCommand));
            RegisterCommand("device", typeof(DeviceCommand));
            RegisterCommand("filter", typeof(FilterCommand));
            RegisterCommand("batch", typeof(BatchCommand));
            RegisterCommand("help", typeof(HelpCommand));
            RegisterCommand("version", typeof(VersionCommand));
        }

        /// <summary>
        /// Registers a custom command handler type with a command name.
        /// </summary>
        public void RegisterCommand(string commandName, Type commandType)
        {
            if (!typeof(CommandHandler).IsAssignableFrom(commandType))
            {
                throw new ArgumentException($"Type {commandType.Name} must derive from CommandHandler");
            }

            _commands[commandName] = commandType;
            _instances.Remove(commandName); // Clear cached instance
        }

        /// <summary>
        /// Dispatches command string to appropriate handler and executes it.
        /// Returns exit code from command execution (0 = success, non-zero = error).
        /// </summary>
        public async Task<int> DispatchAsync(string[] args)
        {
            if (args.Length == 0 || args[0] == "help" || args[0] == "-h" || args[0] == "--help")
            {
                return await ExecuteCommandAsync("help", args).ConfigureAwait(false);
            }

            string commandName = args[0];

            if (!_commands.TryGetValue(commandName, out var commandType))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unknown command: {commandName}");
                Console.ResetColor();
                Console.WriteLine($"Try '{args[0]} help' for more information");
                return 1;
            }

            return await ExecuteCommandAsync(commandName, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a command by name with provided arguments.
        /// Handles command instantiation and argument parsing.
        /// </summary>
        private async Task<int> ExecuteCommandAsync(string commandName, string[] args)
        {
            try
            {
                if (!_instances.TryGetValue(commandName, out var handler))
                {
                    if (!_commands.TryGetValue(commandName, out var commandType))
                    {
                        Console.WriteLine($"Command not found: {commandName}");
                        return 1;
                    }

                    handler = (CommandHandler)Activator.CreateInstance(commandType, _serviceProvider)
                        ?? throw new InvalidOperationException($"Failed to create {commandName} command");

                    _instances[commandName] = handler;
                }

                handler.SetArguments(args);

                return await handler.ExecuteAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error executing command '{commandName}': {ex.Message}");
                Console.ResetColor();
                return 1;
            }
        }

        /// <summary>
        /// Gets all registered command names for help/discovery.
        /// </summary>
        public IEnumerable<string> GetAvailableCommands()
        {
            return _commands.Keys;
        }

        /// <summary>
        /// Gets description for a specific command.
        /// </summary>
        public string GetCommandDescription(string commandName)
        {
            if (!_commands.TryGetValue(commandName, out var commandType))
                return null;

            try
            {
                var handler = (CommandHandler)Activator.CreateInstance(commandType, _serviceProvider);
                return handler?.GetDescription();
            }
            catch
            {
                return null;
            }
        }
    }
}
