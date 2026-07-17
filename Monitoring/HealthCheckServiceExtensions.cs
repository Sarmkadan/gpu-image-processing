using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Monitoring
{
    /// <summary>
    /// Provides extension methods for <see cref="HealthCheckService"/> to simplify health check registration and monitoring.
    /// </summary>
    public static class HealthCheckServiceExtensions
    {
        /// <summary>
        /// Registers a health check component with a specified name and implementation.
        /// </summary>
        /// <param name="healthCheckService">The health check service instance. Cannot be null.</param>
        /// <param name="name">The name of the health check component. Cannot be null.</param>
        /// <param name="healthCheck">The implementation of the health check component. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="healthCheckService"/>, <paramref name="name"/>, or <paramref name="healthCheck"/> is null.</exception>
        public static void RegisterHealthCheck(this HealthCheckService healthCheckService, string name, IHealthCheck healthCheck)
        {
            ArgumentNullException.ThrowIfNull(healthCheckService);
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(healthCheck);

            healthCheckService.RegisterHealthCheck(name, healthCheck);
        }

        /// <summary>
        /// Checks the health of all registered components and returns the overall health status.
        /// </summary>
        /// <param name="healthCheckService">The health check service instance. Cannot be null.</param>
        /// <returns>The overall health status of all registered components.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="healthCheckService"/> is null.</exception>
        public static ValueTask<HealthCheckResult> CheckHealthAsync(this HealthCheckService healthCheckService)
        {
            ArgumentNullException.ThrowIfNull(healthCheckService);

            return new ValueTask<HealthCheckResult>(healthCheckService.CheckHealthAsync());
        }

        /// <summary>
        /// Checks the health of a specific component and returns its health status.
        /// </summary>
        /// <param name="healthCheckService">The health check service instance. Cannot be null.</param>
        /// <param name="componentName">The name of the component to check. Cannot be null or empty.</param>
        /// <returns>The health status of the specified component.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="healthCheckService"/> or <paramref name="componentName"/> is null.</exception>
        public static ValueTask<ComponentHealth> CheckComponentAsync(this HealthCheckService healthCheckService, string componentName)
        {
            ArgumentNullException.ThrowIfNull(healthCheckService);
            ArgumentNullException.ThrowIfNull(componentName);

            return new ValueTask<ComponentHealth>(healthCheckService.CheckComponentAsync(componentName));
        }
    }
}