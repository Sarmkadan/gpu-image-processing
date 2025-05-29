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
    using TransformType = GpuImageProcessing.Core.Constants.TransformType;

    /// <summary>
    /// Represents a geometric or color space transformation operation on images
    /// </summary>
    public class Transform
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TransformType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, float> Parameters { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int ExecutionOrder { get; set; }
        public float ProcessingTimeMs { get; set; }

        /// <summary>
        /// Initializes a new instance of the Transform class
        /// </summary>
        public Transform()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a predefined transform by type with default parameters
        /// </summary>
        public static Transform CreatePredefined(TransformType type)
        {
            var transform = new Transform
            {
                Type = type,
                Name = type.ToString(),
                IsActive = true
            };

            transform.Parameters = type switch
            {
                TransformType.Rotate => new Dictionary<string, float>
                {
                    { "Angle", 0f },
                    { "Interpolation", 1f }
                },
                TransformType.Resize => new Dictionary<string, float>
                {
                    { "ScaleX", 1.0f },
                    { "ScaleY", 1.0f },
                    { "Interpolation", 1f }
                },
                TransformType.ColorSpace => new Dictionary<string, float>
                {
                    { "TargetSpace", 1f }
                },
                TransformType.Normalize => new Dictionary<string, float>
                {
                    { "Min", 0f },
                    { "Max", 255f }
                },
                _ => new Dictionary<string, float>()
            };

            return transform;
        }

        /// <summary>
        /// Sets a parameter value, creating it if it doesn't exist
        /// </summary>
        public void SetParameter(string name, float value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name cannot be empty", nameof(name));

            Parameters[name] = value;
        }

        /// <summary>
        /// Gets a parameter value with a default fallback
        /// </summary>
        public float GetParameter(string name, float defaultValue = 0f)
        {
            return Parameters.TryGetValue(name, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets all parameter names for this transform
        /// </summary>
        public List<string> GetParameterNames()
        {
            return new List<string>(Parameters.Keys);
        }

        /// <summary>
        /// Checks if a parameter exists
        /// </summary>
        public bool HasParameter(string name)
        {
            return Parameters.ContainsKey(name);
        }

        /// <summary>
        /// Clears all parameters
        /// </summary>
        public void ClearParameters()
        {
            Parameters.Clear();
        }

        /// <summary>
        /// Gets a human-readable description of the transform with its parameters
        /// </summary>
        public string GetFullDescription()
        {
            var desc = $"{Name} ({Type})\n";
            foreach (var param in Parameters)
            {
                desc += $"  {param.Key}: {param.Value}\n";
            }
            return desc;
        }

        /// <summary>
        /// Creates a copy of this transform
        /// </summary>
        public Transform Clone()
        {
            return new Transform
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Type = this.Type,
                Description = this.Description,
                Parameters = new Dictionary<string, float>(this.Parameters),
                IsActive = this.IsActive,
                ExecutionOrder = this.ExecutionOrder,
                ProcessingTimeMs = this.ProcessingTimeMs
            };
        }
    }
}
