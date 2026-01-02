#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Extension methods for <see cref="ComputeShaderPipeline"/>.
/// </summary>
public static class ComputeShaderPipelineExtensions
{
    /// <summary>
    /// Executes a compute shader pipeline asynchronously and throws if the result indicates failure.
    /// </summary>
    /// <param name="pipeline">The compute shader pipeline to execute.</param>
    /// <param name="passes">The passes to execute.</param>
    /// <param name="deviceId">The ID of the device to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown if the pipeline execution result indicates failure.</exception>
    public static async Task ExecuteAndAssertSuccessAsync(
        this ComputeShaderPipeline pipeline,
        IReadOnlyList<ComputeShaderPass> passes,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(passes);

        var result = await pipeline.ExecuteAsync(passes, deviceId, cancellationToken);

        if (!result.Succeeded)
            throw new InvalidOperationException($"Pipeline execution failed: {result.ErrorMessage}");
    }

    /// <summary>
    /// Optimizes workgroups for a compute shader pass asynchronously.
    /// </summary>
    /// <param name="pipeline">The compute shader pipeline.</param>
    /// <param name="pass">The pass to optimize.</param>
    /// <param name="deviceId">The ID of the device to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The optimized workgroup configuration.</returns>
    public static async Task<WorkgroupConfiguration> OptimizeWorkgroupAndAssertSuccessAsync(
        this ComputeShaderPipeline pipeline,
        ComputeShaderPass pass,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(pass);

        return await pipeline.OptimizeWorkgroupAsync(pass, deviceId, cancellationToken);
    }
}
