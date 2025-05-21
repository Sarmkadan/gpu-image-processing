#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;
using GpuImageProcessing.Core.Enums; // Added for TransformType

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Service for managing image transformation operations
    /// </summary>
    public class TransformService
    {
        private readonly GenericRepository<Transform> _transformRepository;
        private Dictionary<TransformType, string> _kernelCache = new(); // Added

        public TransformService(GenericRepository<Transform> transformRepository)
        {
            _transformRepository = transformRepository ?? throw new ArgumentNullException(nameof(transformRepository));
        }

        /// <summary>
        /// Creates a new transformation
        /// </summary>
        public async Task<Transform> CreateTransformAsync(TransformType type, string name, string description = "")
        {
            var transform = Transform.CreatePredefined(type);
            transform.Name = name;
            transform.Description = description;

            return await _transformRepository.AddAsync(transform);
        }

        /// <summary>
        /// Gets a transform by ID
        /// </summary>
        public async Task<Transform?> GetTransformAsync(Guid transformId)
        {
            return await _transformRepository.GetByIdAsync(transformId);
        }

        /// <summary>
        /// Gets all transforms
        /// </summary>
        public async Task<IEnumerable<Transform>> GetAllTransformsAsync()
        {
            return await _transformRepository.GetAllAsync();
        }

        /// <summary>
        /// Gets all active transforms
        /// </summary>
        public async Task<IEnumerable<Transform>> GetActiveTransformsAsync()
        {
            var transforms = await _transformRepository.GetAllAsync();
            return transforms.Where(t => t.IsActive)
                .OrderBy(t => t.ExecutionOrder).ToList();
        }

        /// <summary>
        /// Sets a transform parameter
        /// </summary>
        public async Task<bool> SetParameterAsync(Guid transformId, string parameterName, float value)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                return false;

            transform.SetParameter(parameterName, value);
            await _transformRepository.UpdateAsync(transform);
            return true;
        }

        /// <summary>
        /// Sets multiple parameters at once
        /// </summary>
        public async Task<bool> SetParametersAsync(Guid transformId, Dictionary<string, float> parameters)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                return false;

            foreach (var param in parameters)
            {
                transform.SetParameter(param.Key, param.Value);
            }

            await _transformRepository.UpdateAsync(transform);
            return true;
        }

        /// <summary>
        /// Gets a parameter value
        /// </summary>
        public async Task<float> GetParameterAsync(Guid transformId, string parameterName, float defaultValue = 0f)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                return defaultValue;

            return await Task.FromResult(transform.GetParameter(parameterName, defaultValue));
        }

        /// <summary>
        /// Chains multiple transforms for sequential execution
        /// </summary>
        public async Task<List<Transform>> ChainTransformsAsync(List<Guid> transformIds)
        {
            var transforms = new List<Transform>();
            int order = 0;

            foreach (var id in transformIds)
            {
                var transform = await _transformRepository.GetByIdAsync(id);
                if (transform != null)
                {
                    transform.ExecutionOrder = order++;
                    await _transformRepository.UpdateAsync(transform);
                    transforms.Add(transform);
                }
            }

            return transforms;
        }

        /// <summary>
        /// Activates a transform
        /// </summary>
        public async Task<bool> ActivateTransformAsync(Guid transformId)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                return false;

            transform.IsActive = true;
            await _transformRepository.UpdateAsync(transform);
            return true;
        }

        /// <summary>
        /// Deactivates a transform
        /// </summary>
        public async Task<bool> DeactivateTransformAsync(Guid transformId)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                return false;

            transform.IsActive = false;
            await _transformRepository.UpdateAsync(transform);
            return true;
        }

        /// <summary>
        /// Deletes a transform
        /// </summary>
        public async Task<bool> DeleteTransformAsync(Guid transformId)
        {
            return await _transformRepository.DeleteAsync(transformId);
        }

        /// <summary>
        /// Clones a transform
        /// </summary>
        public async Task<Transform> CloneTransformAsync(Guid transformId)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                throw new InvalidOperationException("Transform not found");

            var cloned = transform.Clone();
            cloned.Name = $"{transform.Name} (Copy)";
            return await _transformRepository.AddAsync(cloned);
        }

        /// <summary>
        /// Gets transform configuration for export
        /// </summary>
        public async Task<string> ExportConfigurationAsync(Guid transformId)
        {
            var transform = await _transformRepository.GetByIdAsync(transformId);
            if (transform == null)
                return string.Empty;

            return await Task.FromResult(transform.GetFullDescription());
        }

        /// <summary>
        /// Gets transform pipeline as a string
        /// </summary>
        public async Task<string> GetPipelineDescriptionAsync(List<Guid> transformIds)
        {
            var description = "Transform Pipeline:\n";
            int step = 1;

            foreach (var id in transformIds)
            {
                var transform = await _transformRepository.GetByIdAsync(id);
                if (transform != null)
                {
                    description += $"Step {step}: {transform.Name} ({transform.Type})\n";
                    step++;
                }
            }

            return description;
        }

        /// <summary>
        /// Gets the OpenCL kernel code for a transform type
        /// </summary>
        public async Task<string> GetKernelCodeAsync(TransformType type)
        {
            if (_kernelCache.TryGetValue(type, out var cachedCode))
                return await Task.FromResult(cachedCode);

            var kernelCode = GenerateKernelCode(type);
            _kernelCache[type] = kernelCode;
            return await Task.FromResult(kernelCode);
        }

        /// <summary>
        /// Generates OpenCL kernel code for transform type
        /// </summary>
        private string GenerateKernelCode(TransformType type)
        {
            return type switch
            {
                TransformType.Rotate => RotateKernel(),
                TransformType.Resize => ResizeKernel(),
                TransformType.ColorSpace => ColorSpaceKernel(),
                TransformType.Normalize => NormalizeKernel(),
                _ => BasicTransformKernel()
            };
        }

        private string RotateKernel() => @"
__kernel void rotate_image(__global uchar* input, __global uchar* output, int width, int height, float angle_rad) {
    int x = get_global_id(0);
    int y = get_global_id(1);

    if (x >= width || y >= height) return;

    // Rotate around center
    float cos_angle = cos(angle_rad);
    float sin_angle = sin(angle_rad);

    float center_x = width / 2.0f;
    float center_y = height / 2.0f;

    float new_x = (x - center_x) * cos_angle - (y - center_y) * sin_angle + center_x;
    float new_y = (x - center_x) * sin_angle + (y - center_y) * cos_angle + center_y;

    int input_x = (int)new_x;
    int input_y = (int)new_y;

    if (input_x >= 0 && input_x < width && input_y >= 0 && input_y < height) {
        output[y * width + x] = input[input_y * width + input_x];
    } else {
        output[y * width + x] = 0; // Black for out-of-bounds
    }
}";

        private string ResizeKernel() => @"
__kernel void resize_image(__global uchar* input, __global uchar* output, int input_width, int input_height,
                           int output_width, int output_height, float scale_x, float scale_y) {
    int x = get_global_id(0);
    int y = get_global_id(1);

    if (x >= output_width || y >= output_height) return;

    float original_x = x / scale_x;
    float original_y = y / scale_y;

    int input_x = (int)original_x;
    int input_y = (int)original_y;

    if (input_x >= 0 && input_x < input_width && input_y >= 0 && input_y < input_height) {
        output[y * output_width + x] = input[input_y * input_width + input_x];
    } else {
        output[y * output_width + x] = 0; // Should not happen with proper scaling
    }
}";
        private string ColorSpaceKernel() => @"
__kernel void convert_colorspace(__global uchar* input, __global uchar* output, int width, int height, int target_colorspace) {
    int idx = get_global_id(0);
    if (idx >= width * height) return;

    // Simplified grayscale conversion for demonstration
    // In a real scenario, this would involve proper color matrix conversions
    // target_colorspace could map to an enum or constant for RGB, HSV, etc.
    output[idx] = input[idx]; // No actual conversion, just copy for now
}";

        private string NormalizeKernel() => @"
__kernel void normalize_image(__global uchar* input, __global uchar* output, int width, int height, float min_val, float max_val) {
    int idx = get_global_id(0);
    if (idx >= width * height) return;

    // Normalize pixel values to a new range [min_val, max_val]
    // Assuming input is 0-255 range
    float normalized = (float)input[idx] / 255.0f;
    output[idx] = (uchar)(normalized * (max_val - min_val) + min_val);
}";
        private string BasicTransformKernel() => @"
__kernel void basic_transform(__global uchar* input, __global uchar* output, int width, int height) {
    int idx = get_global_id(0);
    if (idx < width * height) {
        output[idx] = input[idx];
    }
}";

        /// <summary>
        /// Gets transform statistics
        /// </summary>
        public async Task<TransformStatistics> GetStatisticsAsync()
        {
            var allTransforms = await _transformRepository.GetAllAsync();
            var transforms = allTransforms.ToList();

            var stats = new TransformStatistics
            {
                TotalTransforms = transforms.Count,
                ActiveTransforms = transforms.Count(t => t.IsActive),
                TransformTypes = transforms.DistinctBy(t => t.Type).Count(),
                AverageParametersPerTransform = transforms.Count > 0
                    ? transforms.Average(t => t.GetParameterNames().Count)
                    : 0,
                TotalExecutionTime = transforms.Sum(t => t.ProcessingTimeMs)
            };

            return await Task.FromResult(stats);
        }

        /// <summary>
        /// Gets transforms by type
        /// </summary>
        public async Task<IEnumerable<Transform>> GetByTypeAsync(TransformType type)
        {
            var transforms = await _transformRepository.GetAllAsync();
            return transforms.Where(t => t.Type == type).ToList();
        }
    }

    /// <summary>
    /// Transform statistics summary
    /// </summary>
    public class TransformStatistics
    {
        public int TotalTransforms { get; set; }
        public int ActiveTransforms { get; set; }
        public int TransformTypes { get; set; }
        public double AverageParametersPerTransform { get; set; }
        public float TotalExecutionTime { get; set; }
    }
}
