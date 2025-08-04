# JsonResultFormatter

`JsonResultFormatter` is a formatter class that converts various GPU image-processing results, jobs, devices, and errors into JSON-serialized strings. It extends a base formatter with specialized methods for domain-specific types in the `gpu-image-processing` project, ensuring consistent JSON output for API responses and logging.

## API

### `public JsonResultFormatter`

Constructs a new instance of the `JsonResultFormatter`. This formatter is stateless and thread-safe; no instance fields are used.

### `public string GetFileExtension()`

Returns the file extension used for JSON output, which is always `.json`.

- **Parameters:** None
- **Return value:** `"json"`
- **Throws:** Never

### `public string GetMimeType()`

Returns the MIME type for JSON output, which is always `application/json`.

- **Parameters:** None
- **Return value:** `"application/json"`
- **Throws:** Never

### `public string FormatResult(object result)`

Serializes a generic result object into a JSON string. The object is serialized using the system JSON serializer with default settings.

- **Parameters:**
  - `result`: The result object to serialize.
- **Return value:** A JSON string representing the serialized result.
- **Throws:** `System.ArgumentNullException` if `result` is `null`.

### `public string FormatResults(IEnumerable<object> results)`

Serializes a collection of result objects into a JSON array string. Each item in the collection is serialized individually.

- **Parameters:**
  - `results`: The collection of result objects to serialize.
- **Return value:** A JSON array string representing the serialized results.
- **Throws:** `System.ArgumentNullException` if `results` is `null`.

### `public string FormatJob(Job job)`

Serializes a `Job` object into a JSON string. The `Job` type is assumed to be serializable by the system JSON serializer.

- **Parameters:**
  - `job`: The job to serialize.
- **Return value:** A JSON string representing the serialized job.
- **Throws:** `System.ArgumentNullException` if `job` is `null`.

### `public string FormatDevice(Device device)`

Serializes a `Device` object into a JSON string. The `Device` type is assumed to be serializable by the system JSON serializer.

- **Parameters:**
  - `device`: The device to serialize.
- **Return value:** A JSON string representing the serialized device.
- **Throws:** `System.ArgumentNullException` if `device` is `null`.

### `public string FormatError(Error error)`

Serializes an `Error` object into a JSON string. The `Error` type is assumed to be serializable by the system JSON serializer.

- **Parameters:**
  - `error`: The error to serialize.
- **Return value:** A JSON string representing the serialized error.
- **Throws:** `System.ArgumentNullException` if `error` is `null`.

### `public override DateTime Read()`

Reads and returns the current UTC timestamp. This method is part of a base formatter interface and is implemented to return a consistent timestamp for serialization timing.

- **Parameters:** None
- **Return value:** The current UTC `DateTime`.
- **Throws:** Never

### `public override void Write(string value)`

Writes a JSON string directly to the output sink. This method is part of a base formatter interface and is implemented as a no-op since the class serializes objects directly to strings.

- **Parameters:**
  - `value`: The JSON string to write (ignored).
- **Return value:** None
- **Throws:** Never
