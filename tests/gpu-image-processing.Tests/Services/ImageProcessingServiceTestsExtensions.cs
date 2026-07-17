using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Tests.Services
{
    /// <summary>
    /// Provides extension methods that simplify invoking the test methods of <see cref="ImageProcessingServiceTests"/>.
    /// These extension methods act as convenience wrappers around the actual test methods, enabling fluent
    /// test execution patterns in integration and end-to-end test scenarios.
    /// </summary>
    public static class ImageProcessingServiceTestsExtensions
    {
        /// <summary>
        /// Executes the <c>ProcessImageAsync_ImageNotFound_ThrowsInvalidImageException</c> test.
        /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> throws an <see cref="InvalidImageException"/>
        /// when the target image is not found in the repository.
        /// </summary>
        /// <param name="tests">The test instance containing the mocked dependencies.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunProcessImageAsync_ImageNotFound(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.ProcessImageAsync_ImageNotFound_ThrowsInvalidImageException();
        }

        /// <summary>
        /// Executes the <c>ProcessImageAsync_InvalidImage_ThrowsInvalidImageException</c> test.
        /// Validates that <see cref="ImageProcessingService.ProcessImageAsync"/> throws an <see cref="InvalidImageException"/>
        /// when the target image exists but is considered invalid (e.g., zero width).
        /// </summary>
        /// <param name="tests">The test instance containing the mocked dependencies.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunProcessImageAsync_InvalidImage(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.ProcessImageAsync_InvalidImage_ThrowsInvalidImageException();
        }

        /// <summary>
        /// Executes the <c>GetProcessingResultAsync_ExistingResults_ReturnsLatestResult</c> test.
        /// Validates that <see cref="ImageProcessingService.GetProcessingResultAsync"/> returns the most recent
        /// <see cref="ProcessingResult"/> for a given image ID when multiple results exist.
        /// </summary>
        /// <param name="tests">The test instance containing the mocked dependencies.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunGetProcessingResultAsync_ExistingResults(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetProcessingResultAsync_ExistingResults_ReturnsLatestResult();
        }

        /// <summary>
        /// Executes the <c>GetStatisticsAsync_ReturnsCorrectMetrics</c> test.
        /// Validates that <see cref="ImageProcessingService.GetStatisticsAsync"/> calculates and returns the
        /// correct processing metrics, including success rates and average processing times.
        /// </summary>
        /// <param name="tests">The test instance containing the mocked dependencies.</param>
        /// <returns>A task that completes when the test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static Task RunGetStatisticsAsync_ReturnsCorrectMetrics(this ImageProcessingServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetStatisticsAsync_ReturnsCorrectMetrics();
        }
    }
}
