using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GpuImageProcessing.Benchmarks
{
    /// <summary>
    /// Validation helpers for <see cref="EnumerableExtensionsBenchmarks"/>.
    /// </summary>
    public static class EnumerableExtensionsBenchmarksValidation
    {
        /// <summary>
        /// Validates the given <paramref name="value"/> and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>A list of human-readable problems.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this EnumerableExtensionsBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            try
            {
                var shuffle32 = value.Shuffle_32Items();
                if (shuffle32 == null)
                {
                    problems.Add("Shuffle_32Items returned null");
                }
                else if (!shuffle32.Any())
                {
                    problems.Add("Shuffle_32Items returned an empty sequence");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Shuffle_32Items threw an exception: {ex.Message}");
            }

            try
            {
                var shuffle1024 = value.Shuffle_1024Items();
                if (shuffle1024 == null)
                {
                    problems.Add("Shuffle_1024Items returned null");
                }
                else if (!shuffle1024.Any())
                {
                    problems.Add("Shuffle_1024Items returned an empty sequence");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Shuffle_1024Items threw an exception: {ex.Message}");
            }

            try
            {
                var batch32 = value.Batch_1000By32();
                if (batch32 < 0)
                {
                    problems.Add("Batch_1000By32 returned a negative value");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Batch_1000By32 threw an exception: {ex.Message}");
            }

            try
            {
                var batch8 = value.Batch_1000By8();
                if (batch8 < 0)
                {
                    problems.Add("Batch_1000By8 returned a negative value");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Batch_1000By8 threw an exception: {ex.Message}");
            }

            try
            {
                var distinctBy = value.DistinctBy_1000Strings();
                if (distinctBy < 0)
                {
                    problems.Add("DistinctBy_1000Strings returned a negative value");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"DistinctBy_1000Strings threw an exception: {ex.Message}");
            }

            try
            {
                var safeToDictionary = value.SafeToDictionary_1000Items();
                if (safeToDictionary == null)
                {
                    problems.Add("SafeToDictionary_1000Items returned null");
                }
                else if (safeToDictionary.Count == 0)
                {
                    problems.Add("SafeToDictionary_1000Items returned an empty dictionary");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"SafeToDictionary_1000Items threw an exception: {ex.Message}");
            }

            return new ReadOnlyCollection<string>(problems);
        }

        /// <summary>
        /// Checks if the given <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this EnumerableExtensionsBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the given <paramref name="value"/> is valid, throwing an exception if it's not.
        /// </summary>
        /// <param name="value">The value to ensure.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this EnumerableExtensionsBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Invalid EnumerableExtensionsBenchmarks: {string.Join(", ", problems)}", nameof(value));
            }
        }
    }
}
