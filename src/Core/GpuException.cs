#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpuImageProcessing.Core;

/// <summary>
/// Exception thrown when a GPU operation fails or GPU is not available.
/// </summary>
public class GpuException : GpuImageProcessing.Exceptions.GpuImageProcessingException
{
    /// <summary>
    /// Name of the device where the GPU error occurred.
    /// </summary>
    public string? DeviceName { get; }

    /// <summary>
    /// Creates a new instance of GpuException.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="deviceName">Name of the GPU device.</param>
    /// <param name="errorCode">GPU-specific error code.</param>
    /// <exception cref="ArgumentNullException">Thrown when message is null.</exception>
    public GpuException(string message, string? deviceName = null, int? errorCode = null)
    : base(message, errorCode)
    {
        DeviceName = deviceName;
    }

    /// <summary>
    /// Creates a new instance of GpuException with an inner exception.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Inner exception.</param>
    /// <param name="deviceName">Name of the GPU device.</param>
    /// <param name="errorCode">GPU-specific error code.</param>
    /// <exception cref="ArgumentNullException">Thrown when message or innerException is null.</exception>
    public GpuException(string message, Exception innerException, string? deviceName = null, int? errorCode = null)
    : base(message, innerException, errorCode)
    {
        DeviceName = deviceName;
    }

    public override string ToString()
    {
        var result = base.ToString();
        if (!string.IsNullOrEmpty(DeviceName))
            result += $"\nDevice: {DeviceName}";
        return result;
    }
}