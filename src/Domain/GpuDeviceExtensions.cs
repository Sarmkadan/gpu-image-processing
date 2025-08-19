#nullable enable

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using System;
using System.Linq;

namespace GpuImageProcessing.Domain
{
    /// <summary>
    /// Extension methods for <see cref="GpuDevice"/> providing additional functionality.
    /// </summary>
    public static class GpuDeviceExtensions
    {
        /// <summary>
        /// Gets the total memory in megabytes.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>Total memory in MB.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public static long GetTotalMemoryMb(this GpuDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);

            return device.GlobalMemoryBytes / (1024L * 1024);
        }

        /// <summary>
        /// Gets the available memory in megabytes.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>Available memory in MB.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public static long GetAvailableMemoryMb(this GpuDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);

            return device.MaxAllocatableMemoryBytes / (1024L * 1024);
        }

        /// <summary>
        /// Checks if the device supports image formats for the specified color space.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <param name="colorSpace">The color space to check (e.g., "RGB", "RGBA", "sRGB").</param>
        /// <returns>True if any supported format matches the color space.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public static bool SupportsColorSpace(this GpuDevice device, string colorSpace)
        {
            ArgumentNullException.ThrowIfNull(device);

            if (string.IsNullOrWhiteSpace(colorSpace))
            {
                return false;
            }

            return device.SupportedFormats.Any(format =>
                format.Contains(colorSpace, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the device type as a user-friendly display name.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>User-friendly device type name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public static string GetDeviceTypeDisplayName(this GpuDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);

            return device.DeviceType switch
            {
                GpuDeviceType.Gpu => "GPU",
                GpuDeviceType.Cpu => "CPU",
                GpuDeviceType.Accelerator => "Accelerator",
                GpuDeviceType.Custom => "Custom",
                _ => "Unknown Device Type"
            };
        }
    }
}