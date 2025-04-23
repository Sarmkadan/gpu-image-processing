#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Repository for processing job data access operations
    /// </summary>
    public class JobRepository : GenericRepository<ProcessingJob>
    {
        /// <summary>
        /// Gets all jobs with a specific status
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetByStatusAsync(ProcessingStatus status)
        {
            return await Task.FromResult(
                _entities.Where(j => j.Status == status).ToList()
            );
        }

        /// <summary>
        /// Gets all pending jobs
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetPendingAsync()
        {
            return await Task.FromResult(
                _entities.Where(j => j.Status == ProcessingStatus.Pending)
                    .OrderBy(j => j.CreatedAt).ToList()
            );
        }

        /// <summary>
        /// Gets all running jobs
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetRunningAsync()
        {
            return await Task.FromResult(
                _entities.Where(j => j.Status == ProcessingStatus.Running).ToList()
            );
        }

        /// <summary>
        /// Gets all failed jobs
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetFailedAsync()
        {
            return await Task.FromResult(
                _entities.Where(j => j.Status == ProcessingStatus.Failed)
                    .OrderByDescending(j => j.CompletedAt).ToList()
            );
        }

        /// <summary>
        /// Gets jobs completed within a date range
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetCompletedBetweenAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.FromResult(
                _entities.Where(j => j.CompletedAt >= startDate && j.CompletedAt <= endDate)
                    .OrderByDescending(j => j.CompletedAt).ToList()
            );
        }

        /// <summary>
        /// Gets jobs with a specific filter applied
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetByFilterAsync(Guid filterId)
        {
            return await Task.FromResult(
                _entities.Where(j => j.FilterIds.Contains(filterId)).ToList()
            );
        }

        /// <summary>
        /// Gets jobs processing a specific image
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetByImageAsync(Guid imageId)
        {
            return await Task.FromResult(
                _entities.Where(j => j.ImageIds.Contains(imageId)).ToList()
            );
        }

        /// <summary>
        /// Gets jobs with completion rate above threshold
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetHighProgressAsync(float minProgress = 50f)
        {
            return await Task.FromResult(
                _entities.Where(j => j.ProgressPercentage >= minProgress).ToList()
            );
        }

        /// <summary>
        /// Gets the oldest incomplete job
        /// </summary>
        public async Task<ProcessingJob?> GetOldestIncompleteAsync()
        {
            return await Task.FromResult(
                _entities.Where(j => !ProcessingStatusHelper.IsTerminal(j.Status))
                    .OrderBy(j => j.CreatedAt).FirstOrDefault()
            );
        }

        /// <summary>
        /// Gets job processing statistics
        /// </summary>
        public async Task<JobStatistics> GetStatisticsAsync()
        {
            var completedJobs = _entities.Where(j => j.Status == ProcessingStatus.Completed).ToList();
            var failedJobs = _entities.Where(j => j.Status == ProcessingStatus.Failed).ToList();

            var stats = new JobStatistics
            {
                TotalJobs = _entities.Count,
                PendingJobs = _entities.Count(j => j.Status == ProcessingStatus.Pending),
                RunningJobs = _entities.Count(j => j.Status == ProcessingStatus.Running),
                CompletedJobs = completedJobs.Count,
                FailedJobs = failedJobs.Count,
                CancelledJobs = _entities.Count(j => j.Status == ProcessingStatus.Cancelled),
                AverageCompletionTime = completedJobs.Count > 0
                    ? completedJobs.Average(j => j.GetElapsedTime().TotalSeconds)
                    : 0,
                SuccessRate = _entities.Count > 0
                    ? (completedJobs.Count / (double)_entities.Count) * 100
                    : 0,
                TotalImagesProcessed = completedJobs.Sum(j => j.ProcessedImages),
                TotalImagesFailed = failedJobs.Sum(j => j.FailedImages)
            };

            return await Task.FromResult(stats);
        }

        /// <summary>
        /// Clears failed jobs older than specified date
        /// </summary>
        public async Task<int> ClearOldFailedJobsAsync(DateTime beforeDate)
        {
            var jobsToRemove = _entities.Where(j => j.Status == ProcessingStatus.Failed &&
                j.CompletedAt.HasValue && j.CompletedAt < beforeDate).ToList();

            foreach (var job in jobsToRemove)
            {
                _entities.Remove(job);
            }

            return await Task.FromResult(jobsToRemove.Count);
        }
    }

    /// <summary>
    /// Statistics about processing jobs
    /// </summary>
    public class JobStatistics
    {
        public int TotalJobs { get; set; }
        public int PendingJobs { get; set; }
        public int RunningJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int CancelledJobs { get; set; }
        public double AverageCompletionTime { get; set; }
        public double SuccessRate { get; set; }
        public long TotalImagesProcessed { get; set; }
        public long TotalImagesFailed { get; set; }
    }
}
