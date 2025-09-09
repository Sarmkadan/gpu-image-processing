#nullable enable
// =============================================================================
// Author: 
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="FilterParameter"/> class.
    /// </summary>
    public static class FilterParameterExtensions
    {
        /// <summary>
        /// Merges two <see cref="FilterParameter"/> instances, overriding properties in the first instance with those from the second.
        /// </summary>
        /// <param name="parameter">The base <see cref="FilterParameter"/> instance.</param>
        /// <param name="overrides">The <see cref="FilterParameter"/> instance with override values.</param>
        /// <returns>A new <see cref="FilterParameter"/> instance with merged properties.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameter"/> or <paramref name="overrides"/> is null.</exception>
        public static FilterParameter Merge(this FilterParameter parameter, FilterParameter overrides)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            ArgumentNullException.ThrowIfNull(overrides);

            return new FilterParameter
            {
                Id = parameter.Id,
                Name = overrides.Name ?? parameter.Name,
                Value = overrides.Value,
                Min = overrides.Min,
                Max = overrides.Max,
                Type = overrides.Type ?? parameter.Type,
                Unit = overrides.Unit ?? parameter.Unit,
                Description = overrides.Description ?? parameter.Description,
                IsRequired = overrides.IsRequired
            };
        }

        /// <summary>
        /// Determines whether two <see cref="FilterParameter"/> instances have the same configuration (excluding <see cref="FilterParameter.Value"/>).
        /// </summary>
        /// <param name="parameter">The first <see cref="FilterParameter"/> instance.</param>
        /// <param name="other">The second <see cref="FilterParameter"/> instance.</param>
        /// <returns><see langword="true"/> if the configurations match; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameter"/> or <paramref name="other"/> is null.</exception>
        public static bool HasSameConfiguration(this FilterParameter parameter, FilterParameter other)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            ArgumentNullException.ThrowIfNull(other);

            return
                parameter.Id == other.Id &&
                string.Equals(parameter.Name, other.Name, StringComparison.Ordinal) &&
                parameter.Min == other.Min &&
                parameter.Max == other.Max &&
                string.Equals(parameter.Type, other.Type, StringComparison.Ordinal) &&
                string.Equals(parameter.Unit, other.Unit, StringComparison.Ordinal) &&
                string.Equals(parameter.Description, other.Description, StringComparison.Ordinal) &&
                parameter.IsRequired == other.IsRequired;
        }
    }
}
