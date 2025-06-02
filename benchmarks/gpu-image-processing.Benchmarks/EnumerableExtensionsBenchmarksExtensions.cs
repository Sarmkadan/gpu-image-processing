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
    /// <param name="source">The source collection to filter.</param>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>The count of items that match the predicate.</returns>
    public static int Filter_1000ByPredicate(this EnumerableExtensionsBenchmarks benchmarks, Func<int, bool> predicate)
    {
        int count = 0;
        foreach (var item in benchmarks.Batch_1000By32() is int batchCount && batchCount > 0
            ? Enumerable.Range(0, 1000).Where(predicate)
            : Enumerable.Empty<int>())
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Creates a benchmark that measures the performance of projecting each element
    /// to a new value using a selector function.
    /// </summary>
    /// <param name="source">The source collection to project.</param>
    /// <param name="selector">The projection selector.</param>
    /// <returns>The sum of all projected values.</returns>
    public static int Select_1000ToSum(this EnumerableExtensionsBenchmarks benchmarks, Func<int, int> selector)
    {
        return benchmarks.Batch_1000By32() is int batchCount && batchCount > 0
            ? Enumerable.Range(0, 1000).Select(selector).Sum()
            : 0;
    }

    /// <summary>
    /// Creates a benchmark that measures the performance of reversing a collection.
    /// </summary>
    /// <returns>The first element of the reversed collection.</returns>
    public static int Reverse_1000First(this EnumerableExtensionsBenchmarks benchmarks)
    {
        var reversed = Enumerable.Range(0, 1000).Reverse();
        return reversed.FirstOrDefault();
    }

    /// <summary>
    /// Creates a benchmark that measures the performance of ordering a collection
    /// by a key selector function.
    /// </summary>
    /// <returns>The first element of the ordered collection.</returns>
    public static int OrderBy_1000First(this EnumerableExtensionsBenchmarks benchmarks)
    {
        return Enumerable.Range(0, 1000)
            .OrderBy(x => x % 10)
            .FirstOrDefault();
    }
}