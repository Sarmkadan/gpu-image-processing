#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Provides validation extensions for <see cref="TransformService"/> instances.
    /// </summary>
    public static class TransformServiceValidation
    {
        /// <summary>
        /// Validates the <see cref="TransformService"/> instance.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <returns>
        /// A read-only list of validation problem descriptions.
        /// The list is empty when the instance is considered valid.
        /// </returns>
        /// <remarks>
        /// The only observable state of the service is its existence;
        /// all other members are methods that do not expose mutable public properties.
        /// </remarks>
        public static IReadOnlyList<string> Validate(this TransformService? value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the <see cref="TransformService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to check.</param>
        /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public static bool IsValid(this TransformService? value) => value?.Validate().Count == 0;

        /// <summary>
        /// Ensures that the <see cref="TransformService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more validation problems are found.
        /// The exception message contains a semicolon-separated list of problems.
        /// </exception>
        public static void EnsureValid(this TransformService? value)
        {
            var problems = value?.Validate() ?? throw new ArgumentNullException(nameof(value));

            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join("; ", problems));
            }
        }
    }
}
