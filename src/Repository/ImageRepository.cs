// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Repository;

/// <summary>
/// Repository for Image entity with specialized queries.
/// </summary>
public class ImageRepository : IRepository<Image>
{
    private readonly List<Image> _storage = [];
    private readonly object _lockObject = new();

    public Task<Image?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var image = _storage.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(image);
        }
    }

    public Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return Task.FromResult<IEnumerable<Image>>(_storage.ToList());
        }
    }

    public Task<IEnumerable<Image>> GetByCriteriaAsync(Func<Image, bool> predicate, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var results = _storage.Where(predicate).ToList();
            return Task.FromResult<IEnumerable<Image>>(results);
        }
    }

    public Task<Image> CreateAsync(Image entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.Validate())
            throw new InvalidImageException("Image validation failed", entity.FilePath, entity.Format.ToString());

        lock (_lockObject)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
            _storage.Add(entity);
            return Task.FromResult(entity);
        }
    }

    public Task<Image> UpdateAsync(Image entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        lock (_lockObject)
        {
            var existing = _storage.FirstOrDefault(i => i.Id == entity.Id);
            if (existing == null)
                throw new InvalidOperationException($"Image with ID {entity.Id} not found");

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
            var image = _storage.FirstOrDefault(i => i.Id == id);
            if (image == null)
                return Task.FromResult(false);

            image.PixelData = null;
            var removed = _storage.Remove(image);
            return Task.FromResult(removed);
        }
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var exists = _storage.Any(i => i.Id == id);
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

    public Task<IEnumerable<Image>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException("Page number and size must be greater than 0");

        lock (_lockObject)
        {
            var results = _storage
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult<IEnumerable<Image>>(results);
        }
    }

    /// <summary>
    /// Gets images by processing status.
    /// </summary>
    public Task<IEnumerable<Image>> GetByStatusAsync(ProcessingStatus status, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(i => i.Status == status, cancellationToken);
    }

    /// <summary>
    /// Gets images by file format.
    /// </summary>
    public Task<IEnumerable<Image>> GetByFormatAsync(ImageFormat format, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(i => i.Format == format, cancellationToken);
    }

    /// <summary>
    /// Gets images within a size range.
    /// </summary>
    public Task<IEnumerable<Image>> GetBySizeRangeAsync(int minWidth, int maxWidth, int minHeight, int maxHeight, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(i =>
            i.Width >= minWidth && i.Width <= maxWidth &&
            i.Height >= minHeight && i.Height <= maxHeight,
            cancellationToken);
    }

    /// <summary>
    /// Gets images created within a date range.
    /// </summary>
    public Task<IEnumerable<Image>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return GetByCriteriaAsync(i =>
            i.CreatedAt >= startDate && i.CreatedAt <= endDate,
            cancellationToken);
    }

    /// <summary>
    /// Gets failed images with their error messages.
    /// </summary>
    public Task<IEnumerable<Image>> GetFailedImagesAsync(CancellationToken cancellationToken = default)
    {
        return GetByStatusAsync(ProcessingStatus.Failed, cancellationToken);
    }

    /// <summary>
    /// Gets images that need reprocessing.
    /// </summary>
    public Task<IEnumerable<Image>> GetPendingImagesAsync(CancellationToken cancellationToken = default)
    {
        return GetByStatusAsync(ProcessingStatus.Pending, cancellationToken);
    }
}
