using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Domain
{
    /// <summary>
    /// Provides validation helpers for <see cref="FilterChainTests"/> instances.
    /// </summary>
    public static class FilterChainTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="FilterChainTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this FilterChainTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // FilterChainTests is a test class with no actual properties to validate
            // The validation simply ensures the test class instance is properly initialized

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="FilterChainTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this FilterChainTests? value)
            => Validate(value).Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="FilterChainTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
        public static void EnsureValid(this FilterChainTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"FilterChainTests instance is not valid. Problems: {string.Join(", ", errors)}");
            }
        }
    }
}
