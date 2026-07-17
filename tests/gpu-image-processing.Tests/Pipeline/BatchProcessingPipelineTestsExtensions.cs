using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Tests.Pipeline
{
    /// <summary>
    /// Provides extension methods that simplify invoking the public test methods of <see cref="BatchProcessingPipelineTests"/>.
    /// </summary>
    public static class BatchProcessingPipelineTestsExtensions
    {
        /// <summary>
        /// Executes the <c>RunAsync_NullBatch_ThrowsArgumentNullException</c> test on the supplied instance.
        /// </summary>
        /// <param name="test">The <see cref="BatchProcessingPipelineTests"/> instance.</param>
        /// <returns>A <see cref="Task"/> that completes when the underlying test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <see langword="null"/>.</exception>
        public static async Task VerifyNullBatchThrowsAsync(this BatchProcessingPipelineTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            await test.RunAsync_NullBatch_ThrowsArgumentNullException();
        }

        /// <summary>
        /// Executes the <c>RunAsync_AllImagesSucceed_ReturnsFullSuccessResult</c> test on the supplied instance.
        /// </summary>
        /// <param name="test">The <see cref="BatchProcessingPipelineTests"/> instance.</param>
        /// <returns>A <see cref="Task"/> that completes when the underlying test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <see langword="null"/>.</exception>
        public static async Task VerifyAllImagesSucceedAsync(this BatchProcessingPipelineTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            await test.RunAsync_AllImagesSucceed_ReturnsFullSuccessResult();
        }

        /// <summary>
        /// Executes the <c>RunAsync_PartialFailure_ReturnsCorrectCounts</c> test on the supplied instance.
        /// </summary>
        /// <param name="test">The <see cref="BatchProcessingPipelineTests"/> instance.</param>
        /// <returns>A <see cref="Task"/> that completes when the underlying test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <see langword="null"/>.</exception>
        public static async Task VerifyPartialFailureAsync(this BatchProcessingPipelineTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            await test.RunAsync_PartialFailure_ReturnsCorrectCounts();
        }

        /// <summary>
        /// Executes the <c>RunAsync_RetriesFailedImageUpToMaxRetries</c> test on the supplied instance.
        /// </summary>
        /// <param name="test">The <see cref="BatchProcessingPipelineTests"/> instance.</param>
        /// <returns>A <see cref="Task"/> that completes when the underlying test finishes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <see langword="null"/>.</exception>
        public static async Task VerifyRetriesFailedImageAsync(this BatchProcessingPipelineTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            await test.RunAsync_RetriesFailedImageUpToMaxRetries();
        }
    }
}