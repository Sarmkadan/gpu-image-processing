#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides extension methods for <see cref="FilterChain"/> operations.
/// </summary>
public static class FilterChainExtensions
{
    /// <summary>
    /// Finds a filter step by its filter ID.
    /// </summary>
    /// <param name="chain">The filter chain to search.</param>
    /// <param name="filterId">The filter ID to find.</param>
    /// <returns>The found <see cref="FilterStep"/> or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chain"/> is null.</exception>
    public static FilterStep? FindStepByFilterId(this FilterChain chain, Guid filterId)
    {
        ArgumentNullException.ThrowIfNull(chain);

        return chain.Steps.FirstOrDefault(s => s.FilterId == filterId);
    }

    /// <summary>
    /// Gets a filter step by its index in the chain.
    /// </summary>
    /// <param name="chain">The filter chain.</param>
    /// <param name="index">The zero-based index of the step.</param>
    /// <returns>The <see cref="FilterStep"/> at the specified index.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chain"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    public static FilterStep GetStepByIndex(this FilterChain chain, int index)
    {
        ArgumentNullException.ThrowIfNull(chain);

        if (index < 0 || index >= chain.Steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                string.Format(CultureInfo.InvariantCulture,
                    "Index must be between 0 and {0}. Actual: {1}",
                    chain.Steps.Count - 1,
                    index));
        }

        return chain.Steps[index];
    }

    /// <summary>
    /// Determines whether the filter chain contains a step with the specified filter ID.
    /// </summary>
    /// <param name="chain">The filter chain.</param>
    /// <param name="filterId">The filter ID to check.</param>
    /// <returns>True if the filter is in the chain; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chain"/> is null.</exception>
    public static bool HasStep(this FilterChain chain, Guid filterId)
    {
        ArgumentNullException.ThrowIfNull(chain);

        return chain.Steps.Any(s => s.FilterId == filterId);
    }

    /// <summary>
    /// Calculates the estimated memory footprint of processing this filter chain in megabytes.
    /// </summary>
    /// <param name="chain">The filter chain.</param>
    /// <param name="bytesPerPixel">Bytes per pixel for the image being processed (default: 4 for RGBA).</param>
    /// <param name="estimatedImageSize">Estimated image dimensions in pixels (width × height).</param>
    /// <returns>Estimated memory usage in MB.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chain"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bytesPerPixel or estimatedImageSize is invalid.</exception>
    public static double CalculateMemoryFootprint(
        this FilterChain chain,
        int bytesPerPixel = 4,
        int estimatedImageSize = 1024 * 1024) // 1MP image
    {
        ArgumentNullException.ThrowIfNull(chain);

        if (bytesPerPixel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesPerPixel),
                "Bytes per pixel must be positive.");
        }

        if (estimatedImageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedImageSize),
                "Estimated image size must be positive.");
        }

        // Base memory: image buffer × bytes per pixel
        double baseMemoryBytes = estimatedImageSize * bytesPerPixel;

        // Additional memory for parallel execution: MaxParallelSteps × baseMemory × 2 (input + output buffers)
        double parallelMemoryBytes = 0;
        if (chain.AllowParallelExecution && chain.MaxParallelSteps > 1)
        {
            parallelMemoryBytes = baseMemoryBytes * chain.MaxParallelSteps * 2;
        }

        // Memory for intermediate results caching
        double cacheMemoryBytes = 0;
        if (chain.CacheIntermediateResults)
        {
            // Estimate cache size as 2× base image for each enabled step
            cacheMemoryBytes = baseMemoryBytes * chain.GetEnabledFilterCount() * 2;
        }

        // Total estimated memory in bytes
        double totalBytes = baseMemoryBytes + parallelMemoryBytes + cacheMemoryBytes;

        // Convert to MB with 2 decimal places
        return Math.Round(totalBytes / (1024 * 1024), 2, MidpointRounding.AwayFromZero);
    }
}