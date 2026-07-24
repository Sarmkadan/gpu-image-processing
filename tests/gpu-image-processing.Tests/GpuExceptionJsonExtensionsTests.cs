using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Tests
{
    public class GpuExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var gpuException = new GpuException("Test message");

            // Act
            var json = GpuExceptionJsonExtensions.ToJson(gpuException);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{\"Message\":\"", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => GpuExceptionJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsGpuException()
        {
            // Arrange
            var json = "{\"Message\":\"Test message\"}";

            // Act
            var gpuException = GpuExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(gpuException);
            Assert.Equal("Test message", gpuException.Message);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var gpuException = GpuExceptionJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(gpuException);
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var gpuException = GpuExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(gpuException);
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"Message\":\"Test message\"}";

            // Act
            var success = GpuExceptionJsonExtensions.TryFromJson(json, out var gpuException);

            // Assert
            Assert.True(success);
            Assert.NotNull(gpuException);
            Assert.Equal("Test message", gpuException.Message);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = GpuExceptionJsonExtensions.TryFromJson(null, out var gpuException);

            // Assert
            Assert.False(success);
            Assert.Null(gpuException);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var success = GpuExceptionJsonExtensions.TryFromJson("", out var gpuException);

            // Assert
            Assert.False(success);
            Assert.Null(gpuException);
        }
    }
}
