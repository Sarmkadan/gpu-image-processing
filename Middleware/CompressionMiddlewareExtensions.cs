using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Middleware
{
	/// <summary>
	/// Provides extension methods for <see cref="CompressionMiddleware"/>.
	/// </summary>
	public static class CompressionMiddlewareExtensions
	{
		/// <summary>
		/// Determines if the specified payload size meets the threshold for compression.
		/// </summary>
		/// <param name="middleware">The <see cref="CompressionMiddleware"/> instance.</param>
		/// <param name="size">The size of the payload to check.</param>
		/// <returns><c>true</c> if the size is greater than or equal to the minimum threshold; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="IsEligibleForCompression(CompressionMiddleware, int)"/></exception>
		public static bool IsEligibleForCompression(this CompressionMiddleware middleware, int size)
		{
			ArgumentNullException.ThrowIfNull(middleware);
			return size >= middleware.GetStatistics().MinSizeToCompress;
		}

		/// <summary>
		/// Gets the configured compression level for the middleware.
		/// </summary>
		/// <param name="middleware">The <see cref="CompressionMiddleware"/> instance.</param>
		/// <returns>A string representing the configured compression level.</returns>
		/// <exception cref="ArgumentNullException"><inheritdoc cref="GetConfiguredCompressionLevel(CompressionMiddleware)"/></exception>
		public static string GetConfiguredCompressionLevel(this CompressionMiddleware middleware)
		{
			ArgumentNullException.ThrowIfNull(middleware);
			return middleware.GetStatistics().CompressionLevel;
		}

		/// <summary>
		/// Processes the provided context through the compression middleware.
		/// </summary>
		/// <param name="middleware">The <see cref="CompressionMiddleware"/> instance.</param>
		/// <param name="context">The <see cref="RequestMiddlewareContext"/> to process.</param>
		/// <returns>A task that represents the asynchronous operation, containing the result of the middleware processing.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="middleware"/> is null.</exception>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
		public static Task<RequestMiddlewareResult> ProcessContextAsync(this CompressionMiddleware middleware, RequestMiddlewareContext context)
		{
			ArgumentNullException.ThrowIfNull(middleware);
			ArgumentNullException.ThrowIfNull(context);
			return middleware.ProcessAsync(context);
		}
	}
}
