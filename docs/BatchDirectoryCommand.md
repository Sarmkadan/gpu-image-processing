# BatchDirectoryCommand
The `BatchDirectoryCommand` class implements the `batch-dir` CLI subcommand. It processes every supported image in an input directory with one filter, writes the results to an output directory under the same file names, and shows a live progress bar while it runs. It is part of the `gpu-image-processing` application shell (`GpuImageProcessing.Cli`) and delegates the actual work to `DirectoryBatchProcessor`.

The command uses the `IImageProcessor` registered in DI when one is available. Otherwise it creates a `CpuImageProcessor`, so it runs on machines without an OpenCL-capable GPU.

## API
### Constructors
* `public BatchDirectoryCommand(IServiceProvider serviceProvider)`: Initializes a new instance of the command with the `batch-dir` name. Throws `ArgumentNullException` when `serviceProvider` is null.

### Methods
* `public override string GetDescription()`: Returns a one-line description of the command.
* `public override string GetUsage()`: Returns the usage text, including options and examples.
* `public override async Task<int> ExecuteAsync()`: Validates the arguments, runs the batch, prints the summary, and returns an exit code. Returns `0` on success (including when the input contains no supported images) and `1` on invalid arguments or when at least one image failed.

## CLI Usage
```
batch-dir --input <dir> --output <dir> --filter <name> [options]
```

| Option | Required | Description |
|---|---|---|
| `--input <dir>` | yes | Directory to scan for `.ppm`/`.pgm` images. Must exist. Scanned non-recursively. |
| `--output <dir>` | yes | Directory to write processed images to. Created if missing. |
| `--filter <name>` | no | One of `grayscale` (alias `greyscale`), `blur` (alias `boxblur`), `gaussianblur` (alias `gaussian`), `sharpen`, `edgedetection` (aliases `edge`, `sobel`), `threshold`. Default: `grayscale`. |
| `--threshold <v>` | no | Threshold value from `0.0` to `1.0`. Used only with `--filter threshold`; default `0.5`. |

Behavior notes:
* Files are processed in ordinal file-name order. Each output file keeps its input file name.
* A file that fails to load, filter, or save is reported at the end and does not stop the batch.
* Invalid input (missing `--input`/`--output`, unknown filter, out-of-range threshold, missing input directory) prints an error and returns `1` before any file is touched.

## Example
Convert every image in `./scans` to a binary image with threshold `0.6`:

```bash
batch-dir --input ./scans --output ./out --filter threshold --threshold 0.6
```

The command first prints a `Processing '<input>' with filter '<Filter>' -> '<output>'` line, then the progress bar, then a summary line of the form `Processed <succeeded>/<total> image(s) in <seconds>s`. The progress bar is omitted here.

## Notes
When using `BatchDirectoryCommand`, keep the following in mind:
* `ExecuteAsync` processes files one after another on the calling task. The progress bar is updated through an `IProgress<BatchProgress>` callback reported by `DirectoryBatchProcessor`.
* Only files recognized by the image codec registry are picked up. Other files in the input directory are ignored silently.
* If the input directory contains no supported images, the command prints a warning and returns `0`.
* When any image fails, the command lists each failed file name with its error and returns `1`.
* The output directory is not cleared first, so existing files with the same names are overwritten.
