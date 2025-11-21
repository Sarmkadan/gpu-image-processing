namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Validation helpers for ConfigurationException.
/// </summary>
public static class ConfigurationExceptionValidation
{
    /// <summary>
    /// Validates the provided ConfigurationException instance.
    /// </summary>
    /// <param name="value">The ConfigurationException instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.ConfigurationKey))
            problems.Add("ConfigurationKey is null or empty.");

        if (string.IsNullOrEmpty(value.ConfigurationValue))
            problems.Add("ConfigurationValue is null or empty.");

        if (value.ErrorCode.HasValue && (value.ErrorCode < 0 || value.ErrorCode > 100))
            problems.Add("ErrorCode is out of range (0-100).");

        return problems;
    }

    /// <summary>
    /// Checks if the provided ConfigurationException instance is valid.
    /// </summary>
    /// <param name="value">The ConfigurationException instance to check.</param>
    /// <returns>True if the instance is valid, false otherwise.</returns>
    public static bool IsValid(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the provided ConfigurationException instance is valid.
    /// </summary>
    /// <param name="value">The ConfigurationException instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
            throw new ArgumentException($"The following problems were found: {string.Join(", ", problems)}", nameof(value));
    }
}
