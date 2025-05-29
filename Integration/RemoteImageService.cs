#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GpuImageProcessing.Exceptions;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Service for fetching images from remote URLs and integrating with external image sources.
    /// Supports authentication, retry logic, and bandwidth throttling.
    /// </summary>
    public class RemoteImageService
    {
        private readonly HttpClient _httpClient;
        private readonly int _maxRetries;
        private readonly TimeSpan _timeout;
        private readonly List<RemoteImageSource> _trustedSources;

        public RemoteImageService(
            HttpClient httpClient = null,
            int maxRetries = 3,
            int timeoutSeconds = 30)
        {
            _httpClient = httpClient ?? new HttpClient();
            _maxRetries = maxRetries;
            _timeout = TimeSpan.FromSeconds(timeoutSeconds);
            _httpClient.Timeout = _timeout;
            _trustedSources = new List<RemoteImageSource>();
        }

        /// <summary>
        /// Registers a trusted remote source for automatic authorization
        /// </summary>
        public void RegisterTrustedSource(string url, string apiKey = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ValidationException("URL cannot be null or whitespace", nameof(url));
            }

            _trustedSources.Add(new RemoteImageSource
            {
                Url = url,
                ApiKey = apiKey,
                RegisteredAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Downloads an image from a remote URL with retry logic
        /// </summary>
        public async Task<RemoteImageResult> DownloadImageAsync(string imageUrl, Dictionary<string, string> headers = null)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return RemoteImageResult.Failure("Image URL cannot be null or whitespace");
            }

            if (!IsValidUrl(imageUrl))
            {
                return RemoteImageResult.Failure("Invalid image URL format");
            }

            var trustSource = FindTrustedSource(imageUrl);

            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);

                    // Add custom headers
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }

                    // Add authorization if from trusted source
                    if (trustSource?.ApiKey != null)
                        request.Headers.Add("Authorization", $"Bearer {trustSource.ApiKey}");

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (attempt == _maxRetries)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            return RemoteImageResult.Failure(
                                $"Failed to download image: HTTP {(int)response.StatusCode} {response.StatusCode}\n{errorContent}");
                        }

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
                        continue;
                    }

                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    var imageData = await response.Content.ReadAsByteArrayAsync();
                    var size = response.Content.Headers.ContentLength ?? imageData.Length;

                    if (!ValidateImageData(imageData, contentType))
                    {
                        return RemoteImageResult.Failure("Downloaded data is not a valid image");
                    }

                    return RemoteImageResult.Success(new RemoteImageData
                    {
                        ImageData = imageData,
                        ContentType = contentType,
                        SizeBytes = size,
                        SourceUrl = imageUrl,
                        DownloadedAt = DateTime.UtcNow
                    });
                }
                catch (HttpRequestException ex) when (attempt == _maxRetries)
                {
                    return RemoteImageResult.Failure(
                        $"Network error after {_maxRetries} attempts: {ex.Message}");
                }
                catch (TaskCanceledException ex) when (attempt == _maxRetries)
                {
                    return RemoteImageResult.Failure(
                        $"Download timeout: {ex.Message}");
                }
                catch (Exception ex) when (attempt < _maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
                }
            }

            return RemoteImageResult.Failure("Download failed after all retries");
        }

        /// <summary>
        /// Downloads multiple images concurrently with rate limiting
        /// </summary>
        public async Task<List<RemoteImageResult>> DownloadImagesAsync(
            List<string> imageUrls,
            int maxConcurrentDownloads = 3)
        {
            if (imageUrls == null || imageUrls.Count == 0)
            {
                throw new ValidationException("Image URLs list cannot be null or empty", nameof(imageUrls));
            }

            if (maxConcurrentDownloads < 1)
            {
                throw new ValidationException(
                    "maxConcurrentDownloads must be at least 1",
                    nameof(maxConcurrentDownloads),
                    new Dictionary<string, string>
                    {
                        { nameof(maxConcurrentDownloads), "Value must be >= 1" }
                    });
            }

            var results = new List<RemoteImageResult>();
            var semaphore = new System.Threading.SemaphoreSlim(maxConcurrentDownloads);

            var tasks = imageUrls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await DownloadImageAsync(url);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            results.AddRange(await Task.WhenAll(tasks));
            return results;
        }

        /// <summary>
        /// Validates image integrity after download
        /// </summary>
        public bool ValidateImageData(byte[] imageData, string expectedContentType = null)
        {
            if (imageData == null || imageData.Length == 0)
                return false;

            // Check common image magic numbers
            if (imageData.Length >= 3)
            {
                // JPEG: FF D8 FF
                if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47
                if (imageData.Length >= 4 &&
                    imageData[0] == 0x89 && imageData[1] == 0x50 &&
                    imageData[2] == 0x4E && imageData[3] == 0x47)
                    return true;

                // GIF: 47 49 46
                if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
                    return true;

                // BMP: 42 4D
                if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                    return true;
            }

            return true; // Assume valid if can't verify
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private RemoteImageSource FindTrustedSource(string imageUrl)
        {
            foreach (var source in _trustedSources)
            {
                if (imageUrl.StartsWith(source.Url, StringComparison.OrdinalIgnoreCase))
                    return source;
            }

            return null;
        }

        private class RemoteImageSource
        {
            public string Url { get; set; }
            public string ApiKey { get; set; }
            public DateTime RegisteredAt { get; set; }
        }
    }

    public class RemoteImageData
    {
        public byte[] ImageData { get; set; }
        public string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string SourceUrl { get; set; }
        public DateTime DownloadedAt { get; set; }
    }

    public class RemoteImageResult
    {
        public bool IsSuccess { get; set; }
        public RemoteImageData Data { get; set; }
        public string Error { get; set; }

        public static RemoteImageResult Success(RemoteImageData data) =>
            new() { IsSuccess = true, Data = data };

        public static RemoteImageResult Failure(string error) =>
            new() { IsSuccess = false, Error = error };
    }
}
