using System;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests
{
    public class ImageBatchJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidImageBatch_ReturnsJsonString()
        {
            // Arrange
            var batch = new ImageBatch();

            // Act
            var json = batch.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
        }

        [Fact]
        public void ToJson_WithNullArgument_ThrowsArgumentNullException()
        {
            // Arrange
            ImageBatch? batch = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => batch!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsImageBatch()
        {
            // Arrange
            const string json = "{}";

            // Act
            var result = ImageBatchJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_WithNullOrEmpty_ThrowsArgumentException()
        {
            // Null
            Assert.Throws<ArgumentException>(() => ImageBatchJsonExtensions.FromJson(null!));

            // Empty
            Assert.Throws<ArgumentException>(() => ImageBatchJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void FromJson_WithWhitespace_ReturnsNull()
        {
            // Arrange
            const string json = "   ";

            // Act
            var result = ImageBatchJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndInstance()
        {
            // Arrange
            const string json = "{}";

            // Act
            var success = ImageBatchJsonExtensions.TryFromJson(json, out var batch);

            // Assert
            Assert.True(success);
            Assert.NotNull(batch);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalse()
        {
            // Arrange
            const string json = "{ invalid json }";

            // Act
            var success = ImageBatchJsonExtensions.TryFromJson(json, out var batch);

            // Assert
            Assert.False(success);
            Assert.Null(batch);
        }

        [Fact]
        public void TryFromJson_WithNullOrEmpty_ThrowsArgumentException()
        {
            // Null
            Assert.Throws<ArgumentException>(() => ImageBatchJsonExtensions.TryFromJson(null!, out _));

            // Empty
            Assert.Throws<ArgumentException>(() => ImageBatchJsonExtensions.TryFromJson(string.Empty, out _));
        }
    }
}
