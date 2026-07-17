using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GpuImageProcessing.Tests.Services
{
    /// <summary>
    /// Extension methods for <see cref="PerformanceMonitoringServiceTests"/>.
    /// </summary>
    public static class PerformanceMonitoringServiceTestsExtensions
    {
        /// <summary>
        /// Gets the names of all public, parameter‑less test methods declared on
        /// <see cref="PerformanceMonitoringServiceTests"/>.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>An array containing the method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        public static string[] GetTestMethodNames(this PerformanceMonitoringServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return typeof(PerformanceMonitoringServiceTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                .Select(m => m.Name)
                .ToArray();
        }

        /// <summary>
        /// Executes a single test method by its exact name.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="testName">The name of the test method to invoke.</param>
        /// <returns><c>true</c> if the test completed without throwing; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> or <paramref name="testName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testName"/> is empty or does not correspond to a test method.</exception>
        public static bool RunTestByName(this PerformanceMonitoringServiceTests tests, string testName)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(testName);

            var method = typeof(PerformanceMonitoringServiceTests).GetMethod(
                testName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (method is null || method.ReturnType != typeof(void) || method.GetParameters().Length != 0)
                throw new ArgumentException($"No parameterless public test method named '{testName}' was found.", nameof(testName));

            try
            {
                method.Invoke(tests, null);
                return true;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // The test threw an exception; treat it as a failure.
                return false;
            }
        }

        /// <summary>
        /// Executes all test methods on the instance and throws an <see cref="AggregateException"/>
        /// if any of them fail.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="AggregateException">One or more test methods threw exceptions.</exception>
        public static void AssertAllTestsPass(this PerformanceMonitoringServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var failures = typeof(PerformanceMonitoringServiceTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                .Aggregate(new List<Exception>(), (failures, method) =>
                {
                    try
                    {
                        method.Invoke(tests, null);
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException is not null)
                    {
                        failures.Add(new InvalidOperationException($"Test '{method.Name}' failed.", ex.InnerException));
                    }

                    return failures;
                });

            if (failures.Count > 0)
                throw new AggregateException("One or more PerformanceMonitoringServiceTests failed.", failures);
        }
    }
}