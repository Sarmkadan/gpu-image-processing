#nullable enable

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using System;
using System.Linq;
using System.Reflection;

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
        /// Gets the device type as a user‑friendly display name.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>User‑friendly device type name.</returns>
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

        /// <summary>
        /// Determines whether the device can support a workgroup size of the given dimensions.
        /// The implementation attempts to read maximum workgroup size information via reflection,
        /// supporting a few common naming conventions.
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <param name="x">Workgroup size in the X dimension.</param>
        /// <param name="y">Workgroup size in the Y dimension.</param>
        /// <param name="z">Workgroup size in the Z dimension.</param>
        /// <returns>True if the requested workgroup size is within the device limits.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public static bool SupportsWorkgroupSize(this GpuDevice device, int x, int y, int z)
        {
            ArgumentNullException.ThrowIfNull(device);

            if (x <= 0 || y <= 0 || z <= 0)
                return false;

            // Try common property names for max workgroup dimensions.
            var type = typeof(GpuDevice);
            var maxXProp = type.GetProperty("MaxWorkGroupSizeX", BindingFlags.Instance | BindingFlags.Public);
            var maxYProp = type.GetProperty("MaxWorkGroupSizeY", BindingFlags.Instance | BindingFlags.Public);
            var maxZProp = type.GetProperty("MaxWorkGroupSizeZ", BindingFlags.Instance | BindingFlags.Public);

            if (maxXProp?.GetValue(device) is int maxX &&
                maxYProp?.GetValue(device) is int maxY &&
                maxZProp?.GetValue(device) is int maxZ)
            {
                return x <= maxX && y <= maxY && z <= maxZ;
            }

            // Fallback: a single property returning an int[3] or a tuple.
            var maxArrayProp = type.GetProperty("MaxWorkGroupSize", BindingFlags.Instance | BindingFlags.Public);
            if (maxArrayProp?.GetValue(device) is int[] arr && arr.Length >= 3)
            {
                return x <= arr[0] && y <= arr[1] && z <= arr[2];
            }

            // Fallback: a property returning a ValueTuple<int,int,int>
            if (maxArrayProp?.GetValue(device) is ValueTuple<int, int, int> tuple)
            {
                return x <= tuple.Item1 && y <= tuple.Item2 && z <= tuple.Item3;
            }

            // If we cannot determine the limits, conservatively return false.
            return false;
        }

        /// <summary>
        /// Returns a concise, human‑readable description of the device.
        /// Example: "GPU - 8192 MB".
        /// </summary>
        /// <param name="device">The GPU device.</param>
        /// <returns>A short description string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
        public static string ToShortDescription(this GpuDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);

            var typeName = device.GetDeviceTypeDisplayName();
            var totalMb = device.GetTotalMemoryMb();

            return $"{typeName} - {totalMb} MB";
        }
    }
}
