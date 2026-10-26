# HistogramService

The `HistogramService` computes intensity histograms and cumulative distribution functions (CDFs) for 8-bit grayscale pixel data within the `gpu-image-processing` project. It lives in the `GpuImageProcessing.Services` namespace and is a stateless CPU component: each call works only on the arrays passed to it, so a single instance can be shared freely. Typical uses are image analysis (contrast and brightness inspection) and histogram equalization, where the CDF is turned into a per-pixel lookup table.

## API

### Constructors

#### `public HistogramService()`
Implicit parameterless constructor. The service holds no state and has no dependencies.

### Histogram and CDF

#### `public int[] ComputeHistogram(byte[] grayscalePixels)`
Counts how many pixels carry each intensity value from 0 to 255.
*   **Parameters**:
    *   `grayscalePixels`: Grayscale pixel values, one byte per pixel.
*   **Return Value**: A new `int[256]` array where index `i` holds the number of pixels with value `i`. The counts sum to `grayscalePixels.Length`.
*   **Exceptions**:
    *   `ArgumentException` (parameter `grayscalePixels`) if the array is `null` or empty. A `null` argument is reported as `ArgumentException`, not `ArgumentNullException`.

#### `public double[] ComputeCdf(int[] histogram)`
Converts histogram counts into a cumulative distribution. Element `i` is the fraction of all pixels whose value is less than or equal to `i`.
*   **Parameters**:
    *   `histogram`: Histogram bin counts. Must contain exactly 256 entries, as produced by `ComputeHistogram`.
*   **Return Value**: A new `double[256]` array. For a non-empty image the values are non-decreasing and the last element is `1.0`.
*   **Exceptions**:
    *   `ArgumentException` (parameter `histogram`) if the array is `null` or empty.
    *   `ArgumentException` (parameter `histogram`) if the length is not 256.
    *   If every bin is zero, the total pixel count is zero and the result contains `NaN` values. No exception is thrown in this case.

## Usage Example

```csharp
using GpuImageProcessing.Services;

var histogramService = new HistogramService();

// Grayscale pixels, e.g. extracted from an 8-bit image buffer
byte[] pixels = { 0, 0, 128, 255, 255, 255 };

// Count intensity values
int[] histogram = histogramService.ComputeHistogram(pixels);
Console.WriteLine($"Pixels with value 255: {histogram[255]}"); // 3

// Cumulative distribution
double[] cdf = histogramService.ComputeCdf(histogram);
Console.WriteLine($"Fraction of pixels <= 128: {cdf[128]:P0}"); // 50%

// Histogram equalization lookup table: maps each input value to an output value
byte[] lut = new byte[256];
for (int i = 0; i < 256; i++)
{
    lut[i] = (byte)Math.Round(cdf[i] * 255);
}
```

## Notes

*   Both methods allocate a new result array on every call and do not modify their input.
*   `ComputeCdf` expects counts from a single 256-bin histogram. Passing a histogram with a different bin count is rejected rather than rescaled.
