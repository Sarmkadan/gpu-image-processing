#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Validation helpers for ImageUtilitiesBenchmarks to ensure benchmarked methods
/// produce valid, expected results during ingestion pipeline operations.
/// </summary>
public static class ImageUtilitiesBenchmarksValidation
{
    /// <summary>
    /// Validates that all benchmark methods return expected, non-default values.
    /// </summary>
    /// <param name="value">The ImageUtilitiesBenchmarks instance to validate</param>
    /// <returns>List of human-readable problems; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this ImageUtilitiesBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate boolean benchmark methods - invoke them to get actual results
        try { ValidateBooleanMethod(value.IsSupportedImageFile_Jpeg(), nameof(ImageUtilitiesBenchmarks.IsSupportedImageFile_Jpeg), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.IsSupportedImageFile_Jpeg)} threw: {ex.Message}"); }
        try { ValidateBooleanMethod(value.IsSupportedImageFile_WebP(), nameof(ImageUtilitiesBenchmarks.IsSupportedImageFile_WebP), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.IsSupportedImageFile_WebP)} threw: {ex.Message}"); }
        try { ValidateBooleanMethod(value.IsSupportedImageFile_Unsupported(), nameof(ImageUtilitiesBenchmarks.IsSupportedImageFile_Unsupported), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.IsSupportedImageFile_Unsupported)} threw: {ex.Message}"); }

        // Validate string format methods
        try { ValidateStringMethod(value.FormatFileSize_Kilobytes(), nameof(ImageUtilitiesBenchmarks.FormatFileSize_Kilobytes), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.FormatFileSize_Kilobytes)} threw: {ex.Message}"); }
        try { ValidateStringMethod(value.FormatFileSize_Megabytes(), nameof(ImageUtilitiesBenchmarks.FormatFileSize_Megabytes), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.FormatFileSize_Megabytes)} threw: {ex.Message}"); }
        try { ValidateStringMethod(value.FormatFileSize_Gigabytes(), nameof(ImageUtilitiesBenchmarks.FormatFileSize_Gigabytes), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.FormatFileSize_Gigabytes)} threw: {ex.Message}"); }

        // Validate nullable string methods
        try { ValidateNullableStringMethod(value.GetMimeType_Jpeg(), nameof(ImageUtilitiesBenchmarks.GetMimeType_Jpeg), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.GetMimeType_Jpeg)} threw: {ex.Message}"); }
        try { ValidateNullableStringMethod(value.GetMimeType_Png(), nameof(ImageUtilitiesBenchmarks.GetMimeType_Png), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.GetMimeType_Png)} threw: {ex.Message}"); }
        try { ValidateNullableStringMethod(value.GetImageFormat_Tiff(), nameof(ImageUtilitiesBenchmarks.GetImageFormat_Tiff), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.GetImageFormat_Tiff)} threw: {ex.Message}"); }

        // Validate tuple method
        try { ValidateTupleMethod(value.CalculateProportionalSize_2x(), nameof(ImageUtilitiesBenchmarks.CalculateProportionalSize_2x), problems); }
        catch (Exception ex) { problems.Add($"{nameof(ImageUtilitiesBenchmarks.CalculateProportionalSize_2x)} threw: {ex.Message}"); }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the ImageUtilitiesBenchmarks instance is valid.
    /// </summary>
    /// <param name="value">The ImageUtilitiesBenchmarks instance to check</param>
    /// <returns>True if valid; false if any validation problems exist</returns>
    public static bool IsValid(this ImageUtilitiesBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the ImageUtilitiesBenchmarks instance is valid, throwing an exception
    /// with detailed error messages if any problems are found.
    /// </summary>
    /// <param name="value">The ImageUtilitiesBenchmarks instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with problem details</exception>
    public static void EnsureValid(this ImageUtilitiesBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ImageUtilitiesBenchmarks validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates that a boolean benchmark method returned a meaningful value.
    /// </summary>
    /// <param name="result">The boolean result to validate</param>
    /// <param name="methodName">Name of the benchmark method</param>
    /// <param name="problems">List to collect validation problems</param>
    private static void ValidateBooleanMethod(bool result, string methodName, List<string> problems)
    {
        // Boolean methods should return true or false, not default(bool) which is false
        // The actual values will be validated based on the benchmark logic
        if (result == default)
        {
            problems.Add($"{methodName} returned default(false) value");
        }
    }

    /// <summary>
    /// Validates that a string benchmark method returned a valid non-empty result.
    /// </summary>
    /// <param name="result">The string result to validate</param>
    /// <param name="methodName">Name of the benchmark method</param>
    /// <param name="problems">List to collect validation problems</param>
    private static void ValidateStringMethod(string result, string methodName, List<string> problems)
    {
        if (string.IsNullOrEmpty(result))
        {
            problems.Add($"{methodName} returned null or empty string");
        }

        if (result == "Unknown")
        {
            problems.Add($"{methodName} returned 'Unknown' which indicates an error");
        }
    }

    /// <summary>
    /// Validates that a nullable string benchmark method returned a valid non-null result.
    /// </summary>
    /// <param name="result">The nullable string result to validate</param>
    /// <param name="methodName">Name of the benchmark method</param>
    /// <param name="problems">List to collect validation problems</param>
    private static void ValidateNullableStringMethod(string? result, string methodName, List<string> problems)
    {
        if (result is null)
        {
            problems.Add($"{methodName} returned null");
        }
        else if (string.IsNullOrEmpty(result))
        {
            problems.Add($"{methodName} returned empty string");
        }
    }

    /// <summary>
    /// Validates that a tuple benchmark method returned reasonable dimensions.
    /// </summary>
    /// <param name="result">The tuple result to validate</param>
    /// <param name="methodName">Name of the benchmark method</param>
    /// <param name="problems">List to collect validation problems</param>
    private static void ValidateTupleMethod((int, int) result, string methodName, List<string> problems)
    {
        if (result.Item1 <= 0 || result.Item2 <= 0)
        {
            problems.Add($"{methodName} returned non-positive dimensions: ({result.Item1}, {result.Item2})");
        }

        if (result.Item1 > 10_000 || result.Item2 > 10_000)
        {
            problems.Add($"{methodName} returned unexpectedly large dimensions: ({result.Item1}, {result.Item2})");
        }
    }
}
