using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation methods for <see cref="EnumerableExtensions"/> operations.
    /// </summary>
    public static class EnumerableExtensionsValidation
    {
        /// <summary>
        /// Validates the enumerable operations by testing them with sample data.
        /// </summary>
        /// <returns>A list of human-readable problem descriptions. Empty list if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the extension class type cannot be validated.</exception>
        public static IReadOnlyList<string> Validate()
        {
            ArgumentNullException.ThrowIfNull(typeof(EnumerableExtensions));

            var problems = new List<string>();

            // Validate Batch behavior - should return non-null batches
            try
            {
                var source = new[] { 1, 2, 3, 4, 5 };
                var batchResult = source.Batch(2);
                ArgumentNullException.ThrowIfNull(batchResult);

                var batchCount = 0;
                var validBatchCount = 0;
                foreach (var batch in batchResult)
                {
                    ArgumentNullException.ThrowIfNull(batch);
                    validBatchCount++;
                    batchCount++;
                }

                if (batchCount == 0)
                {
                    problems.Add("Batch method returned empty enumerable.");
                }
                else if (validBatchCount != 3)
                {
                    problems.Add($"Batch method did not produce correct number of batches (expected 3, got {validBatchCount}).");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Batch method threw an exception: {ex.Message}");
            }

            // Validate WhereNotNull behavior - should filter nulls correctly
            try
            {
                var list = new List<string> { "a", null, "b", null, "c" };
                var filtered = list.WhereNotNull();
                ArgumentNullException.ThrowIfNull(filtered);

                var count = 0;
                foreach (var item in filtered)
                {
                    count++;
                }

                if (count != 3)
                {
                    problems.Add("WhereNotNull method did not filter nulls correctly.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"WhereNotNull method threw an exception: {ex.Message}");
            }

            // Validate Repeat behavior - should repeat the value
            try
            {
                var source = new[] { 1, 2 };
                var repeatResult = source.Repeat();
                ArgumentNullException.ThrowIfNull(repeatResult);

                var firstItem = repeatResult.FirstOrDefault();
                if (firstItem != 1)
                {
                    problems.Add("Repeat method did not return expected first item.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Repeat method threw an exception: {ex.Message}");
            }

            // Validate DistinctBy behavior - should deduplicate correctly
            try
            {
                var source = new[] { 1, 2, 2, 3, 3, 3 };
                var distinctResult = source.DistinctBy(x => x);
                ArgumentNullException.ThrowIfNull(distinctResult);

                var items = new List<int>();
                foreach (var item in distinctResult)
                {
                    items.Add(item);
                }

                if (items.Count != 3)
                {
                    problems.Add("DistinctBy method did not deduplicate correctly.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"DistinctBy method threw an exception: {ex.Message}");
            }

            // Validate MaxBy behavior - should find maximum
            try
            {
                var source = new[] { new { Value = 1 }, new { Value = 3 }, new { Value = 2 } };
                var maxResult = source.MaxBy(x => x.Value);
                ArgumentNullException.ThrowIfNull(maxResult);

                if (maxResult.Value != 3)
                {
                    problems.Add("MaxBy method did not return the maximum item.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"MaxBy method threw an exception: {ex.Message}");
            }

            // Validate MinBy behavior - should find minimum
            try
            {
                var source = new[] { new { Value = 1 }, new { Value = 3 }, new { Value = 2 } };
                var minResult = source.MinBy(x => x.Value);
                ArgumentNullException.ThrowIfNull(minResult);

                if (minResult.Value != 1)
                {
                    problems.Add("MinBy method did not return the minimum item.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"MinBy method threw an exception: {ex.Message}");
            }

            // Validate IndexOf behavior - should find correct index
            try
            {
                var source = new[] { "a", "b", "c" };
                var indexResult = source.IndexOf("b");

                if (indexResult != 1)
                {
                    problems.Add("IndexOf method did not return correct index.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"IndexOf method threw an exception: {ex.Message}");
            }

            // Validate FindIndex behavior - should find correct index
            try
            {
                var source = new[] { "a", "b", "c" };
                var findIndexResult = source.FindIndex(x => x == "b");

                if (findIndexResult != 1)
                {
                    problems.Add("FindIndex method did not return correct index.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"FindIndex method threw an exception: {ex.Message}");
            }

            // Validate SafeToDictionary behavior - should handle duplicates
            try
            {
                var source = new[] { "a", "b", "c" };
                var dictResult = source.SafeToDictionary(x => x, x => x);
                ArgumentNullException.ThrowIfNull(dictResult);

                if (dictResult.Count != 3)
                {
                    problems.Add("SafeToDictionary method did not create correct dictionary.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"SafeToDictionary method threw an exception: {ex.Message}");
            }

            // Validate GroupConsecutive behavior - should group consecutive items
            try
            {
                var source = new[] { 1, 1, 2, 2, 2, 3 };
                var groupResult = source.GroupConsecutive(x => x);
                ArgumentNullException.ThrowIfNull(groupResult);

                var count = 0;
                foreach (var group in groupResult)
                {
                    ArgumentNullException.ThrowIfNull(group);
                    count++;
                }

                if (count == 0)
                {
                    problems.Add("GroupConsecutive method returned empty enumerable.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"GroupConsecutive method threw an exception: {ex.Message}");
            }

            // Validate TakeWhile behavior - should take items while condition is true
            try
            {
                var source = new[] { 1, 2, 3, 4, 5 };
                var takeWhileResult = source.TakeWhile(x => x < 3);
                ArgumentNullException.ThrowIfNull(takeWhileResult);

                var items = new List<int>();
                foreach (var item in takeWhileResult)
                {
                    items.Add(item);
                }

                if (items.Count != 2 || items[0] != 1 || items[1] != 2)
                {
                    problems.Add("TakeWhile method did not take items correctly.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"TakeWhile method threw an exception: {ex.Message}");
            }

            // Validate Concat behavior - should concatenate sequences
            try
            {
                var concatResult = new[] { 1, 2 }.Concat(new[] { 3, 4 });
                ArgumentNullException.ThrowIfNull(concatResult);

                var items = new List<int>();
                foreach (var item in concatResult)
                {
                    items.Add(item);
                }

                if (items.Count != 4)
                {
                    problems.Add("Concat method did not concatenate correctly.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Concat method threw an exception: {ex.Message}");
            }

            // Validate AllEqual behavior - should check if all items are equal
            try
            {
                var allEqualTrue = new[] { 1, 1, 1 }.AllEqual();
                var allEqualFalse = new[] { 1, 2, 1 }.AllEqual();

                if (!allEqualTrue || allEqualFalse)
                {
                    problems.Add("AllEqual method did not return correct results.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"AllEqual method threw an exception: {ex.Message}");
            }

            // Validate Shuffle behavior - should return same count
            try
            {
                var source = new[] { 1, 2, 3 };
                var shuffleResult = source.Shuffle();
                ArgumentNullException.ThrowIfNull(shuffleResult);

                var items = new List<int>();
                foreach (var item in shuffleResult)
                {
                    items.Add(item);
                }

                if (items.Count != 3)
                {
                    problems.Add("Shuffle method did not preserve count.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Shuffle method threw an exception: {ex.Message}");
            }

            // Validate Pad behavior - should pad to minimum length
            try
            {
                var source = new[] { 1, 2 };
                var padResult = source.Pad(5, 0);
                ArgumentNullException.ThrowIfNull(padResult);

                var items = new List<int>();
                foreach (var item in padResult)
                {
                    items.Add(item);
                }

                if (items.Count != 5 || items[0] != 1 || items[1] != 2)
                {
                    problems.Add("Pad method did not pad correctly.");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Pad method threw an exception: {ex.Message}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the enumerable operations are valid.
        /// </summary>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if validation cannot be performed.</exception>
        public static bool IsValid()
        {
            return Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the enumerable operations are valid.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if validation cannot be performed.</exception>
        /// <exception cref="ArgumentException">Thrown if the operations are not valid, containing a list of problems.</exception>
        public static void EnsureValid()
        {
            var problems = Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"EnumerableExtensions operations are not valid. Problems:\n{string.Join("\n", problems)}",
                    nameof(problems));
            }
        }
    }
}