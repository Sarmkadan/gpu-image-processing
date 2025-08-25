#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Extension methods for <see cref="WebhookHandler"/> providing additional functionality
    /// for managing and querying webhook subscriptions.
    /// </summary>
    public static class WebhookHandlerExtensions
    {
        /// <summary>
        /// Gets the count of active webhook subscriptions.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <returns>The number of active webhook subscriptions.</returns>
        public static int GetActiveWebhookCount(this WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            return handler.GetWebhooks().Count(s => s.IsActive);
        }

        /// <summary>
        /// Gets webhook subscriptions filtered by event type.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <param name="eventType">The event type to filter by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="eventType"/> is null or whitespace.</exception>
        /// <returns>List of matching webhook subscriptions.</returns>
        public static List<WebhookSubscription> GetWebhooksByEventType(this WebhookHandler handler, string eventType)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

            return handler.GetWebhooks()
                .Where(s => s.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Gets webhook subscriptions filtered by URL.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <param name="url">The URL to filter by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="url"/> is null or whitespace.</exception>
        /// <returns>List of matching webhook subscriptions.</returns>
        public static List<WebhookSubscription> GetWebhooksByUrl(this WebhookHandler handler, string url)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentException.ThrowIfNullOrWhiteSpace(url);

            return handler.GetWebhooks()
                .Where(s => s.WebhookUrl.Contains(url, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Checks if a webhook with the specified ID is registered and active.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <param name="webhookId">The webhook ID to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="webhookId"/> is null or whitespace.</exception>
        /// <returns>True if the webhook exists and is active; otherwise false.</returns>
        public static bool IsWebhookActive(this WebhookHandler handler, string webhookId)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentException.ThrowIfNullOrWhiteSpace(webhookId);

            var subscription = handler.GetWebhooks()
                .FirstOrDefault(s => s.Id == webhookId);

            return subscription?.IsActive == true;
        }

        /// <summary>
        /// Gets the oldest active webhook subscription.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <returns>The oldest active webhook subscription, or null if none exists.</returns>
        public static WebhookSubscription? GetOldestActiveWebhook(this WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            return handler.GetWebhooks()
                .Where(s => s.IsActive)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the most recent webhook subscription.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <returns>The most recently created webhook subscription, or null if none exists.</returns>
        public static WebhookSubscription? GetMostRecentWebhook(this WebhookHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            return handler.GetWebhooks()
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();
        }
    }
}