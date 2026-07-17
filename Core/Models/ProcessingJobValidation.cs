#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides validation helpers for <see cref="ProcessingJob"/> instances
    /// </summary>
    public static class ProcessingJobValidation
    {
        /// <summary>
        /// Validates a ProcessingJob instance and returns a list of validation errors
        /// </summary>
        /// <param name="value">The ProcessingJob to validate</param>
        /// <returns>An empty list if valid, otherwise a list of human-readable error messages</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this ProcessingJob value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate required string properties
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Name cannot be null or whitespace.");
            }
            else if (value.Name.Length > 200)
            {
                errors.Add("Name cannot exceed 200 characters.");
            }

            // Validate Description
            if (string.IsNullOrWhiteSpace(value.Description))
            {
                errors.Add("Description cannot be null or whitespace.");
            }
            else if (value.Description.Length > 2000)
            {
                errors.Add("Description cannot exceed 2000 characters.");
            }

            // Validate Status enum value
            if (!Enum.IsDefined(typeof(ProcessingStatus), value.Status))
            {
                errors.Add("Status must be a valid ProcessingStatus enum value.");
            }

            // Validate ImageIds collection
            ArgumentNullException.ThrowIfNull(value.ImageIds);
            if (value.ImageIds.Count > 10000)
            {
                errors.Add("ImageIds cannot contain more than 10000 items.");
            }

            // Validate FilterIds collection
            ArgumentNullException.ThrowIfNull(value.FilterIds);
            if (value.FilterIds.Count > 1000)
            {
                errors.Add("FilterIds cannot contain more than 1000 items.");
            }

            // Validate TransformIds collection
            ArgumentNullException.ThrowIfNull(value.TransformIds);
            if (value.TransformIds.Count > 1000)
            {
                errors.Add("TransformIds cannot contain more than 1000 items.");
            }

            // Validate CreatedAt date
            if (value.CreatedAt == default)
            {
                errors.Add("CreatedAt must be set to a non-default DateTime.");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CreatedAt cannot be in the future.");
            }

            // Validate StartedAt date (if set)
            if (value.StartedAt.HasValue)
            {
                if (value.StartedAt.Value == default)
                {
                    errors.Add("StartedAt cannot be default DateTime when set.");
                }
                else if (value.StartedAt.Value < value.CreatedAt)
                {
                    errors.Add("StartedAt cannot be earlier than CreatedAt.");
                }
                else if (value.StartedAt.Value > DateTime.UtcNow.AddMinutes(5))
                {
                    errors.Add("StartedAt cannot be in the future.");
                }
            }

            // Validate CompletedAt date (if set)
            if (value.CompletedAt.HasValue)
            {
                if (value.CompletedAt.Value == default)
                {
                    errors.Add("CompletedAt cannot be default DateTime when set.");
                }
                else if (value.CompletedAt.Value < (value.StartedAt ?? value.CreatedAt))
                {
                    errors.Add("CompletedAt cannot be earlier than StartedAt or CreatedAt.");
                }
                else if (value.CompletedAt.Value > DateTime.UtcNow.AddMinutes(5))
                {
                    errors.Add("CompletedAt cannot be in the future.");
                }
            }

            // Validate ProgressPercentage
            if (value.ProgressPercentage < 0f || value.ProgressPercentage > 100f)
            {
                errors.Add("ProgressPercentage must be between 0 and 100 inclusive.");
            }

            // Validate TotalImages
            if (value.TotalImages < 0)
            {
                errors.Add("TotalImages cannot be negative.");
            }

            // Validate ProcessedImages
            if (value.ProcessedImages < 0)
            {
                errors.Add("ProcessedImages cannot be negative.");
            }
            else if (value.TotalImages >= 0 && value.ProcessedImages > value.TotalImages)
            {
                errors.Add("ProcessedImages cannot exceed TotalImages.");
            }

            // Validate FailedImages
            if (value.FailedImages < 0)
            {
                errors.Add("FailedImages cannot be negative.");
            }
            else if (value.TotalImages >= 0 && value.FailedImages > value.TotalImages)
            {
                errors.Add("FailedImages cannot exceed TotalImages.");
            }

            // Validate OutputDirectory
            if (string.IsNullOrWhiteSpace(value.OutputDirectory))
            {
                errors.Add("OutputDirectory cannot be null or whitespace.");
            }
            else if (value.OutputDirectory.Length > 2000)
            {
                errors.Add("OutputDirectory cannot exceed 2000 characters.");
            }

            // Validate JobMetadata collection
            ArgumentNullException.ThrowIfNull(value.JobMetadata);
            if (value.JobMetadata.Count > 1000)
            {
                errors.Add("JobMetadata cannot contain more than 1000 key-value pairs.");
            }

            // Validate ErrorMessage (if set)
            if (!string.IsNullOrWhiteSpace(value.ErrorMessage) && value.ErrorMessage.Length > 2000)
            {
                errors.Add("ErrorMessage cannot exceed 2000 characters when set.");
            }

            // Validate consistency between TotalImages, ProcessedImages, and FailedImages
            if (value.TotalImages >= 0 && value.ProcessedImages + value.FailedImages > value.TotalImages)
            {
                errors.Add("Sum of ProcessedImages and FailedImages cannot exceed TotalImages.");
            }

            // Validate that CompletedAt is set when Status is Completed
            if (value.Status == global::GpuImageProcessing.Core.Constants.ProcessingStatus.Completed && !value.CompletedAt.HasValue)
            {
                errors.Add("Completed jobs must have CompletedAt set.");
            }

            // Validate that Failed jobs have ErrorMessage set
            if (value.Status == global::GpuImageProcessing.Core.Constants.ProcessingStatus.Failed && string.IsNullOrWhiteSpace(value.ErrorMessage))
            {
                errors.Add("Failed jobs must have ErrorMessage set.");
            }

            // Validate that Running jobs have StartedAt set
            if (value.Status == global::GpuImageProcessing.Core.Constants.ProcessingStatus.Running && !value.StartedAt.HasValue)
            {
                errors.Add("Running jobs must have StartedAt set.");
            }

            // Validate that ProgressPercentage is 100 when job is Completed
            if (value.Status == global::GpuImageProcessing.Core.Constants.ProcessingStatus.Completed && value.ProgressPercentage != 100f)
            {
                errors.Add("Completed jobs must have ProgressPercentage equal to 100.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a ProcessingJob instance is valid
        /// </summary>
        /// <param name="value">The ProcessingJob to check</param>
        /// <returns>True if valid, otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static bool IsValid(this ProcessingJob value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a ProcessingJob instance is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The ProcessingJob to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages</exception>
        public static void EnsureValid(this ProcessingJob value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ProcessingJob validation failed:{Environment.NewLine}- {
                        string.Join(Environment.NewLine + "- ", errors)
                    }");
            }
        }
    }
}
