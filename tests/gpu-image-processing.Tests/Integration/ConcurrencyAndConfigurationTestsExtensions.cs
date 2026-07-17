#nullable enable
using System.Threading.Tasks;
using System;

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Provides extension methods for <see cref="ConcurrencyAndConfigurationTests"/> to facilitate running test suites.
/// </summary>
public static class ConcurrencyAndConfigurationTestsExtensions
{
    /// <summary>
    /// Executes the core concurrency test suite.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task RunConcurrencySuiteAsync(this ConcurrencyAndConfigurationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.ConcurrentImageProcessing_MultipleThreads_CompletesSuccessfully();
        await tests.PerformanceMetrics_UnderConcurrentLoad_CalculatedCorrectly();
    }

    /// <summary>
    /// Executes the configuration and validation test suite.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task RunConfigurationSuiteAsync(this ConcurrencyAndConfigurationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.ImageDimensions_VariousConfigurations_AllProcessCorrectly();
        await tests.FilterConfiguration_InactiveFilter_RejectedDuringApplication();
    }

    /// <summary>
    /// Executes the GPU resource management test suite.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task RunGpuSuiteAsync(this ConcurrencyAndConfigurationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.GpuMemory_StressTest_AllocateAndDeallocateMultipleTimes();
        tests.GpuDeviceSelection_MultipleDevices_SelectsBestOne();
    }
}