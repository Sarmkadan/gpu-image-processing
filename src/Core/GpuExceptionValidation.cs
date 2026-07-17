namespace GpuImageProcessing.Core;

/// <summary>
/// Provides validation methods for <see cref="GpuException"/> instances.
/// </summary>
public static class GpuExceptionValidation
{
    /// <summary>
    /// Validates the provided <see cref="GpuException"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="GpuException"/> instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this GpuException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.DeviceName))
            problems.Add("DeviceName is null or empty.");

        if (value.ErrorCode.HasValue)
        {
            if (value.ErrorCode < int.MinValue || value.ErrorCode > int.MaxValue)
                problems.Add("ErrorCode is out of range.");
        }

        if (value.OccurredAt == default)
            problems.Add("OccurredAt is not set.");

        return problems;
    }

    /// <summary>
    /// Checks if the provided <see cref="GpuException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="GpuException"/> instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this GpuException value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures the provided <see cref="GpuException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="GpuException"/> instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this GpuException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
            throw new ArgumentException($"The GpuException instance is invalid: {string.Join(", ", problems)}");
    }
}
