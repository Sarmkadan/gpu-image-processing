# EnumerableExtensionsBenchmarksExtensions

`EnumerableExtensionsBenchmarksExtensions` is a static utility class that provides benchmark-oriented operations for evaluating the performance of LINQ-style extension methods on sequences. It exposes pre-configured, fixed-size workloads that operate on collections of 1000 elements, enabling consistent and repeatable measurement of common patterns such as filtering, projection, reversal, and ordering.

## API

### Filter_1000ByPredicate

```csharp
public static int Filter_1000ByPredicate
```

A static read-only field that holds a delegate representing a benchmark operation. The delegate filters a sequence of 1000 integers using a predicate, retaining only elements that satisfy the condition, and materializes the result. The return value is the count of elements that passed the filter.

- **Return Value**: An `int` representing the number of elements that satisfied the predicate.
- **Exceptions**: No exceptions are thrown under normal operation. The underlying delegate may throw if the sequence source or predicate is modified externally, but this is not part of the standard benchmark path.

### Select_1000ToSum

```csharp
public static int Select_1000ToSum
```

A static read-only field that holds a delegate representing a benchmark operation. The delegate projects each element of a 1000-integer sequence using a transformation function and then computes the sum of the projected values. The return value is the total sum.

- **Return Value**: An `int` representing the sum of the projected elements.
- **Exceptions**: No exceptions are thrown under normal operation. Arithmetic overflow is possible if the sum exceeds `Int32.MaxValue`, depending on the projection logic used in the benchmark.

### Reverse_1000First

```csharp
public static int Reverse_1000First
```

A static read-only field that holds a delegate representing a benchmark operation. The delegate reverses a sequence of 1000 integers and retrieves the first element of the reversed sequence. The return value is that first element.

- **Return Value**: An `int` representing the first element after reversal.
- **Exceptions**: No exceptions are thrown under normal operation. If the source sequence were empty (not the case for the fixed 1000-element workload), an `InvalidOperationException` would be thrown by `First()`.

### OrderBy_1000First

```csharp
public static int OrderBy_1000First
```

A static read-only field that holds a delegate representing a benchmark operation. The delegate sorts a sequence of 1000 integers in ascending order using the default comparer and retrieves the first element of the sorted sequence. The return value is the smallest element.

- **Return Value**: An `int` representing the smallest element in the sequence after ordering.
- **Exceptions**: No exceptions are thrown under normal operation. As with `Reverse_1000First`, an empty sequence would cause `First()` to throw, but the fixed workload guarantees 1000 elements.

## Usage

### Example 1: Running a single benchmark operation inline

```csharp
// Invoke the Filter_1000ByPredicate delegate directly to measure filtering performance.
int filteredCount = EnumerableExtensionsBenchmarksExtensions.Filter_1000ByPredicate();
Console.WriteLine($"Elements passing filter: {filteredCount}");
```

### Example 2: Integrating with a benchmarking harness

```csharp
// Using BenchmarkDotNet or a similar framework, reference the static delegates as benchmark targets.
[Benchmark]
public int BenchmarkFilter() => EnumerableExtensionsBenchmarksExtensions.Filter_1000ByPredicate();

[Benchmark]
public int BenchmarkSelectSum() => EnumerableExtensionsBenchmarksExtensions.Select_1000ToSum();

[Benchmark]
public int BenchmarkReverseFirst() => EnumerableExtensionsBenchmarksExtensions.Reverse_1000First();

[Benchmark]
public int BenchmarkOrderByFirst() => EnumerableExtensionsBenchmarksExtensions.OrderBy_1000First();
```

## Notes

- All members operate on fixed-size sequences of exactly 1000 elements. The input data is predetermined and immutable for the duration of each delegate invocation, ensuring deterministic and repeatable benchmark results.
- The delegates are stateless and do not modify shared state. They are safe to invoke concurrently from multiple threads without external synchronization, provided the underlying sequence data is not mutated externally.
- Edge cases such as empty sequences or null sources are not exercised by these members; they are designed exclusively for the 1000-element workload. Any deviation from this (e.g., replacing the internal source with an empty collection) would result in exceptions from enumeration-terminating operations like `First()`.
- The return values are meaningful for correctness verification rather than purely diagnostic. For example, `OrderBy_1000First` returns the minimum value, which can be asserted in tests to confirm that sorting behaved as expected.
- Arithmetic overflow in `Select_1000ToSum` is a theoretical possibility depending on the projection function and input values chosen for the benchmark. The default configuration is assumed to avoid overflow, but custom variations should consider this risk.
