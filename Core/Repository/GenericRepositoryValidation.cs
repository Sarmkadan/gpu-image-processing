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
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="value">The instance to validate</param>
        /// <returns>A list of human-readable problems</returns>
        public static IReadOnlyList<string> Validate<T>(this GenericRepository<T>? value) where T : class
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Check for invalid state
            if (value.GetAllAsync().Result == null)
            {
                problems.Add("Entities list is null");
            }
            else
            {
                var entities = value.GetAllAsync().Result.ToList();
                if (entities.Any(e => e == null))
                {
                    problems.Add("Entities list contains null items");
                }
            }

            // No more specific validation rules for GenericRepository

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a <see cref="GenericRepository{T}"/> instance is valid
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="value">The instance to check</param>
        /// <returns>True if the instance is valid; otherwise, false</returns>
        public static bool IsValid<T>(this GenericRepository<T>? value) where T : class => Validate(value).Count == 0;

        /// <summary>
        /// Ensures a <see cref="GenericRepository{T}"/> instance is valid
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="value">The instance to ensure</param>
        /// <exception cref="ArgumentException">If the instance is not valid</exception>
        public static void EnsureValid<T>(this GenericRepository<T>? value) where T : class
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Invalid GenericRepository: {string.Join(", ", problems)}");
            }
        }
    }
}
