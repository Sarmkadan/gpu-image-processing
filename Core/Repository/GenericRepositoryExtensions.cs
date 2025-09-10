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
    /// Provides extension methods for the <see cref="GenericRepository{T}"/> class.
    /// </summary>
    public static class GenericRepositoryExtensions
    {
        /// <summary>
        /// Retrieves all entities that match the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="repository">The <see cref="GenericRepository{T}"/> to query.</param>
        /// <param name="predicate">The condition to apply.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of entities that match the predicate.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> or <paramref name="predicate"/> is null.</exception>
        public static async Task<IReadOnlyList<T>> FindAllAsync<T>(this GenericRepository<T> repository, Func<T, bool> predicate) 
            where T : class
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(predicate);

            var allEntities = await repository.GetAllAsync();
            return allEntities.Where(predicate).ToList().AsReadOnly();
        }

        /// <summary>
        /// Checks if there are any entities that match the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="repository">The <see cref="GenericRepository{T}"/> to query.</param>
        /// <param name="predicate">The condition to apply.</param>
        /// <returns><see langword="true"/> if any entities match the predicate; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> or <paramref name="predicate"/> is null.</exception>
        public static async Task<bool> AnyAsync<T>(this GenericRepository<T> repository, Func<T, bool> predicate) 
            where T : class
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(predicate);

            var entities = await repository.FindAllAsync(predicate);
            return entities.Count > 0;
        }

        /// <summary>
        /// Gets the first entity that matches the specified predicate, or a default value if no entity is found.
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="repository">The <see cref="GenericRepository{T}"/> to query.</param>
        /// <param name="predicate">The condition to apply.</param>
        /// <returns>The first matching entity, or <see langword="default"/> if none are found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> or <paramref name="predicate"/> is null.</exception>
        public static async Task<T?> FirstOrDefaultAsync<T>(this GenericRepository<T> repository, Func<T, bool> predicate) 
            where T : class
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(predicate);

            var entities = await repository.FindAllAsync(predicate);
            return entities.FirstOrDefault();
        }
    }
}
