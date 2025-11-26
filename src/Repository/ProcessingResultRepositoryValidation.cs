using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Repository
{
    /// <summary>
    /// Validation helpers for <see cref="ProcessingResultRepository"/>.
    /// </summary>
    public static class ProcessingResultRepositoryValidation
    {
        /// <summary>
        /// Validates a <see cref="ProcessingResultRepository"/> instance.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this ProcessingResultRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate public members are not null
            if (value.GetByIdAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetByIdAsync)} cannot be null.");
            }

            if (value.GetAllAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetAllAsync)} cannot be null.");
            }

            if (value.GetByCriteriaAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetByCriteriaAsync)} cannot be null.");
            }

            if (value.CreateAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.CreateAsync)} cannot be null.");
            }

            if (value.UpdateAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.UpdateAsync)} cannot be null.");
            }

            if (value.DeleteAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.DeleteAsync)} cannot be null.");
            }

            if (value.ExistsAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.ExistsAsync)} cannot be null.");
            }

            if (value.CountAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.CountAsync)} cannot be null.");
            }

            if (value.GetPagedAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetPagedAsync)} cannot be null.");
            }

            if (value.GetByImageIdAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetByImageIdAsync)} cannot be null.");
            }

            if (value.GetByStatusAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetByStatusAsync)} cannot be null.");
            }

            if (value.GetSuccessfulResultsAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetSuccessfulResultsAsync)} cannot be null.");
            }

            if (value.GetFailedResultsAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetFailedResultsAsync)} cannot be null.");
            }

            if (value.GetCompletedBetweenAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetCompletedBetweenAsync)} cannot be null.");
            }

            if (value.GetSlowestResultsAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetSlowestResultsAsync)} cannot be null.");
            }

            if (value.GetAverageProcessingTimeAsync == null)
            {
                problems.Add($"{nameof(ProcessingResultRepository)}.{nameof(value.GetAverageProcessingTimeAsync)} cannot be null.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="ProcessingResultRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ProcessingResultRepository value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="ProcessingResultRepository"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the repository is not valid.</exception>
        public static void EnsureValid(this ProcessingResultRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"ProcessingResultRepository is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}