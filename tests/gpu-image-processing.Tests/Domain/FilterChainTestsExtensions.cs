using System;

namespace GpuImageProcessing.Tests.Domain
{
    /// <summary>
    /// Extension methods that make it easier to compose <see cref="FilterChainTests"/> test scenarios.
    /// </summary>
    public static class FilterChainTestsExtensions
    {
        /// <summary>
        /// Executes the default add‑step test and returns the same <see cref="FilterChainTests"/> instance
        /// to enable fluent chaining.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The original <see cref="FilterChainTests"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static FilterChainTests AddStepAndAssert(this FilterChainTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            test.AddStep_DefaultOrder_AppendsStepToEnd();
            return test;
        }

        /// <summary>
        /// Executes the removal‑of‑existing‑filter test and returns the same instance.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The original <see cref="FilterChainTests"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static FilterChainTests RemoveExistingStepAndAssert(this FilterChainTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            test.RemoveStep_ExistingFilter_ReturnsTrue_AndReordersRemainingSteps();
            return test;
        }

        /// <summary>
        /// Executes the removal‑of‑non‑existent‑filter test and returns the same instance.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The original <see cref="FilterChainTests"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static FilterChainTests RemoveNonExistentStepAndAssert(this FilterChainTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            test.RemoveStep_NonExistentFilter_ReturnsFalse();
            return test;
        }

        /// <summary>
        /// Executes the filter‑configuration validation test and returns the same instance.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The original <see cref="FilterChainTests"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static FilterChainTests ValidateMismatchedFilterConfiguration(this FilterChainTests test)
        {
            ArgumentNullException.ThrowIfNull(test);
            test.FilterConfiguration_Validate_ParameterWithoutMatchingType_ReturnsFalse();
            return test;
        }
    }
}
