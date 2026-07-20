# GPU Image Processing

A high-performance GPU-accelerated image processing library using C# and .NET with DirectML and Vulkan compute shaders.

## Table of Contents

- [Architecture](#architecture)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Utilities](#utilities)
- [Examples](#examples)
- [Performance](#performance)
- [Contributing](#contributing)
- [License](#license)

## PortablePixmap

The `PortablePixmap` class provides a minimal reader/writer for the binary Netpbm formats: P6 (24-bit RGB PPM) and P5 (8-bit grayscale PGM). These formats are dependency-free, byte-exact, and trivial to round-trip, making them ideal as a portable on-disk representation for the batch CLI and for golden-image regression fixtures. The pixel buffers produced here are laid out row-major with `bpp` bytes per pixel, exactly what the CPU image processor expects.

### Key Features

- Supports both P6 (RGB) and P5 (grayscale) Netpbm formats
- Dependency-free implementation with minimal dependencies
- Byte-exact round-tripping for regression testing
- Row-major pixel layout compatible with GPU processing pipelines
- File extension detection (.ppm, .pgm)
- Pixel hash computation for change detection

### Usage Examples

```csharp

using GpuImageProcessing.Imaging;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Check if a file is a supported portable pixmap
        bool isSupported = PortablePixmap.IsSupported("test.ppm");
        Console.WriteLine($"Is supported: {isSupported}"); // Output: Is supported: True

        // Load a PPM/PGM file into an Image object
        var image = PortablePixmap.Load("input.ppm");
        Console.WriteLine($"Loaded image: {image.Width}x{image.Height}, {image.Channels} channels, {image.BitsPerPixel} bpp");

        // Compute a stable hash of the pixel data for regression testing
        string pixelHash = PortablePixmap.PixelHash(image);
        Console.WriteLine($"Pixel hash: {pixelHash}");

        // Save the image back to disk (automatically chooses P6 or P5 based on channels)
        PortablePixmap.Save(image, "output.ppm");

        // Decode from a stream (useful for testing or memory-based operations)
        using var memoryStream = new MemoryStream(File.ReadAllBytes("input.pgm"));
        var decodedImage = PortablePixmap.Decode(memoryStream);
        Console.WriteLine($"Decoded: {decodedImage.Width}x{decodedImage.Height} grayscale image");
    }
}

```

## PerformanceUtilities

The `PerformanceUtilities` class provides high-resolution timing and performance measurement utilities for GPU-accelerated image processing operations. It includes stopwatch-based timing, frame rate calculation, and performance statistics collection.

### Key Features


- High-resolution timing using `Stopwatch`
- Frame rate calculation and tracking
- Performance statistics aggregation
- Memory allocation tracking
- Batch processing performance monitoring

### Usage Examples


```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Diagnostics;

class Program

{
    static void Main()

    {

        // Basic timing example
        var timer = PerformanceUtilities.StartTimer("Image Processing");
        
        // Simulate image processing
        System.Threading.Thread.Sleep(100);
        
        timer.Stop();
        Console.WriteLine($"Processing time: {timer.ElapsedMilliseconds}ms");
        Console.WriteLine($"Processing rate: {PerformanceUtilities.CalculateFramesPerSecond(timer.ElapsedMilliseconds, 1):F2} FPS");
        
        // Frame rate tracking
        var fpsTimer = PerformanceUtilities.StartTimer("FPS Measurement");
        for (int i = 0; i < 100; i++)
        {
            System.Threading.Thread.Sleep(16); // ~60 FPS
            PerformanceUtilities.RecordFrame();
        }
        fpsTimer.Stop();
        
        var stats = PerformanceUtilities.GetPerformanceStatistics();
        Console.WriteLine($"Average FPS: {stats.AverageFPS:F2}");
        Console.WriteLine($"Min FPS: {stats.MinFPS:F2}");
        Console.WriteLine($"Max FPS: {stats.MaxFPS:F2}");
        
        // Memory tracking
        var memoryBefore = GC.GetTotalMemory(true);
        var largeArray = new byte[1024 * 1024 * 100]; // 100MB
        var memoryAfter = GC.GetTotalMemory(true);
        var memoryUsed = memoryAfter - memoryBefore;
        
        Console.WriteLine($"Memory allocated: {PerformanceUtilities.FormatBytes(memoryUsed)}");
    }
}
```

## PathUtilitiesJsonExtensions

The `PathUtilitiesJsonExtensions` class provides System.Text.Json serialization utilities for `PathUtilitiesConfiguration` instances. It enables serialization and deserialization of path utilities configurations with support for both compact and indented JSON output formats, safe error handling, and thread-safe serialization with optimized JsonSerializerOptions.

### Key Features

- JSON serialization of `PathUtilitiesConfiguration` instances using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration state management for path utilities
- Thread-safe serialization with optimized JsonSerializerOptions
- Access to `SupportedExtensions`, `DefaultPathNormalization`, and `CrossPlatformSupport` properties

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a PathUtilities configuration with supported extensions and platform settings
        var pathConfig = new PathUtilitiesJsonExtensions.PathUtilitiesConfiguration
        {
            SupportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".ppm", ".pgm" },
            DefaultPathNormalization = true,
            CrossPlatformSupport = true
        };

        // Serialize to compact JSON
        string compactJson = pathConfig.ToJson();
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = pathConfig.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{
    "supportedExtensions": [".jpg", ".jpeg", ".png"],
    "defaultPathNormalization": true,
    "crossPlatformSupport": false
}
";
        var deserialized = PathUtilitiesJsonExtensions.FromJson(json);

        if (deserialized != null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"Supported extensions: {string.Join(", ", deserialized.SupportedExtensions)}");
            Console.WriteLine($"Default path normalization: {deserialized.DefaultPathNormalization}");
            Console.WriteLine($"Cross-platform support: {deserialized.CrossPlatformSupport}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = PathUtilitiesJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = pathConfig.ToJson();
        var roundTripResult = PathUtilitiesJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
    }
}

```


## MetricsUtilities

The `MetricsUtilities` class provides comprehensive image quality metrics and analysis tools for comparing processed images against reference images. It includes PSNR, SSIM, MSE, and other quality assessment algorithms.

### Key Features


- Peak Signal-to-Noise Ratio (PSNR) calculation
- Structural Similarity Index (SSIM) calculation
- Mean Squared Error (MSE) calculation
- Image quality assessment utilities
- Batch metric calculation

### Usage Examples


```csharp

using GpuImageProcessing.Utilities;
using System;
using System.IO;

class Program

{
    static void Main()

    {

        // Example: Load two images and compare them
        var originalImage = File.ReadAllBytes("original.jpg");
        var processedImage = File.ReadAllBytes("processed.jpg");
        
        // Calculate quality metrics
        var psnr = MetricsUtilities.CalculatePSNR(originalImage, processedImage);
        var mse = MetricsUtilities.CalculateMSE(originalImage, processedImage);
        var ssim = MetricsUtilities.CalculateSSIM(originalImage, processedImage);
        
        Console.WriteLine($"PSNR: {psnr:F2} dB");
        Console.WriteLine($"MSE: {mse:F6}");
        Console.WriteLine($"SSIM: {ssim:F4}");
        
        // Batch processing example
        var directory = new DirectoryInfo("test-images");
        var results = new List<ImageQualityResult>();
        
        foreach (var file in directory.GetFiles("*.jpg"))
        {
            var reference = File.ReadAllBytes($"reference/{file.Name}");
            var processed = File.ReadAllBytes($"processed/{file.Name}");
            
            results.Add(new ImageQualityResult
            {
                FileName = file.Name,
                PSNR = MetricsUtilities.CalculatePSNR(reference, processed),
                SSIM = MetricsUtilities.CalculateSSIM(reference, processed)
            });
        }
        
        // Calculate average quality
        var avgPsnr = results.Average(r => r.PSNR);
        var avgSsim = results.Average(r => r.SSIM);
        Console.WriteLine($"Average PSNR: {avgPsnr:F2} dB");
        Console.WriteLine($"Average SSIM: {avgSsim:F4}");
    }
}

public class ImageQualityResult
{
    public string FileName { get; set; }
    public double PSNR { get; set; }
    public double SSIM { get; set; }
}
```

## MetricsUtilitiesJsonExtensions

The `MetricsUtilitiesJsonExtensions` class provides System.Text.Json serialization utilities for `MetricsConfiguration` instances. It enables serialization and deserialization of metrics configurations with support for both compact and indented JSON output formats, safe error handling, and thread-safe serialization with optimized JsonSerializerOptions.

### Key Features

- JSON serialization of `MetricsConfiguration` instances using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration state management for metrics configurations
- Thread-safe serialization with optimized JsonSerializerOptions
- Access to `StatisticalMetrics` and `Histogram` configuration properties

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a metrics configuration with statistical metrics and histogram
        var metricsConfig = new MetricsUtilitiesJsonExtensions.MetricsConfiguration
        {
            StatisticalMetrics = new StatisticalMetrics
            {
                Count = 1000,
                Min = 25.5,
                Max = 98.7,
                Mean = 67.3,
                Median = 68.1,
                P95 = 95.2,
                P99 = 97.8,
                StdDev = 12.4,
                Sum = 67300.0
            },
            Histogram = new Histogram
            {
                TotalCount = 1000,
                Buckets = new System.Collections.Generic.List<HistogramBucket>
                {
                    new HistogramBucket { BucketNumber = 1, Min = 0, Max = 25, Count = 250, Percentage = 25.0 },
                    new HistogramBucket { BucketNumber = 2, Min = 25, Max = 50, Count = 500, Percentage = 50.0 },
                    new HistogramBucket { BucketNumber = 3, Min = 50, Max = 75, Count = 200, Percentage = 20.0 },
                    new HistogramBucket { BucketNumber = 4, Min = 75, Max = 100, Count = 50, Percentage = 5.0 }
                }
            }
        };

        // Serialize to compact JSON
        string compactJson = metricsConfig.ToJson();
        Console.WriteLine("Compact JSON metrics configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = metricsConfig.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON metrics configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{
    "statisticalMetrics": {
        "count": 1000,
        "min": 25.5,
        "max": 98.7,
        "mean": 67.3,
        "median": 68.1,
        "p95": 95.2,
        "p99": 97.8,
        "stdDev": 12.4,
        "sum": 67300.0
    },
    "histogram": {
        "totalCount": 1000,
        "buckets": [
            {"bucketNumber": 1, "min": 0, "max": 25, "count": 250, "percentage": 25.0},
            {"bucketNumber": 2, "min": 25, "max": 50, "count": 500, "percentage": 50.0},
            {"bucketNumber": 3, "min": 50, "max": 75, "count": 200, "percentage": 20.0},
            {"bucketNumber": 4, "min": 75, "max": 100, "count": 50, "percentage": 5.0}
        ]
    }
}";
        var deserialized = MetricsUtilitiesJsonExtensions.FromJson(json);

        if (deserialized != null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"StatisticalMetrics present: {deserialized.StatisticalMetrics != null}");
            Console.WriteLine($"Histogram present: {deserialized.Histogram != null}");
            if (deserialized.StatisticalMetrics != null)
            {
                Console.WriteLine($"Mean: {deserialized.StatisticalMetrics.Mean:F1}");
                Console.WriteLine($"StdDev: {deserialized.StatisticalMetrics.StdDev:F1}");
            }
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = MetricsUtilitiesJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = metricsConfig.ToJson();
        var roundTripResult = MetricsUtilitiesJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
    }
}

```

## DeviceUtilitiesJsonExtensions

The `DeviceUtilitiesJsonExtensions` class provides System.Text.Json serialization utilities for `DeviceConfiguration` instances. It enables serialization and deserialization of GPU device configurations with support for both compact and indented JSON output formats, safe error handling, and thread-safe serialization with optimized JsonSerializerOptions.

### Key Features

- JSON serialization of `DeviceConfiguration` instances using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration state management for device configurations
- Thread-safe serialization with optimized JsonSerializerOptions

### Usage Examples

```csharp
using GpuImageProcessing.Utilities;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a device configuration
        var deviceConfig = new DeviceConfiguration
        {
            DeviceName = "NVIDIA RTX 3090",
            GlobalMemoryBytes = 24 * 1024 * 1024 * 1024L, // 24GB
            ComputeUnits = 82,
            MaxClockFrequency = 1695,
            ComputeCapability = "8.6",
            MemoryPressureLevel = MemoryPressureLevel.Normal,
            RecommendedBatchSize = 32
        };

        // Serialize to compact JSON
        string compactJson = deviceConfig.ToJson();
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = deviceConfig.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{
            "deviceName": "AMD Radeon RX 6800",
            "globalMemoryBytes": 16133422080,
            "computeUnits": 60,
            "maxClockFrequency": 2250,
            "computeCapability": "8.6",
            "memoryPressureLevel": "High",
            "recommendedBatchSize": 16
        }";
        var deserialized = DeviceUtilitiesJsonExtensions.FromJson(json);

        if (deserialized != null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"Device: {deserialized.DeviceName}");
            Console.WriteLine($"Memory: {deserialized.GlobalMemoryBytes / (1024.0 * 1024 * 1024):F2} GB");
            Console.WriteLine($"Compute Units: {deserialized.ComputeUnits}");
            Console.WriteLine($"Clock: {deserialized.MaxClockFrequency} MHz");
            Console.WriteLine($"Compute Capability: {deserialized.ComputeCapability}");
            Console.WriteLine($"Memory Pressure: {deserialized.MemoryPressureLevel}");
            Console.WriteLine($"Recommended Batch Size: {deserialized.RecommendedBatchSize}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = DeviceUtilitiesJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = deviceConfig.ToJson();
        var roundTripResult = DeviceUtilitiesJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
    }
}

```


## CpuFallbackPathTests

The `CpuFallbackPathTests` class exercises the explicit no-OpenCL CPU fallback path end-to-end. These tests verify that the library remains functional on machines without OpenCL by ensuring that image processing operations can run entirely on the CPU without GPU acceleration. The test suite validates supported filter types, ensures unsupported filters are properly identified, and confirms that CPU processing produces correct results with proper metadata.

### Key Features

- Tests CPU-based image processing without requiring GPU devices
- Validates supported filter types that can run on CPU
- Verifies unsupported filters are correctly identified
- Tests cancellation handling during CPU processing
- Validates image resizing produces correct dimensions
- Confirms grayscale conversion produces equal luma values across channels
- Verifies metadata includes processor identification

### Usage Examples

```csharp

using GpuImageProcessing.Fallback;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create a CPU fallback image processor
        var processor = new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance);

        // Check if a filter can be processed on CPU
        bool canProcessGrayscale = processor.CanProcess(FilterType.Grayscale);
        Console.WriteLine($"Can process grayscale on CPU: {canProcessGrayscale}");

        bool canProcessRotation = processor.CanProcess(FilterType.Rotation);
        Console.WriteLine($"Can process rotation on CPU: {canProcessRotation}");

        // Apply a filter to an image on CPU
        var image = new Image
        {
            Width = 100,
            Height = 100,
            Channels = 3,
            BitsPerPixel = 24,
            PixelData = new byte[100 * 100 * 3]
        };
        Array.Fill(image.PixelData, (byte)128);

        var config = new FilterConfiguration
        {
            Name = "grayscale",
            FilterType = FilterType.Grayscale
        };

        var result = await processor.ApplyFilterAsync(image, config);

        Console.WriteLine("Processing completed on CPU");
        Console.WriteLine($"Result metadata contains processor: {result.Metadata.ContainsKey("processor")}");
        Console.WriteLine($"Processor type: {result.Metadata.GetValueOrDefault("processor")}");

        // Resize an image on CPU
        var resized = processor.Resize(image, 50, 50);
        Console.WriteLine($"Resized image dimensions: {resized.Width}x{resized.Height}");

        // Convert to grayscale on CPU
        var grayscale = processor.ToGrayscale(image);
        Console.WriteLine($"Grayscale image channels: {grayscale.Channels}");
        Console.WriteLine($"Grayscale pixel data length: {grayscale.PixelData?.Length}");
    }
}

```

## FilterChainBuilderTestsExtensions

The `FilterChainBuilderTestsExtensions` class provides extension methods for the `FilterChainBuilderTests` class that simplify running groups of test methods programmatically. It includes utilities for executing successful build tests, validating parameter validation tests, and retrieving all test method names, making it easier to automate test execution and validation.

### Key Features

- Execute all successful build tests that should not throw exceptions
- Run invalid parameter tests that are expected to throw specific exception types
- Retrieve all test method names from the test class
- Custom assertion exception for test failures with detailed error messages
- Null safety checks for all public methods

### Usage Examples

```csharp

using GpuImageProcessing.Tests.Domain;
using System;
using System.Linq;

class Program
{
static void Main()
{
// Create an instance of the test class
var testInstance = new FilterChainBuilderTests();

// Get all test method names
var allMethodNames = testInstance.GetAllTestMethodNames();
Console.WriteLine($"Total test methods: {allMethodNames.Count}");
Console.WriteLine("Method names:");
foreach (var methodName in allMethodNames)
{
Console.WriteLine($" - {methodName}");
}

// Run all successful build tests (methods that should not throw)
try
{
testInstance.RunAllSuccessfulBuildTests();
Console.WriteLine("All successful build tests passed!");
}
catch (FilterChainBuilderTestsExtensions.AssertionFailedException ex)
{
Console.WriteLine($"Build test failed: {ex.Message}");
}

// Run all invalid parameter tests (methods that should throw specific exceptions)
try
{
testInstance.RunAllInvalidParameterTests();
Console.WriteLine("All invalid parameter tests completed successfully!");
}
catch (FilterChainBuilderTestsExtensions.AssertionFailedException ex)
{
Console.WriteLine($"Parameter validation test failed: {ex.Message}");
}

// Example: Run specific tests individually
try
{
testInstance.Build_SingleStep_ProducesValidChain();
Console.WriteLine("Build_SingleStep_ProducesValidChain passed");
}
catch (Exception ex)
{
Console.WriteLine($"Test failed: {ex.Message}");
}
}
}

```

## ProcessImageCommandExtensions

The `ProcessImageCommandExtensions` class provides extension methods for the `ProcessImageCommand` class that simplify common image processing operations in the command-line interface. It includes utilities for validating input files and output directories, parsing image processing parameters, generating output filenames, and logging processing statistics, making it easier to build robust image processing pipelines.

### Key Features

- Validate input files exist and are supported image formats
- Validate output directories exist and are writable
- Parse common image processing parameters (width, height, quality) from command line arguments
- Generate unique output filenames based on input filename and processing parameters
- Log detailed processing statistics including file sizes and processing duration

### Usage Examples

```csharp

using GpuImageProcessing.Cli;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Create a ProcessImageCommand instance
        var command = new ProcessImageCommand();

        // Validate input file
        string inputPath = "input.jpg";
        bool isInputValid = command.ValidateInputFile(inputPath);
        Console.WriteLine($"Input file valid: {isInputValid}");

        // Validate output directory
        string outputDir = "/output/images";
        bool isOutputValid = command.ValidateOutputDirectory(outputDir);
        Console.WriteLine($"Output directory valid: {isOutputValid}");

        // Parse image parameters from command line arguments
        var args = new[] { "--width=800", "--height=600", "--quality=90" };
        bool parsed = command.TryParseImageParameters(
            args,
            out int width,
            out int height,
            out int quality
        );
        Console.WriteLine($"Parameters parsed: {parsed}");
        Console.WriteLine($"Width: {width}, Height: {height}, Quality: {quality}");

        // Generate output filename
        string outputPath = command.GenerateOutputFilename(
            inputPath,
            outputDir,
            width,
            height,
            quality
        );
        Console.WriteLine($"Output path: {outputPath}");

        // Simulate processing and log statistics
        var startTime = DateTime.UtcNow;
        // ... perform image processing ...
        var endTime = DateTime.UtcNow;
        long originalSize = new FileInfo(inputPath).Length;
        long processedSize = new FileInfo(outputPath).Length;

        command.LogProcessingStats(
            inputPath,
            outputPath,
            startTime,
            endTime,
            originalSize,
            processedSize
        );
    }
}

```

## TransformServiceExtensions

The `TransformServiceExtensions` class provides extension methods for the `TransformService` that simplify common transform management operations. It includes utilities for bulk operations, parameter management, filtering, and execution order control, making it easier to build flexible image processing pipelines with transforms.



### Key Features

- Bulk create multiple transforms of the same type
- Bulk activate/deactivate transforms by their IDs
- Parameter copying between transforms
- Get default parameter values for transform types
- Filter transforms by active status, type, or name pattern
- Manage execution order with sequential reordering
- Retrieve transforms sorted by execution order

### Usage Examples

```csharp

using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
static async Task Main()
{
    // Initialize the transform service (typically via dependency injection)
    var transformService = new TransformService(...);

    // Bulk create transforms of the same type
    var resizeTransforms = await transformService.CreateTransformsAsync(
        TransformType.Resize, 
        count: 5, 
        namePrefix: "Resize",
        description: "Batch resize transforms"
    );
    Console.WriteLine($"Created {resizeTransforms.Count} resize transforms");

    // Get default parameter value for a transform type
    var defaultScale = await transformService.GetDefaultParameterValueAsync(
        TransformType.Resize, 
        "Scale"
    );
    Console.WriteLine($"Default scale parameter: {defaultScale}");

    // Bulk activate transforms by their IDs
    var activeCount = await transformService.BulkActivateTransformsAsync(
        resizeTransforms.Select(t => t.Id)
    );
    Console.WriteLine($"Activated {activeCount} transforms");

    // Bulk deactivate transforms by their IDs
    var deactivateCount = await transformService.BulkDeactivateTransformsAsync(
        resizeTransforms.Select(t => t.Id)
    );
    Console.WriteLine($"Deactivated {deactivateCount} transforms");

    // Get parameter names for a transform type
    var parameterNames = await transformService.GetParameterNamesAsync(TransformType.Rotate);
    Console.WriteLine($"Rotate transform has {parameterNames.Count} parameters:");
    foreach (var param in parameterNames) Console.WriteLine($" - {param}");

    // Copy parameters from one transform to another
    var rotateTransform = await transformService.CreateTransformAsync(
        TransformType.Rotate, 
        "Rotate 90",
        "Rotate image by 90 degrees"
    );
    
    bool copySuccess = await transformService.CopyParametersAsync(
        sourceTransformId: resizeTransforms[0].Id,
        targetTransformId: rotateTransform.Id,
        parameterNames: new[] { "Scale", "ExecutionOrder" }
    );
    Console.WriteLine($"Parameter copy successful: {copySuccess}");

    // Get transforms sorted by execution order
    var orderedTransforms = await transformService.GetTransformsByExecutionOrderAsync();
    Console.WriteLine($"Transforms in execution order:");
    foreach (var transform in orderedTransforms) 
    {
        Console.WriteLine($" - {transform.Name} (Order: {transform.ExecutionOrder})");
    }

    // Find transforms by name pattern
    var namedTransforms = await transformService.FindTransformsByNameAsync("Resize");
    Console.WriteLine($"Found {namedTransforms.Count} transforms with 'Resize' in name");

    // Get transforms filtered by active status and type
    var activeResizes = await transformService.GetTransformsFilteredAsync(
        isActive: true,
        type: TransformType.Resize
    );
    Console.WriteLine($"Active resize transforms: {activeResizes.Count}");

    // Get the next available execution order number
    var nextOrder = await transformService.GetNextExecutionOrderAsync();
    Console.WriteLine($"Next execution order: {nextOrder}");

    // Reorder transforms to maintain sequential execution order without gaps
    bool reorderSuccess = await transformService.ReorderTransformsSequentiallyAsync();
    Console.WriteLine($"Sequential reordering successful: {reorderSuccess}");
}
}

```

## JsonResultFormatterExtensions

The `JsonResultFormatterExtensions` class provides extension methods for the `JsonResultFormatter` class that extend the JSON formatting capabilities with additional context and statistics. It includes methods for formatting processing results with statistics, batch results with summaries, job progress details, device information with hardware specs, error context, and file output operations, making it easier to generate comprehensive JSON reports for image processing operations.

### Key Features

- Format single processing results with statistics including file sizes, processing speeds, and metadata
- Format batch processing results with summary statistics (success rate, average duration, totals)
- Format processing jobs with progress details including completion percentage and estimated time remaining
- Format device information with hardware details like memory capacity, compute units, and extensions
- Format error information with context including error codes, timestamps, and exception details
- Output formatted results directly to files with optional metadata inclusion
- Helper methods for file size formatting, processing speed calculation, and time estimation

### Usage Examples

```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var formatter = new JsonResultFormatter();

        // Format a single processing result with statistics
        var result = new ProcessingResult
        {
            Id = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            ImageId = Guid.NewGuid(),
            Status = ProcessingStatus.Completed,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            CompletionTime = DateTime.UtcNow,
            ProcessingTimeMs = 1250,
            OutputImagePath = "/output/processed-image.jpg",
            ProcessedSize = 2560000, // 2.5MB
            Metadata = new Dictionary<string, object> { { "filter", "gaussian-blur" }, { "iterations", 3 } }
        };

        string formattedResult = formatter.FormatResultWithStatistics(result);
        Console.WriteLine(formattedResult);

        // Format batch processing results with summary statistics
        var results = new List<ProcessingResult>
        {
            new ProcessingResult { Id = Guid.NewGuid(), JobId = Guid.NewGuid(), ImageId = Guid.NewGuid(), Status = ProcessingStatus.Completed, ProcessingTimeMs = 1000, ProcessedSize = 1024000 },
            new ProcessingResult { Id = Guid.NewGuid(), JobId = Guid.NewGuid(), ImageId = Guid.NewGuid(), Status = ProcessingStatus.Completed, ProcessingTimeMs = 1200, ProcessedSize = 1536000 },
            new ProcessingResult { Id = Guid.NewGuid(), JobId = Guid.NewGuid(), ImageId = Guid.NewGuid(), Status = ProcessingStatus.Failed, ProcessingTimeMs = 500, ProcessedSize = 512000 }
        };

        string formattedBatch = formatter.FormatResultsWithSummary(results);
        Console.WriteLine(formattedBatch);

        // Format a processing job with progress details
        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            Name = "Batch Image Processing Job #12345",
            Status = ProcessingStatus.Running,
            TotalImages = 100,
            ProcessedImages = 45,
            FailedImages = 2,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            StartedAt = DateTime.UtcNow.AddHours(-1),
            Filters = new List<FilterConfiguration> { new FilterConfiguration { Name = "resize", FilterType = FilterType.Resize, Parameters = new Dictionary<string, object> { { "width", 1920 }, { "height", 1080 } } } }
        };

        string formattedJob = formatter.FormatJobWithProgress(job);
        Console.WriteLine(formattedJob);

        // Format device information with hardware details
        var device = new DeviceInfo
        {
            Id = 0,
            Name = "NVIDIA RTX 3090",
            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024, // 24GB
            ComputeUnits = 82,
            IsAvailable = true,
            DriverVersion = "510.47.03",
            Extensions = new List<string> { "cl_khr_fp64", "cl_khr_global_int32_base_atomics" }
        };

        string formattedDevice = formatter.FormatDeviceWithDetails(device);
        Console.WriteLine(formattedDevice);

        // Format error information with context
        try
        {
            // Simulate an error
            throw new InvalidOperationException("Device initialization failed");
        }
        catch (Exception ex)
        {
            string formattedError = formatter.FormatErrorWithContext(
                "Failed to initialize GPU device",
                "DEVICE_INIT_ERROR",
                ex,
                new { DeviceName = device.Name, Attempt = 3 }
            );
            Console.WriteLine(formattedError);
        }

        // Format result to file with metadata
        string outputPath = formatter.FormatResultToFile(result, "/output/result-statistics.json");
        Console.WriteLine($"Result saved to: {outputPath}");

        // Format batch results to file with summary
        string batchOutputPath = formatter.FormatResultsToFile(results, "/output/batch-summary.json");
        Console.WriteLine($"Batch summary saved to: {batchOutputPath}");
    }
}

```

## XmlResultFormatterExtensions

The `XmlResultFormatterExtensions` class provides extension methods for the `XmlResultFormatter` class that extend the XML formatting capabilities with additional context and statistics. It includes methods for formatting processing results with statistics, job progress details, device information with extensions, and standardized XML envelopes, making it easier to generate comprehensive XML reports for image processing operations.

### Key Features

- Format collections of processing results with summary statistics (success rate, average duration, totals)
- Format processing jobs with detailed breakdown by status and completion rate
- Format device information with extended capabilities and OpenCL extensions
- Create standardized XML envelopes around formatted content with metadata
- Helper methods for XML formatting and statistics calculation

### Usage Examples

```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var formatter = new XmlResultFormatter();

        // Format processing results with statistics
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = Guid.NewGuid(),
                JobId = Guid.NewGuid(),
                ImageId = Guid.NewGuid(),
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                CompletionTime = DateTime.UtcNow,
                OutputImagePath = "/output/processed-image-001.jpg"
            },
            new ProcessingResult
            {
                Id = Guid.NewGuid(),
                JobId = Guid.NewGuid(),
                ImageId = Guid.NewGuid(),
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-4),
                CompletionTime = DateTime.UtcNow,
                OutputImagePath = "/output/processed-image-002.jpg"
            },
            new ProcessingResult
            {
                Id = Guid.NewGuid(),
                JobId = Guid.NewGuid(),
                ImageId = Guid.NewGuid(),
                Status = ProcessingStatus.Failed,
                StartTime = DateTime.UtcNow.AddMinutes(-3),
                CompletionTime = DateTime.UtcNow,
                OutputImagePath = "/output/failed-image-003.jpg"
            }
        };

        string formattedResults = formatter.FormatResultsWithStatistics(results);
        Console.WriteLine(formattedResults);

        // Format a processing job with detailed breakdown
        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            Name = "Batch Image Processing Job #12345",
            Status = ProcessingStatus.Completed,
            TotalImages = 100,
            ProcessedImages = 95,
            FailedImages = 5,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            StartedAt = DateTime.UtcNow.AddHours(-1)
        };

        string formattedJob = formatter.FormatJobWithDetails(job);
        Console.WriteLine(formattedJob);

        // Format device information with extensions
        var device = new DeviceInfo
        {
            Id = 0,
            Name = "NVIDIA RTX 3090",
            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024, // 24GB
            ComputeUnits = 82,
            IsAvailable = true,
            DriverVersion = "510.47.03",
            Extensions = new Dictionary<string, string>
            {
                { "cl_khr_fp64", "1.2" },
                { "cl_khr_global_int32_base_atomics", "1.0" },
                { "cl_khr_local_int32_base_atomics", "1.1" }
            }
        };

        string formattedDevice = formatter.FormatDeviceWithExtensions(device);
        Console.WriteLine(formattedDevice);

        // Create a standardized XML envelope with metadata
        string envelope = formatter.WrapInEnvelope(
            "ProcessingResults",
            formattedResults,
            new Dictionary<string, string>
            {
                { "generator", "GPU Image Processing v1.0" },
                { "environment", "Production" },
                { "processedCount", "2" }
            }
        );
        Console.WriteLine(envelope);
    }
}

```

## HelpCommandExtensions



The `HelpCommandExtensions` class provides extension methods for the `HelpCommand` class that simplify displaying help information and command documentation in the command-line interface. It includes utilities for retrieving available commands, getting command usage patterns, descriptions, examples, and summaries, making it easier to build comprehensive help systems.

### Key Features

- Get a formatted list of all available commands with their descriptions
- Retrieve usage patterns for specific commands
- Get detailed descriptions for individual commands
- Access realistic usage examples for each command
- Obtain concise summaries for command listings

### Usage Examples

```csharp

using GpuImageProcessing.Cli;
using System;

class Program
{
static void Main()
{
// Create a HelpCommand instance
var helpCommand = new HelpCommand();

// Get all available commands with their descriptions
var availableCommands = helpCommand.GetAvailableCommands();
Console.WriteLine("Available commands:");
foreach (var (name, description) in availableCommands)
{
Console.WriteLine($"  {name,-10} - {description}");
}

// Get usage information for a specific command
string processUsage = helpCommand.GetCommandUsage("process");
if (processUsage != null)
{
Console.WriteLine("\nProcess command usage:");
Console.WriteLine(processUsage);
}

// Get description for a specific command
string filterDescription = helpCommand.GetCommandDescription("filter");
if (filterDescription != null)
{
Console.WriteLine("\nFilter command description:");
Console.WriteLine(filterDescription);
}

// Get examples for a specific command
var batchExamples = helpCommand.GetCommandExamples("batch");
Console.WriteLine("\nBatch command examples:");
foreach (var example in batchExamples)
{
Console.WriteLine($"  {example}");
}

// Get summary for a specific command
string versionSummary = helpCommand.GetCommandSummary("version");
if (versionSummary != null)
{
Console.WriteLine($"\nVersion command summary: {versionSummary}");
}
}
}

```

## TimeoutUtilitiesJsonExtensions

The `TimeoutUtilitiesJsonExtensions` class provides System.Text.Json serialization utilities for timeout-related operations. It enables serialization and deserialization of timeout configurations with support for both compact and indented JSON output formats, safe error handling, and thread-safe serialization with optimized JsonSerializerOptions.

### Key Features

- JSON serialization of timeout configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Conversion between `TimeSpan` and `TimeoutConfiguration` instances
- Thread-safe serialization with optimized JsonSerializerOptions
- Configurable JSON formatting options

### Usage Examples

```csharp
using GpuImageProcessing.Utilities;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a TimeSpan timeout
        TimeSpan timeout = TimeSpan.FromSeconds(45);

        // Serialize to compact JSON
        string compactJson = TimeoutUtilitiesJsonExtensions.ToJson(timeout);
        Console.WriteLine("Compact JSON timeout configuration:");
        Console.WriteLine(compactJson);
        
        // Output: {"milliseconds":45000,"formatted":"00:00:45"}

        // Serialize to indented JSON for readability
        string indentedJson = TimeoutUtilitiesJsonExtensions.ToJson(timeout, indented: true);
        Console.WriteLine("\nIndented JSON timeout configuration:");
        Console.WriteLine(indentedJson);
        
        /* Output:
        {
          "milliseconds": 45000,
          "formatted": "00:00:45"
        }
        */

        // Deserialize from JSON string
        string json = @"{ "milliseconds": 30000, "formatted": "00:00:30" }";
        var deserializedConfig = TimeoutUtilitiesJsonExtensions.FromJson(json);

        if (deserializedConfig != null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"Milliseconds: {deserializedConfig.Milliseconds}");
            Console.WriteLine($"Formatted: {deserializedConfig.Formatted}");
            
            // Convert back to TimeSpan
            TimeSpan convertedTimeout = deserializedConfig.ToTimeSpan();
            Console.WriteLine($"Converted to TimeSpan: {convertedTimeout.TotalSeconds} seconds");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = TimeoutUtilitiesJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = TimeoutUtilitiesJsonExtensions.ToJson(timeout);
        var roundTripResult = TimeoutUtilitiesJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
    }
}
```

## MetricsPublisherJsonExtensions

The `MetricsPublisherJsonExtensions` class provides System.Text.Json serialization utilities for the `MetricsPublisher` class. It enables serialization and deserialization of metrics publisher configurations with support for both compact and indented JSON output formats, safe error handling, and thread-safe serialization with optimized JsonSerializerOptions.

### Key Features

- JSON serialization of `MetricsPublisher` instances using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration state management for metrics publishing
- Thread-safe serialization with optimized JsonSerializerOptions
- Configurable buffer size and endpoint count properties

### Usage Examples

```csharp

using GpuImageProcessing.Integration;
using System;
using System.Text.Json;

class Program
{
  static void Main()
  {
    // Create a metrics publisher with buffer size
    var publisher = new MetricsPublisher(bufferSize: 1024);

    // Serialize to compact JSON
    string compactJson = publisher.ToJson();
    Console.WriteLine("Compact JSON configuration:");
    Console.WriteLine(compactJson);

    // Serialize to indented JSON for readability
    string indentedJson = publisher.ToJson(indented: true);
    Console.WriteLine("\nIndented JSON configuration:");
    Console.WriteLine(indentedJson);

    // Deserialize from JSON string
    string json = @"{ "bufferSize": 2048, "endpointCount": 3 }";
    var deserialized = MetricsPublisherJsonExtensions.FromJson(json);

    if (deserialized != null)
    {
      Console.WriteLine($"\nDeserialized configuration:");
      Console.WriteLine($"BufferSize: {deserialized.BufferSize}");
      Console.WriteLine($"EndpointCount: {deserialized.EndpointCount}");
    }

    // Try to deserialize with error handling
    string invalidJson = @"{ invalid json }";
    bool success = MetricsPublisherJsonExtensions.TryFromJson(invalidJson, out var result);
    Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

    // Serialize and deserialize round-trip to verify data integrity
    string roundTripJson = publisher.ToJson();
    var roundTripResult = MetricsPublisherJsonExtensions.FromJson(roundTripJson);
    Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
  }
}

```

## ImageUtilitiesValidation

The `ImageUtilitiesValidation` class provides comprehensive validation utilities for image processing operations. It validates parameters and state before performing image processing tasks, ensuring that configurations, file paths, image dimensions, and other inputs are valid and safe for processing. This validation layer helps prevent runtime errors and provides detailed error messages for debugging.

### Key Features

- Validates `ImageUtilities` static class configuration including supported extensions
- Validates file paths for existence, format, and accessibility
- Validates image file access and supported formats
- Validates output directories for existence and writability
- Validates image dimensions (width and height) with reasonable limits
- Validates scale factors for proportional resizing operations
- Validates output filename parameters for safe file generation
- Returns detailed error messages through `Validate()` methods
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Throws descriptive exceptions when validation fails

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Validate ImageUtilities configuration
        var configErrors = ImageUtilitiesValidation.ValidateImageUtilitiesConfiguration();
        if (configErrors.Count > 0)
        {
            Console.WriteLine("Configuration errors:");
            foreach (var error in configErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("ImageUtilities configuration is valid!");
        }

        // Check if configuration is valid using convenience method
        bool isConfigValid = ImageUtilitiesValidation.IsImageUtilitiesConfigurationValid();
        Console.WriteLine($"Is configuration valid: {isConfigValid}");

        // Validate file path
        string filePath = "input.jpg";
        var pathErrors = filePath.ValidateFilePath();
        Console.WriteLine($"File path validation: {(pathErrors.Count == 0 ? "Valid" : string.Join(", ", pathErrors))}");

        // Validate image file access
        var imageErrors = filePath.ValidateImageFileAccess();
        Console.WriteLine($"Image file validation: {(imageErrors.Count == 0 ? "Valid" : string.Join(", ", imageErrors))}");

        // Validate output directory
        string outputDir = "/data/output";
        var dirErrors = outputDir.ValidateOutputDirectory();
        Console.WriteLine($"Output directory validation: {(dirErrors.Count == 0 ? "Valid" : string.Join(", ", dirErrors))}");

        // Validate image dimensions
        var dimensionErrors = 1920.ValidateImageDimensions(1080);
        Console.WriteLine($"Dimension validation: {(dimensionErrors.Count == 0 ? "Valid" : string.Join(", ", dimensionErrors))}");

        // Validate scale factor
        var scaleErrors = 0.75.ValidateScaleFactor();
        Console.WriteLine($"Scale factor validation: {(scaleErrors.Count == 0 ? "Valid" : string.Join(", ", scaleErrors))}");

        // Validate output filename parameters
        var filenameErrors = "input.ppm".ValidateOutputFilenameParameters("grayscale-filter");
        Console.WriteLine($"Filename parameters validation: {(filenameErrors.Count == 0 ? "Valid" : string.Join(", ", filenameErrors))}");

        // Use EnsureValid() to throw exception on invalid configuration
        try
        {
            ImageUtilitiesValidation.EnsureImageUtilitiesConfigurationValid();
            Console.WriteLine("Configuration is valid!");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Configuration validation failed: {ex.Message}");
        }

        // Example with actual file operations
        string testImage = "/data/images/test.ppm";
        if (testImage.ValidateImageFileAccess().Count == 0)
        {
            Console.WriteLine($"Processing image: {testImage}");
            
            // Create output directory
            string outputPath = "/data/output/processed";
            if (outputPath.ValidateOutputDirectory().Count == 0)
            {
                Directory.CreateDirectory(outputPath);
                Console.WriteLine($"Created output directory: {outputPath}");
            }
        }
    }
}

```

## RemoteImageServiceValidation

The `RemoteImageServiceValidation` class provides validation helpers for `RemoteImageResult` and `RemoteImageData` instances. It validates the structure and content of remote image processing results, including success states, error messages, image data integrity, content types, and byte sizes. This ensures that remote image operations produce valid, well-formed results suitable for downstream processing.

### Key Features

- Validates `RemoteImageResult` instances with comprehensive state and data validation
- Validates `RemoteImageData` instances including image data, content type, size, and metadata
- Returns detailed error messages through `Validate()` methods for debugging
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Ensures proper relationships between `IsSuccess`, `Data`, and `Error` fields in `RemoteImageResult`
- Validates content type format (must start with "image/")
- Validates image data integrity and size constraints
- Validates source URL format when present
- Validates timestamp fields (DownloadedAt cannot be in the future)

### Usage Examples

```csharp

using GpuImageProcessing.Integration;
using System;
using System.Collections.Generic;

class Program
{
static void Main()
{

// Create a valid RemoteImageResult
var validResult = new RemoteImageResult
{
    IsSuccess = true,
    Data = new RemoteImageData
    {
        ImageData = new byte[] { 0xFF, 0xD8, 0xFF }, // JPEG header
        ContentType = "image/jpeg",
        SizeBytes = 1024,
        SourceUrl = "https://example.com/image.jpg",
        DownloadedAt = DateTime.UtcNow
    },
    Error = null
};

// Validate the result
var resultErrors = validResult.Validate();
Console.WriteLine($"Valid result has {resultErrors.Count} errors"); // Output: 0

// Check if valid using convenience method
bool isValid = validResult.IsValid();
Console.WriteLine($"Is valid: {isValid}"); // Output: Is valid: True

// Create an invalid result (IsSuccess=true but Data=null)
var invalidResult = new RemoteImageResult
{
    IsSuccess = true,
    Data = null, // Invalid - must not be null when IsSuccess is true
    Error = null
};

// Validate and get detailed errors
var errors = invalidResult.Validate();
Console.WriteLine("Validation errors:");
foreach (var error in errors)
{
    Console.WriteLine($"- {error}");
}
/* Output:
- Data must not be null when IsSuccess is true.
*/

// Validate RemoteImageData separately
var invalidData = new RemoteImageData
{
    ImageData = null, // Invalid - must not be null
    ContentType = "invalid-type", // Invalid - must start with "image/"
    SizeBytes = -100, // Invalid - must be non-negative
    SourceUrl = "not-a-valid-url", // Invalid - must be valid URI if specified
    DownloadedAt = DateTime.UtcNow.AddDays(1) // Invalid - cannot be in the future
};

var dataErrors = invalidData.Validate();
Console.WriteLine($"\nData validation errors: {dataErrors.Count}");
foreach (var error in dataErrors)
{
    Console.WriteLine($"- {error}");
}
/* Output:
- ImageData must not be null.
- ContentType must start with 'image/' prefix.
- SizeBytes must be a non-negative number.
- SourceUrl must be a valid absolute URI if specified.
- DownloadedAt cannot be in the future.
*/

// Use EnsureValid() to throw exception on invalid result
try
{
    invalidResult.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"\nEnsureValid caught error: {ex.Message}");
}

// Use EnsureValid() on data
try
{
    invalidData.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"EnsureValid on data caught error: {ex.Message}");
}

// Validate a successful result with all fields
var completeResult = new RemoteImageResult
{
    IsSuccess = true,
    Data = new RemoteImageData
    {
        ImageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }, // PNG header
        ContentType = "image/png",
        SizeBytes = 2048,
        SourceUrl = "https://cdn.example.com/photos/image.png",
        DownloadedAt = DateTime.UtcNow.AddMinutes(-5)
    },
    Error = null
};

var completeErrors = completeResult.Validate();
Console.WriteLine($"\nComplete result validation: {completeErrors.Count} errors"); // Output: 0
}
}

```

## HealthCheckServiceValidation

The `HealthCheckServiceValidation` class provides validation helpers for health monitoring types in the GPU Image Processing library. It validates `ComponentHealth` and `HealthCheckResult` instances with comprehensive validation rules, returning detailed error messages through `Validate()` methods. This ensures that health monitoring data contains valid, well-formed information suitable for system monitoring and alerting scenarios.

### Key Features

- Validates `ComponentHealth` instances with comprehensive property validation (status, message, timestamp, details)
- Validates `HealthCheckResult` instances including status, timestamp, components collection, and summary
- Validates `MemoryHealthCheck` and `ResponseTimeHealthCheck` instances with basic null checks
- Returns detailed error messages through `Validate()` methods for debugging and validation scenarios
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Ensures proper relationships between health status values and required fields
- Validates component collection keys and nested component health values

### Usage Examples

```csharp

using GpuImageProcessing.Monitoring;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Create a valid ComponentHealth instance
        var validComponentHealth = new ComponentHealth
        {
            Status = HealthStatus.Healthy,
            Message = "GPU device is operational",
            CheckedAt = DateTime.UtcNow,
            Details = new Dictionary<string, string> { { "device", "NVIDIA RTX 3090" } }
        };

        // Validate the component health
        var componentErrors = validComponentHealth.Validate();
        Console.WriteLine($"Valid component has {componentErrors.Count} errors"); // Output: 0

        // Check if valid using convenience method
        bool isComponentValid = validComponentHealth.IsValid();
        Console.WriteLine($"Component is valid: {isComponentValid}"); // Output: True

        // Create an invalid ComponentHealth (missing required fields)
        var invalidComponentHealth = new ComponentHealth
        {
            Status = HealthStatus.Unknown, // Invalid - must be a valid status other than Unknown
            Message = null, // Invalid - must not be null or empty
            CheckedAt = default, // Invalid - must be set to a valid DateTime
            Details = null // Invalid - must not be null
        };

        // Validate and get detailed errors
        var errors = invalidComponentHealth.Validate();
        Console.WriteLine("Component validation errors:");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error}");
        }
        /* Output:
        - ComponentHealth.Status must be set to a valid HealthStatus value other than Unknown
        - ComponentHealth.Message cannot be null or empty
        - ComponentHealth.CheckedAt must be set to a valid DateTime
        - ComponentHealth.Details cannot be null
        */

        // Create a valid HealthCheckResult instance
        var validResult = new HealthCheckResult
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTime.UtcNow,
            Summary = "All systems operational",
            Components = new Dictionary<string, ComponentHealth>
            {
                { "gpu", new ComponentHealth { Status = HealthStatus.Healthy, Message = "GPU device operational", CheckedAt = DateTime.UtcNow, Details = new Dictionary<string, string> { { "device", "NVIDIA RTX 3090" } } } },
                { "memory", new ComponentHealth { Status = HealthStatus.Healthy, Message = "Memory usage within limits", CheckedAt = DateTime.UtcNow, Details = new Dictionary<string, string> { { "used", "4.2GB" }, { "total", "24GB" } } } }
            }
        };

        // Validate the health check result
        var resultErrors = validResult.Validate();
        Console.WriteLine($"\nValid result has {resultErrors.Count} errors"); // Output: 0

        // Check if valid using convenience method
        bool isResultValid = validResult.IsValid();
        Console.WriteLine($"HealthCheckResult is valid: {isResultValid}"); // Output: True

        // Use EnsureValid() to throw exception on invalid result
        try
        {
            invalidComponentHealth.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nEnsureValid caught error: {ex.Message}");
        }

        // Validate MemoryHealthCheck (basic null validation only)
        var memoryCheck = new MemoryHealthCheck();
        bool isMemoryValid = memoryCheck.IsValid();
        Console.WriteLine($"\nMemoryHealthCheck is valid: {isMemoryValid}"); // Output: True
    }
}

```

## HttpImageClientValidation

The `HttpImageClientValidation` class provides comprehensive validation utilities for `HttpImageClient` instances and related HTTP image operations. It includes validation methods for URLs, file paths, directory existence, HTTP status codes, timeouts, and retry configurations, ensuring robust error handling for HTTP-based image processing workflows.

### Key Features

- Validates `HttpImageClient` instances with null checks and instance validation
- Validates HTTP URLs for proper scheme (http/https) and format
- Validates file paths for invalid characters and existence
- Validates output directories for existence or creation requirements
- Validates HTTP status codes (100-599 range)
- Validates timeout durations (1-300 seconds range)
- Validates maximum retry counts (0-10 range)
- Provides both error collection and boolean validation methods
- Includes convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns

### Usage Examples

```csharp

using GpuImageProcessing.Integration;
using System;
using System.Collections.Generic;
using System.Net.Http;

class Program
{
    static void Main()
    {
        // Create an HttpImageClient instance
        var httpClient = new HttpClient();
        var imageClient = new HttpImageClient(httpClient, "https://api.example.com/images");

        // Validate the client instance
        var clientErrors = imageClient.Validate();
        if (clientErrors.Count > 0)
        {
            Console.WriteLine("Client validation errors:");
            foreach (var error in clientErrors)
            {
                Console.WriteLine($" - {error}");
            }
        }
        else
        {
            Console.WriteLine("HttpImageClient instance is valid");
        }

        // Check if valid using convenience method
        bool isValid = imageClient.IsValid();
        Console.WriteLine($"Is valid: {isValid}");

        // Validate a URL
        string testUrl = "https://example.com/image.jpg";
        var urlErrors = HttpImageClientValidation.ValidateUrl(testUrl);
        Console.WriteLine($"URL validation: {(urlErrors.Count == 0 ? "Valid" : string.Join(", ", urlErrors))}");

        // Validate an invalid URL
        string invalidUrl = "ftp://example.com/image.jpg";
        var invalidUrlErrors = HttpImageClientValidation.ValidateUrl(invalidUrl);
        Console.WriteLine($"Invalid URL validation has {invalidUrlErrors.Count} errors");

        // Validate a file path
        string filePath = "/data/images/input.jpg";
        var pathErrors = HttpImageClientValidation.ValidateFilePath(filePath);
        Console.WriteLine($"File path validation: {(pathErrors.Count == 0 ? "Valid" : string.Join(", ", pathErrors))}");

        // Validate file existence
        var fileExistsErrors = HttpImageClientValidation.ValidateFileExists("/nonexistent/file.jpg");
        Console.WriteLine($"File existence check: {(fileExistsErrors.Count == 0 ? "File exists" : string.Join(", ", fileExistsErrors))}");

        // Validate output directory
        string outputDir = "/data/output";
        var dirErrors = HttpImageClientValidation.ValidateOutputDirectory(outputDir);
        Console.WriteLine($"Output directory validation: {(dirErrors.Count == 0 ? "Valid" : string.Join(", ", dirErrors))}");

        // Validate HTTP status code
        var statusErrors = HttpImageClientValidation.ValidateHttpStatusCode(404);
        Console.WriteLine($"Status code validation: {(statusErrors.Count == 0 ? "Valid" : string.Join(", ", statusErrors))}");

        // Validate timeout
        var timeoutErrors = HttpImageClientValidation.ValidateTimeout(TimeSpan.FromSeconds(45));
        Console.WriteLine($"Timeout validation: {(timeoutErrors.Count == 0 ? "Valid" : string.Join(", ", timeoutErrors))}");

        // Validate maximum retries
        var retryErrors = HttpImageClientValidation.ValidateMaxRetries(3);
        Console.WriteLine($"Retry validation: {(retryErrors.Count == 0 ? "Valid" : string.Join(", ", retryErrors))}");

        // Use EnsureValid() to throw exception on invalid client
        try
        {
            var invalidClient = new HttpImageClient(null!, "");
            invalidClient.EnsureValid();
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"EnsureValid caught null client: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"EnsureValid validation failed: {ex.Message}");
        }
    }
}

```

## OpenCLExceptionValidation

The `OpenCLExceptionValidation` class provides validation helpers for `OpenCLException` instances and related OpenCL error types. It validates exception properties like device names, error codes, kernel sources, and compilation logs, returning detailed error messages through `Validate()` methods. This ensures that OpenCL exceptions contain valid, well-formed data suitable for error handling and debugging scenarios.

### Key Features

- Validates `OpenCLException` instances with comprehensive property validation
- Validates derived exception types (`DeviceInitializationException`, `KernelCompilationException`) with type-specific rules
- Returns detailed error messages through `Validate()` methods for debugging
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Ensures proper OpenCL error code ranges (-1 to -1000) and device name formatting
- Validates kernel source code and compilation logs for kernel compilation exceptions
- Validates device initialization exception properties

### Usage Examples

```csharp

using GpuImageProcessing.Core.Exceptions;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        // Create a valid OpenCLException
        var validException = new OpenCLException("Device initialization failed")
        {
            DeviceName = "NVIDIA RTX 3090",
            OpenCLErrorCode = -30 // CL_INVALID_VALUE
        };

        // Validate the exception
        var validationErrors = validException.Validate();
        Console.WriteLine($"Valid exception has {validationErrors.Count} errors"); // Output: 0

        // Check if valid using convenience method
        bool isValid = validException.IsValid();
        Console.WriteLine($"Is valid: {isValid}"); // Output: Is valid: True

        // Create an invalid exception (null device name)
        var invalidException = new OpenCLException("Invalid device")
        {
            DeviceName = null, // Invalid - must not be null or whitespace
            OpenCLErrorCode = -1001 // Invalid - outside valid range [-1000, -1]
        };

        // Validate and get detailed errors
        var errors = invalidException.Validate();
        Console.WriteLine("Validation errors:");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error}");
        }
        /* Output:
        - DeviceName must not be null or whitespace.
        - OpenCLErrorCode must be a negative value in the range [-1000, -1] (was -1001).
        */

        // Validate DeviceInitializationException separately
        var deviceInitException = new DeviceInitializationException("Failed to initialize device")
        {
            DeviceName = "", // Invalid - must not be null or whitespace
            OpenCLErrorCode = -1002 // Invalid - outside valid range
        };

        var deviceErrors = deviceInitException.Validate();
        Console.WriteLine($"\nDevice initialization validation has {deviceErrors.Count} errors");
        foreach (var error in deviceErrors)
        {
            Console.WriteLine($"- {error}");
        }
        /* Output:
        - DeviceName must not be null or whitespace for DeviceInitializationException.
        - OpenCLErrorCode must be a negative value in the range [-1000, -1] (was -1002).
        */

        // Validate KernelCompilationException separately
        var kernelException = new KernelCompilationException("Kernel compilation failed")
        {
            KernelSource = null, // Invalid - must not be null or whitespace
            CompilationLog = "", // Invalid - must not be null or whitespace
            DeviceName = "AMD Radeon RX 6800",
            OpenCLErrorCode = -48 // CL_INVALID_PROGRAM
        };

        var kernelErrors = kernelException.Validate();
        Console.WriteLine($"\nKernel compilation validation has {kernelErrors.Count} errors");
        foreach (var error in kernelErrors)
        {
            Console.WriteLine($"- {error}");
        }
        /* Output:
        - KernelSource must not be null or whitespace for KernelCompilationException.
        - CompilationLog must not be null or whitespace for KernelCompilationException.
        */

        // Use EnsureValid() to throw exception on invalid exception
        try
        {
            invalidException.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nEnsureValid caught error: {ex.Message}");
        }

        // Use EnsureValid() on device initialization exception
        try
        {
            deviceInitException.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"EnsureValid on device exception caught error: {ex.Message}");
        }

        // Use EnsureValid() on kernel compilation exception
        try
        {
            kernelException.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"EnsureValid on kernel exception caught error: {ex.Message}");
        }

        // Validate a complete exception with all fields
        var completeException = new OpenCLException("Operation completed successfully")
        {
            DeviceName = "Intel UHD Graphics 630",
            OpenCLErrorCode = -77 // CL_INVALID_KERNEL_NAME
        };

        var completeErrors = completeException.Validate();
        Console.WriteLine($"\nComplete exception validation: {completeErrors.Count} errors"); // Output: 0
    }
}

```

## JobRepositoryExtensions

The `JobRepositoryExtensions` class provides extension methods for the `JobRepository` class that extend the repository with additional query capabilities for processing jobs. It includes methods for filtering jobs by status, progress, creation date, searching by name or description, retrieving statistics, and finding long-running jobs. These extensions simplify common job management operations and provide a fluent API for job repository queries.

### Key Features

- Filter jobs by status, progress percentage, or creation date range
- Search jobs by name or description with case-insensitive matching
- Retrieve statistics and metrics for jobs by status
- Find active jobs and long-running jobs
- Get most recently completed jobs
- Count total jobs across all statuses

### Usage Examples

```csharp

using GpuImageProcessing.Core.Repository;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Constants;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Initialize the job repository (typically via dependency injection)
        var jobRepository = new JobRepository(...);

        // Get jobs by status
        var pendingJobs = await jobRepository.GetByStatusAsync(ProcessingStatus.Pending);
        Console.WriteLine($"Pending jobs: {pendingJobs.Count}");

        // Get active jobs (Pending, Running, or WarningState)
        var activeJobs = await jobRepository.GetActiveJobsAsync();
        Console.WriteLine($"Active jobs: {activeJobs.Count}");
        foreach (var job in activeJobs)
        {
            Console.WriteLine($" - {job.Name} (Status: {job.Status}, Progress: {job.ProgressPercentage}%)");
        }

        // Get jobs with progress above 75%
        var highProgressJobs = await jobRepository.GetJobsAboveProgressAsync(75f);
        Console.WriteLine($"Jobs with >75% progress: {highProgressJobs.Count}");

        // Get jobs created between specific dates
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var recentJobs = await jobRepository.GetJobsCreatedBetweenAsync(startDate, endDate);
        Console.WriteLine($"Jobs created in last 7 days: {recentJobs.Count}");

        // Get the 20 most recently completed jobs
        var completedJobs = await jobRepository.GetMostRecentCompletedAsync(20);
        Console.WriteLine($"Most recent completed jobs: {completedJobs.Count}");

        // Search jobs by name or description
        var searchResults = await jobRepository.SearchByNameOrDescriptionAsync("resize");
        Console.WriteLine($"Jobs matching 'resize': {searchResults.Count}");

        // Get statistics for completed jobs
        var completedStats = await jobRepository.GetStatisticsByStatusAsync(ProcessingStatus.Completed);
        Console.WriteLine($"Completed jobs stats:");
        Console.WriteLine($"  Total: {completedStats.TotalJobs}");
        Console.WriteLine($"  Success rate: {completedStats.SuccessRate:F1}%");
        Console.WriteLine($"  Avg completion time: {completedStats.AverageCompletionTime:F2}s");

        // Find jobs running longer than 1 hour
        var longRunningJobs = await jobRepository.GetLongRunningJobsAsync(3600);
        Console.WriteLine($"Jobs running >1 hour: {longRunningJobs.Count}");

        // Get total job count
        var totalJobs = await jobRepository.GetTotalJobsCountAsync();
        Console.WriteLine($"Total jobs in system: {totalJobs}");
    }
}

```

## ConfigurationUtilitiesJsonExtensions

The `ConfigurationUtilitiesJsonExtensions` class provides System.Text.Json serialization utilities for reading and updating application configuration state. It enables serialization of current configuration to JSON, and deserialization of configuration updates from JSON strings, with support for both compact and indented output formats, and safe error handling for configuration updates.

### Key Features

- Serialize current configuration state to JSON with `ToJson()` method
- Deserialize configuration updates from JSON with `FromJson()` method
- Safe deserialization with error handling using `TryFromJson()` method
- Support for both compact and indented JSON output formats
- Thread-safe serialization with optimized JsonSerializerOptions
- Configuration state management for environment variables and application settings

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Serialize current configuration to compact JSON
        string compactJson = ConfigurationUtilitiesJsonExtensions.ToJson();
        Console.WriteLine("Current configuration (compact):");
        Console.WriteLine(compactJson);

        // Serialize current configuration to indented JSON for readability
        string indentedJson = ConfigurationUtilitiesJsonExtensions.ToJson(indented: true);
        Console.WriteLine("\nCurrent configuration (indented):");
        Console.WriteLine(indentedJson);

        // Example configuration JSON that you might receive or create
        string exampleConfigJson = @"{
    "environment": "Production",
    "dataDirectory": "/var/lib/gpu-image-processing",
    "logDirectory": "/var/log/gpu-image-processing",
    "tempDirectory": "/tmp/gpu-image-processing",
    "maxConcurrentOperations": 8,
    "operationTimeoutSeconds": 30,
    "preferredDeviceId": 0,
    "logLevel": "Info",
    "enablePerformanceLogging": true,
    "enableDebugLogging": false,
    "cacheSizeMb": 512,
    "useGpuAcceleration": true,
    "defaultProfile": "high-performance"
}";

        // Apply configuration from JSON string
        ConfigurationUtilitiesJsonExtensions.FromJson(exampleConfigJson);
        Console.WriteLine("\nConfiguration applied successfully!");

        // Try to apply configuration with error handling
        string invalidJson = @"{ invalid json }";
        bool success = ConfigurationUtilitiesJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = ConfigurationUtilitiesJsonExtensions.ToJson();
        ConfigurationUtilitiesJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine("\nRound-trip serialization successful!");
    }
}

```

## ConfigurationValidatorValidation

The `ConfigurationValidatorValidation` class provides comprehensive validation utilities for configuration validation scenarios in the GPU Image Processing library. It includes validation methods for configuration dictionaries, integer ranges, timeout durations, batch sizes, memory specifications, URLs, and environment variables, ensuring that configurations are valid and safe for processing.

### Key Features

- Validates configuration dictionaries and required keys with comprehensive error reporting
- Validates integer configuration values within specified bounds
- Validates timeout durations against minimum and maximum constraints
- Validates batch sizes are positive integers
- Validates memory size specifications (e.g., "1GB", "512MB") against minimum requirements
- Validates URL formats (HTTP/HTTPS only)
- Validates environment variable existence when required
- Returns detailed error messages through `Validate()` methods for debugging
- Provides consistent validation patterns across all configuration types

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Collections.Generic;

class Program
{
static void Main()
{
// Example 1: Validate configuration dictionary with required keys
var config = new Dictionary<string, string>
{
{ "apiKey", "your-api-key-here" },
{ "timeout", "30" },
{ "maxConcurrent", "8" }
};

var requiredKeys = new[] { "apiKey", "timeout", "maxConcurrent" };
var configErrors = ConfigurationValidatorValidation.Validate(config, requiredKeys);

if (configErrors.Count > 0)
{
Console.WriteLine("Configuration validation errors:");
foreach (var error in configErrors)
{
Console.WriteLine($"- {error}");
}
}
else
{
Console.WriteLine("Configuration is valid!");
}

// Example 2: Validate integer configuration value within bounds
var intErrors = ConfigurationValidatorValidation.Validate("45", 1, 100, "BatchSize");
Console.WriteLine($"\nInteger validation: {intErrors.Count} errors");

// Example 3: Validate timeout duration
var timeoutErrors = ConfigurationValidatorValidation.Validate(
TimeSpan.FromSeconds(45),
TimeSpan.FromSeconds(1),
TimeSpan.FromSeconds(300),
"ProcessingTimeout"
);
Console.WriteLine($"Timeout validation: {timeoutErrors.Count} errors");

// Example 4: Validate batch size
var batchErrors = ConfigurationValidatorValidation.Validate(16);
Console.WriteLine($"\nBatch size validation: {batchErrors.Count} errors");

// Example 5: Validate memory size specification
var memoryErrors = ConfigurationValidatorValidation.Validate("512MB", 1024 * 1024); // 512MB minimum
Console.WriteLine($"Memory validation: {memoryErrors.Count} errors");

// Example 6: Validate URL format
var urlErrors = ConfigurationValidatorValidation.Validate("https://api.example.com/images");
Console.WriteLine($"\nURL validation: {urlErrors.Count} errors");

// Example 7: Validate environment variable
var envErrors = ConfigurationValidatorValidation.Validate("GPU_IMAGE_PROCESSING_API_KEY", required: true);
Console.WriteLine($"Environment variable validation: {envErrors.Count} errors");

// Example 8: Validate configuration dictionary (basic validation)
var dictErrors = ConfigurationValidatorValidation.Validate(config);
Console.WriteLine($"\nDictionary validation: {dictErrors.Count} errors");
}
}

```

## CliParserExtensions

The `CliParserExtensions` class provides extension methods for the `CliParser` and `ParsedCommand` classes that simplify command-line argument parsing and validation. It includes safe parsing methods that handle errors gracefully, and type-safe option retrieval methods for integers, booleans, doubles, and positional arguments.

### Key Features

- Safe command parsing with `ParseSafely()` that returns an empty result instead of throwing on errors
- Type-safe option retrieval with `GetIntegerOption()`, `GetBooleanOption()`, and `GetDoubleOption()`
- Positional argument extraction with `GetPositionalArguments()` and optional count validation
- Null safety and validation for all public methods
- Culture-invariant parsing for numeric values

### Usage Examples

```csharp

using GpuImageProcessing.Cli;
using System;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Create a CLI parser instance
        var parser = new CliParser();
        
        // Parse command-line arguments safely (returns empty on parsing errors)
        var parsedCommand = parser.ParseSafely(new[] { "--width=800", "--height=600", "--verbose", "input.ppm" });
        
        // Get integer option with default value
        int width = parsedCommand.GetIntegerOption("--width", 1024);
        Console.WriteLine($"Width: {width}"); // Output: Width: 800
        
        // Get boolean option with default value
        bool verbose = parsedCommand.GetBooleanOption("--verbose", false);
        Console.WriteLine($"Verbose mode: {verbose}"); // Output: Verbose mode: True
        
        // Get double option with default value
        double scale = parsedCommand.GetDoubleOption("--scale", 1.0);
        Console.WriteLine($"Scale: {scale:F2}"); // Output: Scale: 1.00
        
        // Get positional arguments with count validation
        var positionalArgs = parsedCommand.GetPositionalArguments(1);
        string inputFile = positionalArgs.First();
        Console.WriteLine($"Input file: {inputFile}"); // Output: Input file: input.ppm
        
        // Example with various argument formats
        var testArgs = new[] { "--quality=95", "--force", "--threshold=0.75", "image.png" };
        var testCommand = parser.ParseSafely(testArgs);
        
        Console.WriteLine($"Quality: {testCommand.GetIntegerOption("--quality", 80)}");
        Console.WriteLine($"Force: {testCommand.GetBooleanOption("--force", false)}");
        Console.WriteLine($"Threshold: {testCommand.GetDoubleOption("--threshold", 0.5)}");
        Console.WriteLine($"Positional args: {testCommand.GetPositionalArguments().Count}");
    }
}

```

## DistributedCacheExtensions

The `DistributedCacheExtensions` class provides extension methods for `DistributedCache` that extend the basic cache operations with higher-level, batch, and metadata operations. It simplifies common caching patterns like cache-aside, bulk operations, and cache statistics monitoring.



### Key Features

- Cache-aside pattern with `GetOrCreateAsync()` for automatic value computation and caching
- Batch operations with `GetManyAsync()`, `SetManyAsync()`, and `RemoveManyAsync()` for efficient bulk cache operations
- Metadata queries with `ContainsKeyAsync()`, `GetExpirationAsync()`, and `GetMetadataAsync()`
- Cache statistics monitoring with `GetStatsString()` for debugging and logging
- Null safety and validation for all public methods

### Usage Examples

```csharp

using GpuImageProcessing.Caching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Initialize cache (typically via dependency injection in real applications)
        var cache = new DistributedCache(maxMemoryBytes: 100_000_000);

        // Cache-aside pattern: get or create value
        var imageData = await cache.GetOrCreateAsync(
            key: "processed:image:12345",
            valueFactory: async () => 
            {
                // Expensive computation or remote API call
                Console.WriteLine("Computing expensive value...");
                await Task.Delay(100); // Simulate work
                return new byte[] { 0xFF, 0xD8, 0xFF }; // JPEG header
            },
            ttl: TimeSpan.FromHours(1) // Cache for 1 hour
        );

        Console.WriteLine($"Retrieved image data: {imageData.Length} bytes");

        // Batch operations: get multiple values efficiently
        var keys = new List<string> { "cache:key1", "cache:key2", "cache:key3" };
        var values = await cache.GetManyAsync<string>(keys);
        
        Console.WriteLine($"Found {values.Count} cached values");

        // Batch set: store multiple values in one operation
        var newValues = new Dictionary<string, string>
        {
            { "config:theme", "dark" },
            { "config:language", "en-US" },
            { "config:timeout", "30" }
        };
        
        await cache.SetManyAsync(newValues, TimeSpan.FromDays(7));
        Console.WriteLine("Stored multiple configuration values");

        // Check key existence without retrieving value
        bool hasKey = await cache.ContainsKeyAsync("config:theme");
        Console.WriteLine($"Has config:theme: {hasKey}");

        // Get expiration time for a key
        var expiresAt = await cache.GetExpirationAsync("config:theme");
        Console.WriteLine($"Expires at: {expiresAt?.ToString("o") ?? "never"}");

        // Get cache entry metadata
        var metadata = await cache.GetMetadataAsync("config:theme");
        if (metadata != null)
        {
            Console.WriteLine($"Cache entry metadata:");
            Console.WriteLine($"  Key: {metadata.Key}");
            Console.WriteLine($"  Size: {metadata.SizeBytes} bytes");
            Console.WriteLine($"  Created: {metadata.CreatedAt:T}");
            Console.WriteLine($"  Last accessed: {metadata.LastAccessedAt:T}");
            Console.WriteLine($"  Access count: {metadata.AccessCount}");
            Console.WriteLine($"  Expires: {metadata.ExpiresAt?.ToString("T") ?? "never"}");
        }

        // Remove multiple keys in one operation
        var keysToRemove = new[] { "cache:key1", "cache:key2" };
        int removedCount = await cache.RemoveManyAsync(keysToRemove);
        Console.WriteLine($"Removed {removedCount} keys");

        // Get cache statistics for monitoring
        string stats = cache.GetStatsString();
        Console.WriteLine($"\n{stats}");
    }
}

```

## ImageProcessingControllerExtensions

The `ImageProcessingControllerExtensions` class provides extension methods for the `ImageProcessingController` that simplify batch image processing operations. It includes convenient methods for registering multiple images, applying filters and transforms to batches, retrieving image information and processing results, and managing batch jobs, making it easier to build scalable image processing pipelines.

### Key Features

- Batch registration of multiple images with `RegisterImagesAsync()`
- Parallel application of filters to multiple images with `ApplyFilterToImagesAsync()`
- Parallel application of transforms to multiple images with `ApplyTransformToImagesAsync()`
- Bulk retrieval of image information with `GetImagesInfoAsync()`
- Batch retrieval of processing results with `GetProcessingResultsByImageAsync()`
- Bulk cancellation of batch jobs with `CancelBatchJobsAsync()`
- Batch retrieval of job statuses with `GetBatchJobsStatusAsync()`
- Error aggregation and reporting for batch operations
- Null safety and validation for all public methods

### Usage Examples

```csharp

using GpuImageProcessing.Api;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
static async Task Main()
{
// Initialize the controller (typically via dependency injection in real applications)
var controller = new ImageProcessingController(...);

// Register multiple images for batch processing
var imagePaths = new List<string> {
"/data/images/photo1.jpg",
"/data/images/photo2.png",
"/data/images/photo3.bmp"
};

var registrationResult = await controller.RegisterImagesAsync(imagePaths, "Vacation photos batch");

if (registrationResult.IsSuccess)
{
Console.WriteLine($"Successfully registered {registrationResult.Data?.Count} images");
foreach (var image in registrationResult.Data ?? new List<ImageMetadata>())
{
Console.WriteLine($" - Image ID: {image.Id}, Name: {image.Name}");
}
}
else
{
Console.WriteLine($"Registration failed: {registrationResult.Message}");
}

// Apply the same filter to multiple images in parallel
var filterId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
var imageIds = registrationResult.Data?.Select(img => img.Id).ToList() ?? new List<Guid>();

var filterResult = await controller.ApplyFilterToImagesAsync(imageIds, filterId);

if (filterResult.IsSuccess)
{
Console.WriteLine($"Successfully applied filter to {filterResult.Data?.Count} images");
foreach (var result in filterResult.Data ?? new List<ProcessingResult>())
{
Console.WriteLine($" - Image {result.ImageId}: {result.Status}");
}
}
else
{
Console.WriteLine($"Filter application failed: {filterResult.Message}");
}

// Apply the same transform to multiple images in parallel
var transformId = Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa7");

var transformResult = await controller.ApplyTransformToImagesAsync(imageIds, transformId);

if (transformResult.IsSuccess)
{
Console.WriteLine($"Successfully applied transform to {transformResult.Data?.Count} images");
foreach (var result in transformResult.Data ?? new List<ProcessingResult>())
{
Console.WriteLine($" - Image {result.ImageId}: {result.Status}");
}
}

// Get information for multiple images
var infoResult = await controller.GetImagesInfoAsync(imageIds);

if (infoResult.IsSuccess)
{
Console.WriteLine($"Retrieved info for {infoResult.Data?.Count} images");
foreach (var image in infoResult.Data ?? new List<ImageMetadata>())
{
Console.WriteLine($" - {image.Name}: {image.Width}x{image.Height}, {image.Channels} channels");
}
}

// Get processing results for multiple images
var resultsResult = await controller.GetProcessingResultsByImageAsync(imageIds);

if (resultsResult.IsSuccess)
{
foreach (var kvp in resultsResult.Data ?? new Dictionary<Guid, List<ProcessingResult>>())
{
Console.WriteLine($"Processing results for image {kvp.Key}:");
foreach (var result in kvp.Value)
{
Console.WriteLine($"  - Result {result.Id}: {result.Status}");
}
}
}

// Cancel multiple batch jobs
var jobIds = new List<Guid> {
	Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
	Guid.Parse("123e4567-e89b-12d3-a456-426614174001")
};

var cancelResult = await controller.CancelBatchJobsAsync(jobIds);

if (cancelResult.IsSuccess)
{
Console.WriteLine($"Successfully cancelled {cancelResult.Data?.Count} jobs");
}
else
{
Console.WriteLine($"Cancellation failed: {cancelResult.Message}");
}

// Get status for multiple batch jobs
var statusResult = await controller.GetBatchJobsStatusAsync(jobIds);

if (statusResult.IsSuccess)
{
foreach (var kvp in statusResult.Data ?? new Dictionary<Guid, BatchJobStatus>())
{
Console.WriteLine($"Job {kvp.Key} status: {kvp.Value}");
}
}
}

```

## BatchProcessingUtilities

The `BatchProcessingUtilities` class provides utilities for managing and processing batches of images efficiently. It includes batch splitting, progress tracking, and result aggregation.

### Usage Example

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class Program

{
    static async Task Main()
    {

        var inputFiles = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            inputFiles.Add($"/data/input/image_{i:D4}.jpg");
        }
        
        // Process in batches of 16
        int batchSize = 16;
        int completed = 0;
        
        var batches = BatchProcessingUtilities.SplitIntoBatches(inputFiles, batchSize);
        Console.WriteLine($"Processing {batches.Count} batches...");
        
        foreach (var batch in batches)
        {
            // Simulate processing
            await Task.Delay(100);
            completed += batch.Count;
            
            var progress = (double)completed / inputFiles.Count * 100;
            Console.WriteLine($"Progress: {progress:F1}% ({completed}/{inputFiles.Count})");
        }
        
        Console.WriteLine("Batch processing completed!");
    }
}
```
Console.WriteLine("Batch processing completed!");
}
}
```

## MetricsUtilitiesValidation

The `MetricsUtilitiesValidation` class provides comprehensive validation utilities for image quality metrics and statistical analysis results. It validates `StatisticalMetrics`, `Histogram`, and `HistogramBucket` instances with detailed error messages through `Validate()` methods, ensuring that computed metrics are valid and safe for downstream processing. This validation layer helps prevent runtime errors and provides detailed error messages for debugging metric calculations.

### Key Features

- Validates `StatisticalMetrics` instances including count, min, max, mean, median, percentiles, standard deviation, and sum with comprehensive range and consistency checks
- Validates `Histogram` instances including bucket validation, total count matching, and range consistency across buckets
- Validates `HistogramBucket` instances including bucket number, count, percentage, and range validation
- Returns detailed error messages through `Validate()` methods for debugging and validation scenarios
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Ensures proper relationships between statistical values (e.g., mean between min/max, std dev non-negative)
- Throws descriptive exceptions when validation fails with comprehensive error details

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Collections.Generic;

class Program
{

static void Main()
{

// Create valid statistical metrics from image quality analysis
var validMetrics = new StatisticalMetrics
{
Count = 1000,
Min = 25.5,
Max = 98.7,
Mean = 67.3,
Median = 68.1,
P95 = 95.2,
P99 = 97.8,
StdDev = 12.4,
Sum = 67300.0
};

// Validate the metrics
var metricErrors = validMetrics.Validate();
Console.WriteLine($"StatisticalMetrics validation: {(metricErrors.Count == 0 ? "Valid" : string.Join(", ", metricErrors))}");

// Check if valid using convenience method
bool isMetricsValid = validMetrics.IsValid();
Console.WriteLine($"Is StatisticalMetrics valid: {isMetricsValid}");

// Create a valid histogram
var validHistogram = new Histogram
{
TotalCount = 1000,
Buckets = new List<HistogramBucket>
{
new HistogramBucket { BucketNumber = 1, Min = 0, Max = 25, Count = 250, Percentage = 25.0 },
new HistogramBucket { BucketNumber = 2, Min = 25, Max = 50, Count = 500, Percentage = 50.0 },
new HistogramBucket { BucketNumber = 3, Min = 50, Max = 75, Count = 200, Percentage = 20.0 },
new HistogramBucket { BucketNumber = 4, Min = 75, Max = 100, Count = 50, Percentage = 5.0 }
}
};

// Validate the histogram
var histogramErrors = validHistogram.Validate();
Console.WriteLine($"\nHistogram validation: {(histogramErrors.Count == 0 ? "Valid" : string.Join(", ", histogramErrors))}");

// Check if valid using convenience method
bool isHistogramValid = validHistogram.IsValid();
Console.WriteLine($"Is Histogram valid: {isHistogramValid}");

// Create a valid histogram bucket
var validBucket = new HistogramBucket
{
BucketNumber = 1,
Min = 0,
Max = 25,
Count = 250,
Percentage = 25.0
};

// Validate the bucket
var bucketErrors = validBucket.Validate();
Console.WriteLine($"\nHistogramBucket validation: {(bucketErrors.Count == 0 ? "Valid" : string.Join(", ", bucketErrors))}");

// Check if valid using convenience method
bool isBucketValid = validBucket.IsValid();
Console.WriteLine($"Is HistogramBucket valid: {isBucketValid}");

// Use EnsureValid() to throw exception on invalid metrics
try
{
var invalidMetrics = new StatisticalMetrics
{
Count = -1, // Invalid - negative count
Mean = double.NaN
};
invalidMetrics.EnsureValid();
}
catch (ArgumentException ex)
{
Console.WriteLine($"\nEnsureValid caught error: {ex.Message}");
}

// Use EnsureValid() on histogram
try
{
var invalidHistogram = new Histogram
{
TotalCount = 100,
Buckets = new List<HistogramBucket> { new HistogramBucket { BucketNumber = 1, Min = 50, Max = 25, Count = 100 } }
};
invalidHistogram.EnsureValid();
}
catch (ArgumentException ex)
{
Console.WriteLine($"EnsureValid on Histogram caught error: {ex.Message}");
}

// Use EnsureValid() on bucket
try
{
var invalidBucket = new HistogramBucket
{
BucketNumber = -1, // Invalid - negative bucket number
Min = 25,
Max = 0, // Invalid - min > max
Count = 100
};
invalidBucket.EnsureValid();
}
catch (ArgumentException ex)
{
Console.WriteLine($"EnsureValid on HistogramBucket caught error: {ex.Message}");
}

// Example with actual metric calculation workflow
Console.WriteLine("\n--- Image Quality Metrics Validation Example ---");

// Simulate computing metrics from image processing results
var qualityMetrics = new StatisticalMetrics
{
Count = 500,
Min = 30.2,
Max = 99.8,
Mean = 75.6,
Median = 78.3,
P95 = 98.1,
P99 = 99.5,
StdDev = 15.2,
Sum = 37800.0
};

// Validate before using metrics in calculations
if (qualityMetrics.IsValid())
{
Console.WriteLine($"Quality metrics are valid!");
Console.WriteLine($"Range: {qualityMetrics.Min:F1}-{qualityMetrics.Max:F1}");
Console.WriteLine($"Mean: {qualityMetrics.Mean:F1} ± {qualityMetrics.StdDev:F1}");
Console.WriteLine($"P95: {qualityMetrics.P95:F1}, P99: {qualityMetrics.P99:F1}");
}

// Create histogram from distribution analysis
var distribution = new Histogram
{
TotalCount = 500,
Buckets = new List<HistogramBucket>
{
new HistogramBucket { BucketNumber = 1, Min = 0, Max = 20, Count = 50, Percentage = 10.0 },
new HistogramBucket { BucketNumber = 2, Min = 20, Max = 40, Count = 150, Percentage = 30.0 },
new HistogramBucket { BucketNumber = 3, Min = 40, Max = 60, Count = 200, Percentage = 40.0 },
new HistogramBucket { BucketNumber = 4, Min = 60, Max = 80, Count = 80, Percentage = 16.0 },
new HistogramBucket { BucketNumber = 5, Min = 80, Max = 100, Count = 20, Percentage = 4.0 }
}
};

if (distribution.IsValid())
{
Console.WriteLine($"\nDistribution histogram is valid!");
Console.WriteLine($"Total items: {distribution.TotalCount}");
Console.WriteLine($"Most common bucket: {distribution.Buckets.OrderByDescending(b => b.Count).First().BucketNumber}");
}
}

```


## TimeoutUtilitiesValidation


The `TimeoutUtilitiesValidation` class provides validation utilities for timeout-related parameters used with the `TimeoutUtilities` class. It validates timeout durations, minimum/maximum bounds, poll intervals, retry parameters, and operation names, returning detailed error messages through `Validate()` methods. This validation layer ensures that timeout configurations are valid and safe for use with timeout operations.

### Key Features

- Validates timeout durations with comprehensive range checking (0-24 hours, minimum 1ms)
- Validates timeout bounds (minimum and maximum constraints)
- Validates poll intervals ensuring they're smaller than timeout for effective polling
- Validates retry parameters including max retries (1-100) and initial delays
- Validates operation names (1-100 characters, not null/whitespace)
- Returns detailed error messages through `Validate()` methods for debugging
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Throws descriptive exceptions when validation fails with comprehensive error details

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Validate a simple timeout
        TimeSpan timeout = TimeSpan.FromSeconds(30);
        var timeoutErrors = TimeoutUtilitiesValidation.Validate(timeout);
        
        if (timeoutErrors.Count > 0)
        {
            Console.WriteLine("Timeout validation errors:");
            foreach (var error in timeoutErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("Timeout is valid!");
        }
        
        // Check if valid using convenience method
        bool isValid = TimeoutUtilitiesValidation.IsValid(timeout);
        Console.WriteLine($"Is timeout valid: {isValid}");
        
        // Validate timeout with bounds (minimum 1s, maximum 5s)
        var boundedErrors = TimeoutUtilitiesValidation.Validate(
            requestedTimeout: TimeSpan.FromSeconds(10),
            minimum: TimeSpan.FromSeconds(1),
            maximum: TimeSpan.FromSeconds(5)
        );
        
        if (boundedErrors.Count > 0)
        {
            Console.WriteLine("Bounded timeout validation failed:");
            foreach (var error in boundedErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        
        // Validate timeout with poll interval (poll interval must be < timeout)
        var pollErrors = TimeoutUtilitiesValidation.Validate(
            timeout: TimeSpan.FromSeconds(30),
            pollInterval: TimeSpan.FromSeconds(2)
        );
        
        Console.WriteLine($"Poll validation: {(pollErrors.Count == 0 ? "Valid" : string.Join(", ", pollErrors))}");
        
        // Validate retry parameters
        var retryErrors = TimeoutUtilitiesValidation.Validate(
            timeout: TimeSpan.FromSeconds(60),
            maxRetries: 5,
            initialDelay: TimeSpan.FromSeconds(1)
        );
        
        Console.WriteLine($"Retry validation: {(retryErrors.Count == 0 ? "Valid" : string.Join(", ", retryErrors))}");
        
        // Validate timeout with operation name
        var opErrors = TimeoutUtilitiesValidation.Validate(
            timeout: TimeSpan.FromSeconds(45),
            operationName: "ImageProcessing"
        );
        
        Console.WriteLine($"Operation validation: {(opErrors.Count == 0 ? "Valid" : string.Join(", ", opErrors))}");
        
        // Use EnsureValid() to throw exception on invalid timeout
        try
        {
            TimeoutUtilitiesValidation.EnsureValid(TimeSpan.FromSeconds(-1));
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"EnsureValid caught error: {ex.Message}");
        }
        
        // Use EnsureValid() with bounded timeout
        try
        {
            TimeoutUtilitiesValidation.EnsureValid(
                requestedTimeout: TimeSpan.FromSeconds(5),
                minimum: TimeSpan.FromSeconds(10), // This will fail
                maximum: TimeSpan.FromSeconds(20)
            );
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Bounded EnsureValid caught error: {ex.Message}");
        }
        
        // Practical example: Validate timeout before using TimeoutUtilities
        TimeSpan processingTimeout = TimeSpan.FromMinutes(2);
        if (TimeoutUtilitiesValidation.IsValid(processingTimeout))
        {
            Console.WriteLine("Processing timeout is valid, proceeding with operation...");
            
            // Simulate using the timeout with TimeoutUtilities
            try
            {
                await TimeoutUtilities.ExecuteWithTimeoutAsync(
                    action: () => Console.WriteLine("Processing image..."),
                    timeout: processingTimeout,
                    operationName: "ImageProcessing"
                );
                Console.WriteLine("Operation completed successfully!");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Operation timed out!");
            }
        }
    }
}

```

## BatchProcessingUtilitiesValidation

### Key Features
{
ProcessedCount = 45,
TotalCount = 100,
ErrorCount = 2,
PercentComplete = 45.0,
ItemsPerSecond = 12.5,
ElapsedTime = TimeSpan.FromSeconds(3600),
EstimatedTimeRemaining = TimeSpan.FromSeconds(4320),
CompletionTime = DateTime.UtcNow.AddMinutes(30)
};

// Validate the batch progress
var progressErrors = batchProgress.Validate();
Console.WriteLine($"\nBatchProgress validation: {(progressErrors.Count == 0 ? "Valid" : string.Join(", ", progressErrors))}");

// Check if valid using convenience method
bool isProgressValid = batchProgress.IsValid();
Console.WriteLine($"Is BatchProgress valid: {isProgressValid}");

// Create a valid ThrottleRecommendation instance
var throttleRecommendation = new ThrottleRecommendation
{
ShouldThrottle = true,
ThrottleLevel = 0.75f,
Reasons = new List<string> { "High GPU memory usage", "Multiple concurrent batch jobs" }
};

// Validate the throttle recommendation
var throttleErrors = throttleRecommendation.Validate();
Console.WriteLine($"\nThrottleRecommendation validation: {(throttleErrors.Count == 0 ? "Valid" : string.Join(", ", throttleErrors))}");

// Check if valid using convenience method
bool isThrottleValid = throttleRecommendation.IsValid();
Console.WriteLine($"Is ThrottleRecommendation valid: {isThrottleValid}");

// Use EnsureValid() to throw exception on invalid batch item
try
{
var invalidItem = new BatchItem<string>
{
SequenceNumber = -1, // Invalid - negative sequence number
Priority = 5,
Item = "/data/images/photo.jpg"
};
invalidItem.EnsureValid();
}
catch (ArgumentException ex)
{
Console.WriteLine($"\nEnsureValid caught error: {ex.Message}");
}

// Use EnsureValid() on batch progress
try
{
var invalidProgress = new BatchProgress
{
ProcessedCount = 150, // Invalid - exceeds total count
TotalCount = 100,
ErrorCount = 2,
PercentComplete = 150.0 // Invalid - exceeds 100%
};
invalidProgress.EnsureValid();
}
catch (ArgumentException ex)
{
Console.WriteLine($"EnsureValid on BatchProgress caught error: {ex.Message}");
}

// Use EnsureValid() on throttle recommendation
try
{
var invalidThrottle = new ThrottleRecommendation
{
ShouldThrottle = true,
ThrottleLevel = -0.5f, // Invalid - negative throttle level
Reasons = null
};
invalidThrottle.EnsureValid();
}
catch (ArgumentException ex)
{
Console.WriteLine($"EnsureValid on ThrottleRecommendation caught error: {ex.Message}");
}

// Example with actual batch processing workflow
Console.WriteLine("\n--- Batch Processing Workflow Example ---");
var batchItems = new List<BatchItem<string>>
{
new BatchItem<string> { SequenceNumber = 1, Priority = 3, Item = "/data/images/image1.jpg", ScheduledAt = DateTime.UtcNow },
new BatchItem<string> { SequenceNumber = 2, Priority = 1, Item = "/data/images/image2.jpg", ScheduledAt = DateTime.UtcNow.AddMinutes(-5) },
new BatchItem<string> { SequenceNumber = 3, Priority = 2, Item = "/data/images/image3.jpg", ScheduledAt = DateTime.UtcNow.AddMinutes(-10) }
};

// Validate all items before processing
foreach (var item in batchItems)
{
if (item.IsValid())
{
Console.WriteLine($"Valid batch item: Sequence {item.SequenceNumber}, Priority {item.Priority}, Item {Path.GetFileName(item.Item)}");
}
else
{
Console.WriteLine($"Invalid batch item: Sequence {item.SequenceNumber}");
}
}

// Calculate progress after processing some items
var progress = new BatchProgress
{
ProcessedCount = 75,
TotalCount = 100,
ErrorCount = 1,
PercentComplete = 75.0,
ItemsPerSecond = 25.0,
ElapsedTime = TimeSpan.FromSeconds(300)
};

if (progress.IsValid())
{
Console.WriteLine($"\nProcessing progress: {progress.PercentComplete}% complete");
Console.WriteLine($"Rate: {progress.ItemsPerSecond} items/second");
Console.WriteLine($"Time elapsed: {progress.ElapsedTime.TotalSeconds:F0} seconds");
}
}

}

```
## BatchProcessingServiceTestsExtensionsJsonExtensions

The `BatchProcessingServiceTestsExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for the `BatchProcessingServiceTestsExtensions` configuration in batch processing test scenarios. It enables serialization and deserialization of test configurations with support for customizable batch sizes, filter counts, and verbose output settings, making it easier to configure and share test scenarios.

### Key Features

- JSON serialization of test configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration state management for batch processing test execution
- Thread-safe serialization with optimized JsonSerializerOptions
- Configurable default image count, filter count, and verbose output settings

### Usage Examples

```csharp

using GpuImageProcessing.Tests.Services;
using System;
using System.Text.Json;

class Program
{

static void Main()
{

// Create test configuration with default settings
var config = new BatchProcessingServiceTestsExtensionsJsonExtensions.BatchProcessingServiceTestsExtensions
{

DefaultImageCount = 5,
DefaultFilterCount = 3,
EnableVerboseOutput = true

};

// Serialize to compact JSON
string compactJson = config.ToJson();
Console.WriteLine("Compact JSON configuration:");
Console.WriteLine(compactJson);

// Serialize to indented JSON for readability
string indentedJson = config.ToJson(indented: true);
Console.WriteLine("\nIndented JSON configuration:");
Console.WriteLine(indentedJson);

// Deserialize from JSON string
string json = @"{ "defaultImageCount": 10, "defaultFilterCount": 5, "enableVerboseOutput": false }";
var deserialized = BatchProcessingServiceTestsExtensionsJsonExtensions.FromJson(json);

if (deserialized != null)
{

Console.WriteLine($"\nDeserialized configuration:");
Console.WriteLine($"DefaultImageCount: {deserialized.DefaultImageCount}");
Console.WriteLine($"DefaultFilterCount: {deserialized.DefaultFilterCount}");
Console.WriteLine($"EnableVerboseOutput: {deserialized.EnableVerboseOutput}");
}

// Try to deserialize with error handling
string invalidJson = @"{ invalid json }";
bool success = BatchProcessingServiceTestsExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);
Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

// Serialize and deserialize round-trip to verify data integrity
string roundTripJson = config.ToJson();
var roundTripResult = BatchProcessingServiceTestsExtensionsJsonExtensions.FromJson(roundTripJson);
Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
}

}

```

## DataConversionUtilitiesValidation

The `DataConversionUtilitiesValidation` class provides comprehensive validation utilities for all data conversion operations in the `DataConversionUtilities` class. It validates method parameters, null safety, and value ranges before performing conversions, ensuring that data formats are valid and safe for processing. This validation layer helps prevent runtime errors and provides detailed error messages for debugging conversion scenarios.

### Key Features

- Validates all `DataConversionUtilities` conversion methods (BytesToHex, HexToBytes, FloatsToBytes, BytesToFloats, etc.)
- Validates null safety for method delegates and input parameters
- Validates value ranges (array lengths, hex string formats, file sizes, tolerances)
- Returns detailed error messages through `Validate()` methods for debugging
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Ensures proper relationships between input parameters and expected formats
- Throws descriptive exceptions when validation fails

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        // Validate BytesToHex conversion
        var hexValidation = DataConversionUtilitiesValidation.BytesToHex.Validate(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F });
        Console.WriteLine($"BytesToHex validation: {(hexValidation.Count == 0 ? "Valid" : string.Join(", ", hexValidation))}");

        // Validate HexToBytes conversion
        var hexToBytesValidation = DataConversionUtilitiesValidation.HexToBytes.Validate("48656C6C6F");
        Console.WriteLine($"HexToBytes validation: {(hexToBytesValidation.Count == 0 ? "Valid" : string.Join(", ", hexToBytesValidation))}");

        // Validate FloatsToBytes conversion
        var floatsToBytesValidation = DataConversionUtilitiesValidation.FloatsToBytes.Validate(new float[] { 1.5f, 2.7f, 3.14f });
        Console.WriteLine($"FloatsToBytes validation: {(floatsToBytesValidation.Count == 0 ? "Valid" : string.Join(", ", floatsToBytesValidation))}");

        // Validate BytesToFloats conversion
        var bytesToFloatsValidation = DataConversionUtilitiesValidation.BytesToFloats.Validate(new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x40, 0x29, 0x3E, 0xB8 });
        Console.WriteLine($"BytesToFloats validation: {(bytesToFloatsValidation.Count == 0 ? "Valid" : string.Join(", ", bytesToFloatsValidation))}");

        // Validate FormatFileSize conversion
        var formatFileSizeValidation = DataConversionUtilitiesValidation.FormatFileSize.Validate(1572864); // 1.5MB
        Console.WriteLine($"FormatFileSize validation: {(formatFileSizeValidation.Count == 0 ? "Valid" : string.Join(", ", formatFileSizeValidation))}");

        // Validate ParseFileSize conversion
        var parseFileSizeValidation = DataConversionUtilitiesValidation.ParseFileSize.Validate("2.5 GB");
        Console.WriteLine($"ParseFileSize validation: {(parseFileSizeValidation.Count == 0 ? "Valid" : string.Join(", ", parseFileSizeValidation))}");

        // Validate FormatTimeSpan conversion
        var formatTimeSpanValidation = DataConversionUtilitiesValidation.FormatTimeSpan.Validate(TimeSpan.FromSeconds(93784));
        Console.WriteLine($"FormatTimeSpan validation: {(formatTimeSpanValidation.Count == 0 ? "Valid" : string.Join(", ", formatTimeSpanValidation))}");

        // Validate ParseDuration conversion
        var parseDurationValidation = DataConversionUtilitiesValidation.ParseDuration.Validate("2h 30m 15s");
        Console.WriteLine($"ParseDuration validation: {(parseDurationValidation.Count == 0 ? "Valid" : string.Join(", ", parseDurationValidation))}");

        // Validate ToBinaryString conversion
        var toBinaryStringValidation = DataConversionUtilitiesValidation.ToBinaryString.Validate(42, 8);
        Console.WriteLine($"ToBinaryString validation: {(toBinaryStringValidation.Count == 0 ? "Valid" : string.Join(", ", toBinaryStringValidation))}");

        // Validate IsWithinTolerance comparison
        var toleranceValidation = DataConversionUtilitiesValidation.IsWithinTolerance.Validate(1.0001f, 1.0002f, 0.01f);
        Console.WriteLine($"IsWithinTolerance validation: {(toleranceValidation.Count == 0 ? "Valid" : string.Join(", ", toleranceValidation))}");

        // Validate Normalize conversion
        var normalizeValidation = DataConversionUtilitiesValidation.Normalize.Validate(127.5f, 0, 255);
        Console.WriteLine($"Normalize validation: {(normalizeValidation.Count == 0 ? "Valid" : string.Join(", ", normalizeValidation))}");

        // Validate Denormalize conversion
        var denormalizeValidation = DataConversionUtilitiesValidation.Denormalize.Validate(0.5f, 0, 255);
        Console.WriteLine($"Denormalize validation: {(denormalizeValidation.Count == 0 ? "Valid" : string.Join(", ", denormalizeValidation))}");

        // Use IsValid() convenience method
        bool isDataConversionValid = DataConversionUtilitiesValidation.IsValid();
        Console.WriteLine($"\nDataConversionUtilitiesValidation is valid: {isDataConversionValid}");

        // Example with actual conversion operations
        byte[] imageData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        
        // Validate before conversion
        var validationErrors = DataConversionUtilitiesValidation.BytesToHex.Validate(imageData);
        if (validationErrors.Count == 0)
        {
            string hexString = DataConversionUtilities.BytesToHex(imageData);
            Console.WriteLine($"\nSuccessfully validated and converted to hex: {hexString}");
        }
        else
        {
            Console.WriteLine("Validation failed:");
            foreach (var error in validationErrors)
            {
                Console.WriteLine($" - {error}");
            }
        }
    }
}

```

## DataConversionUtilities

### Key Features


- Convert between hexadecimal strings and byte arrays
- Transform between byte arrays and floating-point arrays
- Format and parse file sizes with human-readable units
- Format and parse time spans with human-readable strings
- Binary string representation of numeric values
- Tolerance-based floating-point comparison and normalization

### Usage Examples


```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Linq;

class Program

{
static void Main()
{
// Hexadecimal conversion examples
byte[] imageData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };

// Convert bytes to hex string
string hexString = DataConversionUtilities.BytesToHex(imageData);
Console.WriteLine($"Hex: {hexString}"); // Output: Hex: 48656C6C6F

// Convert hex string back to bytes
byte[] decodedBytes = DataConversionUtilities.HexToBytes(hexString);
Console.WriteLine($"Decoded bytes: {string.Join(", ", decodedBytes)}"); // Output: Decoded bytes: 72, 101, 108, 108, 111

// Floating-point conversion examples
float[] floatArray = new float[] { 1.5f, 2.7f, 3.14f, 42.0f };

// Convert floats to bytes (for GPU buffer operations)
byte[] floatBytes = DataConversionUtilities.FloatsToBytes(floatArray);
Console.WriteLine($"Float bytes length: {floatBytes.Length} bytes");

// Convert bytes back to floats
float[] decodedFloats = DataConversionUtilities.BytesToFloats(floatBytes);
Console.WriteLine($"Decoded floats: {string.Join(", ", decodedFloats.Select(f => f.ToString("F2")))}");

// File size formatting examples
string formattedSize = DataConversionUtilities.FormatFileSize(1572864); // 1.5MB
Console.WriteLine(formattedSize); // Output: 1.50 MB

long parsedSize = DataConversionUtilities.ParseFileSize("2.5 GB");
Console.WriteLine($"Parsed size: {parsedSize:N0} bytes"); // Output: Parsed size: 2,684,354,560 bytes

// Time span formatting examples
string formattedTime = DataConversionUtilities.FormatTimeSpan(TimeSpan.FromSeconds(93784));
Console.WriteLine(formattedTime); // Output: 1 day, 2 hours, 3 minutes, 4 seconds

TimeSpan parsedTime = DataConversionUtilities.ParseDuration("2h 30m 15s");
Console.WriteLine($"Parsed time: {parsedTime}"); // Output: Parsed time: 02:30:15

// Binary string representation
string binaryString = DataConversionUtilities.ToBinaryString(42);
Console.WriteLine($"Binary: {binaryString}"); // Output: Binary: 101010

// Tolerance-based operations
float value1 = 1.0001f;
float value2 = 1.0002f;
bool isSimilar = DataConversionUtilities.IsWithinTolerance(value1, value2, 0.01f);
Console.WriteLine($"Within tolerance: {isSimilar}"); // Output: Within tolerance: True

// Normalization for GPU operations (0-1 range)
float normalized = DataConversionUtilities.Normalize(127.5f, 0, 255);
Console.WriteLine($"Normalized: {normalized:F4}"); // Output: Normalized: 0.5000

float denormalized = DataConversionUtilities.Denormalize(0.5f, 0, 255);
Console.WriteLine($"Denormalized: {denormalized:F2}"); // Output: Denormalized: 127.50
}
}
```


## FilterServiceExtensions

The `FilterServiceExtensions` class provides extension methods for the `FilterService` that simplify common filter management operations. It includes convenient methods for creating standard filters (grayscale, blur, sharpen, convolution), managing filter activation states, and retrieving filters by type or name. These extensions help build flexible image processing pipelines with minimal boilerplate code.

### Key Features

- Create standard filters with sensible defaults (grayscale, blur, sharpen, convolution)
- Manage filter activation states (activate/deactivate filters)
- Retrieve filters by type or name with case-insensitive search
- Apply multiple filters to images in sequence
- Priority-based filter ordering for execution control

### Usage Examples

```csharp

using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Initialize the filter service (typically via dependency injection)
        var filterService = new FilterService(...);

        // Create standard filters with default parameters
        var grayscaleFilter = await filterService.CreateGrayscaleFilterAsync(
            name: "Convert to Grayscale",
            priority: 1
        );

        var blurFilter = await filterService.CreateBlurFilterAsync(
            name: "Gaussian Blur",
            radius: 3.5f,
            priority: 2
        );

        var sharpenFilter = await filterService.CreateSharpenFilterAsync(
            name: "Edge Enhance",
            strength: 4.2f,
            priority: 3
        );

        // Create a custom convolution filter (e.g., edge detection)
        var edgeDetectionKernel = new float[] { -1, -1, -1, -1, 8, -1, -1, -1, -1 };
        var convolutionFilter = await filterService.CreateConvolutionFilterAsync(
            name: "Edge Detection",
            kernel: edgeDetectionKernel,
            normalize: true,
            priority: 4
        );

        // Find a filter by name
        var foundFilter = await filterService.FindFilterByNameAsync("Gaussian Blur");
        Console.WriteLine(foundFilter != null 
            ? $"Found filter: {foundFilter.Name}"
            : "Filter not found");

        // Get all active filters sorted by priority
        var activeFilters = await filterService.GetActiveFiltersAsync();
        Console.WriteLine($"Active filters: {activeFilters.Count}");
        foreach (var filter in activeFilters)
        {
            Console.WriteLine($"  - {filter.Name} (Priority: {filter.Priority})");
        }

        // Get filters by type
        var blurFilters = await filterService.GetFiltersByTypeAsync(FilterType.Blur);
        Console.WriteLine($"Blur filters: {blurFilters.Count}");

        // Activate a filter
        var activatedFilter = await filterService.ActivateFilterAsync(blurFilter.Id);
        Console.WriteLine($"Activated: {activatedFilter.IsActive}");

        // Deactivate a filter
        bool wasDeactivated = await filterService.DeactivateFilterAsync(sharpenFilter.Id);
        Console.WriteLine($"Was deactivated: {wasDeactivated}");

        // Apply multiple filters to an image
        var image = new Image(...);
        await filterService.ApplyFiltersAsync(
            image,
            new[] { grayscaleFilter.Id, blurFilter.Id, sharpenFilter.Id }
        );
    }
}

```

## FilterChainBenchmarksExtensionsJsonExtensions

The `FilterChainBenchmarksExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for benchmark configuration and results in the FilterChainBenchmarksExtensions benchmark suite. It enables easy serialization and deserialization of benchmark configurations with support for validation, cloning, and parallel execution settings.

### Key Features

- JSON serialization of benchmark configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration validation through JSON parsing
- Thread-safe serialization with optimized JsonSerializerOptions

### Usage Examples

```csharp

using GpuImageProcessing.Benchmarks;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a benchmark configuration
        var config = new 
        {
            FilterCount = 15,
            EnableValidation = true,
            EnableCloning = true,
            MaxParallelSteps = 8
        };

        // Serialize to compact JSON
        string compactJson = config.ToJson();
        Console.WriteLine("Compact JSON:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON
        string indentedJson = config.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON
        string json = @"{ "filterCount": 20, "enableValidation": false, "enableCloning": true, "maxParallelSteps": 2 }";
        object? deserialized = FilterChainBenchmarksExtensionsJsonExtensions.FromJson(json);
        
        if (deserialized is not null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"FilterCount: {((dynamic)deserialized).FilterCount}");
            Console.WriteLine($"EnableValidation: {((dynamic)deserialized).EnableValidation}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = FilterChainBenchmarksExtensionsJsonExtensions.TryFromJson(invalidJson, out object? result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip
        string roundTripJson = config.ToJson();
        object? roundTripResult = FilterChainBenchmarksExtensionsJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine($"\nRound-trip successful: {roundTripResult is not null}");
    }
}

```

## EnumerableExtensions

The `EnumerableExtensions` class provides a comprehensive set of extension methods for working with `IEnumerable<T>` sequences. These utilities extend LINQ with additional functional programming helpers for filtering, grouping, batching, searching, and aggregation operations that are commonly needed in GPU image processing pipelines.

### Key Features


- Batch processing with fixed-size groups
- Null filtering and safe conversions
- Distinct operations by key selectors
- Min/max operations with custom key comparison
- Index-based operations (IndexOf, FindIndex)
- Dictionary conversion with duplicate key handling
- Consecutive grouping and batching
- Sequence manipulation (Repeat, TakeWhile, ForEach, Concat, Shuffle, Pad)
- Equality checking (AllEqual)

### Usage Examples


```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

class Program

{
    static void Main()

    {

        // Batch processing example
        var numbers = Enumerable.Range(1, 20);
        var batches = numbers.Batch(5);
        
        Console.WriteLine("Batches:");
        foreach (var batch in batches)
        {
            Console.WriteLine(string.Join(", ", batch));
        }
        
        // WhereNotNull example
        var nullableStrings = new List<string?> { "hello", null, "world", null, "test" };
        var nonNullStrings = nullableStrings.WhereNotNull();
        Console.WriteLine($"Non-null count: {nonNullStrings.Count()}"); // Output: 3
        
        // DistinctBy example
        var people = new List<Person> 
        {
            new Person("Alice", 30),
            new Person("Bob", 25),
            new Person("Charlie", 30),
            new Person("David", 25)
        };
        
        var uniqueAges = people.DistinctBy(p => p.Age);
        Console.WriteLine($"Unique ages: {string.Join(", ", uniqueAges.Select(p => p.Age))}"); // Output: 30, 25
        
        // MaxBy/MinBy examples
        var maxPerson = people.MaxBy(p => p.Age);
        var minPerson = people.MinBy(p => p.Age);
        Console.WriteLine($"Oldest: {maxPerson.Name}, Youngest: {minPerson.Name}");
        
        // IndexOf example
        var colors = new List<string> { "red", "green", "blue", "yellow" };
        var greenIndex = colors.IndexOf("green");
        Console.WriteLine($"Green index: {greenIndex}"); // Output: 1
        
        // FindIndex example
        var firstEvenIndex = numbers.FindIndex(n => n % 2 == 0);
        Console.WriteLine($"First even index: {firstEvenIndex}"); // Output: 1
        
        // SafeToDictionary example
        var keyValuePairs = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("one", 1),
            new KeyValuePair<string, int>("two", 2),
            new KeyValuePair<string, int>("one", 10)
        };
        
        var dictionary = keyValuePairs.SafeToDictionary(kv => kv.Key, kv => kv.Value);
        Console.WriteLine($"Dictionary count: {dictionary.Count}"); // Output: 2 (first occurrence kept)
        
        // GroupConsecutive example
        var data = new List<int> { 1, 1, 2, 2, 2, 3, 1, 1 };
        var groups = data.GroupConsecutive(x => x);
        Console.WriteLine("Consecutive groups:");
        foreach (var group in groups)
        {
            Console.WriteLine(string.Join(", ", group));
        }
        
        // Repeat example
        var sequence = new List<int> { 1, 2, 3 };
        var repeated = sequence.Repeat().Take(8); // First 8 items from infinite sequence
        Console.WriteLine("Repeated sequence: " + string.Join(", ", repeated));
        
        // TakeWhile example
        var numbers2 = Enumerable.Range(1, 10);
        var taken = numbers2.TakeWhile(n => n < 5);
        Console.WriteLine("Taken while < 5: " + string.Join(", ", taken));
        
        // ForEach example
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var processed = list.ForEach(x => Console.Write($"Processing {x}... "));
        Console.WriteLine("\nProcessed count: " + processed.Count());
        
        // Concat example
        var list1 = new List<int> { 1, 2, 3 };
        var list2 = new List<int> { 4, 5 };
        var concatenated = list1.Concat(list2);
        Console.WriteLine("Concatenated: " + string.Join(", ", concatenated));
        
        // AllEqual example
        var allSame = new List<int> { 5, 5, 5 };
        var allDifferent = new List<int> { 1, 2, 3 };
        Console.WriteLine($"All equal: {allSame.AllEqual()}, All different: {allDifferent.AllEqual()}");
        
        // Shuffle example
        var shuffled = list.Shuffle();
        Console.WriteLine("Shuffled: " + string.Join(", ", shuffled));
        
        // Pad example
        var shortList = new List<int> { 1, 2, 3 };
        var padded = shortList.Pad(7, 0);
        Console.WriteLine("Padded: " + string.Join(", ", padded));
    }
}

public class Person
{
    public string Name { get; }
    public int Age { get; }
    
    public Person(string name, int age)
    {

        Name = name;
        Age = age;
    }
}
```

## EnumerableExtensionsBenchmarksExtensionsJsonExtensions

The `EnumerableExtensionsBenchmarksExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for benchmark configuration and results in the EnumerableExtensionsBenchmarksExtensions benchmark suite. It enables easy serialization and deserialization of benchmark configurations with support for validation, batch size configuration, shuffle iterations, and parallel execution settings.

### Key Features

- JSON serialization of benchmark configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration validation through JSON parsing
- Thread-safe serialization with optimized JsonSerializerOptions
- Batch size configuration for performance testing
- Shuffle iterations control for randomized benchmark scenarios
- Validation toggle for result validation during benchmark execution

### Usage Examples

```csharp
using GpuImageProcessing.Benchmarks;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a benchmark configuration with custom settings
        var config = new EnumerableExtensionsBenchmarksExtensionsJsonExtensions.BenchmarkConfig
        {
            BatchSize = 1024,
            ShuffleIterations = 100,
            EnableValidation = true
        };

        // Serialize to compact JSON
        string compactJson = config.ToJson();
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = config.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{ "batchSize": 2048, "shuffleIterations": 50, "enableValidation": false }";
        object? deserialized = EnumerableExtensionsBenchmarksExtensionsJsonExtensions.FromJson(json);

        if (deserialized is EnumerableExtensionsBenchmarksExtensionsJsonExtensions.BenchmarkConfig benchmarkConfig)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"BatchSize: {benchmarkConfig.BatchSize}");
            Console.WriteLine($"ShuffleIterations: {benchmarkConfig.ShuffleIterations}");
            Console.WriteLine($"EnableValidation: {benchmarkConfig.EnableValidation}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = EnumerableExtensionsBenchmarksExtensionsJsonExtensions.TryFromJson(invalidJson, out object? result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = config.ToJson();
        object? roundTripResult = EnumerableExtensionsBenchmarksExtensionsJsonExtensions.FromJson(roundTripJson);

        if (roundTripResult is EnumerableExtensionsBenchmarksExtensionsJsonExtensions.BenchmarkConfig roundTripConfig)
        {
            Console.WriteLine($"\nRound-trip successful: {roundTripConfig.BatchSize == config.BatchSize && roundTripConfig.ShuffleIterations == config.ShuffleIterations}");
        }

        // Create configuration with all properties set
        var fullConfig = new EnumerableExtensionsBenchmarksExtensionsJsonExtensions.BenchmarkConfig
        {
            BatchSize = 4096,
            ShuffleIterations = 200,
            EnableValidation = true
        };

        Console.WriteLine($"\nFull configuration:");
        Console.WriteLine(fullConfig.ToJson());
    }
}

```

## EndToEndProcessingTestsExtensionsJsonExtensions

The `EndToEndProcessingTestsExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for the `EndToEndProcessingTestsExtensions` configuration in end-to-end testing scenarios. It enables serialization and deserialization of test execution configurations with support for enabling/disabling specific test operations, compact and indented JSON output formats, and safe error handling.

### Key Features

- JSON serialization of test configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration state management for end-to-end test execution
- Thread-safe serialization with optimized JsonSerializerOptions

### Usage Examples

```csharp
using GpuImageProcessing.Tests.Integration;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create test configuration with default settings
        var config = new EndToEndProcessingTestsExtensionsJsonExtensions.EndToEndProcessingTestsExtensions
        {
            IsRunAllTestsEnabled = true,
            IsGetTestMethodNamesEnabled = true,
            IsAllTestsPassEnabled = true
        };

        // Serialize to compact JSON
        string compactJson = config.ToJson();
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = config.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{ "isRunAllTestsEnabled": false, "isGetTestMethodNamesEnabled": true, "isAllTestsPassEnabled": true }";
        var deserialized = EndToEndProcessingTestsExtensionsJsonExtensions.FromJson(json);

        if (deserialized != null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"IsRunAllTestsEnabled: {deserialized.IsRunAllTestsEnabled}");
            Console.WriteLine($"IsGetTestMethodNamesEnabled: {deserialized.IsGetTestMethodNamesEnabled}");
            Console.WriteLine($"IsAllTestsPassEnabled: {deserialized.IsAllTestsPassEnabled}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = EndToEndProcessingTestsExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = config.ToJson();
        var roundTripResult = EndToEndProcessingTestsExtensionsJsonExtensions.FromJson(roundTripJson);
        Console.WriteLine($"\nRound-trip successful: {roundTripResult != null}");
    }
}

```


## Architecture

The GPU Image Processing library follows a modular architecture with clear separation of concerns:

### Core Components

- **ImageProcessingService**: Main service for image processing operations
- **ComputeShaderPipeline**: Manages GPU compute shader execution
- **ImageRepository**: Stores and retrieves images
- **FilterService**: Applies image filters
- **TransformService**: Applies image transformations
- **DeviceService**: Manages GPU devices and capabilities

### Data Flow

1. Image registration via `ImageProcessingService.RegisterImageAsync()`
2. Profile selection and configuration
3. GPU-accelerated processing using compute shaders
4. Result storage and quality metrics

### GPU Acceleration

- DirectML for DirectX 12 compute shaders
- Vulkan compute shaders for cross-platform support
- CUDA support for NVIDIA GPUs
- Automatic device selection and capability detection

## ImageProcessingService

The `ImageProcessingService` is the main entry point for image processing operations. It coordinates between repositories, services, and the GPU pipeline to process images efficiently.

### Usage Example

```csharp

using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program

{
    static async Task Main()
    {

        // Initialize required services (typically via dependency injection in real applications)
        var imageRepository = new ImageRepository();
        var filterRepository = new GenericRepository<Filter>();
        var transformRepository = new GenericRepository<Transform>();
        var profileRepository = new GenericRepository<ProcessingProfile>();
        var deviceService = new DeviceService();
        var computeShaderPipeline = new ComputeShaderPipeline(...);
        var logger = new ConsoleLogger<ImageProcessingService>();
        var filterService = new FilterService(...);
        var transformService = new TransformService(...);

        var processingService = new ImageProcessingService(
            imageRepository,
            filterRepository,
            transformRepository,
            profileRepository,
            deviceService,
            computeShaderPipeline,
            logger,
            filterService,
            transformService
        );

        // Register an image for processing
        var imageId = await processingService.RegisterImageAsync("/path/to/input.jpg", "vacation-photo");
        Console.WriteLine($"Registered image with ID: {imageId}");

        // Apply a processing profile
        var profile = await profileRepository.GetByIdAsync("default-profile");
        var result = await processingService.ProcessImageAsync(imageId, profile);
        
        Console.WriteLine($"Processing completed in {result.ProcessingTimeMs}ms");
        Console.WriteLine($"Output saved to: {result.OutputPath}");
    }
}
```


## ImageUtilities

The `ImageUtilities` class provides essential image manipulation utilities for format conversion, resizing, cropping, and basic operations. It serves as a convenience layer over the core image processing functionality.

### Usage Example

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.IO;

class Program

{
    static void Main()

    {

        // Load an image
        var imageBytes = File.ReadAllBytes("input.png");
        
        // Convert between formats
        var jpegBytes = ImageUtilities.ConvertFormat(imageBytes, ImageFormat.Jpeg);
        var pngBytes = ImageUtilities.ConvertFormat(imageBytes, ImageFormat.Png);
        
        File.WriteAllBytes("output.jpg", jpegBytes);
        File.WriteAllBytes("output.png", pngBytes);
        
        // Resize image
        var resized = ImageUtilities.ResizeImage(imageBytes, 800, 600);
        File.WriteAllBytes("resized.jpg", resized);
        
        // Crop image
        var cropped = ImageUtilities.CropImage(imageBytes, 100, 100, 400, 400);
        File.WriteAllBytes("cropped.jpg", cropped);
        
        // Get image info
        var info = ImageUtilities.GetImageInfo(imageBytes);
        Console.WriteLine($"Image: {info.Width}x{info.Height}, {info.Format}, {info.BitDepth} bits");
    }
}
```


## ImageProcessingController

The `ImageProcessingController` provides a REST API controller for image processing operations. It exposes endpoints for uploading images, applying filters, and downloading processed results.

### Usage Example

```csharp

using GpuImageProcessing.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ImageProcessingController : ControllerBase
{
    private readonly ImageProcessingService _processingService;

    public ImageProcessingController(ImageProcessingService processingService)
    {

        _processingService = processingService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {

        using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        
        var imageId = await _processingService.RegisterImageAsync(
            memoryStream.ToArray(),
            file.FileName
        );
        
        return Ok(new { ImageId = imageId });
    }

    [HttpPost("{imageId}/process")]
    public async Task<IActionResult> ProcessImage(string imageId, [FromBody] ProcessingRequest request)
    {

        var result = await _processingService.ProcessImageAsync(imageId, request.ProfileId);
        return Ok(new { ResultId = result.ResultId, OutputPath = result.OutputPath });
    }

    [HttpGet("{resultId}/download")]
    public async Task<IActionResult> DownloadResult(string resultId)
    {

        var fileBytes = await _processingService.GetResultAsync(resultId);
        return File(fileBytes, "application/octet-stream", "processed.jpg");
    }
}

public class ProcessingRequest
{
    public string ProfileId { get; set; }
}
```


## FileOperationUtilities

The `FileOperationUtilities` class provides safe file system operations with atomic writes, error handling, and cross-platform compatibility. It ensures reliable file operations even in the presence of errors or interruptions.

### Usage Example

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

class Program

{
    static async Task Main()
    {

        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        string testFile = Path.Combine(tempDir, "test.txt");
        string content = "This is a test file for FileOperationUtilities";

        // Atomic write example
        await FileOperationUtilities.WriteFileAtomicAsync(testFile, content);
        Console.WriteLine("File written atomically");

        // Read the file
        var readContent = await FileOperationUtilities.ReadFileAtomicAsync(testFile);
        Console.WriteLine($"Read content: {readContent}");

        // Safe file deletion
        await FileOperationUtilities.SafeDeleteFileAsync(testFile);
    }
}
```


## MarkdownResultFormatter

The `MarkdownResultFormatter` class formats GPU image processing results, device information, job status, and errors into well-structured Markdown documents. It generates comprehensive reports with tables, statistics, and formatted output suitable for documentation, logging, and reporting purposes.

### Key Features


- Formats processing results into structured Markdown reports
- Generates summary statistics and performance metrics
- Creates detailed tables for individual operations
- Formats device information and job status
- Handles error reporting with proper Markdown escaping
- Supports batch processing with operation breakdowns

### Usage Examples


```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        var formatter = new MarkdownResultFormatter();
        
        // Format a single processing result
        var singleResult = new ProcessingResult
        {
            Id = "result-001",
            ImageId = "image-001",
            IsSuccessful = true,
            ProcessingTimeMs = 150.5f,
            OutputFileSizeBytes = 1024 * 1024, // 1MB
            AppliedFilters = new List<string> { "GaussianBlur", "EdgeDetection" },
            AppliedTransforms = new List<string> { "Resize" }
        };
        
        string singleResultMarkdown = formatter.FormatResult(singleResult);
        Console.WriteLine(singleResultMarkdown);
        
        // Format multiple processing results
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = "result-001",
                ImageId = "image-001",
                IsSuccessful = true,
                ProcessingTimeMs = 150.5f,
                OutputFileSizeBytes = 1024 * 1024,
                AppliedFilters = new List<string> { "GaussianBlur" }
            },
            new ProcessingResult
            {
                Id = "result-002",
                ImageId = "image-002",
                IsSuccessful = false,
                ProcessingTimeMs = 250.0f,
                OutputFileSizeBytes = 512 * 1024,
                ErrorMessage = "Invalid image format"
            },
            new ProcessingResult
            {
                Id = "result-003",
                ImageId = "image-003",
                IsSuccessful = true,
                ProcessingTimeMs = 120.25f,
                OutputFileSizeBytes = 2 * 1024 * 1024,
                AppliedTransforms = new List<string> { "Crop", "Rotate" }
            }
        };
        
        string multipleResultsMarkdown = formatter.FormatResults(results);
        Console.WriteLine(multipleResultsMarkdown);
        
        // Format device information
        var device = new DeviceInfo
        {
            Name = "NVIDIA RTX 3090",
            Vendor = "NVIDIA",
            DeviceType = "GPU",
            IsAvailable = true,
            GlobalMemoryBytes = 24L * 1024 * 1024 * 1024 // 24GB
        };
        
        string deviceMarkdown = formatter.FormatDevice(device);
        Console.WriteLine(deviceMarkdown);
        
        // Format a processing job
        var job = new ProcessingJob
        {
            Name = "Batch Image Processing",
            Status = "Completed",
            ProcessedImages = 45,
            TotalImages = 50,
            ProgressPercentage = 90.0f
        };
        
        string jobMarkdown = formatter.FormatJob(job);
        Console.WriteLine(jobMarkdown);
        
        // Format an error
        string errorMarkdown = formatter.FormatError(
            "Failed to load image: Invalid file format",
            "IMG-ERR-001"
        );
        Console.WriteLine(errorMarkdown);
        
        // Get file extension and MIME type
        Console.WriteLine($"File extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
```


## HtmlResultFormatter

The `HtmlResultFormatter` class formats GPU image processing results, device information, job status, and errors into interactive HTML documents with embedded CSS styling. It generates comprehensive web-ready reports with tables, statistics, and responsive design suitable for web display, dashboards, and reporting purposes.

### Key Features


- Formats processing results into structured HTML reports with embedded CSS
- Generates interactive tables with hover effects and status indicators
- Creates responsive statistics cards with visual indicators
- Formats device information and job status with styled HTML elements
- Handles error reporting with formatted HTML output
- Provides file extension and MIME type information for HTML output

### Usage Examples


```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        var formatter = new HtmlResultFormatter();

        // Format a single processing result
        var singleResult = new ProcessingResult
        {
            Id = "result-001",
            ImageId = "image-001",
            IsSuccessful = true,
            ProcessingTimeMs = 150.5f,
            OutputFileSizeBytes = 1024 * 1024, // 1MB
            AppliedFilters = new List<string> { "GaussianBlur", "EdgeDetection" },
            AppliedTransforms = new List<string> { "Resize" }
        };

        string singleResultHtml = formatter.FormatResult(singleResult);
        Console.WriteLine(singleResultHtml);

        // Format multiple processing results
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = "result-001",
                ImageId = "image-001",
                IsSuccessful = true,
                ProcessingTimeMs = 150.5f,
                OutputFileSizeBytes = 1024 * 1024,
                AppliedFilters = new List<string> { "GaussianBlur" }
            },
            new ProcessingResult
            {
                Id = "result-002",
                ImageId = "image-002",
                IsSuccessful = false,
                ProcessingTimeMs = 250.0f,
                OutputFileSizeBytes = 512 * 1024,
                ErrorMessage = "Invalid image format"
            },
            new ProcessingResult
            {
                Id = "result-003",
                ImageId = "image-003",
                IsSuccessful = true,
                ProcessingTimeMs = 120.25f,
                OutputFileSizeBytes = 2 * 1024 * 1024,
                AppliedTransforms = new List<string> { "Crop", "Rotate" }
            }
        };

        string multipleResultsHtml = formatter.FormatResults(results);
        Console.WriteLine(multipleResultsHtml);

        // Format a processing job
        var job = new ProcessingJob
        {
            Name = "Batch Image Processing Job",
            Status = "Completed",
            ProcessedImages = 45,
            TotalImages = 50,
            ProgressPercentage = 90.0f
        };

        string jobHtml = formatter.FormatJob(job);
        Console.WriteLine(jobHtml);

        // Format device information
        var device = new DeviceInfo
        {
            Name = "NVIDIA RTX 3090 Ti",
            Vendor = "NVIDIA",
            DeviceType = "GPU",
            IsAvailable = true,
            GlobalMemoryBytes = 24L * 1024 * 1024 * 1024 // 24GB
        };

        string deviceHtml = formatter.FormatDevice(device);
        Console.WriteLine(deviceHtml);

        // Format an error
        string errorHtml = formatter.FormatError(
            "Failed to initialize compute shader pipeline",
            "GPU-001",
            new InvalidOperationException("Device not available or driver error")
        );
        Console.WriteLine(errorHtml);

        // Get file extension and MIME type
        Console.WriteLine($"File extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
```


## JsonResultFormatter

The `JsonResultFormatter` class provides JSON serialization utilities for GPU image processing results, device information, job status, and errors. It formats data into structured JSON output with configurable formatting options, camelCase property naming, and proper null handling.

### Key Features


- Formats processing results, jobs, devices, and errors to JSON
- Supports pretty-printing with configurable indentation
- Uses camelCase property naming for JSON output
- Handles null values gracefully with default serialization options
- Provides file extension and MIME type information

### Usage Examples


```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        // Create formatter with pretty printing enabled (default)
        var formatter = new JsonResultFormatter();

        // Format a single processing result
        var result = new ProcessingResult
        {
            Id = "result-001",
            JobId = "job-001",
            ImageId = "image-001",
            Status = ProcessingStatus.Completed,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            CompletionTime = DateTime.UtcNow,
            OutputImagePath = "/output/processed-001.jpg",
            ProcessedSize = 1024 * 1024,
            Metadata = new Dictionary<string, object> { { "format", "JPEG" }, { "quality", 95 } }
        };

        string resultJson = formatter.FormatResult(result);
        Console.WriteLine("Single result:");
        Console.WriteLine(resultJson);

        // Format multiple processing results
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = "result-001",
                JobId = "job-001",
                ImageId = "image-001",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                CompletionTime = DateTime.UtcNow.AddMinutes(-4),
                OutputImagePath = "/output/processed-001.jpg",
                ProcessedSize = 1024 * 1024
            },
            new ProcessingResult
            {
                Id = "result-002",
                JobId = "job-001",
                ImageId = "image-002",
                Status = ProcessingStatus.Failed,
                StartTime = DateTime.UtcNow.AddMinutes(-3),
                CompletionTime = DateTime.UtcNow.AddMinutes(-2),
                ErrorMessage = "Invalid image format: unsupported color space"
            },
            new ProcessingResult
            {
                Id = "result-003",
                JobId = "job-001",
                ImageId = "image-003",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-2),
                CompletionTime = DateTime.UtcNow.AddMinutes(-1),
                OutputImagePath = "/output/processed-003.png",
                ProcessedSize = 2 * 1024 * 1024
            }
        };

        string resultsJson = formatter.FormatResults(results);
        Console.WriteLine("\nBatch results:");
        Console.WriteLine(resultsJson);

        // Format a processing job
        var job = new ProcessingJob
        {
            Id = "job-001",
            Name = "Batch Image Processing Job",
            Status = "Completed",
            TotalImages = 150,
            ProcessedImages = 148,
            FailedImages = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(2),
            CompletedAt = DateTime.UtcNow,
            Filters = new List<string> { "GaussianBlur", "EdgeDetection" },
            Transforms = new List<string> { "Resize", "Crop" }
        };

        string jobJson = formatter.FormatJob(job);
        Console.WriteLine("\nJob information:");
        Console.WriteLine(jobJson);

        // Format device information
        var device = new DeviceInfo
        {
            Id = "device-001",
            Name = "NVIDIA RTX 3090 Ti",
            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024,
            ComputeUnits = 10496,
            IsAvailable = true,
            DriverVersion = "535.86.05",
            Extensions = new List<string> { "DirectML", "CUDA", "Vulkan" }
        };

        string deviceJson = formatter.FormatDevice(device);
        Console.WriteLine("\nDevice information:");
        Console.WriteLine(deviceJson);

        // Format an error
        string errorJson = formatter.FormatError(
            "Failed to initialize compute shader pipeline",
            "GPU-001",
            new InvalidOperationException("Device not available or driver error")
        );
        Console.WriteLine("\nError information:");
        Console.WriteLine(errorJson);

        // Get file extension and MIME type
        Console.WriteLine($"\nFile extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
```


## TextResultFormatter

The `TextResultFormatter` class formats GPU image processing results, device information, job status, and errors into plain text format. It generates human-readable console output and log files with consistent formatting and clear separation between different types of information.

### Key Features


- Formats processing results into structured text reports with clear section headers
- Generates summary statistics and performance metrics in plain text
- Creates detailed tables for individual operations and batch processing
- Formats device information and job status with consistent separators
- Handles error reporting with timestamps and exception details
- Provides file extension and MIME type information for text output

### Usage Examples


```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        var formatter = new TextResultFormatter();

        // Format a single processing result
        var singleResult = new ProcessingResult
        {
            Id = "result-001",
            JobId = "job-001",
            ImageId = "image-001",
            Status = ProcessingStatus.Completed,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            CompletionTime = DateTime.UtcNow,
            OutputImagePath = "/output/processed-001.jpg",
            ProcessedSize = 1024 * 1024,
            Metadata = new Dictionary<string, object> { { "format", "JPEG" }, { "quality", 95 } }
        };

        string singleResultText = formatter.FormatResult(singleResult);
        Console.WriteLine(singleResultText);

        // Format multiple processing results
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = "result-001",
                JobId = "job-001",
                ImageId = "image-001",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                CompletionTime = DateTime.UtcNow.AddMinutes(-4),
                OutputImagePath = "/output/processed-001.jpg",
                ProcessedSize = 1024 * 1024
            },
            new ProcessingResult
            {
                Id = "result-002",
                JobId = "job-001",
                ImageId = "image-002",
                Status = ProcessingStatus.Failed,
                StartTime = DateTime.UtcNow.AddMinutes(-3),
                CompletionTime = DateTime.UtcNow.AddMinutes(-2),
                ErrorMessage = "Invalid image format: unsupported color space"
            },
            new ProcessingResult
            {
                Id = "result-003",
                JobId = "job-001",
                ImageId = "image-003",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-2),
                CompletionTime = DateTime.UtcNow.AddMinutes(-1),
                OutputImagePath = "/output/processed-003.png",
                ProcessedSize = 2 * 1024 * 1024
            }
        };

        string multipleResultsText = formatter.FormatResults(results);
        Console.WriteLine(multipleResultsText);

        // Format a processing job
        var job = new ProcessingJob
        {
            Id = "job-001",
            Name = "Batch Image Processing Job",
            Status = "Completed",
            TotalImages = 150,
            ProcessedImages = 148,
            FailedImages = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(2),
            CompletedAt = DateTime.UtcNow,
            Filters = new List<string> { "GaussianBlur", "EdgeDetection" },
            Transforms = new List<string> { "Resize", "Crop" }
        };

        string jobText = formatter.FormatJob(job);
        Console.WriteLine(jobText);

        // Format device information
        var device = new DeviceInfo
        {
            Id = "device-001",
            Name = "NVIDIA RTX 3090 Ti",
            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024,
            ComputeUnits = 10496,
            IsAvailable = true,
            DriverVersion = "535.86.05",
            Extensions = new List<string> { "DirectML", "CUDA", "Vulkan" }
        };

        string deviceText = formatter.FormatDevice(device);
        Console.WriteLine(deviceText);

        // Format an error
        string errorText = formatter.FormatError(
            "Failed to initialize compute shader pipeline",
            "GPU-001",
            new InvalidOperationException("Device not available or driver error")
        );
        Console.WriteLine(errorText);

        // Get file extension and MIME type
        Console.WriteLine($"\nFile extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
```


## PathUtilities

The `PathUtilities` class provides a comprehensive set of utilities for path manipulation, normalization, and directory management. It handles cross-platform path operations, safe file operations, and directory traversal with robust error handling to ensure reliable file system operations across different operating systems.

## GpuManagementServiceValidation

The `GpuManagementServiceValidation` class provides validation utilities for `GpuManagementService` instances to ensure GPU device availability, memory allocation, and service state are valid before performing GPU-accelerated image processing operations. It validates device properties, memory constraints, compute unit availability, and service configuration, returning detailed error messages when validation fails.

### Key Features

- Validates `GpuManagementService` instances with comprehensive device and service state checks
- Validates `GpuDevice` instances with detailed property validation (memory, compute units, availability)
- Validates device allocation requests with memory and compute unit requirements
- Returns detailed error messages through `Validate()` methods for debugging and error handling
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation patterns
- Includes allocation validation with `ValidateDeviceAllocation()`, `CanAllocate()`, and `EnsureCanAllocate()` methods

### Usage Examples

```csharp

using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Initialize GPU management service (typically via dependency injection)
        var gpuService = new GpuManagementService();
        
        // Validate the service state
        var serviceErrors = gpuService.Validate();
        Console.WriteLine($"Service validation has {serviceErrors.Count} errors");
        
        // Check if service is valid using convenience method
        bool isServiceValid = gpuService.IsValid();
        Console.WriteLine($"Service is valid: {isServiceValid}");
        
        // Get available devices
        var devices = gpuService.GetAvailableDevices();
        Console.WriteLine($"Found {devices.Count} available devices");
        
        // Validate each device
        foreach (var device in devices)
        {
            var deviceErrors = device.Validate();
            if (deviceErrors.Count == 0)
            {
                Console.WriteLine($"Device {device.Name} is valid");
            }
            else
            {
                Console.WriteLine($"Device {device.Name} has {deviceErrors.Count} validation errors:");
                foreach (var error in deviceErrors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }
        
        // Validate device allocation for a specific task
        var allocationErrors = gpuService.ValidateDeviceAllocation(
            deviceId: devices[0].Id,
            requiredMemory: 1024 * 1024 * 1024, // 1GB
            requiredComputeUnits: 8
        );
        
        if (allocationErrors.Count == 0)
        {
            Console.WriteLine("Device allocation is valid - GPU resources are sufficient");
        }
        else
        {
            Console.WriteLine("Device allocation validation failed:");
            foreach (var error in allocationErrors)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        
        // Use CanAllocate() for conditional logic
        bool canAllocate = gpuService.CanAllocate(
            deviceId: devices[0].Id,
            requiredMemory: 512 * 1024 * 1024, // 512MB
            requiredComputeUnits: 4
        );
        Console.WriteLine($"Can allocate resources: {canAllocate}");
        
        // Use EnsureValid() to throw exception on invalid service
        try
        {
            gpuService.EnsureValid();
            Console.WriteLine("Service passed validation");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Service validation failed: {ex.Message}");
        }
        
        // Use EnsureCanAllocate() to throw exception on allocation failure
        try
        {
            gpuService.EnsureCanAllocate(
                deviceId: devices[0].Id,
                requiredMemory: 2L * 1024 * 1024 * 1024, // 2GB
                requiredComputeUnits: 16
            );
            Console.WriteLine("Device allocation passed validation");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Device allocation failed: {ex.Message}");
        }
    }
}

```

## ComputeShaderPassValidation

The `ComputeShaderPassValidation` class provides validation utilities for `ComputeShaderPass` instances to ensure all required properties are properly initialized before GPU execution. It validates critical properties such as kernel names, workgroup configurations, input/output images, and parameter dictionaries, returning detailed error messages when validation fails.

### Key Features

- Validates `ComputeShaderPass` instances with comprehensive property checks
- Returns detailed error messages for invalid configurations
- Includes validation for `WorkgroupConfiguration` with size and memory constraints
- Provides convenience methods like `IsValid()` and `EnsureValid()` for fluent validation
- Validates timestamps, priorities, and parameter dictionaries

### Usage Examples

```csharp

using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Create a valid compute shader pass
        var validPass = new ComputeShaderPass
        {
            Id = Guid.NewGuid(),
            KernelName = "BlurKernel",
            KernelSource = "// shader code...",
            PassType = ShaderPassType.ImageProcessing,
            Priority = 1,
            WorkgroupConfiguration = new WorkgroupConfiguration
            {
                WorkgroupSizeX = 8,
                WorkgroupSizeY = 8,
                WorkgroupSizeZ = 1,
                GlobalWorkSizeX = 1920,
                GlobalWorkSizeY = 1080,
                GlobalWorkSizeZ = 1,
                LocalMemoryRequiredBytes = 1024,
                EstimatedOccupancy = 0.85f,
                OptimizationScore = 95,
                ComputedAt = DateTime.UtcNow
            },
            Parameters = new Dictionary<string, object> { { "sigma", 2.5f } },
            InputImages = new List<ImageReference> { new ImageReference("input-001") },
            OutputImage = new ImageReference("output-001"),
            CreatedAt = DateTime.UtcNow
        };

        // Validate the pass
        var validationErrors = ComputeShaderPassValidation.Validate(validPass);
        Console.WriteLine($"Valid pass has {validationErrors.Count} errors"); // Output: 0

        // Check if valid using convenience method
        bool isValid = ComputeShaderPassValidation.IsValid(validPass);
        Console.WriteLine("Is valid: {isValid}"); // Output: Is valid: True

        // Create an invalid pass (missing required fields)
        var invalidPass = new ComputeShaderPass
        {
            Id = Guid.Empty, // Invalid - empty Guid
            KernelName = "", // Invalid - empty string
            // Missing: KernelSource, WorkgroupConfiguration, Parameters, InputImages, OutputImage
            Priority = -1, // Invalid - negative priority
            CreatedAt = DateTime.UtcNow.AddDays(1) // Invalid - future date
        };

        // Validate and get detailed errors
        var errors = ComputeShaderPassValidation.Validate(invalidPass);
        Console.WriteLine("Validation errors:");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error}");
        }
        /* Output:
        - Id must not be empty.
        - KernelName must not be null or whitespace.
        - KernelSource must not be null.
        - Priority must not be negative.
        - WorkgroupConfiguration must be set for execution.
        - Parameters dictionary should not be empty for most use cases.
        - InputImages must contain at least one image.
        - OutputImage must be set before execution.
        - CreatedAt appears to be in the future.
        */

        // Use EnsureValid() to throw exception on invalid pass
        try
        {
            ComputeShaderPassValidation.EnsureValid(invalidPass);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }

        // Validate WorkgroupConfiguration separately
        var workgroupConfig = new WorkgroupConfiguration
        {
            WorkgroupSizeX = 0, // Invalid - must be positive
            WorkgroupSizeY = 8,
            WorkgroupSizeZ = 1,
            GlobalWorkSizeX = 1920,
            GlobalWorkSizeY = 1080,
            GlobalWorkSizeZ = 1,
            LocalMemoryRequiredBytes = -100, // Invalid - negative memory
            EstimatedOccupancy = 1.5f, // Invalid - > 1.0
            OptimizationScore = 150, // Invalid - > 100
            ComputedAt = DateTime.UtcNow.AddDays(-1)
        };

        var configErrors = ComputeShaderPassValidation.Validate(workgroupConfig);
        Console.WriteLine($"Workgroup configuration has {configErrors.Count} errors");
        /* Output: Workgroup configuration has 5 errors */

        // Use EnsureValid() on workgroup configuration
        try
        {
            ComputeShaderPassValidation.EnsureValid(workgroupConfig);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Workgroup validation failed: {ex.Message}");
        }
    }
}

```

## ValidationUtilities

The `ValidationUtilities` class provides comprehensive validation utilities for image processing parameters and configurations. It includes validation methods for filter parameters, image dimensions, rotation angles, scaling factors, batch jobs, processing profiles, device IDs, and string parameters, ensuring all inputs meet expected criteria before processing.

### Key Features


- Validates filter parameters for different filter types (Gaussian, Sobel, Median, Canny, Bilateral)
- Validates image dimensions, rotation angles, and scaling factors
- Validates batch processing jobs and processing profiles
- Checks for safe file paths and valid device IDs
- Validates string parameters with configurable length constraints
- Provides detailed validation results with error messages

### Usage Examples


```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        // Validate image dimensions
        var dimensionsResult = ValidationUtilities.ValidateImageDimensions(1920, 1080);
        if (!dimensionsResult.IsValid)
        {
            Console.WriteLine($"Invalid dimensions: {dimensionsResult.ErrorMessage}");
        }
        else
        {
            Console.WriteLine("Image dimensions are valid");
        }

        // Validate rotation angle
        var rotationResult = ValidationUtilities.ValidateRotationAngle(45.5f);
        Console.WriteLine($"Rotation angle validation: {(rotationResult.IsValid ? "Valid" : rotationResult.ErrorMessage)}");

        // Validate scale factor
        var scaleResult = ValidationUtilities.ValidateScaleFactor(1.5);
        Console.WriteLine($"Scale factor validation: {(scaleResult.IsValid ? "Valid" : scaleResult.ErrorMessage)}");

        // Validate filter parameters
        var filterParams = new Dictionary<string, object> { { "sigma", 2.5f } };
        var filterResult = ValidationUtilities.ValidateFilterParameters(FilterType.Gaussian, filterParams);
        Console.WriteLine($"Filter validation: {(filterResult.IsValid ? "Valid" : filterResult.ErrorMessage)}");

        // Validate batch job
        var batchResult = ValidationUtilities.ValidateBatchJob(100, 5, 16);
        Console.WriteLine($"Batch job validation: {(batchResult.IsValid ? "Valid" : batchResult.ErrorMessage)}");

        // Validate processing profile
        var profileResult = ValidationUtilities.ValidateProcessingProfile(8, 32, 60);
        Console.WriteLine($"Processing profile validation: {(profileResult.IsValid ? "Valid" : profileResult.ErrorMessage)}");

        // Check safe file path
        bool isSafe = ValidationUtilities.IsSafeFilePath("/data/images/output.jpg");
        Console.WriteLine($"File path is safe: {isSafe}");

        // Validate device ID
        var deviceResult = ValidationUtilities.ValidateDeviceId(1, 4);
        Console.WriteLine($"Device ID validation: {(deviceResult.IsValid ? "Valid" : deviceResult.ErrorMessage)}");

        // Validate string parameter
        var stringResult = ValidationUtilities.ValidateStringParameter("profile-name", "Profile name", minLength: 3, maxLength: 50);
        Console.WriteLine($"String validation: {(stringResult.IsValid ? "Valid" : stringResult.ErrorMessage)}");

        // Create custom validation result and add errors
        var customResult = ValidationUtilities.ValidationResult.Success();
        if (!customResult.IsValid)
        {
            Console.WriteLine("Custom validation failed");
        }
        
        // Add multiple errors to a validation result
        var errorResult = ValidationUtilities.ValidationResult.Failure("Initial error");
        errorResult.AddError("Second error occurred");
        errorResult.AddError("Third error detected");
        Console.WriteLine($"Multiple errors: {errorResult.ErrorMessage}");
        Console.WriteLine($"Error count: {errorResult.Errors.Count}");
    }
}
```


## ConfigurationValidator


The `ConfigurationValidator` class provides comprehensive validation utilities for application configuration. It ensures that configuration values meet expected criteria before they are used at runtime, including required keys, value ranges, timeouts, batch sizes, memory specifications, URLs, and environment variables. The validator supports both individual validation checks and bulk validation of all configuration values.

### Usage Example

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        // Example configuration dictionary
        var config = new Dictionary<string, string>
        {
            {"MaxBatchSize", "128"},
            {"TimeoutSeconds", "30"},
            {"MaxRetries", "3"},
            {"LogLevel", "Info"},
            {"ApiEndpoint", "https://api.example.com/process"},
            {"MemoryLimit", "4GB"},
            {"InputDirectory", "/data/input"}
        };

        // Validate required configuration keys
        var requiredValidation = ConfigurationValidator.ValidateConfiguration(
            config,
            "MaxBatchSize", "TimeoutSeconds", "ApiEndpoint"
        );
        
        if (!requiredValidation.IsValid)
        {
            Console.WriteLine($"Configuration error: {requiredValidation.Message}");
            return;
        }

        // Validate integer range for batch size
        var batchValidation = ConfigurationValidator.ValidateIntegerRange(
            config["MaxBatchSize"],
            minimum: 1,
            maximum: 10000,
            parameterName: "MaxBatchSize"
        );
        
        if (!batchValidation.IsValid)
        {
            Console.WriteLine($"Batch size error: {batchValidation.Message}");
            return;
        }

        // Validate timeout duration
        var timeout = TimeSpan.FromSeconds(int.Parse(config["TimeoutSeconds"]));
        var timeoutValidation = ConfigurationValidator.ValidateTimeout(
            timeout,
            minimum: TimeSpan.FromSeconds(1),
            maximum: TimeSpan.FromSeconds(3600),
            parameterName: "TimeoutSeconds"
        );
        
        if (!timeoutValidation.IsValid)
        {
            Console.WriteLine($"Timeout error: {timeoutValidation.Message}");
            return;
        }

        // Validate batch size
        var batchSize = ConfigurationValidator.GetConfigurationValue<int>(
            config,
            "MaxBatchSize",
            defaultValue: 32
        );
        var batchSizeValidation = ConfigurationValidator.ValidateBatchSize(batchSize);
        
        if (!batchSizeValidation.IsValid)
        {
            Console.WriteLine($"Batch size error: {batchSizeValidation.Message}");
            return;
        }

        // Validate memory size
        var memoryValidation = ConfigurationValidator.ValidateMemorySize(
            config["MemoryLimit"],
            minimumBytes: 1024 * 1024 * 1024 // 1GB minimum
        );
        
        if (!memoryValidation.IsValid)
        {
            Console.WriteLine($"Memory error: {memoryValidation.Message}");
            return;
        }

        // Validate URL
        var urlValidation = ConfigurationValidator.ValidateUrl(config["ApiEndpoint"]);
        
        if (!urlValidation.IsValid)
        {
            Console.WriteLine($"URL error: {urlValidation.Message}");
            return;
        }

        // Validate environment variable
        var envValidation = ConfigurationValidator.ValidateEnvironmentVariable(
            "GPU_DEVICE_ID",
            required: false
        );
        
        if (!envValidation.IsValid)
        {
            Console.WriteLine($"Environment error: {envValidation.Message}");
            return;
        }

        // Get configuration value with fallback
        var logLevel = ConfigurationValidator.GetConfigurationValue<string>(
            config,
            "LogLevel",
            defaultValue: "Info"
        );
        Console.WriteLine($"Using log level: {logLevel}");

        // Validate all configuration at once
        var allErrors = ConfigurationValidator.ValidateAllConfiguration(config);
        if (allErrors.Count > 0)
        {
            Console.WriteLine("Configuration validation errors:");
            foreach (var error in allErrors)
            {
                Console.WriteLine($"  {error.Key}: {error.Message}");
            }
            return;
        }

        Console.WriteLine("Configuration validation successful!");
    }
}
```


            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024,
            ComputeUnits = 10496,
            IsAvailable = true,
            DriverVersion = "535.86.05",
            Extensions = new List<string> { "DirectML", "CUDA", "Vulkan" }
        };

        string deviceXml = formatter.FormatDevice(device);
        Console.WriteLine(deviceXml);

        // Format an error
        string errorXml = formatter.FormatError(
            "Failed to initialize compute shader pipeline",
            "GPU-001",
            new InvalidOperationException("Device not available or driver error")
        );
        Console.WriteLine(errorXml);

        // Get file extension and MIME type
        Console.WriteLine($"\nFile extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
## ImageProcessingExtensionsValidation

The `ImageProcessingExtensionsValidation` class provides extension methods for validating image processing types in the GPU image processing library. It validates critical properties of `ImageFormat`, `FilterType`, `Image`, and `string` (file extension) instances, returning detailed error messages when validation fails and providing convenience methods for fluent validation patterns.

### Key Features

- Validates `ImageFormat` values to ensure they are not `Unknown`
- Validates `FilterType` values to ensure they are not `None`
- Validates `Image` instances with comprehensive property checks (dimensions, channels, file size, paths, format, timestamps)
- Validates file extension strings to ensure they are properly formatted
- Provides `IsValid()` convenience methods for quick validation checks
- Provides `EnsureValid()` methods that throw exceptions when validation fails
- Returns detailed error messages through `Validate()` methods for debugging and error handling

### Usage Examples

```csharp
using GpuImageProcessing.Utilities;
using GpuImageProcessing.Domain;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Validate ImageFormat
        var format = ImageFormat.Jpeg;
        var formatErrors = format.Validate();
        Console.WriteLine($"ImageFormat validation has {formatErrors.Count} errors");
        
        // Check if format is valid using convenience method
        bool isFormatValid = format.IsValid();
        Console.WriteLine($"ImageFormat is valid: {isFormatValid}");
        
        // Use EnsureValid() to throw exception on invalid format
        try
        {
            ImageFormat.Unknown.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Format validation failed: {ex.Message}");
        }

        // Validate FilterType
        var filterType = FilterType.GaussianBlur;
        var filterErrors = filterType.Validate();
        Console.WriteLine($"\nFilterType validation has {filterErrors.Count} errors");
        
        // Check if filter type is valid
        bool isFilterValid = filterType.IsValid();
        Console.WriteLine($"FilterType is valid: {isFilterValid}");
        
        // Use EnsureValid() on filter type
        try
        {
            FilterType.None.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"FilterType validation failed: {ex.Message}");
        }

        // Validate Image instance
        var image = new Image
        {
            Width = 1920,
            Height = 1080,
            Channels = 3,
            FileSizeBytes = 1024 * 1024,
            FilePath = "/data/images/photo.jpg",
            Format = ImageFormat.Jpeg,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ModifiedAt = DateTime.UtcNow
        };
        
        var imageErrors = image.Validate();
        Console.WriteLine($"\nImage validation has {imageErrors.Count} errors");
        
        // Check if image is valid
        bool isImageValid = image.IsValid();
        Console.WriteLine($"Image is valid: {isImageValid}");
        
        // Use EnsureValid() on image
        try
        {
            var invalidImage = new Image
            {
                Width = -100, // Invalid
                Height = 0,   // Invalid
                Channels = 0, // Invalid
                FileSizeBytes = -1, // Invalid
                FilePath = "",
                Format = ImageFormat.Unknown,
                CreatedAt = default,
                ModifiedAt = default
            };
            invalidImage.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Image validation failed: {ex.Message}");
        }

        // Validate file extension
        string extension = ".jpg";
        var extensionErrors = extension.Validate();
        Console.WriteLine($"\nFile extension validation has {extensionErrors.Count} errors");
        
        // Check if extension is valid
        bool isExtensionValid = extension.IsValid();
        Console.WriteLine($"File extension is valid: {isExtensionValid}");
        
        // Use EnsureValid() on file extension
        try
        {
            string invalidExtension = "jpg"; // Missing leading dot
            invalidExtension.EnsureValid();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Extension validation failed: {ex.Message}");
        }
    }
}
```
## ComputeShaderPipelineOptionsExtensions

The `ComputeShaderPipelineOptionsExtensions` class provides extension methods for the `ComputeShaderPipelineOptions` type that simplify common configuration scenarios. It includes methods for cloning options, applying development/testing settings, applying production settings, getting clamped local memory values, and converting options to a dictionary for serialization or logging purposes.

### Key Features

- Clone existing options with `Clone()` for creating modified copies
- Apply development settings with profiling enabled using `WithDevelopmentSettings()`
- Apply production settings with performance optimizations using `WithProductionSettings()`
- Get safe local memory values with `GetClampedLocalMemoryPerThreadBytes()`
- Convert options to dictionary with `ToDictionary()` for serialization

### Usage Examples

```csharp

using GpuImageProcessing.Configuration;
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Create default options
        var options = new ComputeShaderPipelineOptions
        {
            DefaultStrategy = ShaderOptimizationStrategy.Aggressive,
            MaxWorkgroupDimension = 64,
            BenchmarkGuidedOptimization = false,
            EnableProfiling = false,
            MaxPipelineDepth = 8,
            DefaultLocalMemoryPerThreadBytes = 256,
            OccupancyWarningThreshold = 0.3
        };

        // Clone options for modification
        var clonedOptions = options.Clone();
        clonedOptions.MaxWorkgroupDimension = 128;

        // Apply development settings (enables profiling and benchmarking)
        var devOptions = options.Clone().WithDevelopmentSettings(enableBenchmarking: true);
        Console.WriteLine($"Development - Profiling: {devOptions.EnableProfiling}, Benchmarking: {devOptions.BenchmarkGuidedOptimization}");

        // Apply production settings (optimizes for performance)
        var prodOptions = options.WithProductionSettings(maxWorkgroupDimension: 256);
        Console.WriteLine($"Production - Workgroup: {prodOptions.MaxWorkgroupDimension}, Profiling: {prodOptions.EnableProfiling}");

        // Get clamped local memory value
        int clampedMemory = options.GetClampedLocalMemoryPerThreadBytes();
        Console.WriteLine($"Clamped memory: {clampedMemory} bytes");

        // Convert to dictionary for logging/serialization
        Dictionary<string, string> optionsDict = options.ToDictionary();
        foreach (var kvp in optionsDict)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
}

```

## ImageProcessingConfiguration

The `ImageProcessingConfiguration` record serves as a data transfer object for configuring image processing operations. It defines constraints for image dimensions and thresholds for performance monitoring, allowing applications to control processing behavior and detect slow operations.

### Key Features

- Configures minimum and maximum image dimensions (width and height) for validation
- Sets slow operation threshold to identify performance bottlenecks
- Provides JSON serialization/deserialization for configuration persistence
- Uses immutable record pattern with init-only properties for thread safety

### Usage Examples

```csharp

using GpuImageProcessing.Utilities;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Create a configuration with default values
        var defaultConfig = new ImageProcessingConfiguration();
        Console.WriteLine($"Default config - Min: {defaultConfig.MinImageWidth}x{defaultConfig.MinImageHeight}, " +
                        $"Max: {defaultConfig.MaxImageWidth}x{defaultConfig.MaxImageHeight}, " +
                        $"Slow threshold: {defaultConfig.SlowOperationThresholdMs}ms");
        
        // Create a custom configuration for high-resolution processing
        var hdConfig = new ImageProcessingConfiguration
        {
            MinImageWidth = 64,
            MinImageHeight = 64,
            MaxImageWidth = 16384,
            MaxImageHeight = 16384,
            SlowOperationThresholdMs = 10000 // 10 seconds
        };
        
        Console.WriteLine($"HD config - Min: {hdConfig.MinImageWidth}x{hdConfig.MinImageHeight}, " +
                        $"Max: {hdConfig.MaxImageWidth}x{hdConfig.MaxImageHeight}");
        
        // Serialize configuration to JSON
        string json = hdConfig.ToJson();
        Console.WriteLine("\nSerialized JSON:");
        Console.WriteLine(json);
        
        // Deserialize from JSON
        var deserializedConfig = ImageProcessingExtensionsJsonExtensions.FromJson(json);
        Console.WriteLine($"\nDeserialized - Min: {deserializedConfig?.MinImageWidth}x{deserializedConfig?.MinImageHeight}");
        
        // Try to deserialize with error handling
        string invalidJson = "{ invalid json }";
        bool success = ImageProcessingExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");
        
        // Save configuration to file
        string configPath = "image-processing-config.json";
        File.WriteAllText(configPath, hdConfig.ToJson(indented: true));
        Console.WriteLine($"\nConfiguration saved to: {configPath}");
        
        // Load configuration from file
        string loadedJson = File.ReadAllText(configPath);
        var loadedConfig = ImageProcessingExtensionsJsonExtensions.FromJson(loadedJson);
        Console.WriteLine($"Loaded config - Slow threshold: {loadedConfig?.SlowOperationThresholdMs}ms");
    }
}

```

## BatchItemResult

The `BatchItemResult` record represents the outcome for a single file processed during a batch directory operation. It captures the input file path, optional output file path, success status, and any error message that occurred during processing. This type is returned by `DirectoryBatchProcessor.ProcessDirectoryAsync` as part of the `BatchRunSummary.Items` collection, allowing callers to inspect individual file results and handle failures appropriately.

### Usage Example

```csharp

using GpuImageProcessing.Batch;
using GpuImageProcessing.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create a batch processor with CPU image processor (works without GPU)
        var processor = new CpuImageProcessor();
        var batchProcessor = new DirectoryBatchProcessor(processor);
        
        // Process a directory of images with a Gaussian blur filter
        var progress = new Progress<BatchProgress>(report => 
        {
            Console.WriteLine($"Processing: {report.CurrentFile} ({report.Completed}/{report.Total})");
        });
        
        var summary = await batchProcessor.ProcessDirectoryAsync(
            inputDir: "/data/input-images",
            outputDir: "/data/output-images",
            filterType: FilterType.GaussianBlur,
            progress: progress,
            parameters: new Dictionary<string, object> { { "sigma", 2.5f } }
        );
        
        // Inspect individual file results
        Console.WriteLine($"\nBatch completed in {summary.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"Total files: {summary.Total}");
        Console.WriteLine($"Succeeded: {summary.Succeeded}");
        Console.WriteLine($"Failed: {summary.Failed}");
        
        foreach (var result in summary.Items)
        {
            if (result.Success)
            {
                Console.WriteLine($"✓ {Path.GetFileName(result.InputPath)} -> {Path.GetFileName(result.OutputPath)}");
            }
            else
            {
                Console.WriteLine($"✗ {Path.GetFileName(result.InputPath)}: {result.Error}");
            }
        }
    }
}

```

## DirectoryBatchProcessorTests

The `DirectoryBatchProcessorTests` class provides comprehensive unit tests for the `DirectoryBatchProcessor` class, which implements the directory batch-processing pipeline for the `batch-dir` CLI subcommand. These tests verify that the processor correctly handles supported and unsupported file formats, reports progress accurately, produces byte-identical results compared to direct filter application, and handles edge cases like empty directories, missing input directories, and cancellation requests. The test suite uses temporary directories and the CPU fallback processor to ensure it runs without requiring a GPU.

### Key Features

- Tests processing of all supported file formats with automatic output generation
- Validates that unsupported file extensions are properly skipped
- Verifies progress reporting for every file in processing order
- Ensures output is byte-identical to direct filter application
- Tests handling of empty directories (yields zero total)
- Validates proper exception throwing for missing input directories
- Tests cancellation behavior to ensure operations can be interrupted

### Usage Examples

```csharp

using GpuImageProcessing.Batch;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Fallback;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
static async Task Main()
{
// Create a CPU-based image processor (works without GPU)
var processor = new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance);
var batchProcessor = new DirectoryBatchProcessor(processor);

// Create test input/output directories
string inputDir = Path.Combine(Path.GetTempPath(), "batch-input-" + Guid.NewGuid().ToString("N"));
string outputDir = Path.Combine(Path.GetTempPath(), "batch-output-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(inputDir);
Directory.CreateDirectory(outputDir);

try
{
// Create test image files (PPM format is supported)
File.WriteAllText(Path.Combine(inputDir, "test1.ppm"), CreateTestPpm());
File.WriteAllText(Path.Combine(inputDir, "test2.ppm"), CreateTestPpm());
File.WriteAllText(Path.Combine(inputDir, "unsupported.txt"), "This should be skipped");

// Process directory with progress reporting
var progressReports = new System.Collections.Generic.List<BatchProgress>();
var progress = new Progress<BatchProgress>(report => progressReports.Add(report));

// Process all supported files with grayscale filter
var summary = await batchProcessor.ProcessDirectoryAsync(
inputDir: inputDir,
outputDir: outputDir,
filterType: FilterType.Grayscale,
progress: progress
);

Console.WriteLine($"Processing completed!");
Console.WriteLine($"Total files: {summary.Total}");
Console.WriteLine($"Succeeded: {summary.Succeeded}");
Console.WriteLine($"Failed: {summary.Failed}");
Console.WriteLine($"Elapsed: {summary.Elapsed.TotalSeconds:F2}s");

// Verify progress was reported for each file
Console.WriteLine($"\nProgress reports received: {progressReports.Count}");
foreach (var report in progressReports)
{
Console.WriteLine($"  File {report.CurrentFile}: {report.Completed}/{report.Total}");
}

// Check output files were created
foreach (var file in Directory.GetFiles(outputDir))
{
Console.WriteLine($"Output created: {Path.GetFileName(file)}");
}
}
finally
{
// Cleanup
Directory.Delete(inputDir, recursive: true);
Directory.Delete(outputDir, recursive: true);
}
}

private static string CreateTestPpm()
{
// Create a simple 3x3 RGB PPM image
return @"P6
3 3
255
" + new string('A', 27); // 3x3x3 bytes = 27 bytes
}
}

```

## CsvResultFormatter

The `CsvResultFormatter` class formats GPU image processing results, device information, job status, errors, and statistics into CSV (comma-separated values) format. It generates structured tabular data that is ideal for importing into spreadsheet applications, data analysis tools, and reporting systems.

### Key Features

- Formats processing results into CSV with proper escaping and quoting
- Generates device information reports with memory calculations
- Creates job summary tables with completion percentages
- Handles error reporting in CSV format
- Provides statistics summaries in key-value pairs
- Supports both single result and batch processing scenarios

### Usage Examples

```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var formatter = new CsvResultFormatter();

        // Format a single processing result
        var result = new ProcessingResult
        {
            Id = "result-001",
            JobId = "job-001",
            ImageId = "image-001",
            Status = ProcessingStatus.Completed,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            CompletionTime = DateTime.UtcNow,
            OutputImagePath = "/output/processed-001.jpg",
            ProcessedSize = 1024 * 1024 // 1MB
        };

        string resultCsv = formatter.FormatResult(result);
        Console.WriteLine(resultCsv);

        // Format multiple processing results
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = "result-001",
                JobId = "job-001",
                ImageId = "image-001",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                CompletionTime = DateTime.UtcNow.AddMinutes(-4),
                OutputImagePath = "/output/processed-001.jpg",
                ProcessedSize = 1024 * 1024
            },
            new ProcessingResult
            {
                Id = "result-002",
                JobId = "job-001",
                ImageId = "image-002",
                Status = ProcessingStatus.Failed,
                StartTime = DateTime.UtcNow.AddMinutes(-3),
                CompletionTime = DateTime.UtcNow.AddMinutes(-2),
                ErrorMessage = "Invalid image format: unsupported color space"
            },
            new ProcessingResult
            {
                Id = "result-003",
                JobId = "job-001",
                ImageId = "image-003",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-2),
                CompletionTime = DateTime.UtcNow.AddMinutes(-1),
                OutputImagePath = "/output/processed-003.png",
                ProcessedSize = 2 * 1024 * 1024
            }
        };

        string resultsCsv = formatter.FormatResults(results);
        Console.WriteLine(resultsCsv);

        // Format a processing job
        var job = new ProcessingJob
        {
            Id = "job-001",
            Name = "Batch Image Processing Job",
            Status = "Completed",
            TotalImages = 150,
            ProcessedImages = 148,
            FailedImages = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(2),
            CompletedAt = DateTime.UtcNow
        };

        string jobCsv = formatter.FormatJob(job);
        Console.WriteLine(jobCsv);

        // Format device information
        var device = new DeviceInfo
        {
            Id = "device-001",
            Name = "NVIDIA RTX 3090 Ti",
            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024, // 24GB
            ComputeUnits = 10496,
            IsAvailable = true,
            DriverVersion = "535.86.05"
        };

        string deviceCsv = formatter.FormatDevice(device);
        Console.WriteLine(deviceCsv);

        // Format an error
        string errorCsv = formatter.FormatError(
            "Failed to initialize compute shader pipeline",
            "GPU-001",
            new InvalidOperationException("Device not available or driver error")
        );
        Console.WriteLine(errorCsv);

        // Format statistics
        var stats = new Dictionary<string, object>
        {
            {"TotalImages", 150},
            {"ProcessedImages", 148},
            {"FailedImages", 2},
            {"AverageProcessingTimeMs", 150.5},
            {"TotalProcessingTimeMs", 22575.0}
        };

        string statsCsv = formatter.FormatStatistics(stats);
        Console.WriteLine(statsCsv);

        // Get file extension and MIME type
        Console.WriteLine($"\nFile extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
```

## XmlResultFormatter

The `XmlResultFormatter` class formats GPU image processing results, device information, job status, and errors into structured XML documents. It generates comprehensive reports with proper XML schema, attributes, and nested elements suitable for machine parsing, configuration files, and data exchange purposes.


### Key Features


- Formats processing results into well-structured XML with proper attributes and nesting
- Generates XML documents with standardized schema for interoperability
- Creates device information reports with memory calculations and extensions
- Handles error reporting with XML-formatted error elements
- Provides file extension and MIME type information for XML output
- Supports both single result and batch processing scenarios

### Usage Examples


```csharp

using GpuImageProcessing.Formatters;
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

class Program

{
    static void Main()

    {

        var formatter = new XmlResultFormatter();

        // Format a single processing result
        var result = new ProcessingResult
        {
            Id = "result-001",
            JobId = "job-001",
            ImageId = "image-001",
            Status = ProcessingStatus.Completed,
            StartTime = DateTime.UtcNow.AddMinutes(-5),
            CompletionTime = DateTime.UtcNow,
            OutputImagePath = "/output/processed-001.jpg",
            ProcessedSize = 1024 * 1024,
            Metadata = new Dictionary<string, object> { { "format", "JPEG" }, { "quality", 95 } }
        };

        string resultXml = formatter.FormatResult(result);
        Console.WriteLine(resultXml);

        // Format multiple processing results
        var results = new List<ProcessingResult>
        {
            new ProcessingResult
            {
                Id = "result-001",
                JobId = "job-001",
                ImageId = "image-001",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                CompletionTime = DateTime.UtcNow.AddMinutes(-4),
                OutputImagePath = "/output/processed-001.jpg",
                ProcessedSize = 1024 * 1024
            },
            new ProcessingResult
            {
                Id = "result-002",
                JobId = "job-001",
                ImageId = "image-002",
                Status = ProcessingStatus.Failed,
                StartTime = DateTime.UtcNow.AddMinutes(-3),
                CompletionTime = DateTime.UtcNow.AddMinutes(-2),
                ErrorMessage = "Invalid image format: unsupported color space"
            },
            new ProcessingResult
            {
                Id = "result-003",
                JobId = "job-001",
                ImageId = "image-003",
                Status = ProcessingStatus.Completed,
                StartTime = DateTime.UtcNow.AddMinutes(-2),
                CompletionTime = DateTime.UtcNow.AddMinutes(-1),
                OutputImagePath = "/output/processed-003.png",
                ProcessedSize = 2 * 1024 * 1024
            }
        };

        string resultsXml = formatter.FormatResults(results);
        Console.WriteLine(resultsXml);

        // Format a processing job
        var job = new ProcessingJob
        {
            Id = "job-001",
            Name = "Batch Image Processing Job",
            Status = "Completed",
            TotalImages = 150,
            ProcessedImages = 148,
            FailedImages = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(2),
            CompletedAt = DateTime.UtcNow,
            Filters = new List<string> { "GaussianBlur", "EdgeDetection" },
            Transforms = new List<string> { "Resize", "Crop" }
        };

        string jobXml = formatter.FormatJob(job);
        Console.WriteLine(jobXml);

        // Format device information
        var device = new DeviceInfo
        {
            Id = "device-001",
            Name = "NVIDIA RTX 3090 Ti",
            Type = "GPU",
            Vendor = "NVIDIA",
            MemoryBytes = 24L * 1024 * 1024 * 1024,
            ComputeUnits = 10496,
            IsAvailable = true,
            DriverVersion = "535.86.05",
            Extensions = new List<string> { "DirectML", "CUDA", "Vulkan" }
        };

        string deviceXml = formatter.FormatDevice(device);
        Console.WriteLine(deviceXml);

        // Format an error
        string errorXml = formatter.FormatError(
            "Failed to initialize compute shader pipeline",
            "GPU-001",
            new InvalidOperationException("Device not available or driver error")
        );
        Console.WriteLine(errorXml);

        // Get file extension and MIME type
        Console.WriteLine($"\nFile extension: {formatter.GetFileExtension()}");
        Console.WriteLine($"MIME type: {formatter.GetMimeType()}");
    }
}
```


## GpuExceptionExtensionsJsonExtensions

The `GpuExceptionExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for GPU exception configuration in the GpuImageProcessing.Core library. It enables easy serialization and deserialization of GPU exception detection settings with support for timeout detection, memory detection, and compute pipeline detection configurations.

### Key Features

- JSON serialization of GPU exception configuration using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration validation through JSON parsing
- Thread-safe serialization with optimized JsonSerializerOptions
- Timeout, memory, and compute pipeline detection configuration

### Usage Examples

```csharp

using GpuImageProcessing.Core;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a GPU exception configuration with custom settings
        var config = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig
        {
            IsTimeoutDetectionEnabled = true,
            IsMemoryDetectionEnabled = true,
            IsComputePipelineDetectionEnabled = false
        };

        // Serialize to compact JSON
        string compactJson = GpuExceptionExtensionsJsonExtensions.ToJson(config);
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = GpuExceptionExtensionsJsonExtensions.ToJson(config, indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{ "type": "GpuExceptionExtensions", "isTimeoutDetectionEnabled": false, "isMemoryDetectionEnabled": true, "isComputePipelineDetectionEnabled": true }";
        GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig? deserialized = GpuExceptionExtensionsJsonExtensions.FromJson(json);

        if (deserialized is not null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"Type: {deserialized.Type}");
            Console.WriteLine($"IsTimeoutDetectionEnabled: {deserialized.IsTimeoutDetectionEnabled}");
            Console.WriteLine($"IsMemoryDetectionEnabled: {deserialized.IsMemoryDetectionEnabled}");
            Console.WriteLine($"IsComputePipelineDetectionEnabled: {deserialized.IsComputePipelineDetectionEnabled}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = GpuExceptionExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = GpuExceptionExtensionsJsonExtensions.ToJson(config);
        GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig? roundTripResult = GpuExceptionExtensionsJsonExtensions.FromJson(roundTripJson);

        if (roundTripResult is not null)
        {
            Console.WriteLine($"\nRound-trip successful: {roundTripResult.Type == config.Type && roundTripResult.IsTimeoutDetectionEnabled == config.IsTimeoutDetectionEnabled}");
        }

        // Create configuration with all properties set
        var fullConfig = new GpuExceptionExtensionsJsonExtensions.GpuExceptionExtensionsConfig
        {
            IsTimeoutDetectionEnabled = false,
            IsMemoryDetectionEnabled = true,
            IsComputePipelineDetectionEnabled = true
        };

        Console.WriteLine($"\nFull configuration:");
        Console.WriteLine(GpuExceptionExtensionsJsonExtensions.ToJson(fullConfig));
    }
}

```

## FilterConfigurationExtensions

The `FilterConfigurationExtensions` class provides extension methods for `FilterConfiguration` that simplify common operations like parameter management, configuration copying, and filter-specific helpers. It offers type-safe parameter access, configuration cloning, convolution kernel analysis, and normalized value retrieval for building flexible image processing pipelines.

### Key Features

- Type-safe parameter access with default value fallback
- Parameter setting with automatic type inference
- Configuration cloning with new unique identifiers
- Convolution kernel size analysis for custom convolution filters
- Filter type detection (convolution vs other filters)
- Normalized parameter value retrieval (0-1 range)
- Null safety and argument validation

### Usage Examples

```csharp

using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Create a filter configuration for a Gaussian blur
        var filterConfig = new FilterConfiguration
        {
            Name = "Gaussian Blur",
            FilterType = FilterType.GaussianBlur,
            Parameters = new Dictionary<string, object> { { "sigma", 2.5f } }
        };

        // Get a parameter with type safety and fallback
        float sigma = filterConfig.GetParameter("sigma", 1.0f);
        Console.WriteLine($"Gaussian sigma: {sigma:F2}"); // Output: Gaussian sigma: 2.50

        // Set a parameter with automatic type inference
        filterConfig.SetParameter("intensity", 0.8f);
        filterConfig.SetParameter("iterations", 3);
        Console.WriteLine($"Parameters set: {filterConfig.Parameters.Count}"); // Output: Parameters set: 3

        // Clone the configuration with a new ID
        var clonedConfig = filterConfig.WithNewId();
        Console.WriteLine($"Original ID: {filterConfig.Id}");
        Console.WriteLine($"Cloned ID: {clonedConfig.Id}");
        Console.WriteLine($"IDs are different: {filterConfig.Id != clonedConfig.Id}"); // Output: true

        // Check if it's a convolution filter
        bool isConvolution = filterConfig.IsConvolutionFilter();
        Console.WriteLine($"Is convolution filter: {isConvolution}"); // Output: Is convolution filter: True

        // Create a custom convolution filter
        var customConvolution = new FilterConfiguration
        {
            Name = "Edge Detection",
            FilterType = FilterType.CustomConvolution,
            ConvolutionKernel = new float[] { -1, -1, -1, -1, 8, -1, -1, -1, -1 }
        };

        // Get the convolution kernel size
        int? kernelSize = customConvolution.GetConvolutionKernelSize();
        Console.WriteLine($"Convolution kernel size: {kernelSize}"); // Output: Convolution kernel size: 3

        // Get a normalized parameter value (0-1 range)
        float normalized = filterConfig.GetNormalizedParameter("sigma", 0.5f);
        Console.WriteLine($"Normalized sigma: {normalized:F4}"); // Output: Normalized sigma: 0.8000

        // Use with default value when parameter doesn't exist
        float missingParam = filterConfig.GetNormalizedParameter("nonexistent", 0.0f);
        Console.WriteLine($"Missing parameter default: {missingParam}"); // Output: Missing parameter default: 0.00
    }
}

```

## DeviceUtilities

The `DeviceUtilities` class provides utilities for GPU device discovery, capability detection, and resource management. It helps identify optimal GPU devices for image processing workloads by scoring devices based on memory, compute units, clock frequency, and current utilization. The class also analyzes memory pressure and recommends appropriate batch sizes to prevent out-of-memory errors.

### Key Features


- Device scoring based on memory, compute units, and clock frequency
- Peak performance calculation in GFLOPS
- Memory pressure analysis and recommendations
- Compute capability validation
- Bandwidth utilization estimation
- Bottleneck identification

### Usage Examples


```csharp

using GpuImageProcessing.Utilities;
using System;

class Program

{
    static void Main()

    {

        // Score a GPU device for suitability
        float score = DeviceUtilities.ScoreGpuDevice(
            globalMemoryBytes: 16L * 1024 * 1024 * 1024, // 16GB
            computeUnits: 3840,
            maxClockFrequency: 2500,
            utilizationPercent: 45.5f
        );
        Console.WriteLine($"Device score: {score:F1}/100");

        // Calculate theoretical peak performance
        float peakPerformance = DeviceUtilities.CalculatePeakPerformance(
            computeUnits: 3840,
            clockFrequencyMhz: 2500,
            computePerCore: 64
        );
        Console.WriteLine($"Peak performance: {peakPerformance:F1} GFLOPS");

        // Analyze memory pressure
        var memoryAnalysis = DeviceUtilities.AnalyzeMemoryPressure(
            totalMemoryBytes: 16L * 1024 * 1024 * 1024,
            usedMemoryBytes: 12L * 1024 * 1024 * 1024,
            minFreeMemoryBytes: 100 * 1024 * 1024
        );
        
        Console.WriteLine($"Memory usage: {memoryAnalysis.UsagePercent:F1}%");
        Console.WriteLine($"Pressure level: {memoryAnalysis.PressureLevel}");
        Console.WriteLine($"Recommended batch size: {memoryAnalysis.RecommendedBatchSize}");
        
        // Check compute capability
        bool supportsCC8_6 = DeviceUtilities.SupportsComputeCapability("8.6", "8.6");
        Console.WriteLine($"Supports CC 8.6: {supportsCC8_6}");
        
        // Validate memory sufficiency
        bool hasEnoughMemory = DeviceUtilities.ValidateMemorySufficiency(
            availableMemoryBytes: 16L * 1024 * 1024 * 1024,
            requiredMemoryBytes: 4L * 1024 * 1024 * 1024,
            safetyMargin: 0.1f
        );
        Console.WriteLine($"Memory sufficient: {hasEnoughMemory}");
        
        // Generate device capability summary
        string summary = DeviceUtilities.GenerateCapabilitySummary(
            deviceName: "NVIDIA RTX 3090",
            globalMemoryBytes: 24L * 1024 * 1024 * 1024,
            computeUnits: 10496,
            maxClockFrequency: 1700,
            computeCapability: "8.6"
        );
        Console.WriteLine(summary);
        
        // Calculate recommended batch size
        int recommendedBatchSize = DeviceUtilities.CalculateRecommendedBatchSize(
            availableMemoryBytes: 24L * 1024 * 1024 * 1024
        );
        Console.WriteLine($"Recommended batch size: {recommendedBatchSize}");
        
        // Identify bottlenecks
        var bottlenecks = DeviceUtilities.IdentifyBottlenecks(
            computeUtilization: 85.2f,
            memoryBandwidthUtilization: 68.7f,
            memoryUtilization: 82.3f
        );
        
        Console.WriteLine("Bottlenecks:");
        foreach (var bottleneck in bottlenecks)
        {
            Console.WriteLine($"- {bottleneck}");
        }
    }
}
```




## GpuDeviceExtensionsJsonExtensions

The `GpuDeviceExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for GPU device extension configurations. It enables easy serialization and deserialization of device extension settings with support for JSON formatting options and error handling.

### Key Features

- JSON serialization of GPU device extension configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration validation through JSON parsing
- Thread-safe serialization with optimized JsonSerializerOptions
- Boolean flags for memory extensions, color space detection, device type display, and default memory unit configuration

### Usage Examples

```csharp

using GpuImageProcessing.Domain;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a GPU device extensions configuration with custom settings
        var config = new GpuDeviceExtensionsJsonExtensions.GpuDeviceExtensionsConfig
        {
            IsMemoryExtensionsEnabled = true,
            IsColorSpaceDetectionEnabled = false,
            IsDeviceTypeDisplayEnabled = true,
            DefaultMemoryUnit = "GB"
        };

        // Serialize to compact JSON
        string compactJson = config.ToJson();
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);
        
        // Serialize to indented JSON for readability
        string indentedJson = config.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{ "type": "GpuDeviceExtensions", "isMemoryExtensionsEnabled": false, "isColorSpaceDetectionEnabled": true, "isDeviceTypeDisplayEnabled": false, "defaultMemoryUnit": "KB" }";
        GpuDeviceExtensionsJsonExtensions.GpuDeviceExtensionsConfig? deserialized = GpuDeviceExtensionsJsonExtensions.FromJson(json);

        if (deserialized is not null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"Type: {deserialized.Type}");
            Console.WriteLine($"IsMemoryExtensionsEnabled: {deserialized.IsMemoryExtensionsEnabled}");
            Console.WriteLine($"IsColorSpaceDetectionEnabled: {deserialized.IsColorSpaceDetectionEnabled}");
            Console.WriteLine($"IsDeviceTypeDisplayEnabled: {deserialized.IsDeviceTypeDisplayEnabled}");
            Console.WriteLine($"DefaultMemoryUnit: {deserialized.DefaultMemoryUnit}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = GpuDeviceExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = config.ToJson();
        GpuDeviceExtensionsJsonExtensions.GpuDeviceExtensionsConfig? roundTripResult = GpuDeviceExtensionsJsonExtensions.FromJson(roundTripJson);

        if (roundTripResult is not null)
        {
            bool isValid = roundTripResult.Type == config.Type &&
                         roundTripResult.IsMemoryExtensionsEnabled == config.IsMemoryExtensionsEnabled &&
                         roundTripResult.IsColorSpaceDetectionEnabled == config.IsColorSpaceDetectionEnabled &&
                         roundTripResult.IsDeviceTypeDisplayEnabled == config.IsDeviceTypeDisplayEnabled &&
                         roundTripResult.DefaultMemoryUnit == config.DefaultMemoryUnit;
            Console.WriteLine($"\nRound-trip successful: {isValid}");
        }

        // Create configuration with all properties set to false
        var disabledConfig = new GpuDeviceExtensionsJsonExtensions.GpuDeviceExtensionsConfig
        {
            IsMemoryExtensionsEnabled = false,
            IsColorSpaceDetectionEnabled = false,
            IsDeviceTypeDisplayEnabled = false,
            DefaultMemoryUnit = "B"
        };

        Console.WriteLine($"\nDisabled configuration:");
        Console.WriteLine(disabledConfig.ToJson());
    }
}

```

## SimdCapabilitiesExtensionsJsonExtensions

The `SimdCapabilitiesExtensionsJsonExtensions` class provides System.Text.Json serialization utilities for `SimdCapabilitiesExtensions` configurations. It enables easy serialization and deserialization of SIMD capability detection settings with support for JSON formatting options and error handling.

### Key Features

- JSON serialization of SIMD capability configurations using camelCase property naming
- Support for both compact and indented JSON output formats
- Safe deserialization with null handling and error recovery
- Configuration validation through JSON parsing
- Thread-safe serialization with optimized JsonSerializerOptions
- Boolean flags for vector width support, optimal SIMD level detection, SIMD availability, and friendly string formatting

### Usage Examples

```csharp

using GpuImageProcessing.Domain;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a SIMD capabilities configuration with custom settings
        var simdConfig = new SimdCapabilitiesExtensionsJsonExtensions.SimdCapabilitiesExtensions
        {
            IsVectorWidthSupportEnabled = true,
            IsOptimalSimdLevelEnabled = true,
            IsSimdAvailabilityEnabled = true,
            IsFriendlyStringEnabled = false
        };

        // Serialize to compact JSON
        string compactJson = simdConfig.ToJson();
        Console.WriteLine("Compact JSON configuration:");
        Console.WriteLine(compactJson);

        // Serialize to indented JSON for readability
        string indentedJson = simdConfig.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON configuration:");
        Console.WriteLine(indentedJson);

        // Deserialize from JSON string
        string json = @"{ ""isVectorWidthSupportEnabled"": false, ""isOptimalSimdLevelEnabled"": true, ""isSimdAvailabilityEnabled"": true, ""isFriendlyStringEnabled"": true }";
        var deserialized = SimdCapabilitiesExtensionsJsonExtensions.FromJson(json);

        if (deserialized is not null)
        {
            Console.WriteLine($"\nDeserialized configuration:");
            Console.WriteLine($"IsVectorWidthSupportEnabled: {deserialized.IsVectorWidthSupportEnabled}");
            Console.WriteLine($"IsOptimalSimdLevelEnabled: {deserialized.IsOptimalSimdLevelEnabled}");
            Console.WriteLine($"IsSimdAvailabilityEnabled: {deserialized.IsSimdAvailabilityEnabled}");
            Console.WriteLine($"IsFriendlyStringEnabled: {deserialized.IsFriendlyStringEnabled}");
        }

        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = SimdCapabilitiesExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? ""Success"" : ""Failed (expected)"")}");

        // Serialize and deserialize round-trip to verify data integrity
        string roundTripJson = simdConfig.ToJson();
        var roundTripResult = SimdCapabilitiesExtensionsJsonExtensions.FromJson(roundTripJson);

        if (roundTripResult is not null)
        {
            bool isValid = roundTripResult.IsVectorWidthSupportEnabled == simdConfig.IsVectorWidthSupportEnabled &&
                         roundTripResult.IsOptimalSimdLevelEnabled == simdConfig.IsOptimalSimdLevelEnabled &&
                         roundTripResult.IsSimdAvailabilityEnabled == simdConfig.IsSimdAvailabilityEnabled &&
                         roundTripResult.IsFriendlyStringEnabled == simdConfig.IsFriendlyStringEnabled;
            Console.WriteLine($"\nRound-trip successful: {isValid}");
        }

        // Create configuration with all properties set to false
        var disabledConfig = new SimdCapabilitiesExtensionsJsonExtensions.SimdCapabilitiesExtensions
        {
            IsVectorWidthSupportEnabled = false,
            IsOptimalSimdLevelEnabled = false,
            IsSimdAvailabilityEnabled = false,
            IsFriendlyStringEnabled = false
        };

        Console.WriteLine($"\nDisabled configuration:");
        Console.WriteLine(disabledConfig.ToJson());
    }
}

## WebhookHandlerJsonExtensions

The `WebhookHandlerJsonExtensions` class provides System.Text.Json serialization utilities for the `WebhookHandler` class, enabling serialization and deserialization of webhook handler state with support for both compact and indented JSON output formats. It includes methods for converting webhook handlers to JSON strings and reconstructing them from JSON, along with properties that expose the current webhook subscription statistics.

### Key Features

- Serializes `WebhookHandler` instances to JSON with camelCase property naming
- Supports both compact and indented JSON output formats
- Deserializes JSON strings back to `WebhookHandler` instances with logger injection
- Provides safe deserialization with error handling via `TryFromJson`
- Exposes webhook statistics through `ActiveWebhookCount` and `TotalWebhookCount` properties
- Includes subscription information in serialized output

### Usage Examples

```csharp

using GpuImageProcessing.Integration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        // Create a logger (typically from dependency injection in real applications)
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<WebhookHandler>();
        
        // Create a webhook handler instance
        var webhookHandler = new WebhookHandler(logger);
        
        // Serialize to compact JSON
        string compactJson = webhookHandler.ToJson();
        Console.WriteLine("Compact JSON:");
        Console.WriteLine(compactJson);
        
        // Serialize to indented JSON for readability
        string indentedJson = webhookHandler.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON:");
        Console.WriteLine(indentedJson);
        
        // Deserialize from JSON string
        string json = @"{ "activeWebhookCount": 3, "totalWebhookCount": 5, "subscriptions": [] }";
        var deserializedHandler = WebhookHandlerJsonExtensions.FromJson(json, logger);
        
        if (deserializedHandler != null)
        {
            Console.WriteLine($"\nDeserialized handler:");
            Console.WriteLine($"Active webhooks: {webhookHandler.ActiveWebhookCount}");
            Console.WriteLine($"Total webhooks: {webhookHandler.TotalWebhookCount}");
        }
        
        // Try to deserialize with error handling
        string invalidJson = @"{ invalid json }";
        bool success = WebhookHandlerJsonExtensions.TryFromJson(invalidJson, logger, out var result);
        Console.WriteLine($"\nTryFromJson with invalid JSON: {(success ? "Success" : "Failed (expected)")}");
        
        // Access webhook statistics
        Console.WriteLine($"\nWebhook statistics:");
        Console.WriteLine($"Active webhooks: {webhookHandler.ActiveWebhookCount}");
        Console.WriteLine($"Total webhooks: {webhookHandler.TotalWebhookCount}");
    }
}

```

## AsyncTaskQueueExtensions

The `AsyncTaskQueueExtensions` class provides extension methods for the `AsyncTaskQueue` class that extend the queue with additional query capabilities and task management utilities. It includes methods for enqueuing tasks, retrieving tasks by state, getting task counts, finding oldest/newest tasks, and calculating performance metrics.

### Key Features

- Enqueue new tasks with priorities and names
- Get tasks filtered by state (running, queued, completed, failed, cancelled)
- Retrieve oldest and newest tasks based on enqueue time
- Get task counts by state and total task count
- Calculate task durations and average processing times
- Determine task positions based on priority
- Check if tasks exist in specific states

### Usage Examples

```csharp

using GpuImageProcessing.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create an AsyncTaskQueue instance
        var taskQueue = new AsyncTaskQueue();

        // Enqueue some tasks with different priorities
        var task1 = taskQueue.EnqueueTask(
            async ct => 
            {
                await Task.Delay(100, ct);
                Console.WriteLine("Task 1 completed");
            },
            priority: 1,
            name: "High Priority Task"
        );

        var task2 = taskQueue.EnqueueTask(
            async ct => 
            {
                await Task.Delay(200, ct);
                Console.WriteLine("Task 2 completed");
            },
            priority: 2,
            name: "Medium Priority Task"
        );

        var task3 = taskQueue.EnqueueTask(
            async ct => 
            {
                await Task.Delay(50, ct);
                Console.WriteLine("Task 3 completed");
            },
            priority: 0,
            name: "Low Priority Task"
        );

        // Get task counts by state
        Console.WriteLine($"Total tasks: {taskQueue.GetTotalTaskCount()}");
        Console.WriteLine($"Queued tasks: {taskQueue.GetTaskCount(TaskState.Queued)}");
        Console.WriteLine($"Running tasks: {taskQueue.GetTaskCount(TaskState.Running)}");

        // Get oldest and newest tasks
        var oldestTask = taskQueue.GetOldestTask();
        var newestTask = taskQueue.GetNewestTask();
        Console.WriteLine($"Oldest task: {oldestTask?.Name ?? "None"}");
        Console.WriteLine($"Newest task: {newestTask?.Name ?? "None"}");

        // Get tasks by state
        var queuedTasks = taskQueue.GetQueuedTasks();
        var runningTasks = taskQueue.GetRunningTasks();
        var completedTasks = taskQueue.GetCompletedTasks();
        var failedTasks = taskQueue.GetFailedTasks();
        var cancelledTasks = taskQueue.GetCancelledTasks();

        Console.WriteLine($"Queued: {queuedTasks.Count}, Running: {runningTasks.Count}");
        Console.WriteLine($"Completed: {completedTasks.Count}, Failed: {failedTasks.Count}");
        Console.WriteLine($"Cancelled: {cancelledTasks.Count}");

        // Get task position (lower numbers = higher priority)
        int position = taskQueue.GetTaskPosition(task1);
        Console.WriteLine($"Task 1 position: {position}");

        // Check if tasks exist in specific states
        bool hasQueued = taskQueue.HasTasksInState(TaskState.Queued);
        bool hasRunning = taskQueue.HasTasksInState(TaskState.Running);
        Console.WriteLine($"Has queued tasks: {hasQueued}");
        Console.WriteLine($"Has running tasks: {hasRunning}");

        // Wait for tasks to complete
        await Task.Delay(1000);

        // Get task duration after completion
        long? duration = taskQueue.GetTaskDurationMilliseconds(task1);
        Console.WriteLine($"Task 1 duration: {duration}ms");

        // Get average task duration
        double? avgDuration = taskQueue.GetAverageTaskDurationMilliseconds();
        Console.WriteLine($"Average task duration: {avgDuration?.ToString("F2") ?? "N/A"}ms");

        // Get tasks by state
        var allQueued = taskQueue.GetQueuedTasks();
        Console.WriteLine($"Remaining queued tasks: {allQueued.Count}");
    }
}

```

## ImageProcessingExceptionExtensions

The `ImageProcessingExceptionExtensions` class provides extension methods for the `ImageProcessingException` class, facilitating easy error code retrieval, status checking, and detailed message generation. These utilities simplify exception handling and debugging by allowing quick verification of error types, such as whether an exception is file-related or indicates an invalid image format.

### Usage Examples

```csharp

using GpuImageProcessing.Core.Exceptions;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Simulate a collection of exceptions
        var exceptions = new List<ImageProcessingException>
        {
            new InvalidImageException("Invalid format") { ErrorCode = "ERR_001" },
            new ImageFileException("File not found") { ErrorCode = "ERR_002" }
        };

        // Get a list of error codes
        var errorCodes = exceptions.GetErrorCodes();
        Console.WriteLine($"Error codes: {string.Join(", ", errorCodes)}");

        // Check for specific error code
        bool hasError = exceptions[0].HasErrorCode("ERR_001");
        Console.WriteLine($"Exception 0 has ERR_001: {hasError}");

        // Get detailed message
        string message = exceptions[0].GetDetailedMessage();
        Console.WriteLine($"Detailed message: {message}");

        // Check exception types
        Console.WriteLine($"Is file related: {exceptions[1].IsFileRelated()}");
        Console.WriteLine($"Is invalid image: {exceptions[0].IsInvalidImage()}");
    }
}
```

## ProcessingPipeline
