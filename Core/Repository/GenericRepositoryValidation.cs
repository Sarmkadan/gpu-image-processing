#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Validation helpers for <see cref="GenericRepository{T}"/>
    /// </summary>
    public static class GenericRepositoryValidation
    {
        /// <summary>
        /// Validates a <see cref="GenericRepository{T}"/> instance
        /// </summary>
        /// <param name="value">The instance to validate</param>
        /// <returns>A list of human-readable problems</returns>
        public static IReadOnlyList<string> Validate(this GenericRepository? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No specific validation rules for GenericRepository

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a <see cref="GenericRepository{T}"/> instance is valid
        /// </summary>
        /// <param name="value">The instance to check</param>
        /// <returns>True if the instance is valid; otherwise, false</returns>
        public static bool IsValid(this GenericRepository? value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures a <see cref="GenericRepository{T}"/> instance is valid
        /// </summary>
        /// <param name="value">The instance to ensure</param>
        /// <exception cref="ArgumentException">If the instance is not valid</exception>
        public static void EnsureValid(this GenericRepository? value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Invalid GenericRepository: {string.Join(", ", problems)}");
            }
        }
    }
}
