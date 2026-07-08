#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.Configuration;

/// <summary>
/// Extension methods for registering the SIMD fallback processor with the
/// application's dependency-injection container.
/// </summary>
public static class SimdFallbackExtensions
{
    /// <summary>
    /// Registers <see cref="SimdFallbackService"/> as a singleton with the DI container.
    /// </summary>
    /// <remarks>
    /// SIMD capability detection is performed once at construction time and the
    /// resulting <see cref="SimdCapabilities"/> snapshot is immutable, making a
    /// singleton lifetime appropriate.  Call this alongside
    /// <see cref="DependencyInjectionExtensions.AddGpuImageProcessing"/> to enable
    /// transparent CPU fallback when no GPU device is available.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddSimdFallback(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<SimdFallbackService>();

        return services;
    }

    /// <summary>
    /// Resolves <see cref="SimdFallbackService"/> from <paramref name="provider"/> and
    /// emits a startup log entry describing the detected SIMD capabilities.
    /// </summary>
    /// <remarks>
    /// Intended to be called once during application startup (e.g. after
    /// <c>IServiceProvider.BuildServiceProvider()</c>) to surface capability
    /// information in the application log before any processing begins.
    /// Emits <see cref="LogLevel.Information"/> when SIMD is available and
    /// <see cref="LogLevel.Warning"/> when only scalar processing is possible.
    /// </remarks>
    /// <param name="provider">The <see cref="IServiceProvider"/> used to resolve services.</param>
    /// <returns>The same <see cref="IServiceProvider"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceProvider LogSimdCapabilities(this IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var simd   = provider.GetRequiredService<SimdFallbackService>();
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(SimdFallbackExtensions));

        if (simd.Capabilities.IsAnySimdAvailable)
        {
            logger.LogInformation(
                "CPU SIMD fallback active — {Capabilities}",
                simd.Capabilities);
        }
        else
        {
            logger.LogWarning(
                "No SIMD extensions detected on this CPU. " +
                "Fallback processing will use scalar code and may be slower.");
        }

        return provider;
    }
}
