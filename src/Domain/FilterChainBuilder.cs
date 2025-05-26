#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Fluent builder for constructing <see cref="FilterChain"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The builder accumulates filter type selections in declaration order, validates
/// each configuration before it is added, and produces a ready-to-use
/// <see cref="FilterChain"/> on <see cref="Build"/>.
/// </para>
/// <example>
/// <code>
/// var chain = FilterChainBuilder
///     .Create("Portrait Workflow")
///     .WithDescription("Standard portrait post-processing")
///     .AddGrayscale()
///     .AddBlur(radius: 2.0f)
///     .AddSharpen(strength: 0.8f)
///     .AllowParallelExecution(maxParallelSteps: 4)
///     .CacheIntermediates()
///     .Build();
/// </code>
/// </example>
/// </remarks>
public sealed class FilterChainBuilder
{
    private readonly string _name;
    private string _description = string.Empty;
    private bool _allowParallel;
    private int _maxParallelSteps = Constants.Processing.DefaultThreadCount;
    private bool _cacheIntermediates;
    private int _executionOrder;

    // Ordered list of (filterId, estimatedMs) tuples that become FilterSteps.
    private readonly List<(Guid FilterId, double EstimatedMs)> _steps = [];

    private FilterChainBuilder(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Chain name must not be blank.", nameof(name));

        _name = name;
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts building a new filter chain with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Human-readable name for the chain. Must not be blank.</param>
    public static FilterChainBuilder Create(string name) => new(name);

    // ── Chain metadata ────────────────────────────────────────────────────────

    /// <summary>Sets the chain description.</summary>
    public FilterChainBuilder WithDescription(string description)
    {
        _description = description ?? string.Empty;
        return this;
    }

    /// <summary>Sets the execution order relative to other chains in a pipeline.</summary>
    public FilterChainBuilder WithExecutionOrder(int order)
    {
        _executionOrder = order;
        return this;
    }

    // ── Execution options ─────────────────────────────────────────────────────

    /// <summary>
    /// Allows independent steps to be dispatched in parallel on the GPU.
    /// </summary>
    /// <param name="maxParallelSteps">
    /// Maximum concurrent steps. Clamped to [1, <see cref="Constants.Processing.DefaultThreadCount"/> × 4].
    /// </param>
    public FilterChainBuilder AllowParallelExecution(int maxParallelSteps = 0)
    {
        _allowParallel = true;
        _maxParallelSteps = maxParallelSteps > 0
            ? Math.Clamp(maxParallelSteps, 1, Constants.Processing.DefaultThreadCount * 4)
            : Constants.Processing.DefaultThreadCount;
        return this;
    }

    /// <summary>Instructs the runtime to cache intermediate images between steps.</summary>
    public FilterChainBuilder CacheIntermediates()
    {
        _cacheIntermediates = true;
        return this;
    }

    // ── Filter additions ──────────────────────────────────────────────────────

    /// <summary>Appends a grayscale conversion step.</summary>
    public FilterChainBuilder AddGrayscale() =>
        AddStep(FilterType.Grayscale, estimatedMs: 2.0);

    /// <summary>Appends a Gaussian blur step.</summary>
    /// <param name="radius">Blur radius in pixels (0.5 – 50.0). Defaults to the system default.</param>
    public FilterChainBuilder AddBlur(float radius = Constants.Filters.DefaultBlurRadius)
    {
        if (radius < Constants.Filters.MinKernelRadius || radius > Constants.Filters.MaxKernelRadius)
            throw new ArgumentOutOfRangeException(nameof(radius),
                $"Blur radius must be between {Constants.Filters.MinKernelRadius} and {Constants.Filters.MaxKernelRadius}.");

        return AddStep(FilterType.Blur, estimatedMs: 5.0 + radius * 0.5);
    }

    /// <summary>Appends a sharpening step.</summary>
    /// <param name="strength">Sharpening strength (0.0 – 10.0). Defaults to the system default.</param>
    public FilterChainBuilder AddSharpen(float strength = Constants.Filters.DefaultSharpenStrength)
    {
        if (strength < 0.0f || strength > 10.0f)
            throw new ArgumentOutOfRangeException(nameof(strength),
                "Sharpen strength must be between 0.0 and 10.0.");

        return AddStep(FilterType.Sharpen, estimatedMs: 4.0 + strength * 0.3);
    }

    /// <summary>Appends an edge detection step.</summary>
    public FilterChainBuilder AddEdgeDetection() =>
        AddStep(FilterType.EdgeDetection, estimatedMs: 8.0);

    /// <summary>Appends a color correction step.</summary>
    /// <param name="brightness">Brightness delta (-1.0 – 1.0).</param>
    public FilterChainBuilder AddColorCorrection(float brightness = Constants.Filters.DefaultBrightnessAdjustment)
    {
        if (brightness < -1.0f || brightness > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(brightness),
                "Brightness must be between -1.0 and 1.0.");

        return AddStep(FilterType.ColorCorrection, estimatedMs: 3.0);
    }

    /// <summary>Appends a threshold (binarisation) step.</summary>
    /// <param name="thresholdValue">Threshold value (0.0 – 1.0).</param>
    public FilterChainBuilder AddThreshold(float thresholdValue = 0.5f)
    {
        if (thresholdValue < 0.0f || thresholdValue > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(thresholdValue),
                "Threshold value must be between 0.0 and 1.0.");

        return AddStep(FilterType.Threshold, estimatedMs: 2.0);
    }

    /// <summary>Appends a rotation step.</summary>
    /// <param name="angleDegrees">Rotation angle in degrees (-360 – 360).</param>
    public FilterChainBuilder AddRotation(float angleDegrees)
    {
        if (angleDegrees < -360f || angleDegrees > 360f)
            throw new ArgumentOutOfRangeException(nameof(angleDegrees),
                "Rotation angle must be in [-360, 360] degrees.");

        return AddStep(FilterType.Rotation, estimatedMs: 6.0);
    }

    /// <summary>Appends a scaling step.</summary>
    /// <param name="scaleX">Horizontal scale factor (must be &gt; 0).</param>
    /// <param name="scaleY">Vertical scale factor (must be &gt; 0).</param>
    public FilterChainBuilder AddScaling(float scaleX, float scaleY)
    {
        if (scaleX <= 0)
            throw new ArgumentOutOfRangeException(nameof(scaleX), "scaleX must be positive.");
        if (scaleY <= 0)
            throw new ArgumentOutOfRangeException(nameof(scaleY), "scaleY must be positive.");

        return AddStep(FilterType.Scaling, estimatedMs: 7.0);
    }

    /// <summary>Appends a bilateral filter step.</summary>
    public FilterChainBuilder AddBilateral() =>
        AddStep(FilterType.Bilateral, estimatedMs: 15.0);

    /// <summary>Appends a median filter step.</summary>
    public FilterChainBuilder AddMedian() =>
        AddStep(FilterType.Median, estimatedMs: 10.0);

    /// <summary>Appends an emboss effect step.</summary>
    public FilterChainBuilder AddEmboss() =>
        AddStep(FilterType.Emboss, estimatedMs: 5.0);

    /// <summary>Appends a Sobel edge-detection step.</summary>
    public FilterChainBuilder AddSobel() =>
        AddStep(FilterType.Sobel, estimatedMs: 9.0);

    /// <summary>Appends a custom convolution step backed by a caller-supplied filter ID.</summary>
    /// <param name="existingFilterId">
    /// The <see cref="FilterConfiguration.Id"/> of a pre-registered custom convolution filter.
    /// </param>
    /// <param name="estimatedExecutionMs">Optional execution-time hint in milliseconds.</param>
    public FilterChainBuilder AddCustomFilter(Guid existingFilterId, double estimatedExecutionMs = 20.0)
    {
        if (existingFilterId == Guid.Empty)
            throw new ArgumentException("existingFilterId must not be empty.", nameof(existingFilterId));

        _steps.Add((existingFilterId, estimatedExecutionMs));
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates all accumulated settings and constructs the <see cref="FilterChain"/>.
    /// </summary>
    /// <returns>A fully populated, validated <see cref="FilterChain"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no filter steps have been added.
    /// </exception>
    public FilterChain Build()
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException(
                "A filter chain must contain at least one filter step.");

        var chain = new FilterChain
        {
            Name = _name,
            Description = _description,
            ExecutionOrder = _executionOrder,
            AllowParallelExecution = _allowParallel,
            MaxParallelSteps = _maxParallelSteps,
            CacheIntermediateResults = _cacheIntermediates,
            IsEnabled = true
        };

        for (int i = 0; i < _steps.Count; i++)
        {
            var (filterId, estimatedMs) = _steps[i];
            chain.Steps.Add(new FilterStep
            {
                FilterId = filterId,
                Order = i,
                IsEnabled = true,
                EstimatedExecutionTimeMs = estimatedMs
            });
        }

        return chain;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private FilterChainBuilder AddStep(FilterType type, double estimatedMs)
    {
        // Each filter type creates a deterministic synthetic ID so the chain
        // can reference it in a repository look-up later; callers with an
        // existing repository should use AddCustomFilter instead.
        var filterId = DeterministicId(type, _steps.Count);
        _steps.Add((filterId, estimatedMs));
        return this;
    }

    /// <summary>
    /// Produces a deterministic <see cref="Guid"/> for a given filter type and position
    /// so that chains with identical step sequences always produce identical IDs.
    /// </summary>
    private static Guid DeterministicId(FilterType type, int position)
    {
        // Use a name-based (v5-style) deterministic GUID derived from the type
        // and position so tests can assert stable IDs without mocking a repository.
        var bytes = new byte[16];
        BitConverter.TryWriteBytes(bytes.AsSpan(0, 4), (int)type);
        BitConverter.TryWriteBytes(bytes.AsSpan(4, 4), position);
        return new Guid(bytes);
    }
}
