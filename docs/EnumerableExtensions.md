# EnumerableExtensions

The `EnumerableExtensions` class provides a comprehensive set of extension methods for `IEnumerable<T>` designed to augment standard LINQ functionality with specialized operations required for high-throughput GPU image processing pipelines. These utilities facilitate efficient data batching, null filtering, consecutive grouping, and safe collection conversions, ensuring robust handling of large datasets while minimizing memory allocations and maintaining deferred execution where applicable.

## API

### `Batch<T>`
Splits a sequence into chunks of a specified size.
*   **Parameters**: `source` (the sequence to split), `size` (the maximum size of each batch).
*   **Returns**: `IEnumerable<IEnumerable<T>>` representing the sequence of batches.
*   **Throws**: `ArgumentException` if `size` is less than or equal to zero; `ArgumentNullException` if `source` is null.

### `WhereNotNull<T>`
Filters a sequence to exclude null values, primarily useful for sequences of nullable reference types or nullable value types.
*   **Parameters**: `source` (the sequence to filter).
*   **Returns**: `IEnumerable<T>` containing only non-null elements.
*   **Throws**: `ArgumentNullException` if `source` is null.

### `Repeat<T>`
Generates a sequence by repeating a specific value a defined number of times.
*   **Parameters**: `value` (the value to repeat), `count` (the number of times to repeat).
*   **Returns**: `IEnumerable<T>` containing the repeated values.
*   **Throws**: `ArgumentOutOfRangeException` if `count` is negative.

### `DistinctBy<T, K>`
Returns distinct elements from a sequence based on a specified key selector function.
*   **Parameters**: `source` (the sequence), `keySelector` (function to extract the key for comparison).
*   **Returns**: `IEnumerable<T>` containing distinct elements based on the key.
*   **Throws**: `ArgumentNullException` if `source` or `keySelector` is null.

### `MaxBy<T, K>`
Returns the element in the sequence that yields the maximum value according to a specified key selector.
*   **Parameters**: `source` (the sequence), `keySelector` (function to extract the comparable key).
*   **Returns**: `T` representing the element with the maximum key value. Returns default(T) if the sequence is empty.
*   **Throws**: `ArgumentNullException` if `source` or `keySelector` is null.

### `MinBy<T, K>`
Returns the element in the sequence that yields the minimum value according to a specified key selector.
*   **Parameters**: `source` (the sequence), `keySelector` (function to extract the comparable key).
*   **Returns**: `T` representing the element with the minimum key value. Returns default(T) if the sequence is empty.
*   **Throws**: `ArgumentNullException` if `source` or `keySelector` is null.

### `IndexOf<T>`
Determines the index of the first occurrence of a specific value in the sequence.
*   **Parameters**: `source` (the sequence), `item` (the value to locate).
*   **Returns**: `int` representing the zero-based index if found; otherwise, -1.
*   **Throws**: `ArgumentNullException` if `source` is null.

### `FindIndex<T>`
Determines the index of the first element that matches a specified predicate.
*   **Parameters**: `source` (the sequence), `predicate` (the condition to test each element).
*   **Returns**: `int` representing the zero-based index of the first match; otherwise, -1.
*   **Throws**: `ArgumentNullException` if `source` or `predicate` is null.

### `SafeToDictionary<T, K, V>`
Converts a sequence to a `Dictionary<K, V>`, handling duplicate keys by overwriting previous entries rather than throwing an exception.
*   **Parameters**: `source` (the sequence), `keySelector` (function to extract the key), `valueSelector` (function to extract the value).
*   **Returns**: `Dictionary<K, V>` containing the mapped elements.
*   **Throws**: `ArgumentNullException` if `source`, `keySelector`, or `valueSelector` is null.

### `GroupConsecutive<T, K>`
Groups consecutive elements in a sequence that share the same key, preserving order and adjacency.
*   **Parameters**: `source` (the sequence), `keySelector` (function to extract the key for grouping).
*   **Returns**: `IEnumerable<IEnumerable<T>>` where each inner sequence contains consecutive elements with identical keys.
*   **Throws**: `ArgumentNullException` if `source` or `keySelector` is null.

### `TakeWhile<T>`
Returns elements from the sequence as long as a specified condition is true, including the element that fails the condition if an overload with `inclusive` flag is used (otherwise stops before). *Note: Standard behavior stops before the first failure.*
*   **Parameters**: `source` (the sequence), `predicate` (the condition to test).
*   **Returns**: `IEnumerable<T>` containing the elements up to the first failure.
*   **Throws**: `ArgumentNullException` if `source` or `predicate` is null.

### `ForEach<T>`
Executes a specified action on each element of the sequence while returning the original sequence to allow chaining.
*   **Parameters**: `source` (the sequence), `action` (the delegate to execute on each element).
*   **Returns**: `IEnumerable<T>` the original source sequence.
*   **Throws**: `ArgumentNullException` if `source` or `action` is null.

### `Concat<T>`
Concatenates multiple sequences into a single sequence.
*   **Parameters**: `first` (the first sequence), `others` (params array of additional sequences).
*   **Returns**: `IEnumerable<T>` containing all elements from the input sequences.
*   **Throws**: `ArgumentNullException` if any input sequence is null.

### `AllEqual<T>`
Determines whether all elements in the sequence are equal to each other using the default equality comparer.
*   **Parameters**: `source` (the sequence).
*   **Returns**: `bool` true if all elements are equal or the sequence is empty/contains one element; otherwise false.
*   **Throws**: `ArgumentNullException` if `source` is null.

### `Shuffle<T>`
Returns a sequence with the elements randomly reordered.
*   **Parameters**: `source` (the sequence to shuffle).
*   **Returns**: `IEnumerable<T>` with elements in random order.
*   **Throws**: `ArgumentNullException` if `source` is null.

### `Pad<T>`
Pads a sequence to a specified length by appending a default or specified value if the sequence is shorter than the target length.
*   **Parameters**: `source` (the sequence), `width` (the target length), `paddingValue` (optional value to use for padding).
*   **Returns**: `IEnumerable<T>` with at least `width` elements.
*   **Throws**: `ArgumentNullException` if `source` is null; `ArgumentOutOfRangeException` if `width` is negative.

## Usage

### Example 1: Batch Processing and Safe Conversion
This example demonstrates splitting a large list of image file paths into batches for parallel GPU processing and safely converting the results into a dictionary, handling potential duplicate keys gracefully.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class ImageProcessor
{
    public void ProcessImages(List<string> filePaths)
    {
        // Split paths into batches of 32 for GPU kernel dispatch
        var batches = filePaths.Batch(32);

        var results = new List<(string Path, int ResultCode)>();

        foreach (var batch in batches)
        {
            // Simulate processing
            foreach (var path in batch)
            {
                results.Add((path, new Random().Next(0, 5)));
            }
        }

        // Convert to dictionary, keeping the last result if duplicate paths exist
        Dictionary<string, int> resultMap = results.SafeToDictionary(
            x => x.Path,
            x => x.ResultCode
        );

        Console.WriteLine($"Processed {resultMap.Count} unique images.");
    }
}
```

### Example 2: Consecutive Grouping and Filtering
This example illustrates grouping consecutive pixels with the same intensity value (run-length encoding preparation) and filtering out invalid null entries from a nullable collection.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class PixelAnalyzer
{
    public void AnalyzeRunLength(int?[] pixelData)
    {
        // Remove null artifacts from sensor data
        var validPixels = pixelData.WhereNotNull();

        // Group consecutive pixels with the same value
        var runs = validPixels.GroupConsecutive(x => x);

        foreach (var run in runs)
        {
            int value = run.First();
            int count = run.Count();
            
            if (count > 1)
            {
                Console.WriteLine($"Value {value} repeats {count} times consecutively.");
            }
        }
        
        // Check if all valid pixels are identical
        bool isUniform = validPixels.AllEqual();
        Console.WriteLine($"Image uniformity: {isUniform}");
    }
}
```

## Notes

*   **Deferred Execution**: Most methods returning `IEnumerable<T>` (such as `Batch`, `WhereNotNull`, `GroupConsecutive`, and `Shuffle`) utilize deferred execution. The source sequence is not enumerated until the returned collection is iterated. Be aware that if the underlying source changes between the call and the iteration, the results may reflect those changes.
*   **Buffering Requirements**: Methods like `Batch`, `GroupConsecutive`, and `Shuffle` require internal buffering. `Shuffle` specifically must consume the entire source sequence into memory before yielding any results to perform the randomization. `Batch` and `GroupConsecutive` buffer only the current group being processed.
*   **Thread Safety**: The extension methods themselves are stateless and thread-safe regarding their internal logic. However, they are not thread-safe with respect to the `source` collection. If the source collection is modified concurrently while these enumerators are running, undefined behavior or exceptions may occur.
*   **Empty Sequences**: `MaxBy` and `MinBy` return `default(T)` for empty sequences rather than throwing an exception. `AllEqual` returns `true` for empty or single-element sequences. `IndexOf` and `FindIndex` return `-1` if no match is found.
*   **Duplicate Keys**: Unlike standard LINQ `ToDictionary`, `SafeToDictionary` does not throw on duplicate keys; it overwrites the existing entry with the new value. This is intentional for scenarios where the last occurrence in a stream is considered the authoritative state.
