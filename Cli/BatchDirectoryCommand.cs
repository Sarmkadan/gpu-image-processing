#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Batch;
using GpuImageProcessing.Core;
using GpuImageProcessing.Fallback;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Processes every supported image in an input directory with a single
    /// filter, writing the results to an output directory while rendering a live
    /// progress bar. Uses the CPU fallback processor by default so it works on
    /// machines without an OpenCL-capable GPU.
    /// </summary>
    public sealed class BatchDirectoryCommand : CommandHandler
    {
        public BatchDirectoryCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "batch-dir")
        {
        }

        public override string GetDescription()
            => "Batch-process every image in a directory with a filter and a progress bar";

        public override string GetUsage()
        {
            return @"
Usage: batch-dir --input <dir> --output <dir> --filter <name> [options]

Options:
  --input <dir>     Directory to scan for .ppm/.pgm images (required)
  --output <dir>    Directory to write processed images to (required)
  --filter <name>   Filter to apply: grayscale, blur, gaussianblur,
                    sharpen, edgedetection, threshold (default: grayscale)
  --threshold <v>   Threshold value 0.0-1.0 (only for --filter threshold)

Examples:
  batch-dir --input ./photos --output ./out --filter grayscale
  batch-dir --input ./scans --output ./out --filter threshold --threshold 0.6
";
        }

        public override async Task<int> ExecuteAsync()
        {
            string? input = GetArgument("input");
            string? output = GetArgument("output");
            string filterName = GetArgument("filter", "grayscale") ?? "grayscale";

            if (string.IsNullOrWhiteSpace(input))
            {
                PrintError("--input directory is required");
                return 1;
            }
            if (string.IsNullOrWhiteSpace(output))
            {
                PrintError("--output directory is required");
                return 1;
            }
            if (!Directory.Exists(input))
            {
                PrintError($"Input directory does not exist: {input}");
                return 1;
            }
            if (!TryParseFilter(filterName, out var filterType))
            {
                PrintError($"Unknown filter '{filterName}'");
                return 1;
            }

            var parameters = new Dictionary<string, object>();
            if (filterType == FilterType.Threshold)
            {
                string raw = GetArgument("threshold", "0.5") ?? "0.5";
                if (!float.TryParse(raw, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var tv) || tv is < 0f or > 1f)
                {
                    PrintError("--threshold must be a value between 0.0 and 1.0");
                    return 1;
                }
                parameters["thresholdValue"] = tv;
            }

            var processor = ResolveProcessor();
            var batch = new DirectoryBatchProcessor(processor);

            PrintInfo($"Processing '{input}' with filter '{filterType}' -> '{output}'");

            ConsoleProgressBar? bar = null;
            var progress = new Progress<BatchProgress>(p =>
            {
                bar ??= new ConsoleProgressBar(p.Total);
                bar.Report(p.Completed, p.CurrentFile);
            });

            var summary = await batch.ProcessDirectoryAsync(
                input, output, filterType, progress, parameters);

            bar?.Complete();

            if (summary.Total == 0)
            {
                PrintWarning("No supported images (.ppm/.pgm) found in the input directory.");
                return 0;
            }

            Console.WriteLine();
            PrintSuccess($"Processed {summary.Succeeded}/{summary.Total} image(s) in {summary.Elapsed.TotalSeconds:F2}s");

            if (summary.Failed > 0)
            {
                PrintWarning($"{summary.Failed} image(s) failed:");
                foreach (var item in summary.Items.Where(i => !i.Success))
                    Console.WriteLine($"  {System.IO.Path.GetFileName(item.InputPath)}: {item.Error}");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Resolves an <see cref="IImageProcessor"/> from DI when one is
        /// registered, otherwise falls back to a fresh CPU processor. This keeps
        /// the command usable both inside the host and in lightweight contexts.
        /// </summary>
        private IImageProcessor ResolveProcessor()
        {
            if (_serviceProvider.GetService(typeof(IImageProcessor)) is IImageProcessor fromDi)
                return fromDi;

            var loggerFactory = _serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory?.CreateLogger<CpuImageProcessor>()
                         ?? NullLogger<CpuImageProcessor>.Instance;
            return new CpuImageProcessor(logger);
        }

        private static bool TryParseFilter(string name, out FilterType filterType)
        {
            switch (name.Trim().ToLowerInvariant())
            {
                case "grayscale": case "greyscale": filterType = FilterType.Grayscale; return true;
                case "blur": case "boxblur": filterType = FilterType.Blur; return true;
                case "gaussianblur": case "gaussian": filterType = FilterType.GaussianBlur; return true;
                case "sharpen": filterType = FilterType.Sharpen; return true;
                case "edgedetection": case "edge": case "sobel": filterType = FilterType.EdgeDetection; return true;
                case "threshold": filterType = FilterType.Threshold; return true;
                default: filterType = FilterType.None; return false;
            }
        }
    }
}
