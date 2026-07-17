using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Repository
{
    /// <summary>
    /// Provides validation methods for <see cref="ProcessingResultRepository"/> instances.
    /// </summary>
    public static class ProcessingResultRepositoryValidation
    {
        /// <summary>
        /// Validates that the repository instance is not null.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ProcessingResultRepository? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether a <see cref="ProcessingResultRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ProcessingResultRepository? value)
        {
            return value is not null;
        }

        /// <summary>
        /// Ensures that a <see cref="ProcessingResultRepository"/> instance is valid, throwing an <see cref="ArgumentNullException"/> if not.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this ProcessingResultRepository? value)
        {
            ArgumentNullException.ThrowIfNull(value);
        }
    }
}