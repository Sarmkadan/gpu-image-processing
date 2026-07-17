#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Extension methods for <see cref="ComputeShaderPipelineOptions"/> that provide
/// convenient configuration helpers and utility methods.
/// </summary>
public static class ComputeShaderPipelineOptionsExtensions
{
    /// <summary>
    /// Creates a deep copy of the current <see cref="ComputeShaderPipelineOptions"/> instance.
    /// </summary>
    /// <param name="options">The options instance to clone.</param>
    /// <returns>A new <see cref="ComputeShaderPipelineOptions"/> with identical property values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static ComputeShaderPipelineOptions Clone(this ComputeShaderPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new ComputeShaderPipelineOptions
        {
            DefaultStrategy = options.DefaultStrategy,
            MaxWorkgroupDimension = options.MaxWorkgroupDimension,
            BenchmarkGuidedOptimization = options.BenchmarkGuidedOptimization,
            EnableProfiling = options.EnableProfiling,
            MaxPipelineDepth = options.MaxPipelineDepth,
            DefaultLocalMemoryPerThreadBytes = options.DefaultLocalMemoryPerThreadBytes,
            OccupancyWarningThreshold = options.OccupancyWarningThreshold
        };
    }

    /// <summary>
    /// Configures the options for development/testing scenarios where maximum
    /// profiling and validation are desired.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="enableBenchmarking">Whether to enable benchmark-guided optimization.</param>
    /// <returns>The configured <paramref name="options"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static ComputeShaderPipelineOptions WithDevelopmentSettings(
        this ComputeShaderPipelineOptions options,
        bool enableBenchmarking = true)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.BenchmarkGuidedOptimization = enableBenchmarking;
        options.EnableProfiling = true;
        options.OccupancyWarningThreshold = 0.15;
        return options;
    }

    /// <summary>
    /// Configures the options for production scenarios where performance is critical
    /// and profiling overhead should be minimized.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="maxWorkgroupDimension">Maximum workgroup dimension to use.</param>
    /// <returns>The configured <paramref name="options"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxWorkgroupDimension"/> is outside valid range [1, 1024].
    /// </exception>
    public static ComputeShaderPipelineOptions WithProductionSettings(
        this ComputeShaderPipelineOptions options,
        int maxWorkgroupDimension = 32)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (maxWorkgroupDimension < 1 || maxWorkgroupDimension > 1024)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxWorkgroupDimension),
                maxWorkgroupDimension,
                "MaxWorkgroupDimension must be between 1 and 1024 inclusive.");
        }

        options.MaxWorkgroupDimension = maxWorkgroupDimension;
        options.BenchmarkGuidedOptimization = false;
        options.EnableProfiling = false;
        options.OccupancyWarningThreshold = 0.5;
        return options;
    }

    /// <summary>
    /// Gets the effective maximum local memory per thread in bytes, clamped to a
    /// reasonable upper bound for typical GPU architectures.
    /// </summary>
    /// <param name="options">The options instance.</param>
    /// <returns>The clamped local memory value in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static int GetClampedLocalMemoryPerThreadBytes(this ComputeShaderPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return Math.Min(options.DefaultLocalMemoryPerThreadBytes, 512);
    }

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey,TValue}"/> containing all current
    /// option values for serialization or logging purposes.
    /// </summary>
    /// <param name="options">The options instance.</param>
    /// <returns>A dictionary with option names and their string representations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static Dictionary<string, string> ToDictionary(this ComputeShaderPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [nameof(ComputeShaderPipelineOptions.DefaultStrategy)] = options.DefaultStrategy.ToString(),
            [nameof(ComputeShaderPipelineOptions.MaxWorkgroupDimension)] = options.MaxWorkgroupDimension.ToString(CultureInfo.InvariantCulture),
            [nameof(ComputeShaderPipelineOptions.BenchmarkGuidedOptimization)] = options.BenchmarkGuidedOptimization.ToString(),
            [nameof(ComputeShaderPipelineOptions.EnableProfiling)] = options.EnableProfiling.ToString(),
            [nameof(ComputeShaderPipelineOptions.MaxPipelineDepth)] = options.MaxPipelineDepth.ToString(CultureInfo.InvariantCulture),
            [nameof(ComputeShaderPipelineOptions.DefaultLocalMemoryPerThreadBytes)] = options.DefaultLocalMemoryPerThreadBytes.ToString(CultureInfo.InvariantCulture),
            [nameof(ComputeShaderPipelineOptions.OccupancyWarningThreshold)] = options.OccupancyWarningThreshold.ToString(CultureInfo.InvariantCulture)
        };
    }
}
