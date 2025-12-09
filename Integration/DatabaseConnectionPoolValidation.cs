#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides validation helpers for <see cref="DatabaseConnectionPool"/> instances.
    /// </summary>
    public static class DatabaseConnectionPoolValidation
    {
        private static readonly FieldInfo _connectionStringField = typeof(DatabaseConnectionPool)
            .GetField("_connectionString", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _connectionString field in DatabaseConnectionPool");

        private static readonly FieldInfo _minPoolSizeField = typeof(DatabaseConnectionPool)
            .GetField("_minPoolSize", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _minPoolSize field in DatabaseConnectionPool");

        private static readonly FieldInfo _maxPoolSizeField = typeof(DatabaseConnectionPool)
            .GetField("_maxPoolSize", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _maxPoolSize field in DatabaseConnectionPool");

        private static readonly FieldInfo _connectionTimeoutField = typeof(DatabaseConnectionPool)
            .GetField("_connectionTimeout", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find _connectionTimeout field in DatabaseConnectionPool");

        private static readonly PropertyInfo _availableConnectionsProperty = typeof(DatabaseConnectionPool)
            .GetProperty("AvailableCount", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find AvailableCount property in DatabaseConnectionPool");

        private static readonly PropertyInfo _inUseCountProperty = typeof(DatabaseConnectionPool)
            .GetProperty("InUseCount", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find InUseCount property in DatabaseConnectionPool");

        private static readonly PropertyInfo _idleCountProperty = typeof(DatabaseConnectionPool)
            .GetProperty("IdleCount", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find IdleCount property in DatabaseConnectionPool");

        private static readonly PropertyInfo _totalConnectionsProperty = typeof(DatabaseConnectionPool)
            .GetProperty("TotalConnections", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find TotalConnections property in DatabaseConnectionPool");

        private static readonly PropertyInfo _totalRequestsProperty = typeof(DatabaseConnectionPool)
            .GetProperty("TotalRequests", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find TotalRequests property in DatabaseConnectionPool");

        private static readonly PropertyInfo _minPoolSizeProperty = typeof(DatabaseConnectionPool)
            .GetProperty("MinPoolSize", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Could not find MinPoolSize property in DatabaseConnectionPool");

        /// <summary>
        /// Validates the specified database connection pool.
        /// </summary>
        /// <param name="value">The database connection pool to validate.</param>
        /// <returns>A list of validation errors; empty if the pool is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this DatabaseConnectionPool value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate connection string
            var connectionString = GetConnectionString(value);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                errors.Add("ConnectionString cannot be null or whitespace.");
            }
            else if (connectionString.Length > 1024)
            {
                errors.Add("ConnectionString exceeds maximum length of 1024 characters.");
            }

            // Validate min pool size
            var minPoolSize = GetMinPoolSize(value);
            if (minPoolSize < 1)
            {
                errors.Add("MinPoolSize must be at least 1.");
            }

            // Validate max pool size
            var maxPoolSize = GetMaxPoolSize(value);
            if (maxPoolSize < minPoolSize)
            {
                errors.Add($"MaxPoolSize ({maxPoolSize}) cannot be less than MinPoolSize ({minPoolSize}).");
            }

            // Validate connection timeout
            var connectionTimeout = GetConnectionTimeout(value);
            if (connectionTimeout.TotalSeconds < 1)
            {
                errors.Add("ConnectionTimeout must be at least 1 second.");
            }

            // Validate pool statistics properties
            var availableCount = GetAvailableCount(value);
            if (availableCount < 0)
            {
                errors.Add("AvailableCount cannot be negative.");
            }

            var inUseCount = GetInUseCount(value);
            if (inUseCount < 0)
            {
                errors.Add("InUseCount cannot be negative.");
            }

            var idleCount = GetIdleCount(value);
            if (idleCount < 0)
            {
                errors.Add("IdleCount cannot be negative.");
            }

            var totalConnections = GetTotalConnections(value);
            if (totalConnections < 0)
            {
                errors.Add("TotalConnections cannot be negative.");
            }

            var totalRequests = GetTotalRequests(value);
            if (totalRequests < 0)
            {
                errors.Add("TotalRequests cannot be negative.");
            }

            var poolMinPoolSize = GetPoolMinPoolSize(value);
            if (poolMinPoolSize < 1)
            {
                errors.Add("MinPoolSize (from pool stats) must be at least 1.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified database connection pool is valid.
        /// </summary>
        /// <param name="value">The database connection pool to check.</param>
        /// <returns><see langword="true"/> if the pool is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this DatabaseConnectionPool value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified database connection pool is valid.
        /// </summary>
        /// <param name="value">The database connection pool to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the pool is not valid, containing the validation errors.</exception>
        public static void EnsureValid(this DatabaseConnectionPool value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"DatabaseConnectionPool is not valid. Validation errors:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", errors)}",
                    nameof(value));
            }
        }

        private static string GetConnectionString(DatabaseConnectionPool pool)
        {
            return _connectionStringField.GetValue(pool) as string;
        }

        private static int GetMinPoolSize(DatabaseConnectionPool pool)
        {
            return (int)_minPoolSizeField.GetValue(pool)!;
        }

        private static int GetMaxPoolSize(DatabaseConnectionPool pool)
        {
            return (int)_maxPoolSizeField.GetValue(pool)!;
        }

        private static TimeSpan GetConnectionTimeout(DatabaseConnectionPool pool)
        {
            return (TimeSpan)_connectionTimeoutField.GetValue(pool)!;
        }

        private static int GetAvailableCount(DatabaseConnectionPool pool)
        {
            return (int)_availableConnectionsProperty.GetValue(pool)!;
        }

        private static int GetInUseCount(DatabaseConnectionPool pool)
        {
            return (int)_inUseCountProperty.GetValue(pool)!;
        }

        private static int GetIdleCount(DatabaseConnectionPool pool)
        {
            return (int)_idleCountProperty.GetValue(pool)!;
        }

        private static int GetTotalConnections(DatabaseConnectionPool pool)
        {
            return (int)_totalConnectionsProperty.GetValue(pool)!;
        }

        private static long GetTotalRequests(DatabaseConnectionPool pool)
        {
            return (long)_totalRequestsProperty.GetValue(pool)!;
        }

        private static int GetPoolMinPoolSize(DatabaseConnectionPool pool)
        {
            return (int)_minPoolSizeProperty.GetValue(pool)!;
        }
    }
}
