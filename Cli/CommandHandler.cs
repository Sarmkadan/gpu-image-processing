// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Base class for all CLI commands with execution logic and validation.
    /// Implements the command pattern for extensible CLI architecture.
    /// </summary>
    public abstract class CommandHandler
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly string _commandName;
        protected readonly Dictionary<string, string> _arguments;
        protected readonly List<string> _positionalArgs;

        protected CommandHandler(IServiceProvider serviceProvider, string commandName)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _commandName = commandName;
            _arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _positionalArgs = new List<string>();
        }

        /// <summary>
        /// Gets the description of what this command does.
        /// </summary>
        public abstract string GetDescription();

        /// <summary>
        /// Gets the usage/help text for this command.
        /// </summary>
        public abstract string GetUsage();

        /// <summary>
        /// Executes the command with parsed arguments.
        /// </summary>
        public abstract Task<int> ExecuteAsync();

        /// <summary>
        /// Validates all required arguments are present.
        /// </summary>
        protected virtual bool ValidateArguments()
        {
            return true;
        }

        /// <summary>
        /// Gets an argument value with optional default.
        /// </summary>
        public string GetArgument(string key, string defaultValue = null)
        {
            return _arguments.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Checks if an argument flag is set.
        /// </summary>
        public bool HasFlag(string flag)
        {
            return _arguments.ContainsKey(flag);
        }

        /// <summary>
        /// Parses and sets command arguments from raw args array.
        /// </summary>
        public void SetArguments(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.StartsWith("--"))
                {
                    string key = arg.Substring(2);
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        _arguments[key] = args[++i];
                    }
                    else
                    {
                        _arguments[key] = "true";
                    }
                }
                else if (arg.StartsWith("-"))
                {
                    string key = arg.Substring(1);
                    _arguments[key] = "true";
                }
                else
                {
                    _positionalArgs.Add(arg);
                }
            }
        }

        /// <summary>
        /// Prints colored console output with consistent formatting.
        /// </summary>
        protected void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[INFO] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Prints success message in green.
        /// </summary>
        protected void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[✓] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Prints warning message in yellow.
        /// </summary>
        protected void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[!] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Prints error message in red.
        /// </summary>
        protected void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[✗] {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Gets service instance from DI container with type checking.
        /// </summary>
        protected T GetService<T>() where T : class
        {
            try
            {
                return _serviceProvider.GetService(typeof(T)) as T
                    ?? throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to resolve service {typeof(T).Name}", ex);
            }
        }
    }
}
