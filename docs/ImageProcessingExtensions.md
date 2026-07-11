# ImageProcessingExtensions

A static helper class that provides utility methods for common image‑processing tasks such as format inspection, resource estimation, and validation checks. The members are intended to be called directly on the type without instantiation.

## API

### GetColorSpaceForFormat
**Purpose:** Returns the color space that corresponds to a supplied image format.  
**Parameters:**  
- `format` (ImageFormat) – The image format to evaluate.  
**Return Value:** A `ColorSpace` enumeration value indicating the color space (e.g., RGB, YUV, Grayscale).  
**Exceptions:**  
- `ArgumentException` – If `format` does not represent a known image format.  

### CalculateTotalBytes
**Purpose:** Computes the total number of bytes required to store an image of given dimensions and pixel depth.  
**Parameters:**  
- `width` (int) – Image width in pixels.  
- `height` (int) – Image height in pixels.  
- `bitsPerPixel` (int) – Number of bits used to represent each pixel.  
**Return Value:** The total byte count as a `long`.  
**Exceptions:**  
- `ArgumentOutOfRangeException` – If any parameter is less than or equal to zero.  

### IsResolutionValid
**Purpose:** Determines whether a width/height pair conforms to the library’s supported resolution constraints.  
**Parameters:**  
- `width` (int) – Image width in pixels.  
- `height` (int) – Image height in pixels.  
**Return Value:** `true` if the resolution is acceptable; otherwise `false`.  
**Exceptions:** None (returns `false` for invalid inputs).  

### GetFileExtension
**Purpose:** Provides the canonical file extension (including the leading dot) for a given image format.  
**Parameters:**  
- `format` (ImageFormat) – The image format to query.  
**Return Value:** A string such as `".png"` or `".jpg"`.  
**Exceptions:**  
- `ArgumentException` – If `format` is not recognized.  

### GetFormatFromExtension
**Purpose:** Infers the image format from a file extension string.  
**Parameters:**  
- `extension` (string) – The file extension, with or without a leading dot (case‑insensitive).  
**Return Value:** The matching `ImageFormat` enumeration value.  
**Exceptions:**  
- `ArgumentNullException` – If `extension` is `null`.  
- `ArgumentException` – If no known format matches the extension.  

### CanApplyFilter
**Purpose:** Checks whether a particular filter can be applied to an image of a given format without requiring conversion.  
**Parameters:**  
- `filterType` (string) – Identifier of the filter (e.g., `"GaussianBlur"`).  
- `format` (ImageFormat) – The image format to test.  
**Return Value:** `true` if the filter is directly supported; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` – If either parameter is `null`.  
- `ArgumentException` – If `filterType` is unknown.  

### GetAspectRatioDescription
**Purpose:** Produces a human‑readable description of the aspect ratio for a given width and height.  
**Parameters:**  
- `width` (int) – Image width in pixels.  
- `height` (int) – Image height in pixels.  
**Return Value:** A string such as `"16:9"` or `"4:3"`.  
**Exceptions:**  
- `ArgumentOutOfRangeException` – If either dimension is less than or equal to zero.  

### EstimateProcessingTime
**Purpose:** Estimates the elapsed time (in seconds) required to process a single image with a specified filter.  
**Parameters:**  
- `width` (int) – Image width in pixels.  
- `height` (int) – Image height in pixels.  
- `filterType` (string) – Identifier of the filter to apply.  
**Return Value:** A `double` representing the estimated processing time.  
**Exceptions:**  
- `ArgumentOutOfRangeException` – If width or height is non‑positive.  
- `ArgumentException` – If `filterType` is not recognized.  

### GetMemoryRequirement
**Purpose:** Calculates the memory (in bytes) needed to hold an image buffer for a given format and size.  
**Parameters:**  
- `width` (int) – Image width in pixels.  
- `height` (int) – Image height in pixels.  
- `format` (ImageFormat) – Pixel format of the image.  
**Return Value:** A `long` indicating the required memory allocation.  
**Exceptions:**  
- `ArgumentOutOfRangeException` – If width or height is non‑positive.  
- `ArgumentException` – If `format` is unsupported.  

### EstimateTotalTime
**Purpose:** Estimates the cumulative processing time for a batch of images.  
**Parameters:**  
- `imageCount` (int) – Number of images to process.  
- `averageWidth` (int) – Average width of the images in pixels.  
- `averageHeight` (int) – Average height of the images in pixels.  
- `filterType` (string) – Identifier of the filter to apply.  
**Return Value:** A `double` representing the total estimated time in seconds.  
**Exceptions:**  
- `ArgumentOutOfRangeException` – If any count or dimension is non‑positive.  
- `ArgumentException` – If `filterType` is unknown.  

### GetMemoryRequirements
**Purpose:** Returns the memory requirements for multiple images with varying sizes.  
**Parameters:**  
- `sizes` (IEnumerable<(int Width, int Height, ImageFormat Format)>) – Collection of image specifications.  
**Return Value:** A `long` representing the sum of memory needed for all images.  
**Exceptions:**  
- `ArgumentNullException` – If `sizes` is `null`.  
- `ArgumentOutOfRangeException` – If any width or height in the collection is non‑positive.  
- `ArgumentException` – If any format is unsupported.  

### IsSlowdownDetected
**Purpose:** Evaluates whether a performance slowdown has been observed based on recent processing metrics.  
**Parameters:**  
- `recentTimes` (IEnumerable<double>) – Sequence of observed processing times (in seconds) for recent operations.  
- `baseline` (double) – Expected processing time under normal conditions.  
**Return Value:** `true` if a slowdown is detected; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` – If `recentTimes` is `null`.  
- `ArgumentOutOfRangeException` – If `baseline` is less than or equal to zero.  

### GetPerformanceGrade
**Purpose:** Assigns a single‑character performance grade based on the measured processing time relative to a target.  
**Parameters:**  
- `measuredTime` (double) – Observed processing time (in seconds).  
- `targetTime` (double) – Desired maximum processing time (in seconds).  
**Return Value:** A `char` grade (e.g., `'A'` for excellent, `'F'` for poor).  
**Exceptions:**  
- `ArgumentOutOfRangeException` – If either time argument is less than or equal to zero.  

## Usage

```csharp
using GpuImageProcessing;

// Validate image resolution before allocating resources
int width = 1920;
int height = 1080;
if (ImageProcessingExtensions.IsResolutionValid(width, height))
{
    long bytesNeeded = ImageProcessingExtensions.CalculateTotalBytes(width, height, 24);
    Console.WriteLine($"Required buffer size: {bytesNeeded} bytes");
}
else
{
    Console.WriteLine("Resolution not supported.");
}
```

```csharp
using GpuImageProcessing;

// Estimate processing time for a batch of JPEG images using a sharpen filter
int imageCount = 150;
int avgWidth = 2560;
int avgHeight = 1440;
string filter = "Sharpen";

double totalSec = ImageProcessingExtensions.EstimateTotalTime(
    imageCount, avgWidth, avgHeight, filter);
Console.WriteLine($"Estimated total processing time: {totalSec:F2} s");

// Determine performance grade based on a single measurement
double measured = 0.42; // seconds
double target = 0.5;    // seconds
char grade = ImageProcessingExtensions.GetPerformanceGrade(measured, target);
Console.WriteLine($"Performance grade: {grade}");
```

## Notes

- All methods are **static** and do not rely on instance state; therefore they are thread‑safe as long as any delegate or callback supplied by the caller does not introduce mutable shared state.  
- Methods that accept dimensional parameters (width, height, bitsPerPixel) validate that the values are positive; zero or negative values result in an `ArgumentOutOfRangeException`.  
- Format‑related methods (`GetColorSpaceForFormat`, `GetFormatFromExtension`, `GetFileExtension`) throw an `ArgumentException` when the supplied format or extension is not recognized by the library.  
- The `CanApplyFilter` method only indicates whether a filter can be applied *without* requiring a format conversion; it does not perform the conversion itself.  
- Memory estimation methods assume tightly packed pixel data with no padding; actual allocations may differ if the underlying image library adds stride or alignment overhead.  
- Performance‑related methods (`EstimateProcessingTime`, `IsSlowdownDetected`, `GetPerformanceGrade`) rely on heuristics derived from benchmark data; actual runtimes may vary based on hardware, driver version, and concurrent system load.  
- Enumerations such as `ImageFormat` and `ColorSpace` are defined elsewhere in the `gpu-image-processing` project; consult their documentation for the complete set of supported values.
