# RemoteImageService

The `RemoteImageService` class provides functionality for downloading images from remote sources, validating downloaded image data, and managing trusted source registrations. It maintains state for the most recent download operation, including the raw image bytes, content type, size, source URL, and timestamp. The class also exposes static factory members for creating `RemoteImageResult` instances representing success or failure.

## API

### Constructor

- **`public RemoteImageService()`**  
  Initializes a new instance of the `RemoteImageService`. No parameters are required. All properties are set to their default values.

### Methods

- **`public void RegisterTrustedSource()`**  
  Registers the current source (as defined by the `Url` and `ApiKey` properties) as a trusted source. The exact behavior depends on the implementation; typically this adds the source to an internal allowlist used during validation or download.  
  *Throws*: `InvalidOperationException` if `Url` is null or empty.

- **`public async Task<RemoteImageResult> DownloadImageAsync()`**  
  Downloads a single image from the URL specified in the `Url` property. The download is performed asynchronously. On completion, the service’s state properties (`ImageData`, `ContentType`, `SizeBytes`, `SourceUrl`, `DownloadedAt`, `IsSuccess`, `Data`, `Error`) are updated to reflect the result.  
  **Returns**: A `RemoteImageResult` indicating success or failure.  
  *Throws*: `InvalidOperationException` if `Url` is not set; `HttpRequestException` for network errors; `TaskCanceledException` if the operation is cancelled.

- **`public async Task<List<RemoteImageResult>> DownloadImagesAsync()`**  
  Downloads multiple images asynchronously. The specific source URLs are determined by the implementation (e.g., from an internal queue or configuration). Each download result is returned as a `RemoteImageResult` in a list.  
  **Returns**: A list of `RemoteImageResult` objects, one per attempted download.  
  *Throws*: `InvalidOperationException` if no sources are configured; `HttpRequestException` for network errors.

- **`public bool ValidateImageData()`**  
  Validates the currently stored `ImageData`. The validation logic (e.g., checksum, format header verification) is implementation-specific.  
  **Returns**: `true` if the image data is considered valid; otherwise `false`.  
  *Throws*: `InvalidOperationException` if `ImageData` is null or empty.

### Properties

- **`public string Url { get; set; }`**  
  Gets or sets the URL of the remote image to download. Must be set before calling `DownloadImageAsync`.

- **`public string ApiKey { get; set; }`**  
  Gets or sets the API key used for authentication when accessing the remote source.

- **`public DateTime RegisteredAt { get; }`**  
  Gets the timestamp when the current source was last registered via `RegisterTrustedSource`. Default is `DateTime.MinValue` if never registered.

- **`public byte[] ImageData { get; }`**  
  Gets the raw bytes of the most recently downloaded image. `null` if no download has succeeded.

- **`public string ContentType { get; }`**  
  Gets the MIME content type of the most recently downloaded image (e.g., `"image/jpeg"`). `null` if not available.

- **`public long SizeBytes { get; }`**  
  Gets the size in bytes of the most recently downloaded image. `0` if no download has succeeded.

- **`public string SourceUrl { get; }`**  
  Gets the URL from which the most recent image was downloaded.

- **`public DateTime DownloadedAt { get; }`**  
  Gets the timestamp of the most recent successful download. `DateTime.MinValue` if no download has occurred.

- **`public bool IsSuccess { get; }`**  
  Gets a value indicating whether the most recent download operation succeeded.

- **`public RemoteImageData Data { get; }`**  
  Gets a structured representation of the downloaded image (e.g., dimensions, pixel data). `null` if the download failed or has not been performed.

- **`public string Error { get; }`**  
  Gets the error message from the most recent failed download. `null` if the last operation succeeded.

### Static Members

- **`public static RemoteImageResult Success { get; }`**  
  Gets a pre-built `RemoteImageResult` instance representing a successful operation with no associated data. Useful for returning a generic success indicator.

- **`public static RemoteImageResult Failure { get; }`**  
  Gets a pre-built `RemoteImageResult` instance representing a failed operation with no specific error message. Useful for returning a generic failure indicator.

## Usage

### Example 1: Download a single image and validate

```csharp
var service = new RemoteImageService();
service.Url = "https://example.com/image.jpg";
service.ApiKey = "my-api-key";

RemoteImageResult result = await service.DownloadImageAsync();

if (result.IsSuccess)
{
    bool valid = service.ValidateImageData();
    Console.WriteLine($"Downloaded {service.SizeBytes} bytes, valid: {valid}");
}
else
{
    Console.WriteLine($"Download failed: {service.Error}");
}
```

### Example 2: Register a trusted source and download multiple images

```csharp
var service = new RemoteImageService();
service.Url = "https://trusted-cdn.example.com";
service.ApiKey = "trusted-key";

service.RegisterTrustedSource();

List<RemoteImageResult> results = await service.DownloadImagesAsync();

foreach (var res in results)
{
    if (res.IsSuccess)
        Console.WriteLine($"Downloaded from {res.SourceUrl}");
    else
        Console.WriteLine($"Error: {res.Error}");
}

// Use static results for simple checks
if (results.Count == 0)
    return RemoteImageService.Failure;
```

## Notes

- **Thread safety**: `RemoteImageService` is not thread-safe. Instance members (properties and methods) are intended to be used by a single thread at a time. Concurrent calls to `DownloadImageAsync` or `DownloadImagesAsync` may lead to inconsistent state. If concurrent access is required, external synchronization (e.g., a lock) must be used.
- **State mutation**: Methods such as `DownloadImageAsync` and `RegisterTrustedSource` modify the service’s properties. Reading properties like `ImageData` or `IsSuccess` immediately after an asynchronous call is safe only if no other operation is in progress.
- **Empty or null URL**: Calling `DownloadImageAsync` without setting `Url` to a non-null, non-empty string will throw an `InvalidOperationException`.
- **Large images**: The `ImageData` property holds the entire image in memory. For very large images, consider streaming or processing in chunks to avoid high memory usage.
- **Static result instances**: `RemoteImageService.Success` and `RemoteImageService.Failure` are shared instances. They should not be modified; their properties are read-only by convention. Use them for lightweight result comparisons or as default return values.
