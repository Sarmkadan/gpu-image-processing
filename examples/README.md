# GPU Image Processing Examples

This directory contains practical examples demonstrating how to use the GPU Image Processing library.

## Quick Start

All examples are standalone C# files that can be compiled and run independently.

### Compile and Run an Example

```bash
# Navigate to examples directory
cd examples

# Compile a specific example
dotnet csc BasicUsage.cs -r:../bin/Release/net10.0/GpuImageProcessing.dll

# Run the compiled example
./BasicUsage
```

Or build from the project root:

```bash
# Build entire project with examples
make examples

# Run specific example
./examples/BasicUsage
```

## Examples Overview

### 1. Basic Usage (`BasicUsage.cs`)

**Difficulty**: Beginner
**Time**: 5 minutes

The absolute minimum code required to use GPU Image Processing:
- Initialize the system with default settings
- Register an image
- Create and apply a simple filter
- Get the processed result

**Key Concepts**:
- Service initialization
- Image registration
- Filter creation and configuration
- Basic processing

**Prerequisites**:
- `input.jpg` in the current directory (optional)

**Run**:
```bash
dotnet run --project examples -- BasicUsage
```

---

### 2. Advanced Usage (`AdvancedUsage.cs`)

**Difficulty**: Intermediate
**Time**: 15 minutes

Advanced usage patterns including:
- Custom configuration with performance tuning
- Multiple filter types and parameter customization
- Error handling and validation
- Performance monitoring
- Custom processing profiles

**Key Concepts**:
- Advanced configuration
- Multiple filters and transforms
- Error handling
- Performance monitoring
- Processing profiles

**Prerequisites**:
- `input.jpg` in the current directory (optional)


**Run**:
```bash
dotnet run --project examples -- AdvancedUsage
```

**Features**:
- Custom settings configuration
- Multiple filter creation
- Transform application
- Performance metrics collection
- Comprehensive error handling

---

### 3. ASP.NET Core Integration (`IntegrationExample.cs`)

**Difficulty**: Intermediate
**Time**: 15 minutes

Demonstrates how to integrate GPU Image Processing into an ASP.NET Core application:
- Configuring services in Program.cs
- Using the library in controllers
- Best practices for production deployment
- Integration with ASP.NET Core's built-in DI container


**Key Concepts**:
- ASP.NET Core dependency injection
- Service configuration
- Controller pattern usage
- Production deployment considerations

**Prerequisites**:
- Basic ASP.NET Core knowledge


**Run**:
```bash
dotnet run --project examples -- IntegrationExample
```

**Features**:
- Service registration simulation
- Controller pattern demonstration
- Batch processing integration
- Configuration best practices
- Graceful shutdown handling

---

## Building All Examples

```bash
# From project root
make examples

# Or manually
cd examples
foreach ($file in Get-ChildItem "BasicUsage.cs", "AdvancedUsage.cs", "IntegrationExample.cs") {
    dotnet csc $file.Name -r:../bin/Release/net10.0/GpuImageProcessing.dll
}
```

## Running Examples in Docker

```bash
# Build Docker image
docker build -t gpu-image-processing:latest .

# Run with example
docker run -it \
    -v $(pwd)/images:/app/images \
    -v $(pwd)/output:/app/output \
    gpu-image-processing:latest

# Run with GPU support
docker run -it --gpus all \
    -v $(pwd)/images:/app/images \
    -v $(pwd)/output:/app/output \
    gpu-image-processing:latest
```

## Example Output

Each example produces console output showing:
- Configuration information
- Processing progress
- Performance metrics
- Results summary

Example output from `BasicUsage.cs`:

```
=== GPU Image Processing: Basic Usage ===

✓ Image registered: MyImage (ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890)

✓ Processing complete!
 Status: Success
 Output: ./output/processed_image.jpg

✓ Basic usage example completed successfully!
```

## Common Issues and Solutions

### GPU Not Detected

If GPU is not found:
```bash
# Check NVIDIA drivers
nvidia-smi

# Or check OpenCL availability
clinfo
```

Enable CPU fallback by modifying the example:
```csharp
settings.Device.AllowFallbackToCPU = true;
```

### No Input Image Error

Examples work without input images but show a note:
```
Note: input.jpg not found. Using placeholder processing.
```

To use real images, place them in the expected location:
```bash
cp your-image.jpg input.jpg
```

### Insufficient Memory

If you get memory errors:
1. Reduce parallel operations in settings
2. Use speed-optimized profile
3. Close other GPU-using applications
4. Reduce batch size

### Build Errors

Ensure you're using .NET 10.0+:
```bash
dotnet --version
```

## Creating Your Own Example

Here's a template for creating a new example:

```csharp
// =============================================================================
// Author: Your Name
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Configuration;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Examples
{
    class MyExample
    {
        static async Task Main()
        {
            try
            {
                // Initialize
                var settings = ConfigurationValidator.CreateDefaultSettings();
                var sp = await DependencyInjectionSetup
                    .CreateAndInitializeServiceProviderAsync(settings);

                // Get services
                var imageProcessing = sp.GetRequiredService<ImageProcessingService>();

                // Your processing logic here
                Console.WriteLine("✓ Example completed!");
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
```

## Performance Tips

1. **Use appropriate profiles**:
   - Speed-optimized for real-time processing
   - Quality-optimized for offline batch processing  
   - Balanced for general use

2. **Batch processing**:
   - Process multiple images in one job
   - Increases GPU utilization
   - Better throughput

3. **GPU memory**:
   - Monitor with AdvancedUsage example
   - Adjust batch size accordingly
   - Enable caching for repeated operations

4. **Filter optimization**:
   - Simple filters (Gaussian) are faster
   - Edge detection (Sobel) is moderate
   - Complex filters (Canny) are slower

## Troubleshooting Guide

See [FAQ](../docs/faq.md) for comprehensive troubleshooting.

Common questions:
- How do I use custom images?
- How do I optimize for my GPU?
- Why is processing slow?
- How do I debug errors?

## Next Steps

After running examples:

1. **Read** the [Getting Started Guide](../docs/getting-started.md)
2. **Review** the [API Reference](../docs/api-reference.md)
3. **Explore** the [Deployment Guide](../docs/deployment.md)
4. **Check** the [FAQ](../docs/faq.md)

## Support

- 📖 **Documentation**: See [docs/](../docs/) directory
- 🐛 **Issues**: Report on [GitHub Issues](https://github.com/Sarmkadan/gpu-image-processing/issues)
- 💬 **Discussions**: Ask on [GitHub Discussions](https://github.com/Sarmkadan/gpu-image-processing/discussions)
- 📧 **Email**: Contact via GitHub profile

---

**Happy Processing!** 🚀

---

Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect