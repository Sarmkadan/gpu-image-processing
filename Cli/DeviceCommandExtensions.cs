using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides extension methods for the <see cref="DeviceCommand"/> class.
    /// </summary>
    public static class DeviceCommandExtensions
    {
        /// <summary>
        /// Gets a brief description of the command.
        /// </summary>
        /// <param name="command">The command to get the description for.</param>
        /// <returns>A brief description of the command.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static string GetBriefDescription(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetDescription().Split('.')[0];
        }

        /// <summary>
        /// Determines whether the command is executable.
        /// </summary>
        /// <param name="command">The command to check.</param>
        /// <returns>True if the command is executable; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static bool IsExecutable(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetUsage() != null;
        }

        /// <summary>
        /// Executes the command asynchronously and returns the result.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The result of the command execution.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static async Task<int> ExecuteCommandAsync(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return await command.ExecuteAsync();
        }

        /// <summary>
        /// Gets the usage of the command as a list of lines.
        /// </summary>
        /// <param name="command">The command to get the usage for.</param>
        /// <returns>A list of lines representing the usage of the command.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static IReadOnlyList<string> GetUsageLines(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            var usage = command.GetUsage();
            return usage != null ? usage.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList() : new List<string>();
        }
    }
}
