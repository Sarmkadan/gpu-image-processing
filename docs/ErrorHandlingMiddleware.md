# ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` acts as a primary exception interceptor within the `gpu-image-processing` pipeline. It captures unhandled exceptions occurring during image processing, logging diagnostic information and transforming errors into a standardized `MiddlewareResult` format, thereby ensuring system stability and consistent error reporting.

## API

### ErrorHandlingMiddleware()
Initializes a new instance of the `ErrorHandlingMiddleware` class.

### string GetName()
Retrieves the unique identifier of the middleware.
- **Returns:** A `string` representing the middleware name.

### int GetPriority()
Retrieves the execution priority of the middleware.
- **Returns:** An `int` representing the priority, used to determine the execution order within the pipeline.

### Task<MiddlewareResult> ExecuteAsync(ProcessingContext context)
Executes the error handling logic, wrapping the subsequent pipeline stages in a try-catch block.
- **Parameters:** `context` (The `ProcessingContext` containing the state of the current processing task).
- **Returns:** A `Task<MiddlewareResult>` that resolves to a successful result upon completion or an error-containing result if an exception is caught.
- **Throws:** Does not explicitly throw exceptions; it captures them and maps them to a `MiddlewareResult`.

## Usage

### Example 1: Registering in the Pipeline
```csharp
var pipeline = new ProcessingPipeline();
pipeline.RegisterMiddleware(new ErrorHandlingMiddleware());
// Further pipeline configuration...
```

### Example 2: Manually Invoking for Testing
```csharp
var middleware = new ErrorHandlingMiddleware();
var context = new ProcessingContext(image);

var result = await middleware.ExecuteAsync(context);

if (!result.IsSuccess)
{
    // Handle error result
    logger.LogError(result.ErrorMessage);
}
```

## Notes

- **Thread-Safety:** The middleware is designed to be stateless and is intended to be used as a singleton within the pipeline, ensuring thread safety during concurrent image processing operations.
- **Execution Order:** To serve effectively as a safety net for the entire pipeline, it is recommended to register this middleware with a high-priority value, ensuring it wraps all other processing steps.
- **Exception Handling:** While this component captures exceptions within the pipeline, catastrophic system-level failures that occur outside of the `ExecuteAsync` context may still propagate.
