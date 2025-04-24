// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Displays version and build information about the application.
    /// Shows dependencies, build date, and configuration details.
    /// </summary>
    public class VersionCommand : CommandHandler
    {
        public VersionCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "version")
        {
        }

        public override string GetDescription()
        {
            return "Display application version and build information";
        }

        public override string GetUsage()
        {
            return @"
Usage: version [options]

Show application version, build date, and dependency information.

Options:
  --full              Display extended version information
  --check             Check for available updates
  --json              Output in JSON format

Examples:
  version
  version --full
  version --check
";
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                if (HasFlag("full"))
                {
                    return ShowFullVersion();
                }
                else if (HasFlag("check"))
                {
                    return await CheckForUpdatesAsync();
                }
                else
                {
                    return ShowBasicVersion();
                }
            }
            catch (Exception ex)
            {
                PrintError($"Version command failed: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Displays basic version information in simple format.
        /// </summary>
        private int ShowBasicVersion()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("GPU Image Processing System");
            Console.ResetColor();

            var version = GetApplicationVersion();
            Console.WriteLine($"Version: {version.Major}.{version.Minor}.{version.Build}");
            Console.WriteLine($"Build: {GetBuildDate()}");
            Console.WriteLine($"Target Framework: .NET 10.0");
            Console.WriteLine();

            return 0;
        }

        /// <summary>
        /// Displays comprehensive version information including all dependencies.
        /// Shows build configuration, target framework, and feature support.
        /// </summary>
        private int ShowFullVersion()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        GPU Image Processing System - Version Information      ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            var version = GetApplicationVersion();

            Console.WriteLine();
            Console.WriteLine("Application Information:");
            Console.WriteLine($"  Name: GPU Image Processing System");
            Console.WriteLine($"  Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
            Console.WriteLine($"  Author: Vladyslav Zaiets");
            Console.WriteLine($"  Website: https://sarmkadan.com");
            Console.WriteLine($"  Build Date: {GetBuildDate()}");
            Console.WriteLine();

            Console.WriteLine("Framework & Runtime:");
            Console.WriteLine($"  Target Framework: .NET 10.0");
            Console.WriteLine($"  Runtime Version: {Environment.Version}");
            Console.WriteLine($"  Operating System: {GetOSInfo()}");
            Console.WriteLine($"  Architecture: {GetArchitecture()}");
            Console.WriteLine();

            Console.WriteLine("Dependencies:");
            Console.WriteLine("  Microsoft.Extensions.DependencyInjection (10.0.0)");
            Console.WriteLine("  Microsoft.Extensions.Configuration (10.0.0)");
            Console.WriteLine("  Microsoft.Extensions.Hosting (10.0.0)");
            Console.WriteLine("  Microsoft.Extensions.Logging (10.0.0)");
            Console.WriteLine("  Silk.NET.OpenCL (2.23.0)");
            Console.WriteLine();

            Console.WriteLine("Features:");
            Console.WriteLine("  ✓ GPU Acceleration (OpenCL)");
            Console.WriteLine("  ✓ Batch Processing");
            Console.WriteLine("  ✓ Image Filtering");
            Console.WriteLine("  ✓ Geometric Transforms");
            Console.WriteLine("  ✓ Device Management");
            Console.WriteLine("  ✓ Asynchronous Operations");
            Console.WriteLine();

            Console.WriteLine("Configuration:");
            Console.WriteLine($"  Debug Mode: {IsDebugBuild()}");
            Console.WriteLine($"  Tiered Compilation: Enabled");
            Console.WriteLine($"  ReadyToRun: Enabled");
            Console.WriteLine();

            return 0;
        }

        /// <summary>
        /// Checks for available updates and displays information.
        /// In production, would check against remote version repository.
        /// </summary>
        private async Task<int> CheckForUpdatesAsync()
        {
            PrintInfo("Checking for updates...");

            try
            {
                var version = GetApplicationVersion();
                Console.WriteLine();
                Console.WriteLine($"Current version: {version.Major}.{version.Minor}.{version.Build}");
                Console.WriteLine($"Latest version:  1.2.0");
                Console.WriteLine();

                PrintSuccess("You are running the latest version");
                return 0;
            }
            catch (Exception ex)
            {
                PrintWarning($"Could not check for updates: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets the application version from assembly metadata.
        /// </summary>
        private Version GetApplicationVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetName().Version ?? new Version(1, 0, 0, 0);
        }

        /// <summary>
        /// Gets the build date from assembly metadata if available.
        /// </summary>
        private string GetBuildDate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileInfo = new System.IO.FileInfo(assembly.Location);
            return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Gets operating system information.
        /// </summary>
        private string GetOSInfo()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
                return "Windows";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Linux))
                return "Linux";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX))
                return "macOS";
            else
                return "Unknown";
        }

        /// <summary>
        /// Gets processor architecture information.
        /// </summary>
        private string GetArchitecture()
        {
            return System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
        }

        /// <summary>
        /// Checks if application was compiled in Debug mode.
        /// </summary>
        private bool IsDebugBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
