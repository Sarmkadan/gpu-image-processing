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

## PathUtilities

The `PathUtilities` class provides a comprehensive set of utilities for path manipulation, normalization, and directory management. It handles cross-platform path operations, safe file operations, and directory traversal with robust error handling to ensure reliable file system operations across different operating systems.

### Usage Example

```csharp
using GpuImageProcessing.Utilities;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        string basePath = "/home/user/projects/gpu-image-processing";
        string tempDir = PathUtilities.CombinePaths(basePath, "temp");

        // Normalize and get absolute path
        string normalizedPath = PathUtilities.NormalizePath("~/projects/../projects/gpu-image-processing");
        Console.WriteLine($"Normalized: {normalizedPath}");

        // Get relative path
        string relativePath = PathUtilities.GetRelativePath(basePath, "/home/user/data/images");
        Console.WriteLine($"Relative: {relativePath}");

        // Combine multiple path segments
        string combinedPath = PathUtilities.CombinePaths(basePath, "output", "processed", "images");
        Console.WriteLine($"Combined: {combinedPath}");

        // Ensure directory exists
        bool dirCreated = PathUtilities.EnsureDirectoryExists(tempDir);
        Console.WriteLine($"Directory created: {dirCreated}");

        // Get absolute path
        string absolutePath = PathUtilities.GetAbsolutePath("temp/../temp2");
        Console.WriteLine($"Absolute: {absolutePath}");

        // Safe directory operations
        bool cleared = PathUtilities.ClearDirectory(tempDir);
        Console.WriteLine($"Directory cleared: {cleared}");

        // Count files recursively
        int fileCount = PathUtilities.CountFiles(basePath, "*.cs");
        Console.WriteLine($"C# files: {fileCount}");

        // Get all files recursively
        var allFiles = PathUtilities.GetFilesRecursive(basePath, "*.md");
        Console.WriteLine($"Markdown files found: {allFiles.Count}");

        // Generate unique filename
        string testFile = PathUtilities.CombinePaths(tempDir, "test.txt");
        File.WriteAllText(testFile, "test");
        string uniqueFile = PathUtilities.GenerateUniqueFilename(testFile);
        Console.WriteLine($"Unique filename: {Path.GetFileName(uniqueFile)}");

        // Get directory size
        long dirSize = PathUtilities.GetDirectorySize(basePath);
        Console.WriteLine($"Project size: {dirSize:N0} bytes");

        // Safe file operations
        string destFile = PathUtilities.CombinePaths(tempDir, "copy.txt");
        bool copied = PathUtilities.SafeCopyFile(testFile, destFile, overwrite: true);
        Console.WriteLine($"File copied: {copied}");

        bool moved = PathUtilities.SafeMoveFile(destFile, PathUtilities.CombinePaths(tempDir, "moved.txt"), overwrite: true);
        Console.WriteLine($"File moved: {moved}");

        // Get recent files (modified in last 24 hours)
        var recentFiles = PathUtilities.GetRecentFiles(basePath, TimeSpan.FromHours(24), "*.cs");
        Console.WriteLine($"Recently modified C# files: {recentFiles.Count}");

        // Get path size info
        string sizeInfo = PathUtilities.GetPathSizeInfo(basePath);
        Console.WriteLine($"Path size info: {sizeInfo}");

        // Cleanup
        PathUtilities.SafeDeleteDirectory(tempDir);
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

## ProcessingPipeline