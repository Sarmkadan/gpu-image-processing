#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="DeviceInfo"/> class.
    /// </summary>
    public static class DeviceInfoExtensions
    {
        /// <summary>
        /// Determines whether the device has sufficient compute resources for a task.
        /// </summary>
        /// <param name="deviceInfo">The <see cref="DeviceInfo"/> to check.</param>
        /// <param name="requiredComputeUnits">The required number of compute units.</param>
        /// <param name="requiredMemoryBytes">The required amount of memory in bytes.</param>
        /// <returns><see langword="true"/> if the device has sufficient resources.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceInfo"/> is null.</exception>
        public static bool HasSufficientResources(this DeviceInfo deviceInfo, int requiredComputeUnits, long requiredMemoryBytes)
        {
            ArgumentNullException.ThrowIfNull(deviceInfo);

            return deviceInfo.ComputeUnits >= requiredComputeUnits && deviceInfo.HasSufficientMemory(requiredMemoryBytes);
        }

        /// <summary>
        /// Gets a summary of device capabilities in a machine-friendly format.
        /// </summary>
        /// <param name="deviceInfo">The <see cref="DeviceInfo"/> to summarize.</param>
        /// <returns>A dictionary containing device capability information.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceInfo"/> is null.</exception>
        public static IReadOnlyDictionary<string, object> GetCapabilitySummary(this DeviceInfo deviceInfo)
        {
            ArgumentNullException.ThrowIfNull(deviceInfo);

            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["DeviceName"] = deviceInfo.Name,
                ["DeviceType"] = deviceInfo.DeviceType,
                ["MemoryBytes"] = deviceInfo.GlobalMemoryBytes,
                ["ComputeUnits"] = deviceInfo.ComputeUnits,
                ["ClockFrequencyMHz"] = deviceInfo.ClockFrequencyMHz,
                ["OpenCLVersion"] = deviceInfo.OpenCLVersion,
                ["CapabilityScore"] = deviceInfo.GetCapabilityScore()
            };
        }

        /// <summary>
        /// Formats the device's clock frequency in a human-readable format.
        /// </summary>
        /// <param name="deviceInfo">The <see cref="DeviceInfo"/> to format.</param>
        /// <returns>A string representing the clock frequency.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceInfo"/> is null.</exception>
        public static string GetFormattedClockFrequency(this DeviceInfo deviceInfo)
        {
            ArgumentNullException.ThrowIfNull(deviceInfo);

            return $"{deviceInfo.ClockFrequencyMHz:F2} MHz";
        }

        /// <summary>
        /// Gets a formatted string representation of the device's memory capacity.
        /// </summary>
        /// <param name="deviceInfo">The <see cref="DeviceInfo"/> to format.</param>
        /// <returns>A human-readable string representing the device's memory capacity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceInfo"/> is null.</exception>
        public static string GetFormattedMemory(this DeviceInfo deviceInfo)
        {
            ArgumentNullException.ThrowIfNull(deviceInfo);

            return deviceInfo.GetFormattedMemory();
        }

        /// <summary>
        /// Gets a formatted string representation of the device's capabilities.
        /// </summary>
        /// <param name="deviceInfo">The <see cref="DeviceInfo"/> to format.</param>
        /// <returns>A multi-line string describing the device's capabilities.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceInfo"/> is null.</exception>
        public static string GetCapabilitiesSummary(this DeviceInfo deviceInfo)
        {
            ArgumentNullException.ThrowIfNull(deviceInfo);

            return deviceInfo.GetCapabilitiesSummary();
        }

        /// <summary>
        /// Gets the device's capability score as a formatted string.
        /// </summary>
        /// <param name="deviceInfo">The <see cref="DeviceInfo"/> to evaluate.</param>
        /// <returns>A string representation of the capability score (0-100).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceInfo"/> is null.</exception>
        public static string GetFormattedCapabilityScore(this DeviceInfo deviceInfo)
        {
            ArgumentNullException.ThrowIfNull(deviceInfo);

            return $"{deviceInfo.GetCapabilityScore()}/100";
        }
    }
}