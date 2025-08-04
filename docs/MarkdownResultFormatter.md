# MarkdownResultFormatter

A utility class for formatting GPU image processing results, errors, and device information into Markdown-formatted strings. It provides specialized methods for different types of outputs, ensuring consistent formatting across the `gpu-image-processing` project.

## API

### `public string GetFileExtension()`

Returns the standard file extension used for Markdown output files in the project.

- **Parameters**: None
- **Return value**: A string representing the file extension (typically `.md`).
- **Exceptions**: None

### `public string GetMimeType()`

Returns the MIME type associated with Markdown-formatted content.

- **Parameters**: None
- **Return value**: A string representing the MIME type (typically `text/markdown`).
- **Exceptions**: None

### `public string FormatResult(object result)`

Formats a single processing result into a Markdown string.

- **Parameters**:
  - `result`: The object to format (e.g., an image processing result).
- **Return value**: A Markdown-formatted string representing the result.
- **Exceptions**: Throws `ArgumentNullException` if `result` is `null`.

### `public string FormatResults(IEnumerable<object> results)`

Formats a collection of processing results into a Markdown string.

- **Parameters**:
  - `results`: An enumerable collection of objects to format.
- **Return value**: A Markdown-formatted string representing all results.
- **Exceptions**:
  - Throws `ArgumentNullException` if `results` is `null`.
  - Throws `ArgumentException` if `results` contains a `null` element.

### `public string FormatJob(Job job)`

Formats a job object into a Markdown string.

- **Parameters**:
  - `job`: The job object to format.
- **Return value**: A Markdown-formatted string representing the job.
- **Exceptions**: Throws `ArgumentNullException` if `job` is `null`.

### `public string FormatDevice(Device device)`

Formats a device object into a Markdown string.

- **Parameters**:
  - `device`: The device object to format.
- **Return value**: A Markdown-formatted string representing the device.
- **Exceptions**: Throws `ArgumentNullException` if `device` is `null`.

### `public string FormatError(Exception error)`

Formats an exception into a Markdown string.

- **Parameters**:
  - `error`: The exception to format.
- **Return value**: A Markdown-formatted string representing the error.
- **Exceptions**: Throws `ArgumentNullException` if `error` is `null`.

### `public string Format(string content)`

Formats a raw string into a Markdown string with consistent styling.

- **Parameters**:
  - `content`: The raw string to format.
- **Return value**: A Markdown-formatted string.
- **Exceptions**: Throws `ArgumentNullException` if `content` is `null`.

## Usage

### Example 1: Formatting a single result
