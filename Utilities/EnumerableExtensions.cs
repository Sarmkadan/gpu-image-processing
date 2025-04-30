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
    /// Extension methods for IEnumerable providing functional programming utilities.
    /// Provides filtering, grouping, batching, and aggregation helpers.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Batches items into fixed-size groups
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
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
        /// Filters out null values from a sequence
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(x => x != null);
        }

        /// <summary>
        /// Repeats a sequence indefinitely (lazy)
        /// </summary>
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source)
        {
            while (true)
                foreach (var item in source)
                    yield return item;
        }

        /// <summary>
        /// Returns distinct items by a key selector
        /// </summary>
        public static IEnumerable<T> DistinctBy<T, K>(this IEnumerable<T> source, Func<T, K> keySelector)
        {
            var seen = new HashSet<K>();

            foreach (var item in source)
            {
                var key = keySelector(item);

                if (seen.Add(key))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns the maximum item by a specified key
        /// </summary>
        public static T MaxBy<T, K>(this IEnumerable<T> source, Func<T, K> keySelector) where K : IComparable<K>
        {
            return source.Aggregate((a, b) =>
                keySelector(a).CompareTo(keySelector(b)) > 0 ? a : b);
        }

        /// <summary>
        /// Returns the minimum item by a specified key
        /// </summary>
        public static T MinBy<T, K>(this IEnumerable<T> source, Func<T, K> keySelector) where K : IComparable<K>
        {
            return source.Aggregate((a, b) =>
                keySelector(a).CompareTo(keySelector(b)) < 0 ? a : b);
        }

        /// <summary>
        /// Finds the index of an item in the sequence
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
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
        /// Finds the index of an item matching a predicate
        /// </summary>
        public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
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
        /// Converts to a Dictionary with error handling for duplicate keys
        /// </summary>
        public static Dictionary<K, V> SafeToDictionary<T, K, V>(
            this IEnumerable<T> source,
            Func<T, K> keySelector,
            Func<T, V> valueSelector)
        {
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
        /// Groups consecutive items by a key
        /// </summary>
        public static IEnumerable<IEnumerable<T>> GroupConsecutive<T, K>(
            this IEnumerable<T> source,
            Func<T, K> keySelector)
        {
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
        /// Takes items while a condition is true
        /// </summary>
        public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
            {
                if (!predicate(item))
                    break;

                yield return item;
            }
        }

        /// <summary>
        /// Applies an action to each item (for side effects)
        /// </summary>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Chains multiple enumerables together
        /// </summary>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params IEnumerable<T>[] others)
        {
            foreach (var item in source)
                yield return item;

            foreach (var enumerable in others)
            {
                foreach (var item in enumerable)
                    yield return item;
            }
        }

        /// <summary>
        /// Returns true if all items are equal
        /// </summary>
        public static bool AllEqual<T>(this IEnumerable<T> source)
        {
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
        /// Shuffles a sequence using Fisher-Yates algorithm
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
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
        /// Pads a sequence to a minimum length
        /// </summary>
        public static IEnumerable<T> Pad<T>(this IEnumerable<T> source, int minLength, T paddingItem = default)
        {
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
