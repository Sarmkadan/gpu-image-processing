#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Examples
{
    /// <summary>
    /// Example 4: Performance Monitoring and Metrics
    ///
    /// Monitor GPU utilization, memory usage, throughput, and other performance
    /// metrics during image processing operations.
    ///
    /// Prerequisites:
    /// - GPU with OpenCL support or CPU mode
    /// - Performance monitoring service availability
    /// </summary>
    class PerformanceMonitoringExample
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Performance Monitoring Example ===\n");

            try
            {
                var settings = ConfigurationValidator.CreateDefaultSettings();
                settings.ConfigureForDevelopment();

                Console.WriteLine("Initializing GPU service...");
                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                var deviceService = serviceProvider
                    .GetRequiredService<DeviceService>();
                var perfService = serviceProvider
                    .GetRequiredService<PerformanceMonitoringService>();

                // Display device information
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("DEVICE INFORMATION");
                Console.WriteLine(new string('=', 60));

                var devices = await deviceService.GetAvailableDevicesAsync();
                Console.WriteLine($"\nTotal Devices: {devices.Count}\n");

                foreach (var device in devices)
                {
                    Console.WriteLine($"Device: {device.Name}");
                    Console.WriteLine($"  Type: {device.DeviceType}");
                    Console.WriteLine($"  Vendor: {device.Vendor}");
                    Console.WriteLine($"  Memory: {device.GlobalMemoryMB}MB");
                    Console.WriteLine($"  Compute Units: {device.ComputeUnits}");
                    Console.WriteLine($"  Clock Speed: {device.ClockFrequencyMHz}MHz");
                    Console.WriteLine($"  Available: {(device.Available ? "Yes" : "No")}");

                    // Get device capabilities
                    var capabilities = await deviceService.GetDeviceCapabilitiesAsync(device.Id);
                    if (capabilities.Count > 0)
                    {
                        Console.WriteLine("  Capabilities:");
                        foreach (var cap in capabilities.Take(5))
                        {
                            Console.WriteLine($"    - {cap.Key}: {cap.Value}");
                        }
                        if (capabilities.Count > 5)
                        {
                            Console.WriteLine($"    ... and {capabilities.Count - 5} more");
                        }
                    }
                    Console.WriteLine();
                }

                // Select preferred device
                if (devices.Count > 0)
                {
                    var selectedDevice = devices.FirstOrDefault(d => d.DeviceType == "GPU")
                        ?? devices.First();

                    Console.WriteLine($"Selecting device: {selectedDevice.Name}");
                    await deviceService.SelectDeviceAsync(selectedDevice.Id);
                    Console.WriteLine("✓ Device selected\n");
                }

                // Display initial metrics
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("INITIAL PERFORMANCE METRICS");
                Console.WriteLine(new string('=', 60) + "\n");

                var initialMetrics = await perfService.GetMetricsAsync();
                DisplayMetrics(initialMetrics, "Initial");

                // Simulate processing load and monitor metrics
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("MONITORING METRICS DURING PROCESSING");
                Console.WriteLine(new string('=', 60) + "\n");

                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();

                // Register test image if available
                const string testImage = "photo.jpg";
                if (File.Exists(testImage))
                {
                    var image = await imageProcessing.RegisterImageAsync(testImage, "TestImage");
                    Console.WriteLine($"Registered test image: {image.Id}\n");

                    // Monitor metrics over time
                    var samplingInterval = 100; // milliseconds
                    var samplingDuration = 5000; // 5 seconds
                    var startTime = DateTime.UtcNow;

                    Console.WriteLine("Time\t\tGPU%\t\tMemory\t\tThroughput");
                    Console.WriteLine(new string('-', 60));

                    while ((DateTime.UtcNow - startTime).TotalMilliseconds < samplingDuration)
                    {
                        var metrics = await perfService.GetMetricsAsync();

                        var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                        Console.WriteLine(
                            $"{elapsed:F1}s\t\t{metrics.GpuUtilization:F1}%\t\t" +
                            $"{metrics.MemoryUsedMB}/{metrics.MemoryAvailableMB}MB\t\t" +
                            $"{metrics.ImagesPerSecond:F2}img/s"
                        );

                        await Task.Delay(samplingInterval);
                    }
                }
                else
                {
                    Console.WriteLine($"Note: {testImage} not found. Skipping processing simulation.");
                    Console.WriteLine("To test monitoring during processing, place 'photo.jpg' in the current directory.\n");
                }

                // Display final metrics
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("FINAL PERFORMANCE METRICS");
                Console.WriteLine(new string('=', 60) + "\n");

                var finalMetrics = await perfService.GetMetricsAsync();
                DisplayMetrics(finalMetrics, "Final");

                // Performance analysis
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("PERFORMANCE ANALYSIS");
                Console.WriteLine(new string('=', 60) + "\n");

                Console.WriteLine("Metrics Explanation:");
                Console.WriteLine("  • GPU Utilization: Percentage of GPU being actively used");
                Console.WriteLine("  • Memory Used: Current GPU memory consumption");
                Console.WriteLine("  • Throughput: Images processed per second");
                Console.WriteLine("  • Processing Time: Average time per image");
                Console.WriteLine("  • Active Jobs: Number of concurrent processing jobs");

                Console.WriteLine("\nOptimization Tips:");
                if (finalMetrics.GpuUtilization < 50)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠ Low GPU utilization - consider increasing batch size");
                    Console.ResetColor();
                }

                if (finalMetrics.MemoryUsedMB > finalMetrics.MemoryAvailableMB * 0.8)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠ High memory usage - consider reducing batch size");
                    Console.ResetColor();
                }

                if (finalMetrics.AverageProcessingTimeMs > 100)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠ Slow processing - verify GPU drivers are up-to-date");
                    Console.ResetColor();
                }

                Console.WriteLine("\n✓ Example completed successfully!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Display performance metrics in formatted output
        /// </summary>
        static void DisplayMetrics(PerformanceMetrics metrics, string label)
        {
            Console.WriteLine($"{label} Metrics:");
            Console.WriteLine($"  GPU Utilization:        {metrics.GpuUtilization:F1}%");
            Console.WriteLine($"  Memory Used:            {metrics.MemoryUsedMB}MB / {metrics.MemoryAvailableMB}MB");
            Console.WriteLine($"  Throughput:             {metrics.ImagesPerSecond:F2} images/sec");
            Console.WriteLine($"  Avg Processing Time:    {metrics.AverageProcessingTimeMs:F2}ms");
            Console.WriteLine($"  Active Jobs:            {metrics.ActiveJobCount}");
            Console.WriteLine($"  Total Processed:        {metrics.TotalProcessedBytes / (1024 * 1024)}MB");
        }
    }
}
