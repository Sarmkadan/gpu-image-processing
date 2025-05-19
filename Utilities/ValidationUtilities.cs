#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utility methods for validating image processing parameters and configurations.
    /// Provides input validation, constraint checking, and error detection.
    /// </summary>
    public static class ValidationUtilities
    {
        /// <summary>
        /// Validates filter parameters for correct types and ranges.
        /// Returns validation result with error details if invalid.
        /// </summary>
        public static ValidationResult ValidateFilterParameters(FilterType filterType, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return ValidationResult.Success();

            return filterType switch
            {
                FilterType.Gaussian => ValidateGaussianParameters(parameters),
                FilterType.Sobel => ValidateSobelParameters(parameters),
                FilterType.Median => ValidateMedianParameters(parameters),
                FilterType.Canny => ValidateCannyParameters(parameters),
                FilterType.Bilateral => ValidateBilateralParameters(parameters),
                _ => ValidationResult.Failure("Unknown filter type")
            };
        }

        /// <summary>
        /// Validates image dimensions are within acceptable ranges.
        /// </summary>
        public static ValidationResult ValidateImageDimensions(int width, int height)
        {
            const int minDimension = 1;
            const int maxDimension = 65536;

            if (width < minDimension || width > maxDimension)
                return ValidationResult.Failure($"Width must be between {minDimension} and {maxDimension}");

            if (height < minDimension || height > maxDimension)
                return ValidationResult.Failure($"Height must be between {minDimension} and {maxDimension}");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates transformation angle in degrees.
        /// </summary>
        public static ValidationResult ValidateRotationAngle(float angle)
        {
            if (float.IsNaN(angle) || float.IsInfinity(angle))
                return ValidationResult.Failure("Rotation angle must be a valid number");

            // Normalize angle to 0-360 range
            float normalizedAngle = angle % 360;
            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates scaling factor for image resizing.
        /// </summary>
        public static ValidationResult ValidateScaleFactor(double scaleFactor)
        {
            const double minScale = 0.01;
            const double maxScale = 100.0;

            if (double.IsNaN(scaleFactor) || double.IsInfinity(scaleFactor))
                return ValidationResult.Failure("Scale factor must be a valid number");

            if (scaleFactor < minScale || scaleFactor > maxScale)
                return ValidationResult.Failure($"Scale factor must be between {minScale} and {maxScale}");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates batch processing job configuration.
        /// </summary>
        public static ValidationResult ValidateBatchJob(int imageCount, int filterCount, int batchSize)
        {
            if (imageCount <= 0)
                return ValidationResult.Failure("At least one image is required");

            if (filterCount < 0)
                return ValidationResult.Failure("Filter count cannot be negative");

            if (batchSize <= 0 || batchSize > 1000)
                return ValidationResult.Failure("Batch size must be between 1 and 1000");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates processing profile configuration.
        /// </summary>
        public static ValidationResult ValidateProcessingProfile(int maxParallelOps, int batchSize, int timeoutSeconds)
        {
            if (maxParallelOps <= 0 || maxParallelOps > 128)
                return ValidationResult.Failure("Max parallel operations must be between 1 and 128");

            if (batchSize <= 0 || batchSize > 10000)
                return ValidationResult.Failure("Batch size must be between 1 and 10000");

            if (timeoutSeconds <= 0 || timeoutSeconds > 3600)
                return ValidationResult.Failure("Timeout must be between 1 and 3600 seconds");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Checks if a file path is safe (no path traversal attempts).
        /// </summary>
        public static bool IsSafeFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            // Check for path traversal attempts
            if (filePath.Contains(".."))
                return false;

            // Check for null bytes (security risk)
            if (filePath.Contains("\0"))
                return false;

            return true;
        }

        /// <summary>
        /// Validates device ID is within valid range.
        /// </summary>
        public static ValidationResult ValidateDeviceId(int deviceId, int totalDevices)
        {
            if (deviceId < 0 || deviceId >= totalDevices)
                return ValidationResult.Failure($"Device ID must be between 0 and {totalDevices - 1}");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates string parameter is not null or empty.
        /// </summary>
        public static ValidationResult ValidateStringParameter(string value, string parameterName, int minLength = 1, int maxLength = int.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Failure($"{parameterName} cannot be empty");

            if (value.Length < minLength)
                return ValidationResult.Failure($"{parameterName} must be at least {minLength} characters");

            if (value.Length > maxLength)
                return ValidationResult.Failure($"{parameterName} must not exceed {maxLength} characters");

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateGaussianParameters(Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("sigma", out var sigmaObj) && sigmaObj is float sigma)
            {
                if (sigma <= 0 || sigma > 100)
                    return ValidationResult.Failure("Gaussian sigma must be between 0 and 100");
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateSobelParameters(Dictionary<string, object> parameters)
        {
            // Sobel typically doesn't require parameters
            return ValidationResult.Success();
        }

        private static ValidationResult ValidateMedianParameters(Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("kernel_size", out var sizeObj) && sizeObj is int size)
            {
                if (size < 1 || size > 21 || size % 2 == 0)
                    return ValidationResult.Failure("Median kernel size must be odd and between 1-21");
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateCannyParameters(Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("lower_threshold", out var lowerObj) && lowerObj is float lower)
            {
                if (lower < 0 || lower > 255)
                    return ValidationResult.Failure("Lower threshold must be between 0-255");
            }

            if (parameters.TryGetValue("upper_threshold", out var upperObj) && upperObj is float upper)
            {
                if (upper < 0 || upper > 255)
                    return ValidationResult.Failure("Upper threshold must be between 0-255");
            }

            return ValidationResult.Success();
        }

        private static ValidationResult ValidateBilateralParameters(Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("sigma_space", out var spaceObj) && spaceObj is float space)
            {
                if (space <= 0 || space > 100)
                    return ValidationResult.Failure("Sigma space must be between 0 and 100");
            }

            if (parameters.TryGetValue("sigma_range", out var rangeObj) && rangeObj is float range)
            {
                if (range <= 0 || range > 100)
                    return ValidationResult.Failure("Sigma range must be between 0 and 100");
            }

            return ValidationResult.Success();
        }
    }

    /// <summary>
    /// Result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; }

        public ValidationResult()
        {
            Errors = new List<string>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                Errors = new List<string> { errorMessage }
            };
        }

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
            ErrorMessage = string.Join("; ", Errors);
        }
    }
}
