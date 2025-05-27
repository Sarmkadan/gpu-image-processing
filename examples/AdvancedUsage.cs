#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Advanced Usage Example - Configuration, custom options, and error handling
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Examples
{
    /// <summary>
    /// Advanced Usage: Configuration, Custom Options, and Error Handling
    ///
    /// This example demonstrates advanced usage patterns including:
    /// 1. Custom configuration with performance tuning
    /// 2. Multiple filter types and parameter customization
    /// 3. Error handling and validation
    /// 4. Performance monitoring
    /// 5. Custom processing profiles
    /// </summary>
    class AdvancedUsage
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Advanced Usage ===\n");

            try
            {
                // ===================================================================
                // STEP 1: Advanced Configuration
                // ===================================================================
                Console.WriteLine("Configuring advanced settings...\n");

                var settings = ConfigurationValidator.CreateDefaultSettings();

                // Performance tuning - customize for your needs
                settings.Processing.MaxParallelOperations = 4;      // Process 4 images at once
                settings.Processing.UseGPUAcceleration = true;    // Use GPU if available
                settings.Processing.TimeoutSeconds = 300;         // 5 minute timeout
                settings.Processing.BatchSize = 10;              // Process 10 images per batch
                settings.Processing.EnablePrecision = "float32";   // High precision
                settings.Security.ValidateImageDimensions = true;   // Validate image sizes
                settings.Security.MaxImageWidth = 8192;           // Max width limit
                settings.Security.MaxImageHeight = 8192;          // Max height limit

                // Storage configuration
                settings.Storage.InputDirectory = "./input/";
                settings.Storage.OutputDirectory = "./output/";
                settings.Storage.TempDirectory = "./temp/";

                // Cache configuration
                settings.Cache.EnableDistributedCache = true;
                settings.Cache.CacheTTLSeconds = 3600;          // Cache for 1 hour

                // Device preferences
                settings.Device.PreferredDeviceType = "GPU";
                settings.Device.AllowFallbackToCPU = true;        // Fallback to CPU if GPU unavailable

                // ===================================================================
                // STEP 2: Initialize with custom configuration
                // ===================================================================
                Console.WriteLine("Initializing service provider...\n");

                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                // Get services
                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();
                var filterService = serviceProvider
                    .GetRequiredService<FilterService>();
                var transformService = serviceProvider
                    .GetRequiredService<TransformService>();
                var deviceService = serviceProvider
                    .GetRequiredService<DeviceService>();
                var performanceService = serviceProvider
                    .GetRequiredService<PerformanceMonitoringService>();

                // ===================================================================
                // STEP 3: Device Management
                // ===================================================================
                Console.WriteLine("Checking available devices...\n");

                var devices = await deviceService.GetAvailableDevicesAsync();
                Console.WriteLine($"Found {devices.Count} compute devices:");

                foreach (var device in devices)
                {
                    Console.WriteLine($" - {device.Name} ({device.DeviceType})");
                    Console.WriteLine($"   Memory: {device.GlobalMemoryMB}MB");
                    Console.WriteLine($"   Compute Units: {device.ComputeUnits}");
                }

                // Select preferred device (optional)
                if (devices.Count > 0)
                {
                    await deviceService.SelectDeviceAsync(devices[0].Id);
                    Console.WriteLine($"\n✓ Selected device: {devices[0].Name}");
                }

                // ===================================================================
                // STEP 4: Create Multiple Filters with Custom Parameters
                // ===================================================================
                Console.WriteLine("\nCreating filters with custom parameters...\n");

                // Gaussian Blur - soft blur effect
                var gaussianFilter = await filterService.CreateFilterAsync(
                    FilterType.Gaussian,
                    "SoftBlur",
                    "Soft Gaussian blur for background effects"
                );
                await filterService.UpdateFilterParametersAsync(gaussianFilter.Id, new Dictionary<string, float>
                {
                    { "Sigma", 3.5f },      // Standard deviation
                    { "KernelSize", 7f }     // 7x7 kernel size
                });
                Console.WriteLine($"✓ Created Gaussian blur filter (ID: {gaussianFilter.Id})");

                // Bilateral Filter - edge-preserving blur
                var bilateralFilter = await filterService.CreateFilterAsync(
                    FilterType.Bilateral,
                    "SmartBlur",
                    "Edge-preserving blur that keeps edges sharp"
                );
                await filterService.UpdateFilterParametersAsync(bilateralFilter.Id, new Dictionary<string, float>
                {
                    { "SigmaColor", 75.0f },
                    { "SigmaSpace", 75.0f },
                    { "KernelSize", 5f }
                });
                Console.WriteLine($"✓ Created bilateral filter (ID: {bilateralFilter.Id})");

                // Sharpen Filter - enhance details
                var sharpenFilter = await filterService.CreateFilterAsync(
                    FilterType.Sharpen,
                    "EnhanceDetails",
                    "Sharpen image details"
                );
                await filterService.UpdateFilterParametersAsync(sharpenFilter.Id, new Dictionary<string, float>
                {
                    { "Strength", 1.2f }     // Sharpen strength
                });
                Console.WriteLine($"✓ Created sharpen filter (ID: {sharpenFilter.Id})");

                // ===================================================================
                // STEP 5: Create Transforms
                // ===================================================================
                Console.WriteLine("\nCreating transforms...\n");

                // Resize transform
                var resizeTransform = await transformService.CreateTransformAsync(
                    TransformType.Resize,
                    "ScaleDown",
                    "Reduce image size by 50%"
                );
                await transformService.UpdateTransformParametersAsync(resizeTransform.Id, new Dictionary<string, float>
                {
                    { "ScaleX", 0.5f },
                    { "ScaleY", 0.5f }
                });
                Console.WriteLine($"✓ Created resize transform (ID: {resizeTransform.Id})");

                // ===================================================================
                // STEP 6: Create Custom Processing Profile
                // ===================================================================
                Console.WriteLine("\nCreating custom processing profile...\n");

                var profile = new ProcessingProfile
                {
                    Name = "HighQualityProfile",
                    Description = "High quality processing with full precision",
                    MaxParallelOperations = 2,          // Sequential for quality
                    BatchSize = 5,                     // Smaller batches
                    EnablePrecision = "float32",        // Full precision
                    CacheResults = true,                 // Cache results
                    SkipValidation = false              // Full validation
                };

                // Note: In actual usage, you would register this profile with the service
                Console.WriteLine($"✓ Custom profile created: {profile.Name}");
                Console.WriteLine($"  Parallel ops: {profile.MaxParallelOperations}");
                Console.WriteLine($"  Batch size: {profile.BatchSize}");
                Console.WriteLine($"  Precision: {profile.EnablePrecision}");

                // ===================================================================
                // STEP 7: Register Image with Error Handling
                // ===================================================================
                Console.WriteLine("\nRegistering image...\n");

                Image? image = null;
                try
                {
                    image = await imageProcessing.RegisterImageAsync("input.jpg", "AdvancedImage");
                    Console.WriteLine($"✓ Image registered successfully");
                    Console.WriteLine($"  ID: {image.Id}");
                    Console.WriteLine($"  Name: {image.Name}");
                    Console.WriteLine($"  Format: {image.Format}");
                    Console.WriteLine($"  Size: {image.Width}x{image.Height}");
                }
                catch (Exception ex) when (ex is ArgumentException || ex is FileNotFoundException)
                {
                    Console.WriteLine($"⚠ Warning: {ex.Message}");
                    Console.WriteLine("  Using placeholder processing instead.");

                    // Create a placeholder image
                    image = new Image
                    {
                        Id = Guid.NewGuid(),
                        Name = "Placeholder",
                        Format = "JPEG",
                        Width = 1920,
                        Height = 1080
                    };
                }

                // ===================================================================
                // STEP 8: Process Image with Multiple Filters and Transforms
                // ===================================================================
                Console.WriteLine("\nProcessing image with multiple operations...\n");

                var filterIds = new List<Guid> { gaussianFilter.Id, bilateralFilter.Id, sharpenFilter.Id };
                var transformIds = new List<Guid> { resizeTransform.Id };

                var processingStart = DateTime.UtcNow;

                var result = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    filterIds,
                    transformIds,
                    Guid.Empty // Use default profile
                );

                var processingTime = DateTime.UtcNow - processingStart;

                // ===================================================================
                // STEP 9: Validate and Display Results
                // ===================================================================
                Console.WriteLine("\nProcessing results:");
                Console.WriteLine($"✓ Status: {result.Status}");
                Console.WriteLine($"✓ Processing time: {processingTime.TotalMilliseconds:F2}ms");
                Console.WriteLine($"✓ Output path: {result.OutputPath}");
                Console.WriteLine($"✓ Output size: {result.OutputSizeBytes / 1024.0:F2}KB");

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Console.WriteLine($"✗ Error message: {result.ErrorMessage}");
                }

                // ===================================================================
                // STEP 10: Performance Monitoring
                // ===================================================================
                Console.WriteLine("\nPerformance metrics:");
                var metrics = await performanceService.GetMetricsAsync();
                Console.WriteLine($"  GPU Utilization: {metrics.GpuUtilization}%");
                Console.WriteLine($"  Memory Usage: {metrics.MemoryUsageMB}MB");
                Console.WriteLine($"  Active Jobs: {metrics.ActiveJobsCount}");

                Console.WriteLine("\n✓ Advanced usage example completed successfully!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Critical Error: {ex.Message}");
                Console.WriteLine("\nStack Trace:");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                // Return non-zero exit code to indicate failure
                Environment.Exit(1);
            }
        }
    }
}