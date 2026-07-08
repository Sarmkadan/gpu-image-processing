#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Core.Models
{
    using FilterType = GpuImageProcessing.Core.Constants.FilterType;

    /// <summary>
    /// Represents an image filter with configurable parameters
    /// </summary>
    public class Filter
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public FilterType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<FilterParameter> Parameters { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public string? KernelCode { get; set; }
        public int ProcessingOrder { get; set; }
        public Dictionary<string, object> AppliedSettings { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the Filter class
        /// </summary>
        public Filter()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a predefined filter by type with default parameters
        /// </summary>
        public static Filter CreatePredefined(FilterType type)
        {
            var filter = new Filter
            {
                Type = type,
                Name = type.ToString(),
                IsActive = true,
                ProcessingOrder = 0
            };

            filter.Parameters = type switch
            {
                FilterType.Gaussian => new List<FilterParameter>
                {
                    new FilterParameter { Name = "Sigma", Value = 1.0f, Min = 0.1f, Max = 10.0f }
                },
                FilterType.Bilateral => new List<FilterParameter>
                {
                    new FilterParameter { Name = "SigmaSpace", Value = 0.5f, Min = 0.1f, Max = 5.0f },
                    new FilterParameter { Name = "SigmaRange", Value = 0.2f, Min = 0.1f, Max = 1.0f }
                },
                FilterType.Median => new List<FilterParameter>
                {
                    new FilterParameter { Name = "KernelSize", Value = 3.0f, Min = 3f, Max = 31f }
                },
                FilterType.Threshold => new List<FilterParameter>
                {
                    new FilterParameter { Name = "ThresholdValue", Value = 128.0f, Min = 0f, Max = 255f }
                },
                _ => new List<FilterParameter>()
            };

            return filter;
        }

        /// <summary>
        /// Adds a parameter to the filter
        /// </summary>
        public void AddParameter(FilterParameter parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            Parameters.Add(parameter);
        }

        /// <summary>
        /// Gets a parameter by name
        /// </summary>
        public FilterParameter? GetParameter(string name)
        {
            return Parameters.Find(p => p.Name == name);
        }

        /// <summary>
        /// Updates a parameter value by name
        /// </summary>
        public bool UpdateParameterValue(string name, float value)
        {
            var param = GetParameter(name);
            if (param == null)
                return false;

            if (value < param.Min || value > param.Max)
                return false;

            param.Value = value;
            return true;
        }

        /// <summary>
        /// Validates all parameters are within valid ranges
        /// </summary>
        public bool ValidateParameters()
        {
            foreach (var param in Parameters)
            {
                if (param.Value < param.Min || param.Value > param.Max)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the filter configuration as a string representation
        /// </summary>
        public string GetConfiguration()
        {
            var config = $"{Name}({Type}): ";
            var paramStrings = Parameters.ConvertAll(p => $"{p.Name}={p.Value}");
            return config + string.Join(", ", paramStrings);
        }
    }
}
