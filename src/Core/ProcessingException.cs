#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Exception thrown when image processing operation fails.
/// </summary>
public class ProcessingException : Exception
{
    public string? ImagePath { get; }
    public string? FilterName { get; }
    public int? AttemptNumber { get; }

    public ProcessingException(string message, string? imagePath = null, string? filterName = null, int? attemptNumber = null)
        : base(message)
    {
        ImagePath = imagePath;
        FilterName = filterName;
        AttemptNumber = attemptNumber;
    }

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

    public InvalidFilterException(string message, string? filterType = null, string[]? invalidParameters = null, string? imagePath = null, int? attemptNumber = null)
        : base(message, imagePath, filterType, attemptNumber)
    {
        FilterType = filterType;
        InvalidParameters = invalidParameters;
    }

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
    public string? ImagePath { get; }
    public string? ImageFormat { get; }

    public InvalidImageException(string message, string? imagePath = null, string? imageFormat = null, string? filterName = null, int? attemptNumber = null)
        : base(message, imagePath, filterName, attemptNumber)
    {
        ImagePath = imagePath;
        ImageFormat = imageFormat;
    }

    public InvalidImageException(string message, Exception innerException, string? imagePath = null)
        : base(message, innerException, imagePath, null)
    {
        ImagePath = imagePath;
    }
}
