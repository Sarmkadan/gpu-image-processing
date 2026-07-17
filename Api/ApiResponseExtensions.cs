#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Api
{
	/// <summary>
	/// Extension methods that make working with <see cref="ApiResponse{T}"/> more convenient.
	/// </summary>
	public static class ApiResponseExtensions
	{
		/// <summary>
		/// Returns the <see cref="ApiResponse{T}.Data"/> when the response indicates success;
		/// otherwise throws an <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <typeparam name="T">The type of the wrapped data.</typeparam>
		/// <param name="response">The API response to evaluate.</param>
		/// <returns>The data payload of a successful response.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">The response <see cref="ApiResponse{T}.IsSuccess"/> is <c>false</c>.</exception>
		public static T EnsureSuccess<T>(this ApiResponse<T> response)
		{
			ArgumentNullException.ThrowIfNull(response);

			return response.IsSuccess
				? response.Data
				: throw new InvalidOperationException(
					string.Format(CultureInfo.InvariantCulture,
						"API response indicates failure: {0}", response.Message));
		}

		/// <summary>
		/// Retrieves a read‑only collection of error messages from the response.
		/// Returns an empty collection when no errors are present.
		/// </summary>
		/// <typeparam name="T">The type of the wrapped data.</typeparam>
		/// <param name="response">The API response to inspect.</param>
		/// <returns>An <see cref="IReadOnlyList{String}"/> containing each error's message.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> is <c>null</c>.</exception>
		public static IReadOnlyList<string> GetErrorMessages<T>(this ApiResponse<T> response)
		{
			ArgumentNullException.ThrowIfNull(response);

			return response.Errors?.Select(e => e.Message).ToArray() ?? Array.Empty<string>();
		}

		/// <summary>
		/// Sets the <see cref="ApiMetadata.RequestId"/> on the response metadata and returns the same response
		/// instance to enable fluent usage.
		/// </summary>
		/// <typeparam name="T">The type of the wrapped data.</typeparam>
		/// <param name="response">The API response to modify.</param>
		/// <param name="requestId">The request identifier to assign.</param>
		/// <returns>The modified <see cref="ApiResponse{T}"/> instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="requestId"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="requestId"/> is an empty string.</exception>
		public static ApiResponse<T> WithRequestId<T>(this ApiResponse<T> response, string requestId)
		{
			ArgumentNullException.ThrowIfNull(response);
			ArgumentException.ThrowIfNullOrEmpty(requestId);

			// Ensure Metadata is not null (it is always instantiated by the factory methods,
			// but guard against external manipulation).
			response.Metadata ??= new ApiMetadata();

			response.Metadata.RequestId = requestId;
			return response;
		}
	}
}