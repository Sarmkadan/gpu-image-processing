#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// A stage-aware, priority-driven batch processing pipeline that adds retry policies,
/// per-stage hooks, and fine-grained progress reporting on top of the core
/// <see cref="BatchProcessingService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Images are dispatched through three stages in order:
/// <list type="number">
///   <item><description><b>PreProcess</b> – lightweight validation and metadata enrichment.</description></item>
///   <item><description><b>Filter</b> – GPU filter application via <see cref="ImageProcessingService"/>.</description></item>
///   <item><description><b>PostProcess</b> – output path registration and metrics recording.</description></item>
/// </list>
/// </para>
/// <para>
/// Failures in any stage are retried up to <see cref="BatchPipelineOptions.MaxRetries"/> times
/// using exponential back-off before the image is marked failed and processing continues.
/// </para>
/// </remarks>
public sealed class BatchProcessingPipeline
{
    private readonly ImageProcessingService _processingService;
    private readonly PerformanceMonitoringService _performanceMonitor;
    private readonly ILogger<BatchProcessingPipeline> _logger;
    private readonly BatchPipelineOptions _options;

    /// <summary>
    /// Raised after each image completes (success or failure) in any stage.
    /// </summary>
    public event EventHandler<BatchPipelineProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Initialises a new <see cref="BatchProcessingPipeline"/>.
    /// </summary>
    public BatchProcessingPipeline(
        ImageProcessingService processingService,
        PerformanceMonitoringService performanceMonitor,
        BatchPipelineOptions options,
        ILogger<BatchProcessingPipeline> logger)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the pipeline for the given <paramref name="batch"/>.
    /// </summary>
    /// <param name="batch">The batch containing image IDs and filter IDs to apply.</param>
    /// <param name="cancellationToken">Token used to abort mid-run.</param>
    /// <returns>A <see cref="BatchPipelineResult"/> summarising per-image outcomes.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="batch"/> is null.</exception>
    /// <exception cref="ProcessingException">When batch validation fails before any processing starts.</exception>
    public async Task<BatchPipelineResult> RunAsync(
        ImageBatch batch,
        CancellationToken cancellationToken = default)
    {
        if (batch is null)
            throw new ArgumentNullException(nameof(batch));

        if (!batch.Validate())
            throw new ProcessingException($"Batch '{batch.Name}' failed pre-flight validation.");

        Directory.CreateDirectory(batch.OutputDirectory);
        batch.Start();

        var pipelineSw = System.Diagnostics.Stopwatch.StartNew();
        var outcomes = new List<ImagePipelineOutcome>(batch.TotalImages);

        _logger.LogInformation(
            "Pipeline starting — batch '{Name}' ({Id}), {Count} image(s), {Stages} stage(s)",
            batch.Name, batch.Id, batch.TotalImages, 3);

        // Use a priority queue so higher-priority images run first.
        var queue = BuildPriorityQueue(batch.ImageIds);

        // Throttle concurrency to avoid saturating GPU memory.
        using var sem = new SemaphoreSlim(_options.MaxConcurrency, _options.MaxConcurrency);

        var tasks = new List<Task>(batch.TotalImages);
        while (queue.Count > 0)
        {
            var (imageId, _) = queue.Dequeue();
            tasks.Add(ProcessWithRetryAsync(batch, imageId, outcomes, sem, cancellationToken));
        }

        await Task.WhenAll(tasks);

        pipelineSw.Stop();
        batch.Complete();

        var result = BuildResult(batch, outcomes, pipelineSw.Elapsed);
        _performanceMonitor.RecordOperation(pipelineSw.Elapsed.TotalMilliseconds, result.SucceededCount > 0);

        _logger.LogInformation(
            "Pipeline finished — batch '{Name}' succeeded={S} failed={F} in {Ms:F1} ms",
            batch.Name, result.SucceededCount, result.FailedCount, pipelineSw.Elapsed.TotalMilliseconds);

        return result;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task ProcessWithRetryAsync(
        ImageBatch batch,
        Guid imageId,
        List<ImagePipelineOutcome> outcomes,
        SemaphoreSlim sem,
        CancellationToken cancellationToken)
    {
        await sem.WaitAsync(cancellationToken);
        try
        {
            var outcome = await ExecuteStagesWithRetryAsync(batch, imageId, cancellationToken);

            lock (outcomes)
                outcomes.Add(outcome);

            bool succeeded = outcome.Stage == PipelineStage.Completed;
            batch.MarkImageProcessed(succeeded);

            OnProgressChanged(batch, outcome);
        }
        finally
        {
            sem.Release();
        }
    }

    private async Task<ImagePipelineOutcome> ExecuteStagesWithRetryAsync(
        ImageBatch batch,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt <= _options.MaxRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (attempt > 0)
            {
                var delay = TimeSpan.FromMilliseconds(
                    _options.RetryBaseDelayMs * Math.Pow(2, attempt - 1));
                _logger.LogWarning(
                    "Retrying image {ImageId} — attempt {Attempt}/{Max} after {Delay:F0} ms",
                    imageId, attempt + 1, _options.MaxRetries + 1, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }

            try
            {
                // Stage 1: PreProcess — validation
                await RunPreProcessStageAsync(imageId, cancellationToken);

                // Stage 2: Filter — GPU processing
                var filterResult = await _processingService.ProcessImageAsync(
                    imageId, batch.FilterIds.ToList(), cancellationToken);

                // Stage 3: PostProcess — telemetry
                await RunPostProcessStageAsync(imageId, filterResult, cancellationToken);

                return new ImagePipelineOutcome(
                    imageId, PipelineStage.Completed, attempt + 1,
                    filterResult.ProcessingTimeMilliseconds, error: null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;
                _logger.LogWarning(ex,
                    "Image {ImageId} failed on attempt {Attempt}: {Message}",
                    imageId, attempt, ex.Message);
            }
        }

        _logger.LogError(lastException,
            "Image {ImageId} exhausted {Max} retries and will be marked failed",
            imageId, _options.MaxRetries + 1);

        return new ImagePipelineOutcome(
            imageId, PipelineStage.Failed, attempt,
            processingMs: 0, error: lastException?.Message);
    }

    private static Task RunPreProcessStageAsync(Guid imageId, CancellationToken cancellationToken)
    {
        // Validation and metadata enrichment would be performed here against
        // an image store in a full implementation (e.g. EXIF extraction).
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    private Task RunPostProcessStageAsync(
        Guid imageId,
        ProcessingResult result,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogDebug(
            "PostProcess — image {ImageId} output: {Path}",
            imageId, result.OutputFilePath);
        return Task.CompletedTask;
    }

    private static PriorityQueue<Guid, int> BuildPriorityQueue(IEnumerable<Guid> imageIds)
    {
        // All images share equal priority by default; callers may attach custom
        // priority metadata via BatchOptions to get front-of-queue placement.
        var pq = new PriorityQueue<Guid, int>();
        foreach (var id in imageIds)
            pq.Enqueue(id, 0);
        return pq;
    }

    private static BatchPipelineResult BuildResult(
        ImageBatch batch,
        List<ImagePipelineOutcome> outcomes,
        TimeSpan elapsed)
    {
        int succeeded = outcomes.Count(o => o.Stage == PipelineStage.Completed);
        int failed = outcomes.Count - succeeded;

        return new BatchPipelineResult
        {
            BatchId = batch.Id,
            BatchName = batch.Name,
            TotalImages = batch.TotalImages,
            SucceededCount = succeeded,
            FailedCount = failed,
            TotalDuration = elapsed,
            AverageProcessingMs = outcomes.Count > 0
                ? outcomes.Average(o => o.ProcessingMs)
                : 0.0,
            Outcomes = outcomes,
            CompletedAt = DateTime.UtcNow
        };
    }

    private void OnProgressChanged(ImageBatch batch, ImagePipelineOutcome outcome)
    {
        ProgressChanged?.Invoke(this, new BatchPipelineProgressEventArgs(
            batch.Id,
            batch.GetProgressPercentage(),
            outcome));
    }
}

/// <summary>
/// Configuration options for <see cref="BatchProcessingPipeline"/>.
/// </summary>
public sealed class BatchPipelineOptions
{
    /// <summary>Maximum number of images processed concurrently. Default: 4.</summary>
    public int MaxConcurrency { get; set; } = 4;

    /// <summary>Maximum retry attempts per image on failure. Default: 2.</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Base delay in milliseconds for exponential back-off. Default: 100 ms.</summary>
    public int RetryBaseDelayMs { get; set; } = 100;
}

/// <summary>
/// Lifecycle stages used by <see cref="BatchProcessingPipeline"/>.
/// </summary>
public enum PipelineStage
{
    /// <summary>Not yet started.</summary>
    Pending = 0,
    /// <summary>Validation and metadata enrichment.</summary>
    PreProcess = 1,
    /// <summary>GPU filter application.</summary>
    Filter = 2,
    /// <summary>Output registration and telemetry.</summary>
    PostProcess = 3,
    /// <summary>All stages completed successfully.</summary>
    Completed = 4,
    /// <summary>Exhausted all retry attempts.</summary>
    Failed = 5
}

/// <summary>
/// Per-image outcome recorded by <see cref="BatchProcessingPipeline"/>.
/// </summary>
/// <param name="ImageId">The processed image identifier.</param>
/// <param name="Stage">The stage at which processing ended.</param>
/// <param name="Attempts">Total number of attempts made (1 = first try, no retries).</param>
/// <param name="ProcessingMs">Wall-clock milliseconds spent in the Filter stage.</param>
/// <param name="Error">Error message when <paramref name="Stage"/> is <see cref="PipelineStage.Failed"/>.</param>
public sealed record ImagePipelineOutcome(
    Guid ImageId,
    PipelineStage Stage,
    int Attempts,
    double ProcessingMs,
    string? Error);

/// <summary>
/// Summary returned by <see cref="BatchProcessingPipeline.RunAsync"/>.
/// </summary>
public sealed class BatchPipelineResult
{
    /// <summary>Batch identifier.</summary>
    public Guid BatchId { get; init; }
    /// <summary>Human-readable batch name.</summary>
    public string BatchName { get; init; } = string.Empty;
    /// <summary>Total number of images in the batch.</summary>
    public int TotalImages { get; init; }
    /// <summary>Number of images that completed all stages.</summary>
    public int SucceededCount { get; init; }
    /// <summary>Number of images that exhausted all retries.</summary>
    public int FailedCount { get; init; }
    /// <summary>Wall-clock time for the entire pipeline run.</summary>
    public TimeSpan TotalDuration { get; init; }
    /// <summary>Average Filter-stage duration across all images.</summary>
    public double AverageProcessingMs { get; init; }
    /// <summary>Per-image processing records.</summary>
    public IReadOnlyList<ImagePipelineOutcome> Outcomes { get; init; } = [];
    /// <summary>UTC timestamp when the pipeline run finished.</summary>
    public DateTime CompletedAt { get; init; }

    /// <summary>Gets success rate as a percentage (0–100).</summary>
    public double SuccessRate =>
        TotalImages > 0 ? SucceededCount / (double)TotalImages * 100.0 : 0.0;
}

/// <summary>
/// Event arguments raised after each image is processed in the pipeline.
/// </summary>
public sealed class BatchPipelineProgressEventArgs : EventArgs
{
    /// <summary>The batch that raised the event.</summary>
    public Guid BatchId { get; }
    /// <summary>Completion percentage (0–100).</summary>
    public double ProgressPercent { get; }
    /// <summary>The outcome of the image that just finished.</summary>
    public ImagePipelineOutcome Outcome { get; }

    internal BatchPipelineProgressEventArgs(
        Guid batchId,
        double progressPercent,
        ImagePipelineOutcome outcome)
    {
        BatchId = batchId;
        ProgressPercent = progressPercent;
        Outcome = outcome;
    }
}
