using System;
using System.Text.Json;
using Xunit;
using GpuImageProcessing.Configuration;

namespace GpuImageProcessing.Tests.Configuration
{
    public class ComputeShaderPipelineOptionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidObject_ReturnsJsonString()
        {
            // Arrange
            var options = new ComputeShaderPipelineOptions();

            // Act
            var json = options.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
        }

        [Fact]
        public void ToJson_Indented_ReturnsFormattedJson()
        {
            // Arrange
            var options = new ComputeShaderPipelineOptions();

            // Act
            var json = options.ToJson(indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\n", json);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            ComputeShaderPipelineOptions? options = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => options!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsObject()
        {
            // Arrange
            var json = "{}";

            // Act
            var result = ComputeShaderPipelineOptionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ComputeShaderPipelineOptionsJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ComputeShaderPipelineOptionsJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{ not valid json }";

            // Act & Assert
            Assert.Throws<JsonException>(() => ComputeShaderPipelineOptionsJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndObject()
        {
            // Arrange
            var json = "{}";

            // Act
            var result = ComputeShaderPipelineOptionsJsonExtensions.TryFromJson(json, out var options);

            // Assert
            Assert.True(result);
            Assert.NotNull(options);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{ not valid json }";

            // Act
            var result = ComputeShaderPipelineOptionsJsonExtensions.TryFromJson(invalidJson, out var options);

            // Assert
            Assert.False(result);
            Assert.Null(options);
        }

        [Fact]
        public void TryFromJson_NullInput_ThrowsArgumentException()
        {
            // Act & Assert
            // The implementation explicitly throws ArgumentException for null/empty strings
            Assert.Throws<ArgumentException>(() => ComputeShaderPipelineOptionsJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
