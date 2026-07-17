# XmlResultFormatterExtensions

The `XmlResultFormatterExtensions` class provides a set of static formatting methods for converting processing results, job details, device information, and envelopes into XML strings. It also exposes instance properties that aggregate execution statistics (counts and durations) for a batch of operations. This class is intended to simplify the generation of structured XML output from GPU image processing pipelines.

## API

### Static Methods

#### `FormatResultsWithStatistics`

```csharp
public static string FormatResultsWithStatistics(IEnumerable<ProcessingResult> results)
```

Formats a collection of `ProcessingResult` objects into an XML string that includes both individual result entries and summary statistics (total, successful, failed counts and average duration).

- **Parameters**  
  `results` – A sequence of `ProcessingResult` instances to format. Must not be `null`.

- **Returns**  
  A `string` containing the XML representation of the results and statistics.

- **Exceptions**  
  `ArgumentNullException` – Thrown if `results` is `null`.

---

#### `FormatJobWithDetails`

```csharp
public static string FormatJobWithDetails(Job job)
```

Formats a `Job` object into an XML string that includes the job’s metadata and all associated details (e.g., steps, parameters, timestamps).

- **Parameters**  
  `job` – The `Job` instance to format. Must not be `null`.

- **Returns**  
  A `string` containing the XML representation of the job with its details.

- **Exceptions**  
  `ArgumentNullException` – Thrown if `job` is `null`.

---

#### `FormatDeviceWithExtensions`

```csharp
public static string FormatDeviceWithExtensions(Device device)
```

Formats a `Device` object into an XML string that includes the device’s core properties and any extension data (e.g., vendor-specific capabilities).

- **Parameters**  
  `device` – The `Device` instance to format. Must not be `null`.

- **Returns**  
  A `string` containing the XML representation of the device with its extensions.

- **Exceptions**  
  `ArgumentNullException` – Thrown if `device` is `null`.

---

#### `WrapInEnvelope`

```csharp
public static string WrapInEnvelope(string innerXml)
```

Wraps a raw XML string inside a standard envelope element (e.g., `<Envelope><Body>...</Body></Envelope>`).

- **Parameters**  
  `innerXml` – A well-formed XML string to place inside the envelope. Must not be `null` or empty.

- **Returns**  
  A `string` containing the envelope XML.

- **Exceptions**  
  `ArgumentNullException` – Thrown if `innerXml` is `null`.  
  `ArgumentException` – Thrown if `innerXml` is empty or consists only of white space.

### Instance Properties

#### `TotalCount`

```csharp
public int TotalCount { get; }
```

Gets the total number of results processed. This value is typically set after a batch of results has been aggregated.

#### `SuccessfulCount`

```csharp
public int SuccessfulCount { get; }
```

Gets the number of results that completed successfully.

#### `FailedCount`

```csharp
public int FailedCount { get; }
```

Gets the number of results that failed.

#### `TotalDurationMs`

```csharp
public long TotalDurationMs { get; }
```

Gets the cumulative duration (in milliseconds) of all processed results.

#### `AverageDurationMs`

```csharp
public double AverageDurationMs { get; }
```

Gets the average duration (in milliseconds) of the processed results. This is computed as `TotalDurationMs / TotalCount` when `TotalCount` is greater than zero; otherwise it returns `0.0`.

## Usage

### Example 1: Formatting results with statistics and using instance properties

```csharp
var results = new List<ProcessingResult>
{
    new ProcessingResult { Success = true, DurationMs = 120 },
    new ProcessingResult { Success = true, DurationMs = 95 },
    new ProcessingResult { Success = false, DurationMs = 0 }
};

// Format results as XML with statistics
string xml = XmlResultFormatterExtensions.FormatResultsWithStatistics(results);
Console.WriteLine(xml);

// Create an instance to access aggregated statistics
var stats = new XmlResultFormatterExtensions();
// Assume the instance is populated by some internal aggregation (not shown here)
// For demonstration, manually set the properties (in practice they would be set by the class)
stats.TotalCount = 3;
stats.SuccessfulCount = 2;
stats.FailedCount = 1;
stats.TotalDurationMs = 215;
stats.AverageDurationMs = 71.67;

Console.WriteLine($"Total: {stats.TotalCount}, Avg Duration: {stats.AverageDurationMs} ms");
```

### Example 2: Formatting a job and wrapping in an envelope

```csharp
var job = new Job
{
    Id = "job-001",
    CreatedAt = DateTime.UtcNow,
    Steps = new List<JobStep>
    {
        new JobStep { Name = "Load", Status = "Completed" },
        new JobStep { Name = "Process", Status = "Running" }
    }
};

string jobXml = XmlResultFormatterExtensions.FormatJobWithDetails(job);
string envelopeXml = XmlResultFormatterExtensions.WrapInEnvelope(jobXml);

Console.WriteLine(envelopeXml);
```

## Notes

- **Null and empty arguments** – All static methods throw `ArgumentNullException` when a required parameter is `null`. `WrapInEnvelope` additionally throws `ArgumentException` for empty or white-space-only input.
- **Zero-count edge case** – When `TotalCount` is zero, `AverageDurationMs` returns `0.0` to avoid division by zero. The other properties (`SuccessfulCount`, `FailedCount`, `TotalDurationMs`) will also be zero in that scenario.
- **Thread safety** – The static methods are thread-safe as they do not modify any shared state. The instance properties are not guaranteed to be thread-safe; concurrent reads and writes to the same `XmlResultFormatterExtensions` instance should be synchronized by the caller.
- **Property population** – The instance properties (`TotalCount`, `SuccessfulCount`, etc.) are not automatically updated by the static formatting methods. They must be set explicitly, typically after aggregating results from a batch operation. The class does not provide a built-in aggregation method; the properties serve as a container for statistics computed elsewhere.
