# JsonResultFormatterExtensions

Static extension methods that convert domain objects related to GPU image processing into JSON strings, optionally enriching the output with statistics, summaries, progress information, device details, or error context, and providing helpers to write the formatted JSON directly to a file.

## API

### FormatResultWithStatistics
**Purpose** – Serializes a processing result together with its associated statistics into a single JSON string.  
**Parameters**  
- `result`: The result object to format (e.g., an instance containing output image metadata).  
- `statistics`: An object holding quantitative data such as execution time, memory usage, or throughput.  
**Return value** – A JSON‑encoded string representing both the result and the statistics.  
**Exceptions** –  
- `ArgumentNullException` if `result` or `statistics` is `null`.  
- `JsonSerializationException` if the objects cannot be serialized (e.g., due to circular references).

### FormatResultsWithSummary
**Purpose** – Serializes a collection of results and appends a summary object (e.g., totals, averages) to the JSON output.  
**Parameters**  
- `results`: An enumerable of result objects to include.  
- `summary`: An object summarizing the collection (count, success rate, etc.).  
**Return value** – A JSON string containing the array of results followed by the summary section.  
**Exceptions** –  
- `ArgumentNullException` if `results` or `summary` is `null`.  
- `InvalidOperationException` if the enumeration is modified during serialization.  
- `JsonSerializationException` on serialization failure.

### FormatJobWithProgress
**Purpose** – Produces a JSON representation of a job entity enriched with real‑time progress information.  
**Parameters**  
- `job`: The job object describing the task (identifier, type, target parameters).  
- `progress`: A value between 0 and 1 (or a custom progress struct) indicating completion.  
**Return value** – A JSON string containing the job details and a `progress` field.  
**Exceptions** –  
- `ArgumentNullException` if `job` is `null`.  
- `ArgumentOutOfRangeException` if `progress` is outside the expected range.  
- `JsonSerializationException` if serialization fails.

### FormatDeviceWithDetails
**Purpose** – Formats a device descriptor along with detailed capability information into JSON.  
**Parameters**  
- `device`: Basic device information (name, ID, type).  
- `details`: An object with extended specs such as compute units, memory bandwidth, supported formats.  
**Return value** – A JSON string merging the basic device data with the details block.  
**Exceptions** –  
- `ArgumentNullException` if either parameter is `null`.  
- `JsonSerializationException` on serialization error.

### FormatErrorWithContext
**Purpose** – Creates a JSON payload that captures an error together with contextual data helpful for diagnostics (e.g., input parameters, timestamps).  
**Parameters**  
- `error`: The exception or error object to report.  
- `context`: Arbitrary contextual data (request ID, input image properties, etc.).  
**Return value** – A JSON string containing `error` and `context` sections.  
**Exceptions** –  
- `ArgumentNullException` if `error` is `null`.  
- `JsonSerializationException` if either object cannot be serialized.

### FormatResultToFile
**Purpose** – Serializes a result object to JSON and writes the output to a specified file path.  
**Parameters**  
- `result`: The result object to serialize.  
- `filePath`: Full or relative path where the JSON file will be created or overwritten.  
- `append` (optional, default `false`): If `true`, the JSON is appended to the file; otherwise the file is truncated.  
**Return value** – The full path of the file that was written.  
**Exceptions** –  
- `ArgumentNullException` if `result` or `filePath` is `null`.  
- `IOException` if the file cannot be accessed, created, or written to.  
- `JsonSerializationException` if serialization fails before writing.

### FormatResultsToFile
**Purpose** – Serializes a collection of result objects to a JSON array and writes it to a file.  
**Parameters**  
- `results`: Enumerable of result objects to serialize.  
- `filePath`: Destination file path.  
- `append` (optional, default `false`): Controls whether to append to an existing file.  
**Return value** – The path of the file containing the JSON array.  
**Exceptions** –  
- `ArgumentNullException` if `results` or `filePath` is `null`.  
- `IOException` on file‑system errors.  
- `JsonSerializationException` if any element in the collection cannot be serialized.  
- `InvalidOperationException` if the enumeration is modified during iteration.

## Usage

```csharp
using GpuImageProcessing.Formatting;
using System.IO;

// Example 1: Format a single result with its statistics and save to disk
var result = await imageProcessor.ProcessAsync(inputImage);
var stats  = await imageProcessor.GetStatisticsAsync();

string json = JsonResultFormatterExtensions.FormatResultWithStatistics(result, stats);
string outPath = JsonResultFormatterExtensions.FormatResultToFile(json, @"C:\temp\result.json");
// outPath now points to the file containing the formatted JSON
```

```csharp
using GpuImageProcessing.Formatting;
using System.Collections.Generic;

// Example 2: Format a batch of results, add a summary, and write to a file
List<ProcessingResult> batchResults = GetBatchResults();
var summary = new { Total = batchResults.Count, Succeeded = batchResults.Count(r => r.Success) };

string json = JsonResultFormatterExtensions.FormatResultsWithSummary(batchResults, summary);
string file = JsonResultFormatterExtensions.FormatResultsToFile(batchResults, @"C:\temp\batchResults.json", append: false);
// file contains a JSON array of results followed by the summary object
```

## Notes

- All methods are **static** and do not rely on mutable shared state; therefore they are safe to invoke concurrently from multiple threads, provided that the arguments themselves are not modified during the call.  
- Passing `null` for any required argument will always result in an `ArgumentNullException`.  
- The methods depend on the underlying JSON serializer (typically `System.Text.Json.JsonSerializer`). Serialization exceptions propagate as `JsonSerializationException` and may include inner exceptions detailing the cause (e.g., unsupported types, circular references).  
- When using the `*ToFile` overloads, the caller should ensure that the directory for `filePath` exists; otherwise an `IOException` will be thrown.  
- The `append` flag in the file‑writing methods does **not** produce valid JSON when appending to an existing file that already contains a complete JSON document; it is intended for logging scenarios where each line is a separate JSON object. For a single valid JSON document, use `append: false` (the default).  
- Large objects may cause significant memory allocation during serialization; consider streaming alternatives if payload sizes exceed a few megabytes.  
- These helpers are deliberately focused on formatting; they do not perform any validation beyond null checks. Validation of the domain objects should be performed upstream if required.
