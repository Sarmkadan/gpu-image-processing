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

## DataConversionUtilities

The `DataConversionUtilities` class provides essential data conversion utilities for transforming between different data formats commonly used in GPU image processing. It includes hexadecimal string conversion, byte array manipulation, floating-point data conversion, file size formatting, and tolerance-based comparison operations.

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

## ProcessingPipeline