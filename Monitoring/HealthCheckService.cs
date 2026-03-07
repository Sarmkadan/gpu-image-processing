#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Monitoring
{
    /// <summary>
    /// Service for monitoring application health and dependency status.
    /// Provides detailed health check reports for diagnostics and monitoring systems.
    /// </summary>
    public class HealthCheckService
    {
        private readonly Dictionary<string, IHealthCheck> _healthChecks;
        private readonly object _lockObject = new();

        public HealthCheckService()
        {
            _healthChecks = new Dictionary<string, IHealthCheck>();
        }

        /// <summary>
        /// Registers a health check component
        /// </summary>
        public void RegisterHealthCheck(string name, IHealthCheck healthCheck)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Health check name cannot be empty", nameof(name));

            lock (_lockObject)
            {
                _healthChecks[name] = healthCheck;
            }
        }

        /// <summary>
        /// Executes all health checks and returns overall health status
        /// </summary>
        public async Task<HealthCheckResult> CheckHealthAsync()
        {
            var results = new Dictionary<string, ComponentHealth>();
            var overallStatus = HealthStatus.Healthy;

            List<IHealthCheck> checks;
            lock (_lockObject)
            {
                checks = new List<IHealthCheck>(_healthChecks.Values);
            }

            var tasks = checks.Select(async check =>
            {
                try
                {
                    var result = await check.CheckAsync();
                    return (check.GetType().Name, result);
                }
                catch (Exception ex)
                {
                    return (check.GetType().Name, new ComponentHealth
                    {
                        Status = HealthStatus.Unhealthy,
                        Message = $"Health check failed: {ex.Message}"
                    });
                }
            });

            var checkResults = await Task.WhenAll(tasks);

            foreach (var (name, health) in checkResults)
            {
                results[name] = health;

                if (health.Status == HealthStatus.Unhealthy)
                    overallStatus = HealthStatus.Unhealthy;
                else if (health.Status == HealthStatus.Degraded && overallStatus == HealthStatus.Healthy)
                    overallStatus = HealthStatus.Degraded;
            }

            return new HealthCheckResult
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Components = results,
                Summary = GenerateSummary(overallStatus, results)
            };
        }

        /// <summary>
        /// Checks a specific component's health
        /// </summary>
        public async Task<ComponentHealth> CheckComponentAsync(string componentName)
        {
            IHealthCheck check;

            lock (_lockObject)
            {
                if (!_healthChecks.TryGetValue(componentName, out check))
                    return new ComponentHealth
                    {
                        Status = HealthStatus.Unknown,
                        Message = $"No health check registered for {componentName}"
                    };
            }

            try
            {
                return await check.CheckAsync();
            }
            catch (Exception ex)
            {
                return new ComponentHealth
                {
                    Status = HealthStatus.Unhealthy,
                    Message = $"Health check failed: {ex.Message}"
                };
            }
        }

        private string GenerateSummary(HealthStatus status, Dictionary<string, ComponentHealth> components)
        {
            var healthy = components.Count(c => c.Value.Status == HealthStatus.Healthy);
            var degraded = components.Count(c => c.Value.Status == HealthStatus.Degraded);
            var unhealthy = components.Count(c => c.Value.Status == HealthStatus.Unhealthy);

            return $"{status}: {healthy} healthy, {degraded} degraded, {unhealthy} unhealthy out of {components.Count} components";
        }

        public List<string> GetRegisteredChecks()
        {
            lock (_lockObject)
            {
                return _healthChecks.Keys.ToList();
            }
        }
    }

    public interface IHealthCheck
    {
        Task<ComponentHealth> CheckAsync();
    }

    public class ComponentHealth
    {
        public HealthStatus Status { get; set; }
        public string Message { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class HealthCheckResult
    {
        public HealthStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, ComponentHealth> Components { get; set; }
        public string Summary { get; set; }

        public bool IsHealthy => Status == HealthStatus.Healthy;
        public bool HasIssues => Status != HealthStatus.Healthy;
    }

    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy,
        Unknown
    }

    /// <summary>
    /// Memory health check implementation
    /// </summary>
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly float _degradedThreshold;
        private readonly float _unhealthyThreshold;

        public MemoryHealthCheck(float degradedThreshold = 70, float unhealthyThreshold = 90)
        {
            _degradedThreshold = degradedThreshold;
            _unhealthyThreshold = unhealthyThreshold;
        }

        public async Task<ComponentHealth> CheckAsync()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var totalMemory = GC.GetTotalMemory(false);
            var workingSet = process.WorkingSet64;

            // Simplified memory check (in real scenario would check against system total)
            var utilizationPercent = (workingSet / (1024.0 * 1024 * 1024)) * 100;

            var status = utilizationPercent > _unhealthyThreshold ? HealthStatus.Unhealthy :
                        utilizationPercent > _degradedThreshold ? HealthStatus.Degraded :
                        HealthStatus.Healthy;

            return await Task.FromResult(new ComponentHealth
            {
                Status = status,
                Message = $"Memory usage: {utilizationPercent:F1}%",
                Details = new Dictionary<string, object>
                {
                    { "WorkingSetMB", workingSet / (1024.0 * 1024) },
                    { "UsagePercent", utilizationPercent }
                }
            });
        }
    }

    /// <summary>
    /// Simple response time health check
    /// </summary>
    public class ResponseTimeHealthCheck : IHealthCheck
    {
        private readonly Func<Task> _operation;
        private readonly int _thresholdMs;

        public ResponseTimeHealthCheck(Func<Task> operation, int thresholdMs = 1000)
        {
            _operation = operation;
            _thresholdMs = thresholdMs;
        }

        public async Task<ComponentHealth> CheckAsync()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _operation();
                watch.Stop();

                var status = watch.ElapsedMilliseconds > _thresholdMs
                    ? HealthStatus.Degraded
                    : HealthStatus.Healthy;

                return new ComponentHealth
                {
                    Status = status,
                    Message = $"Response time: {watch.ElapsedMilliseconds}ms",
                    Details = new Dictionary<string, object>
                    {
                        { "ResponseTimeMs", watch.ElapsedMilliseconds }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ComponentHealth
                {
                    Status = HealthStatus.Unhealthy,
                    Message = $"Health check failed: {ex.Message}"
                };
            }
        }
    }
}
