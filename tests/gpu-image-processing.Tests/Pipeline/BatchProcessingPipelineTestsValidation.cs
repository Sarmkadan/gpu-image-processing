#nullable enable

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Pipeline
{
    /// <summary>
    /// Provides validation helpers for <see cref="BatchProcessingPipelineTests"/> instances.
    /// This static class offers extension methods to validate test fixture instances.
    /// </summary>
    public static class BatchProcessingPipelineTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="BatchProcessingPipelineTests"/> instance.
        /// </summary>
        /// <remarks>
        /// BatchProcessingPipelineTests is a test fixture class that doesn't expose
        /// any mutable state to validate. This method always returns an empty list
        /// indicating the instance is valid.
        /// </remarks>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BatchProcessingPipelineTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BatchProcessingPipelineTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this BatchProcessingPipelineTests? value)
            => Validate(value).Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="BatchProcessingPipelineTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the instance is not valid, containing a list of validation problems.
        /// </exception>
        public static void EnsureValid(this BatchProcessingPipelineTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"BatchProcessingPipelineTests instance is not valid. Problems: {string.Join(", ", errors)}");
            }
        }
    }
}