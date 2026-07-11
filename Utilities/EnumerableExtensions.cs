#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/> providing functional programming utilities.
    /// Provides filtering, grouping, batching, and aggregation helpers.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Batches items into fixed-size groups.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to batch.</param>
        /// <param name="batchSize">The maximum number of items in each batch. Must be positive.</param>
        /// <returns>An enumerable of batches, each containing up to <paramref name="batchSize"/> items.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="batchSize"/> is not positive.</exception>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (batchSize <= 0)
                throw new ArgumentException("Batch size must be positive", nameof(batchSize));

            var batch = new List<T>();

            foreach (var item in source)
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }

        /// <summary>
        /// Filters out null values from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to filter.</param>
        /// <returns>An enumerable containing only non-null elements.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.Where(x => x != null);
        }

        /// <summary>
        /// Repeats a sequence indefinitely (lazy).
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to repeat.</param>
        /// <returns>An infinite sequence that repeats the source.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            while (true)
                foreach (var item in source)
                    yield return item;
        }

        /// <summary>
        /// Returns distinct items by a key selector.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <typeparam name="K">The type of the key used for comparison.</typeparam>
        /// <param name="source">The source sequence to process.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>An enumerable containing only the first occurrence of each distinct key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        public static IEnumerable<T> DistinctBy<T, K>(this IEnumerable<T> source, Func<T, K> keySelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            var seen = new HashSet<K>();

            foreach (var item in source)
            {
                var key = keySelector(item);

                if (seen.Add(key))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns the maximum item by a specified key.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <typeparam name="K">The type of the key used for comparison.</typeparam>
        /// <param name="source">The source sequence to search.</param>
        /// <param name="keySelector">A function to extract the key for comparison.</param>
        /// <returns>The element with the maximum key value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
        public static T MaxBy<T, K>(this IEnumerable<T> source, Func<T, K> keySelector) where K : IComparable<K>
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            if (!source.Any())
                throw new InvalidOperationException("Sequence contains no elements");

            return source.Aggregate((a, b) => keySelector(a).CompareTo(keySelector(b)) > 0 ? a : b);
        }

        /// <summary>
        /// Returns the minimum item by a specified key.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <typeparam name="K">The type of the key used for comparison.</typeparam>
        /// <param name="source">The source sequence to search.</param>
        /// <param name="keySelector">A function to extract the key for comparison.</param>
        /// <returns>The element with the minimum key value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
        public static T MinBy<T, K>(this IEnumerable<T> source, Func<T, K> keySelector) where K : IComparable<K>
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            if (!source.Any())
                throw new InvalidOperationException("Sequence contains no elements");

            return source.Aggregate((a, b) => keySelector(a).CompareTo(keySelector(b)) < 0 ? a : b);
        }

        /// <summary>
        /// Finds the index of an item in the sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to search.</param>
        /// <param name="item">The item to find.</param>
        /// <returns>The zero-based index of the item, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            ArgumentNullException.ThrowIfNull(source);

            var index = 0;

            foreach (var element in source)
            {
                if (Equals(element, item))
                    return index;

                index++;
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of an item matching a predicate.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to search.</param>
        /// <param name="predicate">A function to test each element.</param>
        /// <returns>The zero-based index of the first matching item, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            var index = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    return index;

                index++;
            }

            return -1;
        }

        /// <summary>
        /// Converts to a Dictionary with error handling for duplicate keys.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of values in the dictionary.</typeparam>
        /// <param name="source">The source sequence to convert.</param>
        /// <param name="keySelector">A function to extract the key from each element.</param>
        /// <param name="valueSelector">A function to extract the value from each element.</param>
        /// <returns>A dictionary containing the first occurrence of each key-value pair.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="valueSelector"/> is null.</exception>
        public static Dictionary<K, V> SafeToDictionary<T, K, V>(
            this IEnumerable<T> source,
            Func<T, K> keySelector,
            Func<T, V> valueSelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(valueSelector);

            var dict = new Dictionary<K, V>();

            foreach (var item in source)
            {
                var key = keySelector(item);
                var value = valueSelector(item);

                if (!dict.ContainsKey(key))
                    dict[key] = value;
            }

            return dict;
        }

        /// <summary>
        /// Groups consecutive items by a key.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <typeparam name="K">The type of the key used for grouping.</typeparam>
        /// <param name="source">The source sequence to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>An enumerable of groups containing consecutive items with the same key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        public static IEnumerable<IEnumerable<T>> GroupConsecutive<T, K>(
            this IEnumerable<T> source,
            Func<T, K> keySelector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
                yield break;

            var currentGroup = new List<T> { enumerator.Current };
            var currentKey = keySelector(enumerator.Current);

            while (enumerator.MoveNext())
            {
                var newKey = keySelector(enumerator.Current);

                if (Equals(newKey, currentKey))
                {
                    currentGroup.Add(enumerator.Current);
                }
                else
                {
                    yield return currentGroup;
                    currentGroup = new List<T> { enumerator.Current };
                    currentKey = newKey;
                }
            }

            if (currentGroup.Count > 0)
                yield return currentGroup;
        }

        /// <summary>
        /// Takes items while a condition is true.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to process.</param>
        /// <param name="predicate">A function to test each element.</param>
        /// <returns>An enumerable containing elements from the start while the predicate returns true.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            foreach (var item in source)
            {
                if (!predicate(item))
                    break;

                yield return item;
            }
        }

        /// <summary>
        /// Applies an action to each item (for side effects).
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to process.</param>
        /// <param name="action">The action to apply to each element.</param>
        /// <returns>An enumerable containing the original elements.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Chains multiple enumerables together.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequences.</typeparam>
        /// <param name="source">The first sequence to concatenate.</param>
        /// <param name="others">Additional sequences to concatenate.</param>
        /// <returns>An enumerable containing all elements from all sequences in order.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="others"/> is null.</exception>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params IEnumerable<T>[] others)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(others);

            foreach (var item in source)
                yield return item;

            foreach (var enumerable in others)
            {
                foreach (var item in enumerable)
                    yield return item;
            }
        }

        /// <summary>
        /// Returns true if all items are equal.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to check.</param>
        /// <returns>True if all elements are equal to each other, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static bool AllEqual<T>(this IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return true;

                var first = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    if (!Equals(enumerator.Current, first))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Shuffles a sequence using Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to shuffle.</param>
        /// <returns>A new list containing the shuffled elements.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            var list = source.ToList();
            // Random.Shared is thread-safe and avoids a per-call heap allocation.
            var random = Random.Shared;

            for (int i = list.Count - 1; i > 0; i--)
            {
                var randomIndex = random.Next(i + 1);
                var temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }

            return list;
        }

        /// <summary>
        /// Pads a sequence to a minimum length.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence to pad.</param>
        /// <param name="minLength">The minimum length the sequence should have.</param>
        /// <param name="paddingItem">The item to use for padding. Defaults to default(T).</param>
        /// <returns>An enumerable containing the original elements followed by padding items if needed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="minLength"/> is negative.</exception>
        public static IEnumerable<T> Pad<T>(this IEnumerable<T> source, int minLength, T paddingItem = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (minLength < 0)
                throw new ArgumentException("Minimum length cannot be negative", nameof(minLength));

            var count = 0;

            foreach (var item in source)
            {
                yield return item;
                count++;
            }

            while (count < minLength)
            {
                yield return paddingItem;
                count++;
            }
        }
    }
}