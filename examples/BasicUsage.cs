#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Basic Usage Example - Minimal setup and first call
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Examples
{
    /// <summary>
    /// Basic Usage: Minimal Setup and First Call
    ///
    /// This example demonstrates the absolute minimum code required to:
    /// 1. Initialize the GPU image processing system
    /// 2. Register an image
    /// 3. Apply a simple filter
    /// 4. Get the processed result
    ///
    /// This is the "Hello World" of GPU image processing.
    /// </summary>
    class BasicUsage
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Basic Usage ===\n");

            try
            {
                // STEP 1: Create default settings (minimal configuration)
                var settings = ConfigurationValidator.CreateDefaultSettings();

                // STEP 2: Initialize service provider
                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                // STEP 3: Get required services
                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();
                var filterService = serviceProvider
                    .GetRequiredService<FilterService>();

                // STEP 4: Register an image (replace "input.jpg" with your image path)
                var image = await imageProcessing.RegisterImageAsync("input.jpg", "MyImage");
                Console.WriteLine($"✓ Image registered: {image.Name} (ID: {image.Id})");

                // STEP 5: Create a simple Gaussian blur filter
                var filter = await filterService.CreateFilterAsync(
                    Core.Constants.FilterType.Gaussian,
                    "SimpleBlur",
                    "Basic blur filter"
                );

                // STEP 6: Process the image with the filter
                var result = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    new List<Guid> { filter.Id },  // Filter IDs to apply
                    new List<Guid>(),               // No transforms
                    Guid.Empty                       // Use default profile
                );

                // STEP 7: Check result
                Console.WriteLine($"✓ Processing complete!");
                Console.WriteLine($"  Status: {result.Status}");
                Console.WriteLine($"  Output: {result.OutputPath}");

                Console.WriteLine("\n✓ Basic usage example completed successfully!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }
    }
}