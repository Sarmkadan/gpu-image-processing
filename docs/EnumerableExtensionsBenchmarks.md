# EnumerableExtensionsBenchmarks

A benchmarking class for measuring the performance of common LINQ operations on `IEnumerable<T>` in C#, particularly focusing on batching, shuffling, and dictionary conversion scenarios. This class is designed to evaluate the overhead of these operations under different input sizes and configurations.

## API

### `public void Setup()`
Initializes the benchmark environment by generating required test data. This method is called once before each benchmark run to ensure consistent and isolated measurements. No parameters are accepted, and no return value is provided. This method does not throw exceptions under normal operation.

---

### `public IEnumerable<int> Shuffle_32Items()`
Generates a shuffled sequence of 32 integers. The sequence is produced by applying a deterministic shuffle algorithm to a base sequence of integers from 0 to 31. No parameters are accepted. Returns an `IEnumerable<int>` representing the shuffled sequence. This method does not throw exceptions.

---

### `public IEnumerable<int> Shuffle_1024Items()`
Generates a shuffled sequence of 1024 integers. Similar to `Shuffle_32Items`, but operates on a larger input size to evaluate performance under increased data volume. No parameters are accepted. Returns an `IEnumerable<int>` representing the shuffled sequence. This method does not throw exceptions.

---
### `public int Batch_1000By32()`
Partitions a sequence of 1000 integers into batches of 32 elements each. The last batch may contain fewer than 32 elements if the total count is not evenly divisible. No parameters are accepted. Returns the total number of batches created. This method does not throw exceptions.

---
### `public int Batch_1000By8()`
Partitions a sequence of 1000 integers into batches of 8 elements each. Similar to `Batch_1000By32`, but uses a smaller batch size to assess performance characteristics with finer granularity. No parameters are accepted. Returns the total number of batches created. This method does not throw exceptions.

---
### `public int DistinctBy_1000Strings()`
Applies a `DistinctBy` operation on a sequence of 1000 strings, using a key selector that extracts the first character of each string. Measures the efficiency of deduplication based on a computed key. No parameters are accepted. Returns the count of distinct elements after applying the operation. This method does not throw exceptions.

---
### `public Dictionary<int, int> SafeToDictionary_1000Items()`
Converts a sequence of 1000 key-value pairs into a `Dictionary<int, int>`, handling potential key collisions by overwriting existing entries with the last occurrence. No parameters are accepted. Returns a populated `Dictionary<int, int>` containing all entries from the source sequence. This method does not throw exceptions under normal operation.

## Usage
