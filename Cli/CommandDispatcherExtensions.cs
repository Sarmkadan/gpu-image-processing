using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides extension methods for <see cref="CommandDispatcher"/>.
    /// </summary>
    public static class CommandDispatcherExtensions
    {
        /// <summary>
        /// Checks if a command with the given name is registered.
        /// </summary>
        /// <param name="dispatcher">The dispatcher instance.</param>
        /// <param name="commandName">The name of the command to check.</param>
        /// <returns>True if the command is registered, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dispatcher"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandName"/> is null or empty.</exception>
        public static bool HasCommand(this CommandDispatcher dispatcher, string commandName)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentException.ThrowIfNullOrEmpty(commandName);

            return dispatcher.GetAvailableCommands().Any(c => string.Equals(c, commandName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves a collection of all registered commands and their descriptions.
        /// </summary>
        /// <param name="dispatcher">The dispatcher instance.</param>
        /// <returns>A read-only list of key-value pairs representing command names and their descriptions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dispatcher"/> is null.</exception>
        public static IReadOnlyList<(string Name, string Description)> GetCommandDescriptions(this CommandDispatcher dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);

            return dispatcher.GetAvailableCommands()
                .Select(name => (Name: name, Description: dispatcher.GetCommandDescription(name) ?? "No description available"))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Registers a collection of commands.
        /// </summary>
        /// <param name="dispatcher">The dispatcher instance.</param>
        /// <param name="commands">The collection of commands to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dispatcher"/> or <paramref name="commands"/> is null.</exception>
        public static void RegisterCommands(this CommandDispatcher dispatcher, IEnumerable<(string Name, Type CommandType)> commands)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentNullException.ThrowIfNull(commands);

            foreach (var command in commands)
            {
                dispatcher.RegisterCommand(command.Name, command.CommandType);
            }
        }
    }
}
