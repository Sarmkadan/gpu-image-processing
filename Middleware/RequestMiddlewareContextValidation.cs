#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides validation helpers for <see cref="RequestMiddlewareContext"/> instances.
    /// Validates business rules and constraints for middleware pipeline contexts.
    /// </summary>
    public static class RequestMiddlewareContextValidation
    {
        /// <summary>
        /// Validates the request middleware context and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The context to validate</param>
        /// <returns>List of validation problems (empty if valid)</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static IReadOnlyList<string> Validate(this RequestMiddlewareContext value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate RequestId
            if (string.IsNullOrWhiteSpace(value.RequestId))
            {
                errors.Add("RequestId cannot be null or whitespace");
            }
            else if (value.RequestId.Length > 100)
            {
                errors.Add("RequestId exceeds maximum length of 100 characters");
            }

            // Validate ApiKey
            if (string.IsNullOrWhiteSpace(value.ApiKey))
            {
                errors.Add("ApiKey cannot be null or whitespace");
            }
            else if (value.ApiKey.Length > 100)
            {
                errors.Add("ApiKey exceeds maximum length of 100 characters");
            }

            // Validate UserId
            if (string.IsNullOrWhiteSpace(value.UserId))
            {
                errors.Add("UserId cannot be null or whitespace");
            }
            else if (value.UserId.Length > 100)
            {
                errors.Add("UserId exceeds maximum length of 100 characters");
            }

            // Validate UserRole
            if (value.UserRole.HasValue && !Enum.IsDefined(typeof(UserRole), value.UserRole.Value))
            {
                errors.Add($"UserRole has invalid value: {value.UserRole}");
            }

            // Validate Scopes
            if (value.Scopes == null)
            {
                errors.Add("Scopes collection cannot be null");
            }
            else
            {
                if (value.Scopes.Count > 50)
                {
                    errors.Add("Scopes collection exceeds maximum size of 50 items");
                }

                foreach (var scope in value.Scopes)
                {
                    if (string.IsNullOrWhiteSpace(scope))
                    {
                        errors.Add("Scopes collection contains null or whitespace entries");
                        break;
                    }

                    if (scope.Length > 50)
                    {
                        errors.Add($"Scope '{scope}' exceeds maximum length of 50 characters");
                        break;
                    }
                }
            }

            // Validate Operation
            if (string.IsNullOrWhiteSpace(value.Operation))
            {
                errors.Add("Operation cannot be null or whitespace");
            }
            else if (value.Operation.Length > 100)
            {
                errors.Add("Operation exceeds maximum length of 100 characters");
            }
            else if (!value.Operation.StartsWith("get_") &&
                    !value.Operation.StartsWith("list_") &&
                    !value.Operation.StartsWith("create_") &&
                    !value.Operation.StartsWith("update_") &&
                    !value.Operation.StartsWith("delete_"))
            {
                errors.Add("Operation must start with a valid CRUD prefix (get_, list_, create_, update_, delete_)");
            }

            // Validate RequestData
            if (value.RequestData == null)
            {
                errors.Add("RequestData dictionary cannot be null");
            }
            else
            {
                if (value.RequestData.Count > 100)
                {
                    errors.Add("RequestData dictionary exceeds maximum size of 100 items");
                }

                foreach (var key in value.RequestData.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        errors.Add("RequestData dictionary contains null or whitespace keys");
                        break;
                    }

                    if (key.Length > 100)
                    {
                        errors.Add($"RequestData key '{key}' exceeds maximum length of 100 characters");
                        break;
                    }
                }
            }

            // Validate Metadata
            if (value.Metadata == null)
            {
                errors.Add("Metadata dictionary cannot be null");
            }
            else
            {
                if (value.Metadata.Count > 100)
                {
                    errors.Add("Metadata dictionary exceeds maximum size of 100 items");
                }

                foreach (var key in value.Metadata.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        errors.Add("Metadata dictionary contains null or whitespace keys");
                        break;
                    }

                    if (key.Length > 100)
                    {
                        errors.Add($"Metadata key '{key}' exceeds maximum length of 100 characters");
                        break;
                    }
                }
            }

            // Validate StartTime
            if (value.StartTime == default)
            {
                errors.Add("StartTime cannot be default DateTime");
            }
            else if (value.StartTime > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("StartTime cannot be in the future");
            }

            // Validate EndTime
            if (value.EndTime.HasValue)
            {
                if (value.EndTime.Value < value.StartTime)
                {
                    errors.Add("EndTime cannot be before StartTime");
                }

                if (value.EndTime.Value > DateTime.UtcNow.AddMinutes(5))
                {
                    errors.Add("EndTime cannot be in the future");
                }
            }

            // Validate ResponseStatusCode
            if (value.ResponseStatusCode.HasValue)
            {
                if (value.ResponseStatusCode.Value < 100 || value.ResponseStatusCode.Value > 599)
                {
                    errors.Add($"ResponseStatusCode {value.ResponseStatusCode} is out of valid HTTP status code range (100-599)");
                }
            }

            // Validate ErrorMessage
            if (!string.IsNullOrEmpty(value.ErrorMessage) && value.ErrorMessage.Length > 500)
            {
                errors.Add("ErrorMessage exceeds maximum length of 500 characters");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the request middleware context is valid.
        /// </summary>
        /// <param name="value">The context to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static bool IsValid(this RequestMiddlewareContext value)
        {
            return !value.Validate().Any();
        }

        /// <summary>
        /// Ensures the request middleware context is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The context to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        /// <exception cref="ArgumentException">Thrown if context is invalid, containing validation errors</exception>
        public static void EnsureValid(this RequestMiddlewareContext value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"RequestMiddlewareContext is invalid. Problems: {string.Join("; ", errors)}");
        }
    }
}