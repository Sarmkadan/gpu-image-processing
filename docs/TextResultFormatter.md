# TextResultFormatter

Provides simple text‑based formatting helpers for the results, jobs, devices, and errors produced by the GPU image processing pipeline. The formatter returns plain‑text strings suitable for logging, console output, or file storage.

## API

### `GetFileExtension`
- **Purpose:** Returns the file extension used when persisting text‑formatted output.
- **Parameters:** None.
- **Return value:** A string such as `".txt"`.
- **Exceptions:** None.

### `GetMimeType`
- **Purpose:** Returns the MIME type associated with the text format.
- **Parameters:** None.
- **Return value:** A string such as `"text/plain"`.
- **Exceptions:** None.

### `FormatResult`
- **Purpose:** Formats a single processing result into a readable string.
- **Parameters:** None (the formatter uses internally stored result data).
- **Return value:** A string representation of the result.
- **Exceptions:** Throws `InvalidOperationException` if the underlying result data has not been set.

### `FormatResults`
- **Purpose:** Formats a collection of processing results into a single string, typically with each result on its own line.
- **Parameters:** None (operates on the internally stored collection).
- **Return value:** A string containing the formatted results.
- **Exceptions:** Throws `InvalidOperationException` if the result collection has not been initialized.

### `FormatJob`
- **Purpose:** Produces a textual description of a processing job (e.g., job ID, parameters, status).
- **Parameters:** None.
- **Return value:** A string describing the job.
- **Exceptions:** Throws `InvalidOperationException` if job information is unavailable.

### `FormatDevice`
- **Purpose:** Formats details about the compute device used (e.g., name, type, memory).
- **Parameters:** None.
- **Return value:** A string describing the device.
- **Exceptions:** Throws `InvalidOperationException` if device information has not been provided.

### `FormatError`
- **Purpose:** Formats an error condition into a user‑friendly message.
- **Parameters:** None.
- **Return value:** A string containing the error details.
- **Exceptions:** Throws `InvalidOperationException` if no error data is present.

## Usage

```csharp
using GpuImageProcessing.Formatting;

// Assume the formatter has been populated with result/job/device data elsewhere.
var formatter = new TextResultFormatter();

// Get the file extension and MIME type for saving output.
string ext = formatter.GetFileExtension;   // => ".txt"
string mime = formatter.GetMimeType;       // => "text/plain"

// Format a single result for logging.
string single = formatter.FormatResult;
Console.WriteLine(single);

// Format all results for a batch report.
string all = formatter.FormatResults;
File.WriteAllText("results.txt", all);
```

```csharp
using GpuImageProcessing.Formatting;

// Example showing error and device formatting.
var formatter = new TextResultFormatter();
// ... (formatter receives error/device info from the pipeline)

string deviceInfo = formatter.FormatDevice;
string errorInfo  = formatter.FormatError;

Console.WriteLine($"Device: {deviceInfo}");
if (!string.IsNullOrEmpty(errorInfo))
{
    Console.Error.WriteLine($"Error: {errorInfo}");
}
```

## Notes

- The formatter does not store any mutable state beyond the data supplied by the calling code; therefore instances are thread‑safe for concurrent read‑only use after initialization.
- If the required data (result, job, device, or error) has not been supplied before calling a formatting method, the method throws `InvalidOperationException` to indicate misuse.
- Passing `null` to any internal setters (not shown in the public API) will result in `ArgumentNullException`; the public get‑only members themselves never accept parameters and thus cannot throw argument‑related exceptions.
- The returned strings are culture‑invariant; no localization is applied. If culture‑specific formatting is needed, the caller should apply it to the returned values.
