// entire file content ...

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GpuImageProcessing.Integration;

public static class RemoteImageServiceExtensions
{
    /// <summary>
    /// Registers a trusted source with the given URL and API key.
    /// </summary>
    /// <param name="service">The RemoteImageService instance.</param>
    /// <param name="url">The URL of the trusted source.</param>
    /// <param name="apiKey">The API key of the trusted source.</param>
    public static void RegisterTrustedSource(this RemoteImageService service, string url, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(apiKey);

        service.RegisterTrustedSource(url, apiKey);
    }

    /// <summary>
    /// Downloads the image at the given URL and returns the result.
    /// </summary>
    /// <param name="service">The RemoteImageService instance.</param>
    /// <param name="url">The URL of the image to download.</param>
    /// <returns>The result of the download operation.</returns>
    public static async Task<RemoteImageResult> DownloadImageAsync(this RemoteImageService service, string url)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(url);

        return await service.DownloadImageAsync(url);
    }

    /// <summary>
    /// Downloads the images at the given URLs and returns the results.
    /// </summary>
    /// <param name="service">The RemoteImageService instance.</param>
    /// <param name="urls">The URLs of the images to download.</param>
    /// <returns>The results of the download operations.</returns>
    public static async Task<List<RemoteImageResult>> DownloadImagesAsync(this RemoteImageService service, IEnumerable<string> urls)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(urls);

        return await service.DownloadImagesAsync(urls);
    }

    /// <summary>
    /// Validates the image data and returns a boolean indicating whether it is valid.
    /// </summary>
    /// <param name="service">The RemoteImageService instance.</param>
    /// <param name="data">The image data to validate.</param>
    /// <returns>True if the image data is valid, false otherwise.</returns>
    public static bool ValidateImageData(this RemoteImageService service, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(data);

        return service.ValidateImageData(data);
    }
}
