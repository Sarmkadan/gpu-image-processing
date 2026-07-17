#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Provides validation helpers for processing events.
    /// </summary>
    public static class ProcessingEventValidation
    {
        /// <summary>
        /// Validates a processing event and returns a list of human-readable validation problems.
        /// </summary>
        /// <param name="value">The processing event to validate.</param>
        /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public static IReadOnlyList<string> Validate(this ProcessingEvent? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate base ProcessingEvent properties
            if (string.IsNullOrWhiteSpace(value.EventId))
            {
                errors.Add("EventId must not be null or whitespace.");
            }

            if (value.Timestamp == default)
            {
                errors.Add("Timestamp must be a valid DateTime, not default(DateTime).");
            }

            if (string.IsNullOrWhiteSpace(value.OperationId))
            {
                errors.Add("OperationId must not be null or whitespace.");
            }

            if (value.Metadata == null)
            {
                errors.Add("Metadata dictionary must not be null.");
            }

            // Validate based on concrete type
            switch (value)
            {
                case JobStartedEvent jobStarted:
                    ValidateJobStartedEvent(jobStarted, errors);
                    break;

                case JobCompletedEvent jobCompleted:
                    ValidateJobCompletedEvent(jobCompleted, errors);
                    break;

                case JobFailedEvent jobFailed:
                    ValidateJobFailedEvent(jobFailed, errors);
                    break;

                case ImageProcessingStartedEvent imageStarted:
                    ValidateImageProcessingStartedEvent(imageStarted, errors);
                    break;

                case ImageProcessingCompletedEvent imageCompleted:
                    ValidateImageProcessingCompletedEvent(imageCompleted, errors);
                    break;

                case ImageProcessingFailedEvent imageFailed:
                    ValidateImageProcessingFailedEvent(imageFailed, errors);
                    break;

                case DeviceStatusChangedEvent deviceStatus:
                    ValidateDeviceStatusChangedEvent(deviceStatus, errors);
                    break;

                case PerformanceDegradedEvent performance:
                    ValidatePerformanceDegradedEvent(performance, errors);
                    break;

                case ResourceLimitReachedEvent resourceLimit:
                    ValidateResourceLimitReachedEvent(resourceLimit, errors);
                    break;

                case CacheOperationEvent cacheOperation:
                    ValidateCacheOperationEvent(cacheOperation, errors);
                    break;

                case BatchProgressEvent batchProgress:
                    ValidateBatchProgressEvent(batchProgress, errors);
                    break;

                case HealthCheckEvent healthCheck:
                    ValidateHealthCheckEvent(healthCheck, errors);
                    break;
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified processing event is valid.
        /// </summary>
        /// <param name="value">The processing event to check.</param>
        /// <returns>True if the event is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public static bool IsValid(this ProcessingEvent? value) => value?.Validate().Count == 0;

        /// <summary>
        /// Ensures that the specified processing event is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The processing event to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the event is invalid, with a detailed message listing all validation problems.</exception>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public static void EnsureValid(this ProcessingEvent? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Processing event validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }

        private static void ValidateJobStartedEvent(JobStartedEvent jobStarted, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(jobStarted.JobId))
            {
                errors.Add("JobStartedEvent.JobId must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(jobStarted.JobName))
            {
                errors.Add("JobStartedEvent.JobName must not be null or whitespace.");
            }

            if (jobStarted.TotalImages < 0)
            {
                errors.Add("JobStartedEvent.TotalImages must be a non-negative integer.");
            }

            if (jobStarted.FilterIds == null)
            {
                errors.Add("JobStartedEvent.FilterIds list must not be null.");
            }
            else if (jobStarted.FilterIds.Contains(null))
            {
                errors.Add("JobStartedEvent.FilterIds contains null entries.");
            }

            if (jobStarted.TransformIds == null)
            {
                errors.Add("JobStartedEvent.TransformIds list must not be null.");
            }
            else if (jobStarted.TransformIds.Contains(null))
            {
                errors.Add("JobStartedEvent.TransformIds contains null entries.");
            }
        }

        private static void ValidateJobCompletedEvent(JobCompletedEvent jobCompleted, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(jobCompleted.JobId))
            {
                errors.Add("JobCompletedEvent.JobId must not be null or whitespace.");
            }

            if (jobCompleted.ProcessedImages < 0)
            {
                errors.Add("JobCompletedEvent.ProcessedImages must be a non-negative integer.");
            }

            if (jobCompleted.FailedImages < 0)
            {
                errors.Add("JobCompletedEvent.FailedImages must be a non-negative integer.");
            }

            if (jobCompleted.DurationMs < 0)
            {
                errors.Add("JobCompletedEvent.DurationMs must be a non-negative long integer.");
            }

            if (jobCompleted.Success && jobCompleted.FailedImages > 0)
            {
                errors.Add("JobCompletedEvent cannot be marked as successful when FailedImages is greater than 0.");
            }
        }

        private static void ValidateJobFailedEvent(JobFailedEvent jobFailed, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(jobFailed.JobId))
            {
                errors.Add("JobFailedEvent.JobId must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(jobFailed.ErrorMessage))
            {
                errors.Add("JobFailedEvent.ErrorMessage must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(jobFailed.ErrorCode))
            {
                errors.Add("JobFailedEvent.ErrorCode must not be null or whitespace.");
            }

            if (jobFailed.AttemptNumber < 1)
            {
                errors.Add("JobFailedEvent.AttemptNumber must be a positive integer (minimum 1).");
            }

            if (jobFailed.Retryable && string.IsNullOrWhiteSpace(jobFailed.ErrorCode))
            {
                errors.Add("JobFailedEvent.ErrorCode must be specified when Retryable is true.");
            }
        }

        private static void ValidateImageProcessingStartedEvent(ImageProcessingStartedEvent imageStarted, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(imageStarted.ImageId))
            {
                errors.Add("ImageProcessingStartedEvent.ImageId must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(imageStarted.ImagePath))
            {
                errors.Add("ImageProcessingStartedEvent.ImagePath must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(imageStarted.FilterName))
            {
                errors.Add("ImageProcessingStartedEvent.FilterName must not be null or whitespace.");
            }

            if (imageStarted.ImageWidth <= 0)
            {
                errors.Add("ImageProcessingStartedEvent.ImageWidth must be a positive integer.");
            }

            if (imageStarted.ImageHeight <= 0)
            {
                errors.Add("ImageProcessingStartedEvent.ImageHeight must be a positive integer.");
            }
        }

        private static void ValidateImageProcessingCompletedEvent(ImageProcessingCompletedEvent imageCompleted, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(imageCompleted.ImageId))
            {
                errors.Add("ImageProcessingCompletedEvent.ImageId must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(imageCompleted.OutputPath))
            {
                errors.Add("ImageProcessingCompletedEvent.OutputPath must not be null or whitespace.");
            }

            if (imageCompleted.ProcessedSize < 0)
            {
                errors.Add("ImageProcessingCompletedEvent.ProcessedSize must be a non-negative long integer.");
            }

            if (imageCompleted.DurationMs < 0)
            {
                errors.Add("ImageProcessingCompletedEvent.DurationMs must be a non-negative long integer.");
            }

            if (imageCompleted.ProcessingMetrics == null)
            {
                errors.Add("ImageProcessingCompletedEvent.ProcessingMetrics dictionary must not be null.");
            }
        }

        private static void ValidateImageProcessingFailedEvent(ImageProcessingFailedEvent imageFailed, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(imageFailed.ImageId))
            {
                errors.Add("ImageProcessingFailedEvent.ImageId must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(imageFailed.ErrorMessage))
            {
                errors.Add("ImageProcessingFailedEvent.ErrorMessage must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(imageFailed.ErrorCode))
            {
                errors.Add("ImageProcessingFailedEvent.ErrorCode must not be null or whitespace.");
            }

            if (imageFailed.AttemptDurationMs < 0)
            {
                errors.Add("ImageProcessingFailedEvent.AttemptDurationMs must be a non-negative long integer.");
            }
        }

        private static void ValidateDeviceStatusChangedEvent(DeviceStatusChangedEvent deviceStatus, List<string> errors)
        {
            if (deviceStatus.DeviceId <= 0)
            {
                errors.Add("DeviceStatusChangedEvent.DeviceId must be a positive integer.");
            }

            if (string.IsNullOrWhiteSpace(deviceStatus.DeviceName))
            {
                errors.Add("DeviceStatusChangedEvent.DeviceName must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(deviceStatus.PreviousStatus))
            {
                errors.Add("DeviceStatusChangedEvent.PreviousStatus must not be null or whitespace.");
            }

            if (string.IsNullOrWhiteSpace(deviceStatus.CurrentStatus))
            {
                errors.Add("DeviceStatusChangedEvent.CurrentStatus must not be null or whitespace.");
            }
        }

        private static void ValidatePerformanceDegradedEvent(PerformanceDegradedEvent performance, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(performance.Reason))
            {
                errors.Add("PerformanceDegradedEvent.Reason must not be null or whitespace.");
            }

            if (performance.CurrentThroughput < 0)
            {
                errors.Add("PerformanceDegradedEvent.CurrentThroughput must be a non-negative double.");
            }

            if (performance.ExpectedThroughput <= 0)
            {
                errors.Add("PerformanceDegradedEvent.ExpectedThroughput must be a positive double.");
            }

            if (performance.Metrics == null)
            {
                errors.Add("PerformanceDegradedEvent.Metrics dictionary must not be null.");
            }
        }

        private static void ValidateResourceLimitReachedEvent(ResourceLimitReachedEvent resourceLimit, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(resourceLimit.ResourceType))
            {
                errors.Add("ResourceLimitReachedEvent.ResourceType must not be null or whitespace.");
            }

            if (resourceLimit.CurrentUsage < 0)
            {
                errors.Add("ResourceLimitReachedEvent.CurrentUsage must be a non-negative long integer.");
            }

            if (resourceLimit.MaxLimit <= 0)
            {
                errors.Add("ResourceLimitReachedEvent.MaxLimit must be a positive long integer.");
            }

            if (resourceLimit.UtilizationPercent < 0 || resourceLimit.UtilizationPercent > 100)
            {
                errors.Add("ResourceLimitReachedEvent.UtilizationPercent must be between 0 and 100 inclusive.");
            }
        }

        private static void ValidateCacheOperationEvent(CacheOperationEvent cacheOperation, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(cacheOperation.Operation))
            {
                errors.Add("CacheOperationEvent.Operation must not be null or whitespace.");
            }
            else if (cacheOperation.Operation != "hit" && cacheOperation.Operation != "miss" &&
                     cacheOperation.Operation != "evict" && cacheOperation.Operation != "clear")
            {
                errors.Add("CacheOperationEvent.Operation must be one of: 'hit', 'miss', 'evict', 'clear'.");
            }

            if (string.IsNullOrWhiteSpace(cacheOperation.CacheKey))
            {
                errors.Add("CacheOperationEvent.CacheKey must not be null or whitespace.");
            }

            if (cacheOperation.CacheSize < 0)
            {
                errors.Add("CacheOperationEvent.CacheSize must be a non-negative long integer.");
            }

            if (cacheOperation.HitRate < 0 || cacheOperation.HitRate > 1)
            {
                errors.Add("CacheOperationEvent.HitRate must be between 0 and 1 inclusive.");
            }
        }

        private static void ValidateBatchProgressEvent(BatchProgressEvent batchProgress, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(batchProgress.BatchId))
            {
                errors.Add("BatchProgressEvent.BatchId must not be null or whitespace.");
            }

            if (batchProgress.ProcessedCount < 0)
            {
                errors.Add("BatchProgressEvent.ProcessedCount must be a non-negative integer.");
            }

            if (batchProgress.TotalCount <= 0)
            {
                errors.Add("BatchProgressEvent.TotalCount must be a positive integer.");
            }

            if (batchProgress.ProcessedCount > batchProgress.TotalCount)
            {
                errors.Add("BatchProgressEvent.ProcessedCount cannot exceed TotalCount.");
            }

            if (batchProgress.ProgressPercent < 0 || batchProgress.ProgressPercent > 100)
            {
                errors.Add("BatchProgressEvent.ProgressPercent must be between 0 and 100 inclusive.");
            }

            if (batchProgress.EstimatedRemainingMs < 0)
            {
                errors.Add("BatchProgressEvent.EstimatedRemainingMs must be a non-negative long integer.");
            }
        }

        private static void ValidateHealthCheckEvent(HealthCheckEvent healthCheck, List<string> errors)
        {
            if (healthCheck.IsHealthy && healthCheck.ComponentHealth?.Values.Any(v => !v) == true)
            {
                errors.Add("HealthCheckEvent cannot be marked as healthy when any component is unhealthy.");
            }

            if (string.IsNullOrWhiteSpace(healthCheck.HealthStatus))
            {
                errors.Add("HealthCheckEvent.HealthStatus must not be null or whitespace.");
            }

            if (healthCheck.ComponentHealth == null)
            {
                errors.Add("HealthCheckEvent.ComponentHealth dictionary must not be null.");
            }

            if (healthCheck.Issues == null)
            {
                errors.Add("HealthCheckEvent.Issues list must not be null.");
            }
            else if (healthCheck.Issues.Contains(null))
            {
                errors.Add("HealthCheckEvent.Issues contains null entries.");
            }
        }
    }
}