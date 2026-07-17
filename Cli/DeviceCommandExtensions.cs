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
        /// Gets a brief description of the command by extracting the text before the first period.
        /// </summary>
        /// <param name="command">The command to get the description for. Cannot be null.</param>
        /// <returns>A brief description of the command, or the full description if no period is present.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static string GetBriefDescription(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetDescription().Split('.')[0].Trim();
        }

        /// <summary>
        /// Determines whether the command has a defined usage pattern and is therefore executable.
        /// </summary>
        /// <param name="command">The command to check. Cannot be null.</param>
        /// <returns>True if the command has a usage pattern defined; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static bool IsExecutable(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetUsage() is not null;
        }

        /// <summary>
        /// Executes the command asynchronously and returns the result.
        /// </summary>
        /// <param name="command">The command to execute. Cannot be null.</param>
        /// <returns>The exit code from the command execution (0 for success, non-zero for failure).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if command execution fails.</exception>
        public static async Task<int> ExecuteCommandAsync(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return await command.ExecuteAsync();
        }

        /// <summary>
        /// Gets the usage pattern of the command as a list of lines.
        /// </summary>
        /// <param name="command">The command to get the usage for. Cannot be null.</param>
        /// <returns>A read-only list of lines representing the usage pattern, or an empty list if no usage is defined.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static IReadOnlyList<string> GetUsageLines(this DeviceCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetUsage() is { Length: > 0 } usage
                ? usage.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                : Array.Empty<string>();
        }
    }
}
