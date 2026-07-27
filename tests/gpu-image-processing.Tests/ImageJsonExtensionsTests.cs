using Xunit;
using GpuImageProcessing.Domain;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests
{
    public class ImageJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var image = new Image();
            var expectedJson = "{\"id\":\"00000000-0000-0000-0000-000000000000\",\"width\":0,\"height\":0,\"format\":\"RGB\",\"data\":null}";

            // Act
            var actualJson = ImageJsonExtensions.ToJson(image);

            // Assert
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void ToJson_NullImage_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ImageJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsImage()
        {
            // Arrange
            var json = "{\"id\":\"00000000-0000-0000-0000-000000000000\",\"width\":0,\"height\":0,\"format\":\"RGB\",\"data\":null}";
            var expectedImage = new Image();

            // Act
            var actualImage = ImageJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(expectedImage, actualImage);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ImageJsonExtensions.FromJson(null));
        }

        [Fact]
        public void FromJson_EmptyJson_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ImageJsonExtensions.FromJson(""));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Act and Assert
            Assert.Throws<JsonException>(() => ImageJsonExtensions.FromJson("Invalid JSON"));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrue()
        {
            // Arrange
            var json = "{\"id\":\"00000000-0000-0000-0000-000000000000\",\"width\":0,\"height\":0,\"format\":\"RGB\",\"data\":null}";
            var expectedImage = new Image();

            // Act
            var actualResult = ImageJsonExtensions.TryFromJson(json, out var actualImage);

            // Assert
            Assert.True(actualResult);
            Assert.Equal(expectedImage, actualImage);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalse()
        {
            // Act and Assert
            var actualResult = ImageJsonExtensions.TryFromJson(null, out _);
            Assert.False(actualResult);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act and Assert
            var actualResult = ImageJsonExtensions.TryFromJson("", out _);
            Assert.False(actualResult);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Act and Assert
            var actualResult = ImageJsonExtensions.TryFromJson("Invalid JSON", out _);
            Assert.False(actualResult);
        }
    }
}
