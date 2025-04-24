// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Events;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Service for managing user notifications about processing events.
    /// Supports multiple notification channels: email, webhook, in-app.
    /// </summary>
    public class NotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly List<INotificationChannel> _channels;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _channels = new List<INotificationChannel>();
        }

        /// <summary>
        /// Registers a notification channel.
        /// </summary>
        public void RegisterChannel(INotificationChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            _channels.Add(channel);
            _logger.LogInformation("Notification channel registered - Type: {ChannelType}", channel.GetType().Name);
        }

        /// <summary>
        /// Sends a notification about processing completion.
        /// </summary>
        public async Task NotifyProcessingCompletedAsync(string jobId, int processedCount, bool success)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = NotificationType.ProcessingCompleted,
                Title = success ? "Processing Completed Successfully" : "Processing Completed with Errors",
                Message = $"Job {jobId}: {processedCount} images processed",
                Severity = success ? NotificationSeverity.Info : NotificationSeverity.Warning,
                Timestamp = DateTime.UtcNow
            };

            await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Sends a notification about processing failure.
        /// </summary>
        public async Task NotifyProcessingFailedAsync(string jobId, string errorMessage)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = NotificationType.ProcessingFailed,
                Title = "Processing Failed",
                Message = $"Job {jobId} failed: {errorMessage}",
                Severity = NotificationSeverity.Error,
                Timestamp = DateTime.UtcNow
            };

            await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Sends a notification about resource limit reached.
        /// </summary>
        public async Task NotifyResourceLimitAsync(string resourceType, double utilization)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = NotificationType.ResourceAlert,
                Title = "Resource Limit Alert",
                Message = $"{resourceType} utilization at {utilization:F1}%",
                Severity = NotificationSeverity.Warning,
                Timestamp = DateTime.UtcNow
            };

            await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Sends a notification about system health status.
        /// </summary>
        public async Task NotifyHealthStatusAsync(bool isHealthy, List<string> issues)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = NotificationType.HealthCheck,
                Title = isHealthy ? "System Healthy" : "System Health Degraded",
                Message = isHealthy ? "All systems operational" : $"{issues.Count} issues detected",
                Severity = isHealthy ? NotificationSeverity.Info : NotificationSeverity.Warning,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "isHealthy", isHealthy },
                    { "issues", issues }
                }
            };

            await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Sends a custom notification.
        /// </summary>
        public async Task NotifyAsync(string title, string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = NotificationType.Custom,
                Title = title,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            };

            await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Sends notification through all registered channels.
        /// </summary>
        private async Task SendNotificationAsync(Notification notification)
        {
            if (_channels.Count == 0)
            {
                _logger.LogDebug("No notification channels registered");
                return;
            }

            _logger.LogDebug(
                "Sending notification - Id: {NotificationId}, Title: {Title}, Severity: {Severity}",
                notification.Id,
                notification.Title,
                notification.Severity);

            var tasks = new List<Task>();

            foreach (var channel in _channels)
            {
                tasks.Add(SendThroughChannelAsync(channel, notification));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Sends notification through a specific channel with error handling.
        /// </summary>
        private async Task SendThroughChannelAsync(INotificationChannel channel, Notification notification)
        {
            try
            {
                await channel.SendAsync(notification);

                _logger.LogDebug(
                    "Notification sent through {ChannelType} - NotificationId: {NotificationId}",
                    channel.GetType().Name,
                    notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send notification through {ChannelType} - NotificationId: {NotificationId}",
                    channel.GetType().Name,
                    notification.Id);
            }
        }
    }

    /// <summary>
    /// Represents a notification message.
    /// </summary>
    public class Notification
    {
        public string Id { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }

    /// <summary>
    /// Notification severity levels.
    /// </summary>
    public enum NotificationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }

    /// <summary>
    /// Notification types.
    /// </summary>
    public enum NotificationType
    {
        ProcessingCompleted,
        ProcessingFailed,
        ResourceAlert,
        HealthCheck,
        Custom
    }

    /// <summary>
    /// Interface for notification delivery channels.
    /// </summary>
    public interface INotificationChannel
    {
        Task SendAsync(Notification notification);
    }

    /// <summary>
    /// Console-based notification channel for testing.
    /// </summary>
    public class ConsoleNotificationChannel : INotificationChannel
    {
        private readonly ILogger<ConsoleNotificationChannel> _logger;

        public ConsoleNotificationChannel(ILogger<ConsoleNotificationChannel> logger)
        {
            _logger = logger;
        }

        public async Task SendAsync(Notification notification)
        {
            _logger.LogInformation(
                "[{Severity}] {Title}: {Message}",
                notification.Severity,
                notification.Title,
                notification.Message);

            await Task.CompletedTask;
        }
    }
}
