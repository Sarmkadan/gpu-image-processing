#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this RequestMiddlewareContext value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate RequestId using pattern matching and expression-bodied members
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

            // Validate UserRole using pattern matching
            if (value.UserRole.HasValue && !Enum.IsDefined(typeof(UserRole), value.UserRole.Value))
            {
                errors.Add($"UserRole has invalid value: {value.UserRole}");
            }

            // Validate Scopes - extract to local function for better readability
            ValidateCollection(
                value.Scopes,
                "Scopes collection",
                maxCount: 50,
                maxLength: 50,
                itemValidator: scope => string.IsNullOrWhiteSpace(scope) || scope.Length > 50,
                errors: errors);

            // Validate Operation using pattern matching
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

            // Validate RequestData - extract to local function
            ValidateDictionary(
                value.RequestData,
                "RequestData dictionary",
                maxCount: 100,
                maxKeyLength: 100,
                errors: errors);

            // Validate Metadata - extract to local function
            ValidateDictionary(
                value.Metadata,
                "Metadata dictionary",
                maxCount: 100,
                maxKeyLength: 100,
                errors: errors);

            // Validate StartTime using pattern matching
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

            // Validate ResponseStatusCode using pattern matching
            if (value.ResponseStatusCode is { } statusCode && (statusCode is < 100 or > 599))
            {
                errors.Add($"ResponseStatusCode {statusCode} is out of valid HTTP status code range (100-599)");
            }

            // Validate ErrorMessage
            if (!string.IsNullOrEmpty(value.ErrorMessage) && value.ErrorMessage.Length > 500)
            {
                errors.Add("ErrorMessage exceeds maximum length of 500 characters");
            }

            return errors.AsReadOnly();

            // Local validation functions for better code organization and reuse
            static void ValidateCollection(
                IReadOnlyCollection<string>? collection,
                string collectionName,
                int maxCount,
                int maxLength,
                Func<string, bool> itemValidator,
                ICollection<string> errors)
            {
                if (collection == null)
                {
                    errors.Add($"{collectionName} cannot be null");
                    return;
                }

                if (collection.Count > maxCount)
                {
                    errors.Add($"{collectionName} exceeds maximum size of {maxCount} items");
                }

                foreach (var item in collection)
                {
                    if (itemValidator(item))
                    {
                        errors.Add($"{collectionName} contains invalid entries");
                        break;
                    }

                    if (item.Length > maxLength)
                    {
                        errors.Add($"{collectionName} item '{item}' exceeds maximum length of {maxLength} characters");
                        break;
                    }
                }
            }

            static void ValidateDictionary(
                IReadOnlyDictionary<string, object>? dictionary,
                string dictionaryName,
                int maxCount,
                int maxKeyLength,
                ICollection<string> errors)
            {
                if (dictionary == null)
                {
                    errors.Add($"{dictionaryName} cannot be null");
                    return;
                }

                if (dictionary.Count > maxCount)
                {
                    errors.Add($"{dictionaryName} exceeds maximum size of {maxCount} items");
                }

                foreach (var key in dictionary.Keys)
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        errors.Add($"{dictionaryName} contains null or whitespace keys");
                        break;
                    }

                    if (key.Length > maxKeyLength)
                    {
                        errors.Add($"{dictionaryName} key '{key}' exceeds maximum length of {maxKeyLength} characters");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the request middleware context is valid.
        /// </summary>
        /// <param name="value">The context to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static bool IsValid(this RequestMiddlewareContext value)
        {
            return !value.Validate().Any();
        }

        /// <summary>
        /// Ensures the request middleware context is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The context to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
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