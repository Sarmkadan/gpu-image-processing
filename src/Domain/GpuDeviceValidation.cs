#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="GpuDevice"/> instances.
/// </summary>
public static class GpuDeviceValidation
{
    /// <summary>
    /// Validates a <see cref="GpuDevice"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The device to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateDevice(this GpuDevice? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id cannot be empty (Guid.Empty).");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name cannot be null, empty, or whitespace.");
        }
        else if (value.Name.Length > 256)
        {
            errors.Add("Name cannot exceed 256 characters.");
        }

        // Validate DeviceType
        if (value.DeviceType == GpuImageProcessing.Core.GpuDeviceType.Unknown)
        {
            errors.Add("DeviceType cannot be Unknown.");
        }

        // Validate Vendor
        if (string.IsNullOrWhiteSpace(value.Vendor))
        {
            errors.Add("Vendor cannot be null, empty, or whitespace.");
        }
        else if (value.Vendor.Length > 128)
        {
            errors.Add("Vendor cannot exceed 128 characters.");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add("Version cannot be null, empty, or whitespace.");
        }
        else if (value.Version.Length > 64)
        {
            errors.Add("Version cannot exceed 64 characters.");
        }

        // Validate Driver
        if (string.IsNullOrWhiteSpace(value.Driver))
        {
            errors.Add("Driver cannot be null, empty, or whitespace.");
        }
        else if (value.Driver.Length > 128)
        {
            errors.Add("Driver cannot exceed 128 characters.");
        }

        // Validate memory values
        if (value.GlobalMemoryBytes <= 0)
        {
            errors.Add("GlobalMemoryBytes must be greater than zero.");
        }

        if (value.LocalMemoryBytes < 0)
        {
            errors.Add("LocalMemoryBytes cannot be negative.");
        }

        if (value.MaxAllocatableMemoryBytes <= 0)
        {
            errors.Add("MaxAllocatableMemoryBytes must be greater than zero.");
        }
        else if (value.MaxAllocatableMemoryBytes > value.GlobalMemoryBytes)
        {
            errors.Add("MaxAllocatableMemoryBytes cannot exceed GlobalMemoryBytes.");
        }

        // Validate compute units
        if (value.MaxComputeUnits <= 0)
        {
            errors.Add("MaxComputeUnits must be greater than zero.");
        }
        else if (value.MaxComputeUnits > 1024)
        {
            errors.Add("MaxComputeUnits cannot exceed 1024.");
        }

        // Validate work group size
        if (value.MaxWorkGroupSize <= 0)
        {
            errors.Add("MaxWorkGroupSize must be greater than zero.");
        }
        else if (value.MaxWorkGroupSize > 1048576) // 1M
        {
            errors.Add("MaxWorkGroupSize cannot exceed 1,048,576.");
        }

        // Validate work item dimensions
        if (value.MaxWorkItemDimensions <= 0)
        {
            errors.Add("MaxWorkItemDimensions must be greater than zero.");
        }
        else if (value.MaxWorkItemDimensions > 16)
        {
            errors.Add("MaxWorkItemDimensions cannot exceed 16.");
        }

        // Validate MaxWorkItemSizes
        if (value.MaxWorkItemSizes is null)
        {
            errors.Add("MaxWorkItemSizes cannot be null.");
        }
        else if (value.MaxWorkItemSizes.Length != value.MaxWorkItemDimensions)
        {
            errors.Add("MaxWorkItemSizes array length must match MaxWorkItemDimensions.");
        }
        else
        {
            for (var i = 0; i < value.MaxWorkItemSizes.Length; i++)
            {
                if (value.MaxWorkItemSizes[i] <= 0)
                {
                    errors.Add($"MaxWorkItemSizes[{i}] must be greater than zero.");
                }
            }
        }

        // Validate clock frequency
        if (value.MaxClockFrequencyMhz <= 0)
        {
            errors.Add("MaxClockFrequencyMhz must be greater than zero.");
        }
        else if (value.MaxClockFrequencyMhz > 100000) // 100 GHz
        {
            errors.Add("MaxClockFrequencyMhz cannot exceed 100,000 MHz.");
        }

        // Validate compute capability
        if (value.ComputeCapabilityMajor < 0)
        {
            errors.Add("ComputeCapabilityMajor cannot be negative.");
        }
        else if (value.ComputeCapabilityMajor > 100)
        {
            errors.Add("ComputeCapabilityMajor cannot exceed 100.");
        }

        if (value.ComputeCapabilityMinor < 0)
        {
            errors.Add("ComputeCapabilityMinor cannot be negative.");
        }
        else if (value.ComputeCapabilityMinor > 100)
        {
            errors.Add("ComputeCapabilityMinor cannot exceed 100.");
        }

        // Validate DetectedAt
        var defaultDate = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        if (value.DetectedAt == defaultDate)
        {
            errors.Add("DetectedAt cannot be the default DateTime value.");
        }
        else if (value.DetectedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("DetectedAt cannot be in the future.");
        }

        // Validate Extensions
        if (value.Extensions is null)
        {
            errors.Add("Extensions dictionary cannot be null.");
        }
        else if (value.Extensions.Count > 1024)
        {
            errors.Add("Extensions dictionary cannot contain more than 1024 entries.");
        }
        else
        {
            foreach (var (key, val) in value.Extensions)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    errors.Add("Extensions dictionary contains a null or empty key.");
                    break;
                }

                if (key.Length > 256)
                {
                    errors.Add("Extension key cannot exceed 256 characters.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(val))
                {
                    errors.Add($"Extensions dictionary contains a null or empty value for key '{key}'");
                    break;
                }

                if (val.Length > 1024)
                {
                    errors.Add($"Extension value for key '{key}' cannot exceed 1024 characters.");
                    break;
                }
            }
        }

        // Validate SupportedFormats
        if (value.SupportedFormats is null)
        {
            errors.Add("SupportedFormats list cannot be null.");
        }
        else if (value.SupportedFormats.Count > 128)
        {
            errors.Add("SupportedFormats list cannot contain more than 128 entries.");
        }
        else
        {
            for (var i = 0; i < value.SupportedFormats.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(value.SupportedFormats[i]))
                {
                    errors.Add($"SupportedFormats[{i}] cannot be null, empty, or whitespace.");
                    break;
                }

                if (value.SupportedFormats[i].Length > 32)
                {
                    errors.Add($"SupportedFormats[{i}] cannot exceed 32 characters.");
                    break;
                }
            }
        }

        // Validate WavefrontSize
        if (value.WavefrontSize < 0)
        {
            errors.Add("WavefrontSize cannot be negative.");
        }
        else if (value.WavefrontSize > 1024)
        {
            errors.Add("WavefrontSize cannot exceed 1024.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="GpuDevice"/> instance is valid.
    /// </summary>
    /// <param name="value">The device to check.</param>
    /// <returns>True if the device is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this GpuDevice? value)
    {
        return value is not null && (value.ValidateDevice()).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="GpuDevice"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with detailed validation messages if it is not.
    /// </summary>
    /// <param name="value">The device to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the device is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this GpuDevice? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateDevice();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"GPU device validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}
