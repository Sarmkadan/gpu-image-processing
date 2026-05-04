// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Repository;

/// <summary>
/// Repository for ProcessingResult entity with specialized queries.
/// </summary>
public class ProcessingResultRepository : IRepository<ProcessingResult>
{
    private readonly List<ProcessingResult> _storage = [];
    private readonly object _lockObject = new();

    public Task<ProcessingResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var result = _storage.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(result);
        }
    }

    public Task<IEnumerable<ProcessingResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult<IEnumerable<ProcessingResult>>(_storage.ToList());
        }
    }

    public Task<IEnumerable<ProcessingResult>> GetByCriteriaAsync(Func<ProcessingResult, bool> predicate, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _storage.Where(predicate).ToList();
            return Task.FromResult<IEnumerable<ProcessingResult>>(results);
        }
    }

    public Task<ProcessingResult> CreateAsync(ProcessingResult entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lockObject)
        {
            entity.Id = Guid.NewGuid();
            _storage.Add(entity);
            return Task.FromResult(entity);
        }
    }

    public Task<ProcessingResult> UpdateAsync(ProcessingResult entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lockObject)
        {
            var existing = _storage.FirstOrDefault(r => r.Id == entity.Id);
            if (existing == null)
                throw new InvalidOperationException($"Result with ID {entity.Id} not found");

            var index = _storage.IndexOf(existing);
            _storage[index] = entity;
            return Task.FromResult(entity);
        }
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var result = _storage.FirstOrDefault(r => r.Id == id);
            if (result == null)
                return Task.FromResult(false);

            return Task.FromResult(_storage.Remove(result));
        }
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var exists = _storage.Any(r => r.Id == id);
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

    public Task<IEnumerable<ProcessingResult>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException("Page number and size must be greater than 0");

        lock (_lockObject)
        {
            var results = _storage
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult<IEnumerable<ProcessingResult>>(results);
        }
    }

    /// <summary>
    /// Gets results by image ID.
    /// </summary>
    public Task<IEnumerable<ProcessingResult>> GetByImageIdAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(r => r.ImageId == imageId, cancellationToken);
    }

    /// <summary>
    /// Gets results by processing status.
    /// </summary>
    public Task<IEnumerable<ProcessingResult>> GetByStatusAsync(ProcessingStatus status, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(r => r.Status == status, cancellationToken);
    }

    /// <summary>
    /// Gets successful processing results.
    /// </summary>
    public Task<IEnumerable<ProcessingResult>> GetSuccessfulResultsAsync(CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(r => r.IsSuccessful, cancellationToken);
    }

    /// <summary>
    /// Gets failed processing results with error details.
    /// </summary>
    public Task<IEnumerable<ProcessingResult>> GetFailedResultsAsync(CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(r => !r.IsSuccessful, cancellationToken);
    }

    /// <summary>
    /// Gets results completed within a date range.
    /// </summary>
    public Task<IEnumerable<ProcessingResult>> GetCompletedBetweenAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(r =>
            r.CompletedAt >= startDate && r.CompletedAt <= endDate && r.IsSuccessful,
            cancellationToken);
    }

    /// <summary>
    /// Gets processing results sorted by execution time.
    /// </summary>
    public Task<IEnumerable<ProcessingResult>> GetSlowestResultsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _storage
                .OrderByDescending(r => r.ProcessingTimeMilliseconds)
                .Take(count)
                .ToList();
            return Task.FromResult<IEnumerable<ProcessingResult>>(results);
        }
    }

    /// <summary>
    /// Gets average processing time for successful operations.
    /// </summary>
    public Task<double> GetAverageProcessingTimeAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            if (!_storage.Any(r => r.IsSuccessful))
                return Task.FromResult(0.0);

            var average = _storage
                .Where(r => r.IsSuccessful)
                .Average(r => r.ProcessingTimeMilliseconds);

            return Task.FromResult(average);
        }
    }
}
