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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
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
    /// <returns>The metadata value, or default(T) if the key is not found or the value is of the wrong type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
    public static T? GetMetadataValue<T>(this ProcessingResult result, string key)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (result.ResultMetadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }
}
