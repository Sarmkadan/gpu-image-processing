#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Provides validation helpers for DeviceService
    /// </summary>
    public static class DeviceServiceValidation
    {
        /// <summary>
        /// Validates a DeviceService instance and returns a list of human-readable problems
        /// </summary>
        /// <param name="value">The DeviceService instance to validate</param>
        /// <returns>List of validation problems; empty if valid</returns>
        public static IReadOnlyList<string> Validate(this DeviceService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate service state by checking statistics
            try
            {
                var stats = value.GetStatisticsAsync().GetAwaiter().GetResult();

                if (stats.TotalDevices < 0)
                {
                    problems.Add("TotalDevices cannot be negative");
                }

                if (stats.AvailableDevices < 0)
                {
                    problems.Add("AvailableDevices cannot be negative");
                }

                if (stats.GpuDevices < 0)
                {
                    problems.Add("GpuDevices cannot be negative");
                }

                if (stats.CpuDevices < 0)
                {
                    problems.Add("CpuDevices cannot be negative");
                }

                if (stats.TotalMemoryBytes < 0)
                {
                    problems.Add("TotalMemoryBytes cannot be negative");
                }

                if (stats.TotalComputeUnits < 0)
                {
                    problems.Add("TotalComputeUnits cannot be negative");
                }

                if (stats.AverageCapabilityScore < 0)
                {
                    problems.Add("AverageCapabilityScore cannot be negative");
                }
            }
            catch (Exception ex)
            {
                problems.Add($"Failed to retrieve device statistics: {ex.Message}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Checks if a DeviceService instance is valid
        /// </summary>
        /// <param name="value">The DeviceService instance to check</param>
        /// <returns>True if valid; otherwise false</returns>
        public static bool IsValid(this DeviceService value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a DeviceService instance is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The DeviceService instance to validate</param>
        /// <exception cref="ArgumentException">Thrown when the DeviceService is invalid</exception>
        public static void EnsureValid(this DeviceService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"DeviceService is invalid:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }
}