#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Provides extension methods for the <see cref="DeviceService"/> class.
    /// </summary>
    public static class DeviceServiceExtensions
    {
        /// <summary>
        /// Gets the top N most capable available devices.
        /// </summary>
        /// <param name="deviceService">The device service instance.</param>
        /// <param name="topN">The number of top devices to return.</param>
        /// <returns>An <see cref="IReadOnlyList{DeviceInfo}"/> containing the top N devices.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceService"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="topN"/> is less than or equal to zero.</exception>
        public static async Task<IReadOnlyList<DeviceInfo>> GetTopNDevicesAsync(this DeviceService deviceService, int topN)
        {
            ArgumentNullException.ThrowIfNull(deviceService);
            if (topN <= 0)
                throw new ArgumentOutOfRangeException(nameof(topN), "Value must be greater than zero.");

            var allDevices = await deviceService.GetAllDevicesAsync();
            var availableDevices = allDevices.Where(d => d.IsAvailable).ToList();
            return availableDevices
                .OrderByDescending(d => d.GetCapabilityScore())
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// Finds the first available device that supports double precision and has sufficient memory for the specified image size.
        /// </summary>
        /// <param name="deviceService">The device service instance.</param>
        /// <param name="requiredMemoryBytes">The required memory in bytes for the image processing task.</param>
        /// <returns>A <see cref="DeviceInfo"/> if a suitable device is found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceService"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="requiredMemoryBytes"/> is less than or equal to zero.</exception>
        public static async Task<DeviceInfo?> FindSuitableDeviceForImageAsync(this DeviceService deviceService, long requiredMemoryBytes)
        {
            ArgumentNullException.ThrowIfNull(deviceService);
            if (requiredMemoryBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredMemoryBytes), "Value must be greater than zero.");

            var allDevices = await deviceService.GetAllDevicesAsync();
            return allDevices
                .Where(d => d.IsAvailable && d.SupportsDoublePrecision)
                .OrderByDescending(d => d.GetCapabilityScore())
                .FirstOrDefault(d => deviceService.HasSufficientMemoryAsync(d.Id, requiredMemoryBytes).Result);
        }

        /// <summary>
        /// Gets all available devices that support double precision floating-point operations.
        /// </summary>
        /// <param name="deviceService">The device service instance.</param>
        /// <returns>An <see cref="IReadOnlyList{DeviceInfo}"/> containing devices that support double precision.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceService"/> is null.</exception>
        public static async Task<IReadOnlyList<DeviceInfo>> GetDevicesSupportingDoublePrecisionAsync(this DeviceService deviceService)
        {
            ArgumentNullException.ThrowIfNull(deviceService);
            return (await deviceService.GetAllDevicesAsync())
                .Where(d => d.IsAvailable && d.SupportsDoublePrecision)
                .ToList();
        }

        /// <summary>
        /// Gets all available devices with a capability score greater than or equal to the specified threshold.
        /// </summary>
        /// <param name="deviceService">The device service instance.</param>
        /// <param name="minCapabilityScore">The minimum capability score threshold.</param>
        /// <returns>An <see cref="IReadOnlyList{DeviceInfo}"/> containing devices meeting the capability threshold.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="deviceService"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="minCapabilityScore"/> is less than zero.</exception>
        public static async Task<IReadOnlyList<DeviceInfo>> GetDevicesByCapabilityScoreThresholdAsync(
            this DeviceService deviceService,
            int minCapabilityScore)
        {
            ArgumentNullException.ThrowIfNull(deviceService);
            if (minCapabilityScore < 0)
                throw new ArgumentOutOfRangeException(nameof(minCapabilityScore), "Value must be non-negative.");

            return (await deviceService.GetAllDevicesAsync())
                .Where(d => d.IsAvailable && d.GetCapabilityScore() >= minCapabilityScore)
                .ToList();
        }
    }
}
