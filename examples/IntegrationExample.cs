#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Integration Example - ASP.NET Core Dependency Injection
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Examples
{
    /// <summary>
    /// Integration Example: ASP.NET Core Dependency Injection
    ///
    /// This example demonstrates how to integrate GPU Image Processing into an
    /// ASP.NET Core application using dependency injection.
    ///
    /// It shows:
    /// 1. Configuring services in Program.cs
    /// 2. Using the library in controllers
    /// 3. Best practices for production deployment
    /// 4. Integration with ASP.NET Core's built-in DI container
    /// </summary>
    class IntegrationExample
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: ASP.NET Core Integration ===\n");

            try
            {
                // ===================================================================
                // STEP 1: Simulate ASP.NET Core Program.cs Configuration
                // ===================================================================
                Console.WriteLine("Simulating ASP.NET Core application startup...\n");

                // Create a mock configuration (in real app, this would come from appsettings.json)
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"Application:Name", "GPU Image Processing API"},
                        {"Application:Environment", "Development"},
                        {"Processing:MaxParallelOperations", "4"},
                        {"Processing:UseGPUAcceleration", "true"},
                        {"Storage:InputDirectory", "./input/"},
                        {"Storage:OutputDirectory", "./output/"},
                        {"Cache:EnableDistributedCache", "true"},
                        {"Device:PreferredDeviceType", "GPU"}
                    })
                    .Build();

                // ===================================================================
                // STEP 2: Configure Services (equivalent to Program.cs)
                // ===================================================================
                Console.WriteLine("Configuring GPU Image Processing services...\n");

                var services = new ServiceCollection();

                // Add application settings from configuration
                var settings = ConfigurationValidator.CreateDefaultSettings();
                services.AddSingleton(settings);

                // Configure GPU Image Processing services
                await DependencyInjectionSetup.ConfigureServicesAsync(services, settings);

                // Add your application services
                services.AddSingleton<IHostApplicationLifetime, MockApplicationLifetime>();
                services.AddLogging();
                services.AddControllers();

                // Build service provider (equivalent to var app = builder.Build())
                var serviceProvider = services.BuildServiceProvider();

                Console.WriteLine("✓ Service provider configured successfully");
                Console.WriteLine("✓ GPU Image Processing services registered");

                // ===================================================================
                // STEP 3: Resolve Services (equivalent to constructor injection)
                // ===================================================================
                Console.WriteLine("\nResolving services for use...\n");

                var imageProcessing = serviceProvider.GetRequiredService<ImageProcessingService>();
                var filterService = serviceProvider.GetRequiredService<FilterService>();
                var deviceService = serviceProvider.GetRequiredService<DeviceService>();
                var batchService = serviceProvider.GetRequiredService<BatchProcessingService>();

                Console.WriteLine("✓ Services resolved successfully:");
                Console.WriteLine("  - ImageProcessingService");
                Console.WriteLine("  - FilterService");
                Console.WriteLine("  - DeviceService");
                Console.WriteLine("  - BatchProcessingService");

                // ===================================================================
                // STEP 4: Check Available Devices
                // ===================================================================
                Console.WriteLine("\nChecking GPU/CPU devices...\n");

                var devices = await deviceService.GetAvailableDevicesAsync();
                Console.WriteLine($"Found {devices.Count} compute devices:");

                foreach (var device in devices)
                {
                    Console.WriteLine($"  - {device.Name} ({device.DeviceType})");
                }

                if (devices.Count > 0)
                {
                    await deviceService.SelectDeviceAsync(devices[0].Id);
                    Console.WriteLine($"\n✓ Selected device: {devices[0].Name}");
                }

                // ===================================================================
                // STEP 5: Create a Sample Controller (simulated)
                // ===================================================================
                Console.WriteLine("\nSimulating ImageController usage...\n");

                // This simulates how you would use the services in a controller
                // In a real application, this would be in a controller class

                try
                {
                    // Simulate image upload and processing
                    Console.WriteLine("Simulating image upload and processing...");

                    // Register image (simulating uploaded file)
                    var image = await imageProcessing.RegisterImageAsync("uploaded-image.jpg", "UserUpload");
                    Console.WriteLine($"✓ Image registered: {image.Name} (ID: {image.Id})");

                    // Create filters (simulating user-selected filters)
                    var blurFilter = await filterService.CreateFilterAsync(
                        Core.Constants.FilterType.Gaussian,
                        "UserBlur",
                        "User-selected blur"
                    );
                    await filterService.UpdateFilterParametersAsync(blurFilter.Id, new Dictionary<string, float>
                    {
                        { "Sigma", 2.0f }
                    });

                    var sharpenFilter = await filterService.CreateFilterAsync(
                        Core.Constants.FilterType.Sharpen,
                        "UserSharpen",
                        "User-selected sharpen"
                    );

                    // Process image
                    var result = await imageProcessing.ProcessImageAsync(
                        image.Id,
                        new List<Guid> { blurFilter.Id, sharpenFilter.Id },
                        new List<Guid>(),
                        Guid.Empty
                    );

                    Console.WriteLine($"✓ Image processed successfully");
                    Console.WriteLine($"  Output: {result.OutputPath}");
                    Console.WriteLine($"  Status: {result.Status}");

                    // Return result to client (simulated)
                    var processingResult = new
                    {
                        Success = result.Status == ProcessingStatus.Success,
                        OutputPath = result.OutputPath,
                        ProcessingTimeMs = result.DurationMs,
                        FileSize = result.OutputSizeBytes,
                        Message = result.Status == ProcessingStatus.Success
                            ? "Image processed successfully"
                            : "Processing failed"
                    };

                    Console.WriteLine($"\n✓ Processing result prepared for API response:");
                    Console.WriteLine($"  Success: {processingResult.Success}");
                    Console.WriteLine($"  Message: {processingResult.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Processing error: {ex.Message}");
                    throw;
                }

                // ===================================================================
                // STEP 6: Batch Processing Example (simulated controller action)
                // ===================================================================
                Console.WriteLine("\nSimulating batch processing endpoint...\n");

                try
                {
                    // Simulate batch job creation
                    var batchImages = new List<Guid>();

                    // In real app, these would be uploaded images
                    Console.WriteLine("Creating batch job for multiple images...");

                    // Create a batch job
                    var job = await batchService.CreateJobAsync(
                        "UserBatchJob",
                        batchImages,
                        new List<Guid> { blurFilter.Id, sharpenFilter.Id },
                        new List<Guid>(),
                        Guid.Empty
                    );

                    Console.WriteLine($"✓ Batch job created: {job.Name} (ID: {job.Id})");
                    Console.WriteLine($"  Status: {job.Status}");
                    Console.WriteLine($"  Total images: {job.TotalCount}");

                    // Monitor job progress (in real app, this would be async)
                    var updatedJob = await batchService.GetJobAsync(job.Id);
                    Console.WriteLine($"✓ Job status updated: {updatedJob.Status}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Batch processing error: {ex.Message}");
                }

                // ===================================================================
                // STEP 7: Configuration Best Practices
                // ===================================================================
                Console.WriteLine("\nConfiguration best practices:\n");

                Console.WriteLine("1. Use appsettings.json for production:");
                Console.WriteLine("   ```json");
                Console.WriteLine("   {");
                Console.WriteLine("     \"Processing\": {");
                Console.WriteLine("       \"MaxParallelOperations\": 4,");
                Console.WriteLine("       \"UseGPUAcceleration\": true");
                Console.WriteLine("     }");
                Console.WriteLine("   }");
                Console.WriteLine("   ```");

                Console.WriteLine("\n2. Environment-specific configuration:");
                Console.WriteLine("   - Development: Use speed-optimized profile");
                Console.WriteLine("   - Production: Use quality-optimized profile");
                Console.WriteLine("   - Test: Use minimal configuration");

                Console.WriteLine("\n3. Required configuration:");
                Console.WriteLine("   - Storage paths must be writable");
                Console.WriteLine("   - Cache settings for production performance");
                Console.WriteLine("   - Device preferences for your hardware");

                // ===================================================================
                // STEP 8: Cleanup (simulated graceful shutdown)
                // ===================================================================
                Console.WriteLine("\nCleanup and disposal...\n");

                // In real app, dispose service provider on application shutdown
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                    Console.WriteLine("✓ Service provider disposed");
                }

                Console.WriteLine("\n✓ ASP.NET Core integration example completed successfully!");
                Console.WriteLine("\nNext steps for real application:");
                Console.WriteLine("1. Create actual ASP.NET Core project");
                Console.WriteLine("2. Add GPU Image Processing NuGet package");
                Console.WriteLine("3. Configure in Program.cs as shown above");
                Console.WriteLine("4. Create controllers with proper error handling");
                Console.WriteLine("5. Add authentication and authorization");
                Console.WriteLine("6. Implement proper logging and monitoring");
                Console.WriteLine("7. Add health checks and metrics");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Integration Error: {ex.Message}");
                Console.WriteLine("\nStack Trace:");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                Environment.Exit(1);
            }
        }
    }

    /// <summary>
    /// Mock implementation of IHostApplicationLifetime for demonstration
    /// In real application, this is provided by ASP.NET Core
    /// </summary>
    public class MockApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => throw new NotImplementedException();
        public CancellationToken ApplicationStopping => throw new NotImplementedException();
        public CancellationToken ApplicationStopped => throw new NotImplementedException();
        public void StopApplication() => throw new NotImplementedException();
    }
}