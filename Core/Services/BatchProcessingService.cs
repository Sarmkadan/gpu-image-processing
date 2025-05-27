#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Service for managing batch processing jobs with progress tracking
    /// </summary>
    public class BatchProcessingService
    {
        private readonly JobRepository _jobRepository;
        private readonly ResultRepository _resultRepository;
        private readonly ImageProcessingService _processingService;
        private readonly ImageRepository _imageRepository;

        public BatchProcessingService(
            JobRepository jobRepository,
            ResultRepository resultRepository,
            ImageProcessingService processingService,
            ImageRepository imageRepository)
        {
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            _resultRepository = resultRepository ?? throw new ArgumentNullException(nameof(resultRepository));
            _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        /// <summary>
        /// Creates and starts a new batch processing job
        /// </summary>
        public async Task<ProcessingJob> CreateJobAsync(string jobName, List<Guid> imageIds,
            List<Guid> filterIds, List<Guid> transformIds, Guid profileId)
        {
            var job = new ProcessingJob
            {
                Name = jobName,
                ImageIds = imageIds,
                FilterIds = filterIds,
                TransformIds = transformIds,
                TotalImages = imageIds.Count,
                Status = ProcessingStatus.Pending,
                OutputDirectory = "./output/"
            };

            return await _jobRepository.AddAsync(job);
        }

        /// <summary>
        /// Executes a batch processing job with progress tracking
        /// </summary>
        /// <param name="jobId">The identifier of the batch job to execute.</param>
        /// <param name="profileId">The identifier of the processing profile to use.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteJobAsync(Guid jobId, Guid profileId, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
                throw new InvalidOperationException("Job not found");

            if (job.Status != ProcessingStatus.Pending)
                throw new InvalidOperationException("Job is not in pending state");

            try
            {
                job.Start();
                await _jobRepository.UpdateAsync(job);

                var results = await _processingService.ProcessBatchAsync(
                    job.ImageIds,
                    job.FilterIds,
                    job.TransformIds,
                    profileId
                );

                // Store results
                foreach (var result in results)
                {
                    result.JobId = jobId;
                    await _resultRepository.AddAsync(result);

                    if (result.IsSuccessful)
                        job.ProcessedImages++;
                    else
                        job.FailedImages++;

                    job.UpdateProgress(job.ProcessedImages, job.FailedImages);

                    // Check for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        job.Cancel();
                        await _jobRepository.UpdateAsync(job);
                        return;
                    }
                }

                job.Complete();
                await _jobRepository.UpdateAsync(job);
            }
            catch (Exception ex)
            {
                job.MarkFailed(ex.Message);
                await _jobRepository.UpdateAsync(job);
                throw;
            }
        }

        /// <summary>
        /// Gets a job by ID
        /// </summary>
        public async Task<ProcessingJob?> GetJobAsync(Guid jobId)
        {
            return await _jobRepository.GetByIdAsync(jobId);
        }

        /// <summary>
        /// Gets all jobs with a specific status
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetJobsByStatusAsync(ProcessingStatus status)
        {
            return await _jobRepository.GetByStatusAsync(status);
        }

        /// <summary>
        /// Gets results for a specific job
        /// </summary>
        public async Task<IEnumerable<ProcessingResult>> GetJobResultsAsync(Guid jobId)
        {
            return await _resultRepository.GetByJobAsync(jobId);
        }

        /// <summary>
        /// Updates job progress
        /// </summary>
        public async Task UpdateJobProgressAsync(Guid jobId, int processedCount, int failedCount)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job != null)
            {
                job.UpdateProgress(processedCount, failedCount);
                await _jobRepository.UpdateAsync(job);
            }
        }

        /// <summary>
        /// Cancels a running job
        /// </summary>
        public async Task<bool> CancelJobAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
                return false;

            if (ProcessingStatusHelper.IsTerminal(job.Status))
                return false;

            job.Cancel();
            await _jobRepository.UpdateAsync(job);
            return true;
        }

        /// <summary>
        /// Pauses a running job
        /// </summary>
        public async Task<bool> PauseJobAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null || job.Status != ProcessingStatus.Running)
                return false;

            job.Status = ProcessingStatus.Paused;
            await _jobRepository.UpdateAsync(job);
            return true;
        }

        /// <summary>
        /// Resumes a paused job
        /// </summary>
        public async Task<bool> ResumeJobAsync(Guid jobId, Guid profileId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null || job.Status != ProcessingStatus.Paused)
                return false;

            job.Status = ProcessingStatus.Running;
            await _jobRepository.UpdateAsync(job);
            // Continue execution from where it left off
            return true;
        }

        /// <summary>
        /// Gets comprehensive job statistics
        /// </summary>
        public async Task<JobExecutionStats> GetJobStatsAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
                throw new InvalidOperationException("Job not found");

            var results = (await _resultRepository.GetByJobAsync(jobId)).ToList();
            var successfulResults = results.Where(r => r.IsSuccessful).ToList();

            var stats = new JobExecutionStats
            {
                JobId = jobId,
                JobName = job.Name,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                ElapsedTime = job.GetElapsedTime(),
                TotalImages = job.TotalImages,
                ProcessedImages = job.ProcessedImages,
                FailedImages = job.FailedImages,
                ProgressPercentage = job.ProgressPercentage,
                SuccessRate = job.TotalImages > 0 ? (job.ProcessedImages / (float)job.TotalImages) * 100 : 0,
                AverageProcessingTimeMs = successfulResults.Count > 0
                    ? successfulResults.Average(r => r.ProcessingTimeMs)
                    : 0,
                EstimatedRemainingTime = job.EstimateRemainingTime(),
                ProcessingRate = job.GetProcessingRate(),
                TotalOperations = job.GetTotalOperations()
            };

            return stats;
        }

        /// <summary>
        /// Gets all pending jobs
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetPendingJobsAsync()
        {
            return await _jobRepository.GetPendingAsync();
        }

        /// <summary>
        /// Gets all running jobs
        /// </summary>
        public async Task<IEnumerable<ProcessingJob>> GetRunningJobsAsync()
        {
            return await _jobRepository.GetRunningAsync();
        }

        /// <summary>
        /// Cleans up old completed jobs
        /// </summary>
        public async Task<int> CleanupOldJobsAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            return await _jobRepository.ClearOldFailedJobsAsync(cutoffDate);
        }

        /// <summary>
        /// Gets queue depth (number of pending jobs)
        /// </summary>
        public async Task<int> GetQueueDepthAsync()
        {
            var pending = await _jobRepository.GetPendingAsync();
            return pending.Count();
        }

        /// <summary>
        /// Prioritizes a pending job to move it to the front of the queue
        /// </summary>
        public async Task<bool> PrioritizeJobAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null || job.Status != ProcessingStatus.Pending)
                return false;

            // In a real implementation, would update queue position
            return await Task.FromResult(true);
        }
    }

    /// <summary>
    /// Job execution statistics
    /// </summary>
    public class JobExecutionStats
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public ProcessingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public int TotalImages { get; set; }
        public int ProcessedImages { get; set; }
        public int FailedImages { get; set; }
        public float ProgressPercentage { get; set; }
        public float SuccessRate { get; set; }
        public float AverageProcessingTimeMs { get; set; }
        public TimeSpan EstimatedRemainingTime { get; set; }
        public double ProcessingRate { get; set; }
        public int TotalOperations { get; set; }
    }
}
