using System;
using System.Text.Json;
using Xunit;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests
{
    public class SimdCapabilitiesExtensionsJsonExtensionsTests
    {
        private const string DefaultJson = 
            "{\"isVectorWidthSupportEnabled\":true,\"isOptimalSimdLevelEnabled\":true,\"isSimdAvailabilityEnabled\":true,\"isFriendlyStringEnabled\":true}";

        [Fact]
        public void ToJson_HappyPath_ReturnsExpectedJson()
        {
            // Arrange
            var config = new SimdCapabilitiesExtensionsJsonExtensions.SimdCapabilitiesExtensions();

            // Act
            var json = config.ToJson();

            // Assert
            Assert.Equal(DefaultJson, json);
        }

        [Fact]
        public void ToJson_Indented_ReturnsFormattedJson()
        {
            // Arrange
            var config = new SimdCapabilitiesExtensionsJsonExtensions.SimdCapabilitiesExtensions();

            // Act
            var json = config.ToJson(indented: true);

            // Assert
            // Indented JSON should contain line breaks
            Assert.Contains("\n", json);
            // And still represent the same data
            var deserialized = SimdCapabilitiesExtensionsJsonExtensions.FromJson(json);
            Assert.NotNull(deserialized);
            Assert.True(deserialized!.IsVectorWidthSupportEnabled);
            Assert.True(deserialized.IsOptimalSimdLevelEnabled);
            Assert.True(deserialized.IsSimdAvailabilityEnabled);
            Assert.True(deserialized.IsFriendlyStringEnabled);
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsObjectWithCorrectValues()
        {
            // Act
            var config = SimdCapabilitiesExtensionsJsonExtensions.FromJson(DefaultJson);

            // Assert
            Assert.NotNull(config);
            Assert.True(config!.IsVectorWidthSupportEnabled);
            Assert.True(config.IsOptimalSimdLevelEnabled);
            Assert.True(config.IsSimdAvailabilityEnabled);
            Assert.True(config.IsFriendlyStringEnabled);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SimdCapabilitiesExtensionsJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var result = SimdCapabilitiesExtensionsJsonExtensions.FromJson(string.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Act & Assert
            Assert.Throws<JsonException>(() => SimdCapabilitiesExtensionsJsonExtensions.FromJson("Invalid JSON"));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndObject()
        {
            // Act
            var success = SimdCapabilitiesExtensionsJsonExtensions.TryFromJson(DefaultJson, out var config);

            // Assert
            Assert.True(success);
            Assert.NotNull(config);
            Assert.True(config!.IsVectorWidthSupportEnabled);
            Assert.True(config.IsOptimalSimdLevelEnabled);
            Assert.True(config.IsSimdAvailabilityEnabled);
            Assert.True(config.IsFriendlyStringEnabled);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Act
            var success = SimdCapabilitiesExtensionsJsonExtensions.TryFromJson("Invalid JSON", out var config);

            // Assert
            Assert.False(success);
            Assert.Null(config);
        }
    }
}
