// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Centralized telemetry service for collecting and reporting system metrics.
    /// Tracks performance, errors, and usage patterns across the application.
    /// </summary>
    public class TelemetryService
    {
        private readonly Dictionary<string, PerformanceCounter> _counters;
        private readonly List<TelemetryEvent> _events;
        private readonly int _maxEventsBuffer;
        private readonly object _lockObject = new();

        public TelemetryService(int maxEventsBuffer = 10000)
        {
            _counters = new Dictionary<string, PerformanceCounter>();
            _events = new List<TelemetryEvent>();
            _maxEventsBuffer = maxEventsBuffer;
        }

        /// <summary>
        /// Records a telemetry event
        /// </summary>
        public void RecordEvent(string eventName, Dictionary<string, object> properties = null, string severity = "info")
        {
            lock (_lockObject)
            {
                var telemetryEvent = new TelemetryEvent
                {
                    Id = Guid.NewGuid(),
                    Name = eventName,
                    Properties = properties ?? new Dictionary<string, object>(),
                    Severity = severity,
                    Timestamp = DateTime.UtcNow
                };

                _events.Add(telemetryEvent);

                // Maintain buffer size
                if (_events.Count > _maxEventsBuffer)
                    _events.RemoveRange(0, _events.Count - _maxEventsBuffer);
            }
        }

        /// <summary>
        /// Starts timing an operation
        /// </summary>
        public TimingToken StartTiming(string operationName)
        {
            return new TimingToken(operationName, this);
        }

        /// <summary>
        /// Records a timed operation
        /// </summary>
        internal void RecordTiming(string operationName, TimeSpan duration, bool success = true)
        {
            var key = $"timing_{operationName}";

            lock (_lockObject)
            {
                if (!_counters.TryGetValue(key, out var counter))
                {
                    counter = new PerformanceCounter { Name = operationName };
                    _counters[key] = counter;
                }

                counter.Count++;
                counter.TotalMilliseconds += duration.TotalMilliseconds;
                counter.LastRecordedAt = DateTime.UtcNow;

                if (success)
                    counter.SuccessCount++;
                else
                    counter.FailureCount++;

                if (duration.TotalMilliseconds > counter.MaxMilliseconds)
                    counter.MaxMilliseconds = duration.TotalMilliseconds;

                if (counter.MinMilliseconds == 0 || duration.TotalMilliseconds < counter.MinMilliseconds)
                    counter.MinMilliseconds = duration.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Increments a counter
        /// </summary>
        public void IncrementCounter(string counterName, int value = 1)
        {
            lock (_lockObject)
            {
                if (!_counters.TryGetValue(counterName, out var counter))
                {
                    counter = new PerformanceCounter { Name = counterName };
                    _counters[counterName] = counter;
                }

                counter.Count += value;
                counter.LastRecordedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Sets a gauge value (snapshot metric)
        /// </summary>
        public void SetGauge(string gaugeName, double value)
        {
            lock (_lockObject)
            {
                if (!_counters.TryGetValue(gaugeName, out var counter))
                {
                    counter = new PerformanceCounter { Name = gaugeName };
                    _counters[gaugeName] = counter;
                }

                counter.Value = value;
                counter.LastRecordedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets summary statistics for an operation
        /// </summary>
        public OperationStats GetOperationStats(string operationName)
        {
            var key = $"timing_{operationName}";

            lock (_lockObject)
            {
                if (!_counters.TryGetValue(key, out var counter))
                    return null;

                var avgMs = counter.Count > 0 ? counter.TotalMilliseconds / counter.Count : 0;
                var successRate = counter.Count > 0 ? (counter.SuccessCount / (double)counter.Count) * 100 : 0;

                return new OperationStats
                {
                    OperationName = operationName,
                    TotalCount = counter.Count,
                    SuccessCount = counter.SuccessCount,
                    FailureCount = counter.FailureCount,
                    SuccessRate = successRate,
                    AverageMilliseconds = avgMs,
                    MinMilliseconds = counter.MinMilliseconds,
                    MaxMilliseconds = counter.MaxMilliseconds,
                    TotalMilliseconds = counter.TotalMilliseconds,
                    LastRecordedAt = counter.LastRecordedAt
                };
            }
        }

        /// <summary>
        /// Gets all recorded events
        /// </summary>
        public List<TelemetryEvent> GetEvents(TimeSpan? timeWindow = null, string severity = null)
        {
            lock (_lockObject)
            {
                var query = _events.AsEnumerable();

                if (timeWindow.HasValue)
                {
                    var cutoffTime = DateTime.UtcNow - timeWindow.Value;
                    query = query.Where(e => e.Timestamp >= cutoffTime);
                }

                if (!string.IsNullOrEmpty(severity))
                    query = query.Where(e => e.Severity == severity);

                return query.ToList();
            }
        }

        /// <summary>
        /// Gets overall system statistics
        /// </summary>
        public SystemTelemetryStats GetSystemStats()
        {
            lock (_lockObject)
            {
                var process = Process.GetCurrentProcess();
                var workingSet = process.WorkingSet64;
                var privateMemory = process.PrivateMemorySize64;

                var timingCounters = _counters.Values
                    .Where(c => c.Name.Contains("timing"))
                    .ToList();

                var totalOps = timingCounters.Sum(c => c.Count);
                var totalSuccessful = timingCounters.Sum(c => c.SuccessCount);
                var avgLatency = timingCounters.Any()
                    ? timingCounters.Average(c => c.Count > 0 ? c.TotalMilliseconds / c.Count : 0)
                    : 0;

                return new SystemTelemetryStats
                {
                    Timestamp = DateTime.UtcNow,
                    ProcessId = process.Id,
                    WorkingSetBytes = workingSet,
                    PrivateMemoryBytes = privateMemory,
                    ThreadCount = process.Threads.Count,
                    TotalOperations = totalOps,
                    SuccessfulOperations = totalSuccessful,
                    FailedOperations = totalOps - totalSuccessful,
                    SuccessRate = totalOps > 0 ? (totalSuccessful / (double)totalOps) * 100 : 0,
                    AverageLatencyMs = avgLatency,
                    EventCount = _events.Count,
                    CounterCount = _counters.Count
                };
            }
        }

        /// <summary>
        /// Clears all telemetry data
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _events.Clear();
                _counters.Clear();
            }
        }

        private class PerformanceCounter
        {
            public string Name { get; set; }
            public long Count { get; set; }
            public long SuccessCount { get; set; }
            public long FailureCount { get; set; }
            public double TotalMilliseconds { get; set; }
            public double MinMilliseconds { get; set; }
            public double MaxMilliseconds { get; set; }
            public double Value { get; set; }
            public DateTime LastRecordedAt { get; set; }
        }
    }

    public class TimingToken : IDisposable
    {
        private readonly string _operationName;
        private readonly TelemetryService _telemetryService;
        private readonly Stopwatch _stopwatch;

        internal TimingToken(string operationName, TelemetryService telemetryService)
        {
            _operationName = operationName;
            _telemetryService = telemetryService;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _telemetryService.RecordTiming(_operationName, _stopwatch.Elapsed);
        }
    }

    public class TelemetryEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public string Severity { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class OperationStats
    {
        public string OperationName { get; set; }
        public long TotalCount { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
        public double SuccessRate { get; set; }
        public double AverageMilliseconds { get; set; }
        public double MinMilliseconds { get; set; }
        public double MaxMilliseconds { get; set; }
        public double TotalMilliseconds { get; set; }
        public DateTime LastRecordedAt { get; set; }
    }

    public class SystemTelemetryStats
    {
        public DateTime Timestamp { get; set; }
        public int ProcessId { get; set; }
        public long WorkingSetBytes { get; set; }
        public long PrivateMemoryBytes { get; set; }
        public int ThreadCount { get; set; }
        public long TotalOperations { get; set; }
        public long SuccessfulOperations { get; set; }
        public long FailedOperations { get; set; }
        public double SuccessRate { get; set; }
        public double AverageLatencyMs { get; set; }
        public int EventCount { get; set; }
        public int CounterCount { get; set; }
    }
}
