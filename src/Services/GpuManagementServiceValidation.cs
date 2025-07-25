#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Services;

/// <summary>
/// Provides validation helpers for <see cref="GpuManagementService"/> instances.
/// </summary>
public static class GpuManagementServiceValidation
{
    /// <summary>
    /// Validates a <see cref="GpuManagementService"/> instance and returns any problems found.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>List of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GpuManagementService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate service state
        if (value.UseFallback)
        {
            problems.Add("Service is using CPU fallback mode, GPU acceleration is not available.");
        }

        // Validate total allocated memory
        var totalAllocated = value.GetTotalAllocatedMemory();
        if (totalAllocated < 0)
        {
            problems.Add("Total allocated memory is negative, which indicates memory tracking corruption.");
        }

        // Validate devices
        var devices = value.GetAvailableDevices();
        if (devices == null)
        {
            problems.Add("GetAvailableDevices() returned null instead of an enumerable.");
        }
        else
        {
            foreach (var device in devices)
            {
                problems.AddRange(Validate(device));
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="GpuDevice"/> instance and returns any problems found.
    /// </summary>
    /// <param name="device">The device to validate.</param>
    /// <returns>List of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this GpuDevice device)
    {
        if (device == null)
        {
            return ["Device reference is null."];
        }

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(device.Name))
        {
            problems.Add("Device.Name is null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(device.Vendor))
        {
            problems.Add("Device.Vendor is null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(device.Version))
        {
            problems.Add("Device.Version is null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(device.Driver))
        {
            problems.Add("Device.Driver is null, empty, or whitespace.");
        }

        // Validate numeric properties
        if (device.GlobalMemoryBytes <= 0)
        {
            problems.Add("Device.GlobalMemoryBytes must be positive, but was " + device.GlobalMemoryBytes.ToString(CultureInfo.InvariantCulture));
        }

        if (device.LocalMemoryBytes < 0)
        {
            problems.Add("Device.LocalMemoryBytes must be non-negative, but was " + device.LocalMemoryBytes.ToString(CultureInfo.InvariantCulture));
        }

        if (device.MaxAllocatableMemoryBytes < 0)
        {
            problems.Add("Device.MaxAllocatableMemoryBytes must be non-negative, but was " + device.MaxAllocatableMemoryBytes.ToString(CultureInfo.InvariantCulture));
        }

        if (device.MaxComputeUnits <= 0)
        {
            problems.Add("Device.MaxComputeUnits must be positive, but was " + device.MaxComputeUnits.ToString(CultureInfo.InvariantCulture));
        }

        if (device.MaxWorkGroupSize <= 0)
        {
            problems.Add("Device.MaxWorkGroupSize must be positive, but was " + device.MaxWorkGroupSize.ToString(CultureInfo.InvariantCulture));
        }

        if (device.MaxWorkItemDimensions < 0)
        {
            problems.Add("Device.MaxWorkItemDimensions must be non-negative, but was " + device.MaxWorkItemDimensions.ToString(CultureInfo.InvariantCulture));
        }

        if (device.MaxWorkItemSizes == null)
        {
            problems.Add("Device.MaxWorkItemSizes is null.");
        }
        else if (device.MaxWorkItemSizes.Length != device.MaxWorkItemDimensions)
        {
            problems.Add("Device.MaxWorkItemSizes length " + device.MaxWorkItemSizes.Length + " does not match MaxWorkItemDimensions " + device.MaxWorkItemDimensions + ".");
        }

        if (device.MaxClockFrequencyMhz < 0)
        {
            problems.Add("Device.MaxClockFrequencyMhz must be non-negative, but was " + device.MaxClockFrequencyMhz.ToString(CultureInfo.InvariantCulture));
        }

        if (device.ComputeCapabilityMajor < 0)
        {
            problems.Add("Device.ComputeCapabilityMajor must be non-negative, but was " + device.ComputeCapabilityMajor.ToString(CultureInfo.InvariantCulture));
        }

        if (device.ComputeCapabilityMinor < 0)
        {
            problems.Add("Device.ComputeCapabilityMinor must be non-negative, but was " + device.ComputeCapabilityMinor.ToString(CultureInfo.InvariantCulture));
        }

        if (device.WavefrontSize < 0)
        {
            problems.Add("Device.WavefrontSize must be non-negative, but was " + device.WavefrontSize.ToString(CultureInfo.InvariantCulture));
        }

        // Validate device type
        if (device.DeviceType == GpuDeviceType.Unknown)
        {
            problems.Add("Device.DeviceType is Unknown, which is not a valid device type.");
        }

        // Validate availability
        if (!device.IsAvailable)
        {
            problems.Add("Device.IsAvailable is false, device is not available for use.");
        }

        // Validate timestamp
        if (device.DetectedAt == default)
        {
            problems.Add("Device.DetectedAt is default(DateTime), device detection timestamp is missing.");
        }
        else if (device.DetectedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Device.DetectedAt is in the future: " + device.DetectedAt.ToString("O", CultureInfo.InvariantCulture));
        }

        // Validate memory consistency
        if (device.MaxAllocatableMemoryBytes > device.GlobalMemoryBytes)
        {
            problems.Add("Device.MaxAllocatableMemoryBytes (" + device.MaxAllocatableMemoryBytes.ToString(CultureInfo.InvariantCulture) +
                       ") exceeds GlobalMemoryBytes (" + device.GlobalMemoryBytes.ToString(CultureInfo.InvariantCulture) + ").");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="GpuManagementService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this GpuManagementService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="GpuManagementService"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this GpuManagementService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException("GpuManagementService validation failed:\n" + string.Join("\n", problems), nameof(value));
        }
    }

    /// <summary>
    /// Validates a device ID and memory requirements for allocation.
    /// </summary>
    /// <param name="service">The GPU management service.</param>
    /// <param name="deviceId">The device ID to validate.</param>
    /// <param name="requiredMemory">Required memory in bytes.</param>
    /// <param name="requiredComputeUnits">Required compute units.</param>
    /// <returns>List of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
    public static IReadOnlyList<string> ValidateDeviceAllocation(
        this GpuManagementService service,
        Guid deviceId,
        long requiredMemory,
        int requiredComputeUnits = 1)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = new List<string>();

        if (requiredMemory <= 0)
        {
            problems.Add("Required memory must be positive, but was " + requiredMemory.ToString(CultureInfo.InvariantCulture));
        }

        if (requiredComputeUnits <= 0)
        {
            problems.Add("Required compute units must be positive, but was " + requiredComputeUnits.ToString(CultureInfo.InvariantCulture));
        }

        // Check if device exists and is available
        var device = service.GetDeviceById(deviceId);
        if (device == null)
        {
            problems.Add("Device with ID " + deviceId + " not found.");
            return problems.AsReadOnly();
        }

        // Validate device itself
        problems.AddRange(GpuManagementServiceValidation.Validate(device));

        if (device.IsAvailable == false)
        {
            problems.Add("Device " + device.Name + " is not available.");
        }

        // Check memory availability
        if (!device.HasSufficientMemory(requiredMemory))
        {
            problems.Add("Device " + device.Name + " has insufficient memory. Required: " +
                       FormatBytes(requiredMemory) + ", Available: " + FormatBytes(device.GetAvailableMemory()) + ".");
        }

        // Check compute units
        if (device.MaxComputeUnits < requiredComputeUnits)
        {
            problems.Add("Device " + device.Name + " has " + device.MaxComputeUnits +
                       " compute units, but " + requiredComputeUnits + " are required.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a device can be used for allocation.
    /// </summary>
    /// <param name="service">The GPU management service.</param>
    /// <param name="deviceId">The device ID to check.</param>
    /// <param name="requiredMemory">Required memory in bytes.</param>
    /// <param name="requiredComputeUnits">Required compute units.</param>
    /// <returns><see langword="true"/> if the device can be used; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
    public static bool CanAllocate(
        this GpuManagementService service,
        Guid deviceId,
        long requiredMemory,
        int requiredComputeUnits = 1)
    {
        return ValidateDeviceAllocation(service, deviceId, requiredMemory, requiredComputeUnits).Count == 0;
    }

    /// <summary>
    /// Ensures that a device can be used for allocation, throwing if not.
    /// </summary>
    /// <param name="service">The GPU management service.</param>
    /// <param name="deviceId">The device ID to check.</param>
    /// <param name="requiredMemory">Required memory in bytes.</param>
    /// <param name="requiredComputeUnits">Required compute units.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if allocation is not possible.</exception>
    public static void EnsureCanAllocate(
        this GpuManagementService service,
        Guid deviceId,
        long requiredMemory,
        int requiredComputeUnits = 1)
    {
        ArgumentNullException.ThrowIfNull(service);

        var problems = ValidateDeviceAllocation(service, deviceId, requiredMemory, requiredComputeUnits);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Device allocation validation failed:\n" + string.Join("\n", problems), nameof(deviceId));
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int counter = 0;
        double value = bytes;

        while (value >= 1024 && counter < suffixes.Length - 1)
        {
            value /= 1024;
            counter++;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", value, suffixes[counter]);
    }
}