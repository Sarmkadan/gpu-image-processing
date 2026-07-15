using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Extensions for ResultRepository
    /// </summary>
    public static class ResultRepositoryExtensions
    {
        /// <summary>
        /// Gets the most recent result for a specific processing job
        /// </summary>
        public static async Task<ProcessingResult> GetLatestResultAsync(this ResultRepository repository, Guid jobId)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(jobId);

            var results = await repository.GetByJobAsync(jobId);
            return results?.OrderByDescending(r => r.ProcessedAt).FirstOrDefault();
        }

        /// <summary>
        /// Gets the average processing time for successful results
        /// </summary>
        public static async Task<float> GetAverageSuccessfulProcessingTimeAsync(this ResultRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var successfulResults = await repository.GetSuccessfulAsync();
            return successfulResults?.Average(r => r.ProcessingTimeMs) ?? 0;
        }
    }
}
