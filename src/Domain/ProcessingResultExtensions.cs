using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides extension methods for the <see cref="ProcessingResult"/> class.
/// </summary>
public static class ProcessingResultExtensions
{
    /// <summary>
    /// Gets the names of all filters applied to the processing result.
    /// </summary>
    /// <param name="result">The processing result.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of filter names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    public static IEnumerable<string> GetFilterNames(this ProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.FiltersApplied.Select(f => f.FilterName);
    }

    /// <summary>
    /// Gets the total processing time as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="result">The processing result.</param>
    /// <returns>The total processing time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    public static TimeSpan GetDurationTimeSpan(this ProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return TimeSpan.FromMilliseconds(result.ProcessingTimeMilliseconds);
    }

    /// <summary>
    /// Gets a value from the result metadata cast to the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="result">The processing result.</param>
    /// <param name="key">The metadata key.</param>
    /// <returns>The metadata value, or <see langword="default"/> if the key is not found or the value is of the wrong type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty.</exception>
    public static T? GetMetadataValue<T>(this ProcessingResult result, string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return result.ResultMetadata.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }
}
