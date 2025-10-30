// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Benchmarks for FilterChain operations: step management, validation, and querying.
/// These represent realistic in-process hot paths during filter pipeline setup.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
public class FilterChainBenchmarks
{
    private FilterChain _chain = null!;
    private List<Guid> _filterIds = null!;

    [GlobalSetup]
    public void Setup()
    {
        _filterIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();

        _chain = new FilterChain { Name = "BenchmarkChain", IsEnabled = true };
        for (int i = 0; i < _filterIds.Count; i++)
            _chain.AddStep(_filterIds[i], i);
    }

    /// <summary>
    /// Measures the cost of building a 10-step chain from scratch, including the
    /// in-place sort performed after each insertion.
    /// </summary>
    [Benchmark]
    public FilterChain AddStep_TenFilters()
    {
        var chain = new FilterChain { Name = "Test", IsEnabled = true };
        for (int i = 0; i < 10; i++)
            chain.AddStep(Guid.NewGuid(), i);
        return chain;
    }

    /// <summary>
    /// Measures the cost of filtering and ordering enabled steps from a 10-step chain.
    /// Called on every filter application in the processing pipeline.
    /// </summary>
    [Benchmark]
    public List<FilterStep> GetEnabledSteps_TenSteps() => _chain.GetEnabledSteps();

    /// <summary>
    /// Measures full chain validation including enabled-step enumeration.
    /// Invoked before every batch job dispatch.
    /// </summary>
    [Benchmark]
    public bool Validate_TenSteps() => _chain.Validate();

    /// <summary>
    /// Measures counting only — avoids allocating the full enabled-step list.
    /// </summary>
    [Benchmark]
    public int GetEnabledFilterCount() => _chain.GetEnabledFilterCount();

    /// <summary>
    /// Measures clone cost for profile duplication workflows.
    /// </summary>
    [Benchmark]
    public FilterChain Clone_TenStepChain() => _chain.Clone();
}
