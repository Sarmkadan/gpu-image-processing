# ImageProcessingServiceTests

The `ImageProcessingServiceTests` class contains unit tests for the `ImageProcessingService` component of the `gpu-image-processing` project. Each test method validates a specific behavior of the service, including error handling for missing or invalid images, GPU device absence, successful processing, sequential filter application, filter failures, processing state management, and retrieval of processing results and statistics. The tests are asynchronous and are intended to be executed by a test runner such as xUnit or MSTest.

## API

### `public ImageProcessingServiceTests()`

Default constructor. Initializes a new instance of the test class. No parameters. No return value. Does not throw.

### `public async Task ProcessImageAsync_ImageNotFound_ThrowsInvalidImageException()`

Tests that `ProcessImageAsync` throws `InvalidImageException` when the specified image path does not exist.  
**Parameters:** None.  
**Returns:** A `Task` representing the asynchronous test operation.  
**Throws:** An assertion exception if the service does not throw the expected exception.

### `public async Task ProcessImageAsync_InvalidImage_ThrowsInvalidImageException()`

Tests that `ProcessImageAsync` throws `InvalidImageException` when the image file exists but its content is invalid or corrupt.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the expected exception is not thrown.

### `public async Task ProcessImageAsync_NoGpuDevice_ThrowsGpuException()`

Tests that `ProcessImageAsync` throws `GpuException` when no compatible GPU device is available.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the service does not throw `GpuException`.

### `public async Task ProcessImageAsync_SuccessfulProcessing_ReturnsCompletedResult()`

Tests that `ProcessImageAsync` returns a result with status `Completed` when processing succeeds.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the result status is not `Completed` or if an exception occurs.

### `public async Task ProcessImageAsync_MultipleFilters_AppliesAllSequentially()`

Tests that when multiple filters are specified, `ProcessImageAsync` applies them in the given order and the final result reflects all transformations.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if filters are not applied sequentially or the output is incorrect.

### `public async Task ProcessImageAsync_FilterApplicationFails_ReturnsFailedResult()`

Tests that if a filter throws an exception during processing, `ProcessImageAsync` returns a result with status `Failed` and does not propagate the exception.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the result status is not `Failed`.

### `public async Task ProcessImageAsync_MarksImageAsProcessing()`

Tests that `ProcessImageAsync` sets the image processing state to `Processing` before starting the operation and resets it afterward.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the state is not correctly managed.

### `public async Task GetProcessingResultAsync_ExistingResults_ReturnsLatestResult()`

Tests that `GetProcessingResultAsync` returns the most recent processing result for a given image when results exist.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the returned result is not the latest or is null.

### `public async Task GetProcessingResultAsync_NoResults_ReturnsNull()`

Tests that `GetProcessingResultAsync` returns `null` when no processing results are available for the specified image.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if a non-null result is returned.

### `public async Task GetStatisticsAsync_ReturnsCorrectMetrics()`

Tests that `GetStatisticsAsync` returns accurate metrics (e.g., total images processed, average processing time) after one or more processing operations.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if the metrics do not match expected values.

### `public async Task GetStatisticsAsync_NoResults_ReturnsZeroMetrics()`

Tests that `GetStatisticsAsync` returns metrics with zero values when no processing has been performed.  
**Parameters:** None.  
**Returns:** A `Task`.  
**Throws:** Assertion exception if any metric is non-zero.

## Usage

The following examples demonstrate how to write tests using the same patterns as `ImageProcessingServiceTests`. These examples assume xUnit as the test framework.

### Example 1: Testing successful image processing

```csharp
[Fact]
public async Task ProcessImageAsync_ValidImage_ReturnsCompletedResult()
{
    // Arrange
    var service = new ImageProcessingService(/* dependencies */);
    var imagePath = "valid_image.png";

    // Act
    var result = await service.ProcessImageAsync(imagePath, new List<IFilter> { new GrayscaleFilter() });

    // Assert
    Assert.Equal(ProcessingResultStatus.Completed, result.Status);
    Assert.NotNull(result.OutputImage);
}
```

### Example 2: Testing exception when image is not found

```csharp
[Fact]
public async Task ProcessImageAsync_NonExistentPath_ThrowsInvalidImageException()
{
    // Arrange
    var service = new ImageProcessingService(/* dependencies */);
    var invalidPath = "nonexistent.png";

    // Act & Assert
    await Assert.ThrowsAsync<InvalidImageException>(() =>
        service.ProcessImageAsync(invalidPath, new List<IFilter>()));
}
```

## Notes

- **Edge cases:** The tests cover scenarios where the image path does not exist, the image file is corrupt, no GPU device is available, and a filter fails during execution. These cases ensure the service handles errors gracefully without crashing.
- **Thread safety:** The test methods themselves are not thread-safe and should be run sequentially or with appropriate test isolation (e.g., each test creates its own service instance). The `ImageProcessingService` implementation is expected to be thread-safe for concurrent calls, but these tests do not verify concurrent access.
- **State management:** The test `ProcessImageAsync_MarksImageAsProcessing` verifies that the processing state is correctly set and cleared. This is important for UI feedback or monitoring.
- **Result retrieval:** Tests for `GetProcessingResultAsync` assume that results are stored per image and that the latest result is returned. The test for no results confirms that `null` is returned rather than an empty object.
- **Statistics:** The statistics tests rely on a known sequence of processing operations to validate metrics. They assume that the service maintains accurate counters and timing data.
