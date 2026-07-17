#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Authorization middleware for validating API keys and user permissions.
    /// Supports role-based access control (RBAC) and scope-based restrictions.
    /// </summary>
    public class AuthorizationMiddleware : IRequestMiddleware
    {
        private readonly List<ApiKey> _validKeys;
        private readonly Dictionary<string, UserRole> _userRoles;

        public AuthorizationMiddleware()
        {
            _validKeys = new List<ApiKey>();
            _userRoles = new Dictionary<string, UserRole>();
        }

        public void RegisterApiKey(string key, string userId, params string[] scopes)
        {
            _validKeys.Add(new ApiKey
            {
                Key = key ?? throw new ArgumentNullException(nameof(key)),
                UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
                Scopes = new List<string>(scopes ?? Array.Empty<string>()),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        public void RegisterUser(string userId, UserRole role)
        {
            ArgumentNullException.ThrowIfNull(userId);
            _userRoles[userId] = role;
        }

        public async Task<RequestMiddlewareResult> ProcessAsync(RequestMiddlewareContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Validate API key if provided
            if (!string.IsNullOrEmpty(context.ApiKey))
            {
                var apiKey = _validKeys.FirstOrDefault(k => k.Key == context.ApiKey && k.IsActive);
                if (apiKey == null)
                {
                    return RequestMiddlewareResult.Failure("Invalid or inactive API key");
                }

                context.UserId = apiKey.UserId;

                // Check scope
                if (!context.Scopes.All(s => apiKey.Scopes.Contains(s)))
                {
                    return RequestMiddlewareResult.Failure("Insufficient scope for requested operation");
                }
            }

            // Validate user role
            if (!string.IsNullOrEmpty(context.UserId) && _userRoles.TryGetValue(context.UserId, out var role))
            {
                context.UserRole = role;

                // Check if role has required permissions
                if (!CanPerformOperation(role, context.Operation))
                {
                    return RequestMiddlewareResult.Failure($"Role {role} is not authorized for {context.Operation}");
                }
            }

            return await Task.FromResult(RequestMiddlewareResult.Success());
        }

        public int Order => 10;

        private bool CanPerformOperation(UserRole role, string operation)
        {
            return role switch
            {
                UserRole.Admin => true,
                UserRole.Editor => !operation.StartsWith("delete_"),
                UserRole.Viewer => operation.StartsWith("get_") || operation.StartsWith("list_"),
                _ => false
            };
        }

        public void RevokeApiKey(string key)
        {
            ArgumentNullException.ThrowIfNull(key);
            var apiKey = _validKeys.FirstOrDefault(k => k.Key == key);
            if (apiKey != null)
            {
                apiKey.IsActive = false;
            }
        }

        /// <summary>
        /// Lists all active API keys, optionally filtered by user.
        /// </summary>
        /// <param name="userId">Optional user ID to filter keys by. If null, returns all active keys.</param>
        /// <returns>A list of <see cref="ApiKeyInfo"/> objects representing active API keys.</returns>
        public List<ApiKeyInfo> ListActiveKeys(string? userId = null)
        {
            return _validKeys
                .Where(k => k.IsActive && (userId == null || k.UserId == userId))
                .Select(k => new ApiKeyInfo
                {
                    UserId = k.UserId,
                    CreatedAt = k.CreatedAt,
                    Scopes = k.Scopes?.ToList() ?? new List<string>(),
                    KeyPreview = k.Key?.Length > 8 ? k.Key.Substring(0, 8) + "..." : k.Key
                })
                .ToList();
        }

        private sealed class ApiKey
        {
            public string Key { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public List<string> Scopes { get; set; } = new();
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }
        }
    }

    /// <summary>
    /// Contains information about an API key without exposing the full key value.
    /// </summary>
    public sealed class ApiKeyInfo
    {
        /// <summary>Gets the user identifier associated with this API key.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>Gets the creation timestamp of this API key.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets the list of scopes granted to this API key.</summary>
        public List<string> Scopes { get; set; } = new();

        /// <summary>Gets a preview of the API key (first 8 characters followed by ellipsis).</summary>
        public string KeyPreview { get; set; } = string.Empty;
    }

    public enum UserRole
    {
        Viewer,
        Editor,
        Admin
    }
}