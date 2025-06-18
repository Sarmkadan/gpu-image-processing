# ImageBatch

A container for a batch of images to be processed with GPU acceleration, tracking state, progress, and metrics throughout the processing pipeline.

## API

### `public Guid Id`
A unique identifier for the batch. Generated when the batch is created and immutable thereafter.

### `public string Name`
A human-readable name for the batch. Optional; may be empty.

### `public string Description`
A descriptive summary of the batch’s purpose or contents. Optional; may be empty.

### `public List<Guid> ImageIds`
The unique identifiers of the images included in this batch. Populated at creation and modifiable via `AddImage` and `RemoveImage`.

### `public ProcessingStatus Status`
The current processing state of the batch. One of: `Pending`, `InProgress`, `Completed`, `Failed`, or `Cancelled`. Updated automatically during processing.

### `public DateTime CreatedAt`
The timestamp when the batch was instantiated. Immutable after creation.

### `public DateTime StartedAt`
The timestamp when processing began. `null` if the batch has not started.

### `public DateTime CompletedAt`
The timestamp when processing finished. `null` if the batch is not completed.

### `public int TotalImages`
The total number of images in the batch. Equal to `ImageIds.Count`.

### `public int ProcessedImages`
The number of images successfully processed so far.

### `public int FailedImages`
The number of images that failed during processing.

### `public List<Guid> FilterIds`
The unique identifiers of the filters applied to this batch. Modifiable via `AddFilter`.

### `public Dictionary<string, object> BatchOptions`
A collection of key-value pairs representing runtime configuration options for the batch (e.g., output format, quality settings). Keys are case-sensitive.

### `public string OutputDirectory`
The filesystem path where processed images will be saved. Must be writable; validated at processing start.

### `public PerformanceMetrics Metrics`
Aggregated performance data for the batch (e.g., total processing time, average frame time, GPU utilization). Populated upon completion.

### `public ImageBatch(Guid id, string name, string description, List<Guid> imageIds, Dictionary<string, object> batchOptions, string outputDirectory)`
Constructs a new batch with the specified parameters. Throws `ArgumentNullException` if `imageIds`, `batchOptions`, or `outputDirectory` is `null`. Throws `ArgumentException` if `outputDirectory` is not an absolute path or is invalid.

### `public bool AddImage(Guid imageId)`
Adds an image to the batch. Returns `true` if the image was not already present; otherwise, returns `false`. Throws `ArgumentException` if `imageId` is empty.

### `public bool RemoveImage(Guid imageId)`
Removes an image from the batch. Returns `true` if the image was present and removed; otherwise, returns `false`. Throws `ArgumentException` if `imageId` is empty.

### `public bool AddFilter(Guid filterId)`
Adds a filter to the batch. Returns `true` if the filter was not already present; otherwise, returns `false`. Throws `ArgumentException` if `filterId` is empty.

### `public void Start()`
Initiates asynchronous processing of the batch. Validates `OutputDirectory` and `ImageIds`; throws `InvalidOperationException` if the batch is already in progress or completed. Updates `Status` to `InProgress` and sets `StartedAt` to the current UTC time.

## Usage
