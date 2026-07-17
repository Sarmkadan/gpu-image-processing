using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Provides extension methods for <see cref="ResultRepository"/> to simplify common repository operations.
    /// </summary>
    public static class ResultRepositoryExtensions
    {
        /// <summary>
        /// Gets the most recent result for a specific processing job.
        /// </summary>
        /// <param name="repository">The repository instance.</param>
        /// <param name="jobId">The job identifier to search for.</param>
        /// <returns>The most recent processing result, or <see langword="null"/> if no results exist for the job.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
        public static async Task<ProcessingResult?> GetLatestResultAsync(this ResultRepository repository, Guid jobId)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(jobId);

            var results = await repository.GetByJobAsync(jobId);
            return results?.OrderByDescending(r => r.ProcessedAt).FirstOrDefault();
        }

        /// <summary>
        /// Gets the average processing time in milliseconds for all successful results.
        /// </summary>
        /// <param name="repository">The repository instance.</param>
        /// <returns>The average processing time in milliseconds, or 0 if no successful results exist.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
        public static async Task<float> GetAverageSuccessfulProcessingTimeAsync(this ResultRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var successfulResults = await repository.GetSuccessfulAsync();
            return successfulResults?.Average(r => r.ProcessingTimeMs) ?? 0;
        }
    }
}