#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
using System;
using System.Collections.Generic;
namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Validation helpers for MetricsUtilities to ensure data quality and consistency.
    /// </summary>
    public static class MetricsUtilitiesValidation
    {
        /// <summary>
        /// Validates a StatisticalMetrics instance for common data quality issues.
        /// </summary>
        /// <param name="value">The metrics to validate</param>
        /// <returns>List of human-readable validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this StatisticalMetrics? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = new List<string>();
            // Validate counts
            if (value.Count < 0)
                problems.Add($"Count cannot be negative (was {value.Count}).");
            // Validate numeric ranges
            if (double.IsNaN(value.Min))
                problems.Add("Min cannot be NaN.");
            if (double.IsInfinity(value.Min))
                problems.Add("Min cannot be infinite.");
            if (double.IsNaN(value.Max))
                problems.Add("Max cannot be NaN.");
            if (double.IsInfinity(value.Max))
                problems.Add("Max cannot be infinite.");
            if (double.IsNaN(value.Mean))
                problems.Add("Mean cannot be NaN.");
            if (double.IsInfinity(value.Mean))
                problems.Add("Mean cannot be infinite.");
            if (double.IsNaN(value.Median))
                problems.Add("Median cannot be NaN.");
            if (double.IsInfinity(value.Median))
                problems.Add("Median cannot be infinite.");
            if (double.IsNaN(value.P95))
                problems.Add("P95 cannot be NaN.");
            if (double.IsInfinity(value.P95))
                problems.Add("P95 cannot be infinite.");
            if (double.IsNaN(value.P99))
                problems.Add("P99 cannot be NaN.");
            if (double.IsInfinity(value.P99))
                problems.Add("P99 cannot be infinite.");
            if (double.IsNaN(value.StdDev))
                problems.Add("StdDev cannot be NaN.");
            if (double.IsInfinity(value.StdDev))
                problems.Add("StdDev cannot be infinite.");
            if (double.IsNaN(value.Sum))
                problems.Add("Sum cannot be NaN.");
            if (double.IsInfinity(value.Sum))
                problems.Add("Sum cannot be infinite.");
            // Validate statistical consistency
            if (value.Count > 0)
            {
                if (value.Min > value.Max)
                    problems.Add("Min cannot be greater than Max.");
                if (value.Mean < value.Min || value.Mean > value.Max)
                    problems.Add("Mean must be between Min and Max.");
                if (value.Median < value.Min || value.Median > value.Max)
                    problems.Add("Median must be between Min and Max.");
                if (value.P95 < value.Min || value.P95 > value.Max)
                    problems.Add("P95 must be between Min and Max.");
                if (value.P99 < value.Min || value.P99 > value.Max)
                    problems.Add("P99 must be between Min and Max.");
                if (value.StdDev < 0)
                    problems.Add("StdDev cannot be negative.");
                if (value.Sum < 0 && value.Mean > 0)
                    problems.Add("Sum and Mean are inconsistent (Sum negative but Mean positive).");
                if (value.Sum > 0 && value.Mean < 0)
                    problems.Add("Sum and Mean are inconsistent (Sum positive but Mean negative).");
            }
            else
            {
                // For empty metrics, all values should be default
                if (value.Min != 0 || value.Max != 0 || value.Mean != 0 ||
                    value.Median != 0 || value.P95 != 0 || value.P99 != 0 ||
                    value.StdDev != 0 || value.Sum != 0)
                {
                    problems.Add("All statistical values should be zero for empty metrics.");
                }
            }
            return problems.AsReadOnly();
        }
        /// <summary>
        /// Validates a Histogram instance for common data quality issues.
        /// </summary>
        /// <param name="value">The histogram to validate</param>
        /// <returns>List of human-readable validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this Histogram? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = new List<string>();
            // Validate bucket list
            if (value.Buckets == null)
                problems.Add("Buckets collection cannot be null.");
            else if (value.Buckets.Count == 0 && value.TotalCount > 0)
                problems.Add("Buckets collection cannot be empty when TotalCount is positive.");
            // Validate total count
            if (value.TotalCount < 0)
                problems.Add($"TotalCount cannot be negative (was {value.TotalCount}).");
            // Validate buckets if they exist
            if (value.Buckets != null)
            {
                for (int i = 0; i < value.Buckets.Count; i++)
                {
                    var bucket = value.Buckets[i];
                    if (bucket == null)
                    {
                        problems.Add($"Bucket at index {i} cannot be null.");
                        continue;
                    }
                    if (bucket.BucketNumber < 0)
                        problems.Add($"Bucket {bucket.BucketNumber} has negative BucketNumber.");
                    if (double.IsNaN(bucket.Min))
                        problems.Add($"Bucket {bucket.BucketNumber} has NaN Min.");
                    if (double.IsInfinity(bucket.Min))
                        problems.Add($"Bucket {bucket.BucketNumber} has infinite Min.");
                    if (double.IsNaN(bucket.Max))
                        problems.Add($"Bucket {bucket.BucketNumber} has NaN Max.");
                    if (double.IsInfinity(bucket.Max))
                        problems.Add($"Bucket {bucket.BucketNumber} has infinite Max.");
                    if (bucket.Count < 0)
                        problems.Add($"Bucket {bucket.BucketNumber} has negative Count (was {bucket.Count}).");
                    if (bucket.Percentage < 0 || bucket.Percentage > 100)
                        problems.Add($"Bucket {bucket.BucketNumber} has invalid Percentage (must be 0-100, was {bucket.Percentage}).");
                    // Validate bucket range consistency
                    if (bucket.Min > bucket.Max)
                        problems.Add($"Bucket {bucket.BucketNumber} has Min greater than Max.");
                    if (i > 0)
                    {
                        var prevBucket = value.Buckets[i - 1];
                        if (prevBucket.Max > bucket.Min)
                            problems.Add($"Buckets {prevBucket.BucketNumber} and {bucket.BucketNumber} have overlapping ranges.");
                    }
                }
                // Validate total count matches sum of bucket counts
                var bucketSum = value.Buckets.Sum(b => b?.Count ?? 0);
                if (bucketSum != value.TotalCount)
                    problems.Add($"TotalCount ({value.TotalCount}) does not match sum of bucket counts ({bucketSum}).");
            }
            return problems.AsReadOnly();
        }
        /// <summary>
        /// Validates a HistogramBucket instance for common data quality issues.
        /// </summary>
        /// <param name="value">The bucket to validate</param>
        /// <returns>List of human-readable validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this HistogramBucket? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = new List<string>();
            if (value.BucketNumber < 0)
                problems.Add($"BucketNumber cannot be negative (was {value.BucketNumber}).");
            if (double.IsNaN(value.Min))
                problems.Add("Min cannot be NaN.");
            if (double.IsInfinity(value.Min))
                problems.Add("Min cannot be infinite.");
            if (double.IsNaN(value.Max))
                problems.Add("Max cannot be NaN.");
            if (double.IsInfinity(value.Max))
                problems.Add("Max cannot be infinite.");
            if (value.Min > value.Max)
                problems.Add("Min cannot be greater than Max.");
            if (value.Count < 0)
                problems.Add($"Count cannot be negative (was {value.Count}).");
            if (value.Percentage < 0 || value.Percentage > 100)
                problems.Add($"Percentage must be between 0 and 100 (was {value.Percentage}).");
            return problems.AsReadOnly();
        }
        /// <summary>
        /// Checks if a StatisticalMetrics instance is valid.
        /// </summary>
        /// <param name="value">The metrics to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(this StatisticalMetrics? value) => value.Validate().Count == 0;
        /// <summary>
        /// Checks if a Histogram instance is valid.
        /// </summary>
        /// <param name="value">The histogram to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(this Histogram? value) => value.Validate().Count == 0;
        /// <summary>
        /// Checks if a HistogramBucket instance is valid.
        /// </summary>
        /// <param name="value">The bucket to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(this HistogramBucket? value) => value.Validate().Count == 0;
        /// <summary>
        /// Ensures a StatisticalMetrics instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The metrics to validate</param>
        /// <exception cref="ArgumentException">Thrown if validation fails</exception>
        public static void EnsureValid(this StatisticalMetrics? value)
        {
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"StatisticalMetrics validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
        /// <summary>
        /// Ensures a Histogram instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The histogram to validate</param>
        /// <exception cref="ArgumentException">Thrown if validation fails</exception>
        public static void EnsureValid(this Histogram? value)
        {
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"Histogram validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
        /// <summary>
        /// Ensures a HistogramBucket instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The bucket to validate</param>
        /// <exception cref="ArgumentException">Thrown if validation fails</exception>
        public static void EnsureValid(this HistogramBucket? value)
        {
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"HistogramBucket validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
    }
}