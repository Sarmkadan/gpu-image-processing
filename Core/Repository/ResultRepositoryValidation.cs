#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Validation helpers for <see cref="ResultRepository"/>.
    /// </summary>
    public static class ResultRepositoryValidation
    {
        /// <summary>
        /// Validates the state of the repository and returns a read‑only list of human‑readable problems.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <returns>A read‑only list of validation error messages. The list is empty when the repository is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this ResultRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Attempt to retrieve statistics – this is the only observable state we can inspect.
            ResultStatistics? stats = null;
            try
            {
                // Synchronously wait for the async method; this is acceptable for a validation helper.
                stats = value.GetStatisticsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                problems.Add($"Unable to obtain statistics: {ex.Message}");
            }

            if (stats is null)
            {
                problems.Add("Statistics object is null.");
                return problems;
            }

            // Validate numeric fields – negative values are never expected.
            if (stats.TotalResults < 0)
                problems.Add("TotalResults is negative.");

            if (stats.SuccessfulResults < 0)
                problems.Add("SuccessfulResults is negative.");

            if (stats.FailedResults < 0)
                problems.Add("FailedResults is negative.");

            if (stats.SuccessRate < 0 || stats.SuccessRate > 100)
                problems.Add("SuccessRate is out of the 0‑100 range.");

            if (stats.AverageProcessingTimeMs < 0)
                problems.Add("AverageProcessingTimeMs is negative.");

            if (stats.FastestProcessingMs < 0)
                problems.Add("FastestProcessingMs is negative.");

            if (stats.SlowestProcessingMs < 0)
                problems.Add("SlowestProcessingMs is negative.");

            if (stats.TotalOutputBytes < 0)
                problems.Add("TotalOutputBytes is negative.");

            if (stats.AverageOutputFileSize < 0)
                problems.Add("AverageOutputFileSize is negative.");

            return problems;
        }

        /// <summary>
        /// Determines whether the repository instance passes all validation checks.
        /// </summary>
        /// <param name="value">The repository to test.</param>
        /// <returns><c>true</c> if no validation problems are found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static bool IsValid(this ResultRepository value) =>
            value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the repository is valid, throwing an <see cref="ArgumentException"/> if any problems are found.
        /// </summary>
        /// <param name="value">The repository to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation problems are detected.</exception>
        public static void EnsureValid(this ResultRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
                throw new ArgumentException(
                    $"ResultRepository validation failed: {string.Join("; ", problems)}",
                    nameof(value));
        }
    }
}
