#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation helpers for timeout-related parameters used with <see cref="TimeoutUtilities"/>.
    /// </summary>
    public sealed class TimeoutUtilitiesValidation
    {
        /// <summary>
        /// Validates timeout parameters used with <see cref="TimeoutUtilities"/> methods.
        /// </summary>
        /// <param name="timeout">The timeout duration to validate.</param>
        /// <returns>List of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentException">Thrown if timeout is invalid.</exception>
        public static IReadOnlyList<string> Validate(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException($"Timeout cannot be negative. Given: {timeout.TotalSeconds:F1}s.", nameof(timeout));
            }

            var problems = new List<string>();

            if (timeout == default)
            {
                problems.Add("Timeout cannot be default(TimeSpan). A valid timeout must be specified.");
            }
            else if (timeout.TotalMilliseconds > 0 && timeout.TotalMilliseconds < 1)
            {
                problems.Add($"Timeout is too small. Given: {timeout.TotalMilliseconds:F3}ms. Minimum: 1ms.");
            }
            else if (timeout.TotalHours > 24)
            {
                problems.Add($"Timeout exceeds reasonable maximum. Given: {timeout.TotalHours:F1}h. Maximum: 24h.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates timeout and minimum/maximum bounds used with <see cref="TimeoutUtilities.GetBoundedTimeout"/>.
        /// </summary>
        /// <param name="requestedTimeout">The requested timeout.</param>
        /// <param name="minimum">The minimum allowed timeout.</param>
        /// <param name="maximum">The maximum allowed timeout.</param>
        /// <returns>List of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentException">Thrown if minimum is greater than maximum.</exception>
        public static IReadOnlyList<string> Validate(TimeSpan requestedTimeout, TimeSpan minimum, TimeSpan maximum)
        {
            if (minimum > maximum)
            {
                throw new ArgumentException($"Minimum timeout ({minimum.TotalSeconds:F1}s) cannot be greater than maximum timeout ({maximum.TotalSeconds:F1}s).", nameof(minimum));
            }

            var problems = new List<string>();

            if (requestedTimeout == default)
            {
                problems.Add("Requested timeout cannot be default(TimeSpan). A valid timeout must be specified.");
            }
            else if (requestedTimeout < minimum)
            {
                problems.Add($"Requested timeout ({requestedTimeout.TotalSeconds:F1}s) is below minimum ({minimum.TotalSeconds:F1}s).");
            }
            else if (requestedTimeout > maximum)
            {
                problems.Add($"Requested timeout ({requestedTimeout.TotalSeconds:F1}s) exceeds maximum ({maximum.TotalSeconds:F1}s).");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates timeout and poll interval used with <see cref="TimeoutUtilities.WaitForConditionAsync"/>.
        /// </summary>
        /// <param name="timeout">The maximum wait timeout.</param>
        /// <param name="pollInterval">The polling interval.</param>
        /// <returns>List of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(TimeSpan timeout, TimeSpan? pollInterval)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException($"Timeout cannot be negative. Given: {timeout.TotalSeconds:F1}s.", nameof(timeout));
            }

            var problems = new List<string>();

            if (timeout == default)
            {
                problems.Add("Timeout cannot be default(TimeSpan). A valid timeout must be specified.");
            }

            if (pollInterval.HasValue)
            {
                if (pollInterval.Value < TimeSpan.Zero)
                {
                    throw new ArgumentException($"Poll interval cannot be negative. Given: {pollInterval.Value.TotalSeconds:F1}s.", nameof(pollInterval));
                }

                if (pollInterval.Value == default)
                {
                    problems.Add("Poll interval cannot be default(TimeSpan). A valid interval must be specified.");
                }
                else if (pollInterval.Value.TotalMilliseconds > 0 && pollInterval.Value.TotalMilliseconds < 1)
                {
                    problems.Add($"Poll interval is too small. Given: {pollInterval.Value.TotalMilliseconds:F3}ms. Minimum: 1ms.");
                }
                else if (pollInterval.Value >= timeout)
                {
                    problems.Add($"Poll interval ({pollInterval.Value.TotalSeconds:F1}s) should be smaller than timeout ({timeout.TotalSeconds:F1}s) to allow multiple polls.");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates timeout and retry parameters used with <see cref="TimeoutUtilities.RetryWithTimeoutAsync{T}"/>.
        /// </summary>
        /// <param name="timeout">The maximum retry timeout.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="initialDelay">Initial delay between retries.</param>
        /// <returns>List of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(TimeSpan timeout, int maxRetries, TimeSpan? initialDelay)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException($"Timeout cannot be negative. Given: {timeout.TotalSeconds:F1}s.", nameof(timeout));
            }

            var problems = new List<string>();

            if (timeout == default)
            {
                problems.Add("Timeout cannot be default(TimeSpan). A valid timeout must be specified.");
            }

            if (maxRetries <= 0)
            {
                problems.Add($"Max retries must be positive. Given: {maxRetries}. Minimum: 1.");
            }
            else if (maxRetries > 100)
            {
                problems.Add($"Max retries exceeds reasonable maximum. Given: {maxRetries}. Maximum: 100.");
            }

            if (initialDelay.HasValue)
            {
                if (initialDelay.Value < TimeSpan.Zero)
                {
                    throw new ArgumentException($"Initial delay cannot be negative. Given: {initialDelay.Value.TotalSeconds:F1}s.", nameof(initialDelay));
                }

                if (initialDelay.Value == default)
                {
                    problems.Add("Initial delay cannot be default(TimeSpan). A valid delay must be specified.");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates timeout and operation name used with <see cref="TimeoutUtilities.ExecuteWithTimeoutAsync{T}"/>.
        /// </summary>
        /// <param name="timeout">The operation timeout.</param>
        /// <param name="operationName">Name of the operation being executed.</param>
        /// <returns>List of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if operation name is null.</exception>
        public static IReadOnlyList<string> Validate(TimeSpan timeout, string operationName)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentException($"Timeout cannot be negative. Given: {timeout.TotalSeconds:F1}s.", nameof(timeout));
            }

            ArgumentNullException.ThrowIfNull(operationName);

            var problems = new List<string>();

            if (timeout == default)
            {
                problems.Add("Timeout cannot be default(TimeSpan). A valid timeout must be specified.");
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                problems.Add("Operation name cannot be null, empty, or whitespace.");
            }
            else if (operationName.Length > 100)
            {
                problems.Add($"Operation name is too long. Given: {operationName.Length} characters. Maximum: 100.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a timeout value is valid.
        /// </summary>
        /// <param name="timeout">The timeout to check.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValid(TimeSpan timeout) => Validate(timeout).Count == 0;

        /// <summary>
        /// Checks if timeout parameters are valid.
        /// </summary>
        /// <param name="requestedTimeout">The requested timeout.</param>
        /// <param name="minimum">The minimum allowed timeout.</param>
        /// <param name="maximum">The maximum allowed timeout.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValid(TimeSpan requestedTimeout, TimeSpan minimum, TimeSpan maximum) => Validate(requestedTimeout, minimum, maximum).Count == 0;

        /// <summary>
        /// Checks if timeout and poll interval are valid.
        /// </summary>
        /// <param name="timeout">The maximum wait timeout.</param>
        /// <param name="pollInterval">The polling interval.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValid(TimeSpan timeout, TimeSpan? pollInterval) => Validate(timeout, pollInterval).Count == 0;

        /// <summary>
        /// Checks if timeout, max retries, and initial delay are valid.
        /// </summary>
        /// <param name="timeout">The maximum retry timeout.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="initialDelay">Initial delay between retries.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValid(TimeSpan timeout, int maxRetries, TimeSpan? initialDelay) => Validate(timeout, maxRetries, initialDelay).Count == 0;

        /// <summary>
        /// Checks if timeout and operation name are valid.
        /// </summary>
        /// <param name="timeout">The operation timeout.</param>
        /// <param name="operationName">Name of the operation being executed.</param>
        /// <returns>True if valid; false otherwise.</returns>
        public static bool IsValid(TimeSpan timeout, string operationName) => Validate(timeout, operationName).Count == 0;

        /// <summary>
        /// Ensures that a timeout value is valid, throwing an exception if not.
        /// </summary>
        /// <param name="timeout">The timeout to validate.</param>
        /// <exception cref="ArgumentException">Thrown if timeout is invalid.</exception>
        public static void EnsureValid(TimeSpan timeout)
        {
            var problems = Validate(timeout);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Timeout is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }

        /// <summary>
        /// Ensures that timeout parameters are valid, throwing an exception if not.
        /// </summary>
        /// <param name="requestedTimeout">The requested timeout.</param>
        /// <param name="minimum">The minimum allowed timeout.</param>
        /// <param name="maximum">The maximum allowed timeout.</param>
        /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
        public static void EnsureValid(TimeSpan requestedTimeout, TimeSpan minimum, TimeSpan maximum)
        {
            var problems = Validate(requestedTimeout, minimum, maximum);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Timeout parameters are invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }

        /// <summary>
        /// Ensures that timeout and poll interval are valid.
        /// </summary>
        /// <param name="timeout">The maximum wait timeout.</param>
        /// <param name="pollInterval">The polling interval.</param>
        /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
        public static void EnsureValid(TimeSpan timeout, TimeSpan? pollInterval)
        {
            var problems = Validate(timeout, pollInterval);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Timeout and poll interval are invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }

        /// <summary>
        /// Ensures that timeout, max retries, and initial delay are valid.
        /// </summary>
        /// <param name="timeout">The maximum retry timeout.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="initialDelay">Initial delay between retries.</param>
        /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
        public static void EnsureValid(TimeSpan timeout, int maxRetries, TimeSpan? initialDelay)
        {
            var problems = Validate(timeout, maxRetries, initialDelay);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Timeout parameters are invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }

        /// <summary>
        /// Ensures that timeout and operation name are valid.
        /// </summary>
        /// <param name="timeout">The operation timeout.</param>
        /// <param name="operationName">Name of the operation being executed.</param>
        /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown if operation name is null.</exception>
        public static void EnsureValid(TimeSpan timeout, string operationName)
        {
            var problems = Validate(timeout, operationName);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"Timeout parameters are invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
    }
}