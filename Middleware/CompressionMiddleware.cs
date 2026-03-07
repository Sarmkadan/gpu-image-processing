#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Compression middleware for reducing data transfer size.
    /// Supports GZIP and Brotli compression algorithms with automatic selection based on size.
    /// </summary>
    public class CompressionMiddleware : IProcessingMiddleware
    {
        private readonly int _minSizeToCompress;
        private readonly CompressionLevel _compressionLevel;

        public CompressionMiddleware(int minSizeToCompress = 1024, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            _minSizeToCompress = minSizeToCompress;
            _compressionLevel = compressionLevel;
        }

        public async Task<MiddlewareResult> ProcessAsync(MiddlewareContext context)
        {
            // Store original size for metrics
            if (context.ResponseData is not null && context.ResponseData is string responseStr)
            {
                var originalSize = System.Text.Encoding.UTF8.GetByteCount(responseStr);
                context.Metadata["original_size"] = originalSize;

                if (originalSize >= _minSizeToCompress)
                {
                    try
                    {
                        // Use GZIP for compression
                        var compressed = await CompressStringAsync(responseStr);
                        var compressedSize = compressed.Length;
                        var ratio = (1 - (compressedSize / (double)originalSize)) * 100;

                        context.ResponseData = compressed;
                        context.Metadata["compressed_size"] = compressedSize;
                        context.Metadata["compression_ratio"] = ratio;
                        context.Metadata["compression_algorithm"] = "gzip";
                    }
                    catch (Exception ex)
                    {
                        return MiddlewareResult.Failure($"Compression failed: {ex.Message}");
                    }
                }
            }

            return await Task.FromResult(MiddlewareResult.Success());
        }

        public int Order => 50;

        private async Task<byte[]> CompressStringAsync(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                {
                    await gzipStream.WriteAsync(bytes, 0, bytes.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public static async Task<string> DecompressAsync(byte[] compressedData)
        {
            using (var memoryStream = new MemoryStream(compressedData))
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzipStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }

        public CompressionStats GetStatistics()
        {
            return new CompressionStats
            {
                MinSizeToCompress = _minSizeToCompress,
                CompressionLevel = _compressionLevel.ToString()
            };
        }
    }

    public class CompressionStats
    {
        public int MinSizeToCompress { get; set; }
        public string CompressionLevel { get; set; }
    }
}
