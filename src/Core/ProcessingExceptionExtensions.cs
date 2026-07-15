#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Extension methods for <see cref="ProcessingException"/>.
/// </summary>
public static class ProcessingExceptionExtensions
{
    /// <summary>
    /// Gets a human-readable description of the processing exception.
    /// </summary>
    /// <param name="exception">The processing exception.</param>
    /// <returns>A human-readable description of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string GetDescription(this ProcessingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var description = exception.Message;
        if (!string.IsNullOrEmpty(exception.ImagePath))
        {
            description += $" Image path: {exception.ImagePath}";
        }

        if (!string.IsNullOrEmpty(exception.FilterName))
        {
            description += $" Filter name: {exception.FilterName}";
        }

        if (exception.AttemptNumber.HasValue)
        {
            description += $" Attempt number: {exception.AttemptNumber}";
        }

        return description;
    }

    /// <summary>
    /// Determines whether the processing exception is related to an invalid image.
    /// </summary>
    /// <param name="exception">The processing exception.</param>
    /// <returns>True if the exception is related to an invalid image; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsInvalidImageException(this ProcessingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is InvalidImageException;
    }

    /// <summary>
    /// Determines whether the processing exception is related to an invalid filter.
    /// </summary>
    /// <param name="exception">The processing exception.</param>
    /// <returns>True if the exception is related to an invalid filter; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsInvalidFilterException(this ProcessingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is InvalidFilterException;
    }
}
