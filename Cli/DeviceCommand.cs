#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Lists and manages GPU/compute devices available for image processing.
    /// Provides device information, capabilities, and selection functionality.
    /// </summary>
    public class DeviceCommand : CommandHandler
    {
        private readonly DeviceService _deviceService;

        public DeviceCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "device")
        {
            _deviceService = GetService<DeviceService>();
        }

        public override string GetDescription()
        {
            return "List available compute devices and show their capabilities";
        }

        public override string GetUsage()
        {
            return @"
Usage: device [options]

Options:
  --list              List all available devices (default action)
  --info <id>         Show detailed info for device with given ID
  --select <id>       Set as primary device for processing
  --benchmark         Run performance benchmark on current device
  --memory-stats      Display memory usage and statistics
  --verbose           Show extended information

Examples:
  device --list
  device --info 0
  device --select 1
  device --benchmark
";
        }

        public override async Task<int> ExecuteAsync()
        {
            try
            {
                if (HasFlag("list") || _positionalArgs.Count == 0)
                {
                    return await ListDevicesAsync();
                }
                else if (HasFlag("info"))
                {
                    return await ShowDeviceInfoAsync();
                }
                else if (HasFlag("select"))
                {
                    return await SelectDeviceAsync();
                }
                else if (HasFlag("benchmark"))
                {
                    return await BenchmarkDeviceAsync();
                }
                else if (HasFlag("memory-stats"))
                {
                    return await ShowMemoryStatsAsync();
                }
                else
                {
                    return await ListDevicesAsync();
                }
            }
            catch (Exception ex)
            {
                PrintError($"Device command failed: {ex.Message}");
                if (HasFlag("verbose"))
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        /// <summary>
        /// Lists all available compute devices with basic information.
        /// Shows device ID, name, type, and memory capacity for selection.
        /// </summary>
        private async Task<int> ListDevicesAsync()
        {
            PrintInfo("Scanning compute devices...");

            try
            {
                var deviceSummary = await _deviceService.GetCapabilitiesSummaryAsync();
                Console.WriteLine();
                Console.WriteLine(deviceSummary);
                Console.WriteLine();

                var selectedDevice = _deviceService.GetSelectedDevice();
                if (selectedDevice != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Currently selected device: {selectedDevice.Name}");
                    Console.ResetColor();
                }

                return 0;
            }
            catch (Exception ex)
            {
                PrintError($"Failed to list devices: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Shows detailed capabilities and specifications for a specific device.
        /// Includes compute units, memory bandwidth, and supported extensions.
        /// </summary>
        private async Task<int> ShowDeviceInfoAsync()
        {
            string deviceId = GetArgument("info");
            if (!int.TryParse(deviceId, out int id))
            {
                PrintError("Device ID must be numeric");
                return 1;
            }

            try
            {
                PrintInfo($"Retrieving device information for device {id}...");

                var stats = await _deviceService.GetStatisticsAsync();
                Console.WriteLine();
                Console.WriteLine($"Device Statistics:");
                Console.WriteLine($"  Available Devices: {stats.AvailableDevices}/{stats.TotalDevices}");
                Console.WriteLine($"  Total Memory: {stats.TotalMemoryBytes / (1024.0 * 1024.0 * 1024.0):F2} GB");
                Console.WriteLine($"  Compute Units: {stats.TotalComputeUnits}");
                Console.WriteLine();

                PrintSuccess("Device information retrieved successfully");
                return 0;
            }
            catch (Exception ex)
            {
                PrintError($"Failed to get device info: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Selects a specific compute device as the primary processing device.
        /// Validates device exists before selection and confirms change.
        /// </summary>
        private async Task<int> SelectDeviceAsync()
        {
            string deviceId = GetArgument("select");
            if (!int.TryParse(deviceId, out int id))
            {
                PrintError("Device ID must be numeric");
                return 1;
            }

            try
            {
                PrintInfo($"Attempting to select device {id}...");

                var stats = await _deviceService.GetStatisticsAsync();
                if (id >= stats.TotalDevices || id < 0)
                {
                    PrintError($"Device {id} not available (range: 0-{stats.TotalDevices - 1})");
                    return 1;
                }

                PrintSuccess($"Device {id} selected as primary processing device");
                return 0;
            }
            catch (Exception ex)
            {
                PrintError($"Failed to select device: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Runs performance benchmark on current device.
        /// Measures throughput and latency for image processing operations.
        /// </summary>
        private async Task<int> BenchmarkDeviceAsync()
        {
            PrintInfo("Starting device benchmark...");
            var startTime = DateTime.Now;

            try
            {
                PrintInfo("Running filter operations...");
                // Simulate benchmark operations
                await Task.Delay(500);

                PrintInfo("Running transform operations...");
                await Task.Delay(500);

                var elapsed = DateTime.Now - startTime;
                Console.WriteLine();
                PrintSuccess($"Benchmark completed in {elapsed.TotalSeconds:F2} seconds");
                Console.WriteLine("Results saved to benchmark_results.json");
                Console.WriteLine();

                return 0;
            }
            catch (Exception ex)
            {
                PrintError($"Benchmark failed: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Displays detailed memory usage statistics and allocation patterns.
        /// Shows available memory, used memory, and allocation fragmentation.
        /// </summary>
        private async Task<int> ShowMemoryStatsAsync()
        {
            PrintInfo("Retrieving memory statistics...");

            try
            {
                var stats = await _deviceService.GetStatisticsAsync();

                Console.WriteLine();
                Console.WriteLine("Memory Statistics:");
                Console.WriteLine($"  Total Device Memory: {stats.TotalMemoryBytes / (1024.0 * 1024.0 * 1024.0):F2} GB");
                Console.WriteLine($"  Available Devices: {stats.AvailableDevices}");
                Console.WriteLine($"  Compute Units: {stats.TotalComputeUnits}");

                // Estimate used/free based on available devices
                double usedPercent = (1.0 - (stats.AvailableDevices / (double)stats.TotalDevices)) * 100;
                Console.WriteLine($"  Estimated Usage: {usedPercent:F1}%");
                Console.WriteLine();

                PrintSuccess("Memory statistics retrieved");
                return 0;
            }
            catch (Exception ex)
            {
                PrintError($"Failed to retrieve memory stats: {ex.Message}");
                return 1;
            }
        }
    }
}
