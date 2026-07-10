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
        public static long GetTotalMemoryMb(this GpuDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            return device.GlobalMemoryBytes / (1024 * 1024);
        }

        /// <summary>
        /// Gets the available memory in megabytes.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>Available memory in MB.</returns>
        public static long GetAvailableMemoryMb(this GpuDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            return device.MaxAllocatableMemoryBytes / (1024 * 1024);
        }

        /// <summary>
        /// Checks if the device supports image formats for the specified color space.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <param name="colorSpace">The color space to check (e.g., "RGB", "RGBA", "sRGB").</param>
        /// <returns>True if any supported format matches the color space.</returns>
        public static bool SupportsColorSpace(this GpuDevice device, string colorSpace)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (string.IsNullOrWhiteSpace(colorSpace))
                return false;

            return device.SupportedFormats.Any(format =>
                format.Contains(colorSpace, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the device type as a user-friendly display name.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>User-friendly device type name.</returns>
        public static string GetDeviceTypeDisplayName(this GpuDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

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