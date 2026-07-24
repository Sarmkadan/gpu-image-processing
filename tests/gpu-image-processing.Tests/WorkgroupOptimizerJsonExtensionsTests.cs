// tests/gpu-image-processing.Tests/WorkgroupOptimizerJsonExtensionsTests.cs
using Xunit;
using GpuImageProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;

namespace GpuImageProcessing.Tests.Pipeline
{
    public class WorkgroupOptimizerJsonExtensionsTests
    {
        private static WorkgroupOptimizer CreateOptimizer()
        {
            var loggerMock = new Mock<ILogger<WorkgroupOptimizer>>();
            return new WorkgroupOptimizer(loggerMock.Object, enableCache: false);
        }

        [Fact]
        public void ToJson_HappyPath_SerializesCorrectly()
        {
            // Arrange
            var optimizer = CreateOptimizer();

            // Act
            var json = optimizer.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
            Assert.Contains("workgroupSizeX", json);
            Assert.Contains("workgroupSizeY", json);
            Assert.Contains("strategy", json);
        }

        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            WorkgroupOptimizer? nullOptimizer = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullOptimizer!.ToJson());
        }

        [Fact]
        public void ToJson_IndentedParameter_ProducesFormattedJson()
        {
            // Act
            var optimizer = CreateOptimizer();
            var compactJson = optimizer.ToJson(indented: false);
            var formattedJson = optimizer.ToJson(indented: true);

            // Assert
            Assert.NotNull(compactJson);
            Assert.NotNull(formattedJson);
            Assert.DoesNotContain(Environment.NewLine, compactJson);
            Assert.Contains(Environment.NewLine, formattedJson);
        }

        [Fact]
        public void FromJson_HappyPath_DeserializesCorrectly()
        {
            // Arrange
            var originalJson = CreateOptimizer().ToJson();

            // Act
            var deserialized = WorkgroupOptimizerJsonExtensions.FromJson(originalJson);

            // Assert
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void FromJson_NullJson_ReturnsNull()
        {
            // Act
            var result = WorkgroupOptimizerJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var result = WorkgroupOptimizerJsonExtensions.FromJson(string.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_WhitespaceJson_ReturnsNull()
        {
            // Act
            var result = WorkgroupOptimizerJsonExtensions.FromJson(" ");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsException()
        {
            // Arrange
            var invalidJson = "{ invalid json {{";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => WorkgroupOptimizerJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndDeserializes()
        {
            // Arrange
            var json = CreateOptimizer().ToJson();

            // Act
            var success = WorkgroupOptimizerJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_NullJson_ReturnsFalseAndNull()
        {
            // Act
            var success = WorkgroupOptimizerJsonExtensions.TryFromJson(null, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalseAndNull()
        {
            // Act
            var success = WorkgroupOptimizerJsonExtensions.TryFromJson(string.Empty, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_WhitespaceJson_ReturnsFalseAndNull()
        {
            // Act
            var success1 = WorkgroupOptimizerJsonExtensions.TryFromJson(" ", out var result1);
            var success2 = WorkgroupOptimizerJsonExtensions.TryFromJson("\t", out var result2);
            var success3 = WorkgroupOptimizerJsonExtensions.TryFromJson("\n", out var result3);

            // Assert
            Assert.False(success1);
            Assert.Null(result1);

            Assert.False(success2);
            Assert.Null(result2);

            Assert.False(success3);
            Assert.Null(result3);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "{ invalid: json }";

            // Act
            var success = WorkgroupOptimizerJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void RoundTrip_SerializationDeserialization_PreservesObject()
        {
            // Arrange
            var originalJson = CreateOptimizer().ToJson();

            // Act
            var deserialized = WorkgroupOptimizerJsonExtensions.FromJson(originalJson);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(typeof(WorkgroupOptimizer), deserialized!.GetType());
        }
    }
}