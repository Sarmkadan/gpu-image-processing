#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Runtime configuration for the compute shader pipeline and the automatic
/// workgroup optimiser.
/// </summary>
/// <remarks>
/// Bind this class from the <c>appsettings.json</c> section keyed
/// <c>"ComputeShaderPipeline"</c> via
/// <see cref="ComputeShaderPipelineExtensions.AddComputeShaderPipeline(IServiceCollection, IConfiguration)"/>,
/// or configure it inline using the overload that accepts an
/// <see cref="Action{ComputeShaderPipelineOptions}"/> delegate.
/// </remarks>
public sealed class ComputeShaderPipelineOptions
{
    /// <summary>The <c>appsettings.json</c> section key used for automatic binding.</summary>
    public const string SectionName = "ComputeShaderPipeline";

    /// <summary>
    /// Gets or sets the optimisation strategy applied when no per-pass strategy is
    /// specified. Defaults to <see cref="WorkgroupOptimizationStrategy.Balanced"/>.
    /// </summary>
    public WorkgroupOptimizationStrategy DefaultStrategy { get; set; } =
        WorkgroupOptimizationStrategy.Balanced;

    /// <summary>
    /// Gets or sets the maximum tile dimension (X or Y) considered by the optimiser.
    /// Must be a positive integer; typical values are powers of two up to 32.
    /// Defaults to <c>32</c>.
    /// </summary>
    public int MaxWorkgroupDimension { get; set; } = 32;

    /// <summary>
    /// Gets or sets whether benchmark-guided workgroup selection is enabled.
    /// When <see langword="true"/> the engine calls
    /// <see cref="IWorkgroupOptimizer.BenchmarkAsync"/> instead of
    /// <see cref="IWorkgroupOptimizer.Compute"/>, providing higher accuracy at
    /// the cost of additional async overhead on the first dispatch.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool BenchmarkGuidedOptimization { get; set; } = false;

    /// <summary>
    /// Gets or sets whether per-pass timing and occupancy data are emitted to
    /// the diagnostic logger. When enabled, low-occupancy passes also trigger
    /// a warning. Defaults to <see langword="false"/>.
    /// </summary>
    public bool EnableProfiling { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of passes accepted in a single
    /// <see cref="IComputeShaderPipeline.ExecuteAsync"/> call.
    /// Defaults to <c>64</c>.
    /// </summary>
    public int MaxPipelineDepth { get; set; } = 64;

    /// <summary>
    /// Gets or sets the bytes of local (shared) GPU memory reserved per thread
    /// when the kernel does not declare explicit local-memory buffers.
    /// One <c>float4</c> (16 bytes) is a typical default.
    /// Defaults to <c>16</c>.
    /// </summary>
    public int DefaultLocalMemoryPerThreadBytes { get; set; } = 16;

    /// <summary>
    /// Gets or sets the occupancy ratio below which the profiling logger emits
    /// a warning for a pass. Expressed as a fraction in [0, 1].
    /// Defaults to <c>0.3</c> (30 %).
    /// </summary>
    public double OccupancyWarningThreshold { get; set; } = 0.3;

    /// <summary>
    /// Validates this options instance and returns <see langword="true"/> when all
    /// constraint invariants are satisfied.
    /// </summary>
    /// <returns><see langword="true"/> if all constraints are satisfied; otherwise, <see langword="false"/>.</returns>
    public bool Validate() =>
        MaxWorkgroupDimension is >= 1 and <= 1024 &&
        MaxPipelineDepth >= 1 &&
        DefaultLocalMemoryPerThreadBytes >= 0 &&
        OccupancyWarningThreshold is >= 0.0 and <= 1.0;
}

/// <summary>
/// Extension methods that register the compute shader pipeline and workgroup optimiser
/// into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ComputeShaderPipelineExtensions
{
    /// <summary>
    /// Adds the compute shader pipeline and supporting services to the DI container,
    /// binding options from the <c>"ComputeShaderPipeline"</c> configuration section.
    /// </summary>
    /// <remarks>
    /// Registers: 1) <see cref="ComputeShaderPipelineOptions"/> — bound from <paramref name="configuration"/>
    /// 2) <see cref="IWorkgroupOptimizer"/> → <see cref="WorkgroupOptimizer"/> (Singleton)
    /// 3) <see cref="IComputeShaderPipeline"/> → <see cref="ComputeShaderPipeline"/> (Singleton)
    /// Requires <see cref="GpuManagementService"/> and
    /// <see cref="PerformanceMonitoringService"/> to be already registered —
    /// typically via <c>AddGpuImageProcessing</c>.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration used to bind pipeline options.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the bound options fail <see cref="ComputeShaderPipelineOptions.Validate"/>.
    /// </exception>
    public static IServiceCollection AddComputeShaderPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new ComputeShaderPipelineOptions();
        configuration.GetSection(ComputeShaderPipelineOptions.SectionName).Bind(options);

        return RegisterPipeline(services, options);
    }

    /// <summary>
    /// Adds the compute shader pipeline and supporting services to the DI container,
    /// configuring options inline via an <paramref name="configure"/> delegate.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Action that mutates a <see cref="ComputeShaderPipelineOptions"/> instance.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the configured options fail <see cref="ComputeShaderPipelineOptions.Validate"/>.
    /// </exception>
    public static IServiceCollection AddComputeShaderPipeline(
        this IServiceCollection services,
        Action<ComputeShaderPipelineOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ComputeShaderPipelineOptions();
        configure(options);

        return RegisterPipeline(services, options);
    }

    /// <summary>
    /// Logs the active pipeline settings using the application logger. Call this after
    /// <see cref="IServiceCollection.BuildServiceProvider"/> for start-up diagnostics.
    /// </summary>
    /// <param name="provider">The built service provider.</param>
    /// <returns>The same <paramref name="provider"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is null.</exception>
    public static IServiceProvider LogComputeShaderPipelineSettings(this IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var options = provider.GetRequiredService<ComputeShaderPipelineOptions>();
        var logger = provider.GetRequiredService<ILogger<ComputeShaderPipeline>>();

        logger.LogInformation(
            "Compute shader pipeline active — " +
            "Strategy={Strategy} MaxDim={MaxDim} Benchmark={Bench} " +
            "Profiling={Prof} MaxDepth={Depth} LocalMem={Mem}b/thread",
            options.DefaultStrategy,
            options.MaxWorkgroupDimension,
            options.BenchmarkGuidedOptimization,
            options.EnableProfiling,
            options.MaxPipelineDepth,
            options.DefaultLocalMemoryPerThreadBytes);

        return provider;
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private static IServiceCollection RegisterPipeline(
        IServiceCollection services,
        ComputeShaderPipelineOptions options)
    {
        if (!options.Validate())
            throw new InvalidOperationException(
                $"'{ComputeShaderPipelineOptions.SectionName}' options failed validation. " +
                "Verify MaxWorkgroupDimension (1–1024), MaxPipelineDepth (≥1), " +
                "DefaultLocalMemoryPerThreadBytes (≥0), and OccupancyWarningThreshold ([0,1]).");

        services.AddSingleton(options);
        services.AddSingleton<IWorkgroupOptimizer, WorkgroupOptimizer>();

        services.AddSingleton<IComputeShaderPipeline>(provider =>
            new ComputeShaderPipeline(
                provider.GetRequiredService<IWorkgroupOptimizer>(),
                provider.GetRequiredService<GpuManagementService>(),
                provider.GetRequiredService<PerformanceMonitoringService>(),
                provider.GetRequiredService<ComputeShaderPipelineOptions>(),
                provider.GetRequiredService<ILogger<ComputeShaderPipeline>>()));

        return services;
    }
}