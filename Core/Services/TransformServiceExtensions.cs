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

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Extension methods for <see cref="TransformService"/> providing additional functionality
    /// </summary>
    public static class TransformServiceExtensions
    {
        /// <summary>
        /// Creates multiple transforms of the same type in a single operation
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="type">The transform type</param>
        /// <param name="count">Number of transforms to create. Must be greater than 0.</param>
        /// <param name="namePrefix">Prefix for transform names (default: "Transform")</param>
        /// <param name="description">Optional description for all transforms</param>
        /// <returns>List of created transforms</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="namePrefix"/> is null or whitespace.</exception>
        public static async Task<IReadOnlyList<Transform>> CreateTransformsAsync(
            this TransformService service,
            global::GpuImageProcessing.Core.Constants.TransformType type,
            int count,
            string namePrefix = "Transform",
            string description = "")
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
            ArgumentException.ThrowIfNullOrWhiteSpace(namePrefix);

            var transforms = new List<Transform>(count);

            for (int i = 0; i < count; i++)
            {
                var name = $"{namePrefix} {i + 1}";
                var transform = await service.CreateTransformAsync(type, name, description);
                transforms.Add(transform);
            }

            return transforms.AsReadOnly();
        }

        /// <summary>
        /// Bulk activates transforms by their IDs
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="transformIds">Collection of transform IDs to activate</param>
        /// <returns>Number of successfully activated transforms</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformIds"/> is null.</exception>
        public static async Task<int> BulkActivateTransformsAsync(
            this TransformService service,
            IEnumerable<Guid> transformIds)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(transformIds);

            int activatedCount = 0;

            foreach (var id in transformIds)
            {
                var success = await service.ActivateTransformAsync(id);
                if (success)
                {
                    activatedCount++;
                }
            }

            return activatedCount;
        }

        /// <summary>
        /// Bulk deactivates transforms by their IDs
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="transformIds">Collection of transform IDs to deactivate</param>
        /// <returns>Number of successfully deactivated transforms</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformIds"/> is null.</exception>
        public static async Task<int> BulkDeactivateTransformsAsync(
            this TransformService service,
            IEnumerable<Guid> transformIds)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(transformIds);

            int deactivatedCount = 0;

            foreach (var id in transformIds)
            {
                var success = await service.DeactivateTransformAsync(id);
                if (success)
                {
                    deactivatedCount++;
                }
            }

            return deactivatedCount;
        }

        /// <summary>
        /// Gets the default parameter value for a specific transform type and parameter name
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="type">The transform type</param>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The default parameter value, or 0 if not found</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName"/> is null or empty.</exception>
        public static async Task<float> GetDefaultParameterValueAsync(
            this TransformService service,
            global::GpuImageProcessing.Core.Constants.TransformType type,
            string parameterName)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentException.ThrowIfNullOrEmpty(parameterName);

            // Create a temporary transform to get default values
            var tempTransform = Transform.CreatePredefined(type);
            return await Task.FromResult(tempTransform.GetParameter(parameterName, 0f));
        }

        /// <summary>
        /// Copies parameters from one transform to another
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="sourceTransformId">Source transform ID</param>
        /// <param name="targetTransformId">Target transform ID</param>
        /// <param name="parameterNames">Optional list of specific parameters to copy. If null, copies all parameters.</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when both transform IDs are empty.</exception>
        public static async Task<bool> CopyParametersAsync(
            this TransformService service,
            Guid sourceTransformId,
            Guid targetTransformId,
            IEnumerable<string>? parameterNames = null)
        {
            ArgumentNullException.ThrowIfNull(service);

            if (sourceTransformId == Guid.Empty || targetTransformId == Guid.Empty)
            {
                throw new ArgumentException("Transform IDs cannot be empty");
            }

            var sourceTransform = await service.GetTransformAsync(sourceTransformId);
            var targetTransform = await service.GetTransformAsync(targetTransformId);

            if (sourceTransform is null || targetTransform is null)
            {
                return false;
            }

            var parameters = parameterNames is not null
                ? sourceTransform.Parameters.Where(p => parameterNames.Contains(p.Key))
                : sourceTransform.Parameters;

            var paramDict = parameters.ToDictionary(p => p.Key, p => p.Value);
            return await service.SetParametersAsync(targetTransformId, paramDict);
        }

        /// <summary>
        /// Gets all parameter names for a specific transform type
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="type">The transform type</param>
        /// <returns>Collection of parameter names</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static async Task<IReadOnlyCollection<string>> GetParameterNamesAsync(
            this TransformService service,
            global::GpuImageProcessing.Core.Constants.TransformType type)
        {
            ArgumentNullException.ThrowIfNull(service);

            var tempTransform = Transform.CreatePredefined(type);
            var parameterNames = tempTransform.GetParameterNames();
            return await Task.FromResult(parameterNames.AsReadOnly());
        }

        /// <summary>
        /// Gets transforms sorted by execution order
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <returns>Transforms sorted by execution order</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static async Task<IReadOnlyList<Transform>> GetTransformsByExecutionOrderAsync(
            this TransformService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var transforms = await service.GetAllTransformsAsync();
            return transforms.OrderBy(t => t.ExecutionOrder).ToList().AsReadOnly();
        }

        /// <summary>
        /// Finds transforms by name pattern (case-insensitive)
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="namePattern">Pattern to search for</param>
        /// <returns>Matching transforms</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="namePattern"/> is null or empty.</exception>
        public static async Task<IReadOnlyList<Transform>> FindTransformsByNameAsync(
            this TransformService service,
            string namePattern)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentException.ThrowIfNullOrEmpty(namePattern);

            var transforms = await service.GetAllTransformsAsync();
            var matches = transforms
                .Where(t => t.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .AsReadOnly();

            return matches;
        }

        /// <summary>
        /// Gets transforms filtered by active status and type
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <param name="isActive">Whether to filter by active status (null for no filter)</param>
        /// <param name="type">Transform type to filter by (null for no filter)</param>
        /// <returns>Filtered transforms</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static async Task<IReadOnlyList<Transform>> GetTransformsFilteredAsync(
            this TransformService service,
            bool? isActive = null,
            global::GpuImageProcessing.Core.Constants.TransformType? type = null)
        {
            ArgumentNullException.ThrowIfNull(service);

            var transforms = await service.GetAllTransformsAsync();

            if (isActive.HasValue)
            {
                transforms = transforms.Where(t => t.IsActive == isActive.Value);
            }

            if (type.HasValue)
            {
                transforms = transforms.Where(t => t.Type == type.Value);
            }

            return transforms.OrderBy(t => t.Name).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the next available execution order number
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <returns>The next execution order number</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static async Task<int> GetNextExecutionOrderAsync(this TransformService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var transforms = await service.GetAllTransformsAsync();
            return transforms.Any()
                ? transforms.Max(t => t.ExecutionOrder) + 1
                : 0;
        }

        /// <summary>
        /// Reorders transforms to maintain sequential execution order without gaps
        /// </summary>
        /// <param name="service">The transform service instance</param>
        /// <returns>True if successful</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static async Task<bool> ReorderTransformsSequentiallyAsync(this TransformService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var transforms = (await service.GetAllTransformsAsync())
                .OrderBy(t => t.ExecutionOrder)
                .ToList();

            int newOrder = 0;
            bool allUpdated = true;

            foreach (var transform in transforms)
            {
                if (transform.ExecutionOrder != newOrder)
                {
                    var paramDict = new Dictionary<string, float> { { "ExecutionOrder", newOrder } };
                    var success = await service.SetParametersAsync(transform.Id, paramDict);
                    if (!success)
                    {
                        allUpdated = false;
                    }
                }
                newOrder++;
            }

            return allUpdated;
        }
    }
}