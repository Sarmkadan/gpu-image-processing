# CsvResultFormatter

A formatter that converts structured result data into CSV (Comma-Separated Values) format. It supports formatting individual results, collections of results, job metadata, device information, errors, and statistics into CSV strings suitable for file output or streaming.

## API

### `public string GetFileExtension()`
Returns the file extension (including the leading dot) recommended for CSV output files.
- **Parameters**: None
- **Return value**: A string representing the file extension (e.g., `.csv`).
- **Exceptions**: Does not throw under normal operation.

### `public string GetMimeType()`
Returns the MIME type for CSV content.
- **Parameters**: None
- **Return value**: A string representing the MIME type (e.g., `text/csv`).
- **Exceptions**: Does not throw under normal operation.

### `public string FormatResult(object result)`
Formats a single result object into a CSV string.
- **Parameters**:
  - `result`: The result object to format. Expected to be a type with public properties.
- **Return value**: A CSV-formatted string representing the result.
- **Exceptions**: Throws `ArgumentNullException` if `result` is `null`.

### `public string FormatResults(IEnumerable<object> results)`
Formats a collection of result objects into a CSV string with a header row.
- **Parameters**:
  - `results`: An enumerable of result objects to format.
- **Return value**: A CSV-formatted string with headers and one row per result.
- **Exceptions**:
  - Throws `ArgumentNullException` if `results` is `null`.
  - Throws `ArgumentException` if `results` is empty.

### `public string FormatJob(object job)`
Formats job metadata into a CSV string.
- **Parameters**:
  - `job`: The job object containing metadata to format.
- **Return value**: A CSV-formatted string representing the job metadata.
- **Exceptions**: Throws `ArgumentNullException` if `job` is `null`.

### `public string FormatDevice(object device)`
Formats device information into a CSV string.
- **Parameters**:
  - `device`: The device object containing information to format.
- **Return value**: A CSV-formatted string representing the device information.
- **Exceptions**: Throws `ArgumentNullException` if `device` is `null`.

### `public string FormatError(object error)`
Formats an error object into a CSV string.
- **Parameters**:
  - `error`: The error object to format.
- **Return value**: A CSV-formatted string representing the error.
- **Exceptions**: Throws `ArgumentNullException` if `error` is `null`.

### `public string FormatStatistics(object statistics)`
Formats statistics data into a CSV string.
- **Parameters**:
  - `statistics`: The statistics object to format.
- **Return value**: A CSV-formatted string representing the statistics.
- **Exceptions**: Throws `ArgumentNullException` if `statistics` is `null`.

## Usage

### Example 1: Formatting a Single Result
