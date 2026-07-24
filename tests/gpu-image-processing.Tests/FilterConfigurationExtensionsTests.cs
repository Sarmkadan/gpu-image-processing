// tests/gpu-image-processing.Tests/FilterConfigurationExtensionsTests.cs
using Xunit;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests
{
    public class FilterConfigurationExtensionsTests
    {
        [Fact]
        public void GetParameter_HappyPath_ReturnsParameterValue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test Filter",
                FilterType = FilterType.Blur
            };
            var paramValue = new TestParameter { Value = 42 };
            config.SetParameter("testKey", paramValue);

            // Act
            var result = config.GetParameter<TestParameter>("testKey", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void GetParameter_WithDefaultValue_ReturnsDefaultWhenNotFound()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test Filter",
                FilterType = FilterType.Blur
            };

            // Act
            var result = config.GetParameter("nonexistentKey", new TestParameter { Value = 99 });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99, result.Value);
        }

        [Fact]
        public void GetParameter_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((FilterConfiguration)null).GetParameter<TestParameter>("key", null));
        }

        [Fact]
        public void GetParameter_NullKey_ThrowsArgumentException()
        {
            // Arrange
            var config = new FilterConfiguration { Name = "Test", FilterType = FilterType.Blur };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => config.GetParameter<TestParameter>(null, null));
        }

        [Fact]
        public void SetParameter_HappyPath_SetsParameterWithTypeName()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test Filter",
                FilterType = FilterType.Blur
            };

            var paramValue = new TestParameter { Value = 123 };

            // Act
            config.SetParameter("myParam", paramValue);

            // Assert
            Assert.Single(config.Parameters);
            Assert.Equal(123, ((TestParameter)config.Parameters["myParam"]).Value);
            Assert.Equal(typeof(TestParameter).FullName, config.ParameterTypes["myParam"]);
        }

        [Fact]
        public void SetParameter_NullConfiguration_ThrowsArgumentNullException()
        {
            // Arrange
            var paramValue = new TestParameter { Value = 456 };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((FilterConfiguration)null).SetParameter("key", paramValue));
        }

        [Fact]
        public void SetParameter_NullKey_ThrowsArgumentException()
        {
            // Arrange
            var config = new FilterConfiguration { Name = "Test", FilterType = FilterType.Blur };
            var paramValue = new TestParameter { Value = 789 };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => config.SetParameter(null, paramValue));
        }

        [Fact]
        public void SetParameter_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            var config = new FilterConfiguration { Name = "Test", FilterType = FilterType.Blur };

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => FilterConfigurationExtensions.SetParameter(config, "key", (object)null));
        }

        [Fact]
        public void WithNewId_HappyPath_ReturnsNewConfigurationWithNewId()
        {
            // Arrange
            var originalConfig = new FilterConfiguration
            {
                Name = "Original Filter",
                FilterType = FilterType.GaussianBlur,
                IsActive = false,
                Priority = 10
            };
            var originalId = originalConfig.Id;
            var originalCreatedAt = originalConfig.CreatedAt;

            // Act
            var newConfig = originalConfig.WithNewId();

            // Assert
            Assert.NotEqual(originalId, newConfig.Id);
            Assert.NotEqual(originalCreatedAt, newConfig.CreatedAt);
            Assert.True(newConfig.ModifiedAt > originalCreatedAt);
            Assert.Equal(originalConfig.Name, newConfig.Name);
            Assert.Equal(originalConfig.FilterType, newConfig.FilterType);
            Assert.Equal(originalConfig.IsActive, newConfig.IsActive);
            Assert.Equal(originalConfig.Priority, newConfig.Priority);
        }

        [Fact]
        public void WithNewId_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((FilterConfiguration)null).WithNewId());
        }

        [Fact]
        public void WithNewId_CopiesParameters()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.SetParameter("radius", 5.5f);
            config.SetParameter("strength", 0.8f);

            // Act
            var newConfig = config.WithNewId();

            // Assert
            Assert.Equal(2, newConfig.Parameters.Count);
            Assert.Equal(5.5f, newConfig.Parameters["radius"]);
            Assert.Equal(0.8f, newConfig.Parameters["strength"]);
        }

        [Fact]
        public void GetConvolutionKernelSize_HappyPath_ReturnsKernelSize()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Convolution Filter",
                FilterType = FilterType.CustomConvolution
            };
            // 3x3 kernel
            config.ConvolutionKernel = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Act
            var result = config.GetConvolutionKernelSize();

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void GetConvolutionKernelSize_NotConvolutionFilter_ReturnsNull()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Blur Filter",
                FilterType = FilterType.Blur
            };

            // Act
            var result = config.GetConvolutionKernelSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetConvolutionKernelSize_NullKernel_ReturnsNull()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Convolution Filter",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = null
            };

            // Act
            var result = config.GetConvolutionKernelSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetConvolutionKernelSize_EmptyKernel_ReturnsNull()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Convolution Filter",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = Array.Empty<float>()
            };

            // Act
            var result = config.GetConvolutionKernelSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetConvolutionKernelSize_EvenSizeKernel_ReturnsNull()
        {
            // Arrange - 4x4 kernel (even size, should be invalid)
            var config = new FilterConfiguration
            {
                Name = "Invalid Convolution Filter",
                FilterType = FilterType.CustomConvolution,
                ConvolutionKernel = new float[16] // 4x4
            };

            // Act
            var result = config.GetConvolutionKernelSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetConvolutionKernelSize_LargeKernel_ReturnsCorrectSize()
        {
            // Arrange - 7x7 kernel
            var config = new FilterConfiguration
            {
                Name = "Large Convolution Filter",
                FilterType = FilterType.CustomConvolution
            };
            config.ConvolutionKernel = new float[49]; // 7x7

            // Act
            var result = config.GetConvolutionKernelSize();

            // Assert
            Assert.Equal(7, result);
        }

        [Fact]
        public void GetConvolutionKernelSize_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((FilterConfiguration)null).GetConvolutionKernelSize());
        }

        [Fact]
        public void IsConvolutionFilter_HappyPath_ConvolutionFilter_ReturnsTrue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Convolution Filter",
                FilterType = FilterType.CustomConvolution
            };

            // Act
            var result = config.IsConvolutionFilter();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsConvolutionFilter_HappyPath_BlurFilter_ReturnsTrue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Blur Filter",
                FilterType = FilterType.Blur
            };

            // Act
            var result = config.IsConvolutionFilter();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsConvolutionFilter_HappyPath_NonConvolutionFilter_ReturnsFalse()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Color Correction",
                FilterType = FilterType.ColorCorrection
            };

            // Act
            var result = config.IsConvolutionFilter();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsConvolutionFilter_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((FilterConfiguration)null).IsConvolutionFilter());
        }

        [Fact]
        public void GetNormalizedParameter_HappyPath_FloatValue_ReturnsClampedValue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.SetParameter("intensity", 0.75f);

            // Act
            var result = config.GetNormalizedParameter("intensity");

            // Assert
            Assert.Equal(0.75f, result);
        }

        [Fact]
        public void GetNormalizedParameter_HappyPath_IntValue_ReturnsNormalizedValue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.SetParameter("intensity", 75); // Should normalize to 0.75

            // Act
            var result = config.GetNormalizedParameter("intensity");

            // Assert
            Assert.Equal(0.75f, result);
        }

        [Fact]
        public void GetNormalizedParameter_HappyPath_DoubleValue_ReturnsClampedValue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.SetParameter("intensity", 0.85);

            // Act
            var result = config.GetNormalizedParameter("intensity");

            // Assert
            Assert.Equal(0.85f, result);
        }

        [Fact]
        public void GetNormalizedParameter_HappyPath_StringValue_ReturnsParsedValue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.SetParameter("intensity", "0.65");

            // Act
            var result = config.GetNormalizedParameter("intensity");

            // Assert
            Assert.Equal(0.65f, result);
        }

        [Fact]
        public void GetNormalizedParameter_OutOfRangeValues_ReturnsClamped()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };

            // Test values outside [0, 1] range
            config.SetParameter("tooHigh", 1.5f);
            config.SetParameter("tooLow", -0.5f);
            config.SetParameter("tooHighInt", 150);
            config.SetParameter("tooLowInt", -50);

            // Act
            var highResult = config.GetNormalizedParameter("tooHigh");
            var lowResult = config.GetNormalizedParameter("tooLow");
            var highIntResult = config.GetNormalizedParameter("tooHighInt");
            var lowIntResult = config.GetNormalizedParameter("tooLowInt");

            // Assert
            Assert.Equal(1.0f, highResult);
            Assert.Equal(0.0f, lowResult);
            Assert.Equal(1.0f, highIntResult);
            Assert.Equal(0.0f, lowIntResult);
        }

        [Fact]
        public void GetNormalizedParameter_DefaultValue_ReturnsDefaultWhenNotFound()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };

            // Act
            var result = config.GetNormalizedParameter("nonexistent", 0.25f);

            // Assert
            Assert.Equal(0.25f, result);
        }

        [Fact]
        public void GetNormalizedParameter_NullConfiguration_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((FilterConfiguration)null).GetNormalizedParameter("key"));
        }

        [Fact]
        public void GetNormalizedParameter_NullKey_ThrowsArgumentException()
        {
            // Arrange
            var config = new FilterConfiguration { Name = "Test", FilterType = FilterType.Blur };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => config.GetNormalizedParameter(null));
        }

        [Fact]
        public void GetNormalizedParameter_UnsupportedType_ReturnsDefaultValue()
        {
            // Arrange
            var config = new FilterConfiguration
            {
                Name = "Test",
                FilterType = FilterType.Blur
            };
            config.SetParameter("unsupported", new object());

            // Act
            var result = config.GetNormalizedParameter("unsupported");

            // Assert
            Assert.Equal(0.5f, result); // Default value
        }

        // Simple test parameter class for testing GetParameter/SetParameter
        private class TestParameter
        {
            public int Value { get; set; }
        }
    }
}