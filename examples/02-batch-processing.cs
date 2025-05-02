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
    /// Example 2: Batch Image Processing
    ///
    /// Process multiple images in a single batch job with progress tracking.
    /// This demonstrates the BatchProcessingService for high-throughput operations.
    ///
    /// Prerequisites:
    /// - Multiple image files in ./images/ directory
    /// - GPU with sufficient memory for parallel processing
    /// </summary>
    class BatchProcessingExample
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Batch Processing Example ===\n");

            try
            {
                // Initialize settings with batch optimization
                var settings = ConfigurationValidator.CreateDefaultSettings();
                settings.Processing.MaxParallelOperations = 4;
                settings.Processing.BatchSize = 10;
                settings.ConfigureForDevelopment();

                Console.WriteLine("Initializing GPU service...");
                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                // Get required services
                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();
                var filterService = serviceProvider
                    .GetRequiredService<FilterService>();
                var batchService = serviceProvider
                    .GetRequiredService<BatchProcessingService>();

                // Create filters to apply
                Console.WriteLine("\nCreating filters...");
                var filters = new List<Guid>();

                var gaussianFilter = await filterService.CreateFilterAsync(
                    FilterType.Gaussian,
                    "Gaussian Blur",
                    "Blur filter for batch processing"
                );
                await filterService.UpdateFilterParametersAsync(
                    gaussianFilter.Id,
                    new Dictionary<string, float> { { "Sigma", 1.5f } }
                );
                filters.Add(gaussianFilter.Id);
                Console.WriteLine($"✓ Created Gaussian Blur filter");

                var sobelFilter = await filterService.CreateFilterAsync(
                    FilterType.Sobel,
                    "Sobel Edge Detection",
                    "Edge detection filter"
                );
                await filterService.UpdateFilterParametersAsync(
                    sobelFilter.Id,
                    new Dictionary<string, float>
                    {
                        { "ThresholdLow", 50f },
                        { "ThresholdHigh", 150f }
                    }
                );
                filters.Add(sobelFilter.Id);
                Console.WriteLine($"✓ Created Sobel Edge Detection filter");

                // Register images
                Console.WriteLine("\nRegistering images...");
                var imageIds = new List<Guid>();
                var imageDirectory = "./images/";

                if (Directory.Exists(imageDirectory))
                {
                    var imageFiles = Directory.GetFiles(imageDirectory, "*.jpg")
                        .Concat(Directory.GetFiles(imageDirectory, "*.png"))
                        .Take(10);

                    foreach (var file in imageFiles)
                    {
                        var image = await imageProcessing.RegisterImageAsync(
                            file,
                            Path.GetFileNameWithoutExtension(file)
                        );
                        imageIds.Add(image.Id);
                        Console.WriteLine($"✓ Registered: {Path.GetFileName(file)}");
                    }

                    if (imageIds.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("No images found. Creating simulated batch...");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Note: {imageDirectory} not found. Using simulated batch.");
                    Console.ResetColor();
                }

                // Create batch job
                Console.WriteLine("\nCreating batch job...");
                var job = await batchService.CreateJobAsync(
                    "Batch Filter Processing",
                    imageIds,
                    filters,
                    new List<Guid>(),  // No transforms
                    Guid.Empty         // Default profile
                );

                Console.WriteLine($"✓ Batch job created with ID: {job.Id}");
                Console.WriteLine($"  Total images: {job.TotalCount}");
                Console.WriteLine($"  Filters applied: {filters.Count}");
                Console.WriteLine($"  Status: {job.Status}");

                // Execute batch job
                Console.WriteLine("\nExecuting batch job...");
                await batchService.ExecuteJobAsync(job.Id, Guid.Empty).ConfigureAwait(false);

                // Monitor progress
                var updateInterval = 500; // milliseconds
                while (true)
                {
                    var updatedJob = await batchService.GetJobAsync(job.Id).ConfigureAwait(false);

                    // Display progress bar
                    var progress = (int)((updatedJob.ProcessedCount / (double)updatedJob.TotalCount) * 50);
                    var progressBar = new string('█', progress) + new string('░', 50 - progress);
                    Console.Write($"\r[{progressBar}] {updatedJob.ProcessedCount}/{updatedJob.TotalCount}");

                    if (updatedJob.Status == "Completed" || updatedJob.Status == "Failed")
                    {
                        Console.WriteLine(); // New line after progress bar
                        break;
                    }

                    await Task.Delay(updateInterval).ConfigureAwait(false);
                }

                // Display final results
                var finalJob = await batchService.GetJobAsync(job.Id).ConfigureAwait(false);
                Console.WriteLine("\n✓ Batch processing complete!");
                Console.WriteLine($"\nJob Summary:");
                Console.WriteLine($"  Status: {finalJob.Status}");
                Console.WriteLine($"  Total Processed: {finalJob.ProcessedCount}");
                Console.WriteLine($"  Successful: {finalJob.SuccessCount}");
                Console.WriteLine($"  Failed: {finalJob.FailureCount}");
                Console.WriteLine($"  Duration: {(finalJob.CompletedAt - finalJob.StartedAt)?.TotalSeconds:F2}s");

                // Calculate throughput
                if (finalJob.CompletedAt.HasValue && finalJob.StartedAt.HasValue)
                {
                    var duration = (finalJob.CompletedAt.Value - finalJob.StartedAt.Value).TotalSeconds;
                    var throughput = finalJob.ProcessedCount / duration;
                    Console.WriteLine($"  Throughput: {throughput:F2} images/second");
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
