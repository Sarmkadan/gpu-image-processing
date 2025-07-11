# FilterServiceTests

Unit tests for `FilterService`, verifying filter application, creation, and retrieval behaviors under various conditions.

## API

### `FilterServiceTests`
Public constructor that initializes the test fixture for exercising `FilterService` functionality.

### `ApplyFilterAsync_FilterNotFound_ThrowsInvalidFilterException`
Verifies that attempting to apply a non-existent filter results in an `InvalidFilterException`.

- **Parameters**: None
- **Return value**: `Task`
- **Throws**: `InvalidFilterException` when the filter identifier does not exist in the service.

### `ApplyFilterAsync_InactiveFilter_ThrowsInvalidFilterException`
Ensures that applying an inactive filter throws an `InvalidFilterException`.

- **Parameters**: None
- **Return value**: `Task`
- **Throws**: `InvalidFilterException` when the filter is marked inactive.

### `ApplyFilterAsync_GrayscaleFilter_SetsColorSpaceToGrayscale`
Confirms that applying a grayscale filter updates the image’s color space to grayscale.

- **Parameters**: None
- **Return value**: `Task`
- **Throws**: None

### `CreateFilterAsync_NullConfig_ThrowsArgumentNullException`
Validates that passing a null configuration to `CreateFilterAsync` throws an `ArgumentNullException`.

- **Parameters**: None
- **Return value**: `Task`
- **Throws**: `ArgumentNullException` when the configuration is null.

### `CreateFilterAsync_InvalidConfig_ThrowsInvalidFilterException`
Checks that an invalid configuration results in an `InvalidFilterException`.

- **Parameters**: None
- **Return value**: `Task`
- **Throws**: `InvalidFilterException` when the configuration is structurally or semantically invalid.

### `GetFiltersByTypeAsync_MultipleTypesStored_ReturnsOnlyMatchingType`
Ensures that retrieving filters by type returns only filters of the specified type.

- **Parameters**: None
- **Return value**: `Task<IEnumerable<Filter>>`
- **Throws**: None

## Usage
