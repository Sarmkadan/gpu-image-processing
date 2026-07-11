#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Services;

/// <summary>
/// Extension methods for <see cref="GpuManagementService"/> providing additional GPU management functionality.
/// </summary>
public static class GpuManagementServiceExtensions
{
    /// <summary>
    /// Checks if any GPU device is available for processing.
    /// </summary>
    /// <param name="service">The GPU management service instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <returns><see langword="true"/> if at least one device is available; otherwise, <see langword="false"/>.</returns>
    public static bool HasAvailableDevices(this GpuManagementService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetAvailableDevices().Any();
    }

    /// <summary>
    /// Gets the device with the highest performance score that meets the specified memory requirement.
    /// </summary>
    /// <param name="service">The GPU management service instance.</param>
    /// <param name="requiredMemory">The minimum required memory in bytes.</param>
    /// <param name="requiredComputeUnits">The minimum required compute units (default: 1).</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="requiredMemory"/> or <paramref name="requiredComputeUnits"/> is not positive.</exception>
    /// <returns>The best device that meets the requirements, or <see langword="null"/> if none found.</returns>
    public static GpuDevice? GetBestDeviceFor(this GpuManagementService service, long requiredMemory, int requiredComputeUnits = 1)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (requiredMemory <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredMemory), "Memory requirement must be positive.");
        }

        if (requiredComputeUnits <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredComputeUnits), "Compute units requirement must be positive.");
        }

        return service.GetAvailableDevices()
            .Where(d => d.IsAvailable && d.Validate())
            .Where(d => d.HasSufficientMemory(requiredMemory))
            .Where(d => d.MaxComputeUnits >= requiredComputeUnits)
            .OrderByDescending(d => d.CalculatePerformanceScore())
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets a dictionary of memory statistics grouped by device.
    /// </summary>
    /// <param name="service">The GPU management service instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <returns>A dictionary mapping device names to their memory statistics.</returns>
    public static Dictionary<string, Dictionary<string, object>> GetPerDeviceMemoryStatistics(this GpuManagementService service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var devices = service.GetAvailableDevices().ToList();
        var result = new Dictionary<string, Dictionary<string, object>>(devices.Count);

        foreach (var device in devices)
        {
            result[device.Name] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["DeviceId"] = device.Id,
                ["TotalMemory"] = device.GlobalMemoryBytes,
                ["AvailableMemory"] = device.GetAvailableMemory(),
                ["AllocatedMemory"] = device.GlobalMemoryBytes - device.GetAvailableMemory(),
                ["MemoryUsagePercent"] = device.GetMemoryUsagePercent(),
                ["MaxComputeUnits"] = device.MaxComputeUnits,
                ["ComputeCapability"] = device.ComputeCapabilityMajor > 0 && device.ComputeCapabilityMinor > 0
                    ? $"{device.ComputeCapabilityMajor}.{device.ComputeCapabilityMinor}"
                    : "N/A"
            };
        }

        return result;
    }

    /// <summary>
    /// Attempts to allocate memory on the best available device that meets the requirements.
    /// </summary>
    /// <param name="service">The GPU management service instance.</param>
    /// <param name="bytes">The number of bytes to allocate.</param>
    /// <param name="requiredComputeUnits">The minimum required compute units (default: 1).</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bytes"/> or <paramref name="requiredComputeUnits"/> is not positive.</exception>
    /// <returns><see langword="true"/> if allocation succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryAllocateOnBestDevice(this GpuManagementService service, long bytes, int requiredComputeUnits = 1)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (bytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "Allocation size must be positive.");
        }

        if (requiredComputeUnits <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredComputeUnits), "Compute units requirement must be positive.");
        }

        var bestDevice = service.GetBestDeviceFor(bytes, requiredComputeUnits);

        if (bestDevice is null)
        {
            return false;
        }

        try
        {
            return service.AllocateMemory(bytes, bestDevice.Id);
        }
        catch (GpuException)
        {
            return false;
        }
    }
}