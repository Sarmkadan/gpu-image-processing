#nullable enable
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
    /// Example 5: Advanced Filtering and Custom Profiles
    ///
    /// Demonstrates applying multiple advanced filters and using custom
    /// processing profiles for speed/quality optimization.
    ///
    /// Prerequisites:
    /// - An image file "photo.jpg" in the current directory
    /// </summary>
    class AdvancedFilteringExample
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Advanced Filtering Example ===\n");

            try
            {
                var settings = ConfigurationValidator.CreateDefaultSettings();
                settings.ConfigureForDevelopment();

                Console.WriteLine("Initializing GPU service...");
                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();
                var filterService = serviceProvider
                    .GetRequiredService<FilterService>();

                // Register image
                const string imageFile = "photo.jpg";
                var image = await imageProcessing.RegisterImageAsync(imageFile, "Original");
                Console.WriteLine($"✓ Image registered: {image.Id}\n");

                // Create and configure filters
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("ADVANCED FILTERS");
                Console.WriteLine(new string('=', 60) + "\n");

                var filters = new List<Guid>();

                // Filter 1: Bilateral Filter (edge-preserving blur)
                Console.WriteLine("1. Bilateral Filter (edge-preserving blur)");
                var bilateralFilter = await filterService.CreateFilterAsync(
                    FilterType.Bilateral,
                    "Bilateral",
                    "Reduces noise while preserving edges"
                );
                await filterService.UpdateFilterParametersAsync(
                    bilateralFilter.Id,
                    new Dictionary<string, float>
                    {
                        { "SigmaColor", 75.0f },   // Color sigma
                        { "SigmaSpace", 75.0f }    // Spatial sigma
                    }
                );
                filters.Add(bilateralFilter.Id);
                Console.WriteLine("  Parameters: SigmaColor=75, SigmaSpace=75");
                Console.WriteLine($"  ✓ Filter created\n");

                // Filter 2: Median Filter (salt-and-pepper noise removal)
                Console.WriteLine("2. Median Filter (noise removal)");
                var medianFilter = await filterService.CreateFilterAsync(
                    FilterType.Median,
                    "Median",
                    "Removes salt-and-pepper noise"
                );
                await filterService.UpdateFilterParametersAsync(
                    medianFilter.Id,
                    new Dictionary<string, float>
                    {
                        { "KernelSize", 5f }       // 5x5 kernel
                    }
                );
                filters.Add(medianFilter.Id);
                Console.WriteLine("  Parameters: KernelSize=5");
                Console.WriteLine($"  ✓ Filter created\n");

                // Filter 3: Sobel Edge Detection
                Console.WriteLine("3. Sobel Edge Detection");
                var sobelFilter = await filterService.CreateFilterAsync(
                    FilterType.Sobel,
                    "Sobel",
                    "Detects edges in the image"
                );
                await filterService.UpdateFilterParametersAsync(
                    sobelFilter.Id,
                    new Dictionary<string, float>
                    {
                        { "ThresholdLow", 50f },   // Low threshold
                        { "ThresholdHigh", 150f }  // High threshold
                    }
                );
                filters.Add(sobelFilter.Id);
                Console.WriteLine("  Parameters: ThresholdLow=50, ThresholdHigh=150");
                Console.WriteLine($"  ✓ Filter created\n");

                // Filter 4: Canny Edge Detection
                Console.WriteLine("4. Canny Edge Detection");
                var cannyFilter = await filterService.CreateFilterAsync(
                    FilterType.Canny,
                    "Canny",
                    "Advanced edge detection algorithm"
                );
                await filterService.UpdateFilterParametersAsync(
                    cannyFilter.Id,
                    new Dictionary<string, float>
                    {
                        { "Threshold1", 100f },    // First threshold
                        { "Threshold2", 200f }     // Second threshold
                    }
                );
                filters.Add(cannyFilter.Id);
                Console.WriteLine("  Parameters: Threshold1=100, Threshold2=200");
                Console.WriteLine($"  ✓ Filter created\n");

                // Filter 5: Morphological Operations
                Console.WriteLine("5. Morphological Operations");
                var morphFilter = await filterService.CreateFilterAsync(
                    FilterType.Morphological,
                    "Morphology",
                    "Erosion, dilation, opening, closing"
                );
                await filterService.UpdateFilterParametersAsync(
                    morphFilter.Id,
                    new Dictionary<string, float>
                    {
                        { "Operation", 0f }        // 0=Erosion, 1=Dilation, 2=Opening, 3=Closing
                    }
                );
                filters.Add(morphFilter.Id);
                Console.WriteLine("  Parameters: Operation=0 (Erosion)");
                Console.WriteLine($"  ✓ Filter created\n");

                // Process with different profiles
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("PROCESSING PROFILES COMPARISON");
                Console.WriteLine(new string('=', 60) + "\n");

                // Profile 1: Speed Optimized
                Console.WriteLine("Profile 1: Speed-Optimized");
                Console.WriteLine("  Settings: Float16, 8 parallel ops, batch size 20");
                var speedSettings = ConfigurationValidator.CreateDefaultSettings();
                speedSettings.Processing.MaxParallelOperations = 8;
                speedSettings.Processing.BatchSize = 20;
                speedSettings.Processing.Precision = "float16";

                var speedStart = DateTime.UtcNow;
                var speedResult = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    filters,
                    new List<Guid>(),
                    Guid.Empty
                );
                var speedDuration = DateTime.UtcNow - speedStart;
                Console.WriteLine($"  Result: {speedResult.Status}");
                Console.WriteLine($"  Duration: {speedDuration.TotalMilliseconds:F2}ms\n");

                // Profile 2: Quality-Optimized
                Console.WriteLine("Profile 2: Quality-Optimized");
                Console.WriteLine("  Settings: Float32, 1 parallel op, batch size 5");
                var qualitySettings = ConfigurationValidator.CreateDefaultSettings();
                qualitySettings.Processing.MaxParallelOperations = 1;
                qualitySettings.Processing.BatchSize = 5;
                qualitySettings.Processing.Precision = "float32";

                var qualityStart = DateTime.UtcNow;
                var qualityResult = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    filters,
                    new List<Guid>(),
                    Guid.Empty
                );
                var qualityDuration = DateTime.UtcNow - qualityStart;
                Console.WriteLine($"  Result: {qualityResult.Status}");
                Console.WriteLine($"  Duration: {qualityDuration.TotalMilliseconds:F2}ms\n");

                // Profile 3: Balanced (Default)
                Console.WriteLine("Profile 3: Balanced (Default)");
                Console.WriteLine("  Settings: Float32, 4 parallel ops, batch size 10");

                var balancedStart = DateTime.UtcNow;
                var balancedResult = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    filters,
                    new List<Guid>(),
                    Guid.Empty
                );
                var balancedDuration = DateTime.UtcNow - balancedStart;
                Console.WriteLine($"  Result: {balancedResult.Status}");
                Console.WriteLine($"  Duration: {balancedDuration.TotalMilliseconds:F2}ms\n");

                // Summary
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("SUMMARY");
                Console.WriteLine(new string('=', 60) + "\n");

                Console.WriteLine("Processing Comparison:");
                Console.WriteLine($"  Speed-Optimized:  {speedDuration.TotalMilliseconds:F2}ms (baseline)");

                var qualityRatio = qualityDuration.TotalMilliseconds / speedDuration.TotalMilliseconds;
                Console.WriteLine($"  Quality-Optimized: {qualityDuration.TotalMilliseconds:F2}ms ({qualityRatio:F1}x slower)");

                var balancedRatio = balancedDuration.TotalMilliseconds / speedDuration.TotalMilliseconds;
                Console.WriteLine($"  Balanced:         {balancedDuration.TotalMilliseconds:F2}ms ({balancedRatio:F1}x slower)");

                Console.WriteLine("\nFilter Information:");
                Console.WriteLine("  Bilateral: Edge-preserving noise reduction");
                Console.WriteLine("  Median: Effective for salt-and-pepper noise");
                Console.WriteLine("  Sobel: Fast edge detection");
                Console.WriteLine("  Canny: Higher quality edge detection");
                Console.WriteLine("  Morphological: Binary image operations");

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
