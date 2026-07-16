#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Fallback;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuImageProcessing.Tests.Golden
{
    /// <summary>
    /// Exercises the explicit no-OpenCL CPU fallback path end to end. None of
    /// these tests touch a GPU device, so they guarantee the library stays
    /// functional - and CI stays green - on machines without OpenCL.
    /// </summary>
    public sealed class CpuFallbackPathTests
    {
        private static CpuImageProcessor NewProcessor() =>
            new(NullLogger<CpuImageProcessor>.Instance);

        private static Image SolidImage(int w, int h, int channels, byte value)
        {
            var img = new Image
            {
                Width = w,
                Height = h,
                Channels = channels,
                BitsPerPixel = channels * 8,
                PixelData = new byte[w * h * channels]
            };
            Array.Fill(img.PixelData!, value);
            return img;
        }

        [Theory]
        [InlineData(FilterType.Grayscale)]
        [InlineData(FilterType.Blur)]
        [InlineData(FilterType.GaussianBlur)]
        [InlineData(FilterType.Sharpen)]
        [InlineData(FilterType.EdgeDetection)]
        [InlineData(FilterType.Threshold)]
        public void CanProcess_reports_supported_cpu_filters(FilterType filterType)
        {
            NewProcessor().CanProcess(filterType).Should().BeTrue();
        }

        [Theory]
        [InlineData(FilterType.Rotation)]
        [InlineData(FilterType.ColorCorrection)]
        public void CanProcess_reports_unsupported_filters(FilterType filterType)
        {
            NewProcessor().CanProcess(filterType).Should().BeFalse();
        }

        [Fact]
        public async Task ApplyFilterAsync_runs_without_any_gpu_device()
        {
            var image = SolidImage(8, 8, 3, 128);
            var config = new FilterConfiguration
            {
                Name = "grayscale",
                FilterType = FilterType.Grayscale
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            result.Metadata.Should().ContainKey("processor");
            result.Metadata["processor"].Should().Be("cpu_fallback");
        }

        [Fact]
        public async Task ApplyFilterAsync_honours_cancellation()
        {
            var image = SolidImage(64, 64, 3, 200);
            var config = new FilterConfiguration { Name = "blur", FilterType = FilterType.Blur };
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await NewProcessor().ApplyFilterAsync(image, config, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public void Resize_produces_target_dimensions_on_cpu()
        {
            var image = SolidImage(16, 16, 3, 90);

            var resized = NewProcessor().Resize(image, 8, 4);

            resized.Width.Should().Be(8);
            resized.Height.Should().Be(4);
            resized.PixelData!.Length.Should().Be(8 * 4 * 3);
        }

        [Fact]
        public void Grayscale_collapses_channels_to_equal_luma()
        {
            var image = new Image
            {
                Width = 2, Height = 1, Channels = 3, BitsPerPixel = 24,
                PixelData = [10, 20, 30, 200, 100, 50]
            };

            var gray = NewProcessor().ToGrayscale(image);

            // Each pixel's R, G, B become the same luma byte.
            gray.PixelData![0].Should().Be(gray.PixelData[1]).And.Be(gray.PixelData[2]);
            gray.PixelData[3].Should().Be(gray.PixelData[4]).And.Be(gray.PixelData[5]);
        }
    }
}
