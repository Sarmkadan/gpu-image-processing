// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Supported image formats.
/// </summary>
public enum ImageFormat
{
    Unknown = 0,
    Jpeg = 1,
    Png = 2,
    Bmp = 3,
    Tiff = 4,
    WebP = 5,
    Raw = 6
}

/// <summary>
/// Available filter types for image processing.
/// </summary>
public enum FilterType
{
    None = 0,
    Grayscale = 1,
    Blur = 2,
    Sharpen = 3,
    EdgeDetection = 4,
    ColorCorrection = 5,
    Rotation = 6,
    Scaling = 7,
    Histogram = 8,
    Threshold = 9,
    GaussianBlur = 10,
    MedianFilter = 11,
    Emboss = 12,
    Sobel = 13
}

/// <summary>
/// Processing status of an image.
/// </summary>
public enum ProcessingStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Queued = 5
}

/// <summary>
/// GPU device types supported.
/// </summary>
public enum GpuDeviceType
{
    Unknown = 0,
    Cpu = 1,
    Gpu = 2,
    Accelerator = 3,
    Custom = 4
}

/// <summary>
/// Color space representation.
/// </summary>
public enum ColorSpace
{
    Unknown = 0,
    Rgb = 1,
    Rgba = 2,
    Bgra = 3,
    Grayscale = 4,
    Hsv = 5,
    Ycbcr = 6
}

/// <summary>
/// Interpolation method for scaling.
/// </summary>
public enum InterpolationMethod
{
    Nearest = 0,
    Linear = 1,
    Cubic = 2,
    Lanczos = 3
}

/// <summary>
/// Edge detection algorithms.
/// </summary>
public enum EdgeDetectionAlgorithm
{
    Sobel = 0,
    Laplacian = 1,
    Canny = 2,
    Roberts = 3
}
