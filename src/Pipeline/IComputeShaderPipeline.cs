#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Defines the contract for executing and managing a compute shader pipeline
/// that dispatches OpenCL workgroups against one or more GPU devices.
/// </summary>
/// <remarks>
/// Implementations are expected to:
/// <list type="bullet">
///   <item><description>Sort passes by <see cref="ComputeShaderPass.Priority"/> before dispatch.</description></item>
///   <item><description>Invoke <see cref="IWorkgroupOptimizer"/> for any pass that does not yet carry a <see cref="WorkgroupConfiguration"/>.</description></item>
///   <item><description>Allocate and release GPU memory through the device manager within each execution.</description></item>
///   <item><description>Accumulate per-pass telemetry accessible via <see cref="GetStatisticsAsync"/>.</description></item>
/// </list>
/// </remarks>
public interface IComputeShaderPipeline
{
    /// <summary>
    /// Executes a sequence of <see cref="ComputeShaderPass"/> instances in priority
    /// order, optimising workgroup layouts automatically before the first dispatch.
    /// </summary>
    /// <param name="passes">
    /// The collection of shader passes to execute. Must contain at least one element.
    /// </param>
    /// <param name="deviceId">
    /// Target GPU device identifier. Pass <see cref="Guid.Empty"/> to let the
    /// engine select the highest-scoring available device automatically.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel execution mid-pipeline.</param>
    /// <returns>
    /// A <see cref="PipelineExecutionResult"/> capturing per-pass timings,
    /// occupancy measurements, and any errors encountered during the run.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="passes"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="passes"/> is empty.</exception>
    /// <exception cref="GpuImageProcessing.Core.GpuException">
    /// Thrown when no suitable device is found for <paramref name="deviceId"/>.
    /// </exception>
    Task<PipelineExecutionResult> ExecuteAsync(
        IReadOnlyList<ComputeShaderPass> passes,
        Guid deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes and assigns the optimal workgroup layout for a single pass before
    /// the pass is added to a pipeline run.
    /// The result is stored on <see cref="ComputeShaderPass.WorkgroupConfiguration"/>.
    /// </summary>
    /// <param name="pass">The pass to optimise.</param>
    /// <param name="deviceId">
    /// Target GPU device identifier; pass <see cref="Guid.Empty"/> for auto-selection.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The computed <see cref="WorkgroupConfiguration"/>.</returns>
    Task<WorkgroupConfiguration> OptimizeWorkgroupAsync(
        ComputeShaderPass pass,
        Guid deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns aggregated statistics accumulated since the service started or was
    /// last reset via <see cref="ResetStatisticsAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A snapshot of current pipeline statistics.</returns>
    Task<PipelineStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>Resets all accumulated pipeline statistics to zero.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Analyses a <see cref="GpuDevice"/>'s hardware characteristics and computes
/// an efficient workgroup layout for 2-D image processing kernels.
/// </summary>
/// <remarks>
/// The optimiser models three dominant hardware constraints:
/// <list type="bullet">
///   <item><description>Maximum threads per workgroup (<see cref="GpuDevice.MaxWorkGroupSize"/>)</description></item>
///   <item><description>Available local (shared) memory (<see cref="GpuDevice.LocalMemoryBytes"/>)</description></item>
///   <item><description>Wavefront / warp width — 32 threads on NVIDIA, 64 on AMD</description></item>
/// </list>
/// </remarks>
public interface IWorkgroupOptimizer
{
    /// <summary>
    /// Derives the optimal workgroup dimensions for the given image size and device,
    /// applying the requested optimisation <paramref name="strategy"/> synchronously.
    /// </summary>
    /// <param name="device">GPU device whose hardware limits constrain the layout.</param>
    /// <param name="imageWidth">Width of the image (pixels) the kernel will process.</param>
    /// <param name="imageHeight">Height of the image (pixels) the kernel will process.</param>
    /// <param name="localMemoryPerThreadBytes">Bytes of local (shared) memory required per thread.</param>
    /// <param name="strategy">Optimisation objective to apply.</param>
    /// <returns>The recommended <see cref="WorkgroupConfiguration"/>.</returns>
    WorkgroupConfiguration Compute(
        GpuDevice device,
        int imageWidth,
        int imageHeight,
        int localMemoryPerThreadBytes = 0,
        WorkgroupOptimizationStrategy strategy = WorkgroupOptimizationStrategy.Balanced);

    /// <summary>
    /// Probes a range of candidate workgroup sizes asynchronously, selecting the
    /// layout with the lowest estimated dispatch time.
    /// Falls back to <see cref="Compute"/> when the candidate space is empty.
    /// </summary>
    /// <param name="device">GPU device to benchmark against.</param>
    /// <param name="imageWidth">Width of the image (pixels).</param>
    /// <param name="imageHeight">Height of the image (pixels).</param>
    /// <param name="localMemoryPerThreadBytes">Bytes of local memory required per thread.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The benchmark-selected <see cref="WorkgroupConfiguration"/>.</returns>
    Task<WorkgroupConfiguration> BenchmarkAsync(
        GpuDevice device,
        int imageWidth,
        int imageHeight,
        int localMemoryPerThreadBytes = 0,
        CancellationToken cancellationToken = default);
}
