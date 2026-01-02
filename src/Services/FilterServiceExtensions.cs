#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Services;

/// <summary>
/// Extension methods for <see cref="FilterService"/> that provide convenient
/// operations for filter management and application.
/// </summary>
public static class FilterServiceExtensions
{
    /// <summary>
    /// Creates a new grayscale filter configuration with default parameters.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="name">The name of the filter.</param>
    /// <param name="priority">The execution priority (lower values run first).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created filter configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">A filter with the same name already exists.</exception>
    public static async Task<FilterConfiguration> CreateGrayscaleFilterAsync(
        this FilterService service,
        string name,
        int priority = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var config = new FilterConfiguration
        {
            Name = name,
            FilterType = FilterType.Grayscale,
            Description = "Converts image to grayscale",
            Priority = priority,
            Parameters = new Dictionary<string, object>(),
            ParameterTypes = new Dictionary<string, string>()
        };

        return await service.CreateFilterAsync(config, cancellationToken);
    }

    /// <summary>
    /// Creates a new blur filter configuration with default parameters.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="name">The name of the filter.</param>
    /// <param name="radius">The blur radius in pixels.</param>
    /// <param name="priority">The execution priority (lower values run first).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created filter configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">A filter with the same name already exists.</exception>
    public static async Task<FilterConfiguration> CreateBlurFilterAsync(
        this FilterService service,
        string name,
        float radius = AppConstants.Filters.DefaultBlurRadius,
        int priority = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var config = new FilterConfiguration
        {
            Name = name,
            FilterType = FilterType.Blur,
            Description = "Applies Gaussian blur to the image",
            Priority = priority,
            Parameters = new Dictionary<string, object> { ["radius"] = radius },
            ParameterTypes = new Dictionary<string, string> { ["radius"] = "float" }
        };

        return await service.CreateFilterAsync(config, cancellationToken);
    }

    /// <summary>
    /// Creates a new sharpen filter configuration with default parameters.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="name">The name of the filter.</param>
    /// <param name="strength">The sharpening strength (0-10).</param>
    /// <param name="priority">The execution priority (lower values run first).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created filter configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">A filter with the same name already exists.</exception>
    public static async Task<FilterConfiguration> CreateSharpenFilterAsync(
        this FilterService service,
        string name,
        float strength = AppConstants.Filters.DefaultSharpenStrength,
        int priority = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var config = new FilterConfiguration
        {
            Name = name,
            FilterType = FilterType.Sharpen,
            Description = "Enhances image edges and details",
            Priority = priority,
            Parameters = new Dictionary<string, object> { ["strength"] = strength },
            ParameterTypes = new Dictionary<string, string> { ["strength"] = "float" }
        };

        return await service.CreateFilterAsync(config, cancellationToken);
    }

    /// <summary>
    /// Gets all active filters sorted by priority in ascending order.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of active filters sorted by priority.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static async Task<IReadOnlyList<FilterConfiguration>> GetActiveFiltersAsync(
        this FilterService service,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var filters = await service.GetActiveFiltersAsync(cancellationToken);
        return filters.OrderBy(f => f.Priority).ToList().AsReadOnly();
    }

    /// <summary>
    /// Applies multiple filters to an image in sequence.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="image">The image to process.</param>
    /// <param name="filterIds">Collection of filter IDs to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="image"/> is null.</exception>
    /// <exception cref="InvalidFilterException">One or more filters are not found or inactive.</exception>
    public static async Task ApplyFiltersAsync(
        this FilterService service,
        Image image,
        IEnumerable<Guid> filterIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(filterIds);

        foreach (var filterId in filterIds)
        {
            await service.ApplyFilterAsync(image, filterId, cancellationToken);
        }
    }

    /// <summary>
    /// Creates a custom convolution filter configuration with the specified kernel.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="name">The name of the filter.</param>
    /// <param name="kernel">The convolution kernel as a flat array.</param>
    /// <param name="normalize">Whether to normalize the kernel.</param>
    /// <param name="priority">The execution priority (lower values run first).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created filter configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="kernel"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">A filter with the same name already exists.</exception>
    public static async Task<FilterConfiguration> CreateConvolutionFilterAsync(
        this FilterService service,
        string name,
        float[] kernel,
        bool normalize = true,
        int priority = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(kernel);

        if (kernel.Length == 0)
        {
            throw new ArgumentException("Kernel cannot be empty", nameof(kernel));
        }

        var config = new FilterConfiguration
        {
            Name = name,
            FilterType = FilterType.CustomConvolution,
            Description = "Applies custom convolution kernel",
            Priority = priority,
            Parameters = new Dictionary<string, object>(),
            ParameterTypes = new Dictionary<string, string>(),
            ConvolutionKernel = (float[])kernel.Clone(),
            NormalizeKernel = normalize
        };

        return await service.CreateFilterAsync(config, cancellationToken);
    }

    /// <summary>
    /// Gets all filters of a specific type, sorted by priority.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="type">The filter type to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of filters sorted by priority.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static async Task<IReadOnlyList<FilterConfiguration>> GetFiltersByTypeAsync(
        this FilterService service,
        FilterType type,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var filters = await service.GetFiltersByTypeAsync(type, cancellationToken);
        return filters.OrderBy(f => f.Priority).ToList().AsReadOnly();
    }

    /// <summary>
    /// Finds a filter by name (case-insensitive).
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="name">The filter name to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The filter configuration if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or whitespace.</exception>
    public static async Task<FilterConfiguration?> FindFilterByNameAsync(
        this FilterService service,
        string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var filters = await service.GetActiveFiltersAsync(cancellationToken);
        return filters.FirstOrDefault(f =>
            f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Activates a filter by ID.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="filterId">The filter ID to activate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated filter configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Filter not found.</exception>
    public static async Task<FilterConfiguration> ActivateFilterAsync(
        this FilterService service,
        Guid filterId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var config = await service.GetFilterAsync(filterId, cancellationToken);
        if (config == null)
        {
            throw new InvalidOperationException($"Filter {filterId} not found");
        }

        if (!config.IsActive)
        {
            config.IsActive = true;
            config.ModifiedAt = DateTime.UtcNow;
            return await service.UpdateFilterAsync(config, cancellationToken);
        }

        return config;
    }

    /// <summary>
    /// Deactivates a filter by ID.
    /// </summary>
    /// <param name="service">The filter service instance.</param>
    /// <param name="filterId">The filter ID to deactivate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the filter was deactivated, false if it was already inactive.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Filter not found.</exception>
    public static async Task<bool> DeactivateFilterAsync(
        this FilterService service,
        Guid filterId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var config = await service.GetFilterAsync(filterId, cancellationToken);
        if (config == null)
        {
            throw new InvalidOperationException($"Filter {filterId} not found");
        }

        if (config.IsActive)
        {
            config.IsActive = false;
            config.ModifiedAt = DateTime.UtcNow;
            await service.UpdateFilterAsync(config, cancellationToken);
            return true;
        }

        return false;
    }
}