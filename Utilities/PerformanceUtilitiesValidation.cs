#nullable enable
using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="PerformanceTimer"/>.
    /// </summary>
    public static class PerformanceUtilitiesValidation
    {
        /// <summary>
        /// Validates the <see cref="PerformanceTimer"/> instance.
        /// </summary>
        /// <param name="value">The timer instance to validate.</param>
        /// <returns>A list of validation problems. Returns empty list if timer is valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this PerformanceTimer value)
        {
            ArgumentNullException.ThrowIfNull(value);
            
            var problems = new List<string>();

            if (value.ElapsedMilliseconds < 0)
            {
                problems.Add("Elapsed time cannot be negative.");
            }

            return problems;
        }

        /// <summary>
        /// Checks if the <see cref="PerformanceTimer"/> instance is valid.
        /// </summary>
        /// <param name="value">The timer instance to validate.</param>
        /// <returns><see langword="true"/> if the timer is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this PerformanceTimer value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.ElapsedMilliseconds >= 0;
        }

        /// <summary>
        /// Ensures the <see cref="PerformanceTimer"/> instance is valid.
        /// </summary>
        /// <param name="value">The timer instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The timer has invalid elapsed time.</exception>
        public static void EnsureValid(this PerformanceTimer value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (value.ElapsedMilliseconds < 0)
            {
                throw new ArgumentException("PerformanceTimer has invalid elapsed time.", nameof(value));
            }
        }
    }
}
