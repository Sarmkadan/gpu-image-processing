using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides extension methods for the <see cref="FilterCommand"/> class.
    /// </summary>
    public static class FilterCommandExtensions
    {
        /// <summary>
        /// Retrieves the description of the filter command.
        /// </summary>
        /// <param name="command">The filter command instance.</param>
        /// <returns>The description of the filter command.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static string GetDescriptionOrDefault(this FilterCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetDescription() ?? string.Empty;
        }

        /// <summary>
        /// Retrieves the usage information of the filter command.
        /// </summary>
        /// <param name="command">The filter command instance.</param>
        /// <returns>The usage information of the filter command.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static string GetUsageOrDefault(this FilterCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            return command.GetUsage() ?? string.Empty;
        }

        /// <summary>
        /// Executes the filter command asynchronously and returns a human-readable result message.
        /// </summary>
        /// <param name="command">The filter command instance.</param>
        /// <returns>A formatted string describing the execution result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
        public static async Task<string> ExecuteAndResultToStringAsync(this FilterCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var exitCode = await command.ExecuteAsync();
            return exitCode switch
            {
                0 => "Filter command completed successfully",
                1 => "Filter command failed",
                _ => $"Filter command completed with unexpected exit code: {exitCode}"
            };
        }
    }
}