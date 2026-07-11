# ApiResponse

`ApiResponse<T>` is a generic wrapper used throughout the GPU image‑processing service to convey the outcome of an operation. It combines a success flag, payload data, optional message, collection of validation errors, and metadata useful for tracing and pagination.

## API

### Properties

| Member | Type | Purpose | Remarks |
|--------|------|---------|---------|
| `IsSuccess` | `bool` | Indicates whether the operation completed without error. | Set to `true` by factory methods representing success; otherwise `false`. |
| `Data` | `T` | The primary result payload when `IsSuccess` is `true`. | May be `null` if the operation yields no meaningful data. |
| `Message` | `string` | A human‑readable summary of the outcome (e.g., “Processing completed”). | Often set by factory methods; can be overridden manually. |
| `Errors` | `List<ApiError>` | Collection of validation or runtime errors when `IsSuccess` is `false`. | Initialized as an empty list; never `null`. |
| `Metadata` | `ApiMetadata` | Operational metadata such as request identifier and environment. | Populated by the infrastructure; never `null`. |
| `Items` | `List<T>` | Paginated collection of results (used for list‑returning endpoints). | Empty when no items are present; not used for single‑item responses. |
| `PageNumber` | `int` | One‑based index of the current page in a paginated result set. | Valid only when `Items` is populated; defaults to `1`. |
| `PageSize` | `int` | Maximum number of items per page in a paginated result set. | Valid only when `Items` is populated; defaults to server‑configured size. |

### Static Members

| Member | Signature | Purpose | Parameters | Return Value | Throws |
|--------|-----------|---------|------------|--------------|--------|
| `Success` | `public static ApiResponse<T> Success(T data = default, string message = null)` | Creates a successful response. | `data` – payload to embed; `message` – optional informational text. | New `ApiResponse<T>` instance with `IsSuccess = true`. | None. |
| `Failure` (overload 1) | `public static ApiResponse<T> Failure(string message)` | Creates a failure response with a simple message. | `message` – description of the failure. | New `ApiResponse<T>` instance with `IsSuccess = false` and a single generic error. | `ArgumentNullException` if `message` is `null`. |
| `Failure` (overload 2) | `public static ApiResponse<T> Failure(IEnumerable<ApiError> errors)` | Creates a failure response from a collection of detailed errors. | `errors` – errors to include; may be empty but not `null`. | New `ApiResponse<T>` instance with `IsSuccess = false` and the supplied errors. | `ArgumentNullException` if `errors` is `null`. |

### Methods

| Member | Signature | Purpose | Parameters | Return Value | Throws |
|--------|-----------|---------|------------|--------------|--------|
| `AddError` | `public void AddError(ApiError error)` | Appends an error to the `Errors` collection. | `error` – error to add; must not be `null`. | `void`. | `ArgumentNullException` if `error` is `null`. |

### Nested Types

#### `ApiError`

| Member | Type | Purpose |
|--------|------|---------|
| `Code` | `string` | Machine‑readable error identifier (e.g., `VALIDATION_FAILED`). |
| `Message` | `string` | Human‑readable description of the error. |
| `Details` | `string` | Optional supplementary information (stack trace, validation fields). |
| `Timestamp` | `DateTime` | UTC time when the error was generated. |
| `Version` | `string` | Version of the API that produced the error. |

#### `ApiMetadata`

| Member | Type | Purpose |
|--------|------|---------|
| `RequestId` | `string` | Unique identifier for the incoming request, useful for log correlation. |
| `Environment` | `string` | Deployment environment name (e.g., `Development`, `Production`). |
| `Timestamp` | `DateTime` | UTC time when the response was assembled. |

## Usage

### Successful operation

```csharp
// Assume ProcessImage returns a processed bitmap.
Bitmap result = await imageProcessor.ProcessImage(input);

// Build a successful response.
ApiResponse<Bitmap> response = ApiResponse<Bitmap>.Success(
    data: result,
    message: "Image processed successfully."
);

if (response.IsSuccess)
{
    // Use response.Data safely.
    SaveBitmap(response.Data, outputPath);
}
```

### Failure with detailed errors

```csharp
var errors = new List<ApiError>
{
    new ApiError
    {
        Code = "INVALID_FORMAT",
        Message = "The supplied image format is not supported.",
        Details = "Only PNG and JPEG are allowed.",
        Timestamp = DateTime.UtcNow,
        Version = "1.0.0"
    }
};

ApiResponse<Bitmap> response = ApiResponse<Bitmap>.Failure(errors);

// Inspect errors for logging or user feedback.
foreach (var err in response.Errors)
{
    logger.Error($"[{err.Code}] {err.Message} - {err.Details}");
}
```

## Notes

- The `Errors` list is initialized to an empty collection; therefore checking `response.Errors.Any()` is safe without null checks.
- Static factory methods (`Success`, `Failure`) return new instances each call; they do not share mutable state, making them thread‑safe.
- The `AddError` method is **not** thread‑safe when invoked concurrently on the same `ApiResponse<T>` instance. External synchronization is required if multiple threads may call it on the same object.
- `Data` and `Items` may be `null`; consumers should guard against dereferencing them when `IsSuccess` is `false` or when the operation does not produce a payload.
- Timestamp values in both `ApiError` and `ApiMetadata` are set to `DateTime.UtcNow` at the point of creation; they are not automatically updated thereafter.
