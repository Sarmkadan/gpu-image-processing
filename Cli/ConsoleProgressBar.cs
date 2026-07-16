#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// A lightweight, redraw-in-place console progress bar for long running CLI
    /// operations such as directory batch processing. It degrades gracefully when
    /// output is redirected (no console) by emitting periodic line updates instead
    /// of carriage-return redraws, so it stays readable in CI logs.
    /// </summary>
    public sealed class ConsoleProgressBar
    {
        private readonly int _total;
        private readonly int _width;
        private readonly TextWriter _out;
        private readonly bool _interactive;
        private int _current;
        private int _lastRenderedPercent = -1;

        public ConsoleProgressBar(int total, int width = 30, TextWriter? output = null)
        {
            if (total < 0) throw new ArgumentOutOfRangeException(nameof(total));
            _total = total;
            _width = Math.Max(5, width);
            _out = output ?? Console.Out;
            _interactive = !Console.IsOutputRedirected && output is null;
        }

        /// <summary>Advances the bar by one step and redraws.</summary>
        public void Advance(string? label = null) => Report(_current + 1, label);

        /// <summary>Sets the absolute progress value and redraws.</summary>
        public void Report(int completed, string? label = null)
        {
            _current = Math.Clamp(completed, 0, _total);
            int percent = _total == 0 ? 100 : (int)(100L * _current / _total);

            // Avoid spamming non-interactive logs: only emit on percent change.
            if (!_interactive && percent == _lastRenderedPercent)
                return;
            _lastRenderedPercent = percent;

            int filled = _total == 0 ? _width : (int)((long)_width * _current / _total);
            var sb = new StringBuilder();
            sb.Append('[');
            sb.Append('#', filled);
            sb.Append('-', _width - filled);
            sb.Append("] ");
            sb.Append(percent.ToString().PadLeft(3));
            sb.Append("% (");
            sb.Append(_current);
            sb.Append('/');
            sb.Append(_total);
            sb.Append(')');
            if (!string.IsNullOrEmpty(label))
            {
                sb.Append("  ");
                sb.Append(label);
            }

            if (_interactive)
            {
                _out.Write('\r');
                _out.Write(sb.ToString().PadRight(Console.WindowWidth > 0 ? Console.WindowWidth - 1 : sb.Length));
            }
            else
            {
                _out.WriteLine(sb.ToString());
            }
        }

        /// <summary>Renders the bar as complete and moves to a fresh line.</summary>
        public void Complete(string? label = null)
        {
            Report(_total, label);
            if (_interactive)
                _out.WriteLine();
        }
    }
}
