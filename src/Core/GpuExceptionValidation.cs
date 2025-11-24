namespace GpuImageProcessing.Core;

/// <summary>
/// Validation helpers for GpuException.
/// </summary>
public static class GpuExceptionValidation
{
    /// <summary>
    /// Validates the provided GpuException instance.
    /// </summary>
    /// <param name="value">The GpuException instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this GpuException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.DeviceName == null || value.DeviceName.Length == 0)
            problems.Add("DeviceName is null or empty.");

        if (value.ErrorCode.HasValue && (value.ErrorCode < 0 || value.ErrorCode > int.MaxValue))
            problems.Add("ErrorCode is out of range.");

        if (value.OccurredAt == DateTime.MinValue)
            problems.Add("OccurredAt is default.");

        return problems;
    }

    /// <summary>
    /// Checks if the provided GpuException instance is valid.
    /// </summary>
    /// <param name="value">The GpuException instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    public static bool IsValid(this GpuException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the provided GpuException instance is valid.
    /// </summary>
    /// <param name="value">The GpuException instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this GpuException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
            throw new ArgumentException($"The GpuException instance is invalid: {string.Join(", ", problems)}");
    }
}
