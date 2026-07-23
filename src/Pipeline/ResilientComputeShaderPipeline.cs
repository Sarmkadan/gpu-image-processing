#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// A resilient decorator for <see cref="IComputeShaderPipeline"/> that implements
/// automatic device-lost recovery and retry policies for compute shader dispatches.
/// </summary>
/// <remarks>
/// <para>
/// This decorator wraps any <see cref="IComputeShaderPipeline"/> implementation and adds:
/// <list type="bullet">
/// <item><description>Automatic detection of transient GPU errors (device lost, OOM under pressure)</description></item>
/// <item><description>Device reinitialization and pipeline recreation on transient errors</description></item>
/// <item><description>Exponential backoff retry policy with configurable limits</description></item>
/// <item><description>Classification of errors as transient vs fatal</description></item>
/// </list>
/// </para>
/// <para>
/// Retry behavior is controlled by <see cref="AppConstants.Performance"/> constants:
/// <list type="bullet">
/// <item><description><see cref="AppConstants.Performance.MaxComputeDispatchRetries"/> - Maximum retry attempts (default: 3)</description></item>
/// <item><description><see cref="AppConstants.Performance.InitialRetryDelayMs"/> - Initial delay in ms (default: 100)</description></item>
/// <item><description><see cref="AppConstants.Performance.MaxRetryDelayMs"/> - Maximum delay in ms (default: 5000)</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ResilientComputeShaderPipeline : IComputeShaderPipeline
{
    private readonly IComputeShaderPipeline _innerPipeline;
    private readonly GpuManagementService _gpuService;
    private readonly ILogger<ResilientComputeShaderPipeline> _logger;
    private readonly int _maxRetries;
    private readonly int _initialDelayMs;
    private readonly int _maxDelayMs;

    /// <summary>
    /// Initialises a new <see cref="ResilientComputeShaderPipeline"/> decorator.
    /// </summary>
    /// <param name="innerPipeline">The inner pipeline to decorate with resilience.</param>
    /// <param name="gpuService">GPU management service for device operations.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="maxRetries">Maximum retry attempts (null to use default from AppConstants).</param>
    /// <param name="initialDelayMs">Initial retry delay in ms (null to use default).</param>
    /// <param name="maxDelayMs">Maximum retry delay in ms (null to use default).</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public ResilientComputeShaderPipeline(
        IComputeShaderPipeline innerPipeline,
        GpuManagementService gpuService,
        ILogger<ResilientComputeShaderPipeline> logger,
        int? maxRetries = null,
        int? initialDelayMs = null,
        int? maxDelayMs = null)
    {
        _innerPipeline = innerPipeline ?? throw new ArgumentNullException(nameof(innerPipeline));
        _gpuService = gpuService ?? throw new ArgumentNullException(nameof(gpuService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _maxRetries = maxRetries ?? AppConstants.Performance.MaxComputeDispatchRetries;
        _initialDelayMs = initialDelayMs ?? AppConstants.Performance.InitialRetryDelayMs;
        _maxDelayMs = maxDelayMs ?? AppConstants.Performance.MaxRetryDelayMs;
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

        if (passes.Count > 100)
            throw new ArgumentException(
                $"Pipeline depth {passes.Count} exceeds maximum allowed depth of 100.",
                nameof(passes));

        var retryCount = 0;
        Exception? lastException = null;
        PipelineExecutionResult? lastResult = null;
        GpuDevice? currentDevice = null;

        while (retryCount <= _maxRetries)
        {
            try
            {
                // On first attempt or after recovery, get the device
                if (currentDevice == null || retryCount > 0)
                {
                    currentDevice = ResolveDevice(deviceId);
                    _logger.LogInformation(
                        "Attempt {Attempt}/{MaxAttempts} - Using device '{DeviceName}' (ID: {DeviceId})",
                        retryCount + 1,
                        _maxRetries + 1,
                        currentDevice.Name,
                        currentDevice.Id);
                }

                // Execute the pipeline
                var result = await _innerPipeline.ExecuteAsync(passes, currentDevice.Id, cancellationToken);

                // If execution succeeded, return the result
                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "Pipeline execution succeeded after {Attempt} attempt(s)",
                        retryCount + 1);
                    return result;
                }

                // If execution failed but not due to GPU exception, don't retry
                if (result.ErrorMessage != null && !result.ErrorMessage.Contains("GpuException"))
                {
                    _logger.LogWarning(
                        "Pipeline failed with non-GPU error after {Attempt} attempt(s): {Error}",
                        retryCount + 1,
                        result.ErrorMessage);
                    return result;
                }

                // Check if the error is transient and retryable
                var gpuException = TryExtractGpuException(result.ErrorMessage);
                if (gpuException == null || !gpuException.IsTransientError())
                {
                    _logger.LogWarning(
                        "Pipeline failed with fatal GPU error after {Attempt} attempt(s): {ErrorCode}",
                        retryCount + 1,
                        gpuException?.ErrorCode?.ToString("X8") ?? "Unknown");
                    return result;
                }

                // Transient error - log and retry
                _logger.LogWarning(
                    "Transient GPU error detected (attempt {Attempt}/{MaxAttempts}): {ErrorCode} - {DeviceName}",
                    retryCount + 1,
                    _maxRetries + 1,
                    gpuException.ErrorCode?.ToString("X8") ?? "Unknown",
                    gpuException.DeviceName ?? "Unknown");

                lastResult = result;
                lastException = gpuException;
                retryCount++;

                // Apply exponential backoff with jitter
                var delayMs = CalculateBackoffDelay(retryCount);
                _logger.LogInformation("Waiting {DelayMs}ms before retry...", delayMs);
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (GpuException gpuEx) when (gpuEx.IsTransientError())
            {
                _logger.LogWarning(
                    "Transient GPU exception (attempt {Attempt}/{MaxAttempts}): {ErrorCode} - {DeviceName}",
                    retryCount + 1,
                    _maxRetries + 1,
                    gpuEx.ErrorCode?.ToString("X8") ?? "Unknown",
                    gpuEx.DeviceName ?? "Unknown");

                lastException = gpuEx;
                retryCount++;

                // Apply exponential backoff with jitter
                var delayMs = CalculateBackoffDelay(retryCount);
                _logger.LogInformation("Waiting {DelayMs}ms before recovery and retry...", delayMs);
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (GpuException gpuEx)
            {
                // Fatal GPU error - don't retry
                _logger.LogError(
                    gpuEx,
                    "Fatal GPU exception after {Attempt} attempt(s): {ErrorCode}",
                    retryCount + 1,
                    gpuEx.ErrorCode?.ToString("X8") ?? "Unknown");

                // Re-throw with appropriate error code
                throw new GpuException(
                    $"GPU operation failed after {retryCount} retry attempts: {gpuEx.Message}",
                    gpuEx,
                    gpuEx.DeviceName,
                    gpuEx.ErrorCode ?? AppConstants.ErrorCodes.GpuInitializationFailed);
            }
            catch (Exception ex)
            {
                // Non-GPU exception - don't retry
                _logger.LogError(
                    ex,
                    "Non-GPU exception after {Attempt} attempt(s)",
                    retryCount + 1);
                throw;
            }
        }

        // If we exhausted retries, throw the last exception
        if (lastException != null)
        {
            _logger.LogError(
                lastException,
                "GPU operation failed after {MaxRetries} retry attempts",
                _maxRetries);

            throw new GpuException(
                $"GPU operation failed after {_maxRetries} retry attempts: {lastException.Message}",
                lastException,
                lastException is GpuException gpuEx ? gpuEx.DeviceName : null,
                lastException is GpuException gpuEx2 ? gpuEx2.ErrorCode ?? AppConstants.ErrorCodes.GpuInitializationFailed : AppConstants.ErrorCodes.GpuInitializationFailed);
        }

        throw new InvalidOperationException("Unexpected error state - no exception recorded but retries exhausted");
    }

    /// <inheritdoc />
    public async Task<WorkgroupConfiguration> OptimizeWorkgroupAsync(
        ComputeShaderPass pass,
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pass);

        var retryCount = 0;
        Exception? lastException = null;
        var currentDeviceId = deviceId;

        while (retryCount <= _maxRetries)
        {
            try
            {
                // After a transient failure the original device may have been lost;
                // re-resolve so a retry can fail over to another available device
                // instead of repeatedly targeting a device that no longer exists.
                if (retryCount > 0)
                {
                    currentDeviceId = ResolveDevice(deviceId).Id;
                }

                var config = await _innerPipeline.OptimizeWorkgroupAsync(pass, currentDeviceId, cancellationToken);
                return config;
            }
            catch (GpuException gpuEx) when (gpuEx.IsTransientError())
            {
                _logger.LogWarning(
                    "Transient GPU exception during workgroup optimization (attempt {Attempt}/{MaxAttempts}): {ErrorCode}",
                    retryCount + 1,
                    _maxRetries + 1,
                    gpuEx.ErrorCode?.ToString("X8") ?? "Unknown");

                lastException = gpuEx;
                retryCount++;

                var delayMs = CalculateBackoffDelay(retryCount);
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (GpuException gpuEx)
            {
                _logger.LogError(
                    gpuEx,
                    "Fatal GPU exception during workgroup optimization after {Attempt} attempt(s): {ErrorCode}",
                    retryCount + 1,
                    gpuEx.ErrorCode?.ToString("X8") ?? "Unknown");

                throw new GpuException(
                    $"Workgroup optimization failed after {retryCount} retry attempts: {gpuEx.Message}",
                    gpuEx,
                    gpuEx.DeviceName,
                    gpuEx.ErrorCode ?? AppConstants.ErrorCodes.KernelCompilationFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Non-GPU exception during workgroup optimization after {Attempt} attempt(s)",
                    retryCount + 1);
                throw;
            }
        }

        if (lastException != null)
        {
            throw new GpuException(
                $"Workgroup optimization failed after {_maxRetries} retry attempts: {lastException.Message}",
                lastException is GpuException gpuEx ? gpuEx : null,
                lastException is GpuException gpuEx2 ? gpuEx2.DeviceName : null,
                AppConstants.ErrorCodes.KernelCompilationFailed);
        }

        throw new InvalidOperationException("Unexpected error state during workgroup optimization");
    }

    /// <inheritdoc />
    public async Task<PipelineStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _innerPipeline.GetStatisticsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ResetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        await _innerPipeline.ResetStatisticsAsync(cancellationToken);
    }

    // -- Private helpers --------------------------------------------------------

    private GpuDevice ResolveDevice(Guid deviceId)
    {
        GpuDevice? device = deviceId == Guid.Empty
            ? _gpuService.GetBestDevice()
            : _gpuService.GetDeviceById(deviceId);

        if (device is null || !device.IsAvailable)
            throw new GpuException(
                $"GPU device '{(deviceId == Guid.Empty ? "auto" : deviceId.ToString())}' is not available.",
                null,
                AppConstants.ErrorCodes.DeviceNotAvailable);

        return device;
    }

    private int CalculateBackoffDelay(int retryCount)
    {
        // Exponential backoff with jitter: min(2^retry * initial, max)
        var exponentialDelay = Math.Min(
            _initialDelayMs * (int)Math.Pow(2, retryCount - 1),
            _maxDelayMs);

        // Add jitter: ±20% random variation
        var random = new Random();
        var jitterFactor = 0.8 + (random.NextDouble() * 0.4); // 0.8 to 1.2
        var delayMs = (int)(exponentialDelay * jitterFactor);

        return delayMs;
    }

    private GpuException? TryExtractGpuException(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return null;

        // Try to extract GpuException from error message
        // Look for patterns like "GpuException" or error codes
        if (errorMessage.Contains("GpuException") || errorMessage.Contains("device lost") ||
            errorMessage.Contains("timeout") || errorMessage.Contains("out of memory"))
        {
            // Create a synthetic GpuException with appropriate error code
            int errorCode = -1; // Generic error

            if (errorMessage.Contains("device lost") || errorMessage.Contains("timeout"))
                errorCode = unchecked((int)0x80000002); // CL_DEVICE_NOT_AVAILABLE or timeout
            else if (errorMessage.Contains("out of memory"))
                errorCode = 0x00000003; // CL_OUT_OF_DEVICE_MEMORY

            return new GpuException(errorMessage, errorCode: errorCode);
        }

        return null;
    }
}