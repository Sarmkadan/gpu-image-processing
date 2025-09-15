using System;
using System.Threading.Tasks;
using GpuImageProcessing.BackgroundWorkers;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Provides extension methods for <see cref="JobProcessingWorker"/>.
    /// </summary>
    public static class JobProcessingWorkerExtensions
    {
        /// <summary>
        /// Restarts the background worker by stopping it and then starting it again.
        /// </summary>
        /// <param name="worker">The <see cref="JobProcessingWorker"/> to restart.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null.</exception>
        public static async Task RestartAsync(this JobProcessingWorker worker)
        {
            ArgumentNullException.ThrowIfNull(worker);

            await worker.StopAsync();
            await worker.StartAsync();
        }

        /// <summary>
        /// Gets the name of the worker in uppercase.
        /// </summary>
        /// <param name="worker">The <see cref="JobProcessingWorker"/> to get the name from.</param>
        /// <returns>The name of the worker in uppercase.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null.</exception>
        public static string GetNameUpper(this JobProcessingWorker worker)
        {
            ArgumentNullException.ThrowIfNull(worker);

            return worker.GetName().ToUpperInvariant();
        }

        /// <summary>
        /// Returns a formatted diagnostic string for the worker.
        /// </summary>
        /// <param name="worker">The <see cref="JobProcessingWorker"/> to diagnose.</param>
        /// <returns>A diagnostic string containing the worker's name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null.</exception>
        public static string ToDiagnosticString(this JobProcessingWorker worker)
        {
            ArgumentNullException.ThrowIfNull(worker);

            return $"Worker: {worker.GetName()}";
        }
    }
}
