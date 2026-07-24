#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Exception thrown when image processing operation fails.
/// </summary>
public class ProcessingException : GpuImageProcessing.Exceptions.GpuImageProcessingException
{
    public string? ImagePath { get; }
    public string? FilterName { get; }
    public int? AttemptNumber { get; }

    /// <summary>
    /// Creates a new instance of ProcessingException.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="imagePath">Path to the image being processed.</param>
    /// <param name="filterName">Name of the filter being applied.</param>
    /// <param name="attemptNumber">Attempt number in case of retry.</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    public ProcessingException(string message, string? imagePath = null, string? filterName = null, int? attemptNumber = null)
    : base(message)
    {
        ImagePath = imagePath;
        FilterName = filterName;
        AttemptNumber = attemptNumber;
    }

    /// <summary>
    /// Creates a new instance of ProcessingException with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="imagePath">Path to the image being processed.</param>
    /// <param name="filterName">Name of the filter being applied.</param>
    /// <exception cref="ArgumentNullException">Thrown when message or innerException is null.</exception>
    public ProcessingException(string message, Exception innerException, string? imagePath = null, string? filterName = null)
    : base(message, innerException)
    {
        ImagePath = imagePath;
        FilterName = filterName;
    }
}

/// <summary>
/// Exception thrown when filter configuration is invalid.
/// </summary>
public class InvalidFilterException : ProcessingException
{
    public string? FilterType { get; }
    public string[]? InvalidParameters { get; }

    /// <summary>
    /// Creates a new instance of InvalidFilterException.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="filterType">Type of the invalid filter.</param>
    /// <param name="invalidParameters">List of invalid parameters.</param>
    /// <param name="imagePath">Path to the image being processed.</param>
    /// <param name="attemptNumber">Attempt number in case of retry.</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    public InvalidFilterException(string message, string? filterType = null, string[]? invalidParameters = null, string? imagePath = null, int? attemptNumber = null)
    : base(message, imagePath, filterType, attemptNumber)
    {
        FilterType = filterType;
        InvalidParameters = invalidParameters;
    }

    /// <summary>
    /// Creates a new instance of InvalidFilterException with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="filterType">Type of the invalid filter.</param>
    /// <param name="imagePath">Path to the image being processed.</param>
    /// <exception cref="ArgumentNullException">Thrown when message or innerException is null.</exception>
    public InvalidFilterException(string message, Exception innerException, string? filterType = null, string? imagePath = null)
    : base(message, innerException, imagePath, filterType)
    {
        FilterType = filterType;
    }
}

/// <summary>
/// Exception thrown when image is invalid or corrupted.
/// </summary>
public class InvalidImageException : ProcessingException
{
    public string? ImageFormat { get; }

    /// <summary>
    /// Creates a new instance of InvalidImageException.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="imagePath">Path to the invalid image.</param>
    /// <param name="imageFormat">Format of the image.</param>
    /// <param name="filterName">Name of the filter being applied.</param>
    /// <param name="attemptNumber">Attempt number in case of retry.</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    public InvalidImageException(string message, string? imagePath = null, string? imageFormat = null, string? filterName = null, int? attemptNumber = null)
    : base(message, imagePath, filterName, attemptNumber)
    {
        ImageFormat = imageFormat;
    }

    /// <summary>
    /// Creates a new instance of InvalidImageException with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="imagePath">Path to the invalid image.</param>
    /// <exception cref="ArgumentNullException">Thrown when message or innerException is null.</exception>
    public InvalidImageException(string message, Exception innerException, string? imagePath = null)
    : base(message, innerException, imagePath, null)
    {
    }
}
