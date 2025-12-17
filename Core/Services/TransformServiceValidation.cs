#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Validation helpers for <see cref="TransformService"/>.
    /// </summary>
    public static class TransformServiceValidation
    {
        /// <summary>
        /// Validates the <see cref="TransformService"/> instance.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <returns>
        /// A read‑only list of validation problem descriptions.
        /// The list is empty when the instance is considered valid.
        /// </returns>
        public static IReadOnlyList<string> Validate(this TransformService? value)
        {
            // The only observable state of the service is its existence;
            // all other members are methods that do not expose mutable public properties.
            return value is null
                ? new[] { "TransformService instance is null." }
                : Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the <see cref="TransformService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to check.</param>
        /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
        public static bool IsValid(this TransformService? value) => !value.Validate().Any();

        /// <summary>
        /// Ensures that the <see cref="TransformService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when one or more validation problems are found.
        /// The exception message contains a semicolon‑separated list of problems.
        /// </exception>
        public static void EnsureValid(this TransformService? value)
        {
            var problems = value.Validate();
            if (problems.Any())
                throw new ArgumentException(string.Join("; ", problems));
        }
    }
}
