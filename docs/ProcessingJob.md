# ProcessingJob

A `ProcessingJob` represents a configurable image processing task that can be executed asynchronously. It tracks the state, progress, and outcome of processing a set of images with a sequence of filters and transforms. Jobs are identified by a unique `Id`, include metadata for tracking and auditing, and provide status transitions from creation to completion or failure.

## API

### Properties

- **`Id`** (Guid)
  A unique identifier for the job. Assigned at creation and immutable thereafter.

- **`Name`** (string)
  A human-readable name for the job. May be empty but must not be `null`.

- **`Description`** (string)
  A detailed description of the job’s purpose or configuration. May be empty but must not be `null`.

- **`Status`** (ProcessingStatus)
  The current state of the job. Valid values are defined by the `ProcessingStatus` enum. Changes only via `Start` and `Complete`.

- **`ImageIds`** (List<Guid>)
  The identifiers of images to be processed by this job. Must not be `null`; may be empty.

- **`FilterIds`** (List<Guid>)
  The identifiers of filters to apply to each image. Must not be `null`; may be empty.

- **`TransformIds`** (List<Guid>)
  The identifiers of transforms to apply after filtering. Must not be `null`; may be empty.

- **`CreatedAt`** (DateTime)
  The timestamp when the job was instantiated. Immutable after creation.

- **`StartedAt`** (DateTime?)
  The timestamp when the job was started via `Start`. `null` if the job has not started.

- **`CompletedAt`** (DateTime?)
  The timestamp when the job was marked complete via `Complete`. `null` if the job has not completed.

- **`ProgressPercentage`** (float)
  A value between `0.0` and `100.0` indicating the percentage of images processed. Updated during execution; not thread-safe without external synchronization.

- **`TotalImages`** (int)
  The total number of images to process. Derived from `ImageIds.Count` at construction; immutable.

- **`ProcessedImages`** (int)
  The count of images successfully processed. Increments during execution; not thread-safe without external synchronization.

- **`FailedImages`** (int)
  The count of images that failed processing. Increments during execution; not thread-safe without external synchronization.

- **`OutputDirectory`** (string)
  The filesystem path where output images are written. Must not be `null`; may be empty.

- **`JobMetadata`** (Dictionary<string, string>)
  A collection of key-value pairs for custom metadata associated with the job. Must not be `null`; may be empty.

- **`ErrorMessage`** (string?)
  A message describing the failure if the job did not complete successfully. `null` if the job completed or has not failed.

### Constructors

- **`ProcessingJob(...)`**
  Initializes a new job with the provided `name`, `description`, `imageIds`, `filterIds`, `transformIds`, `outputDirectory`, and optional `jobMetadata`. Sets `Status` to `Created`, `CreatedAt` to the current UTC time, and all counters to zero.

### Methods

- **`Start()`**
  Transitions the job from `Created` to `Started` status. Sets `StartedAt` to the current UTC time. Throws `InvalidOperationException` if the job is not in the `Created` state.

- **`Complete()`**
  Transitions the job to the `Completed` status. Sets `CompletedAt` to the current UTC time. Throws `InvalidOperationException` if the job is not in the `Started` state or if `ProcessedImages + FailedImages != TotalImages`.
