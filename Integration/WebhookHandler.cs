// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Events;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Handles webhook delivery for processing events.
    /// Allows external systems to subscribe to image processing events.
    /// </summary>
    public class WebhookHandler
    {
        private readonly ILogger<WebhookHandler> _logger;
        private readonly HttpClient _httpClient;
        private readonly List<WebhookSubscription> _subscriptions;
        private readonly object _lockObject = new object();

        public WebhookHandler(ILogger<WebhookHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            _subscriptions = new List<WebhookSubscription>();
        }

        /// <summary>
        /// Registers a webhook endpoint for event notifications.
        /// </summary>
        public string RegisterWebhook(string webhookUrl, string eventType, WebhookRetryPolicy retryPolicy = null)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Webhook URL and event type cannot be empty");

            lock (_lockObject)
            {
                var subscription = new WebhookSubscription
                {
                    Id = Guid.NewGuid().ToString("N"),
                    WebhookUrl = webhookUrl,
                    EventType = eventType,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    RetryPolicy = retryPolicy ?? new WebhookRetryPolicy()
                };

                _subscriptions.Add(subscription);

                _logger.LogInformation(
                    "Webhook registered - Id: {WebhookId}, Url: {Url}, EventType: {EventType}",
                    subscription.Id,
                    webhookUrl,
                    eventType);

                return subscription.Id;
            }
        }

        /// <summary>
        /// Unregisters a webhook by ID.
        /// </summary>
        public bool UnregisterWebhook(string webhookId)
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return false;

            lock (_lockObject)
            {
                var subscription = _subscriptions.Find(s => s.Id == webhookId);
                if (subscription != null)
                {
                    _subscriptions.Remove(subscription);
                    _logger.LogInformation("Webhook unregistered - Id: {WebhookId}", webhookId);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Dispatches an event to all registered webhooks.
        /// Handles retries and failure logging.
        /// </summary>
        public async Task DispatchEventAsync<T>(T @event) where T : ProcessingEvent
        {
            if (@event == null)
                return;

            List<WebhookSubscription> applicableSubscriptions;

            lock (_lockObject)
            {
                applicableSubscriptions = _subscriptions
                    .Where(s => s.IsActive && s.EventType == @event.EventType)
                    .ToList();
            }

            if (applicableSubscriptions.Count == 0)
            {
                _logger.LogDebug("No webhooks registered for event - Type: {EventType}", @event.EventType);
                return;
            }

            var payload = SerializeEvent(@event);

            foreach (var subscription in applicableSubscriptions)
            {
                _ = DeliverWebhookAsync(subscription, payload);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets all registered webhooks.
        /// </summary>
        public List<WebhookSubscription> GetWebhooks()
        {
            lock (_lockObject)
            {
                return new List<WebhookSubscription>(_subscriptions);
            }
        }

        /// <summary>
        /// Delivers a webhook payload with retry logic.
        /// </summary>
        private async Task DeliverWebhookAsync(WebhookSubscription subscription, string payload)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < subscription.RetryPolicy.MaxRetries)
            {
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, subscription.WebhookUrl))
                    {
                        request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                        request.Headers.Add("X-Webhook-Id", subscription.Id);
                        request.Headers.Add("X-Delivery-Attempt", (attempt + 1).ToString());

                        using (var response = await _httpClient.SendAsync(request))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                _logger.LogDebug(
                                    "Webhook delivered successfully - Id: {WebhookId}, Attempt: {Attempt}",
                                    subscription.Id,
                                    attempt + 1);
                                return;
                            }

                            lastException = new HttpRequestException(
                                $"Webhook returned {response.StatusCode}: {response.ReasonPhrase}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                attempt++;

                if (attempt < subscription.RetryPolicy.MaxRetries)
                {
                    int delayMs = (int)(subscription.RetryPolicy.InitialDelayMs * Math.Pow(subscription.RetryPolicy.BackoffMultiplier, attempt - 1));
                    await Task.Delay(Math.Min(delayMs, subscription.RetryPolicy.MaxDelayMs));
                }
            }

            // Log failure after all retries exhausted
            _logger.LogWarning(
                lastException,
                "Webhook delivery failed after {MaxRetries} attempts - Id: {WebhookId}, Url: {Url}",
                subscription.RetryPolicy.MaxRetries,
                subscription.Id,
                subscription.WebhookUrl);

            // Disable webhook after too many failures
            lock (_lockObject)
            {
                var sub = _subscriptions.FirstOrDefault(s => s.Id == subscription.Id);
                if (sub != null)
                {
                    sub.FailureCount++;
                    if (sub.FailureCount > 10)
                    {
                        sub.IsActive = false;
                        _logger.LogWarning(
                            "Webhook disabled due to repeated failures - Id: {WebhookId}",
                            subscription.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes an event to JSON for webhook payload.
        /// </summary>
        private string SerializeEvent<T>(T @event) where T : ProcessingEvent
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                return JsonSerializer.Serialize(@event, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serialize event for webhook");
                return "{}";
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Webhook subscription configuration.
    /// </summary>
    public class WebhookSubscription
    {
        public string Id { get; set; }
        public string WebhookUrl { get; set; }
        public string EventType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int FailureCount { get; set; }
        public WebhookRetryPolicy RetryPolicy { get; set; }
    }

    /// <summary>
    /// Retry configuration for webhook delivery.
    /// </summary>
    public class WebhookRetryPolicy
    {
        public int MaxRetries { get; set; } = 5;
        public int InitialDelayMs { get; set; } = 1000;
        public int MaxDelayMs { get; set; } = 30000;
        public double BackoffMultiplier { get; set; } = 2.0;
    }
}
