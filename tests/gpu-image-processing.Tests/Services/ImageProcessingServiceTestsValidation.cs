using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Services
{
    public static class ImageProcessingServiceTestsValidation
    {
        /// <summary>
        /// Validates the ImageProcessingServiceTests instance for common issues.
        /// </summary>
        /// <param name="value">The service tests instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this ImageProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // No public properties to validate

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the ImageProcessingServiceTests instance is valid.
        /// </summary>
        /// <param name="value">The service tests instance to check.</param>
        /// <returns>true if valid; otherwise, false.</returns>
        public static bool IsValid(this ImageProcessingServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the ImageProcessingServiceTests instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The service tests instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing the validation problems.</exception>
        public static void EnsureValid(this ImageProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ImageProcessingServiceTests is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}