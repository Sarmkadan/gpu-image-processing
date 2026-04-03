#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Checks if an entity with the given ID exists
        /// </summary>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Gets the total count of entities
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Gets entities with pagination
        /// </summary>
        Task<IEnumerable<T>> GetPageAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
