#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Utilities for collecting, aggregating, and analyzing performance metrics.
    /// Supports histograms, percentiles, and statistical analysis.
    /// </summary>
    public static class MetricsUtilities
    {
        /// <summary>
        /// Calculates statistical metrics from a collection of values
        /// </summary>
        public static StatisticalMetrics CalculateStatistics(IEnumerable<double> values)
        {
            var valuesList = values.ToList();
            if (valuesList.Count == 0)
                return new StatisticalMetrics();

            var sorted = valuesList.OrderBy(v => v).ToList();
            var sum = valuesList.Sum();
            var mean = sum / valuesList.Count;
            var variance = valuesList.Sum(v => Math.Pow(v - mean, 2)) / valuesList.Count;
            var stdDev = Math.Sqrt(variance);

            return new StatisticalMetrics
            {
                Count = valuesList.Count,
                Min = sorted.First(),
                Max = sorted.Last(),
                Mean = mean,
                Median = GetPercentile(sorted, 50),
                P95 = GetPercentile(sorted, 95),
                P99 = GetPercentile(sorted, 99),
                StdDev = stdDev,
                Sum = sum
            };
        }

        /// <summary>
        /// Calculates the Nth percentile of a sorted list
        /// </summary>
        public static double GetPercentile(List<double> sortedValues, int percentile)
        {
            if (sortedValues.Count == 0)
                return 0;

            if (percentile < 0 || percentile > 100)
                throw new ArgumentException("Percentile must be between 0 and 100", nameof(percentile));

            var index = (percentile / 100.0) * (sortedValues.Count - 1);
            var lower = (int)Math.Floor(index);
            var upper = (int)Math.Ceiling(index);
            var weight = index - lower;

            if (lower == upper)
                return sortedValues[lower];

            return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
        }

        /// <summary>
        /// Creates a histogram of values with specified bucket count
        /// </summary>
        public static Histogram CreateHistogram(IEnumerable<double> values, int bucketCount = 20)
        {
            var valuesList = values.ToList();
            if (valuesList.Count == 0)
                return new Histogram();

            var min = valuesList.Min();
            var max = valuesList.Max();
            var range = max - min == 0 ? 1 : max - min;
            var bucketSize = range / bucketCount;

            var buckets = new List<HistogramBucket>();
            for (int i = 0; i < bucketCount; i++)
            {
                var bucketMin = min + (i * bucketSize);
                var bucketMax = bucketMin + bucketSize;
                var count = valuesList.Count(v => v >= bucketMin && v < bucketMax);

                buckets.Add(new HistogramBucket
                {
                    BucketNumber = i,
                    Min = bucketMin,
                    Max = bucketMax,
                    Count = count,
                    Percentage = (count / (double)valuesList.Count) * 100
                });
            }

            return new Histogram { Buckets = buckets, TotalCount = valuesList.Count };
        }

        /// <summary>
        /// Detects anomalies using interquartile range method
        /// </summary>
        public static List<double> DetectAnomalies(IEnumerable<double> values, double iqrMultiplier = 1.5)
        {
            var sortedValues = values.OrderBy(v => v).ToList();
            if (sortedValues.Count < 4)
                return new List<double>();

            var q1 = GetPercentile(sortedValues, 25);
            var q3 = GetPercentile(sortedValues, 75);
            var iqr = q3 - q1;

            var lowerBound = q1 - (iqr * iqrMultiplier);
            var upperBound = q3 + (iqr * iqrMultiplier);

            return sortedValues.Where(v => v < lowerBound || v > upperBound).ToList();
        }

        /// <summary>
        /// Calculates rate of change between consecutive measurements
        /// </summary>
        public static List<double> CalculateRateOfChange(IEnumerable<double> values)
        {
            var valuesList = values.ToList();
            var roc = new List<double>();

            for (int i = 1; i < valuesList.Count; i++)
            {
                var change = valuesList[i] - valuesList[i - 1];
                roc.Add(change);
            }

            return roc;
        }

        /// <summary>
        /// Calculates moving average over a window
        /// </summary>
        public static List<double> CalculateMovingAverage(IEnumerable<double> values, int windowSize)
        {
            if (windowSize <= 0)
                throw new ArgumentException("Window size must be positive", nameof(windowSize));

            var valuesList = values.ToList();
            var movingAverage = new List<double>();

            for (int i = 0; i < valuesList.Count; i++)
            {
                var start = Math.Max(0, i - windowSize + 1);
                var window = valuesList.Skip(start).Take(i - start + 1);
                movingAverage.Add(window.Average());
            }

            return movingAverage;
        }

        /// <summary>
        /// Estimates throughput (items per second)
        /// </summary>
        public static double CalculateThroughput(int itemCount, TimeSpan duration)
        {
            if (duration.TotalSeconds == 0)
                return 0;

            return itemCount / duration.TotalSeconds;
        }

        /// <summary>
        /// Calculates efficiency as a percentage of theoretical peak
        /// </summary>
        public static float CalculateEfficiency(float actualPerformance, float peakPerformance)
        {
            if (peakPerformance <= 0)
                return 0;

            return Math.Min((actualPerformance / peakPerformance) * 100, 100);
        }
    }

    public class StatisticalMetrics
    {
        public int Count { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double P95 { get; set; }
        public double P99 { get; set; }
        public double StdDev { get; set; }
        public double Sum { get; set; }
    }

    public class Histogram
    {
        public List<HistogramBucket> Buckets { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class HistogramBucket
    {
        public int BucketNumber { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
