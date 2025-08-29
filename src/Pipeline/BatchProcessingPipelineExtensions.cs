using System;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Provides extension methods for <see cref="BatchProcessingPipeline"/>.
/// </summary>
public static class BatchProcessingPipelineExtensions
{
    /// <summary>
    /// Executes the pipeline for the given <paramref name="batch"/> and throws an exception if any images failed.
    /// </summary>
    /// <param name="pipeline">The pipeline instance.</param>
    /// <param name="batch">The batch to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="BatchPipelineResult"/> summarising per-image outcomes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> or <paramref name="batch"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the batch contains failed images.</exception>
    public static async Task<BatchPipelineResult> RunAndValidateAsync(
        this BatchProcessingPipeline pipeline,
        ImageBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(batch);

        var result = await pipeline.RunAsync(batch, cancellationToken);

        if (result.FailedCount > 0)
        {
            throw new InvalidOperationException($"Batch '{result.BatchName}' completed with {result.FailedCount} failures.");
        }

        return result;
    }

    /// <summary>
    /// Subscribes to the <see cref="BatchProcessingPipeline.ProgressChanged"/> event.
    /// </summary>
    /// <param name="pipeline">The pipeline instance.</param>
    /// <param name="handler">The event handler.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> or <paramref name="handler"/> is null.</exception>
    public static void SubscribeToProgress(
        this BatchProcessingPipeline pipeline,
        EventHandler<BatchPipelineProgressEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(handler);

        pipeline.ProgressChanged += handler;
    }
}
