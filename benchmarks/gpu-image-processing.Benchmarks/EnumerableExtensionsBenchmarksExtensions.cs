#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Extension methods for EnumerableExtensionsBenchmarks to demonstrate additional
/// benchmark scenarios and utility operations that complement the existing benchmarks.
/// </summary>
public static class EnumerableExtensionsBenchmarksExtensions
{
    /// <summary>
    /// Creates a benchmark that measures the performance of filtering a collection
    /// using a predicate function.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="predicate">The filter predicate. Must not be null.</param>
    /// <returns>The count of items that match the predicate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public static int Filter_1000ByPredicate(this EnumerableExtensionsBenchmarks benchmarks, Func<int, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return Enumerable.Range(0, 1000)
            .Where(predicate)
            .Count();
    }

    /// <summary>
    /// Creates a benchmark that measures the performance of projecting each element
    /// to a new value using a selector function.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <param name="selector">The projection selector. Must not be null.</param>
    /// <returns>The sum of all projected values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is null.</exception>
    public static int Select_1000ToSum(this EnumerableExtensionsBenchmarks benchmarks, Func<int, int> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return Enumerable.Range(0, 1000)
            .Select(selector)
            .Sum();
    }

    /// <summary>
    /// Creates a benchmark that measures the performance of reversing a collection.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The first element of the reversed collection.</returns>
    public static int Reverse_1000First(this EnumerableExtensionsBenchmarks benchmarks)
    {
        return Enumerable.Range(0, 1000)
            .Reverse()
            .FirstOrDefault();
    }

    /// <summary>
    /// Creates a benchmark that measures the performance of ordering a collection
    /// by a key selector function.
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance.</param>
    /// <returns>The first element of the ordered collection.</returns>
    public static int OrderBy_1000First(this EnumerableExtensionsBenchmarks benchmarks)
    {
        return Enumerable.Range(0, 1000)
            .OrderBy(x => x % 10)
            .FirstOrDefault();
    }
}