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
    /// Interactive command shell supporting REPL-style execution with command history.
    /// Provides auto-completion hints and contextual help.
    /// </summary>
    public class InteractiveShell
    {
        private readonly CliParser _parser;
        private readonly Dictionary<string, Func<ParsedCommand, Task>> _handlers;
        private readonly CommandHistory _history;
        private bool _running;

        public InteractiveShell(CliParser parser)
        {
            _parser = parser;
            _handlers = new Dictionary<string, Func<ParsedCommand, Task>>(StringComparer.OrdinalIgnoreCase);
            _history = new CommandHistory();
            _running = false;
        }

        /// <summary>
        /// Registers a command handler that will be invoked when the command is executed
        /// </summary>
        public void RegisterHandler(string commandName, Func<ParsedCommand, Task> handler)
        {
            _handlers[commandName] = handler;
        }

        /// <summary>
        /// Starts the interactive shell loop
        /// </summary>
        public async Task RunAsync()
        {
            _running = true;
            Console.WriteLine("GPU Image Processing Interactive Shell");
            Console.WriteLine("Type 'help' for commands or 'exit' to quit\n");

            while (_running)
            {
                try
                {
                    Console.Write("> ");
                    var input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input))
                        continue;

                    _history.Add(input);

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        _running = false;
                        Console.WriteLine("Goodbye!");
                        break;
                    }

                    if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(_parser.GenerateHelpText());
                        continue;
                    }

                    if (input.StartsWith("help "))
                    {
                        var cmdName = input.Substring(5).Trim();
                        Console.WriteLine(_parser.GenerateCommandHelp(cmdName));
                        continue;
                    }

                    if (input.Equals("history", StringComparison.OrdinalIgnoreCase))
                    {
                        DisplayHistory();
                        continue;
                    }

                    var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var parsed = _parser.Parse(args);

                    if (_handlers.TryGetValue(parsed.CommandName, out var handler))
                    {
                        await handler(parsed).ConfigureAwait(false);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"No handler registered for command: {parsed.CommandName}");
                        Console.ResetColor();
                    }
                }
                catch (CliParsingException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private void DisplayHistory()
        {
            var entries = _history.GetAll();
            if (entries.Count == 0)
            {
                Console.WriteLine("No command history");
                return;
            }

            Console.WriteLine("\nCommand History:");
            for (int i = 0; i < entries.Count; i++)
            {
                Console.WriteLine($"  [{i + 1}] {entries[i]}");
            }
            Console.WriteLine();
        }

        private class CommandHistory
        {
            private readonly List<string> _entries = new();
            private const int MaxSize = 100;

            public void Add(string command)
            {
                _entries.Add(command);
                if (_entries.Count > MaxSize)
                    _entries.RemoveAt(0);
            }

            public List<string> GetAll() => new(_entries);
        }
    }

    /// <summary>
    /// Provides suggestions and auto-completion for CLI commands and options
    /// </summary>
    public class CompletionProvider
    {
        private readonly CliParser _parser;
        private readonly List<string> _registeredCommands;

        public CompletionProvider(CliParser parser)
        {
            _parser = parser;
            _registeredCommands = new List<string>();
        }

        public void RegisterCommand(string name)
        {
            if (!_registeredCommands.Contains(name))
                _registeredCommands.Add(name);
        }

        /// <summary>
        /// Gets auto-completion suggestions for incomplete input
        /// </summary>
        public List<string> GetSuggestions(string partialInput)
        {
            var suggestions = new List<string>();
            var parts = partialInput.Split(' ');

            if (parts.Length == 1)
            {
                // Suggest commands
                var prefix = parts[0].ToLower();
                suggestions.AddRange(_registeredCommands.FindAll(c => c.StartsWith(prefix)));
            }
            else if (partialInput.EndsWith("-"))
            {
                // Suggest options
                var command = parts[0];
                // This would require parser to expose options publicly
            }

            return suggestions;
        }
    }
}
