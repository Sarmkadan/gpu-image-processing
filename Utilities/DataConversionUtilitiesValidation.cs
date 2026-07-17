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
    /// Validation helpers for DataConversionUtilities operations.
    /// Provides validation methods for all conversion operations.
    /// </summary>
    public static class DataConversionUtilitiesValidation
    {
        /// <summary>
        /// Checks if the DataConversionUtilities class is valid (always true for static class)
        /// </summary>
        /// <returns>True</returns>
        public static bool IsValid() => true;

        /// <summary>
        /// Ensures the DataConversionUtilities class is valid (no-op for static class)
        /// </summary>
        /// <exception cref="InvalidOperationException">Never thrown - static class has no state to validate</exception>
        public static void EnsureValid()
        {
            // Static class has no state to validate
        }

        /// <summary>
        /// Validates the BytesToHex method with provided parameters
        /// </summary>
        /// <param name="bytesToHex">The BytesToHex conversion method</param>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytesToHex"/> is null</exception>
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
        /// <param name="hexToBytes">The HexToBytes conversion method</param>
        /// <param name="hex">The hexadecimal string to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hexToBytes"/> is null</exception>
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
        /// <param name="floatsToBytes">The FloatsToBytes conversion method</param>
        /// <param name="floats">The float array to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="floatsToBytes"/> is null</exception>
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
        /// <param name="bytesToFloats">The BytesToFloats conversion method</param>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytesToFloats"/> is null</exception>
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
        /// <param name="formatFileSize">The FormatFileSize conversion method</param>
        /// <param name="bytes">The file size in bytes</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="formatFileSize"/> is null</exception>
        public static IReadOnlyList<string> Validate(this Func<long, string> formatFileSize, long bytes)
        {
            ArgumentNullException.ThrowIfNull(formatFileSize);
            var problems = new List<string>();

            if (bytes < 0)
                problems.Add("File size cannot be negative");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the ParseFileSize method with provided parameters
        /// </summary>
        /// <param name="parseFileSize">The ParseFileSize conversion method</param>
        /// <param name="sizeString">The human-readable size string</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parseFileSize"/> is null</exception>
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
        /// <param name="formatTimeSpan">The FormatTimeSpan conversion method</param>
        /// <param name="timeSpan">The TimeSpan to format</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="formatTimeSpan"/> is null</exception>
        public static IReadOnlyList<string> Validate(this Func<TimeSpan, string> formatTimeSpan, TimeSpan timeSpan)
        {
            ArgumentNullException.ThrowIfNull(formatTimeSpan);
            var problems = new List<string>();

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the ParseDuration method with provided parameters
        /// </summary>
        /// <param name="parseDuration">The ParseDuration conversion method</param>
        /// <param name="durationString">The duration string to parse</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parseDuration"/> is null</exception>
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
        /// <param name="valueToBinaryString">The ToBinaryString conversion method</param>
        /// <param name="value">The integer value to convert</param>
        /// <param name="minWidth">The minimum width for binary string</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="valueToBinaryString"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minWidth"/> is negative</exception>
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
        /// <param name="isWithinTolerance">The IsWithinTolerance comparison method</param>
        /// <param name="actual">The actual value to compare</param>
        /// <param name="expected">The expected value to compare against</param>
        /// <param name="tolerance">The tolerance threshold for comparison</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="isWithinTolerance"/> is null</exception>
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
        /// <param name="normalize">The Normalize conversion method</param>
        /// <param name="value">The value to normalize</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="normalize"/> is null</exception>
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
        /// <param name="denormalize">The Denormalize conversion method</param>
        /// <param name="normalized">The normalized value (0-1 range)</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="denormalize"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="normalized"/> is outside [0, 1] range</exception>
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