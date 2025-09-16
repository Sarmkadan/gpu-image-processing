using System;
using System.Collections.Generic;
using GpuImageProcessing.Middleware;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for the <see cref="ProcessingPipeline"/> class.
    /// </summary>
    public static class ProcessingPipelineExtensions
    {
        /// <summary>
        /// Registers a range of middleware components into the pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline instance.</param>
        /// <param name="middlewares">The collection of middleware to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when pipeline or middlewares is null.</exception>
        public static void RegisterRange(this ProcessingPipeline pipeline, IEnumerable<IProcessingMiddleware> middlewares)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(middlewares);

            foreach (var middleware in middlewares)
            {
                pipeline.RegisterMiddleware(middleware);
            }
        }

        /// <summary>
        /// Clears all existing middleware and registers a new range.
        /// </summary>
        /// <param name="pipeline">The pipeline instance.</param>
        /// <param name="middlewares">The new collection of middleware to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when pipeline or middlewares is null.</exception>
        public static void ClearAndRegister(this ProcessingPipeline pipeline, IEnumerable<IProcessingMiddleware> middlewares)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(middlewares);

            pipeline.ClearMiddleware();
            pipeline.RegisterRange(middlewares);
        }

        /// <summary>
        /// Returns the count of registered middleware.
        /// </summary>
        /// <param name="pipeline">The pipeline instance.</param>
        /// <returns>The number of registered middleware.</returns>
        /// <exception cref="ArgumentNullException">Thrown when pipeline is null.</exception>
        public static int GetMiddlewareCount(this ProcessingPipeline pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);

            return pipeline.GetMiddlewareOrder().Count;
        }
    }
}
