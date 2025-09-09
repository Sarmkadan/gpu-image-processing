#nullable enable
// =============================================================================
// Author: 
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

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

            return new Dictionary<string, object>
            {
                { "DeviceName", deviceInfo.Name },
                { "DeviceType", deviceInfo.DeviceType },
                { "MemoryBytes", deviceInfo.GlobalMemoryBytes },
                { "ComputeUnits", deviceInfo.ComputeUnits },
                { "ClockFrequencyMHz", deviceInfo.ClockFrequencyMHz },
                { "OpenCLVersion", deviceInfo.OpenCLVersion },
                { "CapabilityScore", deviceInfo.GetCapabilityScore() }
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
    }
}
