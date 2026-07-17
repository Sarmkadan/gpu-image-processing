#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Validation helpers for <see cref="JobRepository"/> instances.
    /// </summary>
    public static class JobRepositoryValidation
    {
        /// <summary>
        /// Validates a <see cref="JobRepository"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static async Task<IReadOnlyList<string>> ValidateAsync(this JobRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            JobStatistics stats;
            try
            {
                stats = await value.GetStatisticsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                problems.Add($"Unable to obtain statistics: {ex.Message}");
                return problems.AsReadOnly();
            }

            if (stats.TotalJobs < 0) problems.Add("Total jobs cannot be negative.");
            if (stats.PendingJobs < 0) problems.Add("Pending jobs cannot be negative.");
            if (stats.RunningJobs < 0) problems.Add("Running jobs cannot be negative.");
            if (stats.CompletedJobs < 0) problems.Add("Completed jobs cannot be negative.");
            if (stats.FailedJobs < 0) problems.Add("Failed jobs cannot be negative.");
            if (stats.CancelledJobs < 0) problems.Add("Cancelled jobs cannot be negative.");
            if (stats.AverageCompletionTime < 0) problems.Add("Average completion time cannot be negative.");
            if (stats.SuccessRate < 0 || stats.SuccessRate > 100) problems.Add("Success rate must be between 0 and 100.");
            if (stats.TotalImagesProcessed < 0) problems.Add("Total images processed cannot be negative.");
            if (stats.TotalImagesFailed < 0) problems.Add("Total images failed cannot be negative.");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a <see cref="JobRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns>True if the instance is valid; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static async Task<bool> IsValidAsync(this JobRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return (await ValidateAsync(value).ConfigureAwait(false)).Count == 0;
        }

        /// <summary>
        /// Ensures a <see cref="JobRepository"/> instance is valid, throwing an exception if it's not.
        /// </summary>
        /// <param name="value">The instance to ensure.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is invalid.</exception>
        public static async Task EnsureValidAsync(this JobRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = await ValidateAsync(value).ConfigureAwait(false);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Invalid JobRepository instance: {string.Join(", ", problems)}", nameof(value));
            }
        }
    }
}
