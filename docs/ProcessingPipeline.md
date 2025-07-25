# ProcessingPipeline

The `ProcessingPipeline` class provides a modular and configurable framework for orchestrating a sequence of middleware components to perform GPU-accelerated image processing tasks. By maintaining an ordered collection of middleware and a mandatory final handler, the pipeline allows developers to define complex, reusable image transformation workflows that are executed sequentially within a provided processing context.

## API

### ProcessingPipeline()
Initializes a new instance of the `ProcessingPipeline` class.

### void RegisterMiddleware(IMiddleware middleware)
Appends a middleware component to the end of the current execution sequence.
- **Parameters:** `middleware` (The middleware implementation to add to the pipeline).
- **Throws:** `ArgumentNullException` if `middleware` is null.

### void SetFinalHandler(IFinalHandler handler)
Defines the terminal handler that will be executed after all registered middleware have completed.
- **Parameters:** `handler` (The final handler implementation to set).
- **Throws:** `ArgumentNullException` if `handler` is null.

### async Task<MiddlewareResult> ExecuteAsync(ProcessingContext context)
Executes the registered pipeline sequentially with the provided processing context.
- **Parameters:** `context` (The context containing the data and state for the processing task).
- **Returns:** A `Task<MiddlewareResult>` representing the asynchronous operation, which completes with the final `MiddlewareResult`.
- **Throws:** `InvalidOperationException` if no final handler has been set prior to execution.

### IReadOnlyList<string> GetMiddlewareOrder()
Retrieves the current ordered sequence of registered middleware names.
- **Returns:** An `IReadOnlyList<string>` representing the identifiers of the registered middleware in their execution order.

### bool RemoveMiddleware(string name)
Removes a specific middleware component from the pipeline by its identifier.
- **Parameters:** `name` (The identifier of the middleware to remove).
- **Returns:** `true` if the middleware was found and successfully removed; otherwise, `false`.

### void ClearMiddleware()
Removes all registered middleware from the pipeline, resetting the sequence while leaving the final handler unchanged.

## Usage

### Simple Pipeline Configuration
```csharp
var pipeline = new ProcessingPipeline();

pipeline.RegisterMiddleware(new ResizeMiddleware(width: 1920, height: 1080));
pipeline.RegisterMiddleware(new GrayscaleMiddleware());

pipeline.SetFinalHandler(new SaveToDiskHandler(outputPath: "result.png"));

var context = new ProcessingContext(sourceImagePath: "input.png");
var result = await pipeline.ExecuteAsync(context);
```

### Dynamic Middleware Management
```csharp
var pipeline = new ProcessingPipeline();
pipeline.RegisterMiddleware(new NoiseReductionMiddleware("denoise"));
pipeline.RegisterMiddleware(new SharpenMiddleware("sharpen"));

// Remove a specific middleware if it exists
if (pipeline.RemoveMiddleware("denoise"))
{
    Console.WriteLine("Denoise middleware removed.");
}

pipeline.SetFinalHandler(new DisplayHandler());
// Execute the pipeline...
```

## Notes

- **Thread-Safety:** The `ProcessingPipeline` is not thread-safe for concurrent modifications and execution. It is expected that the middleware and final handler configuration be completed before `ExecuteAsync` is invoked in a multi-threaded environment.
- **Execution Order:** Middleware components are executed in the exact order in which they are registered via `RegisterMiddleware`.
- **Mandatory Final Handler:** The `ExecuteAsync` method requires that a final handler be defined via `SetFinalHandler`. If this is not done, an `InvalidOperationException` will be thrown, as the pipeline cannot conclude the processing flow without a destination or terminal action.
- **Middleware Names:** The identifier (`name`) used for removing middleware via `RemoveMiddleware` is typically derived from the middleware's implementation or metadata. Ensure unique naming if removal functionality is required.
