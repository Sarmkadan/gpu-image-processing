using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Extension methods for <see cref="ProcessImageCommand"/> that provide common image processing operations.
    /// </summary>
    public static class ProcessImageCommandExtensions
    {
        /// <summary>
        /// Validates that the input file exists and is a supported image format.
        /// </summary>
        /// <param name="command">The command instance.</param>
        /// <param name="inputPath">Path to the input image file.</param>
        /// <returns>True if the file exists and is valid; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/> is null or empty.</exception>
        public static bool ValidateInputFile(this ProcessImageCommand command, string inputPath)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(inputPath);

            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Input file does not exist: {inputPath}");
                return false;
            }

            var extension = Path.GetExtension(inputPath)[1..].ToLowerInvariant();
            var supportedExtensions = new[] { "jpg", "jpeg", "png", "bmp", "tiff", "webp" };

            if (!supportedExtensions.Contains(extension))
            {
                Console.Error.WriteLine($"Unsupported image format: .{extension}. Supported formats: {string.Join(", ", supportedExtensions)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that the output directory exists and is writable.
        /// </summary>
        /// <param name="command">The command instance.</param>
        /// <param name="outputPath">Path to the output directory.</param>
        /// <returns>True if the directory exists and is writable; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="outputPath"/> is null or empty.</exception>
        public static bool ValidateOutputDirectory(this ProcessImageCommand command, string outputPath)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(outputPath);

            if (!Directory.Exists(outputPath))
            {
                try
                {
                    Directory.CreateDirectory(outputPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to create output directory: {ex.Message}");
                    return false;
                }
            }

            try
            {
                var testFile = Path.Combine(outputPath, $".test-{Guid.NewGuid()}");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Output directory is not writable: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parses common image processing parameters from command line arguments.
        /// </summary>
        /// <param name="command">The command instance.</param>
        /// <param name="args">Command line arguments.</param>
        /// <param name="width">Parsed width parameter (0 if not specified).</param>
        /// <param name="height">Parsed height parameter (0 if not specified).</param>
        /// <param name="quality">Parsed quality parameter (0-100, 0 if not specified).</param>
        /// <returns>True if parsing succeeded; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
        public static bool TryParseImageParameters(
            this ProcessImageCommand command,
            IReadOnlyList<string> args,
            out int width,
            out int height,
            out int quality)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(args);

            width = 0;
            height = 0;
            quality = 0;

            var widthArg = args.FirstOrDefault(a => a.StartsWith("--width=", StringComparison.Ordinal));
            var heightArg = args.FirstOrDefault(a => a.StartsWith("--height=", StringComparison.Ordinal));
            var qualityArg = args.FirstOrDefault(a => a.StartsWith("--quality=", StringComparison.Ordinal));

            if (widthArg is not null)
            {
                if (!int.TryParse(widthArg["--width=".Length..], CultureInfo.InvariantCulture, out var w) || w <= 0)
                {
                    Console.Error.WriteLine("Invalid width parameter. Must be a positive integer.");
                    return false;
                }
                width = w;
            }

            if (heightArg is not null)
            {
                if (!int.TryParse(heightArg["--height=".Length..], CultureInfo.InvariantCulture, out var h) || h <= 0)
                {
                    Console.Error.WriteLine("Invalid height parameter. Must be a positive integer.");
                    return false;
                }
                height = h;
            }

            if (qualityArg is not null)
            {
                if (!int.TryParse(qualityArg["--quality=".Length..], CultureInfo.InvariantCulture, out var q) || q is < 1 or > 100)
                {
                    Console.Error.WriteLine("Invalid quality parameter. Must be an integer between 1 and 100.");
                    return false;
                }
                quality = q;
            }

            return true;
        }

        /// <summary>
        /// Generates a unique output filename based on input filename and processing parameters.
        /// </summary>
        /// <param name="command">The command instance.</param>
        /// <param name="inputPath">Path to the input image file.</param>
        /// <param name="outputDir">Output directory path.</param>
        /// <param name="width">Width parameter (0 if not specified).</param>
        /// <param name="height">Height parameter (0 if not specified).</param>
        /// <param name="quality">Quality parameter (0 if not specified).</param>
        /// <returns>Full path to the generated output filename.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputPath"/> is null or empty.</exception>
        /// <exception cref="ArgumentException"><paramref name="outputDir"/> is null or empty.</exception>
        public static string GenerateOutputFilename(
            this ProcessImageCommand command,
            string inputPath,
            string outputDir,
            int width = 0,
            int height = 0,
            int quality = 0)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(inputPath);
            ArgumentException.ThrowIfNullOrEmpty(outputDir);

            var fileName = Path.GetFileNameWithoutExtension(inputPath);
            var extension = Path.GetExtension(inputPath);
            var parameters = new List<string>();

            if (width > 0)
            {
                parameters.Add($"w{width}");
            }

            if (height > 0)
            {
                parameters.Add($"h{height}");
            }

            if (quality > 0)
            {
                parameters.Add($"q{quality}");
            }

            var paramSuffix = parameters.Count > 0 ? $"_{string.Join("_", parameters)}" : string.Empty;
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            return Path.Combine(outputDir, $"{fileName}{paramSuffix}_{timestamp}{extension}");
        }

        /// <summary>
        /// Logs processing statistics to the console.
        /// </summary>
        /// <param name="command">The command instance.</param>
        /// <param name="inputPath">Path to the input image file.</param>
        /// <param name="outputPath">Path to the output image file.</param>
        /// <param name="startTime">Processing start time.</param>
        /// <param name="endTime">Processing end time.</param>
        /// <param name="originalSize">Original file size in bytes.</param>
        /// <param name="processedSize">Processed file size in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        public static void LogProcessingStats(
            this ProcessImageCommand command,
            string inputPath,
            string outputPath,
            DateTime startTime,
            DateTime endTime,
            long originalSize,
            long processedSize)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrEmpty(inputPath);
            ArgumentException.ThrowIfNullOrEmpty(outputPath);

            var duration = endTime - startTime;
            var compressionRatio = originalSize > 0
                ? (double)processedSize / originalSize
                : 0;

            Console.WriteLine();
            Console.WriteLine("=== Processing Statistics ===");
            Console.WriteLine($"Input:  {inputPath}");
            Console.WriteLine($"Output: {outputPath}");
            Console.WriteLine($"Duration: {duration.TotalSeconds:F2} seconds");
            Console.WriteLine($"Original Size: {FormatFileSize(originalSize)}");
            Console.WriteLine($"Processed Size: {FormatFileSize(processedSize)}");
            Console.WriteLine($"Compression Ratio: {compressionRatio:P2}");
            Console.WriteLine("==========================");
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            double number = bytes;

            while (number >= 1024 && counter < suffixes.Length - 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:0.##} {suffixes[counter]}";
        }
    }
}
