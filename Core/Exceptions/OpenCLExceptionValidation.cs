#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Provides validation helpers for <see cref="OpenCLException"/> instances
    /// </summary>
    public static class OpenCLExceptionValidation
    {
        /// <summary>
        /// Validates an <see cref="OpenCLException"/> instance and returns a list of human-readable validation problems
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <returns>A read-only list of validation problems; empty if the exception is valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this OpenCLException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate DeviceName
            if (string.IsNullOrWhiteSpace(value.DeviceName))
            {
                errors.Add("DeviceName must not be null or whitespace.");
            }

            // Validate OpenCLErrorCode
            if (value.OpenCLErrorCode.HasValue)
            {
                // OpenCL error codes are typically negative values in the range [-1, -1000]
                // See: https://registry.khronos.org/OpenCL/sdk/3.0/docs/man/html/clGetErrorString.html
                if (value.OpenCLErrorCode is <= -1001 or >= 0)
                {
                    errors.Add(
                        $"OpenCLErrorCode must be a negative value in the range [-1000, -1] (was {value.OpenCLErrorCode}).");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="OpenCLException"/> instance is valid
        /// </summary>
        /// <param name="value">The exception to check</param>
        /// <returns><see langword="true"/> if the exception is valid; otherwise, <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static bool IsValid(this OpenCLException value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="OpenCLException"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if the exception is invalid, with a detailed message listing all validation problems</exception>
        public static void EnsureValid(this OpenCLException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"OpenCLException is invalid. Validation errors:\n- {string.Join("\n- ", errors)}");
            }
        }

        /// <summary>
        /// Validates a <see cref="DeviceInitializationException"/> instance and returns a list of human-readable validation problems
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <returns>A read-only list of validation problems; empty if the exception is valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this DeviceInitializationException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate base OpenCLException properties
            errors.AddRange(Validate((OpenCLException)value));

            // DeviceInitializationException-specific validation
            if (string.IsNullOrWhiteSpace(value.DeviceName))
            {
                errors.Add("DeviceName must not be null or whitespace for DeviceInitializationException.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="DeviceInitializationException"/> instance is valid
        /// </summary>
        /// <param name="value">The exception to check</param>
        /// <returns><see langword="true"/> if the exception is valid; otherwise, <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static bool IsValid(this DeviceInitializationException value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="DeviceInitializationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if the exception is invalid, with a detailed message listing all validation problems</exception>
        public static void EnsureValid(this DeviceInitializationException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"DeviceInitializationException is invalid. Validation errors:\n- {string.Join("\n- ", errors)}");
            }
        }

        /// <summary>
        /// Validates a <see cref="KernelCompilationException"/> instance and returns a list of human-readable validation problems
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <returns>A read-only list of validation problems; empty if the exception is valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this KernelCompilationException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate base OpenCLException properties
            errors.AddRange(Validate((OpenCLException)value));

            // KernelCompilationException-specific validation
            if (string.IsNullOrWhiteSpace(value.KernelSource))
            {
                errors.Add("KernelSource must not be null or whitespace for KernelCompilationException.");
            }

            if (string.IsNullOrWhiteSpace(value.CompilationLog))
            {
                errors.Add("CompilationLog must not be null or whitespace for KernelCompilationException.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="KernelCompilationException"/> instance is valid
        /// </summary>
        /// <param name="value">The exception to check</param>
        /// <returns><see langword="true"/> if the exception is valid; otherwise, <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static bool IsValid(this KernelCompilationException value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="KernelCompilationException"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if the exception is invalid, with a detailed message listing all validation problems</exception>
        public static void EnsureValid(this KernelCompilationException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"KernelCompilationException is invalid. Validation errors:\n- {string.Join("\n- ", errors)}");
            }
        }
    }
}