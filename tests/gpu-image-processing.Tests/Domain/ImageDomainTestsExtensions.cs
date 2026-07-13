#nullable enable

using System;
using GpuImageProcessing.Tests.Domain;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Provides extension methods for the <see cref="ImageDomainTests"/> class.
/// </summary>
public static class ImageDomainTestsExtensions
{
    /// <summary>
    /// Executes all validation-related tests for the <see cref="ImageDomainTests"/> class.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void RunValidationTests(this ImageDomainTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.Validate_ValidImage_ReturnsTrue();
        tests.Validate_WidthBelowMinimum_ReturnsFalse();
        tests.Validate_UnsupportedBitsPerPixel_ReturnsFalse();
    }

    /// <summary>
    /// Executes all processing and batch-related tests for the <see cref="ImageDomainTests"/> class.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
    public static void RunProcessingAndBatchTests(this ImageDomainTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.CalculatePixelDataSize_32BitsPerPixel_ReturnsWidthTimesHeightTimesFour();
        tests.MarkAsCompleted_SetsCompletedStatusAndOutputPath();
        tests.ImageBatch_GetProgressPercentage_PartialCompletion_ReturnsCorrectValue();
        tests.ProcessingResult_GetTotalFilterExecutionTime_SumsAllAppliedFilters();
    }
}
