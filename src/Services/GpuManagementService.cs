#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core;
using GpuImageProcessing.Core.Enums;
using GpuImageProcessing.Domain;
using Silk.NET.OpenCL;

namespace GpuImageProcessing.Services;

/// <summary>
/// Service for managing GPU devices and resources.
/// </summary>
public class GpuManagementService
{
    private readonly List<GpuDevice> _devices = [];
    private readonly ILogger<GpuManagementService> _logger;
    private long _allocatedMemory;
    private readonly object _lockObject = new();

    public GpuManagementService(ILogger<GpuManagementService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeDevices();
    }

    /// <summary>
    /// Gets all available GPU devices.
    /// </summary>
    public IEnumerable<GpuDevice> GetAvailableDevices()
    {
        lock (_lockObject)
        {
            return _devices.Where(d => d.IsAvailable).ToList();
        }
    }

    /// <summary>
    /// Gets a specific GPU device by ID.
    /// </summary>
    public GpuDevice? GetDeviceById(Guid deviceId)
    {
        lock (_lockObject)
        {
            return _devices.FirstOrDefault(d => d.Id == deviceId);
        }
    }

    /// <summary>
    /// Gets the best device based on performance score.
    /// </summary>
    public GpuDevice? GetBestDevice()
    {
        lock (_lockObject)
        {
            return _devices
                .Where(d => d.IsAvailable && d.Validate())
                .OrderByDescending(d => d.CalculatePerformanceScore())
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Gets device with most available memory.
    /// </summary>
    public GpuDevice? GetDeviceWithMostMemory()
    {
        lock (_lockObject)
        {
            return _devices
                .Where(d => d.IsAvailable)
                .OrderByDescending(d => d.GetAvailableMemory())
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Allocates GPU memory for an operation.
    /// </summary>
    public bool AllocateMemory(long bytes, Guid deviceId)
    {
        if (bytes <= 0)
            return false;

        var device = GetDeviceById(deviceId);
        if (device == null || !device.IsAvailable)
            throw new GpuException("Device not available", null, Constants.ErrorCodes.DeviceNotAvailable);

        if (!device.HasSufficientMemory(bytes))
        {
            _logger.LogWarning("Insufficient GPU memory on device {DeviceName}: requested {Bytes}, available {Available}",
                device.Name, bytes, device.GetAvailableMemory());
            throw new GpuException("Insufficient GPU memory", device.Name, Constants.ErrorCodes.InsufficientMemory);
        }

        lock (_lockObject)
        {
            _allocatedMemory += bytes;
            device.MaxAllocatableMemoryBytes -= bytes;

            if (device.IsMemoryWarningRequired())
            {
                _logger.LogWarning("GPU memory usage on {DeviceName} is above threshold: {UsagePercent}%",
                    device.Name, device.GetMemoryUsagePercent());
            }

            _logger.LogDebug("Allocated {Bytes} bytes on device {DeviceName}", bytes, device.Name);
            return true;
        }
    }

    /// <summary>
    /// Deallocates GPU memory.
    /// </summary>
    public void DeallocateMemory(long bytes, Guid deviceId)
    {
        if (bytes <= 0)
            return;

        var device = GetDeviceById(deviceId);
        if (device == null)
            return;

        lock (_lockObject)
        {
            _allocatedMemory -= bytes;
            device.MaxAllocatableMemoryBytes += bytes;
            _logger.LogDebug("Deallocated {Bytes} bytes on device {DeviceName}", bytes, device.Name);
        }
    }

    /// <summary>
    /// Gets total allocated memory across all devices.
    /// </summary>
    public long GetTotalAllocatedMemory()
    {
        lock (_lockObject)
        {
            return _allocatedMemory;
        }
    }

    /// <summary>
    /// Validates if a device meets requirements for operation.
    /// </summary>
    public bool ValidateDevice(Guid deviceId, long requiredMemory, int requiredComputeUnits = 1)
    {
        var device = GetDeviceById(deviceId);
        if (device == null || !device.Validate())
            return false;

        if (!device.HasSufficientMemory(requiredMemory))
            return false;

        if (device.MaxComputeUnits < requiredComputeUnits)
            return false;

        return true;
    }

    /// <summary>
    /// Gets memory usage statistics.
    /// </summary>
    public Dictionary<string, object> GetMemoryStatistics()
    {
        lock (_lockObject)
        {
            var totalMemory = _devices.Sum(d => d.GlobalMemoryBytes);
            var totalAvailable = _devices.Sum(d => d.GetAvailableMemory());
            var totalAllocated = _allocatedMemory;

            return new Dictionary<string, object>
            {
                { "TotalGpuMemory", totalMemory },
                { "TotalAvailableMemory", totalAvailable },
                { "TotalAllocatedMemory", totalAllocated },
                { "MemoryUsagePercent", totalMemory > 0 ? ((totalMemory - totalAvailable) / (double)totalMemory * 100) : 0.0 },
                { "DeviceCount", _devices.Count },
                { "AvailableDeviceCount", _devices.Count(d => d.IsAvailable) }
            };
        }
    }

    private unsafe void InitializeDevices()
    {
        try
        {
            _logger.LogInformation("Initializing GPU devices using OpenCL...");

            CL cl = CL.GetApi();

            // Get number of platforms
            uint numPlatforms;
            cl.GetPlatformIDs(0, null, &numPlatforms);
            if (numPlatforms == 0)
            {
                _logger.LogWarning("No OpenCL platforms found.");
                return;
            }

            // Get platform IDs
            var platformIds = new Platform[numPlatforms];
            cl.GetPlatformIDs(numPlatforms, platformIds, null);

            foreach (var platformId in platformIds)
            {
                // Get platform name
                byte[] platformNameBytes = new byte[256];
                fixed (byte* ptr = platformNameBytes)
                {
                    cl.GetPlatformInfo(platformId, PlatformInfo.Name, (nuint)platformNameBytes.Length, ptr, null);
                }
                string platformName = Encoding.ASCII.GetString(platformNameBytes.Where(b => b != 0).ToArray());

                _logger.LogInformation("Found OpenCL Platform: {PlatformName}", platformName);

                // Get number of devices for this platform
                uint numDevices;
                cl.GetDeviceIDs(platformId, DeviceType.All, 0, null, &numDevices);
                if (numDevices == 0)
                {
                    _logger.LogInformation("No devices found for platform {PlatformName}.", platformName);
                    continue;
                }

                // Get device IDs
                var deviceIds = new Device[numDevices];
                cl.GetDeviceIDs(platformId, DeviceType.All, numDevices, deviceIds, null);

                foreach (var deviceId in deviceIds)
                {
                    GpuDevice gpuDevice = new GpuDevice();
                    gpuDevice.Id = Guid.NewGuid(); // Generate a new GUID for internal use

                    // Get device name
                    byte[] deviceNameBytes = new byte[256];
                    fixed (byte* ptr = deviceNameBytes)
                    {
                        cl.GetDeviceInfo(deviceId, DeviceInfo.Name, (nuint)deviceNameBytes.Length, ptr, null);
                    }
                    gpuDevice.Name = Encoding.ASCII.GetString(deviceNameBytes.Where(b => b != 0).ToArray());

                    // Get device vendor
                    byte[] deviceVendorBytes = new byte[256];
                    fixed (byte* ptr = deviceVendorBytes)
                    {
                        cl.GetDeviceInfo(deviceId, DeviceInfo.Vendor, (nuint)deviceVendorBytes.Length, ptr, null);
                    }
                    gpuDevice.Vendor = Encoding.ASCII.GetString(deviceVendorBytes.Where(b => b != 0).ToArray());

                    // Get device type
                    DeviceType devType;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.Type, (nuint)sizeof(DeviceType), &devType, null);
                    gpuDevice.DeviceType = MapOpenCLDeviceType(devType);

                    // Get global memory
                    ulong globalMem;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.GlobalMemSize, (nuint)sizeof(ulong), &globalMem, null);
                    gpuDevice.GlobalMemoryBytes = (long)globalMem;
                    gpuDevice.MaxAllocatableMemoryBytes = (long)globalMem; // Assume all is allocatable initially

                    // Get local memory
                    ulong localMem;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.LocalMemSize, (nuint)sizeof(ulong), &localMem, null);
                    gpuDevice.LocalMemoryBytes = (long)localMem;

                    // Get max compute units
                    uint maxComputeUnits;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.MaxComputeUnits, (nuint)sizeof(uint), &maxComputeUnits, null);
                    gpuDevice.MaxComputeUnits = (int)maxComputeUnits;

                    // Get max work group size
                    nuint maxWorkGroupSize;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.MaxWorkGroupSize, (nuint)sizeof(nuint), &maxWorkGroupSize, null);
                    gpuDevice.MaxWorkGroupSize = (int)maxWorkGroupSize;

                    // Get max clock frequency
                    uint maxClockFrequency;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.MaxClockFrequency, (nuint)sizeof(uint), &maxClockFrequency, null);
                    gpuDevice.MaxClockFrequencyMhz = maxClockFrequency;

                    // Get OpenCL C version
                    byte[] versionBytes = new byte[256];
                    fixed (byte* ptr = versionBytes)
                    {
                        cl.GetDeviceInfo(deviceId, DeviceInfo.OpenCLCVersion, (nuint)versionBytes.Length, ptr, null);
                    }
                    gpuDevice.Version = Encoding.ASCII.GetString(versionBytes.Where(b => b != 0).ToArray());

                    // Get driver version
                    byte[] driverVersionBytes = new byte[256];
                    fixed (byte* ptr = driverVersionBytes)
                    {
                        cl.GetDeviceInfo(deviceId, DeviceInfo.DriverVersion, (nuint)driverVersionBytes.Length, ptr, null);
                    }
                    gpuDevice.Driver = Encoding.ASCII.GetString(driverVersionBytes.Where(b => b != 0).ToArray());

                    // Supports Double Precision
                    // Check for extension "cl_khr_fp64" or CL_DEVICE_DOUBLE_FP_CONFIG
                    ulong doubleFpConfig;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.DoubleFpConfig, (nuint)sizeof(ulong), &doubleFpConfig, null);
                    gpuDevice.SupportsDoublePrecision = (doubleFpConfig & (ulong)fp_config.CorrectlyRoundedDivideSqrt) != 0;


                    // Supports Half Precision
                    // Check for extension "cl_khr_fp16" or CL_DEVICE_HALF_FP_CONFIG
                    ulong halfFpConfig;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.HalfFpConfig, (nuint)sizeof(ulong), &halfFpConfig, null);
                    gpuDevice.SupportsHalfPrecision = (halfFpConfig & (ulong)fp_config.FpDenorm) != 0;

                    // Max work item dimensions and sizes
                    uint maxWorkItemDims;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.MaxWorkItemDimensions, (nuint)sizeof(uint), &maxWorkItemDims, null);
                    gpuDevice.MaxWorkItemDimensions = (int)maxWorkItemDims;

                    nuint[] maxWorkItemSizes = new nuint[maxWorkItemDims];
                    fixed (nuint* ptr = maxWorkItemSizes)
                    {
                        cl.GetDeviceInfo(deviceId, DeviceInfo.MaxWorkItemSizes, (nuint)(sizeof(nuint) * maxWorkItemDims), ptr, null);
                    }
                    gpuDevice.MaxWorkItemSizes = maxWorkItemSizes.Select(s => (int)s).ToArray();

                    // Wavefront size
                    uint preferredWorkGroupSizeMultiple;
                    cl.GetDeviceInfo(deviceId, DeviceInfo.PreferredWorkGroupSizeMultiple, (nuint)sizeof(uint), &preferredWorkGroupSizeMultiple, null);
                    gpuDevice.WavefrontSize = (int)preferredWorkGroupSizeMultiple;


                    gpuDevice.IsAvailable = true; // Mark as available if successfully detected and properties retrieved

                    // Add extensions (example for KHR_fp64)
                    byte[] extensionsBytes = new byte[1024];
                    fixed (byte* ptr = extensionsBytes)
                    {
                        cl.GetDeviceInfo(deviceId, DeviceInfo.Extensions, (nuint)extensionsBytes.Length, ptr, null);
                    }
                    string extensionsString = Encoding.ASCII.GetString(extensionsBytes.Where(b => b != 0).ToArray());
                    foreach (var ext in extensionsString.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        gpuDevice.Extensions[ext] = "supported";
                    }

                    lock (_lockObject)
                    {
                        _devices.Add(gpuDevice);
                    }
                    _logger.LogInformation("  Detected device: {DeviceName} ({DeviceType}) from {Vendor}",
                        gpuDevice.Name, gpuDevice.DeviceType, gpuDevice.Vendor);
                }
            }

            if (_devices.Count == 0)
            {
                _logger.LogWarning("No GPU devices detected using OpenCL. Falling back to simulated data.");
                // Fallback to simulated data if no real devices are found
                AddSimulatedDevice();
            }

            _logger.LogInformation("Finished GPU device initialization. Detected {DeviceCount} device(s).", _devices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize GPU devices using OpenCL.");
            // Fallback to simulated data on error
            AddSimulatedDevice();
            throw new GpuException("GPU initialization failed", ex, null, Constants.ErrorCodes.GpuInitializationFailed);
        }
    }

    private GpuDeviceType MapOpenCLDeviceType(DeviceType openClDeviceType)
    {
        if (openClDeviceType.HasFlag(DeviceType.Gpu))
            return GpuDeviceType.Gpu;
        if (openClDeviceType.HasFlag(DeviceType.Cpu))
            return GpuDeviceType.Cpu;
        if (openClDeviceType.HasFlag(DeviceType.Accelerator))
            return GpuDeviceType.Accelerator;
        return GpuDeviceType.Unknown;
    }

    private void AddSimulatedDevice()
    {
        var device1 = new GpuDevice
        {
            Name = "NVIDIA GeForce RTX 3090 (Simulated)",
            DeviceType = GpuDeviceType.Gpu,
            Vendor = "NVIDIA",
            Version = "12.4",
            Driver = "535.0",
            GlobalMemoryBytes = 24 * 1024L * 1024 * 1024,
            LocalMemoryBytes = 96 * 1024,
            MaxAllocatableMemoryBytes = 24 * 1024L * 1024 * 1024,
            MaxComputeUnits = 82,
            MaxWorkGroupSize = 1024,
            MaxWorkItemDimensions = 3,
            MaxWorkItemSizes = [1024, 1024, 64],
            MaxClockFrequencyMhz = 2520.0,
            SupportsDoublePrecision = true,
            SupportsHalfPrecision = true,
            IsAvailable = true,
            ComputeCapabilityMajor = 8,
            ComputeCapabilityMinor = 6,
            WavefrontSize = 32
        };

        lock (_lockObject)
        {
            _devices.Clear(); // Clear any partially detected devices
            _devices.Add(device1);
        }
    }
}
