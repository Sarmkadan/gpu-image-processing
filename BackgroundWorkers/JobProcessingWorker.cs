// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Events;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Background worker for processing image jobs asynchronously.
    /// Dequeues jobs and coordinates filter and transform application.
    /// </summary>
    public class JobProcessingWorker : IBackgroundWorker
    {
        private readonly ILogger<JobProcessingWorker> _logger;
        private readonly BatchProcessingService _batchProcessingService;
        private readonly ImageProcessingService _imageProcessingService;
        private readonly IEventPublisher _eventPublisher;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _workerTask;
        private bool _isRunning;

        public JobProcessingWorker(
            ILogger<JobProcessingWorker> logger,
            BatchProcessingService batchProcessingService,
            ImageProcessingService imageProcessingService,
            IEventPublisher eventPublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _batchProcessingService = batchProcessingService ?? throw new ArgumentNullException(nameof(batchProcessingService));
            _imageProcessingService = imageProcessingService ?? throw new ArgumentNullException(nameof(imageProcessingService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        }

        /// <summary>
        /// Gets the name of this background worker.
        /// </summary>
        public string GetName()
        {
            return "JobProcessingWorker";
        }

        /// <summary>
        /// Starts the background worker.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                _logger.LogWarning("Worker already running");
                return;
            }

            _isRunning = true;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _workerTask = ProcessJobsAsync(_cancellationTokenSource.Token);

            _logger.LogInformation("Job processing worker started");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Stops the background worker gracefully.
        /// </summary>
        public async Task StopAsync(TimeSpan timeout = default)
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Worker not running");
                return;
            }

            _isRunning = false;
            _cancellationTokenSource?.Cancel();

            if (timeout == default)
                timeout = TimeSpan.FromSeconds(30);

            try
            {
                await _workerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker cancellation token triggered");
            }

            _logger.LogInformation("Job processing worker stopped");
        }

        /// <summary>
        /// Gets whether the worker is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Main loop for processing jobs from the queue.
        /// Continuously dequeues and processes jobs until cancellation.
        /// </summary>
        private async Task ProcessJobsAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Job processing loop starting");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Simulate dequeue operation
                    await Task.Delay(1000, cancellationToken);

                    // In real implementation, would dequeue from job queue
                    // For demo purposes, logging only
                    _logger.LogTrace("Checking for jobs in queue");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in job processing loop");
                }
            }

            _logger.LogDebug("Job processing loop stopped");
        }

        /// <summary>
        /// Processes a single image job with error handling and metrics.
        /// </summary>
        private async Task ProcessImageJobAsync(Guid imageId, List<Guid> filterIds, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Publish job started event
                var startedEvent = new ImageProcessingStartedEvent
                {
                    ImageId = imageId.ToString(),
                    OperationId = Guid.NewGuid().ToString("N")
                };

                await _eventPublisher.PublishAsync(startedEvent);

                // Simulate image processing
                await Task.Delay(100, cancellationToken);

                stopwatch.Stop();

                // Publish job completed event
                var completedEvent = new ImageProcessingCompletedEvent
                {
                    ImageId = imageId.ToString(),
                    OutputPath = $"./output/{imageId}_processed.jpg",
                    ProcessedSize = 1024000,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    ProcessingMetrics = new Dictionary<string, object>
                    {
                        { "filter_count", filterIds.Count },
                        { "quality_score", 0.92 }
                    },
                    OperationId = startedEvent.OperationId
                };

                await _eventPublisher.PublishAsync(completedEvent);

                _logger.LogInformation(
                    "Image processed successfully - ImageId: {ImageId}, Duration: {DurationMs}ms",
                    imageId,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Image processing failed - ImageId: {ImageId}, Error: {ErrorMessage}",
                    imageId,
                    ex.Message);

                // Publish failure event
                var failedEvent = new ImageProcessingFailedEvent
                {
                    ImageId = imageId.ToString(),
                    ErrorMessage = ex.Message,
                    ErrorCode = "PROCESSING_ERROR",
                    AttemptDurationMs = stopwatch.ElapsedMilliseconds
                };

                await _eventPublisher.PublishAsync(failedEvent);
            }
        }

        /// <summary>
        /// Handles graceful shutdown and cleanup.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    /// Interface for background workers.
    /// </summary>
    public interface IBackgroundWorker : IDisposable
    {
        string GetName();
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(TimeSpan timeout = default);
        bool IsRunning { get; }
    }
}
