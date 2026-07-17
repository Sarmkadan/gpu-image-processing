#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Linq;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Repository;

/// <summary>
/// Extension methods for ProcessingResultRepository providing specialized queries
/// and operations beyond the basic CRUD functionality.
/// </summary>
public static class ProcessingResultRepositoryExtensions
{
    /// <summary>
    /// Gets processing results by multiple image IDs in a single query.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="imageIds">Collection of image IDs to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of processing results matching the specified image IDs</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="imageIds"/> is null</exception>
    public static async Task<IReadOnlyList<ProcessingResult>> GetByImageIdsAsync(
        this ProcessingResultRepository repository,
        IEnumerable<Guid> imageIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(imageIds);

        if (!imageIds.Any())
            return Array.Empty<ProcessingResult>();

        var tasks = imageIds.Select(imageId => repository.GetByImageIdAsync(imageId, cancellationToken));
        var batchResults = await Task.WhenAll(tasks);

        var results = batchResults.SelectMany(batch => batch).ToList();
        return results.AsReadOnly();
    }

    /// <summary>
    /// Gets processing results filtered by multiple statuses.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="statuses">Collection of statuses to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of processing results matching any of the specified statuses</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> or <paramref name="statuses"/> is null</exception>
    public static async Task<IReadOnlyList<ProcessingResult>> GetByStatusesAsync(
        this ProcessingResultRepository repository,
        IEnumerable<ProcessingStatus> statuses,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(statuses);

        if (!statuses.Any())
            return Array.Empty<ProcessingResult>();

        var tasks = statuses.Select(status => repository.GetByStatusAsync(status, cancellationToken));
        var batchResults = await Task.WhenAll(tasks);

        var results = batchResults.SelectMany(batch => batch).ToList();
        return results.AsReadOnly();
    }

    /// <summary>
    /// Gets processing results completed within the last N days.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="days">Number of days to look back. Must be greater than 0.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of processing results completed within the specified time window</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="days"/> is less than 1</exception>
    public static async Task<IReadOnlyList<ProcessingResult>> GetRecentlyCompletedAsync(
        this ProcessingResultRepository repository,
        int days,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        if (days < 1)
            throw new ArgumentOutOfRangeException(nameof(days), days, "Days must be greater than 0");

        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-days);

        var results = await repository.GetCompletedBetweenAsync(startDate, endDate, cancellationToken);
        return results.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets processing results with the longest processing times.
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <param name="count">Maximum number of results to return. Default is 10.</param>
    /// <param name="minimumProcessingTimeMs">Minimum processing time threshold in milliseconds. Only results with processing time greater than or equal to this value will be returned.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of processing results sorted by processing time (descending)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1 or <paramref name="minimumProcessingTimeMs"/> is negative</exception>
    public static async Task<IReadOnlyList<ProcessingResult>> GetLongestRunningResultsAsync(
        this ProcessingResultRepository repository,
        int count = 10,
        long minimumProcessingTimeMs = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than 0");
        if (minimumProcessingTimeMs < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumProcessingTimeMs), minimumProcessingTimeMs, "Minimum processing time cannot be negative");

        var allResults = await repository.GetAllAsync(cancellationToken);
        var filteredResults = allResults
            .Where(r => r.IsSuccessful && r.ProcessingTimeMilliseconds >= minimumProcessingTimeMs)
            .OrderByDescending(r => r.ProcessingTimeMilliseconds)
            .Take(count)
            .ToList();

        return filteredResults.AsReadOnly();
    }
}