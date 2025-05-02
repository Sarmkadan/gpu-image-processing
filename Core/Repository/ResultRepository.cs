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

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Repository for processing result data access operations
    /// </summary>
    public class ResultRepository : GenericRepository<ProcessingResult>
    {
        /// <summary>
        /// Gets results for a specific processing job
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetByJobAsync(Guid jobId)
        {
            return await Task.FromResult(
                _entities.Where(r => r.JobId == jobId).ToList()
            );
        }

        /// <summary>
        /// Gets results for a specific image
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetByImageAsync(Guid imageId)
        {
            return await Task.FromResult(
                _entities.Where(r => r.ImageId == imageId)
                    .OrderByDescending(r => r.ProcessedAt).ToList()
            );
        }

        /// <summary>
        /// Gets all successful results
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetSuccessfulAsync()
        {
            return await Task.FromResult(
                _entities.Where(r => r.IsSuccessful).ToList()
            );
        }

        /// <summary>
        /// Gets all failed results
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetFailedAsync()
        {
            return await Task.FromResult(
                _entities.Where(r => !r.IsSuccessful)
                    .OrderByDescending(r => r.ProcessedAt).ToList()
            );
        }

        /// <summary>
        /// Gets results processed within a date range
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetProcessedBetweenAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.FromResult(
                _entities.Where(r => r.ProcessedAt >= startDate && r.ProcessedAt <= endDate)
                    .OrderByDescending(r => r.ProcessedAt).ToList()
            );
        }

        /// <summary>
        /// Gets results that applied a specific filter
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetByAppliedFilterAsync(string filterName)
        {
            return await Task.FromResult(
                _entities.Where(r => r.AppliedFilters.Contains(filterName)).ToList()
            );
        }

        /// <summary>
        /// Gets results with processing time above a threshold
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetSlowProcessingAsync(float minTimeMs = 1000)
        {
            return await Task.FromResult(
                _entities.Where(r => r.ProcessingTimeMs >= minTimeMs)
                    .OrderByDescending(r => r.ProcessingTimeMs).ToList()
            );
        }

        /// <summary>
        /// Gets the average compression ratio for results
        /// </summary>
        public async Task<float> GetAverageCompressionRatioAsync()
        {
            if (_entities.Count == 0)
                return 1.0f;

            var ratios = new List<float>();
            foreach (var result in _entities)
            {
                var inputSize = new System.IO.FileInfo(result.InputFilePath).Length;
                if (inputSize > 0)
                {
                    ratios.Add(result.GetCompressionRatio(inputSize));
                }
            }

            return await Task.FromResult(ratios.Count > 0 ? ratios.Average() : 1.0f).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets processing result statistics
        /// </summary>
        public async Task<ResultStatistics> GetStatisticsAsync()
        {
            var successfulResults = _entities.Where(r => r.IsSuccessful).ToList();
            var failedResults = _entities.Where(r => !r.IsSuccessful).ToList();

            var stats = new ResultStatistics
            {
                TotalResults = _entities.Count,
                SuccessfulResults = successfulResults.Count,
                FailedResults = failedResults.Count,
                SuccessRate = _entities.Count > 0
                    ? (successfulResults.Count / (double)_entities.Count) * 100
                    : 0,
                AverageProcessingTimeMs = successfulResults.Count > 0
                    ? successfulResults.Average(r => r.ProcessingTimeMs)
                    : 0,
                FastestProcessingMs = successfulResults.Count > 0
                    ? successfulResults.Min(r => r.ProcessingTimeMs)
                    : 0,
                SlowestProcessingMs = successfulResults.Count > 0
                    ? successfulResults.Max(r => r.ProcessingTimeMs)
                    : 0,
                TotalOutputBytes = successfulResults.Sum(r => r.OutputFileSizeBytes),
                AverageOutputFileSize = successfulResults.Count > 0
                    ? successfulResults.Average(r => r.OutputFileSizeBytes)
                    : 0
            };

            return await Task.FromResult(stats).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the most commonly applied filters
        /// </summary>
        public async Task<Dictionary<string, int>> GetMostUsedFiltersAsync(int topCount = 10)
        {
            var filterUsage = new Dictionary<string, int>();

            foreach (var result in _entities)
            {
                foreach (var filter in result.AppliedFilters)
                {
                    if (filterUsage.ContainsKey(filter))
                        filterUsage[filter]++;
                    else
                        filterUsage[filter] = 1;
                }
            }

            var topFilters = filterUsage
                .OrderByDescending(kvp => kvp.Value)
                .Take(topCount)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return await Task.FromResult(topFilters).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears results older than specified date
        /// </summary>
        public async Task<int> ClearOldResultsAsync(DateTime beforeDate)
        {
            var resultsToRemove = _entities.Where(r => r.ProcessedAt < beforeDate).ToList();

            foreach (var result in resultsToRemove)
            {
                _entities.Remove(result);
            }

            return await Task.FromResult(resultsToRemove.Count).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Statistics about processing results
    /// </summary>
    public class ResultStatistics
    {
        public int TotalResults { get; set; }
        public int SuccessfulResults { get; set; }
        public int FailedResults { get; set; }
        public double SuccessRate { get; set; }
        public float AverageProcessingTimeMs { get; set; }
        public float FastestProcessingMs { get; set; }
        public float SlowestProcessingMs { get; set; }
        public long TotalOutputBytes { get; set; }
        public double AverageOutputFileSize { get; set; }
    }
}
