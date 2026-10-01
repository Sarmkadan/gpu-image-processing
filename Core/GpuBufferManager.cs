#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Core
{
    /// <summary>
    /// Manages GPU memory buffers with pooling and automatic lifecycle tracking.
    /// Reduces allocation pressure by reusing buffers of matching or larger sizes.
    /// </summary>
    public class GpuBufferManager : IDisposable
    {
        private readonly ILogger<GpuBufferManager> _logger;
        private readonly ConcurrentDictionary<int, ConcurrentBag<byte[]>> _pool = new();
        private readonly long _maxPoolBytes;
        private long _currentPoolBytes;
        private long _totalAllocations;
        private long _poolHits;
        private bool _disposed;

        /// <summary>
        /// Total number of buffer allocations since creation.
        /// </summary>
        public long TotalAllocations => Interlocked.Read(ref _totalAllocations);

        /// <summary>
        /// Number of times a buffer was served from the pool instead of allocating.
        /// </summary>
        public long PoolHits => Interlocked.Read(ref _poolHits);

        /// <summary>
        /// Initializes the buffer manager with an optional maximum pool size.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="maxPoolBytes">Maximum total bytes to keep pooled (default 256 MB).</param>
        public GpuBufferManager(ILogger<GpuBufferManager> logger, long maxPoolBytes = 256 * 1024 * 1024)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxPoolBytes = maxPoolBytes;
            _logger.LogInformation("GpuBufferManager initialized, pool limit {Limit} MB", maxPoolBytes / (1024 * 1024));
        }

        /// <summary>
        /// Acquires a buffer of at least the requested size, from the pool if available.
        /// Buffers are bucketed by the next power-of-two size to reduce fragmentation.
        /// </summary>
        /// <param name="minimumSize">Minimum required buffer size in bytes.</param>
        /// <returns>A byte array of at least the requested size.</returns>
        public byte[] Acquire(int minimumSize)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (minimumSize <= 0) throw new ArgumentOutOfRangeException(nameof(minimumSize));

            int bucket = NextPowerOfTwo(minimumSize);
            Interlocked.Increment(ref _totalAllocations);

            if (_pool.TryGetValue(bucket, out var bag) && bag.TryTake(out var buffer))
            {
                Interlocked.Add(ref _currentPoolBytes, -bucket);
                Interlocked.Increment(ref _poolHits);
                _logger.LogDebug("Pool hit: {Bucket} byte buffer reused", bucket);
                return buffer;
            }

            _logger.LogDebug("Pool miss: allocating {Bucket} byte buffer", bucket);
            return new byte[bucket];
        }

        /// <summary>
        /// Returns a buffer to the pool for future reuse.
        /// Buffers exceeding the pool size limit are discarded.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        public void Release(byte[] buffer)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (buffer == null) return;

            int bucket = buffer.Length;
            long currentTotal = Interlocked.Read(ref _currentPoolBytes);

            if (currentTotal + bucket > _maxPoolBytes)
            {
                _logger.LogDebug("Pool full, discarding {Len} byte buffer", bucket);
                return;
            }

            var bag = _pool.GetOrAdd(bucket, _ => new ConcurrentBag<byte[]>());
            Array.Clear(buffer, 0, buffer.Length);
            bag.Add(buffer);
            Interlocked.Add(ref _currentPoolBytes, bucket);
        }

        /// <summary>
        /// Clears all pooled buffers and resets allocation counters.
        /// </summary>
        public void Flush()
        {
            _pool.Clear();
            Interlocked.Exchange(ref _currentPoolBytes, 0);
            _logger.LogInformation("Buffer pool flushed");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Flush();
            _logger.LogInformation("GpuBufferManager disposed. Total allocs: {A}, pool hits: {H}",
                TotalAllocations, PoolHits);
        }

        private static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            return v + 1;
        }
    }
}
