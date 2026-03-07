#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Base exception for image processing operations
    /// </summary>
    public class ImageProcessingException : Exception
    {
        public string ErrorCode { get; set; } = string.Empty;
        public int? ErrorCode_Numeric { get; set; }
        public DateTime OccurredAt { get; set; }

        public ImageProcessingException()
            : base("An image processing error occurred")
        {
            OccurredAt = DateTime.UtcNow;
        }

        public ImageProcessingException(string message)
            : base(message)
        {
            OccurredAt = DateTime.UtcNow;
        }

        public ImageProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
            OccurredAt = DateTime.UtcNow;
        }

        public ImageProcessingException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
            OccurredAt = DateTime.UtcNow;
        }

        public ImageProcessingException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            OccurredAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            var baseString = base.ToString();
            var details = $"ErrorCode: {ErrorCode}\nOccurredAt: {OccurredAt:O}";
            return $"{baseString}\n{details}";
        }
    }

    /// <summary>
    /// Exception thrown when image file operations fail
    /// </summary>
    public class ImageFileException : ImageProcessingException
    {
        public string? FilePath { get; set; }

        public ImageFileException(string message, string filePath)
            : base(message, "FILE_ERROR")
        {
            FilePath = filePath;
        }

        public ImageFileException(string message, string filePath, Exception innerException)
            : base(message, "FILE_ERROR", innerException)
        {
            FilePath = filePath;
        }
    }

    /// <summary>
    /// Exception thrown when image validation fails
    /// </summary>
    public class InvalidImageException : ImageProcessingException
    {
        public InvalidImageException(string message)
            : base(message, "INVALID_IMAGE")
        {
        }

        public InvalidImageException(string message, Exception innerException)
            : base(message, "INVALID_IMAGE", innerException)
        {
        }
    }
}
