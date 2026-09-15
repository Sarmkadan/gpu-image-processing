# ConsoleProgressBar

The `ConsoleProgressBar` class provides a lightweight, redraw-in-place console progress bar for long running CLI operations such as directory batch processing. It degrades gracefully when output is redirected (no console) by emitting periodic line updates instead of carriage-return redraws, so it stays readable in CI logs.

## API

### Constructor
`ConsoleProgressBar(int total, int width = 30, TextWriter? output = null)`
- **total**: The total number of steps (must be non-negative).
- **width**: The width of the progress bar in characters (minimum 5).
- **output**: Optional `TextWriter` for output (defaults to `Console.Out`). If `null` and output is not redirected, the bar uses interactive mode (carriage-return redraws). If output is redirected or a writer is provided, it uses non-interactive mode (periodic line updates).

### Methods
#### `Advance(string? label = null)`
Advances the bar by one step and redraws. Equivalent to calling `Report(_current + 1, label)`.

#### `Report(int completed, string? label = null)`
Sets the absolute progress value and redraws the bar.
- **completed**: The number of completed steps (clamped between 0 and total).
- **label**: Optional text to display after the progress bar.

#### `Complete(string? label = null)`
Renders the bar as complete and moves to a fresh line. Equivalent to calling `Report(_total, label)` and then writing a newline if in interactive mode.

## Usage
```csharp
var totalFiles = Directory.GetFiles("images", "*.jpg").Length;
using var progress = new ConsoleProgressBar(totalFiles);
foreach (var file in Directory.GetFiles("images", "*.jpg"))
{
    // Process file...
    progress.Advance($"Processing {Path.GetFileName(file)}");
}
progress.Complete("Done!");
```