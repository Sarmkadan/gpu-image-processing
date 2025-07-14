# HttpImageClient

The `HttpImageClient` class provides an asynchronous interface for performing HTTP operations specifically tailored for image resources within the `gpu-image-processing` pipeline. It encapsulates the logic for verifying image availability, downloading image data for local GPU processing, and uploading processed results, while exposing diagnostic properties such as the last recorded HTTP status code and the target URL.

## API

### Constructors

#### `public HttpImageClient()`
Initializes a new instance of the `HttpImageClient` class. The instance is created in a ready state but requires the `Url` property to be set before invoking operation methods.

### Methods

#### `public async Task<bool> VerifyImageUrlAsync()`
Asynchronously validates whether the image resource located at the current `Url` exists and is accessible.
*   **Return Value**: Returns `true` if the server responds with a success status code (typically 200 OK); otherwise, returns `false`.
*   **Exceptions**: Throws `HttpImageClientException` if the request fails due to network errors, invalid URL formats, or if the response cannot be interpreted.

#### `public async Task<bool> DownloadImageAsync()`
Asynchronously retrieves the image data from the specified `Url`.
*   **Return Value**: Returns `true` if the image data was successfully downloaded and handled; otherwise, returns `false`.
*   **Exceptions**: Throws `HttpImageClientException` if the download fails, the connection is lost, or the server returns an error status code.

#### `public async Task<bool> UploadImageAsync()`
Asynchronously sends image data to the specified `Url`.
*   **Return Value**: Returns `true` if the image was successfully uploaded and acknowledged by the server; otherwise, returns `false`.
*   **Exceptions**: Throws `HttpImageClientException` if the upload fails, the server rejects the payload, or a network interruption occurs.

#### `public void Dispose()`
Releases unmanaged resources used by the `HttpImageClient` and optionally releases managed resources. This method should be called when the client is no longer needed to ensure proper cleanup of underlying HTTP handlers or streams.

#### `public override string ToString()`
Returns a string representation of the current `HttpImageClient` instance, typically including the current `Url` and internal state information for debugging purposes.

### Properties

#### `public string? Url`
Gets or sets the Uniform Resource Locator (URI) for the image resource. This property must be assigned a valid HTTP or HTTPS address before calling `VerifyImageUrlAsync`, `DownloadImageAsync`, or `UploadImageAsync`.

#### `public int? HttpStatusCode`
Gets the HTTP status code returned by the most recent asynchronous operation. If no operation has been performed or if the status code is unavailable, this property returns `null`.

### Exceptions

#### `public HttpImageClientException`
Represents errors that occur during HTTP image operations. This exception is thrown by `VerifyImageUrlAsync`, `DownloadImageAsync`, and `UploadImageAsync` when an operation fails. It typically includes details regarding the failure context and the associated `HttpStatusCode`.

*(Note: The signature list indicates `HttpImageClientException` twice; this represents the single exception type used across the class for error signaling.)*

## Usage

### Example 1: Verifying and Downloading an Image
This example demonstrates initializing the client, verifying the remote resource exists, and downloading it for processing.

```csharp
using var client = new HttpImageClient();
client.Url = "https://storage.example.com/input/sample-image.png";

try 
{
    bool isValid = await client.VerifyImageUrlAsync();
    if (isValid)
    {
        bool downloaded = await client.DownloadImageAsync();
        if (downloaded)
        {
            Console.WriteLine($"Image downloaded successfully. Status: {client.HttpStatusCode}");
            // Proceed with GPU processing logic here
        }
        else
        {
            Console.WriteLine("Download failed despite valid URL verification.");
        }
    }
    else
    {
        Console.WriteLine($"Image URL invalid or unreachable. Status: {client.HttpStatusCode}");
    }
}
catch (HttpImageClientException ex)
{
    Console.Error.WriteLine($"Critical error during image retrieval: {ex.Message}");
}
```

### Example 2: Uploading Processed Results
This example shows how to upload a processed image back to a server endpoint, handling potential transmission errors.

```csharp
using var client = new HttpImageClient();
client.Url = "https://api.example.com/results/upload";

try 
{
    // Assume image data is prepared and ready in the client context
    bool uploaded = await client.UploadImageAsync();
    
    if (uploaded)
    {
        Console.WriteLine($"Upload complete. Server responded with: {client.HttpStatusCode}");
    }
    else
    {
        Console.WriteLine("Upload request returned false.");
    }
}
catch (HttpImageClientException ex)
{
    // Log specific failure details including the status code if available
    Console.Error.WriteLine($"Upload failed: {ex.Message} (Status: {client.HttpStatusCode})");
    throw;
}
```

## Notes

*   **Thread Safety**: The `HttpImageClient` instance is not guaranteed to be thread-safe. Concurrent calls to `DownloadImageAsync`, `UploadImageAsync`, or `VerifyImageUrlAsync` on the same instance may result in race conditions regarding the `HttpStatusCode` property or the underlying HTTP connection state. Create separate instances for parallel operations.
*   **State Dependency**: All asynchronous operation methods depend on the `Url` property being set to a non-null, well-formed URI. Invoking these methods without setting `Url` will likely result in a `HttpImageClientException`.
*   **Resource Management**: As the class implements `Dispose`, it should ideally be used within a `using` statement or explicitly disposed of after use to prevent socket exhaustion or memory leaks associated with open HTTP connections.
*   **Status Code Availability**: The `HttpStatusCode` property reflects only the *last* completed operation. In multi-step workflows (e.g., verify then download), the property will update after each call, overwriting the previous status.
