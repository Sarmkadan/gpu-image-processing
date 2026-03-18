#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Displays help information about available commands and usage.
    /// Provides detailed documentation for CLI operations.
    /// </summary>
    public class HelpCommand : CommandHandler
    {
        private readonly CommandDispatcher _dispatcher;

        public HelpCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "help")
        {
            _dispatcher = new CommandDispatcher(serviceProvider);
        }

        public override string GetDescription()
        {
            return "Display help information and command documentation";
        }

        public override string GetUsage()
        {
            return @"
Usage: help [command]

Show help for a specific command or list all available commands.

Examples:
  help
  help process
  help device
  help batch
";
        }

        public override async Task<int> ExecuteAsync()
        {
            if (_positionalArgs.Count > 1)
            {
                return ShowCommandHelp(_positionalArgs[1]);
            }
            else
            {
                return ShowGeneralHelp();
            }
        }

        /// <summary>
        /// Displays general help with all available commands listed.
        /// </summary>
        private int ShowGeneralHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   GPU-Accelerated Image Processing System - CLI Interface     ║");
            Console.WriteLine("║   Author: Vladyslav Zaiets | https://sarmkadan.com           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("Available Commands:");
            Console.WriteLine();

            var commands = new[]
            {
                ("process", "Process images with filters and transformations"),
                ("device", "List and manage compute devices"),
                ("filter", "List and manage image filters"),
                ("batch", "Create and manage batch processing jobs"),
                ("version", "Show application version"),
                ("help", "Show this help message"),
            };

            foreach (var (cmd, desc) in commands)
            {
                Console.WriteLine($"  {cmd,-12} - {desc}");
            }

            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  gpu-image process input.jpg --filter gaussian --output output.jpg");
            Console.WriteLine("  gpu-image batch --create MyJob --images img1.jpg,img2.png");
            Console.WriteLine("  gpu-image device --list");
            Console.WriteLine();

            Console.WriteLine("For detailed help on a command, use:");
            Console.WriteLine("  gpu-image help <command>");
            Console.WriteLine("  gpu-image <command> --help");
            Console.WriteLine();

            return 0;
        }

        /// <summary>
        /// Displays detailed help for a specific command.
        /// </summary>
        private int ShowCommandHelp(string commandName)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Help for command: {commandName}");
            Console.ResetColor();
            Console.WriteLine();

            string description = _dispatcher.GetCommandDescription(commandName);
            if (description != null)
            {
                Console.WriteLine(description);
                Console.WriteLine();
            }

            // Show usage based on command
            string usage = commandName.ToLower() switch
            {
                "process" => GetProcessUsage(),
                "device" => GetDeviceUsage(),
                "filter" => GetFilterUsage(),
                "batch" => GetBatchUsage(),
                "version" => GetVersionUsage(),
                _ => "Command not found"
            };

            Console.WriteLine(usage);
            return 0;
        }

        private string GetProcessUsage()
        {
            return @"
Usage: process <image-path> [options]

Process a single image or batch of images with GPU acceleration.

Options:
  --filter <name>     Apply named filter (gaussian, sobel, median, canny)
  --transform <type>  Apply transformation (rotate, resize, colorspace)
  --angle <degrees>   Rotation angle for rotate transform
  --scale <factor>    Scale factor for resize transform
  --output <path>     Output file path
  --batch             Enable batch processing for directory
  --profile <name>    Processing profile (fast, balanced, quality)
  --verbose           Enable verbose logging

Examples:
  process image.jpg --filter gaussian
  process images/ --batch --filter sobel --profile quality
";
        }

        private string GetDeviceUsage()
        {
            return @"
Usage: device [options]

Manage GPU and compute devices for image processing.

Options:
  --list              List all available devices
  --info <id>         Show device information
  --select <id>       Select primary processing device
  --benchmark         Run device benchmark
  --memory-stats      Show memory usage statistics

Examples:
  device --list
  device --select 1
  device --benchmark
";
        }

        private string GetFilterUsage()
        {
            return @"
Usage: filter [options]

Manage image processing filters.

Options:
  --list              List available filters
  --create <name>     Create custom filter
  --type <type>       Filter type (gaussian, sobel, median, canny, bilateral)
  --param <key=val>   Set filter parameter (repeatable)
  --info <id>         Show filter details
  --delete <id>       Delete filter
  --stats             Show filter statistics

Examples:
  filter --list
  filter --create MyGaussian --type gaussian --param sigma=1.5
  filter --stats
";
        }

        private string GetBatchUsage()
        {
            return @"
Usage: batch [options]

Manage batch image processing jobs.

Options:
  --list              List all jobs
  --create <name>     Create new job
  --status <id>       Show job status
  --results <id>      Get job results
  --cancel <id>       Cancel job
  --remove <id>       Delete job
  --export <path>     Export results to file
  --retry <id>        Retry failed job

Examples:
  batch --list
  batch --create MyJob --images img1.jpg,img2.png
  batch --status job-123
";
        }

        private string GetVersionUsage()
        {
            return @"
Usage: version [options]

Display application version and build information.

Options:
  --full              Show detailed version info
  --check             Check for updates

Examples:
  version
  version --full
";
        }
    }
}
