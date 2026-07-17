using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides validation methods for <see cref="BatchProcessingService"/> instances.
    /// </summary>
    public static class BatchProcessingServiceValidation
    {
        /// <summary>
        /// Validates the specified <see cref="BatchProcessingService"/> instance and its dependencies.
        /// </summary>
        /// <param name="value">The <see cref="BatchProcessingService"/> instance to validate.</param>
        /// <returns>A list of human-readable problems with the instance or its dependencies.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BatchProcessingService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate service dependencies through reflection to check for null fields
            // Since BatchProcessingService has private readonly fields that are validated in constructor,
            // we can only verify they're not null by attempting to use them
            try
            {
                // Trigger validation by accessing a method that uses the dependencies
                _ = value.GetActiveBatchCount();
            }
            catch (NullReferenceException)
            {
                problems.Add("Service dependencies contain null references. The service may not have been properly initialized.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BatchProcessingService"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="BatchProcessingService"/> instance to validate.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this BatchProcessingService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the specified <see cref="BatchProcessingService"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="BatchProcessingService"/> instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this BatchProcessingService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);

            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
            }
        }
    }
}