# IResultFormatter

`IResultFormatter` is the central abstraction for rendering GPU image processing results into structured output formats. It defines a factory-based interface that discovers and instantiates format-specific renderers (such as JSON, HTML, Markdown, XML, or plain text) based on format identifiers or file extensions. The type itself exposes only static factory and introspection members—concrete formatters implement the underlying contract and are returned by these methods.

## API

### CreateFormatter

```csharp
public static IResultFormatter CreateFormatter(string format)
```

Creates and returns an `IResultFormatter` instance for the specified format identifier.

**Parameters:**
- `format` (`string`): A case-insensitive format name (e.g., `"json"`, `"html"`, `"markdown"`, `"xml"`, `"text"`).

**Returns:**
- `IResultFormatter`: A concrete formatter instance that implements the `IResultFormatter` contract for the requested format.

**Throws:**
- `ArgumentNullException`: If `format` is `null`.
- `NotSupportedException`: If the format is not recognised by any registered formatter.

---

### GetSupportedFormats

```csharp
public static List<string> GetSupportedFormats()
```

Enumerates all format identifiers for which a formatter is available.

**Returns:**
- `List<string>`: A list of lowercased format names (e.g., `["json", "html", "markdown", "xml", "text"]`). The list is a new mutable copy; modifying it does not affect internal registrations.

**Throws:**
- No exceptions documented.

---

### IsFormatSupported

```csharp
public static bool IsFormatSupported(string format)
```

Determines whether a given format identifier has a registered formatter.

**Parameters:**
- `format` (`string`): A case-insensitive format name to check.

**Returns:**
- `bool`: `true` if a formatter exists for the format; otherwise `false`.

**Throws:**
- `ArgumentNullException`: If `format` is `null`.

---

### CreateFromExtension

```csharp
public static IResultFormatter CreateFromExtension(string fileExtension)
```

Resolves and instantiates an `IResultFormatter` by mapping a file extension to its corresponding format.

**Parameters:**
- `fileExtension` (`string`): A file extension with or without a leading dot (e.g., `".json"`, `"json"`, `".html"`). Matching is case-insensitive.

**Returns:**
- `IResultFormatter`: A concrete formatter instance suitable for the format associated with the extension.

**Throws:**
- `ArgumentNullException`: If `fileExtension` is `null`.
- `NotSupportedException`: If the extension does not map to any supported format.

## Usage

### Example 1: Selecting a formatter by format name and rendering a result

```csharp
using GpuImageProcessing.Formatters;

// Assume 'result' is an object containing GPU processing output.
var result = new { KernelName = "Sobel", ExecutionTimeMs = 12.4, OutputWidth = 1920 };

if (IResultFormatter.IsFormatSupported("json"))
{
    IResultFormatter formatter = IResultFormatter.CreateFormatter("json");
    string output = formatter.Format(result);
    Console.WriteLine(output);
}
else
{
    Console.WriteLine("JSON formatter is not available.");
}
```

### Example 2: Deriving a formatter from an output file path

```csharp
using GpuImageProcessing.Formatters;

string outputPath = "/results/edge_detection.html";
string extension = Path.GetExtension(outputPath); // ".html"

try
{
    IResultFormatter formatter = IResultFormatter.CreateFromExtension(extension);
    string rendered = formatter.Format(processingResult);
    File.WriteAllText(outputPath, rendered);
}
catch (NotSupportedException)
{
    // Fall back to a known format.
    IResultFormatter fallback = IResultFormatter.CreateFormatter("text");
    File.WriteAllText(outputPath, fallback.Format(processingResult));
}
```

## Notes

- **Format resolution is case-insensitive.** Passing `"JSON"`, `"Json"`, or `"json"` to `CreateFormatter` or `IsFormatSupported` yields identical behaviour.
- **Extension parsing tolerates missing leading dots.** `CreateFromExtension` normalises input so that both `"html"` and `".html"` resolve to the HTML formatter.
- **The list returned by `GetSupportedFormats` is a snapshot.** Adding or removing items from the returned list has no effect on the internal format registry.
- **Thread safety.** All four static members are safe to call concurrently from multiple threads. The underlying formatter instances returned by `CreateFormatter` and `CreateFromExtension` may or may not be thread-safe depending on the concrete implementation; consult the documentation for the specific formatter (e.g., `JsonResultFormatter`, `HtmlResultFormatter`) if shared instances are used across threads.
- **Extensibility.** The set of supported formats is determined by formatter implementations registered at startup. Calling `IsFormatSupported` before `CreateFormatter` avoids `NotSupportedException` in contexts where the available formats may vary.
- **Disposal.** The returned `IResultFormatter` instances do not own unmanaged resources and do not require explicit disposal unless a particular concrete formatter documents otherwise.
