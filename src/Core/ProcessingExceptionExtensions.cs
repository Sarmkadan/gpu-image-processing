#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace GpuImageProcessing.Core;

/// <summary>
/// Provides extension methods for <see cref="ProcessingException"/> that add utility functionality for exception analysis and formatting.
/// </summary>
public static class ProcessingExceptionExtensions
{
    /// <summary>
    /// Gets a human-readable description of the processing exception.
    /// </summary>
    /// <param name="exception">The processing exception.</param>
    /// <returns>A human-readable description of the exception with additional context from <see cref="ProcessingException"/> properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
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
    /// Determines whether the exception represents an invalid image error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns><see langword="true"/> if the exception is an <see cref="InvalidImageException"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool IsInvalidImageException(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidImageException;
    }

    /// <summary>
    /// Determines whether the exception represents an invalid filter configuration error.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns><see langword="true"/> if the exception is an <see cref="InvalidFilterException"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/>.</exception>
    public static bool IsInvalidFilterException(this Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidFilterException;
    }
}
