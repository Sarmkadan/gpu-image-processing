#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Provides validation helpers for <see cref="EventAggregator"/> instances.
    /// </summary>
    public static class EventAggregatorValidation
    {
        /// <summary>
        /// Validates an <see cref="EventAggregator"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The event aggregator to validate</param>
        /// <returns>A read-only list of validation problems; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this EventAggregator value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Check if disposed
            if (value.GetType().GetProperty("IsDisposed")?.GetValue(value) is bool isDisposed && isDisposed)
            {
                problems.Add("EventAggregator is disposed and cannot be used.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="EventAggregator"/> is valid.
        /// </summary>
        /// <param name="value">The event aggregator to check</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
        public static bool IsValid(this EventAggregator value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="EventAggregator"/> is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The event aggregator to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid</exception>
        public static void EnsureValid(this EventAggregator value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"EventAggregator is not valid. Problems: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}