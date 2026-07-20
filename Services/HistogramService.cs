#nullable enable
// =============================================================================
// Author: 
// =============================================================================

using System;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Service for computing histograms and cumulative distribution functions.
    /// </summary>
    public class HistogramService
    {
        /// <summary>
        /// Computes the histogram of a grayscale image.
        /// </summary>
        /// <param name="grayscalePixels">Grayscale pixel values.</param>
        /// <returns>Histogram bin counts.</returns>
        public int[] ComputeHistogram(byte[] grayscalePixels)
        {
            if (grayscalePixels == null || grayscalePixels.Length == 0)
                throw new ArgumentException("Grayscale pixels cannot be null or empty", nameof(grayscalePixels));

            var histogram = new int[256];

            foreach (var pixel in grayscalePixels)
            {
                histogram[pixel]++;
            }

            return histogram;
        }

        /// <summary>
        /// Computes the cumulative distribution function from a histogram.
        /// </summary>
        /// <param name="histogram">Histogram bin counts.</param>
        /// <returns>Cumulative distribution function values.</returns>
        public double[] ComputeCdf(int[] histogram)
        {
            if (histogram == null || histogram.Length == 0)
                throw new ArgumentException("Histogram cannot be null or empty", nameof(histogram));

            if (histogram.Length != 256)
                throw new ArgumentException("Histogram must have 256 bins", nameof(histogram));

            var cdf = new double[256];
            var totalPixels = 0;

            foreach (var bin in histogram)
            {
                totalPixels += bin;
            }

            double cumulativeSum = 0;

            for (int i = 0; i < 256; i++)
            {
                cumulativeSum += histogram[i];
                cdf[i] = cumulativeSum / totalPixels;
            }

            return cdf;
        }
    }
}
