# TransformServiceExtensions
The `TransformServiceExtensions` class provides a set of extension methods for working with transforms in the context of GPU image processing. These methods enable the creation, management, and manipulation of transforms, allowing for efficient and flexible image processing workflows. The class offers a range of functionalities, including creating transforms, activating and deactivating them in bulk, retrieving parameter names and values, and reordering transforms.

## API
* `CreateTransformsAsync`: Creates a list of transforms asynchronously. Returns an `IReadOnlyList<Transform>` containing the created transforms.
* `BulkActivateTransformsAsync`: Activates a list of transforms in bulk asynchronously. Returns an `int` indicating the number of successfully activated transforms.
* `BulkDeactivateTransformsAsync`: Deactivates a list of transforms in bulk asynchronously. Returns an `int` indicating the number of successfully deactivated transforms.
* `GetDefaultParameterValueAsync`: Retrieves the default parameter value for a transform asynchronously. Returns a `float` representing the default parameter value.
* `CopyParametersAsync`: Copies parameters from one transform to another asynchronously. Returns a `bool` indicating whether the operation was successful.
* `GetParameterNamesAsync`: Retrieves a list of parameter names for a transform asynchronously. Returns an `IReadOnlyCollection<string>` containing the parameter names.
* `GetTransformsByExecutionOrderAsync`: Retrieves a list of transforms sorted by their execution order asynchronously. Returns an `IReadOnlyList<Transform>` containing the sorted transforms.
* `FindTransformsByNameAsync`: Finds a list of transforms by name asynchronously. Returns an `IReadOnlyList<Transform>` containing the matching transforms.
* `GetTransformsFilteredAsync`: Retrieves a list of transforms filtered by a specified condition asynchronously. Returns an `IReadOnlyList<Transform>` containing the filtered transforms.
* `GetNextExecutionOrderAsync`: Retrieves the next execution order for a transform asynchronously. Returns an `int` representing the next execution order.
* `ReorderTransformsSequentiallyAsync`: Reorders a list of transforms sequentially asynchronously. Returns a `bool` indicating whether the operation was successful.

## Usage
```csharp
// Example 1: Creating and activating transforms
var transforms = await TransformServiceExtensions.CreateTransformsAsync(new[] { "transform1", "transform2" });
var activatedCount = await TransformServiceExtensions.BulkActivateTransformsAsync(transforms);
Console.WriteLine($"Activated {activatedCount} transforms");

// Example 2: Retrieving and copying parameters
var parameterNames = await TransformServiceExtensions.GetParameterNamesAsync(transforms[0]);
var defaultParameterValue = await TransformServiceExtensions.GetDefaultParameterValueAsync(transforms[0]);
var copyResult = await TransformServiceExtensions.CopyParametersAsync(transforms[0], transforms[1]);
Console.WriteLine($"Parameter names: {string.Join(", ", parameterNames)}");
Console.WriteLine($"Default parameter value: {defaultParameterValue}");
Console.WriteLine($"Parameter copy result: {copyResult}");
```

## Notes
The `TransformServiceExtensions` class is designed to be thread-safe, allowing for concurrent access and manipulation of transforms. However, it is essential to note that the asynchronous nature of the methods means that the execution order of the transforms may not be guaranteed in all scenarios. Additionally, the `BulkActivateTransformsAsync` and `BulkDeactivateTransformsAsync` methods may throw exceptions if any of the transforms in the list fail to activate or deactivate. The `GetDefaultParameterValueAsync` method may also throw an exception if the default parameter value is not available for the specified transform. It is recommended to handle these exceptions accordingly to ensure robust and reliable image processing workflows.
