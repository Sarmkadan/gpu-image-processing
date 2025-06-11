# ImageUtilitiesBenchmarks

Utility class providing benchmarking methods for common image processing operations, including file format detection, MIME type resolution, and size calculations.

## API

### `IsSupportedImageFile_Jpeg`
Determines whether a given file path represents a JPEG image based on its extension.

- **Return value**: `true` if the file extension is `.jpg` or `.jpeg` (case-insensitive); otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the input path is `null`.

### `IsSupportedImageFile_WebP`
Determines whether a given file path represents a WebP image based on its extension.

- **Return value**: `true` if the file extension is `.webp` (case-insensitive); otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the input path is `null`.

### `IsSupportedImageFile_Unsupported`
Determines whether a given file path represents an unsupported image format based on its extension.

- **Return value**: `true` if the file extension is not recognized as a supported format (e.g., `.tiff`, `.bmp`, `.gif`); otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the input path is `null`.

### `FormatFileSize_Kilobytes`
Formats a file size in bytes into a human-readable string with kilobytes as the unit.

- **Return value**: A string representing the size rounded to two decimal places followed by "KB" (e.g., `"12.34KB"`).
- **Exceptions**: Throws `ArgumentOutOfRangeException` if the input size is negative.

### `FormatFileSize_Megabytes`
Formats a file size in bytes into a human-readable string with megabytes as the unit.

- **Return value**: A string representing the size rounded to two decimal places followed by "MB" (e.g., `"5.67MB"`).
- **Exceptions**: Throws `ArgumentOutOfRangeException` if the input size is negative.

### `FormatFileSize_Gigabytes`
Formats a file size in bytes into a human-readable string with gigabytes as the unit.

- **Return value**: A string representing the size rounded to two decimal places followed by "GB" (e.g., `"0.12GB"`).
- **Exceptions**: Throws `ArgumentOutOfRangeException` if the input size is negative.

### `GetMimeType_Jpeg`
Retrieves the MIME type for a JPEG image.

- **Return value**: The string `"image/jpeg"` if the input is recognized as a JPEG; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if the input is `null`.

### `GetMimeType_Png`
Retrieves the MIME type for a PNG image.

- **Return value**: The string `"image/png"` if the input is recognized as a PNG; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if the input is `null`.

### `GetImageFormat_Tiff`
Retrieves the image format identifier for a TIFF image.

- **Return value**: The string `"tiff"` if the input is recognized as a TIFF; otherwise, `null`.
- **Exceptions**: Throws `ArgumentNullException` if the input is `null`.

### `CalculateProportionalSize_2x`
Calculates the proportional dimensions of an image scaled by a factor of 2 while maintaining aspect ratio.

- **Return value**: A tuple `(int width, int height)` representing the scaled dimensions.
- **Parameters**: The input dimensions are inferred from the method name and not explicitly passed.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if either input dimension is zero or negative.

## Usage
