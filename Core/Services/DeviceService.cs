#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core;
using GpuImageProcessing.Core.Exceptions;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Service for managing GPU and compute device information
    /// </summary>
    public class DeviceService
    {
        private readonly GpuManagementService _gpuManagementService;
        private readonly ILogger<DeviceService> _logger;
        private List<DeviceInfo> _devices = new();
        private DeviceInfo? _selectedDevice;

        public DeviceService(GpuManagementService gpuManagementService, ILogger<DeviceService> logger)
        {
            _gpuManagementService = gpuManagementService ?? throw new ArgumentNullException(nameof(gpuManagementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the device service and detects available devices
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await DetectDevicesAsync();
                if (_devices.Count == 0)
                {
                    throw new DeviceInitializationException("No compute devices detected");
                }
                _selectedDevice = _devices.OrderByDescending(d => d.GetCapabilityScore()).First();
                _logger.LogInformation("Selected default device: {DeviceName}", _selectedDevice.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize compute devices");
                throw new DeviceInitializationException("Failed to initialize compute devices", ex.Message);
            }
        }

        /// <summary>
        /// Detects all available compute devices using GpuManagementService
        /// </summary>
        private async Task DetectDevicesAsync()
        {
            _devices.Clear();
            var detectedGpuDevices = _gpuManagementService.GetAvailableDevices();

            foreach (var gpuDevice in detectedGpuDevices)
            {
                var deviceInfo = MapGpuDeviceToDeviceInfo(gpuDevice);
                _devices.Add(deviceInfo);
            }

            // Add a simulated CPU device if no actual CPU device is found through OpenCL, for compatibility
            if (!_devices.Any(d => d.DeviceType == GpuDeviceType.Cpu.ToString()))
            {
                 _devices.Add(new DeviceInfo
                {
                    Name = "CPU (Simulated)",
                    Vendor = "System",
                    DeviceType = GpuDeviceType.Cpu.ToString(),
                    GlobalMemoryBytes = 16_000_000_000, // 16GB
                    LocalMemoryBytes = 32768,
                    ComputeUnits = Environment.ProcessorCount,
                    MaxWorkGroupSize = 1024,
                    MaxWorkItemDimensions = 3,
                    OpenCLVersion = "N/A",
                    DriverVersion = "N/A",
                    IsAvailable = true,
                    ClockFrequencyMHz = 3500,
                    SupportsDoublePrecision = true
                });
            }

            _logger.LogInformation("Detected {Count} compute device(s) in total.", _devices.Count);
            await Task.CompletedTask;
        }

        private DeviceInfo MapGpuDeviceToDeviceInfo(GpuImageProcessing.Domain.GpuDevice gpuDevice)
        {
            return new DeviceInfo
            {
                Id = gpuDevice.Id,
                Name = gpuDevice.Name,
                Vendor = gpuDevice.Vendor,
                DeviceType = gpuDevice.DeviceType.ToString(),
                GlobalMemoryBytes = gpuDevice.GlobalMemoryBytes,
                LocalMemoryBytes = gpuDevice.LocalMemoryBytes,
                ComputeUnits = gpuDevice.MaxComputeUnits,
                MaxWorkGroupSize = gpuDevice.MaxWorkGroupSize,
                MaxWorkItemDimensions = gpuDevice.MaxWorkItemDimensions,
                OpenCLVersion = gpuDevice.Version,
                DriverVersion = gpuDevice.Driver,
                IsAvailable = gpuDevice.IsAvailable,
                ClockFrequencyMHz = (float)gpuDevice.MaxClockFrequencyMhz,
                SupportsDoublePrecision = gpuDevice.SupportsDoublePrecision
            };
        }

        /// <summary>
        /// Gets all available devices
        /// </summary>
        public async Task<IEnumerable<DeviceInfo>> GetAllDevicesAsync()
        {
            return await Task.FromResult(_devices);
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
                return await Task.FromResult(false);

            if (!device.IsAvailable)
                return await Task.FromResult(false);

            _selectedDevice = device;
            _logger.LogInformation("Selected device: {DeviceName}", _selectedDevice.Name);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Gets all GPU devices
        /// </summary>
        public async Task<IEnumerable<DeviceInfo>> GetGpuDevicesAsync()
        {
            return await Task.FromResult(
                _devices.Where(d => d.DeviceType == GpuDeviceType.Gpu.ToString() && d.IsAvailable).ToList()
            );
        }

        /// <summary>
        /// Gets all CPU devices
        /// </summary>
        public async Task<IEnumerable<DeviceInfo>> GetCpuDevicesAsync()
        {
            return await Task.FromResult(
                _devices.Where(d => d.DeviceType == GpuDeviceType.Cpu.ToString() && d.IsAvailable).ToList()
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
            return await Task.FromResult(device?.HasSufficientMemory(requiredBytes) ?? false);
        }

        /// <summary>
        /// Gets device statistics
        /// </summary>
        public async Task<DeviceStatistics> GetStatisticsAsync()
        {
            var availableDevices = _devices.Where(d => d.IsAvailable).ToList();
            var gpuDevices = availableDevices.Where(d => d.DeviceType == GpuDeviceType.Gpu.ToString()).ToList();

            var stats = new DeviceStatistics
            {
                TotalDevices = _devices.Count,
                AvailableDevices = availableDevices.Count,
                GpuDevices = gpuDevices.Count,
                CpuDevices = availableDevices.Count(d => d.DeviceType == GpuDeviceType.Cpu.ToString()),
                TotalMemoryBytes = availableDevices.Sum(d => d.GlobalMemoryBytes),
                TotalComputeUnits = availableDevices.Sum(d => d.ComputeUnits),
                AverageCapabilityScore = availableDevices.Count > 0
                    ? availableDevices.Average(d => d.GetCapabilityScore())
                    : 0,
                MostCapableDevice = availableDevices.OrderByDescending(d => d.GetCapabilityScore()).FirstOrDefault()
            };

            return await Task.FromResult(stats);
        }

        /// <summary>
        /// Refreshes device information
        /// </summary>
        public async Task RefreshDevicesAsync()
        {
            await DetectDevicesAsync();
        }

        /// <summary>
        /// Gets device capabilities summary
        /// </summary>
        public async Task<string> GetCapabilitiesSummaryAsync()
        {
            var summary = "Available Compute Devices:\n";
            foreach (var device in _devices)
            {
                summary += $"{device.GetCapabilitiesSummary()}\n";
            }
            return await Task.FromResult(summary);
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
