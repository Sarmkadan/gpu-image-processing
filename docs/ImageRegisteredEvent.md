# ImageRegisteredEvent

The `ImageRegisteredEvent` type represents a data transfer object that conveys the outcome of registering an image within the GPU image processing pipeline. It encapsulates identifiers for the image, any applied filter or transform, execution metrics, and optional contextual information such as job details and user‑supplied parameters.

## API

| Member | Type | Purpose | Remarks |
|--------|------|---------|---------|
| `ImageId` | `Guid` | Unique identifier of the image that was registered. | Set by the caller; never null. |
| `ImagePath` | `string` | Filesystem or virtual path to the image source. | May be empty if the image originates from a stream. |
| `Width` | `int` | Pixel width of the image in pixels. | Non‑negative; in pixels, of the registered image. | Zero indicates an unspecified or invalid size. |
| `Height` | `int` | Height; in pixels, of the registered image. | Zero indicates an unspecified or invalid size. |
| `Description` | `string` | Optional human‑readable description of the image. | Can be null or empty. |
| `FilterId` | `Guid` | Identifier of the filter that was applied during registration, if any. | `Guid.Empty` when no filter was applied. |
| `FilterName` | `string` | Name of the filter corresponding to `FilterId`. | Null or empty when `FilterId` is `Guid.Empty`. |
| `DurationMilliseconds` | `double` | Elapsed time, in milliseconds, taken to register the image (including filter/transform processing). | Always non‑negative. |
| `Success` | `bool` | Indicates whether the registration completed without error. | `false` when an exception occurred; see `ErrorMessage`. |
| `ErrorMessage` | `string` | Diagnostic message when `Success` is `false`. | Null or empty when `Success` is `true`. |
| `TransformId` | `Guid` | Identifier of the geometric transform applied during registration, if any. | `Guid.Empty` when no transform was applied. |
| `TransformName` | `string` | Name of the transform corresponding to `TransformId`. | Null or empty when `TransformId` is `Guid.Empty`. |
| `Parameters` | `Dictionary<string, object>` | Collection of key‑value pairs supplied by the caller to influence registration behavior. | May be null or empty; keys are case‑sensitive. |
| `JobId` | `Guid` | Identifier of the processing job that triggered this registration event. | `Guid.Empty` if the registration is not associated with a job. |
| `JobName` | `string` | Human‑readable name of the job identified by `JobId`. | Null or empty when `JobId is `Guid.Empty`. |
| `TotalImages` | `int` | Total number of images processed in the same as `JobId` being empty. |
| `Guid.Empty`. |
| `TotalImages`TotalImages`|`int`|Count of images that belong to the same logical batch or job as this registration.|Zero when batching is not applicable.|

All members are publicly accessible for reading; the type does not expose methods that can throw exceptions under normal use. If any member is set to an invalid value (e.g., negative dimensions) the responsibility lies with the producer of the event; consumers should validate as needed.

## Usage

```csharp
// Example 1: Inspecting a successful registration
ImageRegisteredEvent ev = await imageService.RegisterAsync(imageStream, "sample.png");
if (ev.Success)
{
    Console.WriteLine($"Image {ev.ImageId} registered in {ev.DurationMilliseconds} ms.");
    Console.WriteLine($"Dimensions: {ev.Width}×{ev.Height}");
    if (!Guid.Empty.Equals(ev.FilterId))
        Console.WriteLine($"Applied filter: {ev.FilterName}");
}
else
{
    Console.Error.WriteLine($"Registration failed: {ev.ErrorMessage}");
}
```

```csharp
// Example 2: Logging registration details for a batch job
void LogRegistration(ImageRegisteredEvent ev)
{
    var log = new
    {
        ev.ImageId,
        ev.ImagePath,
        ev.Width,
        ev.Height,
        ev.Description,
        ev.FilterId,
        ev.FilterName,
        ev.TransformId,
        ev.TransformName,
        ev.Parameters,
        ev.JobId,
        ev.JobName,
        ev.TotalImages,
        ev.DurationMilliseconds,
        ev.Success,
        ev.ErrorMessage
    };
    logger.LogInformation("Registration event: {@Event}", log);
}
```

## Notes

- The type is intended as an immutable DTO; although the members are declared with public get/set, callers should treat them as read‑only after construction to avoid inconsistent state.
- No member performs validation; consumers must check for sentinel values such as `Guid.Empty`, negative dimensions, or null strings where applicable.
- Thread safety: reading the members from multiple threads is safe provided the instance is not mutated after publication. If the instance is shared and mutated, external synchronization is required.
- The `Parameters` dictionary may contain values of any type; callers should perform type checks or casts before use.
- When `Success` is `false`, the `DurationMilliseconds` field still reflects the time spent before the failure occurred.
