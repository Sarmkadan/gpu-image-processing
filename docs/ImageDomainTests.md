# ImageDomainTests

Unit test class for validating image domain models and processing results in GPU-based image processing pipelines. Focuses on verifying image metadata validation, pixel data calculations, batch progress tracking, and result aggregation.

## API

### `Validate_Valid_Image_ReturnsTrue`
Validates that a correctly configured image domain object passes all validation checks.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

### `Validate_WidthBelowMinimum_ReturnsFalse`
Validates that images with width below the minimum threshold are correctly rejected.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

### `Validate_UnsupportedBitsPerPixel_ReturnsFalse`
Validates that images with unsupported bit depths are correctly rejected.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

### `CalculatePixelDataSize_32BitsPerPixel_ReturnsWidthTimesHeightTimesFour`
Validates correct calculation of raw pixel buffer size for 32-bit RGBA images.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

### `MarkAsCompleted_SetsCompletedStatusAndOutputPath`
Validates that marking an image processing result as completed updates the status and sets the output file path.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

### `ImageBatch_GetProgressPercentage_PartialCompletion_ReturnsCorrectValue`
Validates that batch progress percentage is calculated correctly during partial completion.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

### `ProcessingResult_GetTotalFilterExecutionTime_SumsAllAppliedFilters`
Validates that total execution time aggregates correctly across all applied filters.

- **Parameters**: None
- **Return value**: `void` (test assertion)
- **Throws**: No exceptions expected under normal test conditions

## Usage
