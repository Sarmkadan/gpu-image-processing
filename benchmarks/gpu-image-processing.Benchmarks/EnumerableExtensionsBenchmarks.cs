// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Utilities;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Benchmarks for EnumerableExtensions hot paths used throughout the batch
/// processing pipeline: shuffling, batching, and deduplication.
/// </summary>
[MemoryDiagnoser]
public class EnumerableExtensionsBenchmarks
{
    private List<int> _list32 = null!;
    private List<int> _list1024 = null!;
    private List<int> _source1000 = null!;
    private List<string> _guidStrings = null!;

    [GlobalSetup]
    public void Setup()
    {
        _list32 = Enumerable.Range(0, 32).ToList();
        _list1024 = Enumerable.Range(0, 1024).ToList();
        _source1000 = Enumerable.Range(0, 1000).ToList();
        _guidStrings = Enumerable.Range(0, 500)
            .SelectMany(_ => new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() })
            .ToList();
    }

    // --- Shuffle --------------------------------------------------------------

    /// <summary>
    /// Small-list shuffle: representative of filter-order randomisation
    /// on a per-image processing profile with ~32 candidate filters.
    /// </summary>
    [Benchmark]
    public IEnumerable<int> Shuffle_32Items() => _list32.Shuffle();

    /// <summary>
    /// Large-list shuffle: used when randomising image order in a 1 k-image batch.
    /// </summary>
    [Benchmark]
    public IEnumerable<int> Shuffle_1024Items() => _list1024.Shuffle();

    // --- Batch ----------------------------------------------------------------

    /// <summary>
    /// Batching 1 000 image IDs into GPU-sized chunks of 32 before dispatch.
    /// </summary>
    [Benchmark]
    public int Batch_1000By32()
    {
        int count = 0;
        foreach (var _ in _source1000.Batch(32))
            count++;
        return count;
    }

    /// <summary>
    /// Batching same source into smaller chunks of 8 (quality-optimised profile).
    /// </summary>
    [Benchmark]
    public int Batch_1000By8()
    {
        int count = 0;
        foreach (var _ in _source1000.Batch(8))
            count++;
        return count;
    }

    // --- DistinctBy -----------------------------------------------------------

    /// <summary>
    /// Deduplicating 1 000 strings by prefix — mirrors dedup of filter names
    /// before building the processing pipeline.
    /// </summary>
    [Benchmark]
    public int DistinctBy_1000Strings()
    {
        int count = 0;
        foreach (var _ in _source1000.DistinctBy(x => x % 100))
            count++;
        return count;
    }

    // --- SafeToDictionary -----------------------------------------------------

    [Benchmark]
    public Dictionary<int, int> SafeToDictionary_1000Items()
        => _source1000.SafeToDictionary(x => x, x => x * 2);
}
