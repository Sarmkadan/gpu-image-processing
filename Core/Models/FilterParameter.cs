// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Represents a configurable parameter for an image filter
    /// </summary>
    public class FilterParameter
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Value { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public string Type { get; set; } = "float";
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the FilterParameter class
        /// </summary>
        public FilterParameter()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Validates if the current value is within the allowed range
        /// </summary>
        public bool IsValid()
        {
            return Value >= Min && Value <= Max;
        }

        /// <summary>
        /// Clamps the value to the valid range
        /// </summary>
        public void ClampValue()
        {
            if (Value < Min)
                Value = Min;
            else if (Value > Max)
                Value = Max;
        }

        /// <summary>
        /// Creates a copy of this parameter
        /// </summary>
        public FilterParameter Clone()
        {
            return new FilterParameter
            {
                Id = this.Id,
                Name = this.Name,
                Value = this.Value,
                Min = this.Min,
                Max = this.Max,
                Type = this.Type,
                Unit = this.Unit,
                Description = this.Description,
                IsRequired = this.IsRequired
            };
        }

        /// <summary>
        /// Gets a normalized value between 0 and 1
        /// </summary>
        public float GetNormalizedValue()
        {
            if (Max <= Min)
                return 0f;

            return (Value - Min) / (Max - Min);
        }

        /// <summary>
        /// Sets value from a normalized 0-1 range
        /// </summary>
        public void SetFromNormalizedValue(float normalized)
        {
            Value = Min + (normalized * (Max - Min));
            ClampValue();
        }
    }
}
