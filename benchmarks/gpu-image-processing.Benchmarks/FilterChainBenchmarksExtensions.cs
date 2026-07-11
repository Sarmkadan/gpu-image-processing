using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Benchmarks
{
	public static class FilterChainBenchmarksExtensions
	{
		/// <summary>
		/// Adds a sequence of identical filters to the chain for performance testing.
		/// </summary>
		/// <param name="benchmarks">The benchmarks instance (must not be null)</param>
		/// <param name="filterId">Filter ID to add repeatedly (must not be empty)</param>
		/// <param name="count">Number of identical filters to add (must be positive)</param>
		/// <returns>The modified FilterChain with added filters</returns>
		/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null</exception>
		/// <exception cref="ArgumentException"><paramref name="filterId"/> is empty or <paramref name="count"/> is not positive</exception>
		public static FilterChain AddStep_RepeatedFilters(this FilterChainBenchmarks benchmarks, Guid filterId, int count)
		{
			ArgumentNullException.ThrowIfNull(benchmarks);
			if (filterId == Guid.Empty)
				throw new ArgumentException("Filter ID cannot be empty", nameof(filterId));
			if (count <= 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive");

			var chain = benchmarks.AddStep_TenFilters();

			for (int i = 0; i < count; i++)
			{
				chain.AddStep(filterId, i);
			}

			return chain;
		}

		/// <summary>
		/// Gets the enabled filter count for comparison with validation results.
		/// </summary>
		/// <param name="benchmarks">The benchmarks instance (must not be null)</param>
		/// <returns>Number of enabled filters</returns>
		/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is null</exception>
		public static int GetEnabledFilterCount_ForValidation(this FilterChainBenchmarks benchmarks)
		{
			ArgumentNullException.ThrowIfNull(benchmarks);

			var chain = benchmarks.AddStep_TenFilters();
			var enabledSteps = benchmarks.GetEnabledSteps_TenSteps();

			return enabledSteps.Count;
		}

		/// <summary>
		/// Validates a chain and returns detailed validation information.
		/// </summary>
		/// <param name="benchmarks">The benchmarks instance (must not be null)</param>
		/// <param name="chain">Chain to validate (must not be null)</param>
		/// <returns>Validation result with step-by-step details</returns>
		/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> or <paramref name="chain"/> is null</exception>
		public static string ValidateChain_WithDetails(this FilterChainBenchmarks benchmarks, FilterChain chain)
		{
			ArgumentNullException.ThrowIfNull(benchmarks);
			ArgumentNullException.ThrowIfNull(chain);

			bool isValid = benchmarks.Validate_TenSteps();
			var enabledSteps = chain.GetEnabledSteps();

			var result = new System.Text.StringBuilder();
			result.AppendLine(isValid ? "Validation: PASSED" : "Validation: FAILED");
			result.AppendLine($"Enabled filters: {enabledSteps.Count}");

			foreach (var step in enabledSteps)
			{
				result.AppendLine($" - Filter {step.FilterId} (enabled: {step.IsEnabled})");
			}

			return result.ToString();
		}

		/// <summary>
		/// Creates a modified copy of a chain with specific filters disabled.
		/// </summary>
		/// <param name="benchmarks">The benchmarks instance (must not be null)</param>
		/// <param name="chain">Original chain to clone (must not be null)</param>
		/// <param name="disableIndices">Indices of filters to disable (can be null or empty)</param>
		/// <returns>New chain with specified filters disabled</returns>
		/// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> or <paramref name="chain"/> is null</exception>
		public static FilterChain CloneChain_WithDisabledFilters(this FilterChainBenchmarks benchmarks, FilterChain chain, int[] disableIndices)
		{
			ArgumentNullException.ThrowIfNull(benchmarks);
			ArgumentNullException.ThrowIfNull(chain);

			var clonedChain = benchmarks.Clone_TenStepChain();

			if (disableIndices is { Length: > 0 })
			{
				foreach (int index in disableIndices)
				{
					if (index >= 0 && index < clonedChain.Steps.Count)
					{
						clonedChain.Steps[index].IsEnabled = false;
					}
				}
			}

			return clonedChain;
		}
	}
}