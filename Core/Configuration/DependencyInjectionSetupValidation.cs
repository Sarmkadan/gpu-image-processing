#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Provides validation helpers for DependencyInjectionSetup configuration
    /// </summary>
    public static class DependencyInjectionSetupValidation
    {
        /// <summary>
        /// Validates the DependencyInjectionSetup configuration methods
        /// </summary>
        /// <param name="addGpuImageProcessing">AddGpuImageProcessing method</param>
        /// <param name="initializeServicesAsync">InitializeServicesAsync method</param>
        /// <param name="createServiceProvider">CreateServiceProvider method</param>
        /// <param name="createAndInitializeServiceProviderAsync">CreateAndInitializeServiceProviderAsync method</param>
        /// <param name="addApplicationLogging">AddApplicationLogging method</param>
        /// <param name="configureForProduction">ConfigureForProduction method</param>
        /// <param name="configureForDevelopment">ConfigureForDevelopment method</param>
        /// <returns>List of validation errors, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
        public static IReadOnlyList<string> Validate(
            Func<IServiceCollection, ApplicationSettings, IServiceCollection>? addGpuImageProcessing,
            Func<IServiceProvider, Task>? initializeServicesAsync,
            Func<ApplicationSettings?, IServiceProvider>? createServiceProvider,
            Func<ApplicationSettings?, Task<IServiceProvider>>? createAndInitializeServiceProviderAsync,
            Func<IServiceCollection, LoggingSettings, IServiceCollection>? addApplicationLogging,
            Action<ApplicationSettings>? configureForProduction,
            Action<ApplicationSettings>? configureForDevelopment)
        {
            ArgumentNullException.ThrowIfNull(addGpuImageProcessing);
            ArgumentNullException.ThrowIfNull(initializeServicesAsync);
            ArgumentNullException.ThrowIfNull(createServiceProvider);
            ArgumentNullException.ThrowIfNull(createAndInitializeServiceProviderAsync);
            ArgumentNullException.ThrowIfNull(addApplicationLogging);
            ArgumentNullException.ThrowIfNull(configureForProduction);
            ArgumentNullException.ThrowIfNull(configureForDevelopment);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Validates the DependencyInjectionSetup configuration methods
        /// </summary>
        /// <param name="addGpuImageProcessing">AddGpuImageProcessing method</param>
        /// <param name="initializeServicesAsync">InitializeServicesAsync method</param>
        /// <param name="createServiceProvider">CreateServiceProvider method</param>
        /// <param name="createAndInitializeServiceProviderAsync">CreateAndInitializeServiceProviderAsync method</param>
        /// <param name="addApplicationLogging">AddApplicationLogging method</param>
        /// <param name="configureForProduction">ConfigureForProduction method</param>
        /// <param name="configureForDevelopment">ConfigureForDevelopment method</param>
        /// <returns>True if all methods are valid, false otherwise</returns>
        public static bool IsValid(
            Func<IServiceCollection, ApplicationSettings, IServiceCollection>? addGpuImageProcessing,
            Func<IServiceProvider, Task>? initializeServicesAsync,
            Func<ApplicationSettings?, IServiceProvider>? createServiceProvider,
            Func<ApplicationSettings?, Task<IServiceProvider>>? createAndInitializeServiceProviderAsync,
            Func<IServiceCollection, LoggingSettings, IServiceCollection>? addApplicationLogging,
            Action<ApplicationSettings>? configureForProduction,
            Action<ApplicationSettings>? configureForDevelopment)
        {
            return Validate(
                addGpuImageProcessing,
                initializeServicesAsync,
                createServiceProvider,
                createAndInitializeServiceProviderAsync,
                addApplicationLogging,
                configureForProduction,
                configureForDevelopment).Count == 0;
        }

        /// <summary>
        /// Ensures the DependencyInjectionSetup configuration methods are valid
        /// </summary>
        /// <param name="addGpuImageProcessing">AddGpuImageProcessing method</param>
        /// <param name="initializeServicesAsync">InitializeServicesAsync method</param>
        /// <param name="createServiceProvider">CreateServiceProvider method</param>
        /// <param name="createAndInitializeServiceProviderAsync">CreateAndInitializeServiceProviderAsync method</param>
        /// <param name="addApplicationLogging">AddApplicationLogging method</param>
        /// <param name="configureForProduction">ConfigureForProduction method</param>
        /// <param name="configureForDevelopment">ConfigureForDevelopment method</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
        public static void EnsureValid(
            Func<IServiceCollection, ApplicationSettings, IServiceCollection>? addGpuImageProcessing,
            Func<IServiceProvider, Task>? initializeServicesAsync,
            Func<ApplicationSettings?, IServiceProvider>? createServiceProvider,
            Func<ApplicationSettings?, Task<IServiceProvider>>? createAndInitializeServiceProviderAsync,
            Func<IServiceCollection, LoggingSettings, IServiceCollection>? addApplicationLogging,
            Action<ApplicationSettings>? configureForProduction,
            Action<ApplicationSettings>? configureForDevelopment)
        {
            var errors = Validate(
                addGpuImageProcessing,
                initializeServicesAsync,
                createServiceProvider,
                createAndInitializeServiceProviderAsync,
                addApplicationLogging,
                configureForProduction,
                configureForDevelopment);

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"DependencyInjectionSetup configuration methods are invalid:\n{string.Join("\n", errors)}");
            }
        }
    }
}