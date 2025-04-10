// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utility methods for performance monitoring, profiling, and metrics collection.
    /// Provides timer utilities, memory tracking, and performance analysis.
    /// </summary>
    public static class PerformanceUtilities
    {
        /// <summary>
        /// Starts a performance timer for measuring execution duration.
        /// Returns an object that can be used to get elapsed time.
        /// </summary>
        public static PerformanceTimer StartTimer(string operationName = null)
        {
            return new PerformanceTimer(operationName);
        }

        /// <summary>
        /// Gets current memory usage in bytes.
        /// </summary>
        public static long GetCurrentMemoryUsage()
        {
            return GC.GetTotalMemory(false);
        }

        /// <summary>
        /// Gets peak memory usage in bytes for the process.
        /// </summary>
        public static long GetPeakMemoryUsage()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return process.PeakWorkingSet64;
            }
        }

        /// <summary>
        /// Gets available memory in bytes for the system.
        /// </summary>
        public static long GetAvailableMemory()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return process.WorkingSet64;
            }
        }

        /// <summary>
        /// Measures throughput (operations per second) for a given operation count and duration.
        /// </summary>
        public static double CalculateThroughput(int operationCount, long durationMs)
        {
            if (durationMs <= 0)
                return 0;

            return (operationCount * 1000.0) / durationMs;
        }

        /// <summary>
        /// Calculates latency percentile from a collection of latency measurements.
        /// </summary>
        public static double GetLatencyPercentile(List<long> latencies, double percentile)
        {
            if (latencies == null || latencies.Count == 0)
                return 0;

            if (percentile < 0 || percentile > 100)
                throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

            latencies.Sort();
            int index = (int)Math.Ceiling((percentile / 100.0) * latencies.Count) - 1;
            return latencies[Math.Max(0, index)];
        }

        /// <summary>
        /// Calculates average latency from a collection of measurements.
        /// </summary>
        public static double GetAverageLatency(List<long> latencies)
        {
            if (latencies == null || latencies.Count == 0)
                return 0;

            long sum = 0;
            foreach (var latency in latencies)
            {
                sum += latency;
            }

            return sum / (double)latencies.Count;
        }

        /// <summary>
        /// Calculates median latency from a collection of measurements.
        /// </summary>
        public static double GetMedianLatency(List<long> latencies)
        {
            if (latencies == null || latencies.Count == 0)
                return 0;

            return GetLatencyPercentile(latencies, 50);
        }

        /// <summary>
        /// Forces garbage collection and waits for finalizers.
        /// Use sparingly as GC can impact performance.
        /// </summary>
        public static void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Formats duration in milliseconds to human-readable string.
        /// </summary>
        public static string FormatDuration(long milliseconds)
        {
            if (milliseconds < 1000)
                return $"{milliseconds}ms";
            else if (milliseconds < 60000)
                return $"{milliseconds / 1000.0:F2}s";
            else if (milliseconds < 3600000)
                return $"{milliseconds / 60000.0:F2}m";
            else
                return $"{milliseconds / 3600000.0:F2}h";
        }

        /// <summary>
        /// Formats memory size in bytes to human-readable string.
        /// </summary>
        public static string FormatMemory(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F2} KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F2} MB";
            else
                return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        /// <summary>
        /// Gets CPU usage percentage (0-100).
        /// </summary>
        public static float GetCpuUsage()
        {
            using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                cpuCounter.NextValue();
                return cpuCounter.NextValue();
            }
        }

        /// <summary>
        /// Calculates standard deviation from a collection of measurements.
        /// </summary>
        public static double CalculateStandardDeviation(List<long> values)
        {
            if (values == null || values.Count == 0)
                return 0;

            double average = GetAverageLatency(values);
            double sumSquareDifferences = 0;

            foreach (var value in values)
            {
                double difference = value - average;
                sumSquareDifferences += difference * difference;
            }

            double variance = sumSquareDifferences / values.Count;
            return Math.Sqrt(variance);
        }
    }

    /// <summary>
    /// Timer utility for measuring operation duration with automatic reporting.
    /// </summary>
    public class PerformanceTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;
        private bool _disposed;

        public PerformanceTimer(string operationName = null)
        {
            _operationName = operationName ?? "Operation";
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Gets elapsed time in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Gets elapsed time as TimeSpan.
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <summary>
        /// Stops the timer and returns formatted duration string.
        /// </summary>
        public string Stop()
        {
            _stopwatch.Stop();
            return PerformanceUtilities.FormatDuration(_stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Disposes timer and logs timing information if not already stopped.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _stopwatch.Stop();
            _disposed = true;
        }

        /// <summary>
        /// Gets formatted timing report.
        /// </summary>
        public override string ToString()
        {
            var duration = PerformanceUtilities.FormatDuration(_stopwatch.ElapsedMilliseconds);
            return $"{_operationName}: {duration}";
        }
    }
}
