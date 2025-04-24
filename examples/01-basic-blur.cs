// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Examples
{
    /// <summary>
    /// Example 1: Basic Image Blurring
    ///
    /// This example demonstrates the most basic image processing operation:
    /// applying a Gaussian blur filter to a single image.
    ///
    /// Prerequisites:
    /// - An image file "photo.jpg" in the current directory
    /// - GPU with OpenCL support or CPU fallback enabled
    /// </summary>
    class BasicBlurExample
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Basic Blur Example ===\n");

            try
            {
                // Initialize settings with default configuration
                var settings = ConfigurationValidator.CreateDefaultSettings();
                settings.ConfigureForDevelopment();

                Console.WriteLine("Initializing GPU service...");
                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                // Get required services
                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();
                var filterService = serviceProvider
                    .GetRequiredService<FilterService>();
                var deviceService = serviceProvider
                    .GetRequiredService<DeviceService>();

                // Check available devices
                var devices = await deviceService.GetAvailableDevicesAsync();
                Console.WriteLine($"Found {devices.Count} compute devices:");
                foreach (var device in devices)
                {
                    Console.WriteLine($"  - {device.Name} ({device.DeviceType})");
                }

                // Verify input file exists
                const string imageFile = "photo.jpg";
                if (!File.Exists(imageFile))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nNote: {imageFile} not found. Using placeholder processing.");
                    Console.ResetColor();
                    Console.WriteLine("To test with a real image, place 'photo.jpg' in the current directory.\n");
                }

                // Register image
                Console.WriteLine($"\nRegistering image: {imageFile}");
                var image = await imageProcessing.RegisterImageAsync(imageFile, "MyPhoto");
                Console.WriteLine($"✓ Image registered with ID: {image.Id}");
                Console.WriteLine($"  Name: {image.Name}");
                Console.WriteLine($"  Format: {image.Format}");
                Console.WriteLine($"  Size: {image.Width}x{image.Height}");

                // Create Gaussian blur filter
                Console.WriteLine("\nCreating Gaussian blur filter...");
                var blurFilter = await filterService.CreateFilterAsync(
                    FilterType.Gaussian,
                    "Gaussian Blur",
                    "Standard Gaussian blur filter"
                );
                Console.WriteLine($"✓ Filter created with ID: {blurFilter.Id}");

                // Configure filter parameters
                Console.WriteLine("\nConfiguring filter parameters...");
                var parameters = new Dictionary<string, float>
                {
                    { "Sigma", 2.0f },      // Standard deviation for Gaussian kernel
                    { "KernelSize", 5f }   // Kernel size (5x5)
                };

                await filterService.UpdateFilterParametersAsync(blurFilter.Id, parameters);
                Console.WriteLine("✓ Parameters configured:");
                foreach (var param in parameters)
                {
                    Console.WriteLine($"  - {param.Key}: {param.Value}");
                }

                // Process image
                Console.WriteLine("\nProcessing image...");
                var startTime = DateTime.UtcNow;

                var result = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    new List<Guid> { blurFilter.Id },
                    new List<Guid>(),      // No transforms
                    Guid.Empty             // Use default profile
                );

                var duration = DateTime.UtcNow - startTime;

                // Display results
                Console.WriteLine("✓ Processing complete!");
                Console.WriteLine($"\nResults:");
                Console.WriteLine($"  Status: {result.Status}");
                Console.WriteLine($"  Duration: {duration.TotalMilliseconds:F2}ms");
                Console.WriteLine($"  Output Path: {result.OutputPath}");
                Console.WriteLine($"  Output Size: {result.OutputSizeBytes / 1024.0:F2}KB");

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Console.WriteLine($"  Error: {result.ErrorMessage}");
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
    }
}
