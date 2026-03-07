#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Repository;

/// <summary>
/// Repository for FilterConfiguration entity with specialized queries.
/// </summary>
public class FilterConfigurationRepository : IRepository<FilterConfiguration>
{
    private readonly List<FilterConfiguration> _storage = [];
    private readonly object _lockObject = new();

    public Task<FilterConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var filter = _storage.FirstOrDefault(f => f.Id == id);
            return Task.FromResult(filter);
        }
    }

    public Task<IEnumerable<FilterConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult<IEnumerable<FilterConfiguration>>(_storage.ToList());
        }
    }

    public Task<IEnumerable<FilterConfiguration>> GetByCriteriaAsync(Func<FilterConfiguration, bool> predicate, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _storage.Where(predicate).ToList();
            return Task.FromResult<IEnumerable<FilterConfiguration>>(results);
        }
    }

    public Task<FilterConfiguration> CreateAsync(FilterConfiguration entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.Validate())
            throw new InvalidFilterException("Filter configuration validation failed", entity.FilterType.ToString());

        lock (_lockObject)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
            _storage.Add(entity);
            return Task.FromResult(entity);
        }
    }

    public Task<FilterConfiguration> UpdateAsync(FilterConfiguration entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lockObject)
        {
            var existing = _storage.FirstOrDefault(f => f.Id == entity.Id);
            if (existing == null)
                throw new InvalidOperationException($"Filter with ID {entity.Id} not found");

            entity.ModifiedAt = DateTime.UtcNow;
            var index = _storage.IndexOf(existing);
            _storage[index] = entity;
            return Task.FromResult(entity);
        }
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var filter = _storage.FirstOrDefault(f => f.Id == id);
            if (filter == null)
                return Task.FromResult(false);

            return Task.FromResult(_storage.Remove(filter));
        }
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var exists = _storage.Any(f => f.Id == id);
            return Task.FromResult(exists);
        }
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_storage.Count);
        }
    }

    public Task<IEnumerable<FilterConfiguration>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException("Page number and size must be greater than 0");

        lock (_lockObject)
        {
            var results = _storage
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult<IEnumerable<FilterConfiguration>>(results);
        }
    }

    /// <summary>
    /// Gets filters by type.
    /// </summary>
    public Task<IEnumerable<FilterConfiguration>> GetByTypeAsync(FilterType type, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(f => f.FilterType == type, cancellationToken);
    }

    /// <summary>
    /// Gets active filters sorted by priority.
    /// </summary>
    public Task<IEnumerable<FilterConfiguration>> GetActiveFiltersAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _storage
                .Where(f => f.IsActive)
                .OrderBy(f => f.Priority)
                .ToList();
            return Task.FromResult<IEnumerable<FilterConfiguration>>(results);
        }
    }

    /// <summary>
    /// Gets filter by name.
    /// </summary>
    public Task<FilterConfiguration?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var filter = _storage.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(filter);
        }
    }

    /// <summary>
    /// Gets filters that support specific parameter.
    /// </summary>
    public Task<IEnumerable<FilterConfiguration>> GetByParameterAsync(string parameterKey, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(f => f.ParameterTypes.ContainsKey(parameterKey), cancellationToken);
    }

    /// <summary>
    /// Gets filters with complex kernels.
    /// </summary>
    public Task<IEnumerable<FilterConfiguration>> GetFiltersWithKernelAsync(CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(f => !string.IsNullOrEmpty(f.KernelCode), cancellationToken);
    }
}
