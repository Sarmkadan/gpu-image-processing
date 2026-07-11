# LoggingMiddleware

`LoggingMiddleware` is designed to intercept and record processing operations within the `gpu-image-processing` pipeline. It provides visibility into request and response flows by logging relevant metadata at key stages of the middleware execution, enabling effective monitoring, auditing, and troubleshooting of image processing jobs.

## API

### LoggingMiddleware()
Initializes a new instance of the `LoggingMiddleware` class.

### string GetName()
Returns the unique identifier for this middleware component, used for tracking, identification, and configuration lookups within the pipeline.

### int GetPriority()
Returns the execution priority of this middleware. Lower integer values represent higher priority, determining the order of this middleware relative to others within the processing pipeline.

### async Task<MiddlewareResult> ExecuteAsync(...)
Executes the logging logic for the current request context. This method asynchronously captures relevant metadata, logs the activity, and allows the pipeline to proceed to the next component. It returns a `MiddlewareResult` indicating the outcome of the operation.

## Usage

### Registering the Middleware
```csharp
var pipeline = new ProcessingPipeline();
pipeline.AddMiddleware(new LoggingMiddleware());
```

### Manual Pipeline Invocation
```csharp
var middleware = new LoggingMiddleware();
var result = await middleware.ExecuteAsync(context);

if (result.IsFailure)
{
    // Handle logging failure or pipeline interruption
}
```

## Notes

*   **Thread Safety:** The `LoggingMiddleware` implementation is stateless regarding individual requests; it is safe for concurrent use across multiple threads within the pipeline.
*   **Performance Impact:** As this middleware performs I/O-bound logging operations, high-frequency processing may require an asynchronous, non-blocking logger implementation to avoid pipeline bottlenecks.
*   **Execution Order:** Ensure that the priority returned by `GetPriority()` is configured correctly to ensure logging captures the desired state of the pipeline before or after other critical middleware.
