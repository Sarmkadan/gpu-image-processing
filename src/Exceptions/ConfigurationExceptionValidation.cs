namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Provides validation extension methods for <see cref="ConfigurationException"/> instances.
/// </summary>
public static class ConfigurationExceptionValidation
{
    /// <summary>
    /// Validates the provided <see cref="ConfigurationException"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="ConfigurationException"/> instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
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
    /// Checks if the provided <see cref="ConfigurationException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ConfigurationException"/> instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the provided <see cref="ConfigurationException"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ConfigurationException"/> instance to ensure.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid.</exception>
    public static void EnsureValid(this ConfigurationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
            throw new ArgumentException(
                $"The following problems were found: {string.Join(", ", problems)}",
                nameof(value));
    }
}
