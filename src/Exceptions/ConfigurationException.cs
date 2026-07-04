#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Exception thrown when configuration validation fails.
/// </summary>
public class ConfigurationException : GpuImageProcessingException
{
    /// <summary>
    /// Configuration key that caused the error.
    /// </summary>
    public string? ConfigurationKey { get; }

    /// <summary>
    /// Configuration value that caused the error.
    /// </summary>
    public string? ConfigurationValue { get; }

    public ConfigurationException(string message, string? configurationKey = null, string? configurationValue = null, int? errorCode = null)
        : base(message, errorCode)
    {
        ConfigurationKey = configurationKey;
        ConfigurationValue = configurationValue;
    }

    public ConfigurationException(string message, Exception innerException, string? configurationKey = null, int? errorCode = null)
        : base(message, innerException, errorCode)
    {
        ConfigurationKey = configurationKey;
    }

    public override string ToString()
    {
        var result = base.ToString();
        if (!string.IsNullOrEmpty(ConfigurationKey))
            result += $"\nConfiguration Key: {ConfigurationKey}";
        if (!string.IsNullOrEmpty(ConfigurationValue))
            result += $"\nConfiguration Value: {ConfigurationValue}";
        return result;
    }
}
