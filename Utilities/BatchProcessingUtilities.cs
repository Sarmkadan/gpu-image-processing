#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utilities for batch processing operations including partitioning, scheduling, and progress tracking.
    /// Handles complex batch workflows with optimal GPU utilization and memory management.
    /// </summary>
    public static class BatchProcessingUtilities
    {
        /// <summary>
        /// Partitions items into optimal batch sizes based on GPU memory constraints
        /// </summary>
        public static List<List<T>> PartitionForGpu<T>(IEnumerable<T> items, int batchSize, long availableMemoryBytes)
        {
            if (batchSize <= 0)
                throw new ArgumentException("Batch size must be positive", nameof(batchSize));

            var batches = new List<List<T>>();
            var currentBatch = new List<T>();

            foreach (var item in items)
            {
                currentBatch.Add(item);
                if (currentBatch.Count >= batchSize)
                {
                    batches.Add(currentBatch);
                    currentBatch = new List<T>();
                }
            }

            if (currentBatch.Count > 0)
                batches.Add(currentBatch);

            return batches;
        }

        /// <summary>
        /// Calculates optimal batch size based on image dimensions and available GPU memory
        /// </summary>
        public static int CalculateOptimalBatchSize(int imageWidth, int imageHeight, int channels,
            long availableGpuMemoryBytes, float memoryUsageRatio = 0.8f)
        {
            // Each pixel requires channels * 4 bytes (float32)
            var bytesPerImage = imageWidth * imageHeight * channels * 4;

            // Leave headroom for GPU operations
            var usableMemory = (long)(availableGpuMemoryBytes * memoryUsageRatio);

            var optimalSize = Math.Max(1, (int)(usableMemory / bytesPerImage));
            return Math.Min(optimalSize, 256); // Cap at 256 for practical reasons
        }

        /// <summary>
        /// Schedules batch items for processing with priority weighting
        /// </summary>
        public static List<BatchItem<T>> ScheduleWithPriority<T>(
            IEnumerable<T> items,
            Func<T, int> prioritySelector)
        {
            var scheduled = new List<BatchItem<T>>();
            var priority = 0;

            foreach (var item in items.OrderByDescending(prioritySelector))
            {
                scheduled.Add(new BatchItem<T>
                {
                    Item = item,
                    SequenceNumber = priority++,
                    Priority = prioritySelector(item),
                    ScheduledAt = DateTime.UtcNow
                });
            }

            return scheduled;
        }

        /// <summary>
        /// Estimates total processing time for a batch based on item count and average processing time
        /// </summary>
        public static TimeSpan EstimateProcessingTime(
            int itemCount,
            TimeSpan averageTimePerItem,
            int parallelismLevel = 1)
        {
            var estimatedMilliseconds = (itemCount * averageTimePerItem.TotalMilliseconds) / parallelismLevel;
            return TimeSpan.FromMilliseconds(estimatedMilliseconds);
        }

        /// <summary>
        /// Calculates progress metrics for an ongoing batch operation
        /// </summary>
        public static BatchProgress CalculateProgress(
            int processedCount,
            int totalCount,
            TimeSpan elapsedTime,
            int errorCount = 0)
        {
            var progressPercent = totalCount > 0 ? (double)processedCount / totalCount * 100 : 0;

            var itemsPerSecond = elapsedTime.TotalSeconds > 0
                ? processedCount / elapsedTime.TotalSeconds
                : 0;

            TimeSpan? estimatedTimeRemaining = null;
            if (itemsPerSecond > 0)
            {
                var remainingItems = totalCount - processedCount;
                var remainingSeconds = remainingItems / itemsPerSecond;
                estimatedTimeRemaining = TimeSpan.FromSeconds(remainingSeconds);
            }

            return new BatchProgress
            {
                ProcessedCount = processedCount,
                TotalCount = totalCount,
                ErrorCount = errorCount,
                PercentComplete = progressPercent,
                ItemsPerSecond = itemsPerSecond,
                ElapsedTime = elapsedTime,
                EstimatedTimeRemaining = estimatedTimeRemaining,
                CompletionTime = estimatedTimeRemaining.HasValue
                    ? DateTime.UtcNow.Add(estimatedTimeRemaining.Value)
                    : (DateTime?)null
            };
        }

        /// <summary>
        /// Detects if batch processing should be paused or throttled based on system metrics
        /// </summary>
        public static ThrottleRecommendation EvaluateThrottleNeeded(
            float cpuUsagePercent,
            float gpuUsagePercent,
            float memoryUsagePercent,
            float cpuThreshold = 85,
            float gpuThreshold = 90,
            float memoryThreshold = 85)
        {
            var recommendations = new List<string>();

            if (cpuUsagePercent > cpuThreshold)
                recommendations.Add($"CPU usage at {cpuUsagePercent:F1}%");

            if (gpuUsagePercent > gpuThreshold)
                recommendations.Add($"GPU usage at {gpuUsagePercent:F1}%");

            if (memoryUsagePercent > memoryThreshold)
                recommendations.Add($"Memory usage at {memoryUsagePercent:F1}%");

            return new ThrottleRecommendation
            {
                ShouldThrottle = recommendations.Count > 0,
                ThrottleLevel = recommendations.Count switch
                {
                    1 => 0.8f,  // Reduce speed by 20%
                    2 => 0.5f,  // Reduce speed by 50%
                    _ => 0.2f   // Reduce speed by 80%
                },
                Reasons = recommendations
            };
        }

        /// <summary>
        /// Retries failed batch items with exponential backoff
        /// </summary>
        public static async Task RetryFailedItemsAsync<T>(
            List<BatchItem<T>> failedItems,
            Func<T, Task<bool>> processingFunc,
            int maxRetries = 3,
            int initialDelayMs = 100)
        {
            var retriableItems = failedItems.Where(i => i.RetryCount < maxRetries).ToList();

            foreach (var item in retriableItems)
            {
                var delayMs = initialDelayMs * (int)Math.Pow(2, item.RetryCount);
                await Task.Delay(delayMs).ConfigureAwait(false);

                try
                {
                    var success = await processingFunc(item.Item).ConfigureAwait(false);
                    if (success)
                        item.RetryCount++;
                }
                catch
                {
                    item.RetryCount++;
                }
            }
        }
    }

    public class BatchItem<T>
    {
        public T Item { get; set; }
        public int SequenceNumber { get; set; }
        public int Priority { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int RetryCount { get; set; }
        public string LastError { get; set; }
    }

    public class BatchProgress
    {
        public int ProcessedCount { get; set; }
        public int TotalCount { get; set; }
        public int ErrorCount { get; set; }
        public double PercentComplete { get; set; }
        public double ItemsPerSecond { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
        public DateTime? CompletionTime { get; set; }
    }

    public class ThrottleRecommendation
    {
        public bool ShouldThrottle { get; set; }
        public float ThrottleLevel { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
