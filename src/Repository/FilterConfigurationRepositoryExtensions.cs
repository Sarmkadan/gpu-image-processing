#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Extension methods for <see cref="FilterConfigurationRepository"/> providing additional query capabilities
/// and convenience methods for common filter operations.
/// </summary>
public static class FilterConfigurationRepositoryExtensions
{
    /// <summary>
    /// Gets the first active filter configuration with the specified type.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="type">The filter type to find.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active filter configuration or null if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    public static async Task<FilterConfiguration?> GetActiveByTypeAsync(this FilterConfigurationRepository repository, FilterType type, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var filters = await repository.GetByTypeAsync(type, cancellationToken);
        return filters.FirstOrDefault(f => f.IsActive);
    }

    /// <summary>
    /// Gets filter configurations by name pattern (case-insensitive contains).
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="namePattern">The name pattern to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching filter configurations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="namePattern"/> is <see langword="null"/>.</exception>
    public static async Task<IEnumerable<FilterConfiguration>> GetByNamePatternAsync(this FilterConfigurationRepository repository, string namePattern, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(namePattern);

        if (string.IsNullOrWhiteSpace(namePattern))
            return Enumerable.Empty<FilterConfiguration>();

        return await repository.GetByCriteriaAsync(f => f.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase), cancellationToken);
    }

    /// <summary>
    /// Gets filter configurations filtered by multiple types.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="types">The filter types to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filter configurations matching any of the specified types.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="types"/> is <see langword="null"/>.</exception>
    public static async Task<IEnumerable<FilterConfiguration>> GetByTypesAsync(this FilterConfigurationRepository repository, IEnumerable<FilterType> types, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(types);

        var typeList = types.ToList();
        return typeList.Count > 0
            ? await repository.GetByCriteriaAsync(f => typeList.Contains(f.FilterType), cancellationToken)
            : Enumerable.Empty<FilterConfiguration>();
    }

    /// <summary>
    /// Gets active filter configurations with pagination support.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged collection of active filter configurations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> or <paramref name="pageSize"/> is not positive.</exception>
    public static async Task<IEnumerable<FilterConfiguration>> GetActivePagedAsync(this FilterConfigurationRepository repository, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number and size must be greater than 0");

        var activeFilters = await repository.GetActiveFiltersAsync(cancellationToken);
        return activeFilters
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}
