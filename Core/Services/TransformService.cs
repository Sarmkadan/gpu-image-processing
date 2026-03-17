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

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Service for managing image transformation operations
    /// </summary>
    public class TransformService
    {
        private readonly GenericRepository<Transform> _transformRepository;

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
