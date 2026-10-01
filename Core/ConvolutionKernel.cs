#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Core
{
    /// <summary>
    /// Represents a 2D convolution kernel used for spatial filtering operations
    /// such as blur, sharpen, edge detection, and emboss.
    /// </summary>
    public class ConvolutionKernel
    {
        private readonly ILogger<ConvolutionKernel> _logger;
        private readonly float[] _data;

        /// <summary>
        /// Width of the kernel matrix.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the kernel matrix.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Divisor applied after convolution to normalize results.
        /// </summary>
        public float Divisor { get; private set; }

        /// <summary>
        /// Bias added after division, useful for emboss-style kernels.
        /// </summary>
        public float Bias { get; set; }

        /// <summary>
        /// Initializes a new convolution kernel with the given dimensions.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostics.</param>
        /// <param name="width">Kernel width (must be odd and >= 1).</param>
        /// <param name="height">Kernel height (must be odd and >= 1).</param>
        public ConvolutionKernel(ILogger<ConvolutionKernel> logger, int width, int height)
        {
            if (width < 1 || width % 2 == 0)
                throw new ArgumentException("Width must be a positive odd number.", nameof(width));
            if (height < 1 || height % 2 == 0)
                throw new ArgumentException("Height must be a positive odd number.", nameof(height));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Width = width;
            Height = height;
            _data = new float[width * height];
            Divisor = 1.0f;
            Bias = 0.0f;

            _logger.LogDebug("Created {W}x{H} convolution kernel", width, height);
        }

        /// <summary>
        /// Sets the kernel coefficient at the specified position.
        /// </summary>
        /// <param name="x">Column index (0-based).</param>
        /// <param name="y">Row index (0-based).</param>
        /// <param name="value">Coefficient value.</param>
        public void SetValue(int x, int y, float value)
        {
            if (x < 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));

            _data[y * Width + x] = value;
        }

        /// <summary>
        /// Gets the kernel coefficient at the specified position.
        /// </summary>
        public float GetValue(int x, int y)
        {
            if (x < 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));

            return _data[y * Width + x];
        }

        /// <summary>
        /// Returns the raw kernel data as a flat array in row-major order,
        /// suitable for uploading to a GPU constant buffer.
        /// </summary>
        public float[] ToFlatArray()
        {
            var result = new float[_data.Length];
            Array.Copy(_data, result, _data.Length);
            return result;
        }

        /// <summary>
        /// Normalizes the kernel so the sum of all coefficients equals 1.
        /// Prevents brightness shifts in blur-type kernels.
        /// </summary>
        public void Normalize()
        {
            float sum = _data.Sum();
            if (MathF.Abs(sum) < 1e-9f)
            {
                _logger.LogWarning("Cannot normalize kernel: coefficient sum is near zero");
                return;
            }

            Divisor = sum;
            _logger.LogDebug("Kernel normalized with divisor {D:F4}", Divisor);
        }

        /// <summary>
        /// Creates a standard Gaussian blur kernel of the given size.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="size">Kernel size (must be odd, e.g. 3, 5, 7).</param>
        /// <param name="sigma">Standard deviation of the Gaussian distribution.</param>
        public static ConvolutionKernel CreateGaussian(ILogger<ConvolutionKernel> logger, int size, float sigma = 1.4f)
        {
            var kernel = new ConvolutionKernel(logger, size, size);
            int half = size / 2;
            float twoSigmaSq = 2.0f * sigma * sigma;

            for (int y = -half; y <= half; y++)
            {
                for (int x = -half; x <= half; x++)
                {
                    float value = MathF.Exp(-(x * x + y * y) / twoSigmaSq) / (MathF.PI * twoSigmaSq);
                    kernel.SetValue(x + half, y + half, value);
                }
            }

            kernel.Normalize();
            logger.LogInformation("Created {S}x{S} Gaussian kernel with sigma={Sigma:F2}", size, size, sigma);
            return kernel;
        }
    }
}
