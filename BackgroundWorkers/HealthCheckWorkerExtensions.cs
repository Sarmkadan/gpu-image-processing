using System;
using System.Threading;
using System.Threading.Tasks;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Provides extension methods for <see cref="HealthCheckWorker"/>.
    /// </summary>
    public static class HealthCheckWorkerExtensions
    {
        /// <summary>
        /// Checks if the worker's name matches the specified name.
        /// </summary>
        /// <param name="worker">The <see cref="HealthCheckWorker"/> instance.</param>
        /// <param name="expectedName">The name to compare against.</param>
        /// <returns>True if the names match; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="expectedName"/> is null or empty.</exception>
        public static bool IsNamed(this HealthCheckWorker worker, string expectedName)
        {
            ArgumentNullException.ThrowIfNull(worker);
            ArgumentException.ThrowIfNullOrEmpty(expectedName);

            return worker.GetName().Equals(expectedName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Restarts the health check worker by stopping and then starting it.
        /// </summary>
        /// <param name="worker">The <see cref="HealthCheckWorker"/> instance.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null.</exception>
        public static async Task RestartAsync(this HealthCheckWorker worker, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(worker);

            await worker.StopAsync(TimeSpan.FromSeconds(5));
            await worker.StartAsync(cancellationToken);
        }
    }
}
