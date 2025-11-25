#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="SimdCapabilities"/> instances.
/// </summary>
public static class SimdCapabilitiesValidation
{
    /// <summary>
    /// Validates that a <see cref="SimdCapabilities"/> instance contains only valid values.
    /// </summary>
    /// <param name="value">The instance to validate (must not be null).</param>
    /// <returns>An empty list if the instance is valid; otherwise, a list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SimdCapabilities value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate BestAvailableLevel
        if (value.BestAvailableLevel < SimdLevel.None || value.BestAvailableLevel > SimdLevel.Avx512F)
        {
            errors.Add(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "BestAvailableLevel must be between {0} and {1}, but was {2}.",
                    SimdLevel.None,
                    SimdLevel.Avx512F,
                    value.BestAvailableLevel
                )
            );
        }

        // Validate VectorWidthBytes
        if (value.VectorWidthBytes < 0)
        {
            errors.Add(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "VectorWidthBytes must be non-negative, but was {0}.",
                    value.VectorWidthBytes
                )
            );
        }
        else if (value.VectorWidthBytes > 0 && value.VectorWidthBytes < 16)
        {
            // Minimum reasonable SIMD width is 16 bytes (SSE2)
            errors.Add(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "VectorWidthBytes must be at least 16 for SIMD support, but was {0}.",
                    value.VectorWidthBytes
                )
            );
        }

        // Validate boolean flags consistency with BestAvailableLevel
        if (value.BestAvailableLevel == SimdLevel.None && value.IsAnySimdAvailable)
        {
            errors.Add("BestAvailableLevel is None but IsAnySimdAvailable is true.");
        }
        else if (value.BestAvailableLevel > SimdLevel.None && !value.IsAnySimdAvailable)
        {
            errors.Add("BestAvailableLevel indicates SIMD support but IsAnySimdAvailable is false.");
        }

        // Validate that boolean flags match the detected level
        if (value.BestAvailableLevel >= SimdLevel.Sse41 && !value.SupportsSse41)
        {
            errors.Add("BestAvailableLevel indicates SSE4.1 or higher but SupportsSse41 is false.");
        }

        if (value.BestAvailableLevel >= SimdLevel.Avx && !value.SupportsAvx)
        {
            errors.Add("BestAvailableLevel indicates AVX or higher but SupportsAvx is false.");
        }

        if (value.BestAvailableLevel >= SimdLevel.Avx2 && !value.SupportsAvx2)
        {
            errors.Add("BestAvailableLevel indicates AVX2 or higher but SupportsAvx2 is false.");
        }

        if (value.BestAvailableLevel >= SimdLevel.Avx512F && !value.SupportsAvx512F)
        {
            errors.Add("BestAvailableLevel indicates AVX-512F but SupportsAvx512F is false.");
        }

        // Validate that lower levels are properly set when higher levels are available
        if (value.SupportsSse41 && !value.SupportsSSE2)
        {
            errors.Add("SupportsSse41 is true but SupportsSSE2 is false.");
        }

        if (value.SupportsAvx && !(value.SupportsSSE2 || value.SupportsSse41))
        {
            errors.Add("SupportsAvx is true but neither SSE2 nor SSE4.1 is supported.");
        }

        if (value.SupportsAvx2 && !value.SupportsAvx)
        {
            errors.Add("SupportsAvx2 is true but SupportsAvx is false.");
        }

        if (value.SupportsAvx512F && !value.SupportsAvx2)
        {
            errors.Add("SupportsAvx512F is true but SupportsAvx2 is false.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SimdCapabilities"/> instance contains only valid values.
    /// </summary>
    /// <param name="value">The instance to check (must not be null).</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SimdCapabilities value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SimdCapabilities"/> instance contains only valid values,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it does not.
    /// </summary>
    /// <param name="value">The instance to validate (must not be null).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance contains invalid values.</exception>
    public static void EnsureValid(this SimdCapabilities value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            string.Join(
                Environment.NewLine,
                "SimdCapabilities instance is invalid:",
                string.Join(
                    Environment.NewLine,
                    errors
                )
            )
        );
    }
}
