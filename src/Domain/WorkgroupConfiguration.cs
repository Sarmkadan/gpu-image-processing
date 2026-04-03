#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Defines the objective applied when the workgroup optimiser searches for the
/// most efficient dispatch layout for a compute shader pass.
/// </summary>
public enum WorkgroupOptimizationStrategy
{
    /// <summary>Balances compute-unit occupancy and memory bandwidth.</summary>
    Balanced,

    /// <summary>
    /// Maximises arithmetic throughput by favouring high occupancy,
    /// even at the cost of increased local-memory pressure.
    /// </summary>
    ThroughputMaximized,

    /// <summary>
    /// Minimises dispatch latency by choosing smaller workgroups that
    /// reach the device faster and produce shorter scheduling queues.
    /// </summary>
    LatencyMinimized,

    /// <summary>
    /// Keeps the local (shared) memory footprint per workgroup as small
    /// as possible, leaving more headroom for concurrent occupancy.
    /// </summary>
    MemoryOptimized
}

/// <summary>
/// Immutable record describing the workgroup layout computed for a single
/// <see cref="ComputeShaderPass"/> by
/// <see cref="GpuImageProcessing.Pipeline.WorkgroupOptimizer"/>.
/// </summary>
/// <remarks>
/// All dimension properties are expressed in threads. A 2-D image kernel
/// typically leaves <see cref="WorkgroupSizeZ"/> and
/// <see cref="GlobalWorkSizeZ"/> at their default value of <c>1</c>.
/// </remarks>
public sealed record WorkgroupConfiguration
{
    /// <summary>Gets the number of threads in the X (width) dimension per workgroup.</summary>
    public int WorkgroupSizeX { get; init; }

    /// <summary>Gets the number of threads in the Y (height) dimension per workgroup.</summary>
    public int WorkgroupSizeY { get; init; }

    /// <summary>Gets the number of threads in the Z (depth/layer) dimension per workgroup.</summary>
    public int WorkgroupSizeZ { get; init; } = 1;

    /// <summary>Gets the total global dispatch size in the X dimension.</summary>
    public int GlobalWorkSizeX { get; init; }

    /// <summary>Gets the total global dispatch size in the Y dimension.</summary>
    public int GlobalWorkSizeY { get; init; }

    /// <summary>Gets the total global dispatch size in the Z dimension.</summary>
    public int GlobalWorkSizeZ { get; init; } = 1;

    /// <summary>Gets the bytes of local (shared) memory required per workgroup.</summary>
    public long LocalMemoryRequiredBytes { get; init; }

    /// <summary>
    /// Gets the estimated occupancy ratio in [0, 1], representing the fraction of
    /// device wavefronts or warps that will be simultaneously active.
    /// </summary>
    public double EstimatedOccupancy { get; init; }

    /// <summary>
    /// Gets a composite quality score in [0, 100] produced by the optimiser.
    /// Higher values indicate a more efficient dispatch layout.
    /// </summary>
    public double OptimizationScore { get; init; }

    /// <summary>Gets the strategy that was applied to produce this configuration.</summary>
    public WorkgroupOptimizationStrategy Strategy { get; init; }

    /// <summary>Gets the identifier of the GPU device this configuration targets.</summary>
    public Guid DeviceId { get; init; }

    /// <summary>Gets the UTC timestamp at which this configuration was computed.</summary>
    public DateTime ComputedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Returns the total number of threads per workgroup (X × Y × Z).
    /// </summary>
    public int GetTotalWorkgroupSize() => WorkgroupSizeX * WorkgroupSizeY * WorkgroupSizeZ;

    /// <summary>
    /// Returns the total number of workgroups that will be dispatched, computing
    /// the ceiling-division of each global dimension by the corresponding local dimension.
    /// </summary>
    public long GetTotalDispatchCount()
    {
        long x = (long)Math.Ceiling((double)GlobalWorkSizeX / WorkgroupSizeX);
        long y = (long)Math.Ceiling((double)GlobalWorkSizeY / WorkgroupSizeY);
        long z = (long)Math.Ceiling((double)GlobalWorkSizeZ / WorkgroupSizeZ);
        return x * y * z;
    }

    /// <summary>
    /// Returns <see langword="true"/> when all dimensions are positive and the
    /// local-memory requirement does not exceed <paramref name="deviceLocalMemoryBytes"/>.
    /// </summary>
    /// <param name="deviceLocalMemoryBytes">Local memory capacity of the target device.</param>
    public bool IsValidForDevice(long deviceLocalMemoryBytes) =>
        WorkgroupSizeX > 0 && WorkgroupSizeY > 0 && WorkgroupSizeZ > 0 &&
        GlobalWorkSizeX > 0 && GlobalWorkSizeY > 0 && GlobalWorkSizeZ > 0 &&
        LocalMemoryRequiredBytes <= deviceLocalMemoryBytes;

    /// <inheritdoc />
    public override string ToString() =>
        $"Workgroup [{WorkgroupSizeX}×{WorkgroupSizeY}×{WorkgroupSizeZ}] " +
        $"Global [{GlobalWorkSizeX}×{GlobalWorkSizeY}×{GlobalWorkSizeZ}] " +
        $"Score={OptimizationScore:F1} Occupancy={EstimatedOccupancy:P0} Strategy={Strategy}";
}
