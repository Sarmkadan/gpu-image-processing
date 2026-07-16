#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Fallback;
using GpuImageProcessing.Imaging;

namespace GpuImageProcessing.Batch;

/// <summary>Per-file progress notification raised while a directory is processed.</summary>
public readonly record struct BatchProgress(int Completed, int Total, string CurrentFile);

/// <summary>Outcome for a single processed file.</summary>
public sealed record BatchItemResult(string InputPath, string? OutputPath, bool Success, string? Error);

/// <summary>Aggregate outcome for a whole directory run.</summary>
public sealed record BatchRunSummary(
    IReadOnlyList<BatchItemResult> Items,
    TimeSpan Elapsed)
{
    public int Total => Items.Count;
    public int Succeeded => Items.Count(i => i.Success);
    public int Failed => Items.Count(i => !i.Success);
}

/// <summary>
/// Processes every supported image in a directory by applying a single filter
/// through an <see cref="IImageProcessor"/> backend. The default backend is the
/// pure-CPU <see cref="CpuImageProcessor"/>, so this runs with no GPU/OpenCL
/// device present - which is exactly what CI needs.
///
/// The class is deliberately UI-free: it reports progress through an
/// <see cref="IProgress{T}"/> callback so both the CLI (with a live progress
/// bar) and unit tests (collecting notifications) can drive it identically.
/// </summary>
public sealed class DirectoryBatchProcessor
{
    private readonly IImageProcessor _processor;

    public DirectoryBatchProcessor(IImageProcessor processor)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    /// <summary>
    /// Enumerates supported pixmaps in <paramref name="inputDir"/>, applies
    /// <paramref name="filterType"/>, and writes results into
    /// <paramref name="outputDir"/> preserving file names.
    /// </summary>
    public async Task<BatchRunSummary> ProcessDirectoryAsync(
        string inputDir,
        string outputDir,
        FilterType filterType,
        IProgress<BatchProgress>? progress = null,
        IReadOnlyDictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputDir);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDir);
        if (!Directory.Exists(inputDir))
            throw new DirectoryNotFoundException($"Input directory not found: {inputDir}");

        var files = Directory.EnumerateFiles(inputDir)
            .Where(PortablePixmap.IsSupported)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        Directory.CreateDirectory(outputDir);

        var config = new FilterConfiguration
        {
            Name = filterType.ToString(),
            FilterType = filterType
        };
        if (parameters is not null)
        {
            foreach (var (key, value) in parameters)
                config.Parameters[key] = value;
        }

        var results = new List<BatchItemResult>(files.Count);
        var startedAt = DateTime.UtcNow;

        for (int i = 0; i < files.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string input = files[i];
            string output = System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName(input));

            try
            {
                var image = PortablePixmap.Load(input);
                var processed = await _processor.ApplyFilterAsync(image, config, cancellationToken)
                    .ConfigureAwait(false);
                PortablePixmap.Save(processed, output);
                results.Add(new BatchItemResult(input, output, true, null));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                results.Add(new BatchItemResult(input, null, false, ex.Message));
            }

            progress?.Report(new BatchProgress(i + 1, files.Count, System.IO.Path.GetFileName(input)));
        }

        return new BatchRunSummary(results, DateTime.UtcNow - startedAt);
    }
}
