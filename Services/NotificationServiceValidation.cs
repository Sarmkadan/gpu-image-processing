using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Services;

/// <summary>
/// Provides validation helpers for <see cref="Notification"/> instances.
/// </summary>
public static class NotificationServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="Notification"/> instance.
    /// </summary>
    /// <param name="value">The notification instance to validate.</param>
    /// <returns>An immutable list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Notification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Id cannot be null or whitespace.");
        }
        else if (value.Id.Length > 256)
        {
            problems.Add("Id exceeds maximum length of 256 characters.");
        }

        // Validate Type
        if (!Enum.IsDefined(value.Type))
        {
            problems.Add($"Type '{value.Type}' is not a valid NotificationType value.");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            problems.Add("Title cannot be null or whitespace.");
        }
        else if (value.Title.Length > 512)
        {
            problems.Add("Title exceeds maximum length of 512 characters.");
        }

        // Validate Message
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be null or whitespace.");
        }
        else if (value.Message.Length > 4096)
        {
            problems.Add("Message exceeds maximum length of 4096 characters.");
        }

        // Validate Severity
        if (!Enum.IsDefined(value.Severity))
        {
            problems.Add($"Severity '{value.Severity}' is not a valid NotificationSeverity value.");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            problems.Add("Timestamp cannot be the default DateTime value.");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Timestamp cannot be in the future.");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("Timestamp cannot be more than one year in the past.");
        }

        // Validate Details
        if (value.Details is null)
        {
            problems.Add("Details dictionary cannot be null.");
        }
        else
        {
            if (value.Details.Count > 100)
            {
                problems.Add("Details dictionary exceeds maximum size of 100 entries.");
            }

            foreach (var kvp in value.Details)
            {
                if (kvp.Key is null)
                {
                    problems.Add("Details dictionary contains a null key.");
                    break;
                }

                if (kvp.Key.Length > 256)
                {
                    problems.Add("Details dictionary key exceeds maximum length of 256 characters.");
                    break;
                }

                if (kvp.Value is null)
                {
                    problems.Add($"Details dictionary contains a null value for key '{kvp.Key}'.");
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="Notification"/> instance is valid.
    /// </summary>
    /// <param name="value">The notification instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Notification value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="Notification"/> instance is valid.
    /// </summary>
    /// <param name="value">The notification instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing all validation problems.</exception>
    public static void EnsureValid(this Notification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Notification is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}