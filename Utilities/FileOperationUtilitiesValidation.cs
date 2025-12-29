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
    /// Provides validation helpers for file operation utilities and their metadata.
    /// Validates file metadata including names, paths, sizes, and dates.
    /// </summary>
    public static class FileOperationUtilitiesValidation
    {
        /// <summary>
        /// Validates a file metadata instance.
        /// </summary>
        /// <param name="value">The file metadata to validate</param>
        /// <returns>List of validation problems; empty if valid</returns>
        public static IReadOnlyList<string> Validate(this FileMetadata value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate Name
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                problems.Add("Name cannot be null, empty, or whitespace.");
            }
            else if (value.Name.Length > 255)
            {
                problems.Add($"Name is too long. Given: {value.Name.Length} characters. Maximum: 255.");
            }

            // Validate FullPath
            if (string.IsNullOrWhiteSpace(value.FullPath))
            {
                problems.Add("FullPath cannot be null, empty, or whitespace.");
            }
            else if (value.FullPath.Length > 4096)
            {
                problems.Add($"FullPath is too long. Given: {value.FullPath.Length} characters. Maximum: 4096.");
            }
            else if (!Path.IsPathRooted(value.FullPath))
            {
                problems.Add("FullPath must be an absolute path.");
            }
            else if (!Path.GetFullPath(value.FullPath).Equals(value.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                problems.Add("FullPath contains relative path components that were not properly resolved.");
            }

            // Validate SizeBytes
            if (value.SizeBytes < 0)
            {
                problems.Add($"SizeBytes cannot be negative. Given: {value.SizeBytes}.");
            }
            else if (value.SizeBytes > 1024L * 1024 * 1024 * 1024) // 1TB
            {
                problems.Add($"SizeBytes exceeds reasonable maximum. Given: {value.SizeBytes} bytes. Maximum: 1TB.");
            }

            // Validate SizeFormatted
            if (string.IsNullOrWhiteSpace(value.SizeFormatted))
            {
                problems.Add("SizeFormatted cannot be null, empty, or whitespace.");
            }
            else if (value.SizeFormatted.Length > 50)
            {
                problems.Add($"SizeFormatted is too long. Given: {value.SizeFormatted.Length} characters. Maximum: 50.");
            }

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                problems.Add("CreatedAt cannot be default(DateTime). A valid creation date must be specified.");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("CreatedAt cannot be in the future.");
            }

            // Validate ModifiedAt
            if (value.ModifiedAt == default)
            {
                problems.Add("ModifiedAt cannot be default(DateTime). A valid modification date must be specified.");
            }
            else if (value.ModifiedAt > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add("ModifiedAt cannot be in the future.");
            }
            else if (value.ModifiedAt < value.CreatedAt)
            {
                problems.Add("ModifiedAt cannot be earlier than CreatedAt.");
            }

            // Validate Extension
            if (value.Extension != null)
            {
                if (value.Extension.Length > 10)
                {
                    problems.Add($"Extension is too long. Given: {value.Extension.Length} characters. Maximum: 10.");
                }
                else if (value.Extension.StartsWith(".", StringComparison.Ordinal) && value.Extension.Length == 1)
                {
                    problems.Add("Extension must contain a file extension after the dot.");
                }
                else if (!value.Extension.StartsWith(".", StringComparison.Ordinal) && value.Extension.Length > 0)
                {
                    problems.Add("Extension must start with a dot (e.g., .txt).");
                }
            }

            // Validate IsReadOnly
            // Boolean is always valid, no validation needed

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a file metadata instance is valid.
        /// </summary>
        /// <param name="value">The file metadata to check</param>
        /// <returns>True if valid; false otherwise</returns>
        public static bool IsValid(this FileMetadata value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a file metadata instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The file metadata to validate</param>
        /// <exception cref="ArgumentException">Thrown if the instance is invalid</exception>
        public static void EnsureValid(this FileMetadata value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException($"FileMetadata instance is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
    }
}
