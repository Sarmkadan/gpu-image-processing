#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    private void InitializeDevices()
    {
        try
        {
            _logger.LogInformation("Initializing GPU devices...");

            // Simulate GPU device detection
            var device1 = new GpuDevice
            {
                Name = "NVIDIA GeForce RTX 3090",
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
                ComputeCapabilityMinor = 6
            };

            lock (_lockObject)
            {
                _devices.Add(device1);
            }

            _logger.LogInformation("Detected {DeviceCount} GPU device(s)", _devices.Count);

            foreach (var device in _devices)
            {
                _logger.LogInformation("Device: {DeviceName} - {Memory}GB, {ComputeUnits} CUs",
                    device.Name, device.GlobalMemoryBytes / (1024L * 1024 * 1024), device.MaxComputeUnits);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize GPU devices");
            throw new GpuException("GPU initialization failed", ex, null, Constants.ErrorCodes.GpuInitializationFailed);
        }
    }
}
