using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Tests.Services
{
    /// <summary>
    /// Provides extension methods that simplify invoking the test methods of <see cref="ImageProcessingServiceTests"/>.
    /// </summary>
    public static class ImageProcessingServiceTestsExtensions
    {
        /// <summary>
        /// Executes the <c>ProcessImageAsync_ImageNotFound_ThrowsInvalidImageException</c> test.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunProcessImageAsync_ImageNotFound(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.ProcessImageAsync_ImageNotFound_ThrowsInvalidImageException();
        }

        /// <summary>
        /// Executes the <c>ProcessImageAsync_InvalidImage_ThrowsInvalidImageException</c> test.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunProcessImageAsync_InvalidImage(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.ProcessImageAsync_InvalidImage_ThrowsInvalidImageException();
        }

        /// <summary>
        /// Executes the <c>GetProcessingResultAsync_ExistingResults_ReturnsLatestResult</c> test.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunGetProcessingResultAsync_ExistingResults(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetProcessingResultAsync_ExistingResults_ReturnsLatestResult();
        }

        /// <summary>
        /// Executes the <c>GetStatisticsAsync_ReturnsCorrectMetrics</c> test.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunGetStatisticsAsync_ReturnsCorrectMetrics(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetStatisticsAsync_ReturnsCorrectMetrics();
        }
    }
}
