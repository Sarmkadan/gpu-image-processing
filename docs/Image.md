# Image

Represents a bitmap image with metadata and processing state, optimized for GPU-based image processing pipelines. Encapsulates raw pixel data, color space information, and processing history while providing validation and size calculation utilities.

## API

### `public Guid Id`
Unique identifier for the image instance. Assigned at construction and immutable thereafter.

### `public string FilePath`
Absolute or relative filesystem path where the image was loaded from or will be saved to. May be `null` for in-memory images.

### `public string FileName`
Name of the file without directory components. Derived from `FilePath` if available.

### `public ImageFormat Format`
Format of the image (e.g., PNG, JPEG, BMP). Determines encoding/decoding behavior and metadata expectations.

### `public ColorSpace ColorSpace`
Color representation model (e.g., sRGB, AdobeRGB, Grayscale). Affects pixel interpretation and processing algorithms.

### `public int Width`
Horizontal dimension in pixels. Must be positive; enforced by constructor and `Validate`.

### `public int Height`
Vertical dimension in pixels. Must be positive; enforced by constructor and `Validate`.

### `public int Channels`
Number of color channels per pixel (e.g., 3 for RGB, 4 for RGBA, 1 for Grayscale). Must match expectations of `Format` and `ColorSpace`.

### `public int BitsPerPixel`
Total bits used to represent a single pixel (e.g., 24 for 8-bit RGB, 32 for 8-bit RGBA). Calculated as `Channels * 8` unless otherwise specified.

### `public long FileSizeBytes`
Size of the original file in bytes, if loaded from disk. Zero for in-memory images.

### `public byte[]? PixelData`
Raw pixel buffer in row-major order. May be `null` for uninitialized or externally managed images. Layout and interpretation depend on `Format`, `ColorSpace`, and `Channels`.

### `public ProcessingStatus Status`
Current processing state (e.g., `Unprocessed`, `InProgress`, `Processed`, `Failed`). Updated during pipeline operations.

### `public DateTime CreatedAt`
Timestamp of image creation or loading. Set once at construction and immutable.

### `public DateTime ModifiedAt`
Timestamp of last modification (metadata or pixel data). Updated automatically on relevant property changes.

### `public string? ProcessedOutputPath`
Filesystem path where processed output was saved. `null` if not yet processed or saved.

### `public Dictionary<string, object> Metadata`
Key-value store for arbitrary image metadata (e.g., EXIF tags, processing parameters). Case-insensitive keys; values may be strings, numbers, or arrays.

### `public Image(int width, int height, int channels)`
Constructs an in-memory image with the specified dimensions and channel count. Allocates `PixelData` as a zero-initialized byte array of size `width * height * channels`.

- **Parameters**:
  - `width`: Horizontal pixel count (≥ 1).
  - `height`: Vertical pixel count (≥ 1).
  - `channels`: Number of channels per pixel (≥ 1).
- **Throws**: `ArgumentOutOfRangeException` if any parameter is ≤ 0.

### `public bool Validate()`
Validates internal consistency of the image state. Checks:
- `Width`, `Height`, `Channels` > 0.
- `PixelData` length matches `Width * Height * Channels`.
- `Format` and `ColorSpace` are valid enum values.
- `BitsPerPixel` equals `Channels * 8` unless overridden by format-specific rules.

- **Returns**: `true` if valid; `false` otherwise.
- **Throws**: Never.

### `public long CalculatePixelDataSize()`
Computes the expected byte size of the pixel buffer based on `Width`, `Height`, and `Channels`.

- **Returns**: Total bytes required for `PixelData` (i.e., `Width * Height * Channels`).
- **Throws**: Never.

## Usage
