#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Services;

/// <summary>
/// Service for managing and applying image filters.
/// </summary>
public class FilterService
{
    private readonly FilterConfigurationRepository _repository;
    private readonly ILogger<FilterService> _logger;
    private readonly FrozenDictionary<FilterType, Func<Image, FilterConfiguration, ValueTask<Image>>> _filterHandlers;

    public FilterService(FilterConfigurationRepository repository, ILogger<FilterService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _filterHandlers = InitializeFilterHandlers();
    }

    /// <summary>
    /// Applies a filter to an image.
    /// </summary>
    public async Task<Image> ApplyFilterAsync(Image image, Guid filterId, CancellationToken cancellationToken = default)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        var config = await _repository.GetByIdAsync(filterId, cancellationToken);
        if (config == null)
            throw new InvalidFilterException($"Filter {filterId} not found");

        if (!config.IsActive)
            throw new InvalidFilterException($"Filter '{config.Name}' is not active");

        try
        {
            _logger.LogInformation("Applying filter {FilterName} to image {ImageId}", config.Name, image.Id);

            var handler = _filterHandlers.TryGetValue(config.FilterType, out var h) ? h : ApplyGenericFilter;
            var result = await handler(image, config);

            _logger.LogInformation("Filter {FilterName} applied successfully to {ImageId}", config.Name, image.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply filter {FilterName} to {ImageId}", config.Name, image.Id);
            throw new ProcessingException($"Filter application failed: {config.Name}", ex, image.FilePath, config.Name);
        }
    }

    /// <summary>
    /// Creates a new filter configuration.
    /// </summary>
    public async Task<FilterConfiguration> CreateFilterAsync(FilterConfiguration config, CancellationToken cancellationToken = default)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (!config.Validate())
            throw new InvalidFilterException("Filter configuration validation failed");

        var existing = await _repository.GetByNameAsync(config.Name, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Filter with name '{config.Name}' already exists");

        _logger.LogInformation("Creating filter {FilterName} of type {FilterType}", config.Name, config.FilterType);
        return await _repository.CreateAsync(config, cancellationToken);
    }

    /// <summary>
    /// Gets a filter configuration by ID.
    /// </summary>
    public async Task<FilterConfiguration?> GetFilterAsync(Guid filterId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(filterId, cancellationToken);
    }

    /// <summary>
    /// Gets all filters of a specific type.
    /// </summary>
    public async Task<IEnumerable<FilterConfiguration>> GetFiltersByTypeAsync(FilterType type, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByTypeAsync(type, cancellationToken);
    }

    /// <summary>
    /// Updates a filter configuration.
    /// </summary>
    public async Task<FilterConfiguration> UpdateFilterAsync(FilterConfiguration config, CancellationToken cancellationToken = default)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        var existing = await _repository.GetByIdAsync(config.Id, cancellationToken);
        if (existing == null)
            throw new InvalidOperationException($"Filter {config.Id} not found");

        _logger.LogInformation("Updating filter {FilterName}", config.Name);
        return await _repository.UpdateAsync(config, cancellationToken);
    }

    /// <summary>
    /// Deletes a filter configuration.
    /// </summary>
    public async Task<bool> DeleteFilterAsync(Guid filterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting filter {FilterId}", filterId);
        return await _repository.DeleteAsync(filterId, cancellationToken);
    }

    /// <summary>
    /// Gets active filters sorted by priority.
    /// </summary>
    public async Task<IEnumerable<FilterConfiguration>> GetActiveFiltersAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetActiveFiltersAsync(cancellationToken);
    }

    private FrozenDictionary<FilterType, Func<Image, FilterConfiguration, ValueTask<Image>>> InitializeFilterHandlers()
    {
        return new Dictionary<FilterType, Func<Image, FilterConfiguration, ValueTask<Image>>>
        {
            { FilterType.Grayscale, ApplyGrayscaleFilter },
            { FilterType.Blur, ApplyBlurFilter },
            { FilterType.Sharpen, ApplySharpenFilter },
            { FilterType.EdgeDetection, ApplyEdgeDetectionFilter },
            { FilterType.Rotation, ApplyRotationFilter },
            { FilterType.Scaling, ApplyScalingFilter },
            { FilterType.CustomConvolution, ApplyCustomConvolutionFilter }
        }.ToFrozenDictionary();
    }

    private ValueTask<Image> ApplyGrayscaleFilter(Image image, FilterConfiguration config)
    {
        if (image.ColorSpace != ColorSpace.Grayscale)
        {
            image.ColorSpace = ColorSpace.Grayscale;
            image.BitsPerPixel = 8;
        }
        return ValueTask.FromResult(image);
    }

    private ValueTask<Image> ApplyBlurFilter(Image image, FilterConfiguration config)
    {
        var radius = GetParameterValue(config, "radius", AppConstants.Filters.DefaultBlurRadius);
        _logger.LogDebug("Applying blur filter with radius {Radius} to image {ImageId}", radius, image.Id);
        // GPU kernel execution would happen here in real implementation
        return ValueTask.FromResult(image);
    }

    private ValueTask<Image> ApplySharpenFilter(Image image, FilterConfiguration config)
    {
        var strength = GetParameterValue(config, "strength", AppConstants.Filters.DefaultSharpenStrength);
        _logger.LogDebug("Applying sharpen filter with strength {Strength}", strength);
        return ValueTask.FromResult(image);
    }

    private ValueTask<Image> ApplyEdgeDetectionFilter(Image image, FilterConfiguration config)
    {
        _logger.LogDebug("Applying edge detection filter to image {ImageId}", image.Id);
        image.Metadata["edgeDetected"] = true;
        return ValueTask.FromResult(image);
    }

    private ValueTask<Image> ApplyRotationFilter(Image image, FilterConfiguration config)
    {
        var angle = GetParameterValue(config, "angle", 0.0f);
        _logger.LogDebug("Applying rotation filter with angle {Angle}", angle);
        // Swap dimensions for 90-degree rotations
        if (Math.Abs(angle % 90) < 0.01f && Math.Abs((int)(angle / 90) % 2) > 0.5f)
            (image.Width, image.Height) = (image.Height, image.Width);
        return ValueTask.FromResult(image);
    }

    private ValueTask<Image> ApplyScalingFilter(Image image, FilterConfiguration config)
    {
        var scaleX = GetParameterValue(config, "scaleX", 1.0f);
        var scaleY = GetParameterValue(config, "scaleY", 1.0f);
        _logger.LogDebug("Applying scaling filter with scale ({ScaleX}, {ScaleY})", scaleX, scaleY);
        image.Width = (int)(image.Width * scaleX);
        image.Height = (int)(image.Height * scaleY);
        return ValueTask.FromResult(image);
    }

    private static float GetParameterValue(FilterConfiguration config, string key, float defaultValue)
    {
        if (config.Parameters.TryGetValue(key, out var value))
        {
            if (value is float f)
                return f;
            if (float.TryParse(value.ToString(), out var parsed))
                return parsed;
        }
        return defaultValue;
    }

    private ValueTask<Image> ApplyCustomConvolutionFilter(Image image, FilterConfiguration config)
    {
        if (config.ConvolutionKernel is null || config.ConvolutionKernel.Length == 0)
            throw new ProcessingException("CustomConvolution filter requires a non-empty ConvolutionKernel.", null, image.FilePath, config.Name);

        int side = (int)Math.Round(Math.Sqrt(config.ConvolutionKernel.Length));
        float[] kernel = config.ConvolutionKernel;

        if (config.NormalizeKernel)
        {
            float sum = kernel.Sum();
            if (Math.Abs(sum) > float.Epsilon)
            {
                kernel = (float[])kernel.Clone();
                for (int i = 0; i < kernel.Length; i++)
                    kernel[i] /= sum;
            }
        }

        _logger.LogDebug(
            "Applying custom {Side}x{Side} convolution kernel to image {ImageId} (normalize={Normalize})",
            side, side, image.Id, config.NormalizeKernel);

        // In a full OpenCL implementation, the kernel would be compiled with the
        // supplied coefficients and dispatched via clEnqueueNDRangeKernel.
        image.Metadata["convolutionKernelSize"] = side;
        image.Metadata["convolutionNormalized"] = config.NormalizeKernel;
        return ValueTask.FromResult(image);
    }

    private ValueTask<Image> ApplyGenericFilter(Image image, FilterConfiguration config)    {
        _logger.LogWarning("No specific handler for filter type {FilterType}, applying generic filter", config.FilterType);
        image.Metadata[$"filter_{config.FilterType}"] = config.Name;
        return ValueTask.FromResult(image);
    }
}
