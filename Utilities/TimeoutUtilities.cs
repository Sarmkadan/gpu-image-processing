// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utilities for handling timeouts and cancellation across async operations.
    /// Provides safe timeout wrapping with detailed timeout information.
    /// </summary>
    public static class TimeoutUtilities
    {
        /// <summary>
        /// Executes an async operation with timeout protection
        /// </summary>
        public static async Task<T> ExecuteWithTimeoutAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            TimeSpan timeout,
            string operationName = "Operation")
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    return await operation(cts.Token);
                }
                catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException(
                        $"{operationName} exceeded timeout of {timeout.TotalSeconds:F1} seconds");
                }
            }
        }

        /// <summary>
        /// Executes an async operation without return value, with timeout
        /// </summary>
        public static async Task ExecuteWithTimeoutAsync(
            Func<CancellationToken, Task> operation,
            TimeSpan timeout,
            string operationName = "Operation")
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    await operation(cts.Token);
                }
                catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException(
                        $"{operationName} exceeded timeout of {timeout.TotalSeconds:F1} seconds");
                }
            }
        }

        /// <summary>
        /// Retries an operation with exponential backoff until timeout
        /// </summary>
        public static async Task<T> RetryWithTimeoutAsync<T>(
            Func<Task<T>> operation,
            TimeSpan timeout,
            int maxRetries = 5,
            TimeSpan? initialDelay = null)
        {
            var deadline = DateTime.UtcNow + timeout;
            var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
            var lastException = (Exception)null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (DateTime.UtcNow >= deadline)
                    throw new TimeoutException(
                        $"Operation timeout exceeded after {attempt - 1} attempts",
                        lastException);

                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (attempt == maxRetries)
                        throw;

                    var remainingTime = deadline - DateTime.UtcNow;
                    var delayTime = TimeSpan.FromMilliseconds(Math.Min(
                        delay.TotalMilliseconds,
                        remainingTime.TotalMilliseconds
                    ));

                    if (delayTime.TotalMilliseconds > 0)
                        await Task.Delay(delayTime);

                    delay = TimeSpan.FromMilliseconds(Math.Min(
                        delay.TotalMilliseconds * 2,
                        timeout.TotalMilliseconds
                    ));
                }
            }

            throw new TimeoutException("Retry timeout exceeded", lastException);
        }

        /// <summary>
        /// Waits for a condition to be true within a timeout
        /// </summary>
        public static async Task<bool> WaitForConditionAsync(
            Func<Task<bool>> condition,
            TimeSpan timeout,
            TimeSpan? pollInterval = null)
        {
            var deadline = DateTime.UtcNow + timeout;
            var poll = pollInterval ?? TimeSpan.FromMilliseconds(100);

            while (DateTime.UtcNow < deadline)
            {
                if (await condition())
                    return true;

                var remainingTime = deadline - DateTime.UtcNow;
                var delayTime = TimeSpan.FromMilliseconds(Math.Min(
                    poll.TotalMilliseconds,
                    remainingTime.TotalMilliseconds
                ));

                if (delayTime.TotalMilliseconds > 0)
                    await Task.Delay(delayTime);
            }

            return false;
        }

        /// <summary>
        /// Gets safe timeout value with minimum and maximum bounds
        /// </summary>
        public static TimeSpan GetBoundedTimeout(
            TimeSpan requestedTimeout,
            TimeSpan minimum,
            TimeSpan maximum)
        {
            if (requestedTimeout < minimum)
                return minimum;

            if (requestedTimeout > maximum)
                return maximum;

            return requestedTimeout;
        }

        /// <summary>
        /// Creates a combined cancellation token from multiple sources
        /// </summary>
        public static CancellationToken CreateLinkedToken(
            TimeSpan timeout,
            params CancellationToken[] tokens)
        {
            var cts = new CancellationTokenSource(timeout);

            if (tokens.Length > 0)
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    tokens.Concat(new[] { cts.Token }).ToArray()
                );
                return linkedCts.Token;
            }

            return cts.Token;
        }

        /// <summary>
        /// Formats timeout duration as human-readable string
        /// </summary>
        public static string FormatTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < 1000)
                return $"{timeout.TotalMilliseconds:F0}ms";

            if (timeout.TotalSeconds < 60)
                return $"{timeout.TotalSeconds:F1}s";

            if (timeout.TotalMinutes < 60)
                return $"{timeout.TotalMinutes:F1}m";

            return $"{timeout.TotalHours:F1}h";
        }
    }
}
