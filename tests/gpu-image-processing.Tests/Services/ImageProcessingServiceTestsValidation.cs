using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Services
{
    /// <summary>
    /// Provides validation extension methods for <see cref="ImageProcessingServiceTests"/> instances.
    /// </summary>
    public static class ImageProcessingServiceTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="ImageProcessingServiceTests"/> instance for common issues.
        /// </summary>
        /// <param name="value">The service tests instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this ImageProcessingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // ImageProcessingServiceTests has no public properties to validate
            // This validation method exists for consistency with other test validation extensions

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the <see cref="ImageProcessingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The service tests instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ImageProcessingServiceTests value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures that the <see cref="ImageProcessingServiceTests"/> instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The service tests instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
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