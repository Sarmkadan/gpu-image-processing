using System;
using System.Runtime.Intrinsics;

namespace GpuImageProcessing.Domain
{
    /// <summary>
    /// Provides extension methods for <see cref="SimdCapabilities"/> to query and interpret SIMD instruction set capabilities.
    /// </summary>
    public static class SimdCapabilitiesExtensions
    {
        /// <summary>
        /// Determines whether the current CPU supports SIMD operations with the specified vector width.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to check.</param>
        /// <param name="vectorWidthBytes">The required vector width in bytes.</param>
        /// <returns>True if the CPU supports SIMD operations with at least the specified width; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="capabilities"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="vectorWidthBytes"/> is not a positive number.</exception>
        public static bool SupportsVectorWidth(this SimdCapabilities capabilities, int vectorWidthBytes)
        {
            ArgumentNullException.ThrowIfNull(capabilities);

            if (vectorWidthBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(vectorWidthBytes), "Vector width must be positive.");

            return capabilities.VectorWidthBytes >= vectorWidthBytes;
        }

        /// <summary>
        /// Gets the maximum SIMD level that can be used for optimal performance based on available capabilities.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to evaluate.</param>
        /// <returns>The optimal SIMD level for the current hardware.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="capabilities"/> is <see langword="null"/>.</exception>
        public static SimdLevel GetOptimalSimdLevel(this SimdCapabilities capabilities)
        {
            ArgumentNullException.ThrowIfNull(capabilities);

            return capabilities.SupportsAvx512F ? SimdLevel.Avx512F :
                   capabilities.SupportsAvx2 ? SimdLevel.Avx2 :
                   capabilities.SupportsAvx ? SimdLevel.Avx :
                   capabilities.SupportsSse41 ? SimdLevel.Sse41 :
                   capabilities.SupportsSSE2 ? SimdLevel.Sse2 :
                   SimdLevel.None;
        }

        /// <summary>
        /// Checks if the current CPU supports any form of SIMD acceleration.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to check.</param>
        /// <returns>True if any SIMD instruction set is supported; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="capabilities"/> is <see langword="null"/>.</exception>
        public static bool HasSIMD(this SimdCapabilities capabilities)
        {
            ArgumentNullException.ThrowIfNull(capabilities);

            return capabilities.SupportsSSE2 ||
                   capabilities.SupportsSse41 ||
                   capabilities.SupportsAvx ||
                   capabilities.SupportsAvx2 ||
                   capabilities.SupportsAvx512F;
        }

        /// <summary>
        /// Gets a string representation of the SIMD capabilities suitable for display purposes.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to format.</param>
        /// <returns>A formatted string showing available SIMD instruction sets.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="capabilities"/> is <see langword="null"/>.</exception>
        public static string ToFriendlyString(this SimdCapabilities capabilities)
        {
            ArgumentNullException.ThrowIfNull(capabilities);

            if (!capabilities.HasSIMD())
                return "No SIMD support detected";

            var parts = new System.Collections.Generic.List<string>();

            if (capabilities.SupportsAvx512F)
                parts.Add("AVX-512F");

            if (capabilities.SupportsAvx2)
                parts.Add("AVX2");

            if (capabilities.SupportsAvx)
                parts.Add("AVX");

            if (capabilities.SupportsSse41)
                parts.Add("SSE4.1");

            if (capabilities.SupportsSSE2)
                parts.Add("SSE2");

            return $"SIMD: {string.Join(", ", parts)} (Vector width: {capabilities.VectorWidthBytes} bytes)";
        }
    }
}