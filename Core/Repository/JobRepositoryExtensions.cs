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
    using ProcessingStatus = GpuImageProcessing.Core.Constants.ProcessingStatus;

    /// <summary>
    /// Extension methods for <see cref="JobRepository"/> providing additional query capabilities
    /// </summary>
    public static class JobRepositoryExtensions
    {
        /// <summary>
        /// Gets jobs with the specified status
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="status">The status to filter by</param>
        /// <returns>Collection of jobs with the specified status</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        public static async Task<IReadOnlyList<ProcessingJob>> GetByStatusAsync(this JobRepository repository, ProcessingStatus status)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var jobs = await repository.GetByStatusAsync(status);
            return (await Task.FromResult(jobs.ToList())).AsReadOnly();
        }

        /// <summary>
        /// Gets all jobs that are in an active state (Pending, Running, or WarningState)
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <returns>Collection of active jobs ordered by creation time</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        public static async Task<IReadOnlyList<ProcessingJob>> GetActiveJobsAsync(this JobRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var jobs = await repository.GetAllAsync();
            var activeJobs = jobs.Where(j => ProcessingStatusHelper.IsActive(j.Status))
                .OrderBy(j => j.CreatedAt)
                .ToList();

            return activeJobs.AsReadOnly();
        }

        /// <summary>
        /// Gets jobs with completion progress above a specified threshold
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="minProgressPercentage">Minimum progress percentage (0-100)</param>
        /// <returns>Collection of jobs with progress above threshold</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minProgressPercentage"/> is outside valid range</exception>
        public static async Task<IReadOnlyList<ProcessingJob>> GetJobsAboveProgressAsync(this JobRepository repository, float minProgressPercentage = 75f)
        {
            ArgumentNullException.ThrowIfNull(repository);
            if (minProgressPercentage < 0 || minProgressPercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(minProgressPercentage), "Progress percentage must be between 0 and 100");
            }

            var jobs = await repository.GetAllAsync();
            var filteredJobs = jobs.Where(j => j.ProgressPercentage >= minProgressPercentage)
                .OrderByDescending(j => j.ProgressPercentage)
                .ToList();

            return filteredJobs.AsReadOnly();
        }

        /// <summary>
        /// Gets jobs created within a specific time range
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="startDate">Start date of the range (inclusive)</param>
        /// <param name="endDate">End date of the range (inclusive)</param>
        /// <returns>Collection of jobs created within the specified time range</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startDate"/> is after <paramref name="endDate"/></exception>
        public static async Task<IReadOnlyList<ProcessingJob>> GetJobsCreatedBetweenAsync(this JobRepository repository, DateTime startDate, DateTime endDate)
        {
            ArgumentNullException.ThrowIfNull(repository);
            if (startDate > endDate)
            {
                throw new ArgumentOutOfRangeException(nameof(startDate), "Start date cannot be after end date");
            }

            var jobs = await repository.GetAllAsync();
            var filteredJobs = jobs.Where(j => j.CreatedAt >= startDate && j.CreatedAt <= endDate)
                .OrderBy(j => j.CreatedAt)
                .ToList();

            return filteredJobs.AsReadOnly();
        }

        /// <summary>
        /// Gets the most recently completed jobs
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="count">Number of jobs to return</param>
        /// <returns>Collection of the most recent completed jobs</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1</exception>
        public static async Task<IReadOnlyList<ProcessingJob>> GetMostRecentCompletedAsync(this JobRepository repository, int count = 10)
        {
            ArgumentNullException.ThrowIfNull(repository);
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1");
            }

            var jobs = await repository.GetAllAsync();
            var completedJobs = jobs.Where(j => j.Status == ProcessingStatus.Completed)
                .OrderByDescending(j => j.CompletedAt)
                .Take(count)
                .ToList();

            return completedJobs.AsReadOnly();
        }

        /// <summary>
        /// Gets jobs by their name or description containing the specified search term
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="searchTerm">Term to search for in name or description</param>
        /// <param name="caseSensitive">Whether the search should be case sensitive</param>
        /// <returns>Collection of jobs matching the search criteria</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="searchTerm"/> is null</exception>
        public static async Task<IReadOnlyList<ProcessingJob>> SearchByNameOrDescriptionAsync(this JobRepository repository, string searchTerm, bool caseSensitive = false)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(searchTerm);

            var jobs = await repository.GetAllAsync();
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var filteredJobs = jobs.Where(j => j.Name.Contains(searchTerm, comparison) ||
                j.Description.Contains(searchTerm, comparison))
                .OrderByDescending(j => j.CreatedAt)
                .ToList();

            return filteredJobs.AsReadOnly();
        }

        /// <summary>
        /// Gets statistics for jobs matching a specific status
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="status">The status to filter by</param>
        /// <returns>Statistics for jobs with the specified status</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        public static async Task<JobStatistics> GetStatisticsByStatusAsync(this JobRepository repository, ProcessingStatus status)
        {
            ArgumentNullException.ThrowIfNull(repository);

            var jobs = await repository.GetByStatusAsync(status);
            var jobList = jobs.ToList();

            var stats = new JobStatistics
            {
                TotalJobs = jobList.Count,
                PendingJobs = jobList.Count(j => j.Status == ProcessingStatus.Pending),
                RunningJobs = jobList.Count(j => j.Status == ProcessingStatus.Running),
                CompletedJobs = jobList.Count(j => j.Status == ProcessingStatus.Completed),
                FailedJobs = jobList.Count(j => j.Status == ProcessingStatus.Failed),
                CancelledJobs = jobList.Count(j => j.Status == ProcessingStatus.Cancelled),
                AverageCompletionTime = jobList.Count > 0
                    ? jobList.Average(j => j.GetElapsedTime().TotalSeconds)
                    : 0,
                SuccessRate = jobList.Count > 0
                    ? (jobList.Count(j => j.Status == ProcessingStatus.Completed) / (double)jobList.Count) * 100
                    : 0,
                TotalImagesProcessed = jobList.Sum(j => j.ProcessedImages),
                TotalImagesFailed = jobList.Sum(j => j.FailedImages)
            };

            return stats;
        }

        /// <summary>
        /// Gets jobs that have been running for longer than a specified duration
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <param name="minDuration">Minimum duration in seconds</param>
        /// <returns>Collection of jobs running longer than the specified duration</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minDuration"/> is negative</exception>
        public static async Task<IReadOnlyList<ProcessingJob>> GetLongRunningJobsAsync(this JobRepository repository, double minDuration = 3600)
        {
            ArgumentNullException.ThrowIfNull(repository);
            if (minDuration < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minDuration), "Duration cannot be negative");
            }

            var jobs = await repository.GetAllAsync();
            var longRunningJobs = jobs.Where(j => j.Status == ProcessingStatus.Running)
                .Where(j => j.StartedAt.HasValue)
                .Where(j => (DateTime.UtcNow - j.StartedAt.Value).TotalSeconds >= minDuration)
                .OrderByDescending(j => (DateTime.UtcNow - j.StartedAt.Value).TotalSeconds)
                .ToList();

            return longRunningJobs.AsReadOnly();
        }

        /// <summary>
        /// Gets the total number of jobs across all statuses
        /// </summary>
        /// <param name="repository">The job repository</param>
        /// <returns>Total job count</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
        public static async Task<int> GetTotalJobsCountAsync(this JobRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            return await repository.CountAsync();
        }
    }
}