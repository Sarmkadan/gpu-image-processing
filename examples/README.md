# GPU Image Processing Examples

This directory contains practical examples demonstrating how to use the GPU Image Processing library.

## Quick Start

All examples are standalone C# files that can be compiled and run independently.

### Compile and Run an Example

```bash
# Navigate to examples directory
cd examples

# Compile a specific example
dotnet csc 01-basic-blur.cs -r:../bin/Release/net10.0/GpuImageProcessing.dll

# Run the compiled example
./01-basic-blur
```

Or build from the project root:

```bash
# Build entire project with examples
make examples

# Run specific example
./examples/01-basic-blur
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
- `photo.jpg` in the current directory (optional)

**Run**:
```bash
dotnet run --project examples -- 01-basic-blur
```

---

### 2. Batch Processing (`02-batch-processing.cs`)

**Difficulty**: Intermediate  
**Time**: 10 minutes

Process multiple images efficiently with:
- Batch job creation and execution
- Progress monitoring
- Multiple filter application
- Throughput measurement

**Key Concepts**:
- Batch processing service
- Job management
- Progress tracking
- Performance measurement

**Prerequisites**:
- Multiple image files in `./images/` directory (optional)

**Run**:
```bash
dotnet run --project examples -- 02-batch-processing
```

**Features**:
- Progress bar visualization
- Success/failure tracking
- Throughput calculation
- Job summary statistics

---

### 3. Image Transforms (`03-transforms.cs`)

**Difficulty**: Intermediate  
**Time**: 10 minutes

Apply geometric transformations:
- Resizing images
- Rotation
- Color space conversion
- Combining filters with transforms

**Key Concepts**:
- Transform creation
- Parameter configuration
- Combining filters and transforms
- Available transform types

**Prerequisites**:
- `photo.jpg` in the current directory (optional)

**Run**:
```bash
dotnet run --project examples -- 03-transforms
```

**Transform Types**:
- Resize: Scale dimensions
- Rotate: Rotate by angle
- ColorSpaceConversion: RGB/HSV/LAB conversion
- Normalization: Normalize pixel values
- HistogramEqualization: Enhance contrast
- AffineTransform: Matrix-based transformation
- WarpPerspective: Perspective correction
- Crop: Extract region of interest

---

### 4. Performance Monitoring (`04-performance-monitoring.cs`)

**Difficulty**: Intermediate  
**Time**: 15 minutes

Monitor and analyze performance:
- Device detection and selection
- Real-time metrics collection
- Device capabilities
- Performance optimization tips

**Key Concepts**:
- Device management
- Metrics collection
- Real-time monitoring
- Performance analysis

**Prerequisites**:
- GPU with OpenCL support (CPU mode works too)
- `photo.jpg` for processing simulation (optional)

**Run**:
```bash
dotnet run --project examples -- 04-performance-monitoring
```

**Metrics Tracked**:
- GPU utilization percentage
- Memory usage
- Throughput (images/second)
- Average processing time
- Active job count
- Total processed bytes

---

### 5. Advanced Filtering (`05-advanced-filtering.cs`)

**Difficulty**: Advanced  
**Time**: 15 minutes

Explore advanced filtering with:
- Multiple filter types (bilateral, median, Sobel, Canny, morphological)
- Custom processing profiles
- Speed vs. quality tradeoffs
- Filter comparison

**Key Concepts**:
- Advanced filter types
- Custom processing profiles
- Parameter tuning
- Performance profiling

**Prerequisites**:
- `photo.jpg` in the current directory (optional)

**Run**:
```bash
dotnet run --project examples -- 05-advanced-filtering
```

**Filters Demonstrated**:
- Bilateral: Edge-preserving blur
- Median: Salt-and-pepper noise removal
- Sobel: Fast edge detection
- Canny: High-quality edge detection
- Morphological: Binary image operations

---

## Building All Examples

```bash
# From project root
make examples

# Or manually
cd examples
foreach ($file in Get-ChildItem "*.cs") {
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

Example output from `01-basic-blur.cs`:

```
=== GPU Image Processing: Basic Blur Example ===

Found 1 compute devices:
  - NVIDIA RTX 3080 (GPU)

Registering image: photo.jpg
✓ Image registered with ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
  Name: MyPhoto
  Format: JPEG
  Size: 1920x1080

Creating Gaussian blur filter...
✓ Filter created with ID: b2c3d4e5-f6a7-8901-bcde-f12345678901

Configuring filter parameters...
✓ Parameters configured:
  - Sigma: 2
  - KernelSize: 5

Processing image...
✓ Processing complete!

Results:
  Status: Success
  Duration: 2.34ms
  Output Path: ./output/processed_image.jpg
  Output Size: 512.50KB

✓ Example completed successfully!
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
Note: photo.jpg not found. Using placeholder processing.
```

To use real images, place them in the expected location:
```bash
cp your-image.jpg photo.jpg
```

### Insufficient Memory

If you get memory errors:
1. Reduce batch size in batch processing example
2. Use speed-optimized profile
3. Close other GPU-using applications

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
   - Monitor with example 4
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
