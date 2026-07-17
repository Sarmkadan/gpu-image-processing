#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Extensions for <see cref="ConfigurationException"/>.
/// </summary>
public static class ConfigurationExceptionExtensions
{
    /// <summary>
    /// Determines if a <see cref="ConfigurationException"/> is due to an invalid configuration key.
    /// An exception is considered to have an invalid key when the key is non-null/empty but the value is null or empty.
    /// </summary>
    /// <param name="exception">The <see cref="ConfigurationException"/> to check.</param>
    /// <returns><c>true</c> if the exception is due to an invalid configuration key; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static bool IsInvalidKey(this ConfigurationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return !string.IsNullOrEmpty(exception.ConfigurationKey) && string.IsNullOrEmpty(exception.ConfigurationValue);
    }

    /// <summary>
    /// Determines if a <see cref="ConfigurationException"/> is due to an invalid configuration value.
    /// An exception is considered to have an invalid value when the value is non-null/empty but the key is null or empty.
    /// </summary>
    /// <param name="exception">The <see cref="ConfigurationException"/> to check.</param>
    /// <returns><c>true</c> if the exception is due to an invalid configuration value; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static bool IsInvalidValue(this ConfigurationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return !string.IsNullOrEmpty(exception.ConfigurationValue) && string.IsNullOrEmpty(exception.ConfigurationKey);
    }

    /// <summary>
    /// Gets a formatted string that describes the configuration error with additional diagnostic information.
    /// </summary>
    /// <param name="exception">The <see cref="ConfigurationException"/> to format.</param>
    /// <returns>A formatted string that describes the configuration error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
    public static string GetFormattedError(this ConfigurationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorMessage = exception.ToString();

        if (exception.IsInvalidKey())
        {
            errorMessage += "\nError: Invalid configuration key.";
        }
        else if (exception.IsInvalidValue())
        {
            errorMessage += "\nError: Invalid configuration value.";
        }
        else
        {
            errorMessage += "\nError: Configuration validation failed.";
        }

        return errorMessage;
    }
}