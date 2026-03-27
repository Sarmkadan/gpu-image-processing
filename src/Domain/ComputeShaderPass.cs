// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Classifies the computational role of a shader pass inside the pipeline,
/// allowing the engine to apply pass-specific scheduling and memory policies.
/// </summary>
public enum ShaderPassType
{
    /// <summary>Reads one or more input images and writes filtered output.</summary>
    ImageFilter,

    /// <summary>
    /// Reduces the input image to a statistical summary such as a colour
    /// histogram or luminance average.
    /// </summary>
    Reduction,

    /// <summary>
    /// Reads from an earlier <see cref="Reduction"/> result and applies
    /// per-pixel corrections derived from the summary.
    /// </summary>
    PostProcess,

    /// <summary>
    /// Converts between colour-space representations, for example RGB to YCbCr
    /// or BGRA to greyscale.
    /// </summary>
    ColorTransform,

    /// <summary>Writes the final composited result to the target output image.</summary>
    Output
}

/// <summary>
/// Describes a single dispatchable compute shader pass within a
/// <see cref="GpuImageProcessing.Pipeline.ComputeShaderPipeline"/>.
/// </summary>
/// <remarks>
/// Before a pass can be dispatched, <see cref="WorkgroupConfiguration"/> must be
/// assigned — either explicitly or automatically by the pipeline engine.
/// Use <see cref="IsReady"/> to verify readiness before dispatch.
/// </remarks>
public sealed class ComputeShaderPass
{
    /// <summary>Gets the unique identifier for this pass instance.</summary>
    public Guid Id { get; }

    /// <summary>Gets the OpenCL kernel entrypoint name.</summary>
    public string KernelName { get; }

    /// <summary>
    /// Gets the OpenCL C source code of the kernel.
    /// May be an empty string when the kernel is pre-compiled and cached on the device.
    /// </summary>
    public string KernelSource { get; }

    /// <summary>Gets the computational role that classifies this pass.</summary>
    public ShaderPassType PassType { get; }

    /// <summary>Gets the execution priority; lower values are dispatched first.</summary>
    public int Priority { get; }

    /// <summary>
    /// Gets or sets the computed workgroup layout for this pass.
    /// <see langword="null"/> until the pipeline engine optimises the pass before dispatch.
    /// </summary>
    public WorkgroupConfiguration? WorkgroupConfiguration { get; set; }

    /// <summary>Gets the mutable dictionary of named parameters passed to the kernel.</summary>
    public Dictionary<string, object> Parameters { get; }

    /// <summary>Gets the list of input images supplied to this kernel invocation.</summary>
    public List<Image> InputImages { get; }

    /// <summary>Gets or sets the image this kernel writes its output into.</summary>
    public Image? OutputImage { get; set; }

    /// <summary>Gets the UTC time at which this pass was created.</summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initialises a new <see cref="ComputeShaderPass"/>.
    /// </summary>
    /// <param name="kernelName">Entrypoint name of the OpenCL kernel.</param>
    /// <param name="kernelSource">Full kernel source code; may be empty for pre-compiled kernels.</param>
    /// <param name="passType">Computational role of this pass.</param>
    /// <param name="priority">Execution priority; lower value means higher priority.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="kernelName"/> is null or whitespace.
    /// </exception>
    public ComputeShaderPass(
        string kernelName,
        string kernelSource = "",
        ShaderPassType passType = ShaderPassType.ImageFilter,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(kernelName))
            throw new ArgumentException("Kernel name must not be empty.", nameof(kernelName));

        Id = Guid.NewGuid();
        KernelName = kernelName;
        KernelSource = kernelSource;
        PassType = passType;
        Priority = priority;
        Parameters = [];
        InputImages = [];
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns <see langword="true"/> when a workgroup configuration has been assigned,
    /// at least one input image is present, and an output image has been set — the
    /// minimum requirements for dispatch.
    /// </summary>
    public bool IsReady() =>
        WorkgroupConfiguration is not null &&
        InputImages.Count > 0 &&
        OutputImage is not null;
}

/// <summary>
/// Captures timing and outcome data for a single <see cref="ComputeShaderPass"/>
/// that has been executed by the pipeline engine.
/// </summary>
/// <param name="PassId">Identifier of the executed pass.</param>
/// <param name="KernelName">Entrypoint name of the executed kernel.</param>
/// <param name="Succeeded">Whether the pass completed without error.</param>
/// <param name="Duration">Wall-clock time taken to execute the pass.</param>
/// <param name="Occupancy">Measured or estimated device occupancy during execution.</param>
/// <param name="ErrorMessage">Error detail when <paramref name="Succeeded"/> is <see langword="false"/>.</param>
public sealed record PassExecutionRecord(
    Guid PassId,
    string KernelName,
    bool Succeeded,
    TimeSpan Duration,
    double Occupancy,
    string? ErrorMessage = null);

/// <summary>
/// Aggregates the outcome of a complete pipeline run across all dispatched passes.
/// </summary>
public sealed class PipelineExecutionResult
{
    /// <summary>Gets the unique identifier for this pipeline run.</summary>
    public Guid ExecutionId { get; init; } = Guid.NewGuid();

    /// <summary>Gets whether every pass in the pipeline completed without error.</summary>
    public bool Succeeded { get; init; }

    /// <summary>Gets the top-level error message when <see cref="Succeeded"/> is <see langword="false"/>.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets the total wall-clock duration of the complete pipeline run.</summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>Gets the number of passes that completed successfully.</summary>
    public int PassesExecuted { get; init; }

    /// <summary>Gets the number of passes that encountered an error.</summary>
    public int PassesFailed { get; init; }

    /// <summary>Gets per-pass execution records in dispatch order.</summary>
    public IReadOnlyList<PassExecutionRecord> PassRecords { get; init; } = [];

    /// <summary>Gets the arithmetic mean occupancy across all executed passes.</summary>
    public double AverageOccupancy { get; init; }

    /// <summary>Gets the cumulative pixel count processed across all input images.</summary>
    public long TotalPixelsProcessed { get; init; }

    /// <summary>Gets the identifier of the GPU device that executed this run.</summary>
    public Guid DeviceId { get; init; }

    /// <summary>Gets the UTC timestamp when the pipeline run completed.</summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Returns the throughput in pixels per second, or zero when
    /// <see cref="TotalDuration"/> is zero.
    /// </summary>
    public double GetPixelThroughput() =>
        TotalDuration.TotalSeconds > 0
            ? TotalPixelsProcessed / TotalDuration.TotalSeconds
            : 0.0;
}

/// <summary>
/// Provides aggregated throughput and reliability statistics for all pipeline runs
/// since the <see cref="GpuImageProcessing.Pipeline.ComputeShaderPipeline"/> was
/// started or last reset.
/// </summary>
public sealed record PipelineStatistics
{
    /// <summary>Gets the total number of pipeline runs recorded.</summary>
    public required long TotalExecutions { get; init; }

    /// <summary>Gets the cumulative count of passes dispatched across all runs.</summary>
    public required long TotalPassesExecuted { get; init; }

    /// <summary>Gets the cumulative count of passes that failed across all runs.</summary>
    public required long TotalPassesFailed { get; init; }

    /// <summary>Gets the cumulative number of pixels processed.</summary>
    public required long TotalPixelsProcessed { get; init; }

    /// <summary>Gets the sum of all pipeline run durations.</summary>
    public required TimeSpan TotalProcessingTime { get; init; }

    /// <summary>Gets the mean single-pass execution time in milliseconds.</summary>
    public required double AveragePassDurationMs { get; init; }

    /// <summary>Gets the mean device occupancy across all dispatched passes.</summary>
    public required double AverageOccupancy { get; init; }

    /// <summary>
    /// Gets the fraction of passes that completed without error, in [0, 1].
    /// Returns <c>1.0</c> when no passes have been executed yet.
    /// </summary>
    public required double SuccessRate { get; init; }

    /// <summary>Gets the UTC timestamp at which these statistics were collected.</summary>
    public DateTime CollectedAt { get; init; } = DateTime.UtcNow;
}
