// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core.Constants
{
    /// <summary>
    /// Enumeration of supported image transformation types
    /// </summary>
    public enum TransformType
    {
        /// <summary>Rotation transformation</summary>
        Rotate = 0,

        /// <summary>Resize/scale transformation</summary>
        Resize = 1,

        /// <summary>Color space conversion</summary>
        ColorSpace = 2,

        /// <summary>Normalize pixel values</summary>
        Normalize = 3,

        /// <summary>Flip horizontally</summary>
        FlipHorizontal = 4,

        /// <summary>Flip vertically</summary>
        FlipVertical = 5,

        /// <summary>Crop image region</summary>
        Crop = 6,

        /// <summary>Brightness adjustment</summary>
        Brightness = 7,

        /// <summary>Contrast adjustment</summary>
        Contrast = 8,

        /// <summary>Gamma correction</summary>
        Gamma = 9,

        /// <summary>Histogram equalization</summary>
        HistogramEqualize = 10,

        /// <summary>Perspective transformation</summary>
        Perspective = 11
    }
}
