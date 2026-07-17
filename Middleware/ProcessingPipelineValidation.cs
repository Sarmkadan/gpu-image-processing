using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides validation helpers for <see cref="ProcessingPipeline"/> instances.
    /// </summary>
    public static class ProcessingPipelineValidation
    {
        /// <summary>
        /// Validates a <see cref="ProcessingPipeline"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The pipeline to validate.</param>
        /// <returns>A read-only list of validation problems; empty if the pipeline is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ProcessingPipeline value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate middleware order
            var middlewareOrder = value.GetMiddlewareOrder();
            ArgumentNullException.ThrowIfNull(middlewareOrder);

            if (middlewareOrder.Count == 0)
            {
                problems.Add("At least one middleware must be registered in the pipeline.");
            }
            else
            {
                for (int i = 0; i < middlewareOrder.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(middlewareOrder[i]))
                    {
                        problems.Add(string.Format(CultureInfo.InvariantCulture,
                            "Middleware at position {0} has an invalid name (null, empty, or whitespace).", i));
                    }
                }
            }

            // Validate final handler
            if (value.ExecuteAsync == null)
            {
                problems.Add("Final handler must be set before execution.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="ProcessingPipeline"/> instance is valid.
        /// </summary>
        /// <param name="value">The pipeline to check.</param>
        /// <returns><see langword="true"/> if the pipeline is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ProcessingPipeline value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="ProcessingPipeline"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
        /// </summary>
        /// <param name="value">The pipeline to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the pipeline is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this ProcessingPipeline value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture,
                        "ProcessingPipeline is invalid. Problems:{0}{1}",
                        Environment.NewLine,
                        string.Join(Environment.NewLine + "- ", problems)));
            }
        }
    }
}