#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// HTTP client wrapper for downloading images from remote sources.
    /// Provides retry logic, timeout handling, and content validation.
    /// </summary>
    public class HttpImageClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpImageClient> _logger;
        private readonly int _maxRetries;
        private readonly TimeSpan _timeout;

        public HttpImageClient(ILogger<HttpImageClient> logger, int maxRetries = 3, TimeSpan? timeout = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxRetries = maxRetries;
            _timeout = timeout ?? TimeSpan.FromSeconds(30);

            _httpClient = new HttpClient
            {
                Timeout = _timeout
            };

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GpuImageProcessing/1.0");
        }

        /// <summary>
        /// Downloads an image from a URL and saves it to specified path.
        /// Implements retry logic with exponential backoff.
        /// </summary>
        public async Task<bool> DownloadImageAsync(string url, string outputPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(outputPath))
            {
                _logger.LogError("Invalid URL or output path provided");
                return false;
            }

            for (int attempt = 0; attempt < _maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation(
                        "Downloading image - URL: {Url}, Attempt: {Attempt}/{MaxRetries}",
                        url,
                        attempt + 1,
                        _maxRetries);

                    using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning(
                                "HTTP request failed - URL: {Url}, StatusCode: {StatusCode}",
                                url,
                                response.StatusCode);

                            if (attempt < _maxRetries - 1)
                            {
                                await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
                                continue;
                            }

                            return false;
                        }

                        // Validate content type
                        if (!IsValidImageContent(response.Content.Headers.ContentType?.MediaType))
                        {
                            _logger.LogError(
                                "Invalid content type - Expected image/* but got {ContentType}",
                                response.Content.Headers.ContentType?.MediaType);
                            return false;
                        }

                        // Save to file
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                            {
                                await stream.CopyToAsync(fileStream, 8192, cancellationToken);
                            }
                        }

                        _logger.LogInformation(
                            "Image downloaded successfully - URL: {Url}, Size: {FileSize} bytes",
                            url,
                            new FileInfo(outputPath).Length);

                        return true;
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "HTTP request failed - URL: {Url}, Error: {ErrorMessage}, Attempt: {Attempt}",
                        url,
                        ex.Message,
                        attempt + 1);

                    if (attempt < _maxRetries - 1)
                    {
                        await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error downloading image - URL: {Url}", url);
                    return false;
                }
            }

            _logger.LogError("Failed to download image after {MaxRetries} attempts - URL: {Url}", _maxRetries, url);
            return false;
        }

        /// <summary>
        /// Uploads a processed image to a remote endpoint.
        /// </summary>
        public async Task<bool> UploadImageAsync(string localPath, string uploadUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
            {
                _logger.LogError("Local file not found - Path: {LocalPath}", localPath);
                return false;
            }

            try
            {
                _logger.LogInformation("Uploading image - LocalPath: {LocalPath}, UploadUrl: {UploadUrl}", localPath, uploadUrl);

                using (var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                {
                    using (var content = new StreamContent(fileStream))
                    {
                        var fileName = Path.GetFileName(localPath);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(localPath));

                        using (var response = await _httpClient.PostAsync(uploadUrl, content, cancellationToken))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogError(
                                    "Upload failed - StatusCode: {StatusCode}, Error: {ErrorContent}",
                                    response.StatusCode,
                                    await response.Content.ReadAsStringAsync());
                                return false;
                            }

                            _logger.LogInformation(
                                "Image uploaded successfully - LocalPath: {LocalPath}, ResponseCode: {StatusCode}",
                                localPath,
                                response.StatusCode);

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image - LocalPath: {LocalPath}", localPath);
                return false;
            }
        }

        /// <summary>
        /// Checks if a remote image URL is accessible.
        /// </summary>
        public async Task<bool> VerifyImageUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                using (var response = await _httpClient.HeadAsync(url, cancellationToken))
                {
                    bool accessible = response.IsSuccessStatusCode;
                    bool validContent = IsValidImageContent(response.Content.Headers.ContentType?.MediaType);

                    if (accessible && validContent)
                    {
                        _logger.LogDebug("Image URL verified - URL: {Url}", url);
                        return true;
                    }

                    _logger.LogWarning(
                        "Image URL verification failed - URL: {Url}, Accessible: {Accessible}, ValidContent: {ValidContent}",
                        url,
                        accessible,
                        validContent);

                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying image URL - URL: {Url}", url);
                return false;
            }
        }

        /// <summary>
        /// Calculates exponential backoff delay for retry attempts.
        /// </summary>
        private TimeSpan GetBackoffDelay(int attemptNumber)
        {
            // Exponential backoff: 100ms, 200ms, 400ms, ...
            int delayMs = 100 * (int)Math.Pow(2, attemptNumber);
            return TimeSpan.FromMilliseconds(delayMs);
        }

        /// <summary>
        /// Validates that content type is a valid image format.
        /// </summary>
        private bool IsValidImageContent(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets MIME type for a file.
        /// </summary>
        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
