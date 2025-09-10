#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="Transform"/> class.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Determines whether two transforms have identical parameter sets.
        /// </summary>
        /// <param name="left">The first transform to compare.</param>
        /// <param name="right">The second transform to compare.</param>
        /// <returns><see langword="true"/> if both transforms have the same parameters; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="left"/> or <paramref name="right"/> is null.</exception>
        public static bool AreParametersEqual(this Transform left, Transform right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);

            if (left.Parameters.Count != right.Parameters.Count)
                return false;

            foreach (var kvp in left.Parameters)
            {
                if (!right.Parameters.TryGetValue(kvp.Key, out var rightValue) || 
                    !kvp.Value.Equals(rightValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the transform contains all required parameters for its type.
        /// </summary>
        /// <param name="transform">The transform to validate.</param>
        /// <returns><see langword="true"/> if all required parameters are present; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="transform"/> is null.</exception>
        public static bool HasAllRequiredParameters(this Transform transform)
        {
            ArgumentNullException.ThrowIfNull(transform);

            var predefined = Transform.CreatePredefined(transform.Type);
            var requiredKeys = predefined.GetParameterNames();

            foreach (var key in requiredKeys)
            {
                if (!transform.HasParameter(key))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Merges parameters from another transform into this one.
        /// </summary>
        /// <param name="target">The transform to receive merged parameters.</param>
        /// <param name="source">The transform to copy parameters from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> or <paramref name="source"/> is null.</exception>
        public static void MergeParameters(this Transform target, Transform source)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(source);

            foreach (var kvp in source.Parameters)
            {
                target.SetParameter(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Gets a compact string representation of the transform's parameters.
        /// </summary>
        /// <param name="transform">The transform to summarize.</param>
        /// <returns>A string containing parameter names and values.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="transform"/> is null.</exception>
        public static string GetParameterSummary(this Transform transform)
        {
            ArgumentNullException.ThrowIfNull(transform);

            return string.Join(", ", transform.Parameters.Select(kvp => 
                $"{kvp.Key}={kvp.Value.ToString(CultureInfo.InvariantCulture)}"));
        }
    }
}
