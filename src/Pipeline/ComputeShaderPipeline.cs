#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Configuration;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Orchestrates compute shader execution against GPU devices, combining automatic
/// workgroup optimisation, per-pass profiling, and GPU memory lifecycle management.
/// </summary>
/// <remarks>
/// <para>
/// Before each pipeline run the engine sorts passes by <see cref="ComputeShaderPass.Priority"/>,
/// calls <see cref="IWorkgroupOptimizer"/> for any pass that has not yet been optimised,
/// then dispatches the passes sequentially against the resolved device.
/// </para>
/// <para>
/// GPU memory is allocated from <see cref="GpuManagementService"/> before execution
/// and released in a <c>finally</c> block so resources are always returned to the
/// pool even when a pass throws.
/// </para>
/// <para>
/// All counters are protected by a private lock and are safe for concurrent callers.
/// </para>
/// </remarks>
public sealed class ComputeShaderPipeline : IComputeShaderPipeline
{
    private readonly IWorkgroupOptimizer         _optimizer;
    private readonly GpuManagementService        _gpuService;
    private readonly PerformanceMonitoringService _performanceMonitor;
    private readonly ComputeShaderPipelineOptions _options;
    private readonly ILogger<ComputeShaderPipeline> _logger;

    private readonly object _statsLock = new();
    private long   _totalExecutions;
    private long   _totalPassesExecuted;
    private long   _totalPassesFailed;
    private long   _totalPixelsProcessed;
    private double _totalProcessingMs;
    private double _totalOccupancySum;
    private long   _totalOccupancySamples;

    /// <summary>
    /// Initialises a new <see cref="ComputeShaderPipeline"/>.
    /// </summary>
    /// <param name="optimizer">Workgroup optimiser called before each dispatch.</param>
    /// <param name="gpuService">Device manager used for memory allocation and device resolution.</param>
    /// <param name="performanceMonitor">Service that records per-execution telemetry.</param>
    /// <param name="options">Pipeline configuration options.</param>
    /// <param name="logger">Logger for diagnostic and profiling output.</param>
    public ComputeShaderPipeline(
        IWorkgroupOptimizer           optimizer,
        GpuManagementService          gpuService,
        PerformanceMonitoringService  performanceMonitor,
        ComputeShaderPipelineOptions  options,
        ILogger<ComputeShaderPipeline> logger)
    {
        _optimizer          = optimizer          ?? throw new ArgumentNullException(nameof(optimizer));
        _gpuService         = gpuService         ?? throw new ArgumentNullException(nameof(gpuService));
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _options            = options            ?? throw new ArgumentNullException(nameof(options));
        _logger             = logger             ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PipelineExecutionResult> ExecuteAsync(
        IReadOnlyList<ComputeShaderPass> passes,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(passes);

        if (passes.Count == 0)
            throw new ArgumentException("At least one pass is required.", nameof(passes));

        if (passes.Count > _options.MaxPipelineDepth)
            throw new ArgumentException(
                $"Pipeline depth {passes.Count} exceeds the configured maximum of {_options.MaxPipelineDepth}.",
                nameof(passes));

        var device  = ResolveDevice(deviceId);
        var ordered = passes.OrderBy(p => p.Priority).ToList();

        long estimatedBytes   = EstimateMemoryRequirement(ordered);
        bool memoryAllocated  = false;
        var  passRecords      = new List<PassExecutionRecord>(ordered.Count);
        var  pipelineSw       = Stopwatch.StartNew();

        _logger.LogInformation(
            "Starting pipeline — {Count} pass(es) on '{Device}'",
            ordered.Count, device.Name);

        try
        {
            _gpuService.AllocateMemory(estimatedBytes, device.Id);
            memoryAllocated = true;

            foreach (var pass in ordered)
            {
                cancellationToken.ThrowIfCancellationRequested();
                passRecords.Add(await DispatchPassAsync(pass, device, cancellationToken));
            }

            pipelineSw.Stop();

            long pixels = ordered
                .SelectMany(p => p.InputImages)
                .Sum(img => img.CalculatePixelDataSize());

            var result = BuildResult(passRecords, device.Id, pipelineSw.Elapsed, pixels, succeeded: true);
            AccumulateStats(result);

            _performanceMonitor.RecordOperation(pipelineSw.Elapsed.TotalMilliseconds, success: true);
            _performanceMonitor.UpdateThroughput(
                pipelineSw.Elapsed.TotalSeconds > 0 && pixels > 0
                    ? (long)(pixels / pipelineSw.Elapsed.TotalSeconds)
                    : 0,
                pipelineSw.Elapsed.TotalSeconds > 0
                    ? estimatedBytes / (1024.0 * 1024.0) / pipelineSw.Elapsed.TotalSeconds
                    : 0.0);

            _logger.LogInformation(
                "Pipeline completed in {Ms:F1} ms — passed={P} failed={F} occupancy={Occ:P0}",
                pipelineSw.Elapsed.TotalMilliseconds,
                result.PassesExecuted, result.PassesFailed, result.AverageOccupancy);

            return result;
        }
        catch (OperationCanceledException)
        {
            pipelineSw.Stop();
            _logger.LogWarning("Pipeline cancelled after {Ms:F1} ms", pipelineSw.Elapsed.TotalMilliseconds);
            var result = BuildResult(passRecords, device.Id, pipelineSw.Elapsed, 0,
                succeeded: false, error: "Pipeline cancelled by caller.");
            AccumulateStats(result);
            throw;
        }
        catch (Exception ex)
        {
            pipelineSw.Stop();
            _performanceMonitor.RecordOperation(pipelineSw.Elapsed.TotalMilliseconds, success: false);
            _logger.LogError(ex, "Pipeline failed after {Ms:F1} ms", pipelineSw.Elapsed.TotalMilliseconds);
            var result = BuildResult(passRecords, device.Id, pipelineSw.Elapsed, 0,
                succeeded: false, error: ex.Message);
            AccumulateStats(result);
            return result;
        }
        finally
        {
            if (memoryAllocated)
                _gpuService.DeallocateMemory(estimatedBytes, device.Id);
        }
    }

    /// <inheritdoc />
    public async Task<WorkgroupConfiguration> OptimizeWorkgroupAsync(
        ComputeShaderPass pass,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pass);

        var device       = ResolveDevice(deviceId);
        var (width, height) = InferImageDimensions(pass);

        WorkgroupConfiguration config = _options.BenchmarkGuidedOptimization
            ? await _optimizer.BenchmarkAsync(device, width, height,
                _options.DefaultLocalMemoryPerThreadBytes, cancellationToken)
            : _optimizer.Compute(device, width, height,
                _options.DefaultLocalMemoryPerThreadBytes, _options.DefaultStrategy);

        pass.WorkgroupConfiguration = config;
        return config;
    }

    /// <inheritdoc />
    public Task<PipelineStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        lock (_statsLock)
        {
            long total = _totalPassesExecuted + _totalPassesFailed;

            return Task.FromResult(new PipelineStatistics
            {
                TotalExecutions       = _totalExecutions,
                TotalPassesExecuted   = _totalPassesExecuted,
                TotalPassesFailed     = _totalPassesFailed,
                TotalPixelsProcessed  = _totalPixelsProcessed,
                TotalProcessingTime   = TimeSpan.FromMilliseconds(_totalProcessingMs),
                AveragePassDurationMs = total > 0 ? _totalProcessingMs / total : 0.0,
                AverageOccupancy      = _totalOccupancySamples > 0
                    ? _totalOccupancySum / _totalOccupancySamples
                    : 0.0,
                SuccessRate = total > 0
                    ? (double)_totalPassesExecuted / total
                    : 1.0,
                CollectedAt = DateTime.UtcNow
            });
        }
    }

    /// <inheritdoc />
    public Task ResetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        lock (_statsLock)
        {
            _totalExecutions       = 0;
            _totalPassesExecuted   = 0;
            _totalPassesFailed     = 0;
            _totalPixelsProcessed  = 0;
            _totalProcessingMs     = 0;
            _totalOccupancySum     = 0;
            _totalOccupancySamples = 0;
        }

        _logger.LogInformation("Pipeline statistics reset.");
        return Task.CompletedTask;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private GpuDevice ResolveDevice(Guid deviceId)
    {
        GpuDevice? device = deviceId == Guid.Empty
            ? _gpuService.GetBestDevice()
            : _gpuService.GetDeviceById(deviceId);

        if (device is null || !device.IsAvailable)
            throw new GpuException(
                $"GPU device '{(deviceId == Guid.Empty ? "auto" : deviceId)}' is not available.",
                device?.Name,
                AppConstants.ErrorCodes.DeviceNotAvailable);

        return device;
    }

    private async Task<PassExecutionRecord> DispatchPassAsync(
        ComputeShaderPass pass,
        GpuDevice         device,
        CancellationToken cancellationToken)
    {
        if (pass.WorkgroupConfiguration == null)
        {
            var (w, h) = InferImageDimensions(pass);
            var kernelSource = string.IsNullOrEmpty(pass.KernelSource) ? string.Empty : pass.KernelSource;
            pass.WorkgroupConfiguration = _options.BenchmarkGuidedOptimization
                ? await _optimizer.BenchmarkAsync(device, w, h,
                    _options.DefaultLocalMemoryPerThreadBytes, cancellationToken)
                : _optimizer.Compute(device, w, h,
                    _options.DefaultLocalMemoryPerThreadBytes, _options.DefaultStrategy);
        }

        var sw = Stopwatch.StartNew();

        try
        {
            await SimulateKernelDispatchAsync(pass, cancellationToken);
            sw.Stop();

            double occupancy = pass.WorkgroupConfiguration.EstimatedOccupancy;

            if (_options.EnableProfiling && occupancy < _options.OccupancyWarningThreshold)
            {
                _logger.LogWarning(
                    "Pass '{Kernel}' low occupancy {Occ:P0} (threshold {Thr:P0})",
                    pass.KernelName, occupancy, _options.OccupancyWarningThreshold);
            }

            return new PassExecutionRecord(pass.Id, pass.KernelName, true, sw.Elapsed, occupancy);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            _logger.LogError(ex, "Pass '{Kernel}' failed after {Ms:F1} ms",
                pass.KernelName, sw.Elapsed.TotalMilliseconds);

            return new PassExecutionRecord(pass.Id, pass.KernelName, false, sw.Elapsed, 0.0, ex.Message);
        }
    }

    private static async Task SimulateKernelDispatchAsync(
        ComputeShaderPass pass,
        CancellationToken cancellationToken)
    {
        // Yield to model non-blocking GPU dispatch submission.
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        // In a full OpenCL host implementation the kernel would be compiled via
        // clCreateKernel, buffers bound with clSetKernelArg, and the work enqueued
        // via clEnqueueNDRangeKernel with the dimensions from WorkgroupConfiguration.
        _ = pass;
    }

    private static PipelineExecutionResult BuildResult(
        List<PassExecutionRecord> records,
        Guid                      deviceId,
        TimeSpan                  duration,
        long                      pixels,
        bool                      succeeded,
        string?                   error = null)
    {
        double avgOccupancy = records.Count > 0
            ? records.Average(r => r.Occupancy)
            : 0.0;

        return new PipelineExecutionResult
        {
            Succeeded            = succeeded,
            ErrorMessage         = error,
            TotalDuration        = duration,
            PassesExecuted       = records.Count(r => r.Succeeded),
            PassesFailed         = records.Count(r => !r.Succeeded),
            PassRecords          = records,
            AverageOccupancy     = avgOccupancy,
            TotalPixelsProcessed = pixels,
            DeviceId             = deviceId,
            CompletedAt          = DateTime.UtcNow
        };
    }

    private void AccumulateStats(PipelineExecutionResult result)
    {
        lock (_statsLock)
        {
            _totalExecutions++;
            _totalPassesExecuted   += result.PassesExecuted;
            _totalPassesFailed     += result.PassesFailed;
            _totalPixelsProcessed  += result.TotalPixelsProcessed;
            _totalProcessingMs     += result.TotalDuration.TotalMilliseconds;
            _totalOccupancySum     += result.AverageOccupancy;
            _totalOccupancySamples++;
        }
    }

    private static long EstimateMemoryRequirement(IReadOnlyList<ComputeShaderPass> passes)
    {
        long total = 0;
        foreach (var pass in passes)
        {
            foreach (var img in pass.InputImages)
                total += img.CalculatePixelDataSize();

            if (pass.OutputImage is not null)
                total += pass.OutputImage.CalculatePixelDataSize();
        }

        // Reserve at least 1 MB to avoid zero-byte allocations.
        return Math.Max(total, 1024 * 1024);
    }

    private static (int Width, int Height) InferImageDimensions(ComputeShaderPass pass)
    {
        var first = pass.InputImages.FirstOrDefault() ?? pass.OutputImage;

        // Fall back to a common HD resolution when no images are attached yet.
        return first is not null
            ? (Math.Max(first.Width, 1), Math.Max(first.Height, 1))
            : (1920, 1080);
    }
}
