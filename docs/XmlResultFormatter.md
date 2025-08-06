# XmlResultFormatter

`XmlResultFormatter` is a result formatter that serializes processing outcomes, device information, and error details into XML strings. It implements the standard formatting contract used across the `gpu-image-processing` project, providing consistent XML representations for single results, result collections, job metadata, device capabilities, and error conditions.

## API

### XmlResultFormatter

```csharp
public XmlResultFormatter()
```

Default constructor. Creates a new instance of the formatter with standard XML serialization settings. No configuration parameters are required.

### GetFileExtension

```csharp
public string GetFileExtension()
```

Returns the file extension associated with XML-formatted output.

**Returns:** `".xml"` as a constant string.

### GetMimeType

```csharp
public string GetMimeType()
```

Returns the MIME type for XML content.

**Returns:** `"application/xml"` as a constant string.

### FormatResult

```csharp
public string FormatResult(object result)
```

Serializes a single processing result object into an XML string.

**Parameters:**
- `result` — The result object to format. Must not be null.

**Returns:** An XML string representation of the result.

**Throws:** `ArgumentNullException` if `result` is null. May throw `InvalidOperationException` or XML serialization exceptions if the object type is not serializable.

### FormatResults

```csharp
public string FormatResults(IEnumerable<object> results)
```

Serializes a collection of processing results into an XML string, typically wrapping them in a root element.

**Parameters:**
- `results` — An enumerable collection of result objects. Must not be null.

**Returns:** An XML string containing all results in a structured format.

**Throws:** `ArgumentNullException` if `results` is null. May throw XML serialization exceptions if any element in the collection is not serializable.

### FormatJob

```csharp
public string FormatJob(object job)
```

Serializes job metadata (such as job ID, status, timestamps, and parameters) into an XML string.

**Parameters:**
- `job` — The job object to format. Must not be null.

**Returns:** An XML string representation of the job.

**Throws:** `ArgumentNullException` if `job` is null. May throw XML serialization exceptions if the job object type is not serializable.

### FormatDevice

```csharp
public string FormatDevice(object device)
```

Serializes device information (such as GPU name, driver version, memory, and capabilities) into an XML string.

**Parameters:**
- `device` — The device object to format. Must not be null.

**Returns:** An XML string representation of the device.

**Throws:** `ArgumentNullException` if `device` is null. May throw XML serialization exceptions if the device object type is not serializable.

### FormatError

```csharp
public string FormatError(Exception error)
```

Serializes an exception into an XML error representation, typically including the exception type, message, and optionally stack trace information.

**Parameters:**
- `error` — The exception to format. Must not be null.

**Returns:** An XML string representation of the error.

**Throws:** `ArgumentNullException` if `error` is null. May throw XML serialization exceptions if the exception object cannot be serialized.

## Usage

### Example 1: Formatting a Single Processing Result

```csharp
var formatter = new XmlResultFormatter();
var processingResult = new
{
    ImageId = "img-001",
    FilterApplied = "GaussianBlur",
    ProcessingTimeMs = 42.5,
    OutputWidth = 1920,
    OutputHeight = 1080
};

string xml = formatter.FormatResult(processingResult);
Console.WriteLine(xml);

// Save to file
string extension = formatter.GetFileExtension();
File.WriteAllText($"result{extension}", xml);
```

### Example 2: Formatting a Batch of Results with Error Handling

```csharp
var formatter = new XmlResultFormatter();
var results = new List<object>
{
    new { ImageId = "img-001", Status = "Success" },
    new { ImageId = "img-002", Status = "Success" },
    new { ImageId = "img-003", Status = "Success" }
};

try
{
    string batchXml = formatter.FormatResults(results);
    Console.WriteLine(batchXml);

    // Verify MIME type for HTTP response
    string mimeType = formatter.GetMimeType();
    Console.WriteLine($"Content-Type: {mimeType}");
}
catch (Exception ex)
{
    string errorXml = formatter.FormatError(ex);
    Console.Error.WriteLine(errorXml);
}
```

## Notes

- **Null handling:** All `Format*` methods throw `ArgumentNullException` when passed a null argument. Callers should validate inputs before invoking these methods.
- **Serialization constraints:** Objects passed to formatting methods must be serializable by the underlying XML serializer. Types that contain circular references, non-public properties without appropriate attributes, or unsupported data types may cause `InvalidOperationException` or other serialization-specific exceptions at runtime.
- **Thread safety:** `XmlResultFormatter` holds no mutable instance state beyond its serialization configuration. Concurrent calls to formatting methods on the same instance are safe, provided the objects being serialized are not mutated during formatting.
- **Collection formatting:** `FormatResults` wraps multiple elements in a root container element. The exact structure depends on the internal serialization conventions of the project. An empty collection produces valid XML with no child elements under the root.
- **Error formatting:** `FormatError` serializes the exception object itself, not merely its message. The depth of detail (stack trace, inner exceptions) depends on the exception type's serialization characteristics and the formatter's configuration.
