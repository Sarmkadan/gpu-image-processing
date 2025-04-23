// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Extension methods for dependency injection configuration.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all GPU image processing services.
    /// </summary>
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

        // Register configuration
        var appSettings = new AppSettings();
        configuration.GetSection(AppSettings.SectionName).Bind(appSettings);
        services.AddSingleton(appSettings);

        return services;
    }

    /// <summary>
    /// Configures logging for GPU image processing.
    /// </summary>
    public static ILoggingBuilder AddGpuImageProcessingLogging(this ILoggingBuilder builder)
    {
        return builder
            .AddConsole()
            .AddFilter("GpuImageProcessing", LogLevel.Debug)
            .AddFilter("Microsoft", LogLevel.Warning);
    }
}
