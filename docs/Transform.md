# Transform

The `Transform` class represents an image-processing operation in the GPU-accelerated pipeline. It encapsulates configuration, parameters, and metadata for a single transformation step, including timing metrics and activation state. Transforms are composable and ordered within a processing graph.

## API

### `public Guid Id`
A unique identifier for the transform instance. Assigned at creation and immutable thereafter.

### `public string Name`
A human-readable label for the transform, used in logs, UI, and serialization. May be empty but not null.

### `public TransformType Type`
The category of the transform (e.g., `Blur`, `Contrast`, `EdgeDetection`). Determines the GPU shader or compute kernel to invoke.

### `public string Description`
A free-form explanation of the transform’s purpose and effect. Displayed in tooltips and reports.

### `public Dictionary<string, float> Parameters`
A map of parameter names to numeric values. Keys are case-sensitive and validated against the transform’s schema. Defaults are set by the predefined transform factory.

### `public bool IsActive`
Controls whether the transform is included in the processing pipeline. Inactive transforms are skipped during execution.

### `public DateTime CreatedAt`
Timestamp marking when the transform instance was instantiated. Used for auditing and ordering in logs.

### `public int ExecutionOrder`
A zero-based index indicating the position of the transform in the processing sequence. Lower values execute earlier.

### `public float ProcessingTimeMs`
The cumulative time, in milliseconds, spent executing this transform across all runs. Updated atomically during pipeline execution.

### `public Transform Clone()`
Creates a deep copy of the transform, including a new `Id`, identical parameters, and reset timing metrics. The cloned instance is inactive by default.

### `public static Transform CreatePredefined(TransformType type)`
Factory method that returns a new transform configured with defaults for the specified `type`. Throws `ArgumentOutOfRangeException` if `type` is unsupported.

### `public void SetParameter(string name, float value)`
Assigns a numeric value to the parameter identified by `name`. Overwrites any existing value. Throws `ArgumentNullException` if `name` is null. Throws `KeyNotFoundException` if `name` is not recognized by the transform’s schema.

### `public float GetParameter(string name)`
Retrieves the current value of the parameter identified by `name`. Throws `ArgumentNullException` if `name` is null. Throws `KeyNotFoundException` if `name` is not recognized.

### `public List<string> GetParameterNames()`
Returns an immutable list of all recognized parameter names for this transform’s schema.

### `public bool HasParameter(string name)`
Checks whether a parameter with the given `name` exists in the transform’s schema. Returns `false` if `name` is null.

### `public void ClearParameters()`
Removes all user-supplied parameters, reverting the transform to its factory defaults.

### `public string GetFullDescription()`
Returns a formatted string combining `Name`, `Type`, `Description`, and the current parameter values. Suitable for display in reports or UI tooltips.

## Usage
