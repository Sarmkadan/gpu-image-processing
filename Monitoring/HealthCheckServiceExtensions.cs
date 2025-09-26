using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Monitoring
{
    public static class HealthCheckServiceExtensions
    {
        /// <summary>
        /// Registers a health check component with a specified name and implementation.
        /// </summary>
        /// <param name="healthCheckService">The health check service instance.</param>
        /// <param name="name">The name of the health check component.</param>
        /// <param name="healthCheck">The implementation of the health check component.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="healthCheckService"/> or <paramref name="name"/> is null.</exception>
        public static void RegisterHealthCheck(this HealthCheckService healthCheckService, string name, IHealthCheck healthCheck)
        {
            ArgumentNullException.ThrowIfNull(healthCheckService);
            ArgumentNullException.ThrowIfNull(name);

            healthCheckService.RegisterHealthCheck(name, healthCheck);
        }

        /// <summary>
        /// Checks the health of all registered components and returns the overall health status.
        /// </summary>
        /// <param name="healthCheckService">The health check service instance.</param>
        /// <returns>The overall health status of all registered components.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="healthCheckService"/> is null.</exception>
        public static async Task<HealthCheckResult> CheckHealthAsync(this HealthCheckService healthCheckService)
        {
            ArgumentNullException.ThrowIfNull(healthCheckService);

            return await healthCheckService.CheckHealthAsync();
        }

        /// <summary>
        /// Checks the health of a specific component and returns its health status.
        /// </summary>
        /// <param name="healthCheckService">The health check service instance.</param>
        /// <param name="componentName">The name of the component to check.</param>
        /// <returns>The health status of the specified component.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="healthCheckService"/> or <paramref name="componentName"/> is null.</exception>
        public static async Task<ComponentHealth> CheckComponentAsync(this HealthCheckService healthCheckService, string componentName)
        {
            ArgumentNullException.ThrowIfNull(healthCheckService);
            ArgumentNullException.ThrowIfNull(componentName);

            return await healthCheckService.CheckComponentAsync(componentName);
        }
    }
}
