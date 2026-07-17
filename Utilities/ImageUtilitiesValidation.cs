#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation helpers for <see cref="ImageUtilities"/> operations.
    /// Validates parameters and state before performing image processing operations.
    /// </summary>
    public static class ImageUtilitiesValidation
    {
        /// <summary>
        /// Validates the image utilities static class configuration.
        /// </summary>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see cref="ImageUtilities.SupportedExtensions"/> is null.</exception>
        public static IReadOnlyList<string> ValidateImageUtilitiesConfiguration()
        {
            ArgumentNullException.ThrowIfNull(ImageUtilities.SupportedExtensions);

            var errors = new List<string>();

            if (ImageUtilities.SupportedExtensions.Length == 0)
            {
                errors.Add("SupportedExtensions collection is empty.");
            }
            else
            {
                foreach (var extension in ImageUtilities.SupportedExtensions)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        errors.Add("SupportedExtensions contains null or whitespace entry.");
                        break;
                    }

                    if (!extension.StartsWith(".", StringComparison.Ordinal))
                    {
                        errors.Add($"SupportedExtensions contains invalid extension '{extension}' - must start with dot.");
                        break;
                    }
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the image utilities static class configuration is valid.
        /// </summary>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsImageUtilitiesConfigurationValid()
            => ValidateImageUtilitiesConfiguration().Count == 0;

        /// <summary>
        /// Ensures that the image utilities static class configuration is valid.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when validation fails with a detailed error message.</exception>
        public static void EnsureImageUtilitiesConfigurationValid()
        {
            var errors = ValidateImageUtilitiesConfiguration();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ImageUtilities configuration validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
            }
        }

        /// <summary>
        /// Validates a file path for image operations.
        /// </summary>
        /// <param name="filePath">The file path to validate.</param>
        /// <param name="paramName">Name of the parameter for exception messages.</param>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null.</exception>
        public static IReadOnlyList<string> ValidateFilePath(this string? filePath, string paramName = "filePath")
        {
            ArgumentNullException.ThrowIfNull(filePath);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errors.Add($"{paramName} cannot be null or whitespace.");
            }
            else
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.DirectoryName != null && !Directory.Exists(fileInfo.DirectoryName))
                    {
                        errors.Add($"Directory for {paramName} does not exist: {fileInfo.DirectoryName}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{paramName} is not a valid file path: {ex.Message}");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates that a file path points to an existing, accessible image file.
        /// </summary>
        /// <param name="filePath">The file path to validate.</param>
        /// <param name="paramName">Name of the parameter for exception messages.</param>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null.</exception>
        public static IReadOnlyList<string> ValidateImageFileAccess(this string? filePath, string paramName = "filePath")
        {
            ArgumentNullException.ThrowIfNull(filePath);

            var errors = new List<string>(filePath.ValidateFilePath(paramName));

            if (errors.Count == 0)
            {
                if (!File.Exists(filePath))
                {
                    errors.Add($"Image file does not exist: {filePath}");
                }
                else if (new FileInfo(filePath).Length == 0)
                {
                    errors.Add($"Image file is empty: {filePath}");
                }
                else if (!ImageUtilities.IsSupportedImageFile(filePath))
                {
                    errors.Add($"File is not a supported image format: {filePath}. Supported extensions: {string.Join(", ", ImageUtilities.SupportedExtensions)}");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates output directory path.
        /// </summary>
        /// <param name="directoryPath">The directory path to validate.</param>
        /// <param name="paramName">Name of the parameter for exception messages.</param>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="directoryPath"/> is null.</exception>
        public static IReadOnlyList<string> ValidateOutputDirectory(this string? directoryPath, string paramName = "directoryPath")
        {
            ArgumentNullException.ThrowIfNull(directoryPath);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                errors.Add($"{paramName} cannot be null or whitespace.");
            }
            else
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(directoryPath);
                    if (!directoryInfo.Exists)
                    {
                        errors.Add($"Output directory does not exist and cannot be created: {directoryPath}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{paramName} is not a valid directory path: {ex.Message}");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates image dimensions.
        /// </summary>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="paramNameWidth">Name of the width parameter for exception messages.</param>
        /// <param name="paramNameHeight">Name of the height parameter for exception messages.</param>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        public static IReadOnlyList<string> ValidateImageDimensions(this int width, int height, string paramNameWidth = "width", string paramNameHeight = "height")
        {
            var errors = new List<string>();

            if (width <= 0)
            {
                errors.Add($"{paramNameWidth} must be greater than zero. Got: {width}");
            }

            if (height <= 0)
            {
                errors.Add($"{paramNameHeight} must be greater than zero. Got: {height}");
            }

            if (width > 100000)
            {
                errors.Add($"{paramNameWidth} is unreasonably large. Got: {width}");
            }

            if (height > 100000)
            {
                errors.Add($"{paramNameHeight} is unreasonably large. Got: {height}");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates scale factor for proportional resizing.
        /// </summary>
        /// <param name="scaleFactor">The scale factor to validate.</param>
        /// <param name="paramName">Name of the parameter for exception messages.</param>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        public static IReadOnlyList<string> ValidateScaleFactor(this double scaleFactor, string paramName = "scaleFactor")
        {
            var errors = new List<string>();

            if (double.IsNaN(scaleFactor))
            {
                errors.Add($"{paramName} cannot be NaN.");
            }
            else if (double.IsInfinity(scaleFactor))
            {
                errors.Add($"{paramName} cannot be infinite.");
            }
            else if (scaleFactor <= 0)
            {
                errors.Add($"{paramName} must be greater than zero. Got: {scaleFactor}");
            }
            else if (scaleFactor > 100)
            {
                errors.Add($"{paramName} is unreasonably large. Got: {scaleFactor}");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates input path and filter name for output filename generation.
        /// </summary>
        /// <param name="inputPath">The input file path.</param>
        /// <param name="filterName">The filter name.</param>
        /// <param name="paramNameInput">Name of the input parameter for exception messages.</param>
        /// <param name="paramNameFilter">Name of the filter parameter for exception messages.</param>
        /// <returns>A list of validation errors. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inputPath"/> or <paramref name="filterName"/> is null.</exception>
        public static IReadOnlyList<string> ValidateOutputFilenameParameters(this string? inputPath, string? filterName,
            string paramNameInput = "inputPath", string paramNameFilter = "filterName")
        {
            ArgumentNullException.ThrowIfNull(inputPath);
            ArgumentNullException.ThrowIfNull(filterName);

            var errors = new List<string>();

            var inputErrors = inputPath.ValidateFilePath(paramNameInput);
            if (inputErrors.Count > 0)
            {
                errors.AddRange(inputErrors);
            }

            if (string.IsNullOrWhiteSpace(filterName))
            {
                errors.Add($"{paramNameFilter} cannot be null or whitespace.");
            }
            else if (filterName.Length > 255)
            {
                errors.Add($"{paramNameFilter} is too long. Max 255 characters. Got: {filterName.Length}");
            }

            return errors.AsReadOnly();
        }
    }
}