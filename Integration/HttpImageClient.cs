#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Exceptions;
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
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ValidationException("URL cannot be null or whitespace", nameof(url));
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ValidationException("Output path cannot be null or whitespace", nameof(outputPath));
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
                            var statusCode = response.StatusCode;
                            var reason = response.ReasonPhrase ?? "Unknown reason";

                            if (attempt < _maxRetries - 1)
                            {
                                _logger.LogWarning(
                                    "HTTP request failed - URL: {Url}, StatusCode: {StatusCode}, Reason: {Reason}",
                                    url,
                                    statusCode,
                                    reason);
                                await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
                                continue;
                            }

                            throw new HttpImageClientException(
                                $"HTTP request failed after {_maxRetries} attempts",
                                url,
                                (int)statusCode,
                                reason);
                        }

                        // Validate content type
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (!IsValidImageContent(contentType))
                        {
                            throw new HttpImageClientException(
                                $"Invalid content type received: {contentType}",
                                url,
                                (int)HttpStatusCode.UnsupportedMediaType);
                        }

                        // Validate output directory exists
                        var outputDirectory = Path.GetDirectoryName(outputPath);
                        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                            _logger.LogInformation("Created output directory: {OutputDirectory}", outputDirectory);
                        }

                        // Save to file
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                            {
                                await stream.CopyToAsync(fileStream, 8192, cancellationToken);
                            }
                        }

                        var fileSize = new FileInfo(outputPath).Length;
                        _logger.LogInformation(
                            "Image downloaded successfully - URL: {Url}, Size: {FileSize} bytes",
                            url,
                            fileSize);

                        return true;
                    }
                }
                catch (HttpRequestException ex) when (attempt == _maxRetries - 1)
                {
                    throw new HttpImageClientException(
                        $"Network error after {_maxRetries} attempts",
                        ex,
                        url,
                        (int)HttpStatusCode.BadGateway);
                }
                catch (OperationCanceledException ex)
                {
                    throw new HttpImageClientException(
                        "Download operation was cancelled",
                        ex,
                        url,
                        (int)HttpStatusCode.RequestTimeout);
                }
                catch (IOException ex) when (attempt == _maxRetries - 1)
                {
                    throw new HttpImageClientException(
                        "Failed to write image to disk",
                        ex,
                        url,
                        (int)HttpStatusCode.InternalServerError);
                }
                catch (Exception ex) when (attempt < _maxRetries - 1)
                {
                    _logger.LogWarning(
                        ex,
                        "Attempt {Attempt} failed for URL: {Url}, Error: {ErrorMessage}",
                        attempt + 1,
                        url,
                        ex.Message);
                    await Task.Delay(GetBackoffDelay(attempt), cancellationToken);
                }
            }

            throw new HttpImageClientException(
                $"Failed to download image after {_maxRetries} attempts",
                url,
                (int)HttpStatusCode.ServiceUnavailable);
        }

        /// <summary>
        /// Uploads a processed image to a remote endpoint.
        /// </summary>
        public async Task<bool> UploadImageAsync(string localPath, string uploadUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(localPath))
            {
                throw new ValidationException("Local path cannot be null or whitespace", nameof(localPath));
            }

            if (!File.Exists(localPath))
            {
                throw new ValidationException("Local file does not exist", nameof(localPath), validationErrors: new Dictionary<string, string>
                {
                    { nameof(localPath), $"File not found: {localPath}" }
                });
            }

            if (string.IsNullOrWhiteSpace(uploadUrl))
            {
                throw new ValidationException("Upload URL cannot be null or whitespace", nameof(uploadUrl));
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
                                var errorContent = await response.Content.ReadAsStringAsync();
                                _logger.LogError(
                                    "Upload failed - StatusCode: {StatusCode}, Error: {ErrorContent}",
                                    response.StatusCode,
                                    errorContent);
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
            catch (HttpRequestException ex)
            {
                throw new HttpImageClientException(
                    "Failed to upload image",
                    ex,
                    uploadUrl,
                    (int)HttpStatusCode.BadGateway);
            }
            catch (OperationCanceledException ex)
            {
                throw new HttpImageClientException(
                    "Upload operation was cancelled",
                    ex,
                    uploadUrl,
                    (int)HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                throw new HttpImageClientException(
                    "Error uploading image",
                    ex,
                    uploadUrl,
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Checks if a remote image URL is accessible.
        /// </summary>
        public async Task<bool> VerifyImageUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ValidationException("URL cannot be null or whitespace", nameof(url));
            }

            try
            {
                using (var response = await _httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, url),
                    cancellationToken))
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
            catch (HttpRequestException ex)
            {
                throw new HttpImageClientException(
                    "Failed to verify image URL",
                    ex,
                    url,
                    (int)HttpStatusCode.BadGateway);
            }
            catch (OperationCanceledException ex)
            {
                throw new HttpImageClientException(
                    "URL verification was cancelled",
                    ex,
                    url,
                    (int)HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                throw new HttpImageClientException(
                    "Error verifying image URL",
                    ex,
                    url,
                    (int)HttpStatusCode.InternalServerError);
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

    /// <summary>
    /// Exception thrown when HTTP image client operations fail.
    /// </summary>
    public class HttpImageClientException : GpuImageProcessingException
    {
        /// <summary>
        /// URL that caused the error.
        /// </summary>
        public string? Url { get; }

        /// <summary>
        /// HTTP status code associated with the error.
        /// </summary>
        public int? HttpStatusCode { get; }

        public HttpImageClientException(string message, string? url, int? httpStatusCode, string? errorDetails = null)
            : base(message, httpStatusCode)
        {
            Url = url;
            HttpStatusCode = httpStatusCode;
        }

        public HttpImageClientException(string message, Exception innerException, string? url, int? httpStatusCode)
            : base(message, innerException, httpStatusCode)
        {
            Url = url;
            HttpStatusCode = httpStatusCode;
        }

        public override string ToString()
        {
            var result = base.ToString();
            if (!string.IsNullOrEmpty(Url))
                result += $"\nURL: {Url}";
            if (HttpStatusCode.HasValue)
                result += $"\nHTTP Status: {HttpStatusCode} ({(HttpStatusCode)HttpStatusCode.Value})\nError Details: {Message}";
            return result;
        }
    }
}
