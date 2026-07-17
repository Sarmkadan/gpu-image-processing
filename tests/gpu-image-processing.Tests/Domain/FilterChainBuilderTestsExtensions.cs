using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GpuImageProcessing.Tests.Domain
{
    /// <summary>
    /// Extension methods that make it easier to run groups of <see cref="FilterChainBuilderTests"/>
    /// test methods programmatically.
    /// </summary>
    public static class FilterChainBuilderTestsExtensions
    {
        /// <summary>
        /// Executes all public <c>Build_*</c> test methods that are expected to succeed
        /// (i.e. they should not throw). If any of them throws, an <see cref="AssertionFailedException"/>
        /// is thrown containing the name of the failing test.
        /// </summary>
        /// <param name="tests">The <see cref="FilterChainBuilderTests"/> instance to operate on.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="AssertionFailedException">If a test method throws an unexpected exception.</exception>
        public static void RunAllSuccessfulBuildTests(this FilterChainBuilderTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            // Names of the Build_* methods that are supposed to succeed.
            var successfulBuildMethods = new[]
            {
                nameof(FilterChainBuilderTests.Build_SingleStep_ProducesValidChain),
                nameof(FilterChainBuilderTests.Build_MultipleSteps_PreservesDeclarationOrder),
                nameof(FilterChainBuilderTests.Build_WithDescription_SetsChainDescription),
                nameof(FilterChainBuilderTests.Build_AllowParallelExecution_SetsFlag),
                nameof(FilterChainBuilderTests.Build_CacheIntermediates_SetsFlag),
                nameof(FilterChainBuilderTests.Build_EstimatedProcessingTimeSumsStepEstimates),
                nameof(FilterChainBuilderTests.Build_FluentChaining_ReturnsSameBuilderInstance),
                nameof(FilterChainBuilderTests.ForCi_Preset_ProducesThreeCategories)
            };

            foreach (var methodName in successfulBuildMethods)
            {
                InvokeTestMethod(tests, methodName);
            }
        }

        /// <summary>
        /// Executes all public <c>Add*</c> test methods that are expected to validate arguments
        /// and therefore throw an exception. The method verifies that the thrown exception type
        /// matches the one indicated by the test method name. If the method does not throw,
        /// or throws an unexpected exception type, an <see cref="AssertionFailedException"/>
        /// is raised.
        /// </summary>
        /// <param name="tests">The <see cref="FilterChainBuilderTests"/> instance to operate on.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is <c>null</c>.</exception>
        /// <exception cref="AssertionFailedException">If a test method does not throw or throws the wrong exception.</exception>
        public static void RunAllInvalidParameterTests(this FilterChainBuilderTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            // Mapping of test method name to the exception type it is expected to throw.
            var expectedExceptions = new Dictionary<string, Type>
            {
                { nameof(FilterChainBuilderTests.AddBlur_InvalidRadius_ThrowsArgumentOutOfRangeException), typeof(ArgumentOutOfRangeException) },
                { nameof(FilterChainBuilderTests.AddSharpen_StrengthAboveMax_ThrowsArgumentOutOfRangeException), typeof(ArgumentOutOfRangeException) },
                { nameof(FilterChainBuilderTests.AddRotation_AngleOutOfRange_ThrowsArgumentOutOfRangeException), typeof(ArgumentOutOfRangeException) },
                { nameof(FilterChainBuilderTests.AddScaling_NegativeScaleX_ThrowsArgumentOutOfRangeException), typeof(ArgumentOutOfRangeException) },
                { nameof(FilterChainBuilderTests.AddColorCorrection_BrightnessOutOfRange_ThrowsArgumentOutOfRangeException), typeof(ArgumentOutOfRangeException) },
                { nameof(FilterChainBuilderTests.AddCustomFilter_EmptyGuid_ThrowsArgumentException), typeof(ArgumentException) }
            };

            foreach (var kvp in expectedExceptions)
            {
                var methodName = kvp.Key;
                var expectedException = kvp.Value;

                try
                {
                    InvokeTestMethod(tests, methodName);
                    // If we reach here the test method did not throw.
                    throw new AssertionFailedException(
                        $"Method '{methodName}' was expected to throw {expectedException.Name} but completed successfully.");
                }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    if (tie.InnerException.GetType() != expectedException)
                    {
                        throw new AssertionFailedException(
                            $"Method '{methodName}' threw {tie.InnerException.GetType().Name} instead of expected {expectedException.Name}.",
                            tie.InnerException);
                    }
                    // Expected exception – swallow it.
                }
            }
        }

        /// <summary>
        /// Returns the names of all public instance test methods declared on
        /// <see cref="FilterChainBuilderTests"/>.
        /// The result is read‑only and ordered alphabetically.
        /// </summary>
        /// <param name="tests">The <see cref="FilterChainBuilderTests"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> containing the method names.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetAllTestMethodNames(this FilterChainBuilderTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var methodNames = tests.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                .Select(m => m.Name)
                .OrderBy(name => name, StringComparer.InvariantCulture)
                .ToArray();

            return Array.AsReadOnly(methodNames);
        }

        /// <summary>
        /// Invokes a parameterless void method by name on the test instance.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="methodName">Name of the method to invoke.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="tests"/> or <paramref name="methodName"/> is <c>null</c>.</exception>
        /// <exception cref="MissingMethodException">If the method is not found.</exception>
        /// <exception cref="InvalidOperationException">If the method doesn't return void or has parameters.</exception>
        /// <exception cref="TargetInvocationException">If the invoked method throws an exception.</exception>
        private static void InvokeTestMethod(FilterChainBuilderTests tests, string methodName)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentNullException.ThrowIfNull(methodName);

            var method = tests.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (method is null)
                throw new MissingMethodException($"Method '{methodName}' not found on type '{tests.GetType().FullName}'.");

            if (method.ReturnType != typeof(void))
                throw new InvalidOperationException($"Method '{methodName}' must return void.");

            if (method.GetParameters().Length != 0)
                throw new InvalidOperationException($"Method '{methodName}' must be parameterless.");

            method.Invoke(tests, null);
        }
    }

    /// <summary>
    /// Represents a failure of an assertion performed by the extension helpers.
    /// </summary>
    public sealed class AssertionFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssertionFailedException"/> with the specified message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AssertionFailedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of <see cref="AssertionFailedException"/> with the specified message
        /// and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public AssertionFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}