# DatabaseConnectionPool

A connection pool manager for database connections that provides efficient reuse of connections, automatic cleanup of idle connections, and statistics tracking. It is designed for high-throughput scenarios where frequent database access is required, such as in GPU-accelerated image processing pipelines that need to persist intermediate results or metadata.

## API

### `DatabaseConnectionPool`

Initializes a new instance of the `DatabaseConnectionPool` with the specified connection string and optional minimum pool size.

- **Parameters**
  - `connectionString` (string): The connection string used to establish new database connections.
  - `minPoolSize` (int, optional): The minimum number of connections to maintain in the pool. Defaults to 2.

### `async Task InitializeAsync()`

Asynchronously initializes the connection pool by creating the minimum number of connections specified during construction. This method must be called before any connection is acquired.

- **Returns**
  - `Task`: A task representing the asynchronous initialization operation.

- **Exceptions**
  - `InvalidOperationException`: Thrown if the pool has already been initialized.
  - `DbException`: Thrown if connection creation fails.

### `async Task<DatabaseConnection> AcquireConnectionAsync()`

Asynchronously acquires a database connection from the pool. If no idle connection is available, a new one may be created up to the pool's capacity.

- **Returns**
  - `Task<DatabaseConnection>`: A task that resolves to a `DatabaseConnection` instance ready for use.

- **Exceptions**
  - `InvalidOperationException`: Thrown if the pool has not been initialized.
  - `ObjectDisposedException`: Thrown if the pool has been disposed or closed.

### `async Task ReleaseConnectionAsync(DatabaseConnection connection)`

Asynchronously releases a previously acquired database connection back to the pool, making it available for reuse.

- **Parameters**
  - `connection` (`DatabaseConnection`): The connection to release.

- **Exceptions**
  - `ArgumentNullException`: Thrown if `connection` is `null`.
  - `InvalidOperationException`: Thrown if the connection was not acquired from this pool or if the pool is not initialized.
  - `ObjectDisposedException`: Thrown if the pool has been disposed or closed.

### `PoolStatistics GetStatistics()`

Retrieves a snapshot of the current statistics of the connection pool.

- **Returns**
  - `PoolStatistics`: A structure containing metrics such as total connections, in-use count, idle count, and total requests.

### `async Task CloseConnectionAsync(DatabaseConnection connection)`

Asynchronously closes a specific connection and removes it from the pool. This should be used for connections that are no longer valid or when the pool is being shut down.

- **Parameters**
  - `connection` (`DatabaseConnection`): The connection to close.

- **Exceptions**
  - `ArgumentNullException`: Thrown if `connection` is `null`.
  - `InvalidOperationException`: Thrown if the connection was not acquired from this pool or if the pool is not initialized.
  - `ObjectDisposedException`: Thrown if the pool has been disposed or closed.

### `async Task CleanupIdleConnectionsAsync()`

Asynchronously removes idle connections from the pool that have been unused for a configurable period. This helps manage resource usage and prevents pool bloat.

- **Returns**
  - `Task`: A task representing the cleanup operation.

- **Exceptions**
  - `InvalidOperationException`: Thrown if the pool has not been initialized.
  - `ObjectDisposedException`: Thrown if the pool has been disposed or closed.

### `Guid Id`

Gets the unique identifier of the connection pool instance.

- **Type**: `Guid`
- **Access**: Read-only

### `string ConnectionString`

Gets the connection string used by the pool to create new connections.

- **Type**: `string`
- **Access**: Read-only

### `ConnectionState State`

Gets the current state of the connection pool (e.g., Open, Closed, Connecting).

- **Type**: `ConnectionState`
- **Access**: Read-only

### `DateTime CreatedAt`

Gets the timestamp when the pool was created.

- **Type**: `DateTime`
- **Access**: Read-only

### `DateTime LastUsedAt`

Gets the timestamp when the pool was last used to acquire or release a connection.

- **Type**: `DateTime`
- **Access**: Read-only

### `DateTime LastReleasedAt`

Gets the timestamp when a connection was last released back to the pool.

- **Type**: `DateTime`
- **Access**: Read-only

### `long UseCount`

Gets the total number of times connections have been acquired from the pool.

- **Type**: `long`
- **Access**: Read-only

### `int AvailableCount`

Gets the current number of idle connections available for reuse.

- **Type**: `int`
- **Access**: Read-only

### `int InUseCount`

Gets the current number of connections that have been acquired and are in use.

- **Type**: `int`
- **Access**: Read-only

### `int IdleCount`

Gets the current number of idle connections in the pool.

- **Type**: `int`
- **Access**: Read-only

### `int TotalConnections`

Gets the total number of connections currently managed by the pool, including both idle and in-use connections.

- **Type**: `int`
- **Access**: Read-only

### `long TotalRequests`

Gets the total number of connection acquisition requests made to the pool.

- **Type**: `long`
- **Access**: Read-only

### `int MinPoolSize`

Gets the minimum number of connections maintained in the pool.

- **Type**: `int`
- **Access**: Read-only

## Usage

### Example 1: Basic Usage with Initialization and Cleanup
