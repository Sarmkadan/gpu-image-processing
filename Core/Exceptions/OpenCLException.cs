#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Exception thrown for OpenCL-related errors
    /// </summary>
    public class OpenCLException : ImageProcessingException
    {
        public string? DeviceName { get; set; }
        public int? OpenCLErrorCode { get; set; }

        public OpenCLException(string message)
            : base(message, "OPENCL_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }

        public OpenCLException(string message, int errorCode)
            : base(message, "OPENCL_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            OpenCLErrorCode = errorCode;
        }

        public OpenCLException(string message, string deviceName)
            : base(message, "OPENCL_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (deviceName == null)
                throw new ArgumentNullException(nameof(deviceName));
            DeviceName = deviceName;
        }

        public OpenCLException(string message, Exception innerException)
            : base(message, "OPENCL_ERROR", innerException)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }

        public OpenCLException(string message, int errorCode, string deviceName)
            : base(message, "OPENCL_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (deviceName == null)
                throw new ArgumentNullException(nameof(deviceName));
            OpenCLErrorCode = errorCode;
            DeviceName = deviceName;
        }

        public override string ToString()
        {
            var baseString = base.ToString();
            var deviceInfo = string.IsNullOrEmpty(DeviceName) ? string.Empty : $"\nDevice: {DeviceName}";
            var errorInfo = OpenCLErrorCode.HasValue ? $"\nOpenCL Error Code: {OpenCLErrorCode}" : string.Empty;
            return baseString + deviceInfo + errorInfo;
        }
    }

    /// <summary>
    /// Exception thrown when device initialization fails
    /// </summary>
    public class DeviceInitializationException : OpenCLException
    {
        public DeviceInitializationException(string message)
            : base(message, "DEVICE_INIT_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            ErrorCode = "DEVICE_INIT_ERROR";
        }

        public DeviceInitializationException(string message, string deviceName)
            : base(message, deviceName)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (deviceName == null)
                throw new ArgumentNullException(nameof(deviceName));
            ErrorCode = "DEVICE_INIT_ERROR";
            DeviceName = deviceName;
        }
    }

    /// <summary>
    /// Exception thrown when kernel compilation fails
    /// </summary>
    public class KernelCompilationException : OpenCLException
    {
        public string? KernelSource { get; set; }
        public string? CompilationLog { get; set; }

        public KernelCompilationException(string message, string compilationLog)
            : base(message, "KERNEL_COMPILE_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (compilationLog == null)
                throw new ArgumentNullException(nameof(compilationLog));
            CompilationLog = compilationLog;
        }

        public KernelCompilationException(string message, string kernelSource, string compilationLog)
            : base(message, "KERNEL_COMPILE_ERROR")
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (kernelSource == null)
                throw new ArgumentNullException(nameof(kernelSource));
            if (compilationLog == null)
                throw new ArgumentNullException(nameof(compilationLog));
            KernelSource = kernelSource;
            CompilationLog = compilationLog;
        }
    }
}
