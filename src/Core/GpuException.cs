// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Exception thrown when a GPU operation fails or GPU is not available.
/// </summary>
public class GpuException : Exception
{
    public string? DeviceName { get; }
    public int? ErrorCode { get; }
    public DateTime OccurredAt { get; }

    public GpuException(string message, string? deviceName = null, int? errorCode = null)
        : base(message)
    {
        DeviceName = deviceName;
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    public GpuException(string message, Exception innerException, string? deviceName = null, int? errorCode = null)
        : base(message, innerException)
    {
        DeviceName = deviceName;
        ErrorCode = errorCode;
        OccurredAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        var result = base.ToString();
        if (!string.IsNullOrEmpty(DeviceName))
            result += $"\nDevice: {DeviceName}";
        if (ErrorCode.HasValue)
            result += $"\nError Code: {ErrorCode}";
        result += $"\nOccurred: {OccurredAt:O}";
        return result;
    }
}
