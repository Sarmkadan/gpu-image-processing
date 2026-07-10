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
        /// <returns>The number of active webhook subscriptions.</returns>
        public static int GetActiveWebhookCount(this WebhookHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var subscriptions = handler.GetWebhooks();
            return subscriptions.Count(s => s.IsActive);
        }

        /// <summary>
        /// Gets webhook subscriptions filtered by event type.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <param name="eventType">The event type to filter by.</param>
        /// <returns>List of matching webhook subscriptions.</returns>
        public static List<WebhookSubscription> GetWebhooksByEventType(this WebhookHandler handler, string eventType)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be empty", nameof(eventType));

            var subscriptions = handler.GetWebhooks();
            return subscriptions.Where(s => s.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Gets webhook subscriptions filtered by URL.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <param name="url">The URL to filter by.</param>
        /// <returns>List of matching webhook subscriptions.</returns>
        public static List<WebhookSubscription> GetWebhooksByUrl(this WebhookHandler handler, string url)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));

            var subscriptions = handler.GetWebhooks();
            return subscriptions.Where(s => s.WebhookUrl.Contains(url, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Checks if a webhook with the specified ID is registered and active.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <param name="webhookId">The webhook ID to check.</param>
        /// <returns>True if the webhook exists and is active; otherwise false.</returns>
        public static bool IsWebhookActive(this WebhookHandler handler, string webhookId)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (string.IsNullOrWhiteSpace(webhookId))
                return false;

            var subscriptions = handler.GetWebhooks();
            var subscription = subscriptions.FirstOrDefault(s => s.Id == webhookId);
            return subscription != null && subscription.IsActive;
        }

        /// <summary>
        /// Gets the oldest active webhook subscription.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <returns>The oldest active webhook subscription, or null if none exists.</returns>
        public static WebhookSubscription? GetOldestActiveWebhook(this WebhookHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var subscriptions = handler.GetWebhooks();
            return subscriptions
                .Where(s => s.IsActive)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the most recent webhook subscription.
        /// </summary>
        /// <param name="handler">The webhook handler instance.</param>
        /// <returns>The most recently created webhook subscription, or null if none exists.</returns>
        public static WebhookSubscription? GetMostRecentWebhook(this WebhookHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var subscriptions = handler.GetWebhooks();
            return subscriptions
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();
        }
    }
}