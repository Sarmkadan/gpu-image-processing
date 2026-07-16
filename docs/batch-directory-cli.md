# `batch-dir` - Directory Batch Processing

The `batch-dir` subcommand applies a single filter to every supported image in
a directory and writes the results to an output directory, rendering a live
progress bar as it goes. It uses the CPU fallback processor by default, so it
runs on machines without an OpenCL-capable GPU.

## Usage

```
batch-dir --input <dir> --output <dir> --filter <name> [options]
```

| Option | Description |
|--------|-------------|
| `--input <dir>`   | Directory to scan for `.ppm` / `.pgm` images (required) |
| `--output <dir>`  | Directory to write processed images to (required) |
| `--filter <name>` | `grayscale`, `blur`, `gaussianblur`, `sharpen`, `edgedetection`, `threshold` (default: `grayscale`) |
| `--threshold <v>` | Threshold value `0.0`-`1.0` (only for `--filter threshold`) |

## Examples

```bash
# Grayscale an entire folder
batch-dir --input ./photos --output ./out --filter grayscale

# Binarize scans at a custom threshold
batch-dir --input ./scans --output ./out --filter threshold --threshold 0.6

# Edge-detect a batch
batch-dir --input ./frames --output ./out --filter edgedetection
```

## Supported formats

Input and output use the dependency-free binary Netpbm formats:

- **P6** - 24-bit RGB (`.ppm`)
- **P5** - 8-bit grayscale (`.pgm`)

These are byte-exact and trivial to round-trip, which is why they also back the
project's golden-image regression fixtures under
`tests/gpu-image-processing.Tests/fixtures`.

## Programmatic API

The command is a thin wrapper over `DirectoryBatchProcessor`, which is UI-free
and reports progress through `IProgress<BatchProgress>`:

```csharp
var processor = new DirectoryBatchProcessor(
    new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance));

var summary = await processor.ProcessDirectoryAsync(
    inputDir: "./in",
    outputDir: "./out",
    filterType: FilterType.Grayscale,
    progress: new Progress<BatchProgress>(p =>
        Console.WriteLine($"{p.Completed}/{p.Total} {p.CurrentFile}")));

Console.WriteLine($"{summary.Succeeded}/{summary.Total} in {summary.Elapsed}");
```
