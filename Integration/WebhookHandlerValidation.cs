#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides validation helpers for <see cref="WebhookHandler"/> instances.
    /// </summary>
    public static class WebhookHandlerValidation
    {
        private static readonly FieldInfo _subscriptionsField = typeof(WebhookHandler)
            .GetField("_subscriptions", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _subscriptions field in WebhookHandler");

        private static readonly PropertyInfo _httpClientProperty = typeof(WebhookHandler)
            .GetProperty("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _httpClient property in WebhookHandler");

        private static readonly PropertyInfo _loggerProperty = typeof(WebhookHandler)
            .GetProperty("_logger", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _logger property in WebhookHandler");

        private static readonly PropertyInfo _lockObjectProperty = typeof(WebhookHandler)
            .GetProperty("_lockObject", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _lockObject property in WebhookHandler");

        /// <summary>
        /// Validates a <see cref="WebhookHandler"/> instance.
        /// </summary>
        /// <param name="value">The webhook handler to validate.</param>
        /// <returns>A read‑only list of validation error messages; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this WebhookHandler value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate subscriptions list
            var subscriptions = GetSubscriptions(value);
            if (subscriptions is not System.Collections.IList)
            {
                errors.Add("Subscriptions list must be a non‑null IList instance.");
            }

            // Validate http client
            var httpClient = GetHttpClient(value);
            if (httpClient is null)
            {
                errors.Add("HttpClient cannot be null.");
            }

            // Validate logger
            var logger = GetLogger(value);
            if (logger is null)
            {
                errors.Add("Logger cannot be null.");
            }

            // Validate lock object
            var lockObject = GetLockObject(value);
            if (lockObject is null)
            {
                errors.Add("Lock object cannot be null.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="WebhookHandler"/> instance is valid.
        /// </summary>
        /// <param name="value">The webhook handler to check.</param>
        /// <returns><c>true</c> if the instance is non‑null and has no validation errors; otherwise <c>false</c>.</returns>
        public static bool IsValid(this WebhookHandler value) =>
            value is not null && value.Validate().Count == 0;

        /// <summary>
        /// Ensures that a <see cref="WebhookHandler"/> instance is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The webhook handler to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid; the message contains the validation errors.</exception>
        public static void EnsureValid(this WebhookHandler value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count is not 0)
            {
                ThrowValidationException("WebhookHandler", errors);
            }
        }

        private static System.Collections.IList? GetSubscriptions(WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            return _subscriptionsField.GetValue(handler) as System.Collections.IList;
        }

        private static object? GetHttpClient(WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            return _httpClientProperty.GetValue(handler);
        }

        private static object? GetLogger(WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            return _loggerProperty.GetValue(handler);
        }

        private static object? GetLockObject(WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            return _lockObjectProperty.GetValue(handler);
        }

        private static void ThrowValidationException(string typeName, IReadOnlyList<string> errors)
        {
            throw new ArgumentException(
                $"{typeName} is not valid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Provides validation helpers for <see cref="WebhookRetryPolicy"/> instances.
    /// </summary>
    internal static class WebhookRetryPolicyValidation
    {
        /// <summary>
        /// Validates a <see cref="WebhookRetryPolicy"/> instance.
        /// </summary>
        /// <param name="value">The retry policy to validate.</param>
        /// <returns>A read‑only list of validation error messages; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        internal static IReadOnlyList<string> Validate(this WebhookRetryPolicy value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate MaxRetries
            if (value.MaxRetries < 0)
            {
                errors.Add("MaxRetries cannot be negative.");
            }

            // Validate InitialDelayMs
            if (value.InitialDelayMs <= 0)
            {
                errors.Add("InitialDelayMs must be positive.");
            }

            // Validate MaxDelayMs
            if (value.MaxDelayMs <= 0)
            {
                errors.Add("MaxDelayMs must be positive.");
            }
            else if (value.MaxDelayMs is { } maxDelay && maxDelay < value.InitialDelayMs)
            {
                errors.Add("MaxDelayMs must be greater than or equal to InitialDelayMs.");
            }

            // Validate BackoffMultiplier
            if (value.BackoffMultiplier <= 0.0)
            {
                errors.Add("BackoffMultiplier must be positive.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="WebhookRetryPolicy"/> instance is valid.
        /// </summary>
        /// <param name="value">The retry policy to check.</param>
        /// <returns><c>true</c> if the instance is non‑null and has no validation errors; otherwise <c>false</c>.</returns>
        internal static bool IsValid(this WebhookRetryPolicy value) =>
            value is not null && value.Validate().Count == 0;

        /// <summary>
        /// Ensures that a <see cref="WebhookRetryPolicy"/> instance is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The retry policy to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid; the message contains the validation errors.</exception>
        internal static void EnsureValid(this WebhookRetryPolicy value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count is not 0)
            {
                ThrowValidationException("WebhookRetryPolicy", errors);
            }
        }

        private static void ThrowValidationException(string typeName, IReadOnlyList<string> errors)
        {
            throw new ArgumentException(
                $"{typeName} is not valid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Provides validation helpers for <see cref="WebhookSubscription"/> instances.
    /// </summary>
    internal static class WebhookSubscriptionValidation
    {
        /// <summary>
        /// Validates a <see cref="WebhookSubscription"/> instance.
        /// </summary>
        /// <param name="value">The subscription to validate.</param>
        /// <returns>A read‑only list of validation error messages; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        internal static IReadOnlyList<string> Validate(this WebhookSubscription value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Id
            if (value.Id is not { Length: > 0 } || string.IsNullOrWhiteSpace(value.Id))
            {
                errors.Add("Id cannot be null or whitespace.");
            }

            // Validate WebhookUrl
            if (value.WebhookUrl is not { Length: > 0 } || string.IsNullOrWhiteSpace(value.WebhookUrl))
            {
                errors.Add("WebhookUrl cannot be null or whitespace.");
            }
            else if (!Uri.IsWellFormedUriString(value.WebhookUrl, UriKind.Absolute))
            {
                errors.Add("WebhookUrl must be a valid absolute URI.");
            }

            // Validate EventType
            if (value.EventType is not { Length: > 0 } || string.IsNullOrWhiteSpace(value.EventType))
            {
                errors.Add("EventType cannot be null or whitespace.");
            }

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                errors.Add("CreatedAt cannot be the default DateTime value.");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CreatedAt cannot be in the future.");
            }

            // Validate FailureCount
            if (value.FailureCount < 0)
            {
                errors.Add("FailureCount cannot be negative.");
            }

            // Validate RetryPolicy
            if (value.RetryPolicy is null)
            {
                errors.Add("RetryPolicy cannot be null.");
            }
            else
            {
                errors.AddRange(value.RetryPolicy.Validate());
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="WebhookSubscription"/> instance is valid.
        /// </summary>
        /// <param name="value">The subscription to check.</param>
        /// <returns><c>true</c> if the instance is non‑null and has no validation errors; otherwise <c>false</c>.</returns>
        internal static bool IsValid(this WebhookSubscription value) =>
            value is not null && value.Validate().Count == 0;

        /// <summary>
        /// Ensures that a <see cref="WebhookSubscription"/> instance is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The subscription to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid; the message contains the validation errors.</exception>
        internal static void EnsureValid(this WebhookSubscription value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count is not 0)
            {
                ThrowValidationException("WebhookSubscription", errors);
            }
        }

        private static void ThrowValidationException(string typeName, IReadOnlyList<string> errors)
        {
            throw new ArgumentException(
                $"{typeName} is not valid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
