using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="TelemetryService"/> to simplify common telemetry tasks.
    /// </summary>
    public static class TelemetryServiceExtensions
    {
        /// <summary>
        /// Records an error event in the telemetry service.
        /// </summary>
        /// <param name="service">The telemetry service instance.</param>
        /// <param name="message">A descriptive error message.</param>
        /// <param name="exception">The exception to record.</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="exception"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="message"/> is <see langword="null"/> or empty.</exception>
        public static void RecordError(this TelemetryService service, string message, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentException.ThrowIfNullOrEmpty(message);
            ArgumentNullException.ThrowIfNull(exception);

            var properties = new Dictionary<string, object>(2)
            {
                ["Message"] = message,
                ["Exception"] = exception.ToString()
            };

            service.RecordEvent("Error", properties, "error");
        }

        /// <summary>
        /// Starts a timed operation. The returned <see cref="IDisposable"/> should be used in a 'using' block.
        /// </summary>
        /// <param name="service">The telemetry service instance.</param>
        /// <param name="operationName">The name of the operation to time.</param>
        /// <returns>An <see cref="IDisposable"/> token that stops the timer when disposed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="operationName"/> is <see langword="null"/> or empty.</exception>
        public static IDisposable MeasureOperation(this TelemetryService service, string operationName)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentException.ThrowIfNullOrEmpty(operationName);

            return service.StartTiming(operationName);
        }
    }
}
