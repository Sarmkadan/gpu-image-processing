# FilterConfigurationRepositoryExtensions

The `FilterConfigurationRepositoryExtensions` class provides a set of asynchronous extension methods designed to simplify querying and retrieving `FilterConfiguration` entities from an underlying repository. These methods abstract common data access patterns, such as filtering by type, matching name patterns, retrieving multiple types simultaneously, and handling pagination for active configurations, ensuring consistent interaction with the GPU image processing configuration store.

## API

### GetActiveByTypeAsync
Retrieves a single active filter configuration that matches a specific filter type.
*   **Purpose**: To fetch the currently active configuration for a given filter kind without retrieving inactive or deprecated entries.
*   **Parameters**: Accepts the repository instance (via extension), the target filter type, and an optional cancellation token.
*   **Return Value**: Returns a `Task<FilterConfiguration?>`. The result is the matching configuration if found and active; otherwise, it returns `null`.
*   **Throws**: Throws `OperationCanceledException` if the cancellation token is triggered. May throw repository-specific exceptions if the underlying data source is unavailable.

### GetByNamePatternAsync
Retrieves a collection of filter configurations whose names match a specified string pattern.
*   **Purpose**: To search for configurations using partial name matches, useful for dynamic discovery or administrative listing.
*   **Parameters**: Accepts the repository instance, a string pattern (typically supporting SQL-like wildcards or regex depending on the provider implementation), and an optional cancellation token.
*   **Return Value**: Returns a `Task<IEnumerable<FilterConfiguration>>`. The result is an enumerable of matching configurations, which may be empty if no matches are found.
*   **Throws**: Throws `OperationCanceledException` if the cancellation token is triggered. Throws `ArgumentNullException` if the provided pattern is null.

### GetByTypesAsync
Retrieves all filter configurations that match any of the types provided in a collection.
*   **Purpose**: To efficiently batch-fetch configurations for multiple filter types in a single operation, reducing round-trips compared to individual lookups.
*   **Parameters**: Accepts the repository instance, an enumerable of filter types to search for, and an optional cancellation token.
*   **Return Value**: Returns a `Task<IEnumerable<FilterConfiguration>>`. The result contains all configurations matching the requested types, regardless of their active status unless filtered by the underlying repository logic.
*   **Throws**: Throws `OperationCanceledException` if the cancellation token is triggered. Throws `ArgumentNullException` if the types collection is null.

### GetActivePagedAsync
Retrieves a page of active filter configurations based on skip and take parameters.
*   **Purpose**: To support UI listing or batch processing scenarios where loading the entire set of active configurations is inefficient.
*   **Parameters**: Accepts the repository instance, the number of records to skip, the number of records to take (page size), and an optional cancellation token.
*   **Return Value**: Returns a `Task<IEnumerable<FilterConfiguration>>`. The result is a subset of active configurations corresponding to the requested page.
*   **Throws**: Throws `OperationCanceledException` if the cancellation token is triggered. Throws `ArgumentOutOfRangeException` if skip or take values are negative.

## Usage

The following example demonstrates retrieving a specific active configuration for a Gaussian Blur filter and handling the potential absence of such a configuration.

```csharp
using gpu_image_processing.Configuration;
using gpu_image_processing.Repositories;

public async Task LoadGaussianBlurConfigAsync(IFilterConfigurationRepository repository)
{
    // Retrieve the active configuration specifically for the GaussianBlur type
    var config = await repository.GetActiveByTypeAsync(FilterType.GaussianBlur);

    if (config != null)
    {
        Console.WriteLine($"Loaded kernel size: {config.Parameters["KernelSize"]}");
    }
    else
    {
        Console.WriteLine("No active Gaussian Blur configuration found.");
    }
}
```

The following example illustrates fetching a page of active filters for a dashboard view and simultaneously retrieving all configurations related to edge detection algorithms.

```csharp
using gpu_image_processing.Configuration;
using gpu_image_processing.Repositories;

public async Task LoadDashboardDataAsync(IFilterConfigurationRepository repository)
{
    // Fetch the second page of active configurations (skip 10, take 10)
    var activePage = await repository.GetActivePagedAsync(10, 10);
    
    // Fetch all configurations for specific edge detection types
    var edgeTypes = new[] { FilterType.Sobel, FilterType.Canny, FilterType.Laplacian };
    var edgeConfigs = await repository.GetByTypesAsync(edgeTypes);

    foreach (var config in activePage)
    {
        Console.WriteLine($"Active: {config.Name} ({config.Type})");
    }

    foreach (var config in edgeConfigs)
    {
        Console.WriteLine($"Edge Filter Available: {config.Name}");
    }
}
```

## Notes

*   **Null Handling**: Consumers of `GetActiveByTypeAsync` must explicitly check for `null` return values, as the method returns `null` rather than throwing an exception when no active configuration matches the requested type.
*   **Empty Collections**: Methods returning `IEnumerable<FilterConfiguration>` (`GetByNamePatternAsync`, `GetByTypesAsync`, `GetActivePagedAsync`) will return an empty collection rather than `null` if no records match the criteria.
*   **Thread Safety**: As these are static extension methods operating on an injected repository instance, thread safety depends entirely on the implementation of the underlying `IFilterConfigurationRepository`. If the repository is not thread-safe, concurrent calls using the same repository instance should be synchronized externally.
*   **Cancellation**: All methods support cooperative cancellation via `CancellationToken`. Long-running queries or large dataset enumerations should propagate cancellation tokens to prevent resource contention during application shutdown or user navigation.
*   **Pattern Matching**: The behavior of `GetByNamePatternAsync` regarding wildcard characters (e.g., `%`, `*`) is determined by the underlying data provider. Documentation for the specific repository implementation should be consulted for exact pattern syntax.
