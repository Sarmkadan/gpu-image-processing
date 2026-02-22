// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Caching;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Background worker for cache maintenance including cleanup and optimization.
    /// Periodically removes expired entries and monitors cache health.
    /// </summary>
    public class CacheMaintenanceWorker : BackgroundWorkerBase
    {
        private readonly DistributedCache _cache;
        private readonly TimeSpan _cleanupInterval;
        private readonly float _memoryWarningThreshold;

        public CacheMaintenanceWorker(
            DistributedCache cache,
            TimeSpan? cleanupInterval = null,
            float memoryWarningThreshold = 80)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(5);
            _memoryWarningThreshold = memoryWarningThreshold;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Clean expired entries
                    await _cache.CleanupExpiredAsync();

                    // Check memory usage
                    var stats = _cache.GetStats();
                    if (stats.MemoryUsagePercent > _memoryWarningThreshold)
                    {
                        OnWarning($"Cache memory usage high: {stats.MemoryUsagePercent:F1}% " +
                                 $"({stats.UsedMemoryBytes / 1024 / 1024}MB / {stats.MaxMemoryBytes / 1024 / 1024}MB)");
                    }

                    OnProgressUpdated($"Cache maintenance completed. " +
                                    $"Items: {stats.ItemCount}, " +
                                    $"Memory: {stats.MemoryUsagePercent:F1}%");

                    await Task.Delay(_cleanupInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnError($"Cache maintenance error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }
    }

    /// <summary>
    /// Abstract base class for background worker implementations
    /// </summary>
    public abstract class BackgroundWorkerBase
    {
        protected bool _isRunning;

        public event EventHandler<WorkerStatusChangedEventArgs> StatusChanged;
        public event EventHandler<WorkerProgressEventArgs> ProgressUpdated;
        public event EventHandler<WorkerErrorEventArgs> ErrorOccurred;

        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
                throw new InvalidOperationException("Worker is already running");

            _isRunning = true;
            OnStatusChanged(WorkerStatus.Running);

            try
            {
                await ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                OnError($"Worker failed: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                OnStatusChanged(WorkerStatus.Stopped);
            }
        }

        public virtual void Stop()
        {
            _isRunning = false;
            OnStatusChanged(WorkerStatus.Stopping);
        }

        protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

        protected void OnStatusChanged(WorkerStatus status)
        {
            StatusChanged?.Invoke(this, new WorkerStatusChangedEventArgs
            {
                Status = status,
                Timestamp = DateTime.UtcNow
            });
        }

        protected void OnProgressUpdated(string message)
        {
            ProgressUpdated?.Invoke(this, new WorkerProgressEventArgs
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        protected void OnWarning(string message)
        {
            ProgressUpdated?.Invoke(this, new WorkerProgressEventArgs
            {
                Message = message,
                IsWarning = true,
                Timestamp = DateTime.UtcNow
            });
        }

        protected void OnError(string message)
        {
            ErrorOccurred?.Invoke(this, new WorkerErrorEventArgs
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public enum WorkerStatus
    {
        Idle,
        Running,
        Paused,
        Stopping,
        Stopped,
        Failed
    }

    public class WorkerStatusChangedEventArgs : EventArgs
    {
        public WorkerStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class WorkerProgressEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool IsWarning { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class WorkerErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
