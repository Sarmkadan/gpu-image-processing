using System;
using System.Diagnostics.CodeAnalysis;

namespace GpuImageProcessing.Tests.Benchmarking;

/// <summary>
/// Extension methods for <see cref="BenchmarkSuiteConfigurationTests"/>.
/// </summary>
public static class BenchmarkSuiteConfigurationTestsExtensions
{
    /// <summary>
    /// Executes all validation-related tests on the provided <see cref="BenchmarkSuiteConfigurationTests"/> instance.
    /// </summary>
    /// <param name="tests">The test instance to operate on. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static void ExecuteAllValidationTests(this BenchmarkSuiteConfigurationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.Validate_ValidDefaultConfig_ReturnsNoErrors();
        tests.Validate_BlankRunName_ReturnsNameError();
        tests.Validate_AllCategoriesDisabled_ReturnsAtLeastOneError();
    }

    /// <summary>
    /// Executes all category-related tests on the provided <see cref="BenchmarkSuiteConfigurationTests"/> instance.
    /// </summary>
    /// <param name="tests">The test instance to operate on. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static void ExecuteAllCategoryTests(this BenchmarkSuiteConfigurationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.GetEnabledCategories_DefaultConfig_ContainsFourCategories();
        tests.GetEnabledCategories_AllEnabled_ContainsFiveCategories();
    }

    /// <summary>
    /// Executes all preset-related tests on the provided <see cref="BenchmarkSuiteConfigurationTests"/> instance.
    /// </summary>
    /// <param name="tests">The test instance to operate on. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static void ExecuteAllPresetTests(this BenchmarkSuiteConfigurationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.ForCi_Preset_HasQuickAccuracyAndThreeCategories();
        tests.ForRelease_Preset_HasThoroughAccuracyAndAllCategories();
        tests.ForRelease_Preset_IsValid();
        tests.ForCi_Preset_IsValid();
    }
}
