#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="ImageBatch"/> core operations: creation, validation,
/// progress tracking, and priority-queue construction.
/// These represent hot-paths exercised once per batch in the processing pipeline.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
public class BatchProcessingBenchmarks
{
    private List<Guid> _imageIds = null!;
    private List<Guid> _filterIds = null!;
    private ImageBatch _batch = null!;

    [Params(10, 100, 500)]
    public int ImageCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _imageIds = Enumerable.Range(0, ImageCount).Select(_ => Guid.NewGuid()).ToList();
        _filterIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();

        _batch = new ImageBatch
        {
            Name = "BenchmarkBatch",
            OutputDirectory = "./bench-output",
            TotalImages = ImageCount
        };

        foreach (var id in _imageIds)
            _batch.AddImage(id);

        foreach (var id in _filterIds)
            _batch.AddFilter(id);
    }

    /// <summary>
    /// Measures the cost of creating and fully populating an <see cref="ImageBatch"/>
    /// from scratch, including all AddImage and AddFilter calls.
    /// </summary>
    [Benchmark]
    public ImageBatch CreateAndPopulateBatch()
    {
        var batch = new ImageBatch
        {
            Name = "Benchmark",
            OutputDirectory = "/tmp/bench",
            TotalImages = ImageCount
        };

        foreach (var id in _imageIds)
            batch.AddImage(id);

        foreach (var id in _filterIds)
            batch.AddFilter(id);

        return batch;
    }

    /// <summary>
    /// Measures batch validation, which is called before every pipeline dispatch.
    /// </summary>
    [Benchmark]
    public bool ValidateBatch() => _batch.Validate();

    /// <summary>
    /// Measures the running <see cref="ImageBatch.GetProgressPercentage"/> calculation
    /// called after each image completes.
    /// </summary>
    [Benchmark]
    public double GetProgressPercentage() => _batch.GetProgressPercentage();

    /// <summary>
    /// Measures <see cref="ImageBatch.GetSuccessRate"/> used in dashboard polling.
    /// </summary>
    [Benchmark]
    public double GetSuccessRate() => _batch.GetSuccessRate();

    /// <summary>
    /// Measures <see cref="ImageBatch.GetEstimatedRemainingTime"/> which is invoked
    /// on every progress-bar refresh.
    /// </summary>
    [Benchmark]
    public TimeSpan? GetEstimatedRemainingTime() => _batch.GetEstimatedRemainingTime();

    /// <summary>
    /// Measures the cost of building a <see cref="PriorityQueue{TElement,TPriority}"/>
    /// from the batch image IDs — performed once at pipeline startup.
    /// </summary>
    [Benchmark]
    public PriorityQueue<Guid, int> BuildPriorityQueue()
    {
        var pq = new PriorityQueue<Guid, int>(_imageIds.Count);
        foreach (var id in _imageIds)
            pq.Enqueue(id, 0);
        return pq;
    }

    /// <summary>
    /// Measures MarkImageProcessed calls which run in the hot inner loop.
    /// </summary>
    [Benchmark]
    [OperationsPerInvoke(10)]
    public void MarkImageProcessed_TenSuccesses()
    {
        var batch = new ImageBatch { Name = "T", OutputDirectory = "/t", TotalImages = 10 };
        for (int i = 0; i < 10; i++)
            batch.MarkImageProcessed(success: true);
    }
}
