// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Exceptions;
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Repository;

namespace GpuImageProcessing.Core.Services
{
    /// <summary>
    /// Service for managing and applying image filters
    /// </summary>
    public class FilterService
    {
        private readonly GenericRepository<Filter> _filterRepository;
        private Dictionary<FilterType, string> _kernelCache = new();

        public FilterService(GenericRepository<Filter> filterRepository)
        {
            _filterRepository = filterRepository ?? throw new ArgumentNullException(nameof(filterRepository));
        }

        /// <summary>
        /// Creates and stores a new filter
        /// </summary>
        public async Task<Filter> CreateFilterAsync(FilterType type, string name, string description = "")
        {
            var filter = Filter.CreatePredefined(type);
            // Fix: Only update name and description if provided and not empty, preserving predefined defaults.
            if (!string.IsNullOrWhiteSpace(name))
            {
                filter.Name = name;
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                filter.Description = description;
            }

            return await _filterRepository.AddAsync(filter);
        }

        /// <summary>
        /// Gets a filter by ID
        /// </summary>
        public async Task<Filter?> GetFilterAsync(Guid filterId)
        {
            return await _filterRepository.GetByIdAsync(filterId);
        }

        /// <summary>
        /// Gets all available filters
        /// </summary>
        public async Task<IEnumerable<Filter>> GetAllFiltersAsync()
        {
            return await _filterRepository.GetAllAsync();
        }

        /// <summary>
        /// Gets all active filters
        /// </summary>
        public async Task<IEnumerable<Filter>> GetActiveFiltersAsync()
        {
            var filters = await _filterRepository.GetAllAsync();
            return filters.Where(f => f.IsActive).ToList();
        }

        /// <summary>
        /// Applies a filter to an image (configuration only - actual processing in ImageProcessingService)
        /// </summary>
        public async Task<bool> ApplyFilterAsync(Guid filterId, Dictionary<string, float> parameterValues)
        {
            var filter = await _filterRepository.GetByIdAsync(filterId);
            if (filter == null)
                return false;

            foreach (var paramValue in parameterValues)
            {
                if (!filter.UpdateParameterValue(paramValue.Key, paramValue.Value))
                    return false;
            }

            if (!filter.ValidateParameters())
                return false;

            await _filterRepository.UpdateAsync(filter);
            return true;
        }

        /// <summary>
        /// Updates filter parameters
        /// </summary>
        public async Task<bool> UpdateFilterParametersAsync(Guid filterId, Dictionary<string, float> parameters)
        {
            var filter = await _filterRepository.GetByIdAsync(filterId);
            if (filter == null)
                return false;

            foreach (var param in parameters)
            {
                if (!filter.UpdateParameterValue(param.Key, param.Value))
                    return false;
            }

            await _filterRepository.UpdateAsync(filter);
            return true;
        }

        /// <summary>
        /// Deletes a filter
        /// </summary>
        public async Task<bool> DeleteFilterAsync(Guid filterId)
        {
            return await _filterRepository.DeleteAsync(filterId);
        }

        /// <summary>
        /// Gets the OpenCL kernel code for a filter type
        /// </summary>
        public async Task<string> GetKernelCodeAsync(FilterType type)
        {
            if (_kernelCache.TryGetValue(type, out var cachedCode))
                return await Task.FromResult(cachedCode);

            var kernelCode = GenerateKernelCode(type);
            _kernelCache[type] = kernelCode;
            return await Task.FromResult(kernelCode);
        }

        /// <summary>
        /// Generates OpenCL kernel code for filter type
        /// </summary>
        private string GenerateKernelCode(FilterType type)
        {
            return type switch
            {
                FilterType.Gaussian => GaussianKernel(),
                FilterType.Bilateral => BilateralKernel(),
                FilterType.Median => MedianKernel(),
                FilterType.Sobel => SobelKernel(),
                FilterType.Threshold => ThresholdKernel(),
                _ => BasicKernel()
            };
        }

        private string GaussianKernel() => @"
__kernel void gaussian_blur(__global uchar* input, __global uchar* output, int width, int height, float sigma) {
    int gx = get_global_id(0);
    int gy = get_global_id(1);

    if (gx >= width || gy >= height) return;

    float result = 0.0f;
    float weight_sum = 0.0f;
    int kernel_size = 5;
    float sigma_sq = sigma * sigma;

    for (int i = -kernel_size/2; i <= kernel_size/2; i++) {
        for (int j = -kernel_size/2; j <= kernel_size/2; j++) {
            int px = min(max(gx + j, 0), width - 1);
            int py = min(max(gy + i, 0), height - 1);

            float dist_sq = i*i + j*j;
            float weight = exp(-dist_sq / (2.0f * sigma_sq));

            result += input[py * width + px] * weight;
            weight_sum += weight;
        }
    }

    output[gy * width + gx] = (uchar)(result / weight_sum);
}";

        private string BilateralKernel() => @"
__kernel void bilateral_filter(__global uchar* input, __global uchar* output, int width, int height,
                               float sigma_space, float sigma_range) {
    // Bilateral filter implementation
    int gx = get_global_id(0);
    int gy = get_global_id(1);

    if (gx >= width || gy >= height) return;

    float result = 0.0f;
    float weight_sum = 0.0f;
    uchar center_val = input[gy * width + gx];

    for (int i = -2; i <= 2; i++) {
        for (int j = -2; j <= 2; j++) {
            int px = min(max(gx + j, 0), width - 1);
            int py = min(max(gy + i, 0), height - 1);
            uchar val = input[py * width + px];

            float space_dist = sqrt((float)(i*i + j*j));
            float range_dist = fabs((float)(val - center_val));

            float weight = exp(-(space_dist*space_dist)/(2.0f*sigma_space*sigma_space)) *
                          exp(-(range_dist*range_dist)/(2.0f*sigma_range*sigma_range));

            result += val * weight;
            weight_sum += weight;
        }
    }

    output[gy * width + gx] = (uchar)(result / weight_sum);
}";

        private string MedianKernel() => @"
__kernel void median_filter(__global uchar* input, __global uchar* output, int width, int height, int kernel_size) {
    int gx = get_global_id(0);
    int gy = get_global_id(1);

    if (gx >= width || gy >= height) return;

    int radius = kernel_size / 2;
    uchar values[25];
    int count = 0;

    for (int i = -radius; i <= radius; i++) {
        for (int j = -radius; j <= radius; j++) {
            int px = min(max(gx + j, 0), width - 1);
            int py = min(max(gy + i, 0), height - 1);
            values[count++] = input[py * width + px];
        }
    }

    // Simple sorting for median
    for (int i = 0; i < count; i++) {
        for (int j = i + 1; j < count; j++) {
            if (values[i] > values[j]) {
                uchar tmp = values[i];
                values[i] = values[j];
                values[j] = tmp;
            }
        }
    }

    output[gy * width + gx] = values[count / 2];
}";

        private string SobelKernel() => @"
__kernel void sobel_edge(__global uchar* input, __global uchar* output, int width, int height) {
    int gx = get_global_id(0);
    int gy = get_global_id(1);

    if (gx < 1 || gx >= width - 1 || gy < 1 || gy >= height - 1) {
        output[gy * width + gx] = 0;
        return;
    }

    float gx_val = -input[(gy-1)*width+(gx-1)] + input[(gy-1)*width+(gx+1)]
                   - 2*input[gy*width+(gx-1)] + 2*input[gy*width+(gx+1)]
                   - input[(gy+1)*width+(gx-1)] + input[(gy+1)*width+(gx+1)];

    float gy_val = -input[(gy-1)*width+(gx-1)] - 2*input[(gy-1)*width+gx] - input[(gy-1)*width+(gx+1)]
                   + input[(gy+1)*width+(gx-1)] + 2*input[(gy+1)*width+gx] + input[(gy+1)*width+(gx+1)];

    float magnitude = sqrt(gx_val*gx_val + gy_val*gy_val);
    output[gy * width + gx] = (uchar)min(255.0f, magnitude);
}";

        private string ThresholdKernel() => @"
__kernel void threshold(__global uchar* input, __global uchar* output, int width, int height, uchar threshold_val) {
    int idx = get_global_id(0);
    if (idx >= width * height) return;

    output[idx] = input[idx] > threshold_val ? 255 : 0;
}";

        private string BasicKernel() => @"
__kernel void basic_operation(__global uchar* input, __global uchar* output, int width, int height) {
    int idx = get_global_id(0);
    if (idx < width * height) {
        output[idx] = input[idx];
    }
}";

        /// <summary>
        /// Gets filter statistics
        /// </summary>
        public async Task<FilterStatistics> GetStatisticsAsync()
        {
            var allFilters = await _filterRepository.GetAllAsync();
            var filters = allFilters.ToList();

            var stats = new FilterStatistics
            {
                TotalFilters = filters.Count,
                ActiveFilters = filters.Count(f => f.IsActive),
                FilterTypes = filters.DistinctBy(f => f.Type).Count(),
                CustomFilters = filters.Count(f => f.Type == FilterType.Custom),
                ParameterCount = filters.Sum(f => f.Parameters.Count)
            };

            return await Task.FromResult(stats);
        }
    }

    /// <summary>
    /// Filter statistics summary
    /// </summary>
    public class FilterStatistics
    {
        public int TotalFilters { get; set; }
        public int ActiveFilters { get; set; }
        public int FilterTypes { get; set; }
        public int CustomFilters { get; set; }
        public int ParameterCount { get; set; }
    }
}
