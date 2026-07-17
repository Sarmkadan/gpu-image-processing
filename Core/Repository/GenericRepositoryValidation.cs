#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static async Task<IReadOnlyList<string>> ValidateAsync<T>(this GenericRepository<T>? value) where T : class
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Check for invalid state
            var entities = await value.GetAllAsync();
            if (entities == null)
            {
                problems.Add("Entities list is null");
            }
            else
            {
                if (entities.Any(e => e == null))
                {
                    problems.Add("Entities list contains null items");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a <see cref="GenericRepository{T}"/> instance is valid
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="value">The instance to check</param>
        /// <returns>True if the instance is valid; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static async Task<bool> IsValidAsync<T>(this GenericRepository<T>? value) where T : class
        {
            ArgumentNullException.ThrowIfNull(value);
            return (await ValidateAsync(value)).Count == 0;
        }

        /// <summary>
        /// Ensures a <see cref="GenericRepository{T}"/> instance is valid
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="value">The instance to ensure</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">If the instance is not valid</exception>
        public static async Task EnsureValidAsync<T>(this GenericRepository<T>? value) where T : class
        {
            var problems = await ValidateAsync(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Invalid GenericRepository: {string.Join(", ", problems)}");
            }
        }
    }
}