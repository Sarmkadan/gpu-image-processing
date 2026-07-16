#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Batch;
using GpuImageProcessing.Fallback;
using GpuImageProcessing.Repository;
using GpuImageProcessing.Services;
using GpuImageProcessing.Pipeline;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Extension methods for configuring dependency injection in GPU image processing applications.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all GPU image processing services, repositories, pipeline components, and configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing AppSettings section.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddGpuImageProcessing(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register repositories
        services.AddSingleton<ImageRepository>();
        services.AddSingleton<FilterConfigurationRepository>();
        services.AddSingleton<ProcessingResultRepository>();

        // Register services
        services.AddSingleton<GpuManagementService>();
        services.AddSingleton<PerformanceMonitoringService>();
        services.AddSingleton<FilterService>();
        services.AddSingleton<ImageProcessingService>();
        services.AddSingleton<BatchProcessingService>();
        services.AddSingleton<CpuImageProcessor>();

        // Expose the CPU processor through the backend seam so consumers depend on
        // IImageProcessor, not a concrete backend. A GPU-backed implementation can
        // replace this single registration without touching anything else.
        services.AddSingleton<IImageProcessor>(sp => sp.GetRequiredService<CpuImageProcessor>());
        services.AddSingleton<DirectoryBatchProcessor>();

        // Register pipeline
        services.AddSingleton(new BatchPipelineOptions());
        services.AddSingleton<BatchProcessingPipeline>();

        // Register configuration
        var appSettings = new AppSettings();
        configuration.GetSection(AppSettings.SectionName).Bind(appSettings);
        services.AddSingleton(appSettings);

        return services;
    }

    /// <summary>
    /// Configures logging for GPU image processing with appropriate filters and console output.
    /// </summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <returns>The configured <see cref="ILoggingBuilder"/> for method chaining.</returns>
    public static ILoggingBuilder AddGpuImageProcessingLogging(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .AddConsole()
            .AddFilter("GpuImageProcessing", LogLevel.Debug)
            .AddFilter("Microsoft", LogLevel.Warning);
    }
}
