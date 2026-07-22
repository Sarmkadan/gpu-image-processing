#nullable enable
using System;
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests.Domain
{
    public class FilterConfigurationTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesWithDefaults()
        {
            // Act
            var config = new FilterConfiguration();

            // Assert
            config.Id.Should().NotBe(Guid.Empty);
            config.Name.Should().BeEmpty();
            config.FilterType.Should().Be(FilterType.None);
            config.Description.Should().BeEmpty();
            config.Parameters.Should().NotBeNull().And.BeEmpty();
            config.ParameterTypes.Should().NotBeNull().And.BeEmpty();
            config.IsActive.Should().BeTrue();
            config.Priority.Should().Be(0);
            config.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            config.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            config.MaxThreadsPerBlock.Should().Be(256);
            config.NormalizeKernel.Should().BeFalse();
            config.ConvolutionKernel.Should().BeNull();
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new FilterConfiguration
            {
                Name = "Test Filter",
                FilterType = FilterType.Blur,
                Description = "Test description",
                IsActive = false,
                Priority = 5,
                MaxThreadsPerBlock = 512,
                NormalizeKernel = true,
                ConvolutionKernel = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
            };
            original.SetParameter("radius", 2.5f, "float");

            // Act
            var clone = original.Clone();

            // Assert - values are copied
            clone.Name.Should().Be(original.Name);
            clone.FilterType.Should().Be(original.FilterType);
            clone.Description.Should().Be(original.Description);
            clone.IsActive.Should().Be(original.IsActive);
            clone.Priority.Should().Be(original.Priority);
            clone.MaxThreadsPerBlock.Should().Be(original.MaxThreadsPerBlock);
            clone.NormalizeKernel.Should().Be(original.NormalizeKernel);
            clone.ConvolutionKernel.Should().BeEquivalentTo(original.ConvolutionKernel);

            // Assert - collections are independent
            clone.Parameters.Should().NotBeSameAs(original.Parameters);
            clone.Parameters.Should().BeEquivalentTo(original.Parameters);
            clone.ParameterTypes.Should().NotBeSameAs(original.ParameterTypes);
            clone.ParameterTypes.Should().BeEquivalentTo(original.ParameterTypes);

            // Assert - convolution kernel is cloned
            clone.ConvolutionKernel.Should().NotBeSameAs(original.ConvolutionKernel);
        }

        [Fact]
        public void SetParameter_AddsParameterAndType()
        {
            // Arrange
            var config = new FilterConfiguration();

            // Act
            config.SetParameter("radius", 2.5f, "float");
            config.SetParameter("threshold", 0.75f, "float");

            // Assert
            config.Parameters.Should().HaveCount(2);
            config.Parameters["radius"].Should().Be(2.5f);
            config.Parameters["threshold"].Should().Be(0.75f);
            config.ParameterTypes.Should().HaveCount(2);
            config.ParameterTypes["radius"].Should().Be("float");
            config.ParameterTypes["threshold"].Should().Be("float");
            config.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GetParameter_ReturnsCorrectType_WhenParameterExists()
        {
            // Arrange
            var config = new FilterConfiguration();
            config.SetParameter("brightness", 0.5f, "float");
            config.SetParameter("enabled", true, "bool");
            config.SetParameter("name", "test", "string");

            // Act
            var brightness = config.GetParameter<object>("brightness") as float?;
            var enabled = config.GetParameter<object>("enabled") as bool?;
            var name = config.GetParameter<string>("name");
            var missing = config.GetParameter<object>("missing");

            // Assert
            brightness.Should().Be(0.5f);
            enabled.Should().BeTrue();
            name.Should().Be("test");
            missing.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ReturnsFalse_WhenNameIsInvalid(string? invalidName)
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = invalidName!,
                FilterType = FilterType.Blur
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ReturnsFalse_WhenFilterTypeIsNone()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.None
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(31)]
        [InlineData(1025)]
        public void Validate_ReturnsFalse_WhenMaxThreadsPerBlockIsOutOfRange(int invalidThreads)
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur,
                MaxThreadsPerBlock = invalidThreads
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ReturnsFalse_WhenParameterTypeMissing()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.Parameters["radius"] = 2.5f;

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(FilterType.Blur, "radius", 0.5f, true)]
        [InlineData(FilterType.Blur, "radius", 0.1f, false)]
        [InlineData(FilterType.Blur, "radius", 50.1f, false)]
        [InlineData(FilterType.Sharpen, "strength", 5.0f, true)]
        [InlineData(FilterType.Sharpen, "strength", -0.1f, false)]
        [InlineData(FilterType.Sharpen, "strength", 10.1f, false)]
        [InlineData(FilterType.Rotation, "angle", 45.0f, true)]
        [InlineData(FilterType.Rotation, "angle", -361f, false)]
        [InlineData(FilterType.Rotation, "angle", 361f, false)]
        [InlineData(FilterType.Scaling, "scaleX", 1.0f, true)]
        [InlineData(FilterType.Scaling, "scaleX", 0.0f, false)]
        [InlineData(FilterType.ColorCorrection, "brightness", 0.0f, true)]
        [InlineData(FilterType.ColorCorrection, "brightness", -1.1f, false)]
        [InlineData(FilterType.ColorCorrection, "brightness", 1.1f, false)]
        [InlineData(FilterType.Threshold, "thresholdValue", 0.5f, true)]
        [InlineData(FilterType.Threshold, "thresholdValue", -0.1f, false)]
        [InlineData(FilterType.Threshold, "thresholdValue", 1.1f, false)]
        public void Validate_ReturnsExpectedResult_ForFilterTypeParameters(
            FilterType filterType, string paramName, float paramValue, bool expectedValid)
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = filterType
            };

            // Special handling for Scaling which requires both scaleX and scaleY
            if (filterType == FilterType.Scaling)
            {
                config.SetParameter("scaleX", paramValue, "float");
                config.SetParameter("scaleY", paramValue, "float");
            }
            else
            {
                config.SetParameter(paramName, paramValue, "float");
            }

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().Be(expectedValid);
        }

        [Fact]
        public void Validate_ReturnsTrue_WhenConvolutionKernelIsValid()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 } // 3x3
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new float[0])]
        public void Validate_ReturnsFalse_WhenConvolutionKernelIsInvalid(float[]? invalidKernel)
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = invalidKernel
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(2)]  // 2x2 = 4 elements
        [InlineData(4)]  // 4x4 = 16 elements
        public void Validate_ReturnsFalse_WhenConvolutionKernelSizeIsNotOddSquare(int sideLength)
        {
            // Arrange
            int kernelLength = sideLength * sideLength;
            var kernel = new float[kernelLength];
            for (int i = 0; i < kernelLength; i++)
            {
                kernel[i] = i + 1;
            }

            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = kernel
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(2)]  // Too small
        [InlineData(17)] // Too large
        public void Validate_ReturnsFalse_WhenConvolutionKernelSizeIsOutOfRange(int sideLength)
        {
            // Arrange
            int kernelLength = sideLength * sideLength;
            var kernel = new float[kernelLength];
            for (int i = 0; i < kernelLength; i++)
            {
                kernel[i] = i + 1;
            }

            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = kernel
            };

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_ReturnsTrue_WhenAllRequiredParametersAreValid()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Valid Filter",
                FilterType = FilterType.Blur,
                MaxThreadsPerBlock = 128
            };
            config.SetParameter("radius", 2.5f, "float");

            // Act
            var isValid = config.Validate();

            // Assert
            isValid.Should().BeTrue();
        }
    }
}
