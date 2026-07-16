#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Batch;
using FluentAssertions;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Fallback;
using GpuImageProcessing.Imaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuImageProcessing.Tests.Batch
{
    /// <summary>
    /// Tests for the directory batch-processing pipeline that backs the
    /// <c>batch-dir</c> CLI subcommand. Uses temporary directories and the CPU
    /// fallback processor so it needs no GPU.
    /// </summary>
    public sealed class DirectoryBatchProcessorTests : IDisposable
    {
        private readonly string _root;
        private readonly string _in;
        private readonly string _out;

        public DirectoryBatchProcessorTests()
        {
            _root = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "gip-batch-" + Guid.NewGuid().ToString("N"));
            _in = System.IO.Path.Combine(_root, "in");
            _out = System.IO.Path.Combine(_root, "out");
            Directory.CreateDirectory(_in);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); }
            catch { /* best-effort temp cleanup */ }
        }

        private static DirectoryBatchProcessor NewProcessor() =>
            new(new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance));

        private void WriteGradient(string name, int w = 12, int h = 8)
        {
            var img = new Image { Width = w, Height = h, Channels = 3, BitsPerPixel = 24 };
            var data = new byte[w * h * 3];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int i = (y * w + x) * 3;
                data[i] = (byte)(x * 7 % 256);
                data[i + 1] = (byte)(y * 11 % 256);
                data[i + 2] = (byte)((x + y) * 5 % 256);
            }
            img.PixelData = data;
            PortablePixmap.Save(img, System.IO.Path.Combine(_in, name));
        }

        [Fact]
        public async Task Processes_all_supported_files_and_writes_outputs()
        {
            WriteGradient("a.ppm");
            WriteGradient("b.ppm");
            WriteGradient("c.ppm");

            var summary = await NewProcessor().ProcessDirectoryAsync(_in, _out, FilterType.Grayscale);

            summary.Total.Should().Be(3);
            summary.Succeeded.Should().Be(3);
            summary.Failed.Should().Be(0);

            File.Exists(System.IO.Path.Combine(_out, "a.ppm")).Should().BeTrue();
            File.Exists(System.IO.Path.Combine(_out, "b.ppm")).Should().BeTrue();
            File.Exists(System.IO.Path.Combine(_out, "c.ppm")).Should().BeTrue();
        }

        [Fact]
        public async Task Skips_unsupported_extensions()
        {
            WriteGradient("keep.ppm");
            File.WriteAllText(System.IO.Path.Combine(_in, "notes.txt"), "ignore me");
            File.WriteAllBytes(System.IO.Path.Combine(_in, "photo.jpg"), [0xFF, 0xD8, 0xFF]);

            var summary = await NewProcessor().ProcessDirectoryAsync(_in, _out, FilterType.Blur);

            summary.Total.Should().Be(1);
            Directory.GetFiles(_out).Should().ContainSingle();
        }

        [Fact]
        public async Task Reports_progress_for_every_file_in_order()
        {
            WriteGradient("1.ppm");
            WriteGradient("2.ppm");
            WriteGradient("3.ppm");

            var seen = new List<BatchProgress>();
            var progress = new Progress<BatchProgress>(seen.Add);

            // Progress<T> posts callbacks to the captured sync context; drain via TCS.
            var summary = await NewProcessor().ProcessDirectoryAsync(
                _in, _out, FilterType.Grayscale, progress);

            // Give any queued progress callbacks a chance to flush.
            await Task.Delay(50);

            summary.Total.Should().Be(3);
            seen.Should().NotBeEmpty();
            seen[^1].Completed.Should().Be(3);
            seen[^1].Total.Should().Be(3);
        }

        [Fact]
        public async Task Output_is_byte_identical_to_direct_filter_application()
        {
            WriteGradient("g.ppm");

            await NewProcessor().ProcessDirectoryAsync(_in, _out, FilterType.Grayscale);

            var direct = PortablePixmap.Load(System.IO.Path.Combine(_in, "g.ppm"));
            var config = new FilterConfiguration { Name = "grayscale", FilterType = FilterType.Grayscale };
            var expected = await new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance)
                .ApplyFilterAsync(direct, config);

            var produced = PortablePixmap.Load(System.IO.Path.Combine(_out, "g.ppm"));
            PortablePixmap.PixelHash(produced).Should().Be(PortablePixmap.PixelHash(expected));
        }

        [Fact]
        public async Task Empty_directory_yields_zero_total()
        {
            var summary = await NewProcessor().ProcessDirectoryAsync(_in, _out, FilterType.Grayscale);
            summary.Total.Should().Be(0);
            summary.Succeeded.Should().Be(0);
        }

        [Fact]
        public async Task Missing_input_directory_throws()
        {
            var proc = NewProcessor();
            var missing = System.IO.Path.Combine(_root, "does-not-exist");

            var act = async () => await proc.ProcessDirectoryAsync(missing, _out, FilterType.Blur);

            await act.Should().ThrowAsync<DirectoryNotFoundException>();
        }

        [Fact]
        public async Task Cancellation_stops_the_run()
        {
            for (int i = 0; i < 5; i++) WriteGradient($"{i}.ppm");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await NewProcessor().ProcessDirectoryAsync(
                _in, _out, FilterType.Grayscale, cancellationToken: cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
