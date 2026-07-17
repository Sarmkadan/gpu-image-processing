#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.Services;

/// <summary>
/// Extension methods that add convenient query capabilities to <see cref="BatchProcessingService"/>.
/// </summary>
public static class BatchProcessingServiceExtensions
{
    /// <summary>
    /// Returns the identifiers of all batches that are currently active.
    /// </summary>
    /// <param name="service">The <see cref="BatchProcessingService"/> instance.</param>
    /// <returns>A read-only list of batch <see cref="Guid"/> values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    public static IReadOnlyList<Guid> GetActiveBatchIds(this BatchProcessingService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.GetActiveBatches()
            .Select(b => b.Id)
            .ToList();
    }

    /// <summary>
    /// Retrieves the progress dictionaries for every active batch.
    /// </summary>
    /// <param name="service">The <see cref="BatchProcessingService"/> instance.</param>
    /// <returns>
    /// A read-only dictionary where the key is the batch <see cref="Guid"/> and the value is the
    /// progress information returned by <see cref="BatchProcessingService.GetBatchProgress"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    public static IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object>> GetAllActiveBatchProgress(this BatchProcessingService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.GetActiveBatches()
            .ToDictionary(
                b => b.Id,
                b => (IReadOnlyDictionary<string, object>)service.GetBatchProgress(b.Id));
    }

    /// <summary>
    /// Produces a concise, human-readable summary of a specific batch.
    /// </summary>
    /// <param name="service">The <see cref="BatchProcessingService"/> instance.</param>
    /// <param name="batchId">The identifier of the batch to summarize.</param>
    /// <returns>
    /// A string containing the batch identifier, status, processed/total image counts,
    /// failed image count, and progress percentage formatted to two decimal places.
    /// If the batch cannot be found, a message indicating this is returned.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="batchId"/> is <see cref="Guid.Empty"/>.</exception>
    public static string GetBatchSummary(this BatchProcessingService service, Guid batchId)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch identifier cannot be empty.", nameof(batchId));

        var batch = service.GetBatchStatus(batchId);
        if (batch is null)
            return $"Batch {batchId} not found.";

        // Use invariant culture for deterministic formatting.
        var progress = batch.GetProgressPercentage().ToString("F2", CultureInfo.InvariantCulture);
        return $"Batch {batch.Id}: Status={batch.Status}, Processed={batch.ProcessedImages}/{batch.TotalImages}, Failed={batch.FailedImages}, Progress={progress}%";
    }
}
