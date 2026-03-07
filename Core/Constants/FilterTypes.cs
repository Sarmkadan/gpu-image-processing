#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core.Constants
{
    /// <summary>
    /// Enumeration of supported image filter types
    /// </summary>
    public enum FilterType
    {
        /// <summary>Gaussian blur filter</summary>
        Gaussian = 0,

        /// <summary>Bilateral filter for edge-preserving smoothing</summary>
        Bilateral = 1,

        /// <summary>Median filter for noise reduction</summary>
        Median = 2,

        /// <summary>Threshold filter for binary conversion</summary>
        Threshold = 3,

        /// <summary>Sobel edge detection filter</summary>
        Sobel = 4,

        /// <summary>Canny edge detection filter</summary>
        Canny = 5,

        /// <summary>Morphological opening operation</summary>
        MorphOpen = 6,

        /// <summary>Morphological closing operation</summary>
        MorphClose = 7,

        /// <summary>Laplacian filter for edge detection</summary>
        Laplacian = 8,

        /// <summary>Custom user-defined filter</summary>
        Custom = 9,

        /// <summary>Sharpen filter</summary>
        Sharpen = 10,

        /// <summary>Unsharp mask filter</summary>
        UnsharpMask = 11
    }
}
