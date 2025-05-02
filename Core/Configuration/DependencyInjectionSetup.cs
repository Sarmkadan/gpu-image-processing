#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Configures dependency injection for the application
    /// </summary>
    public static class DependencyInjectionSetup
    {
        /// <summary>
        /// Registers all services in the dependency injection container
        /// </summary>
        public static IServiceCollection AddGpuImageProcessing(this IServiceCollection services, ApplicationSettings settings)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // Validate settings
            var validationErrors = ConfigurationValidator.ValidateSettings(settings);
            if (validationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Configuration validation failed:\n{string.Join("\n", validationErrors)}"
                );
            }

            // Register configuration
            services.AddSingleton(settings);
            services.AddSingleton(settings.OpenCL);
            services.AddSingleton(settings.Processing);
            services.AddSingleton(settings.Storage);
            services.AddSingleton(settings.Performance);
            services.AddSingleton(settings.Logging);

            // Register repositories
            services.AddSingleton<ImageRepository>();
            services.AddSingleton<JobRepository>();
            services.AddSingleton<ResultRepository>();
            services.AddSingleton<GenericRepository<Image>>();
            services.AddSingleton<GenericRepository<Filter>>();
            services.AddSingleton<GenericRepository<Transform>>();
            services.AddSingleton<GenericRepository<ProcessingProfile>>();
            services.AddSingleton<GenericRepository<DeviceInfo>>();

            // Register device service
            services.AddSingleton<DeviceService>();

            // Register filter service
            services.AddSingleton<FilterService>(provider =>
            {
                var filterRepo = provider.GetRequiredService<GenericRepository<Filter>>();
                return new FilterService(filterRepo);
            });

            // Register transform service
            services.AddSingleton<TransformService>(provider =>
            {
                var transformRepo = provider.GetRequiredService<GenericRepository<Transform>>();
                return new TransformService(transformRepo);
            });

            // Register image processing service
            services.AddSingleton<ImageProcessingService>(provider =>
            {
                var imageRepo = provider.GetRequiredService<ImageRepository>();
                var filterRepo = provider.GetRequiredService<GenericRepository<Filter>>();
                var transformRepo = provider.GetRequiredService<GenericRepository<Transform>>();
                var profileRepo = provider.GetRequiredService<GenericRepository<ProcessingProfile>>();
                var deviceService = provider.GetRequiredService<DeviceService>();

                return new ImageProcessingService(imageRepo, filterRepo, transformRepo, profileRepo, deviceService);
            });

            // Register batch processing service
            services.AddSingleton<BatchProcessingService>(provider =>
            {
                var jobRepo = provider.GetRequiredService<JobRepository>();
                var resultRepo = provider.GetRequiredService<ResultRepository>();
                var imageProc = provider.GetRequiredService<ImageProcessingService>();
                var imageRepo = provider.GetRequiredService<ImageRepository>();

                return new BatchProcessingService(jobRepo, resultRepo, imageProc, imageRepo);
            });

            return services;
        }

        /// <summary>
        /// Initializes services that require async initialization
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeServicesAsync(IServiceProvider serviceProvider)
        {
            var deviceService = serviceProvider.GetRequiredService<DeviceService>();
            await deviceService.InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a service provider with all dependencies configured
        /// </summary>
        public static IServiceProvider CreateServiceProvider(ApplicationSettings? settings = null)
        {
            settings ??= ConfigurationValidator.CreateDefaultSettings();

            var services = new ServiceCollection();
            services.AddGpuImageProcessing(settings);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Creates a fully initialized service provider with all services ready
        /// </summary>
        public static async System.Threading.Tasks.Task<IServiceProvider> CreateAndInitializeServiceProviderAsync(ApplicationSettings? settings = null)
        {
            var provider = CreateServiceProvider(settings);
            await InitializeServicesAsync(provider).ConfigureAwait(false);
            return provider;
        }
    }

    /// <summary>
    /// Extension methods for service configuration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds logging services
        /// </summary>
        public static IServiceCollection AddApplicationLogging(this IServiceCollection services, LoggingSettings settings)
        {
            if (settings.LogToConsole)
            {
                services.AddLogging(builder => builder.AddConsole());
            }

            return services;
        }

        /// <summary>
        /// Gets the GPU image processing services
        /// </summary>
        public static void ConfigureForProduction(this ApplicationSettings settings)
        {
            settings.Environment = "Production";
            settings.Logging.LogLevel = "Warning";
            settings.Performance.EnableOptimizations = true;
            settings.Storage.CacheIntermediateResults = true;
            settings.Processing.MaxParallelOperations = 8;
        }

        /// <summary>
        /// Configures settings for development/testing
        /// </summary>
        public static void ConfigureForDevelopment(this ApplicationSettings settings)
        {
            settings.Environment = "Development";
            settings.Logging.LogLevel = "Debug";
            settings.Logging.LogPerformanceMetrics = true;
            settings.Performance.EnableProfiling = true;
        }
    }
}
