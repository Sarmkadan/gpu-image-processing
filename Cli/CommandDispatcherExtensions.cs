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
        /// Determines whether a command with the specified name is registered in the dispatcher.
        /// </summary>
        /// <param name="dispatcher">The command dispatcher instance. Cannot be null.</param>
        /// <param name="commandName">The name of the command to check. Cannot be null or empty.</param>
        /// <returns><see langword="true"/> if the command is registered; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dispatcher"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="commandName"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
        public static bool HasCommand(this CommandDispatcher dispatcher, string commandName)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);
            ArgumentException.ThrowIfNullOrEmpty(commandName);

            return dispatcher.GetAvailableCommands().Any(c => string.Equals(c, commandName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves a collection of all registered commands along with their descriptions.
        /// </summary>
        /// <param name="dispatcher">The command dispatcher instance. Cannot be null.</param>
        /// <returns>A read-only list of tuples containing the command name and its description.
        /// If a command has no description, the description will be "No description available".</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dispatcher"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<(string Name, string Description)> GetCommandDescriptions(this CommandDispatcher dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);

            return dispatcher.GetAvailableCommands()
                .Select(name => (Name: name, Description: dispatcher.GetCommandDescription(name) ?? "No description available"))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Registers a collection of commands with the dispatcher.
        /// </summary>
        /// <param name="dispatcher">The command dispatcher instance. Cannot be null.</param>
        /// <param name="commands">The collection of commands to register. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dispatcher"/> or <paramref name="commands"/> is <see langword="null"/>.</exception>
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
