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
    /// Example 3: Image Transforms
    ///
    /// Apply geometric transformations to images including resizing, rotation,
    /// and color space conversion. Demonstrates combining filters with transforms.
    ///
    /// Prerequisites:
    /// - An image file "photo.jpg" in the current directory
    /// </summary>
    class TransformsExample
    {
        static async Task Main()
        {
            Console.WriteLine("=== GPU Image Processing: Transforms Example ===\n");

            try
            {
                var settings = ConfigurationValidator.CreateDefaultSettings();
                settings.ConfigureForDevelopment();

                Console.WriteLine("Initializing GPU service...");
                var serviceProvider = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                var imageProcessing = serviceProvider
                    .GetRequiredService<ImageProcessingService>();
                var transformService = serviceProvider
                    .GetRequiredService<TransformService>();
                var filterService = serviceProvider
                    .GetRequiredService<FilterService>();

                // Register image
                const string imageFile = "photo.jpg";
                var image = await imageProcessing.RegisterImageAsync(imageFile, "Original");
                Console.WriteLine($"✓ Image registered: {image.Width}x{image.Height}");

                // Create transforms
                Console.WriteLine("\nCreating transforms...");
                var transforms = new List<Guid>();

                // Transform 1: Resize
                Console.WriteLine("  1. Resize transform (2x scale)");
                var resizeTransform = await transformService.CreateTransformAsync(
                    TransformType.Resize,
                    "Scale 2x",
                    "Scale image to 2x size"
                );
                await transformService.UpdateTransformParametersAsync(
                    resizeTransform.Id,
                    new Dictionary<string, float>
                    {
                        { "ScaleX", 2.0f },
                        { "ScaleY", 2.0f }
                    }
                );
                transforms.Add(resizeTransform.Id);
                Console.WriteLine($"     ✓ Created: {resizeTransform.Id}");

                // Transform 2: Rotate
                Console.WriteLine("  2. Rotation transform (45 degrees)");
                var rotateTransform = await transformService.CreateTransformAsync(
                    TransformType.Rotate,
                    "Rotate 45°",
                    "Rotate image 45 degrees"
                );
                await transformService.UpdateTransformParametersAsync(
                    rotateTransform.Id,
                    new Dictionary<string, float>
                    {
                        { "Angle", 45.0f }
                    }
                );
                transforms.Add(rotateTransform.Id);
                Console.WriteLine($"     ✓ Created: {rotateTransform.Id}");

                // Transform 3: Color Space Conversion
                Console.WriteLine("  3. Color space conversion (RGB to HSV)");
                var colorTransform = await transformService.CreateTransformAsync(
                    TransformType.ColorSpaceConversion,
                    "RGB to HSV",
                    "Convert from RGB to HSV color space"
                );
                await transformService.UpdateTransformParametersAsync(
                    colorTransform.Id,
                    new Dictionary<string, float>
                    {
                        { "TargetSpace", 1.0f }  // 1 = HSV
                    }
                );
                transforms.Add(colorTransform.Id);
                Console.WriteLine($"     ✓ Created: {colorTransform.Id}");

                // Create filters to apply alongside transforms
                Console.WriteLine("\nCreating filters to apply with transforms...");
                var filters = new List<Guid>();

                var blurFilter = await filterService.CreateFilterAsync(
                    FilterType.Gaussian,
                    "Blur",
                    "Smooth the image"
                );
                await filterService.UpdateFilterParametersAsync(
                    blurFilter.Id,
                    new Dictionary<string, float> { { "Sigma", 1.0f } }
                );
                filters.Add(blurFilter.Id);
                Console.WriteLine("  ✓ Added Gaussian blur filter");

                // Apply filters and transforms together
                Console.WriteLine("\nApplying filters and transforms...");
                var startTime = DateTime.UtcNow;

                var result = await imageProcessing.ProcessImageAsync(
                    image.Id,
                    filters,
                    transforms,
                    Guid.Empty
                );

                var duration = DateTime.UtcNow - startTime;

                // Display results
                Console.WriteLine("✓ Processing complete!");
                Console.WriteLine($"\nResults:");
                Console.WriteLine($"  Status: {result.Status}");
                Console.WriteLine($"  Duration: {duration.TotalMilliseconds:F2}ms");
                Console.WriteLine($"  Output: {result.OutputPath}");
                Console.WriteLine($"  Output Size: {result.OutputSizeBytes / 1024.0:F2}KB");

                // Demonstrate transform combinations
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("Available Transform Types:");
                Console.WriteLine("  • Resize: Scale image dimensions");
                Console.WriteLine("  • Rotate: Rotate by angle");
                Console.WriteLine("  • ColorSpaceConversion: Convert between RGB/HSV/LAB");
                Console.WriteLine("  • Normalization: Normalize pixel values");
                Console.WriteLine("  • HistogramEqualization: Enhance contrast");
                Console.WriteLine("  • AffineTransform: Apply affine matrix");
                Console.WriteLine("  • WarpPerspective: Apply perspective transformation");
                Console.WriteLine("  • Crop: Crop to region");

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
