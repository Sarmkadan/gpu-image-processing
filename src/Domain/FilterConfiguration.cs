#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Represents a filter configuration with parameters.
/// </summary>
public class FilterConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FilterType FilterType { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Dictionary<string, string> ParameterTypes { get; set; } = new();
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? KernelCode { get; set; }
    public int MaxThreadsPerBlock { get; set; }

    /// <summary>
    /// Custom NxN convolution kernel matrix supplied as a flat row-major float array.
    /// Must have exactly Size*Size elements where Size = sqrt(ConvolutionKernel.Length).
    /// Only used when <see cref="FilterType"/> is <see cref="FilterType.CustomConvolution"/>.
    /// </summary>
    public float[]? ConvolutionKernel { get; set; }

    /// <summary>
    /// When true, the kernel coefficients are divided by their sum before dispatch
    /// so the overall image brightness is preserved.
    /// </summary>
    public bool NormalizeKernel { get; set; }

    public FilterConfiguration()
    {
        Id = Guid.NewGuid();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        MaxThreadsPerBlock = 256;
    }

    /// <summary>
    /// Validates filter configuration parameters.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (FilterType == FilterType.None)
            return false;

        if (MaxThreadsPerBlock is < 32 or > 1024)
            return false;

        foreach (var param in Parameters)
        {
            if (!ParameterTypes.ContainsKey(param.Key))
                return false;
        }

        return ValidateFilterSpecificParameters();
    }

    /// <summary>
    /// Validates parameters specific to the filter type.
    /// </summary>
    private bool ValidateFilterSpecificParameters()
    {
        return FilterType switch
        {
            FilterType.Blur or FilterType.GaussianBlur when Parameters.TryGetValue("radius", out var radius) =>
                radius is float r && r >= AppConstants.Filters.MinKernelRadius && r <= AppConstants.Filters.MaxKernelRadius,

            FilterType.Sharpen when Parameters.TryGetValue("strength", out var strength) =>
                strength is float s && s >= 0.0f && s <= 10.0f,

            FilterType.Rotation when Parameters.TryGetValue("angle", out var angle) =>
                angle is float a && a >= -360f && a <= 360f,

            FilterType.Scaling when Parameters.TryGetValue("scaleX", out var sx) && Parameters.TryGetValue("scaleY", out var sy) =>
                sx is float scx && sy is float scy && scx > 0 && scy > 0,

            FilterType.ColorCorrection when Parameters.TryGetValue("brightness", out var b) =>
                b is float br && br >= -1.0f && br <= 1.0f,

            FilterType.Threshold when Parameters.TryGetValue("thresholdValue", out var tv) =>
                tv is float t && t >= 0.0f && t <= 1.0f,

            FilterType.CustomConvolution => ValidateConvolutionKernel(),

            _ => true
        };
    }

    /// <summary>
    /// Validates that ConvolutionKernel is a non-empty square matrix (odd side length, 3–15).
    /// </summary>
    private bool ValidateConvolutionKernel()
    {
        if (ConvolutionKernel is null || ConvolutionKernel.Length == 0)
            return false;

        int len = ConvolutionKernel.Length;
        int side = (int)Math.Round(Math.Sqrt(len));
        if (side * side != len)
            return false; // must be a perfect square

        // Accept odd sizes 3x3 through 15x15
        return side >= 3 && side <= 15 && side % 2 == 1;
    }

    /// <summary>
    /// Gets a parameter value by key with type safety.
    /// </summary>
    public T? GetParameter<T>(string key) where T : class
    {
        if (Parameters.TryGetValue(key, out var value))
            return value as T;
        return null;
    }

    /// <summary>
    /// Sets a parameter with validation.
    /// </summary>
    public void SetParameter(string key, object value, string typeName)
    {
        Parameters[key] = value;
        ParameterTypes[key] = typeName;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a copy of this configuration.
    /// </summary>
    public FilterConfiguration Clone()
    {
        return new FilterConfiguration
        {
            Name = Name,
            FilterType = FilterType,
            Description = Description,
            Parameters = new Dictionary<string, object>(Parameters),
            ParameterTypes = new Dictionary<string, string>(ParameterTypes),
            IsActive = IsActive,
            Priority = Priority,
            KernelCode = KernelCode,
            MaxThreadsPerBlock = MaxThreadsPerBlock,
            ConvolutionKernel = ConvolutionKernel is null ? null : (float[])ConvolutionKernel.Clone(),
            NormalizeKernel = NormalizeKernel
        };
    }
}
