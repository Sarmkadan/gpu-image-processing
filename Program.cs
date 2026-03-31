// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== GPU-Accelerated Image Processing System ===");
            Console.WriteLine("Author: Vladyslav Zaiets");
            Console.WriteLine("Website: https://sarmkadan.com\n");

            try
            {
                var settings = ConfigurationValidator.CreateDefaultSettings();
                settings.ConfigureForDevelopment();

                Console.WriteLine("Initializing application...");
                var serviceProvider = await DependencyInjectionSetup.CreateAndInitializeServiceProviderAsync(settings);

                await RunApplicationAsync(serviceProvider);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nCritical Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Runs the main application workflow
        /// </summary>
        private static async Task RunApplicationAsync(IServiceProvider serviceProvider)
        {
            var deviceService = serviceProvider.GetRequiredService<DeviceService>();
            var imageProcessing = serviceProvider.GetRequiredService<ImageProcessingService>();
            var batchProcessing = serviceProvider.GetRequiredService<BatchProcessingService>();
            var filterService = serviceProvider.GetRequiredService<FilterService>();
            var transformService = serviceProvider.GetRequiredService<TransformService>();

            // Display device information
            Console.WriteLine("\n--- Available Compute Devices ---");
            var deviceSummary = await deviceService.GetCapabilitiesSummaryAsync();
            Console.WriteLine(deviceSummary);

            var selectedDevice = deviceService.GetSelectedDevice();
            Console.WriteLine($"Selected Device: {selectedDevice?.Name}\n");

            // Create sample filters
            Console.WriteLine("--- Creating Sample Filters ---");
            var gaussianFilter = await filterService.CreateFilterAsync(FilterType.Gaussian, "GaussianBlur", "Gaussian blur with sigma=1.0");
            var sobelFilter = await filterService.CreateFilterAsync(FilterType.Sobel, "EdgeDetection", "Sobel edge detection");
            Console.WriteLine($"✓ Created Gaussian filter: {gaussianFilter.Id}");
            Console.WriteLine($"✓ Created Sobel filter: {sobelFilter.Id}\n");

            // Create sample transforms
            Console.WriteLine("--- Creating Sample Transforms ---");
            var rotateTransform = await transformService.CreateTransformAsync(TransformType.Rotate, "Rotation45", "Rotate by 45 degrees");
            var resizeTransform = await transformService.CreateTransformAsync(TransformType.Resize, "Scale2x", "Scale to 2x size");
            await transformService.SetParameterAsync(rotateTransform.Id, "Angle", 45f);
            await transformService.SetParameterAsync(resizeTransform.Id, "ScaleX", 2.0f);
            await transformService.SetParameterAsync(resizeTransform.Id, "ScaleY", 2.0f);
            Console.WriteLine($"✓ Created Rotation transform: {rotateTransform.Id}");
            Console.WriteLine($"✓ Created Resize transform: {resizeTransform.Id}\n");

            // Register sample images
            Console.WriteLine("--- Registering Sample Images ---");
            var image1 = await imageProcessing.RegisterImageAsync("./samples/image1.jpg", "Sample Image 1");
            var image2 = await imageProcessing.RegisterImageAsync("./samples/image2.png", "Sample Image 2");
            image1.Width = 1920;
            image1.Height = 1080;
            image1.Channels = 3;
            image1.FileSizeBytes = 2_000_000;
            image1.Format = ImageFormat.Jpeg;

            image2.Width = 3840;
            image2.Height = 2160;
            image2.Channels = 4;
            image2.FileSizeBytes = 8_000_000;
            image2.Format = ImageFormat.Png;

            Console.WriteLine($"✓ Registered Image 1: {image1.Id} ({image1.Width}x{image1.Height})");
            Console.WriteLine($"✓ Registered Image 2: {image2.Id} ({image2.Width}x{image2.Height})\n");

            // Create processing profile
            Console.WriteLine("--- Creating Processing Profile ---");
            var profile = ProcessingProfile.CreateBalanced();
            Console.WriteLine($"✓ Profile: {profile.Name}");
            Console.WriteLine($"  GPU Acceleration: {profile.UseGPUAcceleration}");
            Console.WriteLine($"  Batch Size: {profile.BatchSize}\n");

            // Create batch job
            Console.WriteLine("--- Creating Batch Processing Job ---");
            var job = await batchProcessing.CreateJobAsync(
                "Sample Processing Job",
                new List<Guid> { image1.Id, image2.Id },
                new List<Guid> { gaussianFilter.Id, sobelFilter.Id },
                new List<Guid> { rotateTransform.Id },
                profile.Id
            );
            Console.WriteLine($"✓ Created Job: {job.Id}");
            Console.WriteLine($"  Name: {job.Name}");
            Console.WriteLine($"  Images: {job.TotalImages}");
            Console.WriteLine($"  Status: {job.Status}\n");

            // Display statistics
            Console.WriteLine("--- System Statistics ---");
            var deviceStats = await deviceService.GetStatisticsAsync();
            var filterStats = await filterService.GetStatisticsAsync();
            var transformStats = await transformService.GetStatisticsAsync();

            Console.WriteLine($"Devices: {deviceStats.AvailableDevices}/{deviceStats.TotalDevices}");
            Console.WriteLine($"Total Device Memory: {deviceStats.TotalMemoryBytes / (1024.0 * 1024.0 * 1024.0):F2} GB");
            Console.WriteLine($"Compute Units: {deviceStats.TotalComputeUnits}");
            Console.WriteLine($"Filters: {filterStats.TotalFilters} ({filterStats.ActiveFilters} active)");
            Console.WriteLine($"Transforms: {transformStats.TotalTransforms} ({transformStats.ActiveTransforms} active)\n");

            // Show processing capability
            Console.WriteLine("--- Processing Capability ---");
            var canProcess = await imageProcessing.CanProcessAsync(new List<Guid> { image1.Id, image2.Id }, profile.Id);
            Console.WriteLine($"Can Process Images: {(canProcess ? "Yes ✓" : "No ✗")}\n");

            // Display configuration
            Console.WriteLine("--- Application Configuration ---");
            var settings = serviceProvider.GetRequiredService<ApplicationSettings>();
            Console.WriteLine($"Environment: {settings.Environment}");
            Console.WriteLine($"Max Batch Size: {settings.Processing.DefaultBatchSize}");
            Console.WriteLine($"Max Parallel Ops: {settings.Processing.MaxParallelOperations}");
            Console.WriteLine($"Log Level: {settings.Logging.LogLevel}");
            Console.WriteLine($"Output Directory: {settings.Storage.OutputDirectory}\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Application initialized successfully!");
            Console.WriteLine("✓ GPU image processing system is ready for use.");
            Console.ResetColor();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
