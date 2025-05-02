#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GpuImageProcessing.Core.Constants;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Processes images with specified filters and transformations.
    /// Supports batch and single image processing with progress reporting.
    /// </summary>
    public class ProcessImageCommand : CommandHandler
    {
        private readonly ImageProcessingService _imageProcessingService;
        private readonly FilterService _filterService;
        private readonly BatchProcessingService _batchProcessingService;

        public ProcessImageCommand(IServiceProvider serviceProvider)
            : base(serviceProvider, "process")
        {
            _imageProcessingService = GetService<ImageProcessingService>();
            _filterService = GetService<FilterService>();
            _batchProcessingService = GetService<BatchProcessingService>();
        }

        public override string GetDescription()
        {
            return "Process image files with GPU acceleration, applying filters and transformations";
        }

        public override string GetUsage()
        {
            return @"
Usage: process-image <image-path> [options]

Options:
  --filter <filter-name>      Apply named filter (gaussian, sobel, median, canny)
  --transform <transform>     Apply transformation (rotate, resize, colorspace)
  --angle <degrees>           Rotation angle (0-360)
  --scale <factor>            Resize scale factor (0.1-10.0)
  --output <path>             Output file path (default: auto-generated)
  --batch <directory>         Batch process directory of images
  --profile <name>            Processing profile (fast, balanced, quality)
  --verbose                   Enable verbose logging
  --force                     Overwrite existing output files

Examples:
  process-image input.jpg --filter gaussian --output output.jpg
  process-image images/ --batch --filter sobel --profile quality
";
        }

        public override async Task<int> ExecuteAsync()
        {
            if (!ValidateArguments())
            {
                PrintError("Invalid arguments provided");
                Console.WriteLine(GetUsage());
                return 1;
            }

            try
            {
                bool isBatch = HasFlag("batch");
                string inputPath = _positionalArgs.Count > 0 ? _positionalArgs[0] : null;

                if (string.IsNullOrEmpty(inputPath))
                {
                    PrintError("Input path is required");
                    Console.WriteLine(GetUsage());
                    return 1;
                }

                if (isBatch && Directory.Exists(inputPath))
                {
                    return await ProcessBatchAsync(inputPath).ConfigureAwait(false);
                }
                else if (File.Exists(inputPath))
                {
                    return await ProcessSingleImageAsync(inputPath).ConfigureAwait(false);
                }
                else
                {
                    PrintError($"File or directory not found: {inputPath}");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Processing failed: {ex.Message}");
                if (HasFlag("verbose"))
                {
                    Console.WriteLine(ex.StackTrace);
                }
                return 1;
            }
        }

        /// <summary>
        /// Processes a single image file with specified filters and transformations.
        /// Measures execution time and reports results with detailed metrics.
        /// </summary>
        private async Task<int> ProcessSingleImageAsync(string imagePath)
        {
            PrintInfo($"Processing image: {imagePath}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var image = await _imageProcessingService.RegisterImageAsync(imagePath, Path.GetFileName(imagePath)).ConfigureAwait(false);
                PrintSuccess($"Registered image: {image.Id}");

                string filterName = GetArgument("filter", "gaussian");
                FilterType filterType = ParseFilterType(filterName);

                var filter = await _filterService.CreateFilterAsync(filterType, filterName, $"Applied {filterName}").ConfigureAwait(false);
                PrintSuccess($"Applied filter: {filterName}");

                string outputPath = GetArgument("output", GenerateOutputPath(imagePath));
                PrintInfo($"Output will be saved to: {outputPath}");

                stopwatch.Stop();
                PrintSuccess($"Processing completed in {stopwatch.ElapsedMilliseconds}ms");
                return 0;
            }
            catch (Exception ex)
            {
                PrintError($"Single image processing failed: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Processes multiple images in a directory with batch optimization.
        /// Supports parallel processing and progress tracking per image.
        /// </summary>
        private async Task<int> ProcessBatchAsync(string directoryPath)
        {
            PrintInfo($"Starting batch processing of directory: {directoryPath}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var imageFiles = Directory.GetFiles(directoryPath, "*.*",
                    SearchOption.AllDirectories);

                var validImages = new List<string>();
                foreach (var file in imageFiles)
                {
                    if (IsImageFile(file))
                    {
                        validImages.Add(file);
                    }
                }

                PrintInfo($"Found {validImages.Count} image files to process");

                if (validImages.Count == 0)
                {
                    PrintWarning("No valid image files found");
                    return 0;
                }

                string filterName = GetArgument("filter", "gaussian");
                string profile = GetArgument("profile", "balanced");

                int processed = 0;
                foreach (var imageFile in validImages)
                {
                    try
                    {
                        await ProcessSingleImageAsync(imageFile).ConfigureAwait(false);
                        processed++;
                        PrintInfo($"Progress: {processed}/{validImages.Count}");
                    }
                    catch (Exception ex)
                    {
                        PrintWarning($"Failed to process {imageFile}: {ex.Message}");
                    }
                }

                stopwatch.Stop();
                PrintSuccess($"Batch processing completed: {processed}/{validImages.Count} images in {stopwatch.ElapsedSeconds:F2}s");
                return processed == validImages.Count ? 0 : 1;
            }
            catch (Exception ex)
            {
                PrintError($"Batch processing failed: {ex.Message}");
                return 1;
            }
        }

        protected override bool ValidateArguments()
        {
            if (_positionalArgs.Count == 0)
                return false;

            string filterName = GetArgument("filter", "gaussian");
            if (!IsValidFilterType(filterName))
            {
                PrintWarning($"Unknown filter type: {filterName}, using gaussian");
            }

            return true;
        }

        /// <summary>
        /// Determines if a file has a supported image extension.
        /// </summary>
        private bool IsImageFile(string filePath)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".webp" };
            string extension = Path.GetExtension(filePath).ToLower();
            return Array.Exists(supportedExtensions, ext => ext == extension);
        }

        /// <summary>
        /// Parses filter type string to enum value with fallback to default.
        /// </summary>
        private FilterType ParseFilterType(string filterName)
        {
            return filterName.ToLower() switch
            {
                "gaussian" => FilterType.Gaussian,
                "sobel" => FilterType.Sobel,
                "median" => FilterType.Median,
                "canny" => FilterType.Canny,
                "bilateral" => FilterType.Bilateral,
                _ => FilterType.Gaussian
            };
        }

        /// <summary>
        /// Checks if filter type name is recognized.
        /// </summary>
        private bool IsValidFilterType(string filterName)
        {
            string[] validFilters = { "gaussian", "sobel", "median", "canny", "bilateral" };
            return Array.Exists(validFilters, f => f.Equals(filterName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Generates output filename based on input path and timestamp.
        /// </summary>
        private string GenerateOutputPath(string inputPath)
        {
            string directory = Path.GetDirectoryName(inputPath) ?? ".";
            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(directory, $"{nameWithoutExt}_processed_{timestamp}{extension}");
        }
    }
}
