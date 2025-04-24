// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utilities for converting between different data formats and types.
    /// Handles byte array conversions, number formatting, and data serialization.
    /// </summary>
    public static class DataConversionUtilities
    {
        /// <summary>
        /// Converts byte array to hexadecimal string
        /// </summary>
        public static string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /// <summary>
        /// Converts hexadecimal string to byte array
        /// </summary>
        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hex string", nameof(hex));

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        /// <summary>
        /// Converts floating point array to byte array (little-endian)
        /// </summary>
        public static byte[] FloatsToBytes(float[] floats)
        {
            if (floats == null || floats.Length == 0)
                return new byte[0];

            var bytes = new byte[floats.Length * sizeof(float)];
            Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// Converts byte array back to floating point array (little-endian)
        /// </summary>
        public static float[] BytesToFloats(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return new float[0];

            if (bytes.Length % sizeof(float) != 0)
                throw new ArgumentException("Byte array length must be multiple of 4", nameof(bytes));

            var floats = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            return floats;
        }

        /// <summary>
        /// Formats file size in human-readable format
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            var size = (double)bytes;
            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            var unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:F2} {units[unitIndex]}";
        }

        /// <summary>
        /// Parses file size from human-readable format back to bytes
        /// </summary>
        public static long ParseFileSize(string sizeString)
        {
            if (string.IsNullOrEmpty(sizeString))
                return 0;

            var units = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
            {
                { "B", 1 },
                { "KB", 1024 },
                { "MB", 1024 * 1024 },
                { "GB", 1024 * 1024 * 1024 },
                { "TB", 1024L * 1024 * 1024 * 1024 }
            };

            var parts = sizeString.Trim().Split(' ');
            if (parts.Length != 2)
                throw new FormatException("Invalid size format. Expected: <number> <unit>");

            if (!double.TryParse(parts[0], out var value))
                throw new FormatException($"Invalid number: {parts[0]}");

            var unit = parts[1];
            if (!units.TryGetValue(unit, out var multiplier))
                throw new FormatException($"Unknown unit: {unit}");

            return (long)(value * multiplier);
        }

        /// <summary>
        /// Converts timespan to human-readable string
        /// </summary>
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 1)
                return $"{timeSpan.TotalMilliseconds:F0}ms";

            if (timeSpan.TotalMinutes < 1)
                return $"{timeSpan.TotalSeconds:F1}s";

            if (timeSpan.TotalHours < 1)
                return $"{timeSpan.TotalMinutes:F1}m";

            if (timeSpan.TotalDays < 1)
                return $"{timeSpan.TotalHours:F1}h";

            return $"{timeSpan.TotalDays:F1}d";
        }

        /// <summary>
        /// Parses duration string (e.g., "5h 30m 45s") to TimeSpan
        /// </summary>
        public static TimeSpan ParseDuration(string durationString)
        {
            if (string.IsNullOrEmpty(durationString))
                return TimeSpan.Zero;

            var timeSpan = TimeSpan.Zero;
            var parts = durationString.ToLower().Split(' ');

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.Length < 2)
                    continue;

                var unit = part[^1];
                if (!double.TryParse(part.Substring(0, part.Length - 1), out var value))
                    continue;

                timeSpan = timeSpan.Add(unit switch
                {
                    'd' => TimeSpan.FromDays(value),
                    'h' => TimeSpan.FromHours(value),
                    'm' => TimeSpan.FromMinutes(value),
                    's' => TimeSpan.FromSeconds(value),
                    _ => TimeSpan.Zero
                });
            }

            return timeSpan;
        }

        /// <summary>
        /// Converts integer to binary string representation
        /// </summary>
        public static string ToBinaryString(int value, int minWidth = 8)
        {
            var binary = Convert.ToString(value, 2);
            return binary.PadLeft(minWidth, '0');
        }

        /// <summary>
        /// Checks if value is within expected range with tolerance
        /// </summary>
        public static bool IsWithinTolerance(float actual, float expected, float tolerance)
        {
            var difference = Math.Abs(actual - expected);
            return difference <= tolerance;
        }

        /// <summary>
        /// Normalizes value to 0-1 range based on min-max bounds
        /// </summary>
        public static float Normalize(float value, float min, float max)
        {
            if (max <= min)
                return 0;

            return (value - min) / (max - min);
        }

        /// <summary>
        /// Denormalizes value from 0-1 range back to original bounds
        /// </summary>
        public static float Denormalize(float normalized, float min, float max)
        {
            return min + (normalized * (max - min));
        }
    }
}
