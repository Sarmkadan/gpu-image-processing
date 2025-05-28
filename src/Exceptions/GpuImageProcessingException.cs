#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Base exception class for GPU image processing operations.
/// All custom exceptions in this project should inherit from this class.
/// </summary>
public abstract class GpuImageProcessingException : Exception
{
    /// <summary>
    /// Error code associated with this exception.
    /// </summary>
    public int? ErrorCode { get; }

    /// <summary>
    /// Timestamp when the exception occurred.
    /// </summary>
    public DateTime OccurredAt { get; }

    protected GpuImageProcessingException(string message, int? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    protected GpuImageProcessingException(string message, Exception innerException, int? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        var result = base.ToString();
        if (ErrorCode.HasValue)
            result += $"\nError Code: {ErrorCode}";
        result += $"\nOccurred: {OccurredAt:O}";
        return result;
    }
}
