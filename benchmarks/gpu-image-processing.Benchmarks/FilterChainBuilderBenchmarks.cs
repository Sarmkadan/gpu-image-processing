#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="FilterChainBuilder"/>: fluent construction, validation,
/// and clone operations across chain sizes representative of real-world workflows.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
public class FilterChainBuilderBenchmarks
{
    private FilterChain _builtChain = null!;

    [GlobalSetup]
    public void Setup()
    {
        _builtChain = FilterChainBuilder
            .Create("Portrait Processing")
            .WithDescription("Standard 10-step portrait workflow")
            .AddGrayscale()
            .AddBlur(radius: 2.0f)
            .AddSharpen(strength: 0.8f)
            .AddEdgeDetection()
            .AddColorCorrection(brightness: 0.1f)
            .AddThreshold(thresholdValue: 0.45f)
            .AddBilateral()
            .AddMedian()
            .AddEmboss()
            .AddSobel()
            .Build();
    }

    /// <summary>
    /// Measures the full builder execution: 3-step chain create + Build.
    /// Representative of the "quick preset" code path.
    /// </summary>
    [Benchmark]
    public FilterChain Build_ThreeStep()
    {
        return FilterChainBuilder
            .Create("Quick Preset")
            .AddGrayscale()
            .AddBlur()
            .AddSharpen()
            .Build();
    }

    /// <summary>
    /// Measures a 10-step builder, representative of professional filter chains.
    /// </summary>
    [Benchmark]
    public FilterChain Build_TenStep()
    {
        return FilterChainBuilder
            .Create("Full Workflow")
            .AddGrayscale()
            .AddBlur(radius: 2.0f)
            .AddSharpen(strength: 0.8f)
            .AddEdgeDetection()
            .AddColorCorrection()
            .AddThreshold()
            .AddBilateral()
            .AddMedian()
            .AddEmboss()
            .AddSobel()
            .Build();
    }

    /// <summary>
    /// Measures <see cref="FilterChain.Validate"/> on the pre-built 10-step chain.
    /// Called before every batch job dispatch.
    /// </summary>
    [Benchmark]
    public bool Validate_TenStep() => _builtChain.Validate();

    /// <summary>
    /// Measures cloning the 10-step chain — used when duplicating profiles.
    /// </summary>
    [Benchmark]
    public FilterChain Clone_TenStep() => _builtChain.Clone();

    /// <summary>
    /// Measures <see cref="FilterChain.EstimateTotalProcessingTime"/> which is
    /// called on every scheduler tick to predict queue drain time.
    /// </summary>
    [Benchmark]
    public double EstimateTotalProcessingTime() => _builtChain.EstimateTotalProcessingTime();

    /// <summary>
    /// Measures GetEnabledSteps for a fully-enabled 10-step chain.
    /// </summary>
    [Benchmark]
    public System.Collections.Generic.List<FilterStep> GetEnabledSteps()
        => _builtChain.GetEnabledSteps();
}
