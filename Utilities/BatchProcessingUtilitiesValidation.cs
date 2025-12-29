using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="BatchProcessingUtilities"/> operations.
    /// </summary>
    public static class BatchProcessingUtilitiesValidation
    {
        /// <summary>
        /// Validates a <see cref="BatchItem{T}"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate<T>(this BatchItem<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.SequenceNumber < 0)
            {
                problems.Add($"SequenceNumber must be non-negative, but was {value.SequenceNumber}.");
            }

            if (value.Priority < 0)
            {
                problems.Add($"Priority must be non-negative, but was {value.Priority}.");
            }

            if (value.ScheduledAt == default)
            {
                problems.Add("ScheduledAt must be set to a valid DateTime.");
            }

            if (value.ScheduledAt > DateTime.UtcNow.AddHours(24))
            {
                problems.Add("ScheduledAt cannot be more than 24 hours in the future.");
            }

            if (value.ProcessedAt.HasValue && value.ProcessedAt.Value == default)
            {
                problems.Add("ProcessedAt must be null or a valid DateTime.");
            }

            if (value.ProcessedAt.HasValue && value.ProcessedAt.Value > DateTime.UtcNow)
            {
                problems.Add("ProcessedAt cannot be in the future.");
            }

            if (value.RetryCount < 0)
            {
                problems.Add($"RetryCount must be non-negative, but was {value.RetryCount}.");
            }

            if (value.LastError is { Length: > 500 })
            {
                problems.Add("LastError must not exceed 500 characters.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a <see cref="BatchProgress"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BatchProgress value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.ProcessedCount < 0)
            {
                problems.Add($"ProcessedCount must be non-negative, but was {value.ProcessedCount}.");
            }

            if (value.TotalCount <= 0)
            {
                problems.Add($"TotalCount must be positive, but was {value.TotalCount}.");
            }

            if (value.ProcessedCount > value.TotalCount)
            {
                problems.Add($"ProcessedCount ({value.ProcessedCount}) cannot exceed TotalCount ({value.TotalCount}).");
            }

            if (value.ErrorCount < 0)
            {
                problems.Add($"ErrorCount must be non-negative, but was {value.ErrorCount}.");
            }

            if (value.ErrorCount > value.TotalCount)
            {
                problems.Add($"ErrorCount ({value.ErrorCount}) cannot exceed TotalCount ({value.TotalCount}).");
            }

            if (value.PercentComplete < 0.0 || value.PercentComplete > 100.0)
            {
                problems.Add($"PercentComplete must be between 0.0 and 100.0, but was {value.PercentComplete.ToString(CultureInfo.InvariantCulture)}.");
            }

            if (value.ItemsPerSecond < 0.0)
            {
                problems.Add($"ItemsPerSecond must be non-negative, but was {value.ItemsPerSecond.ToString(CultureInfo.InvariantCulture)}.");
            }

            if (value.ElapsedTime < TimeSpan.Zero)
            {
                problems.Add($"ElapsedTime must be non-negative, but was {value.ElapsedTime}.");
            }

            if (value.EstimatedTimeRemaining.HasValue && value.EstimatedTimeRemaining.Value < TimeSpan.Zero)
            {
                problems.Add($"EstimatedTimeRemaining must be non-negative, but was {value.EstimatedTimeRemaining}.");
            }

            if (value.CompletionTime.HasValue && value.CompletionTime.Value > DateTime.UtcNow.AddHours(1))
            {
                problems.Add("CompletionTime cannot be more than 1 hour in the future.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a <see cref="ThrottleRecommendation"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read-only list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ThrottleRecommendation value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            if (value.ShouldThrottle && value.ThrottleLevel <= 0.0f)
            {
                problems.Add("ShouldThrottle is true but ThrottleLevel is not positive.");
            }

            if (value.ThrottleLevel < 0.0f || value.ThrottleLevel > 1.0f)
            {
                problems.Add($"ThrottleLevel must be between 0.0 and 1.0, but was {value.ThrottleLevel.ToString(CultureInfo.InvariantCulture)}.");
            }

            if (value.Reasons is null)
            {
                problems.Add("Reasons collection must not be null.");
            }
            else if (value.Reasons.Any(r => string.IsNullOrWhiteSpace(r)))
            {
                problems.Add("Reasons collection must not contain null or whitespace entries.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BatchItem{T}"/> instance is valid.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid<T>(this BatchItem<T> value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="BatchProgress"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this BatchProgress value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ThrottleRecommendation"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ThrottleRecommendation value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="BatchItem{T}"/> instance is valid.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid<T>(this BatchItem<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BatchItem<{typeof(T).Name}> instance is invalid. Problems:\n\n{string.Join("\n\n", problems)}");
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="BatchProgress"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this BatchProgress value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BatchProgress instance is invalid. Problems:\n\n{string.Join("\n\n", problems)}");
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="ThrottleRecommendation"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this ThrottleRecommendation value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ThrottleRecommendation instance is invalid. Problems:\n\n{string.Join("\n\n", problems)}");
            }
        }
    }
}