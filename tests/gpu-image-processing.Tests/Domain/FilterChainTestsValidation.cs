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
        /// <remarks>
        /// This method currently returns an empty list as <see cref="FilterChainTests"/> is a test class
        /// with no validation-worthy properties. The method is retained for API consistency and potential
        /// future extension.
        /// </remarks>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this FilterChainTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="FilterChainTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this FilterChainTests? value)
            => value is not null;

        /// <summary>
        /// Ensures that the specified <see cref="FilterChainTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this FilterChainTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);
        }
    }
}
