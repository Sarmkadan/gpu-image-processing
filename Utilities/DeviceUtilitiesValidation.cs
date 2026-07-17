#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Utilities
{
	/// <summary>
	/// Provides validation helpers for <see cref="MemoryPressureAnalysis"/>.
	/// </summary>
	public static class DeviceUtilitiesValidation
	{
		/// <summary>
		/// Validates the provided <see cref="MemoryPressureAnalysis"/> instance.
		/// </summary>
		/// <param name="value">The <see cref="MemoryPressureAnalysis"/> instance to validate.</param>
		/// <returns>A read-only list of validation error messages.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		public static IReadOnlyList<string> Validate(this MemoryPressureAnalysis value)
		{
			ArgumentNullException.ThrowIfNull(value);

			var problems = new List<string>();

			if (value.TotalMemoryBytes < 0)
				problems.Add("TotalMemoryBytes cannot be negative.");

			if (value.UsedMemoryBytes < 0)
				problems.Add("UsedMemoryBytes cannot be negative.");

			if (value.UsedMemoryBytes > value.TotalMemoryBytes)
				problems.Add("UsedMemoryBytes cannot exceed TotalMemoryBytes.");

			if (value.FreeMemoryBytes < 0)
				problems.Add("FreeMemoryBytes cannot be negative.");

			if (value.UsagePercent is < 0f or > 100f)
				problems.Add("UsagePercent must be between 0 and 100.");

			if (!Enum.IsDefined(typeof(MemoryPressureLevel), value.PressureLevel))
				problems.Add("PressureLevel has an invalid value.");

			if (value.RecommendedBatchSize < 1)
				problems.Add("RecommendedBatchSize must be at least 1.");

			return problems;
		}

		/// <summary>
		/// Checks if the provided <see cref="MemoryPressureAnalysis"/> instance is valid.
		/// </summary>
		/// <param name="value">The <see cref="MemoryPressureAnalysis"/> instance to check.</param>
		/// <returns>True if valid, false otherwise.</returns>
		public static bool IsValid(this MemoryPressureAnalysis value) => value.Validate().Count == 0;

		/// <summary>
		/// Ensures the provided <see cref="MemoryPressureAnalysis"/> instance is valid.
		/// </summary>
		/// <param name="value">The <see cref="MemoryPressureAnalysis"/> instance to ensure is valid.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if validation fails.</exception>
		public static void EnsureValid(this MemoryPressureAnalysis value)
		{
			var problems = value.Validate();
			if (problems.Count > 0)
			{
				throw new ArgumentException($"Invalid {nameof(MemoryPressureAnalysis)}: {string.Join(", ", problems)}", nameof(value));
			}
		}
	}
}
