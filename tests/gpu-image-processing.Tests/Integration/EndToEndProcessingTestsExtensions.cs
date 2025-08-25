using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GpuImageProcessing.Tests.Integration
{
    /// <summary>
    /// Extension methods that make it easier to work with <see cref="EndToEndProcessingTests"/>.
    /// </summary>
    public static class EndToEndProcessingTestsExtensions
    {
        /// <summary>
        /// Executes all public test methods on the supplied <see cref="EndToEndProcessingTests"/> instance.
        /// The methods are invoked sequentially in the order they are discovered via reflection.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
        public static async Task RunAllAsync(this EndToEndProcessingTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var testMethods = typeof(EndToEndProcessingTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.GetParameters().Length == 0)
                // Exclude the extension methods themselves (they are static and live on a different type)
                .ToArray();

            foreach (var method in testMethods)
            {
                var task = (Task)method.Invoke(tests, null)!;
                await task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the names of all public test methods defined on <see cref="EndToEndProcessingTests"/>.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>An enumerable of method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
        public static IEnumerable<string> GetTestMethodNames(this EndToEndProcessingTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            return typeof(EndToEndProcessingTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.GetParameters().Length == 0)
                .Select(m => m.Name);
        }

        /// <summary>
        /// Executes all test methods and returns <c>true</c> if every test completes without throwing.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>True when all tests succeed; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/></exception>
        public static async Task<bool> AllTestsPassAsync(this EndToEndProcessingTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var testMethods = typeof(EndToEndProcessingTests)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(Task) && m.GetParameters().Length == 0)
                .ToArray();

            foreach (var method in testMethods)
            {
                try
                {
                    var task = (Task)method.Invoke(tests, null)!;
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
