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
                    $"DatabaseConnectionPool is not valid. Validation errors:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", errors)}",
                    nameof(value));
            }
        }

        /// <summary>
        /// Gets the connection string from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The connection string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection string field cannot be retrieved or is null.</exception>
        private static string GetConnectionString(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return _connectionStringField.GetValue(pool) as string
                ?? throw new InvalidOperationException("ConnectionString field cannot be null.");
        }

        /// <summary>
        /// Gets the minimum pool size from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The minimum pool size.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the minimum pool size field cannot be retrieved or is null.</exception>
        private static int GetMinPoolSize(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_minPoolSizeField.GetValue(pool)
                ?? throw new InvalidOperationException("MinPoolSize field cannot be null."));
        }

        /// <summary>
        /// Gets the maximum pool size from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The maximum pool size.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the maximum pool size field cannot be retrieved or is null.</exception>
        private static int GetMaxPoolSize(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_maxPoolSizeField.GetValue(pool)
                ?? throw new InvalidOperationException("MaxPoolSize field cannot be null."));
        }

        /// <summary>
        /// Gets the connection timeout from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The connection timeout.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection timeout field cannot be retrieved or is null.</exception>
        private static TimeSpan GetConnectionTimeout(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (TimeSpan)(_connectionTimeoutField.GetValue(pool)
                ?? throw new InvalidOperationException("ConnectionTimeout field cannot be null."));
        }

        /// <summary>
        /// Gets the available connection count from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The available connection count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the available connections property cannot be retrieved or is null.</exception>
        private static int GetAvailableCount(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_availableConnectionsProperty.GetValue(pool)
                ?? throw new InvalidOperationException("AvailableCount property cannot be null."));
        }

        /// <summary>
        /// Gets the in-use connection count from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The in-use connection count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the in-use count property cannot be retrieved or is null.</exception>
        private static int GetInUseCount(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_inUseCountProperty.GetValue(pool)
                ?? throw new InvalidOperationException("InUseCount property cannot be null."));
        }

        /// <summary>
        /// Gets the idle connection count from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The idle connection count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the idle count property cannot be retrieved or is null.</exception>
        private static int GetIdleCount(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_idleCountProperty.GetValue(pool)
                ?? throw new InvalidOperationException("IdleCount property cannot be null."));
        }

        /// <summary>
        /// Gets the total connection count from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The total connection count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the total connections property cannot be retrieved or is null.</exception>
        private static int GetTotalConnections(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_totalConnectionsProperty.GetValue(pool)
                ?? throw new InvalidOperationException("TotalConnections property cannot be null."));
        }

        /// <summary>
        /// Gets the total request count from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The total request count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the total requests property cannot be retrieved or is null.</exception>
        private static long GetTotalRequests(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (long)(_totalRequestsProperty.GetValue(pool)
                ?? throw new InvalidOperationException("TotalRequests property cannot be null."));
        }

        /// <summary>
        /// Gets the minimum pool size from pool statistics from the specified database connection pool.
        /// </summary>
        /// <param name="pool">The database connection pool.</param>
        /// <returns>The minimum pool size from statistics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pool"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the minimum pool size property cannot be retrieved or is null.</exception>
        private static int GetPoolMinPoolSize(DatabaseConnectionPool pool)
        {
            ArgumentNullException.ThrowIfNull(pool);
            return (int)(_minPoolSizeProperty.GetValue(pool)
                ?? throw new InvalidOperationException("MinPoolSize property cannot be null."));
        }
    }
}