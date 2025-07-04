# CpuImageProcessor

A utility class for performing common image processing operations on the CPU without GPU acceleration. It provides methods for resizing, color space conversion, blurring, edge detection, and other filters, returning results as `System.Drawing.Image` objects or raw byte arrays. The class is designed for scenarios where GPU resources are unavailable or when small images and simple operations make CPU processing preferable.

## API

### `public CpuImageProcessor()`
Initializes a new instance of the `CpuImageProcessor` class. No external resources are allocated during construction.

### `public bool CanProcess`
Gets a value indicating whether the current system supports the required image processing operations.

- **Return value**: `true` if the system can perform the operations; otherwise, `false`.
- **Remarks**: This property may return `false` on platforms lacking certain system libraries or when critical dependencies are missing.

### `public async Task ApplyFilterAsync(Image input, Func<Image, Image> filter)`
Applies a user-defined image filter asynchronously to the provided input image.

- **Parameters**:
  - `input`: The source image to process.
  - `filter`: A delegate that defines the transformation to apply.
- **Return value**: A `Task` representing the asynchronous operation.
- **Remarks**:
  - The method does not validate `input` or `filter` for `null`.
  - Exceptions thrown by `filter` will propagate to the caller.
  - This method is intended for simple, non-blocking filter application.

### `public Image Resize(Image input, int width, int height)`
Resizes the input image to the specified dimensions using nearest-neighbor interpolation.

- **Parameters**:
  - `input`: The source image to resize.
  - `width`: The target width in pixels.
  - `height`: The target height in pixels.
- **Return value**: A new `Image` instance with the specified dimensions.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `width` or `height` is less than or equal to zero.
- **Remarks**: The original aspect ratio is not preserved; the image is stretched to fit the new dimensions.

### `public Image ToGrayscale(Image input)`
Converts the input image to grayscale using standard luminance averaging.

- **Parameters**:
  - `input`: The source image to convert.
- **Return value**: A new `Image` in grayscale.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
- **Remarks**: The output image retains the same dimensions as the input.

### `public Image Blur(Image input, int radius)`
Applies a simple box blur to the input image using the specified radius.

- **Parameters**:
  - `input`: The source image to blur.
  - `radius`: The blur radius in pixels (must be positive).
- **Return value**: A new `Image` with the blur applied.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `radius` is less than or equal to zero.
- **Remarks**: Larger radii produce more pronounced blurring but increase processing time.

### `public static byte[] Grayscale(byte[] input, int width, int height, int channels)`
Converts a raw RGB or RGBA byte array to a grayscale byte array in-place.

- **Parameters**:
  - `input`: The source pixel data (RGB or RGBA).
  - `width`: The image width in pixels.
  - `height`: The image height in pixels.
  - `channels`: The number of color channels per pixel (3 for RGB, 4 for RGBA).
- **Return value**: A new `byte[]` containing the grayscale pixel data.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `width`, `height`, or `channels` is invalid.
- **Remarks**: The output array length equals `width * height * 1`. Alpha values (if present) are discarded.

### `public static byte[] Threshold(byte[] input, int width, int height, int channels, byte threshold)`
Applies a binary threshold to the input image, converting it to black and white.

- **Parameters**:
  - `input`: The source pixel data (RGB or RGBA).
  - `width`: The image width in pixels.
  - `height`: The image height in pixels.
  - `channels`: The number of color channels per pixel (3 for RGB, 4 for RGBA).
  - `threshold`: The grayscale threshold value (0–255).
- **Return value**: A new `byte[]` containing the thresholded pixel data.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `width`, `height`, `channels`, or `threshold` is invalid.
- **Remarks**: Pixels above the threshold are set to white (255); others are set to black (0). Alpha values are ignored.

### `public static byte[] BoxBlur(byte[] input, int width, int height, int channels, int radius)`
Applies a box blur filter to the input image using the specified radius.

- **Parameters**:
  - `input`: The source pixel data (RGB or RGBA).
  - `width`: The image width in pixels.
  - `height`: The image height in pixels.
  - `channels`: The number of color channels per pixel (3 for RGB, 4 for RGBA).
  - `radius`: The blur radius in pixels (must be positive).
- **Return value**: A new `byte[]` containing the blurred pixel data.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `width`, `height`, `channels`, or `radius` is invalid.
- **Remarks**: The operation is performed on each color channel independently. Larger radii increase blur intensity.

### `public static byte[] Convolve(byte[] input, int width, int height, int channels, float[] kernel)`
Applies a custom convolution kernel to the input image.

- **Parameters**:
  - `input`: The source pixel data (RGB or RGBA).
  - `width`: The image width in pixels.
  - `height`: The image height in pixels.
  - `channels`: The number of color channels per pixel (3 for RGB, 4 for RGBA).
  - `kernel`: A square convolution kernel (e.g., 3x3) flattened into a `float[]`.
- **Return value**: A new `byte[]` containing the convolved pixel data.
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` or `kernel` is `null`.
  - Throws `ArgumentException` if `kernel` is not a square matrix or has zero size.
  - Throws `ArgumentOutOfRangeException` if `width`, `height`, or `channels` is invalid.
- **Remarks**: The kernel must have odd dimensions (e.g., 3x3). Edge pixels are clamped to the image boundary.

### `public static byte[] Sobel(byte[] input, int width, int height, int channels)`
Applies the Sobel edge detection filter to the input image.

- **Parameters**:
  - `input`: The source pixel data (RGB or RGBA).
  - `width`: The image width in pixels.
  - `height`: The image height in pixels.
  - `channels`: The number of color channels per pixel (3 for RGB, 4 for RGBA).
- **Return value**: A new `byte[]` containing the edge-detected pixel data (grayscale).
- **Exceptions**:
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `width`, `height`, or `channels` is invalid.
- **Remarks**: The output is a single-channel grayscale image. Alpha values are ignored.

## Usage

### Example 1: Basic Filter Pipeline
