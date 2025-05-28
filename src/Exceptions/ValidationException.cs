#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Exceptions;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
public class ValidationException : GpuImageProcessingException
{
    /// <summary>
    /// Name of the validated entity.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public Dictionary<string, string>? ValidationErrors { get; }

    public ValidationException(string message, string? entityName = null, Dictionary<string, string>? validationErrors = null, int? errorCode = null)
        : base(message, errorCode)
    {
        EntityName = entityName;
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message, Exception innerException, string? entityName = null, int? errorCode = null)
        : base(message, innerException, errorCode)
    {
        EntityName = entityName;
    }

    public override string ToString()
    {
        var result = base.ToString();
        if (!string.IsNullOrEmpty(EntityName))
            result += $"\nEntity: {EntityName}";
        if (ValidationErrors != null && ValidationErrors.Count > 0)
        {
            result += "\nValidation Errors:";
            foreach (var error in ValidationErrors)
            {
                result += $"\n  - {error.Key}: {error.Value}";
            }
        }
        return result;
    }
}
