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
using GpuImageProcessing.Filters;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuImageProcessing.Tests.Filters
{
    public sealed class HistogramEqualizationFilterTests
    {
        private static Image CreateRgbImage(int width, int height, byte[] pixels)
        {
            return new Image(width, height, 3)
            {
                BitsPerPixel = 24,
                PixelData = pixels
            };
        }

        [Fact]
        public void Equalize_UniformChannel_KeepsValues()
        {
            var src = new byte[] { 100, 100, 100, 100, 100, 100 };

            var dst = HistogramEqualizationFilter.Equalize(src, 3);

            dst.Should().Equal(src);
        }

        [Fact]
        public void Equalize_LowContrastChannel_StretchesToFullRange()
        {
            // Two intensities in a narrow band; equalization should push them to 0 and 255.
            var src = new byte[] { 10, 10, 10, 20, 20, 20 };

            var dst = HistogramEqualizationFilter.Equalize(src, 3);

            dst[0].Should().Be(0);
            dst[3].Should().Be(255);
        }

        [Fact]
        public void Equalize_ChannelsAreIndependent()
        {
            // R channel is constant, G channel is two-valued; B is ignored by the other channels.
            var src = new byte[] { 50, 0, 7, 50, 255, 7 };

            var dst = HistogramEqualizationFilter.Equalize(src, 3);

            dst[0].Should().Be(50);
            dst[3].Should().Be(50);
            dst[1].Should().Be(0);
            dst[4].Should().Be(255);
            dst[2].Should().Be(7);
            dst[5].Should().Be(7);
        }

        [Fact]
        public void Equalize_AlphaChannel_IsCopiedUnchanged()
        {
            var src = new byte[] { 10, 10, 10, 77, 20, 20, 20, 99 };

            var dst = HistogramEqualizationFilter.Equalize(src, 4);

            dst[3].Should().Be(77);
            dst[7].Should().Be(99);
        }

        [Fact]
        public void ComputeHistograms_CountsEachChannelSeparately()
        {
            var src = new byte[] { 1, 2, 3, 1, 2, 3 };

            var histograms = HistogramEqualizationFilter.ComputeHistograms(src, 3);

            histograms.Should().HaveCount(3);
            histograms[0][1].Should().Be(2);
            histograms[1][2].Should().Be(2);
            histograms[2][3].Should().Be(2);
        }

        [Fact]
        public void Apply_NullImage_ThrowsArgumentNullException()
        {
            var act = () => HistogramEqualizationFilter.Apply(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Apply_Image_ReplacesPixelDataInPlace()
        {
            var image = CreateRgbImage(2, 1, new byte[] { 10, 10, 10, 20, 20, 20 });

            var result = HistogramEqualizationFilter.Apply(image);

            result.Should().BeSameAs(image);
            image.PixelData![0].Should().Be(0);
            image.PixelData[3].Should().Be(255);
        }

        [Fact]
        public void CreateConfiguration_SelectsHistogramFilterType()
        {
            var config = HistogramEqualizationFilter.CreateConfiguration();

            config.FilterType.Should().Be(FilterType.Histogram);
            config.Validate().Should().BeTrue();
        }

        [Fact]
        public void FilterChainBuilder_AddHistogramEqualization_AddsStep()
        {
            var chain = FilterChainBuilder.Create("equalize")
                .AddHistogramEqualization()
                .Build();

            chain.Steps.Should().HaveCount(1);
        }

        [Fact]
        public async Task CpuImageProcessor_HistogramConfiguration_EqualizesImage()
        {
            var processor = new CpuImageProcessor(NullLogger<CpuImageProcessor>.Instance);
            var image = CreateRgbImage(2, 1, new byte[] { 10, 10, 10, 20, 20, 20 });

            var result = await processor.ApplyFilterAsync(image, HistogramEqualizationFilter.CreateConfiguration());

            processor.CanProcess(FilterType.Histogram).Should().BeTrue();
            result.PixelData![0].Should().Be(0);
            result.PixelData[3].Should().Be(255);
        }
    }
}
