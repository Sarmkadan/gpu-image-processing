# ImageUtilities

Provides a collection of static helper methods for common image‑file operations such as validation, format detection, size calculation, filename generation, and safe cleanup. The utilities are intended to be used by image‑processing pipelines and UI layers that need lightweight, filesystem‑centric checks without pulling in heavyweight image libraries.

## API

### IsSupportedImageFile
```csharp
public static bool IsSupportedImageFile(string filePath);
```
* **Purpose** – Determines whether the file at `filePath` has an extension and internal signature that the library recognizes as an image (e.g., JPEG, PNG, BMP, TIFF, GIF, WebP).  
* **Parameters** – `filePath`: Full or relative path to the file to test.  
* **Return value** – `true` if the file is considered a supported image; otherwise `false`.  
* **Throws** –  
  * `ArgumentNullException` if `filePath` is `null`.  
  * `ArgumentException` if `filePath` is an empty string or consists only of whitespace.  
  * `FileNotFoundException` if the file does not exist.  
  * `UnauthorizedAccessException` if the caller lacks permission to read the file.  
  * `IOException` for other I/O errors (e.g., sharing violations).

### GetImageFormat
```csharp
public static string GetImageFormat(string filePath);
```
* **Purpose** – Returns a string identifier for the image format of the file (e.g., "jpeg", "png", "bmp"). The identifier matches the canonical names used by the library’s encoders/decoders.  
* **Parameters** – `filePath`: Path to the image file.  
* **Return value** – Format name in lower‑case; returns `null` if the file is not a recognized image.  
* **Throws** – Same exceptions as `IsSupportedImageFile` for path validation and file access.

### ValidateImageFile
```csharp
public static bool ValidateImageFile(string filePath);
```
* **Purpose** – Performs a deeper validation than extension checking by reading the file header and confirming that the image data is structurally sound (e.g., correct magic numbers, plausible dimensions).  
* **Parameters** – `filePath`: Path to the file to validate.  
* **Return value** – `true` if the file is a valid image; otherwise `false`.  
* **Throws** –  
  * `ArgumentNullException` / `ArgumentException` for invalid `filePath`.  
  * `FileNotFoundException` if the file is missing.  
  * `UnauthorizedAccessException` if the file cannot be opened for reading.  
  * `IOException` if an I/O error occurs during reading.  
  * `NotSupportedException` if the file format is recognized but the library cannot validate it (e.g., unsupported subtype).

### GetImageFileSize
```csharp
public static long GetImageFileSize(string filePath);
```
* **Purpose** – Retrieves the size of the image file in bytes.  
* **Parameters** – `filePath`: Path to the image file.  
* **Return value** – File size as a signed 64‑bit integer.  
* **Throws** –  
  * `ArgumentNullException` / `ArgumentException` for invalid `filePath`.  
  * `FileNotFoundException` if the file does not exist.  
  * `UnauthorizedAccessException` if access is denied.  
  * `IOException` for other I/O problems.

### FormatFileSize
```csharp
public static string FormatFileSize(long sizeInBytes);
```
* **Purpose** – Converts a raw byte count into a human‑readable string using appropriate units (B, KB, MB, GB, TB) with one decimal place.  
* **Parameters** – `sizeInBytes`: The size to format; negative values are treated as zero.  
* **Return value** – Formatted size string (e.g., "1.2 MB").  
* **Throws** – None; the method is pure and does not access the filesystem.

### GenerateOutputFilename
```csharp
public static string GenerateOutputFilename(
    string inputFilePath,
    string outputDirectory,
    string prefix = null,
    string suffix = null);
```
* **Purpose** – Constructs a safe output filename based on the input file’s name, optionally adding a prefix and/or suffix, and placing it in the specified directory. The method ensures the filename does not contain invalid characters and preserves the original extension.  
* **Parameters** –  
  * `inputFilePath`: Path to the source image; used to extract the base name and extension.  
  * `outputDirectory`: Directory where the output file will be placed.  
  * `prefix`: Optional string to prepend to the base name (may be `null` or empty).  
  * `suffix`: Optional string to append to the base name before the extension (may be `null` or empty).  
* **Return value** – Full path to the generated output file.  
* **Throws** –  
  * `ArgumentNullException` if `inputFilePath` or `outputDirectory` is `null`.  
  * `ArgumentException` if either path is empty/whitespace or contains invalid path characters.  
  * `DirectoryNotFoundException` if `outputDirectory` does not exist and cannot be created (see `EnsureOutputDirectory`).  
  * `IOException` if the resulting path would exceed system limits.

### EnsureOutputDirectory
```csharp
public static bool EnsureOutputDirectory(string directoryPath);
```
* **Purpose** – Verifies that the directory exists; if it does not, attempts to create it (including any necessary parent directories).  
* **Parameters** – `directoryPath`: Path of the directory to ensure.  
* **Return value** – `true` if the directory exists after the call; `false` if creation failed (e.g., due to insufficient permissions).  
* **Throws** –  
  * `ArgumentNullException` if `directoryPath` is `null`.  
  * `ArgumentException` if the path is empty/whitespace or contains invalid characters.  
  * `UnauthorizedAccessException` if the user lacks rights to create the directory.  
  * `IOException` for other I/O failures (e.g., disk full).

### CalculateAspectRatio
```csharp
public static double CalculateAspectRatio(int width, int height);
```
* **Purpose** – Computes the aspect ratio (width ÷ height) of an image or rectangle.  
* **Parameters** –  
  * `width`: Width in pixels; must be greater than zero.  
  * `height`: Height in pixels; must be greater than zero.  
* **Return value** – Aspect ratio as a `double`.  
* **Throws** –  
  * `ArgumentOutOfRangeException` if either `width` or `height` is less than or equal to zero.

### CalculateProportionalSize
```csharp
public static (int width, int height) CalculateProportionalSize(
    int originalWidth,
    int originalHeight,
    int targetWidth = 0,
    int targetHeight = 0);
```
* **Purpose** – Calculates new dimensions that preserve the original aspect ratio while fitting within optional target constraints. If only one target dimension is supplied (non‑zero), the other is derived proportionally; if both are supplied, the image is scaled to the largest size that fits inside both bounds.  
* **Parameters** –  
  * `originalWidth`: Original width (> 0).  
  * `originalHeight`: Original height (> 0).  
  * `targetWidth`: Desired maximum width; set to 0 to ignore width constraint.  
  * `targetHeight`: Desired maximum height; set to 0 to ignore height constraint.  
* **Return value** – A tuple `(width, height)` representing the scaled dimensions.  
* **Throws** –  
  * `ArgumentOutOfRangeException` if `originalWidth` or `originalHeight` ≤ 0.  
  * `ArgumentOutOfRangeException` if either target dimension is negative.

### GetDirectoryImageSize
```csharp
public static long GetDirectoryImageSize(string directoryPath);
```
* **Purpose** – Recursively sums the file sizes of all files within `directoryPath` that are identified as supported images by `IsSupportedImageFile`.  
* **Parameters** – `directoryPath`: Root directory to scan.  
* **Return value** – Total size in bytes of all image files found.  
* **Throws** –  
  * `ArgumentNullException` if `directoryPath` is `null`.  
  * `ArgumentException` if the path is empty/whitespace.  
  * `DirectoryNotFoundException` if the directory does not exist.  
  * `UnauthorizedAccessException` if access to any sub‑directory or file is denied.  
  * `IOException` for I/O errors during traversal.

### SafeDeleteImage
```csharp
public static bool SafeDeleteImage(string filePath);
```
* **Purpose** – Attempts to delete the specified image file after performing a basic safety check (verifies the file exists and is not read‑only). The method does not throw on failure; instead it returns a boolean indicating success.  
* **Parameters** – `filePath`: Path to the image file to delete.  
* **Return value** – `true` if the file was deleted; `false` if the file could not be found, was read‑only, or deletion failed for any other reason.  
* **Throws** –  
  * `ArgumentNullException` if `filePath` is `null`.  
  * `ArgumentException` if `filePath` is empty/whitespace.  
  * No other exceptions are thrown; all filesystem errors are caught internally and result in a `false` return value.

### GetMimeType
```csharp
public static string GetMimeType(string filePath);
```
* **Purpose** – Returns the MIME type string associated with the image file (e.g., "image/jpeg", "image/png") based on its format. Returns `null` if the file is not a recognized image.  
* **Parameters** – `filePath`: Path to the image file.  
* **Return value** – MIME type string or `null`.  
* **Throws** – Same exceptions as `IsSupportedImageFile` for path validation and file access.

## Usage

### Example 1: Validating and preparing an output file
```csharp
string input = @"C:\Photos\input.jpg";
string outputDir = @"C:\Photos\Processed";

if (!ImageUtilities.IsSupportedImageFile(input))
{
    throw new InvalidOperationException("The source file is not a supported image.");
}

// Ensure the output folder exists
if (!ImageUtilities.EnsureOutputDirectory(outputDir))
{
    throw new IOException("Unable to create output directory.");
}

// Generate a safe output filename with a prefix
string output = ImageUtilities.GenerateOutputFilename(
    inputFilePath: input,
    outputDirectory: outputDir,
    prefix: "thumb_");

// Get the size of the source image for logging
long size = ImageUtilities.GetImageFileSize(input);
Console.WriteLine($"Processing {ImageUtilities.FormatFileSize(size)} image...");

// Perform some image‑processing work (omitted)
// ...

// Save the result to `output` (omitted)
```

### Example 2: Batch size reporting and cleanup
```csharp
string folder = @"D:\Archive\RawImages";

long totalSize = ImageUtilities.GetDirectoryImageSize(folder);
Console.WriteLine($"Total image data in {folder}: {ImageUtilities.FormatFileSize(totalSize)}");

// Iterate over files and delete any that are corrupt or zero‑length
foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
{
    if (!ImageUtilities.ValidateImageFile(file))
    {
        // Log the issue
        Console.WriteLine($"Invalid image detected: {file}");
        // Attempt safe deletion; ignore the result as we already logged the problem
        ImageUtilities.SafeDeleteImage(file);
    }
}
```

## Notes

* **Edge cases** –  
  * Methods that accept file paths treat an empty string or whitespace‑only input as invalid and throw `ArgumentException`.  
  * `GetImageFormat` and `GetMimeType` return `null` when the file exists but does not match any known image signature; callers should check for `null` before using the result.  
  * `CalculateProportionalSize` treats a target dimension of zero as “no constraint”; supplying zero for both dimensions returns the original size unchanged.  
  * `SafeDeleteImage` will not delete a file that is marked read‑only; it returns `false` in that scenario without throwing.  
  * Recursive operations (`GetDirectoryImageSize`) may encounter reparse points or junction loops; the implementation follows the default behavior of `Directory.EnumerateFiles` and does not attempt to detect cycles.

* **Thread‑safety** –  
  * All members are static and operate only on their input parameters; they do not rely on mutable static state. Consequently, the type is thread‑safe for concurrent calls as long as the caller does not pass mutable objects that are modified elsewhere during execution.  
  * Filesystem‑based methods (`IsSupportedImageFile`, `ValidateImageFile`, `GetImageFileSize`, etc.) are subject to the usual race conditions inherent to file I/O (e.g., a file may be deleted between a validation call and a subsequent operation). Callers should handle such scenarios appropriately in their own synchronization logic if required.
