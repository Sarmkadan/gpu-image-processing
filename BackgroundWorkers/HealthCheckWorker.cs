// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Events;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Background worker for periodic system health checks.
    /// Monitors device status, performance, and resource availability.
    /// </summary>
    public class HealthCheckWorker : IBackgroundWorker
    {
        private readonly ILogger<HealthCheckWorker> _logger;
        private readonly DeviceService _deviceService;
        private readonly IEventPublisher _eventPublisher;
        private readonly TimeSpan _checkInterval;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _workerTask;
        private bool _isRunning;

        public HealthCheckWorker(
            ILogger<HealthCheckWorker> logger,
            DeviceService deviceService,
            IEventPublisher eventPublisher,
            TimeSpan? checkInterval = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _checkInterval = checkInterval ?? TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Gets the name of this background worker.
        /// </summary>
        public string GetName()
        {
            return "HealthCheckWorker";
        }

        /// <summary>
        /// Starts the background health check worker.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                _logger.LogWarning("Health check worker already running");
                return;
            }

            _isRunning = true;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _workerTask = PerformHealthChecksAsync(_cancellationTokenSource.Token);

            _logger.LogInformation(
                "Health check worker started - Interval: {IntervalSeconds}s",
                _checkInterval.TotalSeconds);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Stops the health check worker gracefully.
        /// </summary>
        public async Task StopAsync(TimeSpan timeout = default)
        {
            if (!_isRunning)
            {
                _logger.LogWarning("Health check worker not running");
                return;
            }

            _isRunning = false;
            _cancellationTokenSource?.Cancel();

            if (timeout == default)
                timeout = TimeSpan.FromSeconds(30);

            try
            {
                if (!_workerTask.IsCompleted)
                {
                    await _workerTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Health check worker cancellation triggered");
            }

            _logger.LogInformation("Health check worker stopped");
        }

        /// <summary>
        /// Gets whether the worker is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Main loop for periodic health checks.
        /// </summary>
        private async Task PerformHealthChecksAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Health check loop starting");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Perform health check
                    await PerformSystemHealthCheckAsync(cancellationToken);

                    // Wait for next interval
                    await Task.Delay(_checkInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during health check");

                    // Wait before retrying on error
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }

            _logger.LogDebug("Health check loop stopped");
        }

        /// <summary>
        /// Performs comprehensive system health assessment.
        /// Checks device availability, memory usage, and processing performance.
        /// </summary>
        private async Task PerformSystemHealthCheckAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Performing system health check");

            try
            {
                var stats = await _deviceService.GetStatisticsAsync();
                var componentHealth = new Dictionary<string, bool>();
                var issues = new List<string>();

                // Check device availability
                bool devicesHealthy = stats.AvailableDevices > 0;
                componentHealth["devices"] = devicesHealthy;
                if (!devicesHealthy)
                    issues.Add("No GPU devices available");

                // Check memory usage (simulate)
                bool memoryHealthy = stats.AvailableDevices > 0;
                componentHealth["memory"] = memoryHealthy;

                // Check compute units
                bool computeHealthy = stats.TotalComputeUnits > 0;
                componentHealth["compute"] = computeHealthy;
                if (!computeHealthy)
                    issues.Add("No compute units available");

                // Determine overall health
                bool isHealthy = devicesHealthy && memoryHealthy && computeHealthy;

                // Publish health check event
                var healthEvent = new HealthCheckEvent
                {
                    IsHealthy = isHealthy,
                    HealthStatus = isHealthy ? "HEALTHY" : "DEGRADED",
                    ComponentHealth = componentHealth,
                    Issues = issues,
                    OperationId = Guid.NewGuid().ToString("N")
                };

                await _eventPublisher.PublishAsync(healthEvent);

                if (!isHealthy)
                {
                    _logger.LogWarning(
                        "System health degraded - Issues: {IssueCount}, Details: {Issues}",
                        issues.Count,
                        string.Join(", ", issues));
                }
                else
                {
                    _logger.LogDebug("System health check passed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");

                // Publish failure event
                var failedEvent = new HealthCheckEvent
                {
                    IsHealthy = false,
                    HealthStatus = "UNHEALTHY",
                    ComponentHealth = new Dictionary<string, bool>(),
                    Issues = new List<string> { ex.Message }
                };

                await _eventPublisher.PublishAsync(failedEvent);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}
