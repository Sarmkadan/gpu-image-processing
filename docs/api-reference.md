# API Reference

Complete reference for all public APIs in GPU Image Processing.

## Table of Contents

- [Services](#services)
- [Models](#models)
- [Enums](#enums)
- [Exceptions](#exceptions)
- [Repositories](#repositories)
- [Middleware](#middleware)

## Services

### ImageProcessingService

Core service for image processing operations.

#### Methods

##### RegisterImageAsync

```csharp
Task<Image> RegisterImageAsync(string filePath, string name)
```

Register an image file for processing.

**Parameters**:
- `filePath` (string): Path to the image file
- `name` (string): Display name for the image

**Returns**: `Task<Image>` - The registered image object

**Throws**: `ImageProcessingException` if file not found or invalid format

**Example**:
```csharp
var image = await imageProcessing.RegisterImageAsync(
    "photos/landscape.jpg", 
    "Landscape"
);
```

##### ProcessImageAsync

```csharp
Task<ProcessingResult> ProcessImageAsync(
    Guid imageId,
    List<Guid> filterIds,
    List<Guid> transformIds,
    Guid profileId)
```

Process an image with specified filters and transforms.

**Parameters**:
- `imageId` (Guid): ID of the registered image
- `filterIds` (List<Guid>): List of filter IDs to apply
- `transformIds` (List<Guid>): List of transform IDs to apply
- `profileId` (Guid): Processing profile ID (use Guid.Empty for default)

**Returns**: `Task<ProcessingResult>` - The processing result

**Example**:
```csharp
var result = await imageProcessing.ProcessImageAsync(
    imageId,
    new List<Guid> { filterId },
    new List<Guid> { transformId },
    profileId
);
```

##### GetResultAsync

```csharp
Task<ProcessingResult> GetResultAsync(Guid resultId)
```

Retrieve a previously processed result.

**Parameters**:
- `resultId` (Guid): ID of the processing result

**Returns**: `Task<ProcessingResult>` - The result object

##### GetImagesAsync

```csharp
Task<List<Image>> GetImagesAsync()
```

List all registered images.

**Returns**: `Task<List<Image>>` - All registered images

##### DeleteImageAsync

```csharp
Task DeleteImageAsync(Guid imageId)
```

Delete a registered image.

**Parameters**:
- `imageId` (Guid): ID of the image to delete

---

### FilterService

Manages image filters and their parameters.

#### Methods

##### CreateFilterAsync

```csharp
Task<Filter> CreateFilterAsync(
    FilterType type,
    string name,
    string description)
```

Create a new filter.

**Parameters**:
- `type` (FilterType): Type of filter (Gaussian, Bilateral, Median, Sobel, Canny, etc.)
- `name` (string): Filter name
- `description` (string): Filter description

**Returns**: `Task<Filter>` - The created filter

**Supported Types**:
- `FilterType.Gaussian` - Gaussian blur
- `FilterType.Bilateral` - Bilateral filter
- `FilterType.Median` - Median filter
- `FilterType.Sobel` - Sobel edge detection
- `FilterType.Canny` - Canny edge detection
- `FilterType.Morphological` - Morphological operations

**Example**:
```csharp
var filter = await filterService.CreateFilterAsync(
    FilterType.Gaussian,
    "Blur",
    "Gaussian blur filter"
);
```

##### UpdateFilterParametersAsync

```csharp
Task UpdateFilterParametersAsync(
    Guid filterId,
    Dictionary<string, float> parameters)
```

Update filter parameters.

**Parameters**:
- `filterId` (Guid): ID of the filter
- `parameters` (Dictionary<string, float>): Parameter name-value pairs

**Available Parameters**:
- **Gaussian**: `Sigma` (0.1-10.0), `KernelSize` (3-31)
- **Bilateral**: `SigmaColor` (1.0-255.0), `SigmaSpace` (1.0-255.0)
- **Median**: `KernelSize` (3-31)
- **Sobel**: `ThresholdLow` (0-255), `ThresholdHigh` (0-255)
- **Canny**: `Threshold1` (0-255), `Threshold2` (0-255)

**Example**:
```csharp
await filterService.UpdateFilterParametersAsync(
    filterId,
    new Dictionary<string, float>
    {
        { "Sigma", 2.5f },
        { "KernelSize", 7f }
    }
);
```

##### GetFilterAsync

```csharp
Task<Filter> GetFilterAsync(Guid filterId)
```

Retrieve filter details.

**Parameters**:
- `filterId` (Guid): ID of the filter

**Returns**: `Task<Filter>` - The filter object

##### GetFiltersAsync

```csharp
Task<List<Filter>> GetFiltersAsync()
```

List all filters.

**Returns**: `Task<List<Filter>>` - All filters

##### DeleteFilterAsync

```csharp
Task DeleteFilterAsync(Guid filterId)
```

Delete a filter.

**Parameters**:
- `filterId` (Guid): ID of the filter to delete

---

### TransformService

Manages geometric transformations.

#### Methods

##### CreateTransformAsync

```csharp
Task<Transform> CreateTransformAsync(
    TransformType type,
    string name,
    string description)
```

Create a new transform.

**Parameters**:
- `type` (TransformType): Type of transform
- `name` (string): Transform name
- `description` (string): Transform description

**Supported Types**:
- `TransformType.Resize` - Resize image
- `TransformType.Rotate` - Rotate image
- `TransformType.ColorSpaceConversion` - Convert color space
- `TransformType.Normalization` - Normalize values
- `TransformType.HistogramEqualization` - Equalize histogram
- `TransformType.AffineTransform` - Affine transformation

##### UpdateTransformParametersAsync

```csharp
Task UpdateTransformParametersAsync(
    Guid transformId,
    Dictionary<string, float> parameters)
```

Update transform parameters.

**Available Parameters**:
- **Resize**: `ScaleX`, `ScaleY` (0.1-10.0)
- **Rotate**: `Angle` (0-360)
- **ColorSpaceConversion**: `TargetSpace` (0=RGB, 1=HSV, 2=LAB)
- **Normalization**: `Min` (0-1), `Max` (0-1)

##### GetTransformAsync

```csharp
Task<Transform> GetTransformAsync(Guid transformId)
```

Retrieve transform details.

##### GetTransformsAsync

```csharp
Task<List<Transform>> GetTransformsAsync()
```

List all transforms.

##### DeleteTransformAsync

```csharp
Task DeleteTransformAsync(Guid transformId)
```

Delete a transform.

---

### BatchProcessingService

Manages batch processing jobs.

#### Methods

##### CreateJobAsync

```csharp
Task<ProcessingJob> CreateJobAsync(
    string name,
    List<Guid> imageIds,
    List<Guid> filterIds,
    List<Guid> transformIds,
    Guid profileId)
```

Create a batch processing job.

**Parameters**:
- `name` (string): Job name
- `imageIds` (List<Guid>): IDs of images to process
- `filterIds` (List<Guid>): IDs of filters to apply
- `transformIds` (List<Guid>): IDs of transforms to apply
- `profileId` (Guid): Processing profile ID

**Returns**: `Task<ProcessingJob>` - The created job

##### ExecuteJobAsync

```csharp
Task ExecuteJobAsync(Guid jobId, Guid profileId)
```

Execute a batch job.

**Parameters**:
- `jobId` (Guid): ID of the job
- `profileId` (Guid): Processing profile ID

##### GetJobAsync

```csharp
Task<ProcessingJob> GetJobAsync(Guid jobId)
```

Get job status and details.

**Returns**: `Task<ProcessingJob>` - Job with current status and progress

**Properties**:
- `Status` (string): "Pending", "Running", "Completed", "Failed", "Cancelled"
- `ProcessedCount` (int): Number of processed images
- `TotalCount` (int): Total images in job
- `SuccessCount` (int): Successful processing count
- `FailureCount` (int): Failed processing count

##### CancelJobAsync

```csharp
Task CancelJobAsync(Guid jobId)
```

Cancel a running job.

##### GetJobsAsync

```csharp
Task<List<ProcessingJob>> GetJobsAsync()
```

List all jobs.

---

### DeviceService

Manages compute devices.

#### Methods

##### GetAvailableDevicesAsync

```csharp
Task<List<DeviceInfo>> GetAvailableDevicesAsync()
```

Get all available compute devices.

**Returns**: `Task<List<DeviceInfo>>` - List of devices

**DeviceInfo Properties**:
```csharp
public class DeviceInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DeviceType { get; set; }  // "GPU", "CPU", "ACCELERATOR"
    public string Vendor { get; set; }       // "NVIDIA", "AMD", "Intel"
    public long GlobalMemoryMB { get; set; }
    public int ComputeUnits { get; set; }
    public int ClockFrequencyMHz { get; set; }
    public bool Available { get; set; }
}
```

##### SelectDeviceAsync

```csharp
Task SelectDeviceAsync(Guid deviceId)
```

Select preferred compute device.

**Parameters**:
- `deviceId` (Guid): ID of the device to use

##### GetCurrentDeviceAsync

```csharp
Task<DeviceInfo> GetCurrentDeviceAsync()
```

Get currently selected device.

##### GetDeviceCapabilitiesAsync

```csharp
Task<Dictionary<string, string>> GetDeviceCapabilitiesAsync(Guid deviceId)
```

Get device capabilities and extensions.

---

### PerformanceMonitoringService

Monitor system performance metrics.

#### Methods

##### GetMetricsAsync

```csharp
Task<PerformanceMetrics> GetMetricsAsync()
```

Get current performance metrics.

**PerformanceMetrics Properties**:
```csharp
public class PerformanceMetrics
{
    public double GpuUtilization { get; set; }      // 0-100%
    public long MemoryUsedMB { get; set; }
    public long MemoryAvailableMB { get; set; }
    public double ImagesPerSecond { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public int ActiveJobCount { get; set; }
    public long TotalProcessedBytes { get; set; }
}
```

##### GetHistoricalMetricsAsync

```csharp
Task<List<PerformanceMetrics>> GetHistoricalMetricsAsync(
    DateTime startTime,
    DateTime endTime)
```

Get historical metrics.

---

## Models

### Image

```csharp
public class Image
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FilePath { get; set; }
    public string Format { get; set; }              // JPEG, PNG, BMP, TIFF, WebP
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string Metadata { get; set; }
}
```

### Filter

```csharp
public class Filter
{
    public Guid Id { get; set; }
    public FilterType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, FilterParameter> Parameters { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Transform

```csharp
public class Transform
{
    public Guid Id { get; set; }
    public TransformType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, float> Parameters { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### ProcessingResult

```csharp
public class ProcessingResult
{
    public Guid Id { get; set; }
    public Guid ImageId { get; set; }
    public string Status { get; set; }              // "Success", "Failed", "Pending"
    public string OutputPath { get; set; }
    public long OutputSizeBytes { get; set; }
    public double ProcessingTimeMs { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; }
}
```

### ProcessingJob

```csharp
public class ProcessingJob
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public int TotalCount { get; set; }
    public int ProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

---

## Enums

### FilterType

```csharp
public enum FilterType
{
    Gaussian = 0,
    Bilateral = 1,
    Median = 2,
    Sobel = 3,
    Canny = 4,
    Morphological = 5,
    LaplacianOfGaussian = 6,
    GaborFilter = 7
}
```

### TransformType

```csharp
public enum TransformType
{
    Resize = 0,
    Rotate = 1,
    ColorSpaceConversion = 2,
    Normalization = 3,
    HistogramEqualization = 4,
    AffineTransform = 5,
    WarpPerspective = 6,
    Crop = 7
}
```

### ProcessingStatus

```csharp
public enum ProcessingStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}
```

---

## Exceptions

### ImageProcessingException

Base exception for image processing errors.

```csharp
public class ImageProcessingException : Exception
{
    public ImageProcessingException(string message) : base(message) { }
    public ImageProcessingException(string message, Exception inner) 
        : base(message, inner) { }
}
```

### OpenCLException

GPU/OpenCL specific exceptions.

```csharp
public class OpenCLException : ImageProcessingException
{
    public int ErrorCode { get; set; }
    
    public OpenCLException(string message, int errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
```

---

## Repositories

### IRepository<T>

Generic repository interface.

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
```

### Specialized Repositories

#### ImageRepository
```csharp
Task<Image> GetByNameAsync(string name);
Task<List<Image>> GetByFormatAsync(string format);
Task<List<Image>> GetRecentAsync(int count);
```

#### JobRepository
```csharp
Task<List<ProcessingJob>> GetByStatusAsync(string status);
Task<List<ProcessingJob>> GetRecentAsync(int count);
Task<ProcessingJob> GetActiveJobAsync();
```

#### ResultRepository
```csharp
Task<List<ProcessingResult>> GetByImageIdAsync(Guid imageId);
Task<List<ProcessingResult>> GetByStatusAsync(string status);
Task<List<ProcessingResult>> GetRecentAsync(int count);
```

---

## Middleware

### IProcessingMiddleware

```csharp
public interface IProcessingMiddleware
{
    Task<MiddlewareContext> ExecuteAsync(MiddlewareContext context);
}
```

### Built-in Middleware

- **ErrorHandlingMiddleware** - Catches and handles exceptions
- **LoggingMiddleware** - Logs processing operations
- **RateLimitingMiddleware** - Enforces rate limits
- **CompressionMiddleware** - Compresses output
- **AuthorizationMiddleware** - Enforces authorization

---

For more examples, see the [examples/](../examples/) directory.
