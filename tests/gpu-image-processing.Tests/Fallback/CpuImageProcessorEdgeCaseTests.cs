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
using GpuImageProcessing.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuImageProcessing.Tests.Fallback
{
    /// <summary>
    /// Edge-case tests for CpuImageProcessor focusing on boundary conditions,
    /// extreme values, and null argument handling.
    /// </summary>
    public sealed class CpuImageProcessorEdgeCaseTests
    {
        private static CpuImageProcessor NewProcessor() =>
            new(NullLogger<CpuImageProcessor>.Instance);

        private static Image CreateTestImage(int width, int height, int channels = 3, byte value = 128)
        {
            var img = new Image
            {
                Width = width,
                Height = height,
                Channels = channels,
                BitsPerPixel = channels * 8,
                PixelData = new byte[width * height * channels]
            };
            Array.Fill(img.PixelData!, value);
            return img;
        }

        // =====================================================================
        // 1x1 Image Tests
        // =====================================================================

        [Fact]
        public void Resize_1x1_image_to_1x1_preserves_dimensions()
        {
            var image = CreateTestImage(1, 1, 3, 255);
            var processor = NewProcessor();

            var resized = processor.Resize(image, 1, 1);

            resized.Width.Should().Be(1);
            resized.Height.Should().Be(1);
            resized.PixelData.Should().NotBeNull();
            resized.PixelData!.Length.Should().Be(3);
        }

        [Fact]
        public void Resize_1x1_image_to_larger_dimensions_works()
        {
            var image = CreateTestImage(1, 1, 3, 100);
            var processor = NewProcessor();

            var resized = processor.Resize(image, 10, 10);

            resized.Width.Should().Be(10);
            resized.Height.Should().Be(10);
            resized.PixelData.Should().NotBeNull();
            resized.PixelData!.Length.Should().Be(10 * 10 * 3);
        }

        [Fact]
        public void Resize_1x1_image_to_smaller_dimensions_works()
        {
            var image = CreateTestImage(1, 1, 3, 50);
            var processor = NewProcessor();

            var resized = processor.Resize(image, 1, 1);

            resized.Width.Should().Be(1);
            resized.Height.Should().Be(1);
        }

        [Fact]
        public async Task ApplyFilterAsync_1x1_image_with_Grayscale()
        {
            var image = CreateTestImage(1, 1, 3, 200);
            var config = new FilterConfiguration
            {
                Name = "grayscale-1x1",
                FilterType = FilterType.Grayscale
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(3);
            result.Width.Should().Be(1);
            result.Height.Should().Be(1);
        }

        [Fact]
        public async Task ApplyFilterAsync_1x1_image_with_Blur()
        {
            var image = CreateTestImage(1, 1, 3, 150);
            var config = new FilterConfiguration
            {
                Name = "blur-1x1",
                FilterType = FilterType.Blur,
                Parameters = { ["radius"] = 5 }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
        }

        [Fact]
        public async Task ApplyFilterAsync_1x1_image_with_Threshold()
        {
            var image = CreateTestImage(1, 1, 3, 128);
            var config = new FilterConfiguration
            {
                Name = "threshold-1x1",
                FilterType = FilterType.Threshold,
                Parameters = { ["thresholdValue"] = 0.5f }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            result.PixelData![0].Should().Be(255); // 128 >= 127.5 (0.5 * 255)
        }

        // =====================================================================
        // Empty Filter Chain Tests
        // =====================================================================

        [Fact]
        public void Resize_with_zero_width_throws_ArgumentOutOfRangeException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Resize(image, 0, 10);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*targetWidth*");
        }

        [Fact]
        public void Resize_with_zero_height_throws_ArgumentOutOfRangeException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Resize(image, 10, 0);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*targetHeight*");
        }

        [Fact]
        public void Resize_with_negative_width_throws_ArgumentOutOfRangeException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Resize(image, -1, 10);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*targetWidth*");
        }

        [Fact]
        public void Resize_with_negative_height_throws_ArgumentOutOfRangeException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Resize(image, 10, -1);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*targetHeight*");
        }

        // =====================================================================
        // Extreme Filter Parameter Values
        // =====================================================================

        [Fact]
        public async Task ApplyFilterAsync_Blur_with_extreme_radius()
        {
            var image = CreateTestImage(20, 20);
            var config = new FilterConfiguration
            {
                Name = "blur-extreme-radius",
                FilterType = FilterType.Blur,
                Parameters = { ["radius"] = 100 }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
        }

        [Fact]
        public async Task ApplyFilterAsync_GaussianBlur_with_extreme_radius()
        {
            var image = CreateTestImage(20, 20);
            var config = new FilterConfiguration
            {
                Name = "gaussian-extreme-radius",
                FilterType = FilterType.GaussianBlur,
                Parameters = { ["radius"] = 100 }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
        }

        [Fact]
        public async Task ApplyFilterAsync_Threshold_with_zero_threshold()
        {
            var image = CreateTestImage(10, 10, 3, 0);
            var config = new FilterConfiguration
            {
                Name = "threshold-zero",
                FilterType = FilterType.Threshold,
                Parameters = { ["thresholdValue"] = 0.0f }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            // All pixels should be 255 (0 >= 0)
            result.PixelData.Should().AllBeEquivalentTo(255);
        }

        [Fact]
        public async Task ApplyFilterAsync_Threshold_with_max_threshold()
        {
            var image = CreateTestImage(10, 10, 3, 255);
            var config = new FilterConfiguration
            {
                Name = "threshold-max",
                FilterType = FilterType.Threshold,
                Parameters = { ["thresholdValue"] = 1.0f }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            // All pixels should be 255 (255 >= 255)
            result.PixelData.Should().AllBeEquivalentTo(255);
        }

        [Fact]
        public async Task ApplyFilterAsync_Threshold_with_negative_threshold_clamped_to_zero()
        {
            var image = CreateTestImage(10, 10, 3, 128);
            var config = new FilterConfiguration
            {
                Name = "threshold-negative",
                FilterType = FilterType.Threshold,
                Parameters = { ["thresholdValue"] = -0.5f }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            // Clamped to 0, so all pixels should be 255 (128 >= 0)
            result.PixelData.Should().AllBeEquivalentTo(255);
        }

        [Fact]
        public async Task ApplyFilterAsync_Threshold_with_above_max_threshold_clamped_to_255()
        {
            var image = CreateTestImage(10, 10, 3, 128);
            var config = new FilterConfiguration
            {
                Name = "threshold-above-max",
                FilterType = FilterType.Threshold,
                Parameters = { ["thresholdValue"] = 2.0f }
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            // Clamped to 255, so all pixels should be 0 (128 >= 255 is false)
            result.PixelData.Should().AllBeEquivalentTo(0);
        }

        [Fact]
        public void Blur_with_zero_radius()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            var result = processor.Blur(image, 0);

            result.PixelData.Should().NotBeNull();
        }

        [Fact]
        public void Blur_with_large_radius()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            var result = processor.Blur(image, 100);

            result.PixelData.Should().NotBeNull();
        }

        [Fact]
        public void ToGrayscale_with_grayscale_color_space()
        {
            var image = new Image
            {
                Width = 5,
                Height = 5,
                Channels = 3,
                BitsPerPixel = 24,
                ColorSpace = ColorSpace.Grayscale,
                PixelData = new byte[75]
            };
            Array.Fill(image.PixelData, (byte)128);

            var result = NewProcessor().ToGrayscale(image);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(75);
        }

        [Fact]
        public void ToGrayscale_with_four_channel_image()
        {
            var image = new Image
            {
                Width = 5,
                Height = 5,
                Channels = 4,
                BitsPerPixel = 32,
                PixelData = new byte[100]
            };
            Array.Fill(image.PixelData, (byte)128);

            var result = NewProcessor().ToGrayscale(image);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(100);
            // Alpha channel should be preserved
            for (int i = 3; i < 100; i += 4)
            {
                result.PixelData[i].Should().Be(128);
            }
        }

        // =====================================================================
        // Null Argument Handling Tests
        // =====================================================================

        [Fact]
        public void Resize_with_null_image_throws_ArgumentNullException()
        {
            var processor = NewProcessor();

            Action act = () => processor.Resize(null!, 10, 10);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToGrayscale_with_null_image_throws_ArgumentNullException()
        {
            var processor = NewProcessor();

            Action act = () => processor.ToGrayscale(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Blur_with_null_image_throws_ArgumentNullException()
        {
            var processor = NewProcessor();

            Action act = () => processor.Blur(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Crop_with_null_image_throws_ArgumentNullException()
        {
            var processor = NewProcessor();

            Action act = () => processor.Crop(null!, 0, 0, 10, 10);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task ApplyFilterAsync_with_null_image_throws_ArgumentNullException()
        {
            var config = new FilterConfiguration
            {
                Name = "test",
                FilterType = FilterType.Grayscale
            };

            var act = async () => await NewProcessor().ApplyFilterAsync(null!, config);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ApplyFilterAsync_with_null_config_throws_ArgumentNullException()
        {
            var image = CreateTestImage(10, 10);

            var act = async () => await NewProcessor().ApplyFilterAsync(image, null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ApplyFilterAsync_with_null_image_and_null_config_throws_ArgumentNullException()
        {
            var act = async () => await NewProcessor().ApplyFilterAsync(null!, null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        // =====================================================================
        // Unsupported Filter Type Tests
        // =====================================================================

        [Fact]
        public async Task ApplyFilterAsync_with_unsupported_filter_type_does_not_throw()
        {
            var image = CreateTestImage(10, 10);
            var config = new FilterConfiguration
            {
                Name = "unsupported",
                FilterType = FilterType.Rotation // Not in _supported set
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            // Should return original pixel data unchanged
            result.PixelData.Should().BeSameAs(image.PixelData);
        }

        [Fact]
        public async Task ApplyFilterAsync_with_None_filter_type_returns_original_data()
        {
            var image = CreateTestImage(10, 10);
            var originalData = image.PixelData;
            var config = new FilterConfiguration
            {
                Name = "none",
                FilterType = FilterType.None
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().BeSameAs(originalData);
        }

        // =====================================================================
        // Image with Null PixelData Tests
        // =====================================================================

        [Fact]
        public void Resize_image_with_null_PixelData()
        {
            var image = new Image
            {
                Width = 10,
                Height = 10,
                Channels = 3,
                BitsPerPixel = 24,
                PixelData = null
            };

            var result = NewProcessor().Resize(image, 5, 5);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(5 * 5 * 3);
        }

        [Fact]
        public void ToGrayscale_image_with_null_PixelData()
        {
            var image = new Image
            {
                Width = 10,
                Height = 10,
                Channels = 3,
                BitsPerPixel = 24,
                PixelData = null
            };

            var result = NewProcessor().ToGrayscale(image);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(10 * 10 * 3);
        }

        [Fact]
        public void Blur_image_with_null_PixelData()
        {
            var image = new Image
            {
                Width = 10,
                Height = 10,
                Channels = 3,
                BitsPerPixel = 24,
                PixelData = null
            };

            var result = NewProcessor().Blur(image);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(10 * 10 * 3);
        }

        [Fact]
        public void Crop_image_with_null_PixelData()
        {
            var image = new Image
            {
                Width = 10,
                Height = 10,
                Channels = 3,
                BitsPerPixel = 24,
                PixelData = null
            };

            var result = NewProcessor().Crop(image, 2, 2, 5, 5);

            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(5 * 5 * 3);
        }

        // =====================================================================
        // Edge Cases for Crop Method
        // =====================================================================

        [Fact]
        public void Crop_with_zero_width_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 0, 0, 0, 10);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_zero_height_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 0, 0, 10, 0);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_negative_x_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, -1, 0, 5, 5);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_negative_y_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 0, -1, 5, 5);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_x_beyond_image_width_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 15, 0, 5, 5);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_y_beyond_image_height_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 0, 15, 5, 5);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_width_exceeding_available_space_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 8, 0, 5, 5);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_with_height_exceeding_available_space_throws_ValidationException()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            Action act = () => processor.Crop(image, 0, 8, 5, 5);

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Crop_full_image_returns_same_dimensions()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            var result = processor.Crop(image, 0, 0, 10, 10);

            result.Width.Should().Be(10);
            result.Height.Should().Be(10);
        }

        [Fact]
        public void Crop_single_pixel()
        {
            var image = CreateTestImage(10, 10);
            var processor = NewProcessor();

            var result = processor.Crop(image, 5, 5, 1, 1);

            result.Width.Should().Be(1);
            result.Height.Should().Be(1);
            result.PixelData.Should().NotBeNull();
            result.PixelData!.Length.Should().Be(3);
        }

        // =====================================================================
        // Large Image Tests (stress test boundaries)
        // =====================================================================

        [Fact]
        public async Task ApplyFilterAsync_with_large_image()
        {
            // 1000x1000 image = 3MB of data
            var image = CreateTestImage(1000, 1000);
            var config = new FilterConfiguration
            {
                Name = "grayscale-large",
                FilterType = FilterType.Grayscale
            };

            var result = await NewProcessor().ApplyFilterAsync(image, config);

            result.PixelData.Should().NotBeNull();
            result.Width.Should().Be(1000);
            result.Height.Should().Be(1000);
        }

        [Fact]
        public void Resize_very_small_image()
        {
            var image = CreateTestImage(2, 2);
            var processor = NewProcessor();

            var result = processor.Resize(image, 1, 1);

            result.Width.Should().Be(1);
            result.Height.Should().Be(1);
        }

        // =====================================================================
        // Different Color Spaces
        // =====================================================================

        [Fact]
        public void ToGrayscale_with_Rgba_color_space()
        {
            var image = new Image
            {
                Width = 5,
                Height = 5,
                Channels = 4,
                BitsPerPixel = 32,
                ColorSpace = ColorSpace.Rgba,
                PixelData = new byte[100]
            };
            Array.Fill(image.PixelData, (byte)128);

            var result = NewProcessor().ToGrayscale(image);

            result.PixelData.Should().NotBeNull();
        }

        [Fact]
        public void ToGrayscale_with_Bgra_color_space()
        {
            var image = new Image
            {
                Width = 5,
                Height = 5,
                Channels = 4,
                BitsPerPixel = 32,
                ColorSpace = ColorSpace.Bgra,
                PixelData = new byte[100]
            };
            Array.Fill(image.PixelData, (byte)128);

            var result = NewProcessor().ToGrayscale(image);

            result.PixelData.Should().NotBeNull();
        }
    }
}
