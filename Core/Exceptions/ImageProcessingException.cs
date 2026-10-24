#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Base exception for image processing operations.
    /// Derives from <see cref="GpuImageProcessing.Exceptions.GpuImageProcessingException"/>; the numeric
    /// <c>ErrorCode</c> and <c>OccurredAt</c> come from that base class, while <see cref="ErrorCategory"/>
    /// carries the string category (for example <c>FILE_ERROR</c>).
    /// </summary>
    public class ImageProcessingException : GpuImageProcessing.Exceptions.GpuImageProcessingException
    {
        /// <summary>
        /// String category of the error, for example <c>FILE_ERROR</c> or <c>INVALID_IMAGE</c>.
        /// </summary>
        public string ErrorCategory { get; set; } = string.Empty;

        public ImageProcessingException()
            : base("An image processing error occurred")
        {
        }

        public ImageProcessingException(string message)
            : base(message ?? throw new ArgumentNullException(nameof(message)))
        {
        }

        public ImageProcessingException(string message, Exception innerException)
            : base(message ?? throw new ArgumentNullException(nameof(message)), innerException)
        {
        }

        public ImageProcessingException(string message, string errorCategory)
            : base(message ?? throw new ArgumentNullException(nameof(message)))
        {
            ErrorCategory = errorCategory ?? throw new ArgumentNullException(nameof(errorCategory));
        }

        public ImageProcessingException(string message, string errorCategory, Exception innerException)
            : base(message ?? throw new ArgumentNullException(nameof(message)), innerException)
        {
            ErrorCategory = errorCategory ?? throw new ArgumentNullException(nameof(errorCategory));
        }

        public override string ToString()
        {
            var baseString = base.ToString();
            return $"{baseString}\nErrorCategory: {ErrorCategory}";
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
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            FilePath = filePath;
        }

        public ImageFileException(string message, string filePath, Exception innerException)
            : base(message, "FILE_ERROR", innerException)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
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
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }

        public InvalidImageException(string message, Exception innerException)
            : base(message, "INVALID_IMAGE", innerException)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }
    }
}
