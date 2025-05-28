#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpuImageProcessing.Exceptions;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Connection pool manager for database connections.
    /// Manages connection lifecycle, pool statistics, and health checks.
    /// </summary>
    public class DatabaseConnectionPool
    {
        private readonly string _connectionString;
        private readonly Queue<DatabaseConnection> _availableConnections;
        private readonly HashSet<DatabaseConnection> _allConnections;
        private readonly int _minPoolSize;
        private readonly int _maxPoolSize;
        private readonly TimeSpan _connectionTimeout;
        private readonly object _lockObject = new();

        public event EventHandler<ConnectionPoolEventArgs> PoolEvent;

        public DatabaseConnectionPool(
            string connectionString,
            int minPoolSize = 5,
            int maxPoolSize = 20,
            int timeoutSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ValidationException(
                    "Connection string cannot be null or whitespace",
                    nameof(connectionString));
            }

            if (minPoolSize < 1)
            {
                throw new ValidationException(
                    "minPoolSize must be at least 1",
                    nameof(minPoolSize),
                    new Dictionary<string, string>
                    {
                        { nameof(minPoolSize), "Value must be >= 1" }
                    });
            }

            if (maxPoolSize < minPoolSize)
            {
                throw new ValidationException(
                    "maxPoolSize cannot be less than minPoolSize",
                    nameof(maxPoolSize),
                    new Dictionary<string, string>
                    {
                        { nameof(maxPoolSize), $"Value must be >= {minPoolSize}" }
                    });
            }

            if (timeoutSeconds < 1)
            {
                throw new ValidationException(
                    "timeoutSeconds must be at least 1",
                    nameof(timeoutSeconds),
                    new Dictionary<string, string>
                    {
                        { nameof(timeoutSeconds), "Value must be >= 1" }
                    });
            }

            _connectionString = connectionString;
            _minPoolSize = minPoolSize;
            _maxPoolSize = maxPoolSize;
            _connectionTimeout = TimeSpan.FromSeconds(timeoutSeconds);
            _availableConnections = new Queue<DatabaseConnection>();
            _allConnections = new HashSet<DatabaseConnection>();
        }

        /// <summary>
        /// Initializes the connection pool
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                for (int i = 0; i < _minPoolSize; i++)
                {
                    var connection = new DatabaseConnection
                    {
                        Id = Guid.NewGuid(),
                        ConnectionString = _connectionString,
                        CreatedAt = DateTime.UtcNow,
                        State = ConnectionState.Available
                    };

                    lock (_lockObject)
                    {
                        _availableConnections.Enqueue(connection);
                        _allConnections.Add(connection);
                    }
                }

                OnPoolEvent($"Pool initialized with {_minPoolSize} connections");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException(
                    "Failed to initialize database connection pool",
                    nameof(DatabaseConnectionPool),
                    ex.Message);
            }
        }

        /// <summary>
        /// Gets an available connection from the pool
        /// </summary>
        public async Task<DatabaseConnection> AcquireConnectionAsync()
        {
            var deadline = DateTime.UtcNow + _connectionTimeout;

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    lock (_lockObject)
                    {
                        // Try to reuse available connection
                        if (_availableConnections.TryDequeue(out var connection))
                        {
                            if (connection.State == ConnectionState.Available)
                            {
                                connection.State = ConnectionState.InUse;
                                connection.LastUsedAt = DateTime.UtcNow;
                                connection.UseCount++;
                                return connection;
                            }
                        }

                        // Try to create new connection if under max pool size
                        if (_allConnections.Count < _maxPoolSize)
                        {
                            var newConnection = new DatabaseConnection
                            {
                                Id = Guid.NewGuid(),
                                ConnectionString = _connectionString,
                                CreatedAt = DateTime.UtcNow,
                                State = ConnectionState.InUse
                            };

                            _allConnections.Add(newConnection);
                            OnPoolEvent($"Created new connection. Pool size: {_allConnections.Count}/{_maxPoolSize}");
                            return newConnection;
                        }
                    }

                    // Wait and retry
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    throw new DatabaseConnectionPoolException(
                        "Error acquiring database connection",
                        ex);
                }
            }

            throw new DatabaseConnectionPoolException(
                $"Could not acquire database connection within {_connectionTimeout.TotalSeconds:F1} seconds");
        }

        /// <summary>
        /// Returns a connection to the pool
        /// </summary>
        public async Task ReleaseConnectionAsync(DatabaseConnection connection)
        {
            if (connection == null)
            {
                throw new ValidationException("Connection cannot be null", nameof(connection));
            }

            try
            {
                lock (_lockObject)
                {
                    if (_allConnections.Contains(connection))
                    {
                        connection.State = ConnectionState.Available;
                        connection.LastReleasedAt = DateTime.UtcNow;
                        _availableConnections.Enqueue(connection);
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseConnectionPoolException(
                    "Error releasing database connection",
                    ex);
            }
        }

        /// <summary>
        /// Gets pool statistics
        /// </summary>
        public PoolStatistics GetStatistics()
        {
            try
            {
                lock (_lockObject)
                {
                    var available = _availableConnections.Count;
                    var inUse = _allConnections.Count(c => c.State == ConnectionState.InUse);
                    var idle = _allConnections.Count(c => c.State == ConnectionState.Idle);
                    var totalUsage = _allConnections.Sum(c => c.UseCount);

                    return new PoolStatistics
                    {
                        AvailableCount = available,
                        InUseCount = inUse,
                        IdleCount = idle,
                        TotalConnections = _allConnections.Count,
                        TotalRequests = totalUsage,
                        MinPoolSize = _minPoolSize,
                        MaxPoolSize = _maxPoolSize,
                        AvgConnectionAge = _allConnections.Any()
                            ? TimeSpan.FromMilliseconds(
                                _allConnections.Average(c => (DateTime.UtcNow - c.CreatedAt).TotalMilliseconds))
                            : TimeSpan.Zero
                    };
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseConnectionPoolException(
                    "Error getting pool statistics",
                    ex);
            }
        }

        /// <summary>
        /// Closes and returns a connection (removes from pool)
        /// </summary>
        public async Task CloseConnectionAsync(DatabaseConnection connection)
        {
            if (connection == null)
            {
                throw new ValidationException("Connection cannot be null", nameof(connection));
            }

            try
            {
                lock (_lockObject)
                {
                    _allConnections.Remove(connection);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseConnectionPoolException(
                    "Error closing database connection",
                    ex);
            }
        }

        /// <summary>
        /// Cleans up idle connections
        /// </summary>
        public async Task CleanupIdleConnectionsAsync(TimeSpan idleTimeout)
        {
            try
            {
                lock (_lockObject)
                {
                    var idleConnections = _allConnections
                        .Where(c => c.State == ConnectionState.Available &&
                                   (DateTime.UtcNow - c.LastReleasedAt) > idleTimeout)
                        .ToList();

                    foreach (var conn in idleConnections)
                    {
                        _allConnections.Remove(conn);
                    }

                    if (idleConnections.Count > 0)
                    {
                        OnPoolEvent($"Cleaned up {idleConnections.Count} idle connections");
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new DatabaseConnectionPoolException(
                    "Error cleaning up idle connections",
                    ex);
            }
        }

        private void OnPoolEvent(string message)
        {
            try
            {
                PoolEvent?.Invoke(this, new ConnectionPoolEventArgs
                {
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Don't throw from event handlers
                Console.Error.WriteLine($"Error in pool event handler: {ex.Message}");
            }
        }
    }

    public class DatabaseConnection
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public ConnectionState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public DateTime LastReleasedAt { get; set; }
        public long UseCount { get; set; }
    }

    public class PoolStatistics
    {
        public int AvailableCount { get; set; }
        public int InUseCount { get; set; }
        public int IdleCount { get; set; }
        public int TotalConnections { get; set; }
        public long TotalRequests { get; set; }
        public int MinPoolSize { get; set; }
        public int MaxPoolSize { get; set; }
        public TimeSpan AvgConnectionAge { get; set; }
    }

    public class ConnectionPoolEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ConnectionState
    {
        Available,
        InUse,
        Idle,
        Closed
    }

    /// <summary>
    /// Exception thrown when database connection pool operations fail.
    /// </summary>
    public class DatabaseConnectionPoolException : GpuImageProcessingException
    {
        public DatabaseConnectionPoolException(string message)
            : base(message)
        {
        }

        public DatabaseConnectionPoolException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
