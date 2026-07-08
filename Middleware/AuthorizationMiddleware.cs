#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
                Key = key,
                UserId = userId,
                Scopes = new List<string>(scopes),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        public void RegisterUser(string userId, UserRole role)
        {
            _userRoles[userId] = role;
        }

        public async Task<RequestMiddlewareResult> ProcessAsync(RequestMiddlewareContext context)
        {
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
            var apiKey = _validKeys.FirstOrDefault(k => k.Key == key);
            if (apiKey != null)
                apiKey.IsActive = false;
        }

        public List<ApiKeyInfo> ListActiveKeys(string userId = null)
        {
            return _validKeys
                .Where(k => k.IsActive && (userId == null || k.UserId == userId))
                .Select(k => new ApiKeyInfo
                {
                    UserId = k.UserId,
                    CreatedAt = k.CreatedAt,
                    Scopes = new List<string>(k.Scopes),
                    KeyPreview = k.Key.Substring(0, 8) + "..."
                })
                .ToList();
        }

        private class ApiKey
        {
            public string Key { get; set; }
            public string UserId { get; set; }
            public List<string> Scopes { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }
        }
    }

    public class ApiKeyInfo
    {
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Scopes { get; set; }
        public string KeyPreview { get; set; }
    }

    public enum UserRole
    {
        Viewer,
        Editor,
        Admin
    }
}
