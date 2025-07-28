# IProcessingMiddleware

`IProcessingMiddleware` defines the contract for components participating in the `gpu-image-processing` pipeline, enabling modular, sequential execution of image operations. Implementations of this interface can inspect or modify operational parameters, maintain transient state during an operation's lifecycle, record performance metrics, and control the continuation of the pipeline through the `Next` delegate.

## API

### Properties
*   **`OperationType`** (`string`): Gets the category or type of the operation performed by the middleware.
*   **`Parameters`** (`Dictionary<string, object>`): Gets or sets the dictionary of configured parameters for the operation.
*   **`State`** (`Dictionary<string, object>`): Gets or sets the dictionary of transient state information maintained during the operation's execution.
*   **`OperationId`** (`string`): Gets the unique identifier for the current operation.
*   **`StartTime`** (`DateTime`): Gets the timestamp indicating when the middleware operation was initiated.
*   **`Next`** (`Func<Task<MiddlewareResult>>`): Gets or sets the delegate to invoke the next middleware component in the pipeline.
*   **`MiddlewareContext`**: Gets or sets the context object associated with the current pipeline execution.
*   **`IsSuccess`** (`bool`): Indicates whether the operation completed successfully.
*   **`Data`** (`object`): Gets or sets the data resulting from the operation.
*   **`ErrorMessage`** (`string`): Gets or sets the error message if the operation failed.
*   **`Metrics`** (`Dictionary<string, object>`): Gets or sets the dictionary of recorded performance metrics.
*   **`DurationMs`** (`long`): Gets or sets the duration of the operation in milliseconds.

### Methods
*   **`SetParameter(string key, object value)`** (`void`): Sets a parameter value associated with the specified key.
*   **`GetParameter<T>(string key)`** (`T`): Retrieves a parameter value of type `T` associated with the specified key.
*   **`SetState(string key, object value)`** (`void`): Sets a state value associated with the specified key.
*   **`GetState<T>(string key)`** (`T`): Retrieves a state value of type `T` associated with the specified key.
*   **`AddMetric(string key, object value)`** (`void`): Records a metric with the specified key and value.

### MiddlewareResult Class
*   **`MiddlewareResult`**: Represents the result of a middleware operation.
*   **`static MiddlewareResult Success`**: A static property representing a successful result.
*   **`static MiddlewareResult Failure`**: A static property representing a failed result.

## Usage

### Example 1: Basic Parameter Validation and Pipeline Continuation
```csharp
public async Task<MiddlewareResult> InvokeAsync(IProcessingMiddleware middleware)
{
    var threshold = middleware.GetParameter<int>("Threshold");
    
    if (threshold < 0 || threshold > 255)
    {
        return MiddlewareResult.Failure;
    }
    
    // Pass control to the next middleware in the pipeline
    return await middleware.Next();
}
```

### Example 2: Tracking State and Recording Metrics
```csharp
public async Task<MiddlewareResult> InvokeAsync(IProcessingMiddleware middleware)
{
    middleware.SetState("ProcessingStage", "Initialization");
    var startTime = DateTime.UtcNow;

    var result = await middleware.Next();
    
    var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
    middleware.AddMetric("ExecutionDuration", duration);
    middleware.SetState("ProcessingStage", "Complete");
    
    return result;
}
```

## Notes
*   **Thread Safety:** The `Parameters` and `State` dictionaries are not inherently thread-safe. If these collections are accessed or modified by multiple threads, appropriate external synchronization must be implemented.
*   **Parameter/State Access:** `GetParameter<T>` and `GetState<T>` will throw an exception if the specified key does not exist or if the value cannot be cast to the requested type `T`.
*   **Pipeline Execution:** Failing to invoke `Next` will terminate the pipeline early. Ensure `Next` is called appropriately to allow downstream middleware to execute.
