using System;
using System.Runtime.Intrinsics;

namespace GpuImageProcessing.Domain
{
    public static class SimdCapabilitiesExtensions
    {
        /// <summary>
        /// Determines whether the current CPU supports SIMD operations with the specified vector width.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to check.</param>
        /// <param name="vectorWidthBytes">The required vector width in bytes.</param>
        /// <returns>True if the CPU supports SIMD operations with at least the specified width; otherwise, false.</returns>
        public static bool SupportsVectorWidth(this SimdCapabilities capabilities, int vectorWidthBytes)
        {
            if (vectorWidthBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(vectorWidthBytes), "Vector width must be positive.");

            return capabilities.VectorWidthBytes >= vectorWidthBytes;
        }

        /// <summary>
        /// Gets the maximum SIMD level that can be used for optimal performance based on available capabilities.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to evaluate.</param>
        /// <returns>The optimal SIMD level for the current hardware.</returns>
        public static SimdLevel GetOptimalSimdLevel(this SimdCapabilities capabilities)
        {
            if (capabilities.SupportsAvx512F)
                return SimdLevel.Avx512F;

            if (capabilities.SupportsAvx2)
                return SimdLevel.Avx2;

            if (capabilities.SupportsAvx)
                return SimdLevel.Avx;

            if (capabilities.SupportsSse41)
                return SimdLevel.Sse41;

            if (capabilities.SupportsSSE2)
                return SimdLevel.Sse2;

            return SimdLevel.None;
        }

        /// <summary>
        /// Checks if the current CPU supports any form of SIMD acceleration.
        /// </summary>
        /// <param name="capabilities">The SIMD capabilities to check.</param>
        /// <returns>True if any SIMD instruction set is supported; otherwise, false.</returns>
        public static bool HasSIMD(this SimdCapabilities capabilities)
        {
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
        public static string ToFriendlyString(this SimdCapabilities capabilities)
        {
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