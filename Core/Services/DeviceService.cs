#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Exceptions;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Service for managing GPU and compute device information
    /// </summary>
    public class DeviceService
    {
        private List<DeviceInfo> _devices = new();
        private DeviceInfo? _selectedDevice;

        /// <summary>
        /// Initializes the device service and detects available devices
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await DetectDevicesAsync().ConfigureAwait(false);
                if (_devices.Count == 0)
                {
                    throw new DeviceInitializationException("No compute devices detected");
                }
                _selectedDevice = _devices.OrderByDescending(d => d.GetCapabilityScore()).First();
            }
            catch (Exception ex)
            {
                throw new DeviceInitializationException("Failed to initialize compute devices", ex.Message);
            }
        }

        /// <summary>
        /// Detects all available compute devices
        /// </summary>
        private async Task DetectDevicesAsync()
        {
            // Simulated device detection - in production, would use OpenCL
            _devices = new List<DeviceInfo>
            {
                new DeviceInfo
                {
                    Name = "Intel UHD Graphics",
                    Vendor = "Intel",
                    DeviceType = "GPU",
                    GlobalMemoryBytes = 2_147_483_648, // 2GB
                    LocalMemoryBytes = 65536,
                    ComputeUnits = 12,
                    MaxWorkGroupSize = 256,
                    MaxWorkItemDimensions = 3,
                    OpenCLVersion = "2.1",
                    DriverVersion = "27.20.100.9316",
                    IsAvailable = true,
                    ClockFrequencyMHz = 1100,
                    SupportsDoublePrecision = true
                },
                new DeviceInfo
                {
                    Name = "CPU",
                    Vendor = "System",
                    DeviceType = "CPU",
                    GlobalMemoryBytes = 16_000_000_000, // 16GB
                    LocalMemoryBytes = 32768,
                    ComputeUnits = 8,
                    MaxWorkGroupSize = 1024,
                    MaxWorkItemDimensions = 3,
                    OpenCLVersion = "2.0",
                    DriverVersion = "1.0",
                    IsAvailable = true,
                    ClockFrequencyMHz = 3500,
                    SupportsDoublePrecision = true
                }
            };

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets all available devices
        /// </summary>
        public async Task<IEnumerable<DeviceInfo>> GetAllDevicesAsync()
        {
            return await Task.FromResult(_devices).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a device by name
        /// </summary>
        public async Task<DeviceInfo?> GetDeviceByNameAsync(string deviceName)
        {
            return await Task.FromResult(
                _devices.FirstOrDefault(d => d.Name.Equals(deviceName, StringComparison.OrdinalIgnoreCase))
            );
        }

        /// <summary>
        /// Gets the currently selected device for processing
        /// </summary>
        public DeviceInfo? GetSelectedDevice()
        {
            return _selectedDevice;
        }

        /// <summary>
        /// Selects a device for processing
        /// </summary>
        public async Task<bool> SelectDeviceAsync(Guid deviceId)
        {
            var device = _devices.FirstOrDefault(d => d.Id == deviceId);
            if (device == null)
                return await Task.FromResult(false).ConfigureAwait(false);

            if (!device.IsAvailable)
                return await Task.FromResult(false).ConfigureAwait(false);

            _selectedDevice = device;
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all GPU devices
        /// </summary>
        public async Task<IEnumerable<DeviceInfo>> GetGpuDevicesAsync()
        {
            return await Task.FromResult(
                _devices.Where(d => d.DeviceType == "GPU" && d.IsAvailable).ToList()
            );
        }

        /// <summary>
        /// Gets all CPU devices
        /// </summary>
        public async Task<IEnumerable<DeviceInfo>> GetCpuDevicesAsync()
        {
            return await Task.FromResult(
                _devices.Where(d => d.DeviceType == "CPU" && d.IsAvailable).ToList()
            );
        }

        /// <summary>
        /// Gets the most capable device
        /// </summary>
        public async Task<DeviceInfo?> GetMostCapableDeviceAsync()
        {
            return await Task.FromResult(
                _devices.Where(d => d.IsAvailable)
                    .OrderByDescending(d => d.GetCapabilityScore())
                    .FirstOrDefault()
            );
        }

        /// <summary>
        /// Checks if a device has sufficient memory
        /// </summary>
        public async Task<bool> HasSufficientMemoryAsync(Guid deviceId, long requiredBytes)
        {
            var device = _devices.FirstOrDefault(d => d.Id == deviceId);
            return await Task.FromResult(device?.HasSufficientMemory(requiredBytes) ?? false).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets device statistics
        /// </summary>
        public async Task<DeviceStatistics> GetStatisticsAsync()
        {
            var availableDevices = _devices.Where(d => d.IsAvailable).ToList();
            var gpuDevices = availableDevices.Where(d => d.DeviceType == "GPU").ToList();

            var stats = new DeviceStatistics
            {
                TotalDevices = _devices.Count,
                AvailableDevices = availableDevices.Count,
                GpuDevices = gpuDevices.Count,
                CpuDevices = availableDevices.Count(d => d.DeviceType == "CPU"),
                TotalMemoryBytes = availableDevices.Sum(d => d.GlobalMemoryBytes),
                TotalComputeUnits = availableDevices.Sum(d => d.ComputeUnits),
                AverageCapabilityScore = availableDevices.Count > 0
                    ? availableDevices.Average(d => d.GetCapabilityScore())
                    : 0,
                MostCapableDevice = availableDevices.OrderByDescending(d => d.GetCapabilityScore()).FirstOrDefault()
            };

            return await Task.FromResult(stats).ConfigureAwait(false);
        }

        /// <summary>
        /// Refreshes device information
        /// </summary>
        public async Task RefreshDevicesAsync()
        {
            await DetectDevicesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets device capabilities summary
        /// </summary>
        public async Task<string> GetCapabilitiesSummaryAsync()
        {
            var summary = "Available Compute Devices:\n";
            foreach (var device in _devices)
            {
                summary += $"{device.GetCapabilitiesSummary()}\n\n";
            }
            return await Task.FromResult(summary).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Device statistics summary
    /// </summary>
    public class DeviceStatistics
    {
        public int TotalDevices { get; set; }
        public int AvailableDevices { get; set; }
        public int GpuDevices { get; set; }
        public int CpuDevices { get; set; }
        public long TotalMemoryBytes { get; set; }
        public int TotalComputeUnits { get; set; }
        public double AverageCapabilityScore { get; set; }
        public DeviceInfo? MostCapableDevice { get; set; }
    }
}
