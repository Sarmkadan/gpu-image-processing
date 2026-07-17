#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides useful extension methods for the <see cref="HelpCommand"/> class.
    /// </summary>
    public static class HelpCommandExtensions
    {
        /// <summary>
        /// Gets a formatted list of all available commands with their descriptions.
        /// </summary>
        /// <param name="helpCommand">The help command instance.</param>
        /// <returns>A read-only list of command information tuples containing command names and descriptions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helpCommand"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<(string Name, string Description)> GetAvailableCommands(
            this HelpCommand helpCommand)
        {
            ArgumentNullException.ThrowIfNull(helpCommand);

            return new List<(string, string)>
            {
                ("process", "Process images with filters and transformations"),
                ("device", "List and manage compute devices"),
                ("filter", "List and manage image filters"),
                ("batch", "Create and manage batch processing jobs"),
                ("version", "Show application version"),
                ("help", "Show this help message")
            }.AsReadOnly();
        }

        /// <summary>
        /// Gets the formatted usage string for a specific command.
        /// </summary>
        /// <param name="helpCommand">The help command instance.</param>
        /// <param name="commandName">The name of the command to get usage for.</param>
        /// <returns>The formatted usage string, or <see langword="null"/> if command not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helpCommand"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandName"/> is <see langword="null"/> or empty.</exception>
        public static string? GetCommandUsage(
            this HelpCommand helpCommand,
            string commandName)
        {
            ArgumentNullException.ThrowIfNull(helpCommand);
            ArgumentException.ThrowIfNullOrEmpty(commandName, nameof(commandName));

            return commandName.ToLowerInvariant() switch
            {
                "process" => @"Usage: process <image-path> [options]

Process a single image or batch of images with GPU acceleration.

Options:
--filter <name>        Apply named filter (gaussian, sobel, median, canny)
--transform <type>      Apply transformation (rotate, resize, colorspace)
--angle <degrees>      Rotation angle for rotate transform
--scale <factor>       Scale factor for resize transform
--output <path>        Output file path
--batch                Enable batch processing for directory
--profile <name>       Processing profile (fast, balanced, quality)
--verbose              Enable verbose logging

Examples:
process image.jpg --filter gaussian
process images/ --batch --filter sobel --profile quality",

                "device" => @"Usage: device [options]

Manage GPU and compute devices for image processing.

Options:
--list                List all available devices
--info <id>          Show device information
--select <id>        Select primary processing device
--benchmark           Run device benchmark
--memory-stats        Show memory usage statistics

Examples:
device --list
device --select 1
device --benchmark",

                "filter" => @"Usage: filter [options]

Manage image processing filters.

Options:
--list                List available filters
--create <name>       Create custom filter
--type <type>         Filter type (gaussian, sobel, median, canny, bilateral)
--param <key=val>    Set filter parameter (repeatable)
--info <id>          Show filter details
--delete <id>        Delete filter
--stats               Show filter statistics

Examples:
filter --list
filter --create MyGaussian --type gaussian --param sigma=1.5
filter --stats",

                "batch" => @"Usage: batch [options]

Manage batch image processing jobs.

Options:
--list                List all jobs
--create <name>       Create new job
--status <id>        Show job status
--results <id>       Get job results
--cancel <id>        Cancel job
--remove <id>        Delete job
--export <path>       Export results to file
--retry <id>         Retry failed job

Examples:
batch --list
batch --create MyJob --images img1.jpg,img2.png
batch --status job-123",

                "version" => @"Usage: version [options]

Display application version and build information.

Options:
--full                Show detailed version info
--check               Check for updates

Examples:
version
version --full",

                _ => null
            };
        }

        /// <summary>
        /// Gets the formatted command description for a specific command.
        /// </summary>
        /// <param name="helpCommand">The help command instance.</param>
        /// <param name="commandName">The name of the command to get description for.</param>
        /// <returns>The formatted description string, or <see langword="null"/> if command not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helpCommand"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandName"/> is <see langword="null"/> or empty.</exception>
        public static string? GetCommandDescription(
            this HelpCommand helpCommand,
            string commandName)
        {
            ArgumentNullException.ThrowIfNull(helpCommand);
            ArgumentException.ThrowIfNullOrEmpty(commandName, nameof(commandName));

            return commandName.ToLowerInvariant() switch
            {
                "process" => "Process images with GPU acceleration using various filters and transformations. Supports batch processing of directories and individual files.",
                "device" => "List, select, and manage available GPU and compute devices. Includes benchmarking and memory statistics for performance monitoring.",
                "filter" => "Create, manage, and configure image processing filters. Supports built-in filters and custom filter creation with configurable parameters.",
                "batch" => "Create and manage batch processing jobs for processing multiple images. Includes job status tracking, results retrieval, and export capabilities.",
                "version" => "Display application version information including build details and update status. Use --full for detailed version information.",
                "help" => "Display help information and command documentation. Use 'help <command>' for detailed help on specific commands.",
                _ => null
            };
        }

        /// <summary>
        /// Gets the formatted examples for a specific command.
        /// </summary>
        /// <param name="helpCommand">The help command instance.</param>
        /// <param name="commandName">The name of the command to get examples for.</param>
        /// <returns>A read-only list of example strings.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helpCommand"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandName"/> is <see langword="null"/> or empty.</exception>
        public static IReadOnlyList<string> GetCommandExamples(
            this HelpCommand helpCommand,
            string commandName)
        {
            ArgumentNullException.ThrowIfNull(helpCommand);
            ArgumentException.ThrowIfNullOrEmpty(commandName, nameof(commandName));

            return commandName.ToLowerInvariant() switch
            {
                "process" => new List<string>
                {
                    "process image.jpg --filter gaussian",
                    "process images/ --batch --filter sobel --profile quality",
                    "process photo.png --transform rotate --angle 45 --output rotated.png",
                    "process input.jpg --filter median --param radius=3"
                }.AsReadOnly(),

                "device" => new List<string>
                {
                    "device --list",
                    "device --select 1",
                    "device --benchmark",
                    "device --info 0 --memory-stats"
                }.AsReadOnly(),

                "filter" => new List<string>
                {
                    "filter --list",
                    "filter --create MyGaussian --type gaussian --param sigma=1.5",
                    "filter --stats",
                    "filter --info gaussian"
                }.AsReadOnly(),

                "batch" => new List<string>
                {
                    "batch --list",
                    "batch --create MyJob --images img1.jpg,img2.png",
                    "batch --status job-123",
                    "batch --export results.csv"
                }.AsReadOnly(),

                "version" => new List<string>
                {
                    "version",
                    "version --full",
                    "version --check"
                }.AsReadOnly(),

                _ => Array.Empty<string>().AsReadOnly()
            };
        }

        /// <summary>
        /// Gets the formatted command summary for display in help listings.
        /// </summary>
        /// <param name="helpCommand">The help command instance.</param>
        /// <param name="commandName">The name of the command to get summary for.</param>
        /// <returns>The formatted summary string, or <see langword="null"/> if command not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="helpCommand"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandName"/> is <see langword="null"/> or empty.</exception>
        public static string? GetCommandSummary(
            this HelpCommand helpCommand,
            string commandName)
        {
            ArgumentNullException.ThrowIfNull(helpCommand);
            ArgumentException.ThrowIfNullOrEmpty(commandName, nameof(commandName));

            return commandName.ToLowerInvariant() switch
            {
                "process" => "Process images with GPU acceleration using filters and transformations",
                "device" => "List and manage compute devices for GPU-accelerated processing",
                "filter" => "Create and manage image processing filters",
                "batch" => "Create and manage batch processing jobs",
                "version" => "Display application version and build information",
                "help" => "Display help information and command documentation",
                _ => null
            };
        }
    }
}
