#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides validation helpers for <see cref="DeviceInfo"/> instances
    /// </summary>
    public static class DeviceInfoValidation
    {
        /// <summary>
        /// Validates a DeviceInfo instance and returns a list of human-readable problems
        /// </summary>
        /// <param name="value">The DeviceInfo instance to validate</param>
        /// <returns>A read-only list of validation error messages (empty if valid)</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this DeviceInfo value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Id
            if (value.Id == Guid.Empty)
            {
                errors.Add("DeviceInfo.Id must be a non-empty GUID");
            }

            // Validate Name
            ArgumentException.ThrowIfNullOrEmpty(value.Name?.Trim());
            if (value.Name.Length > 256)
            {
                errors.Add("DeviceInfo.Name cannot exceed 256 characters");
            }

            // Validate Vendor
            ArgumentException.ThrowIfNullOrEmpty(value.Vendor?.Trim());
            if (value.Vendor.Length > 128)
            {
                errors.Add("DeviceInfo.Vendor cannot exceed 128 characters");
            }

            // Validate DeviceType
            ArgumentException.ThrowIfNullOrEmpty(value.DeviceType?.Trim());
            if (value.DeviceType.Length > 64)
            {
                errors.Add("DeviceInfo.DeviceType cannot exceed 64 characters");
            }
            else if (value.DeviceType is not ("GPU" or "CPU" or "Accelerator"))
            {
                errors.Add("DeviceInfo.DeviceType must be one of: GPU, CPU, Accelerator");
            }

            // Validate GlobalMemoryBytes
            if (value.GlobalMemoryBytes < 0)
            {
                errors.Add("DeviceInfo.GlobalMemoryBytes cannot be negative");
            }
            else if (value.GlobalMemoryBytes == 0)
            {
                errors.Add("DeviceInfo.GlobalMemoryBytes should be greater than 0 for a valid device");
            }

            // Validate LocalMemoryBytes
            if (value.LocalMemoryBytes < 0)
            {
                errors.Add("DeviceInfo.LocalMemoryBytes cannot be negative");
            }

            // Validate ComputeUnits
            if (value.ComputeUnits < 0)
            {
                errors.Add("DeviceInfo.ComputeUnits cannot be negative");
            }

            // Validate MaxWorkGroupSize
            if (value.MaxWorkGroupSize <= 0)
            {
                errors.Add("DeviceInfo.MaxWorkGroupSize must be greater than 0");
            }

            // Validate MaxWorkItemDimensions
            if (value.MaxWorkItemDimensions <= 0)
            {
                errors.Add("DeviceInfo.MaxWorkItemDimensions must be greater than 0");
            }

            // Validate OpenCLVersion
            ArgumentException.ThrowIfNullOrEmpty(value.OpenCLVersion?.Trim());
            if (value.OpenCLVersion.Length > 64)
            {
                errors.Add("DeviceInfo.OpenCLVersion cannot exceed 64 characters");
            }

            // Validate DriverVersion
            ArgumentException.ThrowIfNullOrEmpty(value.DriverVersion?.Trim());
            if (value.DriverVersion.Length > 128)
            {
                errors.Add("DeviceInfo.DriverVersion cannot exceed 128 characters");
            }

            // Validate DetectedAt
            if (value.DetectedAt == default)
            {
                errors.Add("DeviceInfo.DetectedAt must be a valid DateTime (cannot be default)");
            }
            else if (value.DetectedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("DeviceInfo.DetectedAt cannot be in the future");
            }
            else if (value.DetectedAt < DateTime.UtcNow.AddYears(-1))
            {
                errors.Add("DeviceInfo.DetectedAt appears to be too old (older than 1 year)");
            }

            // Validate ClockFrequencyMHz
            if (value.ClockFrequencyMHz <= 0)
            {
                errors.Add("DeviceInfo.ClockFrequencyMHz must be greater than 0");
            }

            // Validate EstimatedFlops
            if (value.EstimatedFlops < 0)
            {
                errors.Add("DeviceInfo.EstimatedFlops cannot be negative");
            }

            // Validate Extensions
            ArgumentNullException.ThrowIfNull(value.Extensions);

            foreach (var kvp in value.Extensions)
            {
                ArgumentException.ThrowIfNullOrEmpty(kvp.Key?.Trim());
                if (kvp.Key.Length > 256)
                {
                    errors.Add("DeviceInfo.Extensions key cannot exceed 256 characters");
                    break;
                }
                if (kvp.Value is not null && kvp.Value.Length > 256)
                {
                    errors.Add("DeviceInfo.Extensions value cannot exceed 256 characters");
                    break;
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Checks if a DeviceInfo instance is valid
        /// </summary>
        /// <param name="value">The DeviceInfo instance to check</param>
        /// <returns>True if the instance is valid; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static bool IsValid(this DeviceInfo value) =>
            value is not null && Validate(value).Count == 0;

        /// <summary>
        /// Ensures that a DeviceInfo instance is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The DeviceInfo instance to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> contains validation errors</exception>
        public static void EnsureValid(this DeviceInfo value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    string.Join(
                        $"\n- ",
                        errors
                    )
                );
            }
        }
    }
}
