using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Services
{
    /// <summary>
    /// Validation helpers for <see cref="BatchProcessingServiceTests"/> test fixture.
    /// </summary>
    public static class BatchProcessingServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="BatchProcessingServiceTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BatchProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // BatchProcessingServiceTests is a test class with no public state to validate
            // All validation is handled by the test methods themselves
            // This class exists to maintain consistency with the validation pattern used throughout the codebase

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BatchProcessingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this BatchProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="BatchProcessingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this BatchProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BatchProcessingServiceTests is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}