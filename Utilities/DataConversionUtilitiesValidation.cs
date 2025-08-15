#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Validation helpers for DataConversionUtilities operations.
    /// Provides validation, IsValid, and EnsureValid methods for all public members.
    /// </summary>
    public static class DataConversionUtilitiesValidation
    {
        /// <summary>
        /// Checks if the DataConversionUtilities class is valid (always true for static class)
        /// </summary>
        /// <returns>True</returns>
        public static bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Ensures the DataConversionUtilities class is valid (no-op for static class)
        /// </summary>
        /// <exception cref="ArgumentException">Never thrown - static class has no state to validate</exception>
        public static void EnsureValid()
        {
            // Static class has no state to validate
        }

        /// <summary>
        /// Validates the BytesToHex method with provided parameters
        /// </summary>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<byte[], string> bytesToHex, byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytesToHex);
            var problems = new List<string>();

            if (bytes == null)
                problems.Add("Bytes array cannot be null");
            else if (bytes.Length == 0)
                problems.Add("Bytes array cannot be empty");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the HexToBytes method with provided parameters
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<string, byte[]> hexToBytes, string hex)
        {
            ArgumentNullException.ThrowIfNull(hexToBytes);
            var problems = new List<string>();

            if (string.IsNullOrEmpty(hex))
                problems.Add("Hex string cannot be null or empty");
            else if (hex.Length % 2 != 0)
                problems.Add("Hex string must have even length");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the FloatsToBytes method with provided parameters
        /// </summary>
        /// <param name="floats">The float array to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<float[], byte[]> floatsToBytes, float[] floats)
        {
            ArgumentNullException.ThrowIfNull(floatsToBytes);
            var problems = new List<string>();

            if (floats == null)
                problems.Add("Float array cannot be null");
            else if (floats.Length == 0)
                problems.Add("Float array cannot be empty");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the BytesToFloats method with provided parameters
        /// </summary>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<byte[], float[]> bytesToFloats, byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytesToFloats);
            var problems = new List<string>();

            if (bytes == null)
                problems.Add("Bytes array cannot be null");
            else if (bytes.Length == 0)
                problems.Add("Bytes array cannot be empty");
            else if (bytes.Length % sizeof(float) != 0)
                problems.Add($"Bytes array length must be multiple of {sizeof(float)} (got {bytes.Length})");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the FormatFileSize method with provided parameters
        /// </summary>
        /// <param name="bytes">The file size in bytes</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<long, string> formatFileSize, long bytes)
        {
            ArgumentNullException.ThrowIfNull(formatFileSize);
            var problems = new List<string>();

            // bytes can be any long value, including negative
            // No validation needed beyond null check on the delegate

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the ParseFileSize method with provided parameters
        /// </summary>
        /// <param name="sizeString">The human-readable size string</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<string, long> parseFileSize, string sizeString)
        {
            ArgumentNullException.ThrowIfNull(parseFileSize);
            var problems = new List<string>();

            if (string.IsNullOrEmpty(sizeString))
                problems.Add("Size string cannot be null or empty");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the FormatTimeSpan method with provided parameters
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<TimeSpan, string> formatTimeSpan, TimeSpan timeSpan)
        {
            ArgumentNullException.ThrowIfNull(formatTimeSpan);
            var problems = new List<string>();

            // TimeSpan can be any value
            // No validation needed beyond null check on the delegate

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the ParseDuration method with provided parameters
        /// </summary>
        /// <param name="durationString">The duration string to parse</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<string, TimeSpan> parseDuration, string durationString)
        {
            ArgumentNullException.ThrowIfNull(parseDuration);
            var problems = new List<string>();

            if (string.IsNullOrEmpty(durationString))
                problems.Add("Duration string cannot be null or empty");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the ToBinaryString method with provided parameters
        /// </summary>
        /// <param name="valueToBinaryString">The method to validate</param>
        /// <param name="value">The integer value</param>
        /// <param name="minWidth">The minimum width for binary string</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<int, int, string> valueToBinaryString, int value, int minWidth)
        {
            ArgumentNullException.ThrowIfNull(valueToBinaryString);
            var problems = new List<string>();

            if (minWidth < 0)
                problems.Add("Minimum width cannot be negative");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the IsWithinTolerance method with provided parameters
        /// </summary>
        /// <param name="isWithinTolerance">The method to validate</param>
        /// <param name="actual">The actual value</param>
        /// <param name="expected">The expected value</param>
        /// <param name="tolerance">The tolerance threshold</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(this Func<float, float, float, bool> isWithinTolerance, float actual, float expected, float tolerance)
        {
            ArgumentNullException.ThrowIfNull(isWithinTolerance);
            var problems = new List<string>();

            if (float.IsNaN(actual))
                problems.Add("Actual value cannot be NaN");
            else if (float.IsInfinity(actual))
                problems.Add("Actual value cannot be infinite");

            if (float.IsNaN(expected))
                problems.Add("Expected value cannot be NaN");
            else if (float.IsInfinity(expected))
                problems.Add("Expected value cannot be infinite");

            if (float.IsNaN(tolerance))
                problems.Add("Tolerance cannot be NaN");
            else if (float.IsInfinity(tolerance))
                problems.Add("Tolerance cannot be infinite");
            else if (tolerance < 0)
                problems.Add("Tolerance cannot be negative");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the Normalize method with provided parameters
        /// </summary>
        /// <param name="normalize">The method to validate</param>
        /// <param name="value">The value to normalize</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> ValidateNormalize(this Func<float, float, float, float> normalize, float value, float min, float max)
        {
            ArgumentNullException.ThrowIfNull(normalize);
            var problems = new List<string>();

            if (float.IsNaN(value))
                problems.Add("Value cannot be NaN");
            else if (float.IsInfinity(value))
                problems.Add("Value cannot be infinite");

            if (float.IsNaN(min))
                problems.Add("Minimum bound cannot be NaN");
            else if (float.IsInfinity(min))
                problems.Add("Minimum bound cannot be infinite");

            if (float.IsNaN(max))
                problems.Add("Maximum bound cannot be NaN");
            else if (float.IsInfinity(max))
                problems.Add("Maximum bound cannot be infinite");
            else if (max <= min)
                problems.Add("Maximum bound must be greater than minimum bound");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the Denormalize method with provided parameters
        /// </summary>
        /// <param name="denormalize">The method to validate</param>
        /// <param name="normalized">The normalized value (0-1 range)</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> ValidateDenormalize(this Func<float, float, float, float> denormalize, float normalized, float min, float max)
        {
            ArgumentNullException.ThrowIfNull(denormalize);
            var problems = new List<string>();

            if (float.IsNaN(normalized))
                problems.Add("Normalized value cannot be NaN");
            else if (float.IsInfinity(normalized))
                problems.Add("Normalized value cannot be infinite");
            else if (normalized < 0 || normalized > 1)
                problems.Add("Normalized value must be in range [0, 1]");

            if (float.IsNaN(min))
                problems.Add("Minimum bound cannot be NaN");
            else if (float.IsInfinity(min))
                problems.Add("Minimum bound cannot be infinite");

            if (float.IsNaN(max))
                problems.Add("Maximum bound cannot be NaN");
            else if (float.IsInfinity(max))
                problems.Add("Maximum bound cannot be infinite");
            else if (max <= min)
                problems.Add("Maximum bound must be greater than minimum bound");

            return problems.AsReadOnly();
        }
    }
}