#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Middleware for enforcing rate limiting on image processing operations.
    /// Prevents resource exhaustion and maintains system stability under load.
    /// Implements token bucket algorithm for fair rate limiting.
    /// </summary>
    public class RateLimitingMiddleware : IProcessingMiddleware
    {
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitConfig _config;
        private readonly TokenBucket _tokenBucket;
        private readonly SemaphoreSlim _semaphore;

        public RateLimitingMiddleware(ILogger<RateLimitingMiddleware> logger, RateLimitConfig config = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? new RateLimitConfig();
            _tokenBucket = new TokenBucket(_config.MaxTokens, _config.RefillRate);
            _semaphore = new SemaphoreSlim(_config.MaxConcurrentOperations);
        }

        /// <summary>
        /// Gets middleware name for pipeline identification.
        /// </summary>
        public string GetName()
        {
            return "RateLimitingMiddleware";
        }

        /// <summary>
        /// Gets execution order priority (lower executes first).
        /// Rate limiting executes early to gate all operations.
        /// </summary>
        public int GetPriority()
        {
            return 10;
        }

        /// <summary>
        /// Executes middleware with rate limiting and concurrency control.
        /// Acquires token from bucket and semaphore before proceeding.
        /// </summary>
        public async Task<MiddlewareResult> ExecuteAsync(MiddlewareContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Check concurrency limit
            if (!_semaphore.Wait(0))
            {
                _logger.LogWarning(
                    "Rate limit exceeded - Max concurrent operations ({MaxConcurrent}) reached",
                    _config.MaxConcurrentOperations);

                return MiddlewareResult.Failure(
                    $"Rate limit exceeded - Max {_config.MaxConcurrentOperations} concurrent operations allowed");
            }

            try
            {
                // Check token availability
                if (!_tokenBucket.TryConsume(_config.TokenCost))
                {
                    _logger.LogWarning(
                        "Rate limit exceeded - Insufficient tokens. Available: {AvailableTokens}/{MaxTokens}",
                        _tokenBucket.AvailableTokens,
                        _tokenBucket.Capacity);

                    return MiddlewareResult.Failure(
                        $"Rate limit exceeded - Please retry in {_config.RetryAfterSeconds} seconds");
                }

                _logger.LogDebug(
                    "Rate limit check passed - Tokens remaining: {AvailableTokens}",
                    _tokenBucket.AvailableTokens);

                context.SetState("rate_limit_tokens", _tokenBucket.AvailableTokens);

                return await context.Next();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets current rate limiting status including available tokens and capacity.
        /// </summary>
        public RateLimitStatus GetStatus()
        {
            return new RateLimitStatus
            {
                AvailableTokens = _tokenBucket.AvailableTokens,
                MaxTokens = _tokenBucket.Capacity,
                UtilizationPercent = (1.0 - (_tokenBucket.AvailableTokens / (double)_tokenBucket.Capacity)) * 100,
                ActiveOperations = _config.MaxConcurrentOperations - _semaphore.CurrentCount,
                MaxConcurrentOperations = _config.MaxConcurrentOperations
            };
        }
    }

    /// <summary>
    /// Configuration for rate limiting behavior.
    /// </summary>
    public class RateLimitConfig
    {
        /// <summary>
        /// Maximum number of tokens in bucket (capacity).
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Number of tokens added per second (refill rate).
        /// </summary>
        public int RefillRate { get; set; } = 100;

        /// <summary>
        /// Cost in tokens for each operation.
        /// </summary>
        public int TokenCost { get; set; } = 10;

        /// <summary>
        /// Maximum concurrent operations allowed simultaneously.
        /// </summary>
        public int MaxConcurrentOperations { get; set; } = 5;

        /// <summary>
        /// Seconds to advise client to wait before retrying.
        /// </summary>
        public int RetryAfterSeconds { get; set; } = 5;
    }

    /// <summary>
    /// Token bucket implementation for rate limiting.
    /// Refills tokens at configured rate while enforcing capacity limit.
    /// </summary>
    public class TokenBucket
    {
        private double _availableTokens;
        private readonly int _capacity;
        private readonly int _refillRate;
        private DateTime _lastRefillTime;
        private readonly object _lockObject = new object();

        public int Capacity => _capacity;

        public int AvailableTokens
        {
            get
            {
                lock (_lockObject)
                {
                    RefillTokens();
                    return (int)_availableTokens;
                }
            }
        }

        public TokenBucket(int capacity, int refillRate)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _availableTokens = capacity;
            _lastRefillTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Attempts to consume tokens from bucket.
        /// Returns true if sufficient tokens available, false otherwise.
        /// </summary>
        public bool TryConsume(int tokensNeeded)
        {
            lock (_lockObject)
            {
                RefillTokens();

                if (_availableTokens >= tokensNeeded)
                {
                    _availableTokens -= tokensNeeded;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Refills tokens based on elapsed time since last refill.
        /// Respects capacity limit to prevent overflow.
        /// </summary>
        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - _lastRefillTime).TotalSeconds;
            _lastRefillTime = now;

            double tokensToAdd = elapsedSeconds * _refillRate;
            _availableTokens = Math.Min(_capacity, _availableTokens + tokensToAdd);
        }
    }

    /// <summary>
    /// Current status of rate limiting system.
    /// </summary>
    public class RateLimitStatus
    {
        public int AvailableTokens { get; set; }
        public int MaxTokens { get; set; }
        public double UtilizationPercent { get; set; }
        public int ActiveOperations { get; set; }
        public int MaxConcurrentOperations { get; set; }
    }
}
