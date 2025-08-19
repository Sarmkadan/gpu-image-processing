using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Domain
{
public static class ComputeShaderPassExtensions
{
/// <summary>
/// Determines whether this pass is ready for execution based on its configuration.
/// A pass is ready when it has a workgroup configuration, at least one input image,
/// and an output image set.
/// </summary>
/// <param name="pass">The compute shader pass.</param>
/// <returns>True if the pass is ready for execution; otherwise, false.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="pass"/> is null.</exception>
public static bool IsReadyForExecution(this ComputeShaderPass pass)
{
ArgumentNullException.ThrowIfNull(pass);
return pass.IsReady();
}

/// <summary>
/// Gets the number of input images configured for this pass.
/// </summary>
/// <param name="pass">The compute shader pass.</param>
/// <returns>The count of input images.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="pass"/> is null.</exception>
public static int GetInputImageCount(this ComputeShaderPass pass)
{
ArgumentNullException.ThrowIfNull(pass);
return pass.InputImages.Count;
}

/// <summary>
/// Determines whether this pass has any parameters configured.
/// </summary>
/// <param name="pass">The compute shader pass.</param>
/// <returns>True if the pass has parameters; otherwise, false.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="pass"/> is null.</exception>
public static bool HasParameters(this ComputeShaderPass pass)
{
ArgumentNullException.ThrowIfNull(pass);
return pass.Parameters.Count > 0;
}

/// <summary>
/// Gets the number of parameters configured for this pass.
/// </summary>
/// <param name="pass">The compute shader pass.</param>
/// <returns>The count of parameters.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="pass"/> is null.</exception>
public static int GetParameterCount(this ComputeShaderPass pass)
{
ArgumentNullException.ThrowIfNull(pass);
return pass.Parameters.Count;
}
}
}