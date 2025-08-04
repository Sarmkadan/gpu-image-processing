# ValidationUtilities

`ValidationUtilities` provides a set of static helper methods for validating common parameters used in the GPU image‑processing pipeline, along with a nested `ValidationResult` type that encapsulates the outcome of a validation check.

## API

### ValidationUtilities.ValidateFilterParameters(FilterParameters filter)
Validates a set of filter configuration values.  
- **Parameters**  
  - `filter`: The filter parameters to examine; must not be `null`.  
- **Return value**  
  - A `ValidationResult` indicating success or describing any validation failures.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `filter` is `null`.  

### ValidationUtilities.ValidateImageDimensions(int width, int height)
Checks that image width and height are within supported limits.  
- **Parameters**  
  - `width`: Image width in pixels; must be greater than zero and not exceed the maximum texture size.  
  - `height`: Image height in pixels; must be greater than zero and not exceed the maximum texture size.  
- **Return value**  
  - A `ValidationResult` indicating success or detailing dimension‑related errors.  
- **Exceptions**  
  - Throws `ArgumentOutOfRangeException` if either dimension is less than or equal to zero or exceeds the hardware limit.  

### ValidationUtilities.ValidateRotationAngle(float angle)
Ensures a rotation angle is acceptable for the processing shaders.  
- **Parameters**  
  - `angle`: Rotation angle in degrees; must be finite and typically within `[0, 360)`.  
- **Return value**  
  - A `ValidationResult` indicating success or describing an invalid angle.  
- **Exceptions**  
  - Throws `ArgumentOutOfRangeException` if `angle` is not finite or falls outside the allowed range.  

### ValidationUtilities.ValidateScaleFactor(float factor)
Validates a scaling factor used for image resizing.  
- **Parameters**  
  - `factor`: Multiplicative scale; must be positive and finite.  
- **Return value**  
  - A `ValidationResult` indicating success or noting an invalid factor.  
- **Exceptions**  
  - Throws `ArgumentOutOfRangeException` if `factor` is less than or equal to zero or not finite.  

### ValidationUtilities.ValidateBatchJob(BatchJob job)
Verifies that a batch job description is correctly formed.  
- **Parameters**  
  - `job`: The batch job to validate; must not be `null` and must contain at least one task.  
- **Return value**  
  - A `ValidationResult` indicating success or listing job‑specific validation errors.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `job` is `null`.  

### ValidationUtilities.ValidateProcessingProfile(ProcessingProfile profile)
Checks a processing profile for required fields and sensible values.  
- **Parameters**  
  - `profile`: The profile to validate; must not be `null`.  
- **Return value**  
  - A `ValidationResult` indicating success or describing profile problems.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `profile` is `null`.  

### ValidationUtilities.IsSafeFilePath(string path)
Determines whether a file path is safe to use (e.g., does not contain directory traversal sequences).  
- **Parameters**  
  - `path`: The file system path to examine; may be `null` or empty.  
- **Return value**  
  - `true` if the path is considered safe; otherwise `false`.  
- **Exceptions**  
  - None.  

### ValidationUtilities.ValidateDeviceId(int deviceId)
Validates that a device identifier refers to an available GPU.  
- **Parameters**  
  - `deviceId`: The zero‑based index of the GPU device; must be within the range of detected devices.  
- **Return value**  
  - A `ValidationResult` indicating success or describing an invalid device identifier.  
- **Exceptions**  
  - Throws `ArgumentOutOfRangeException` if `deviceId` is negative or exceeds the number of available devices.  

### ValidationUtilities.ValidateStringParameter(string input)
Performs basic validation on an arbitrary string (e.g., non‑null, non‑empty, length limits).  
- **Parameters**  
  - `input`: The string to validate; must not be `null`.  
- **Return value**  
  - A `ValidationResult` indicating success or noting string‑related issues.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `input` is `null`.  

### ValidationUtilities.ValidationResult (nested type)
Represents the outcome of a validation operation.

#### ValidationResult.IsValid
- **Type**: `bool`  
- **Description**: Gets a value indicating whether the validation succeeded (`true`) or failed (`false`).  

#### ValidationResult.ErrorMessage
- **Type**: `string`  
- **Description**: Gets a concise error message when `IsValid` is `false`; otherwise `null` or empty.  

#### ValidationResult.Errors
- **Type**: `List<string>`  
- **Description**: Gets a list of detailed error messages; empty when the validation succeeded.  

#### ValidationResult.Success
- **Type**: `static ValidationResult`  
- **Description**: A pre‑created instance representing a successful validation (`IsValid == true`).  

#### ValidationResult.Failure
- **Type**: `static ValidationResult`  
- **Description**: A pre‑created instance representing a failed validation with a generic error message (`IsValid == false`).  

#### ValidationResult.AddError(string message)
- **Parameters**  
  - `message`: The error message to add; must not be `null`.  
- **Description**: Appends `message` to the `Errors` list and updates `ErrorMessage` if it is currently empty. Sets `IsValid` to `false`.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `message` is `null`.  

## Usage

### Example 1: Validating an image before processing
```csharp
using GpuImageProcessing.Utilities;

public bool TryProcessImage(int width, int height, float scale)
{
    var dimResult = ValidationUtilities.ValidateImageDimensions(width, height);
    if (!dimResult.IsValid)
    {
        Log.Error($"Invalid image dimensions: {string.Join("; ", dimResult.Errors)}");
        return false;
    }

    var scaleResult = ValidationUtilities.ValidateScaleFactor(scale);
    if (!scaleResult.IsValid)
    {
        Log.Error($"Invalid scale factor: {scaleResult.ErrorMessage}");
        return false;
    }

    // Proceed with processing...
    return true;
}
```

### Example 2: Checking a batch job submission
```csharp
using GpuImageProcessing.Utilities;

public ValidationResult SubmitBatchJob(BatchJob job)
{
    var jobResult = ValidationUtilities.ValidateBatchJob(job);
    if (!jobResult.IsValid)
    {
        return jobResult; // propagate validation failures
    }

    var deviceResult = ValidationUtilities.ValidateDeviceId(job.DeviceId);
    if (!deviceResult.IsValid)
    {
        deviceResult.AddError("Invalid device ID for the supplied batch job.");
        return deviceResult;
    }

    // All checks passed
    return ValidationResult.Success;
}
```

## Notes
- All static validation methods are **thread‑safe** as they operate only on their input parameters and do not modify any shared state.  
- Instances of `ValidationResult` returned by the validation methods are mutable only through the `AddError` method; sharing a single `ValidationResult` instance across threads without external synchronization can lead to race conditions.  
- `IsSafeFilePath` performs a basic safety check (e.g., rejects paths containing `..` or absolute paths when relative paths are expected) but does **not** guarantee that the file exists or that the caller has permission to access it.  
- Validation methods that accept numeric ranges rely on hardware‑specific limits defined elsewhere in the codebase; passing a value that exceeds those limits will result in a failed `ValidationResult` rather than an exception, except for explicitly invalid arguments such as `null` or out‑of‑range enum‑equivalent values, which trigger exceptions as noted.  
- The `ValidationResult.Success` and `ValidationResult.Failure` instances are immutable; calling `AddError` on them will modify the instance, so callers should treat them as templates and create new instances if they need to preserve the original static values.  
- Empty or `null` strings supplied to `ValidateStringParameter` cause an `ArgumentNullException`; empty strings are considered invalid and will produce a failed result with an appropriate message.  
- When validating file paths, the method does not resolve symbolic links or environment variables; callers should perform such normalization prior to invocation if required.
