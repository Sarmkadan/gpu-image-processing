// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Generic in-memory repository implementation for data access
    /// </summary>
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected List<T> _entities = new();
        protected object _lockObject = new();

        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        public virtual Task<T?> GetByIdAsync(Guid id)
        {
            lock (_lockObject)
            {
                var entity = _entities.FirstOrDefault(e => GetId(e) == id);
                return Task.FromResult(entity);
            }
        }

        /// <summary>
        /// Gets all entities
        /// </summary>
        public virtual Task<IEnumerable<T>> GetAllAsync()
        {
            lock (_lockObject)
            {
                return Task.FromResult(_entities.AsEnumerable());
            }
        }

        /// <summary>
        /// Adds a new entity
        /// </summary>
        public virtual Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lockObject)
            {
                _entities.Add(entity);
                return Task.FromResult(entity);
            }
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        public virtual Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            lock (_lockObject)
            {
                var id = GetId(entity);
                var existingIndex = _entities.FindIndex(e => GetId(e) == id);
                if (existingIndex >= 0)
                {
                    _entities[existingIndex] = entity;
                }
                return Task.FromResult(entity);
            }
        }

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        public virtual Task<bool> DeleteAsync(Guid id)
        {
            lock (_lockObject)
            {
                var entity = _entities.FirstOrDefault(e => GetId(e) == id);
                if (entity != null)
                {
                    _entities.Remove(entity);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        public virtual Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
                return Task.FromResult(false);

            lock (_lockObject)
            {
                bool removed = _entities.Remove(entity);
                return Task.FromResult(removed);
            }
        }

        /// <summary>
        /// Checks if an entity with the given ID exists
        /// </summary>
        public virtual Task<bool> ExistsAsync(Guid id)
        {
            lock (_lockObject)
            {
                bool exists = _entities.Any(e => GetId(e) == id);
                return Task.FromResult(exists);
            }
        }

        /// <summary>
        /// Gets the total count of entities
        /// </summary>
        public virtual Task<int> CountAsync()
        {
            lock (_lockObject)
            {
                return Task.FromResult(_entities.Count);
            }
        }

        /// <summary>
        /// Gets entities with pagination
        /// </summary>
        public virtual Task<IEnumerable<T>> GetPageAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("Page number and size must be >= 1");

            lock (_lockObject)
            {
                var page = _entities
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                return Task.FromResult(page.AsEnumerable());
            }
        }

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        public virtual Task<int> SaveChangesAsync()
        {
            lock (_lockObject)
            {
                return Task.FromResult(_entities.Count);
            }
        }

        /// <summary>
        /// Gets the ID from an entity using reflection
        /// </summary>
        protected virtual Guid GetId(T entity)
        {
            var idProperty = entity?.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                return (Guid)(idProperty.GetValue(entity) ?? Guid.Empty);
            }
            return Guid.Empty;
        }

        /// <summary>
        /// Clears all entities
        /// </summary>
        public virtual void Clear()
        {
            lock (_lockObject)
            {
                _entities.Clear();
            }
        }
    }
}
