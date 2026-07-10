using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Domain
{
    public static class ComputeShaderPassExtensions
    {
        /// <summary>
        /// Determines whether this pass is ready for execution based on its configuration.
        /// A pass is ready when it has a workgroup configuration, at least one input image,
        /// and an output image set.
        /// </summary>
        /// <param name="pass">The compute shader pass</param>
        /// <returns>True if the pass is ready for execution, false otherwise</returns>
        public static bool IsReadyForExecution(this ComputeShaderPass pass)
        {
            return pass?.IsReady() ?? false;
        }

        /// <summary>
        /// Gets the number of input images configured for this pass.
        /// </summary>
        /// <param name="pass">The compute shader pass</param>
        /// <returns>The count of input images, or 0 if pass is null</returns>
        public static int GetInputImageCount(this ComputeShaderPass pass)
        {
            return pass?.InputImages?.Count ?? 0;
        }

        /// <summary>
        /// Determines whether this pass has any parameters configured.
        /// </summary>
        /// <param name="pass">The compute shader pass</param>
        /// <returns>True if the pass has parameters, false otherwise</returns>
        public static bool HasParameters(this ComputeShaderPass pass)
        {
            return pass?.Parameters?.Count > 0;
        }

        /// <summary>
        /// Gets the number of parameters configured for this pass.
        /// </summary>
        /// <param name="pass">The compute shader pass</param>
        /// <returns>The count of parameters, or 0 if pass or Parameters is null</returns>
        public static int GetParameterCount(this ComputeShaderPass pass)
        {
            return pass?.Parameters?.Count ?? 0;
        }
    }
}