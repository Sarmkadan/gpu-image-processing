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
using GpuImageProcessing.Imaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuImageProcessing.Tests.Golden
{
    /// <summary>
    /// Golden-image regression tests. Each case loads a checked-in fixture image
    /// from <c>tests/fixtures</c>, runs it through the CPU fallback processor, and
    /// asserts the SHA-256 hash of the output pixels matches a pinned reference.
    ///
    /// Because the CPU path is fully deterministic and requires no GPU/OpenCL
    /// device, these tests keep CI green on hardware without a compatible GPU. A
    /// single-byte change in any filter kernel breaks the matching hash, which is
    /// exactly the regression signal we want.
    /// </summary>
    public sealed class GoldenImageRegressionTests
    {
        private static string FixtureDir =>
            System.IO.Path.Combine(AppContext.BaseDirectory, "fixtures");

        private static CpuImageProcessor NewProcessor() =>
            new(NullLogger<CpuImageProcessor>.Instance);

        public static IEnumerable<object[]> GoldenCases()
        {
            yield return ["gradient_rgb.ppm", FilterType.Grayscale, 0.5f,
                "51a117aa0aef78c2c364705586b2d5a758f3b784ff11c1656c725a697331e17d"];
            yield return ["gradient_rgb.ppm", FilterType.Blur, 0.5f,
                "d7fccbe714ab0ddbbb0dc929e00546cd8e6a41d2250261b148f5b01d042960bb"];
            yield return ["gradient_rgb.ppm", FilterType.Sharpen, 0.5f,
                "95397efce3ac89bad7ac8e9c8788f75ac5faf57b3ea7802d6cbf876e1f962424"];
            yield return ["gradient_rgb.ppm", FilterType.EdgeDetection, 0.5f,
                "0a27e50a60f3d0fb54e016cf4fb3bb69a43a4e5b7c0223705d5f1a18ed555b82"];
            yield return ["checker_gray.pgm", FilterType.Threshold, 0.5f,
                "190d2def02d171d21950d654cadb79b47934f105174e0f8097b916380d0f362c"];
            yield return ["checker_gray.pgm", FilterType.Blur, 0.5f,
                "ffdc4f47d0fe937e5ebb74fd086eb794437808a8cf2c0c714deb13d2f4ea37fe"];
        }

        [Theory]
        [MemberData(nameof(GoldenCases))]
        public async Task Filter_output_matches_golden_hash(
            string fixture, FilterType filterType, float threshold, string expectedHash)
        {
            var image = PortablePixmap.Load(System.IO.Path.Combine(FixtureDir, fixture));
            var config = new FilterConfiguration
            {
                Name = filterType.ToString(),
                FilterType = filterType,
                Parameters = { ["thresholdValue"] = threshold }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            PortablePixmap.PixelHash(result).Should().Be(
                expectedHash,
                $"filter {filterType} on {fixture} must produce byte-identical output");
        }

        [Fact]
        public void Fixtures_are_present_and_decodable()
        {
            foreach (var fixture in new[] { "gradient_rgb.ppm", "checker_gray.pgm" })
            {
                var path = System.IO.Path.Combine(FixtureDir, fixture);
                File.Exists(path).Should().BeTrue($"fixture {fixture} must ship with the test project");

                var image = PortablePixmap.Load(path);
                image.PixelData.Should().NotBeNull();
                image.Width.Should().BeGreaterThan(0);
                image.Height.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public void Golden_hashes_are_deterministic_across_runs()
        {
            var image = PortablePixmap.Load(System.IO.Path.Combine(FixtureDir, "gradient_rgb.ppm"));
            string first = PortablePixmap.PixelHash(image);
            string second = PortablePixmap.PixelHash(PortablePixmap.Load(
                System.IO.Path.Combine(FixtureDir, "gradient_rgb.ppm")));
            first.Should().Be(second);
        }
    }
}
