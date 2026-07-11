# CompressionMiddleware

The `CompressionMiddleware` is a component within the `gpu-image-processing` pipeline designed to handle the automatic compression of request payloads. It integrates into the middleware chain to reduce network bandwidth and storage requirements by compressing data based on configurable size thresholds and intensity levels before processing or transmission.

## API

### CompressionMiddleware
Initializes a new instance of the `CompressionMiddleware` class.

### ProcessAsync
Executes the primary middleware logic to evaluate and optionally compress the incoming request data. 
*   **Parameters:** `RequestMiddlewareContext context` representing the current request.
*   **Return Value:** A `Task<RequestMiddlewareResult>` indicating the outcome of the processing attempt.
*   **Exceptions:** Throws an exception if the context is null or if the compression operation fails.

### DecompressAsync
A static utility method for decompressing data that was previously processed by this middleware.
*   **Parameters:** `string compressedData` containing the data to be decompressed.
*   **Return Value:** A `Task<string>` containing the original, uncompressed content.
*   **Exceptions:** Throws an exception if the input string is invalid or not in the expected compressed format.

### GetStatistics
Retrieves the current performance metrics of the compression engine.
*   **Return Value:** A `CompressionStats` object detailing compression ratios and throughput.

### MinSizeToCompress
A property defining the minimum byte size threshold required for the middleware to trigger compression. Data smaller than this value remains uncompressed.

### CompressionLevel
A property specifying the intensity of the compression algorithm. Accepted string values define the balance between compression ratio and processing speed (e.g., "Optimal", "Fastest").

## Usage

### Configuring in the Pipeline
```csharp
var pipeline = new ProcessingPipeline();
var compression = new CompressionMiddleware 
{ 
    MinSizeToCompress = 1024 * 1024, // 1MB threshold
    CompressionLevel = "Optimal" 
};
pipeline.AddMiddleware(compression);
```

### Decompressing Data
```csharp
string compressedPayload = await GetPayloadFromStorage();
string originalData = await CompressionMiddleware.DecompressAsync(compressedPayload);
// Proceed with processing originalData
```

## Notes

*   **Thread-Safety:** The `CompressionMiddleware` instance is designed to be thread-safe, allowing for concurrent processing of multiple requests in a multi-threaded environment.
*   **Edge Cases:** Setting `MinSizeToCompress` to a negative value or zero may lead to inefficient processing of trivial payloads. Ensure that the `CompressionLevel` string matches the supported configurations of the underlying compression provider; providing an unsupported level will result in a runtime exception.
*   **Performance:** While reducing payload size, compression introduces CPU overhead. If system throughput is critical, monitor `CompressionStats` via `GetStatistics` to evaluate the impact of different `CompressionLevel` settings on overall latency.
