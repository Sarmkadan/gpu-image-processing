#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// Standardized API response wrapper for consistency across all endpoints.
    /// Provides success/failure with typed data, error messages, and request metadata.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<ApiError> Errors { get; set; }
        public ApiMetadata Metadata { get; set; }

        public static ApiResponse<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Errors = new List<ApiError>(),
                Metadata = new ApiMetadata()
            };
        }

        public static ApiResponse<T> Failure(string message, T defaultData = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = defaultData,
                Message = message,
                Errors = new List<ApiError> { new ApiError { Message = message } },
                Metadata = new ApiMetadata()
            };
        }

        public static ApiResponse<T> Failure(string message, List<ApiError> errors, T defaultData = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = defaultData,
                Message = message,
                Errors = errors,
                Metadata = new ApiMetadata()
            };
        }

        public void AddError(string code, string message, string details = null)
        {
            if (Errors == null)
                Errors = new List<ApiError>();

            Errors.Add(new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            });
        }
    }

    public class ApiError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ApiMetadata
    {
        public string Version { get; set; } = "1.0";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string Environment { get; set; }
    }

    /// <summary>
    /// Pagination wrapper for list responses
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;

        public static PaginatedResponse<T> Create(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            return new PaginatedResponse<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
