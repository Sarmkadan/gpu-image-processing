using System;
using System.Text.Json;
using Xunit;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests
{
    public class SimdCapabilitiesJsonExtensionsTests
    {
        private const string DefaultJson =
            "{\"supportsSSE2\":true,\"supportsSse41\":true,\"supportsAvx\":true,\"supportsAvx2\":true,\"supportsAvx512F\":true,\"bestAvailableLevel\":5,\"vectorWidthBytes\":64,\"isAnySimdAvailable\":true}";

        [Fact]
        public void ToJson_HappyPath_ReturnsExpectedJson()
        {
            // Arrange
            var capabilities = new SimdCapabilities
            {
                SupportsSSE2 = true,
                SupportsSse41 = true,
                SupportsAvx = true,
                SupportsAvx2 = true,
                SupportsAvx512F = true,
                BestAvailableLevel = SimdLevel.Avx512F,
                VectorWidthBytes = 64
            };

            // Act
            var json = capabilities.ToJson();

            // Assert
            Assert.Equal(DefaultJson, json);
        }

        [Fact]
        public void ToJson_Indented_ReturnsFormattedJson()
        {
            // Arrange
            var capabilities = new SimdCapabilities
            {
                SupportsSSE2 = true,
                SupportsSse41 = true,
                SupportsAvx = true,
                SupportsAvx2 = true,
                SupportsAvx512F = true,
                BestAvailableLevel = SimdLevel.Avx512F,
                VectorWidthBytes = 64
            };

            // Act
            var json = capabilities.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json);
            // And still represent the same data
            var deserialized = SimdCapabilitiesJsonExtensions.FromJson(json);
            Assert.NotNull(deserialized);
            Assert.True(deserialized!.SupportsSSE2);
            Assert.True(deserialized.SupportsSse41);
            Assert.True(deserialized.SupportsAvx);
            Assert.True(deserialized.SupportsAvx2);
            Assert.True(deserialized.SupportsAvx512F);
            Assert.Equal(SimdLevel.Avx512F, deserialized.BestAvailableLevel);
            Assert.Equal(64, deserialized.VectorWidthBytes);
        }

        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SimdCapabilitiesJsonExtensions.ToJson(null!));
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsObjectWithCorrectValues()
        {
            // Act
            var capabilities = SimdCapabilitiesJsonExtensions.FromJson(DefaultJson);

            // Assert
            Assert.NotNull(capabilities);
            Assert.True(capabilities!.SupportsSSE2);
            Assert.True(capabilities.SupportsSse41);
            Assert.True(capabilities.SupportsAvx);
            Assert.True(capabilities.SupportsAvx2);
            Assert.True(capabilities.SupportsAvx512F);
            Assert.Equal(SimdLevel.Avx512F, capabilities.BestAvailableLevel);
            Assert.Equal(64, capabilities.VectorWidthBytes);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SimdCapabilitiesJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyJson_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => SimdCapabilitiesJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Act & Assert
            Assert.Throws<JsonException>(() => SimdCapabilitiesJsonExtensions.FromJson("Invalid JSON"));
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndObject()
        {
            // Act
            var success = SimdCapabilitiesJsonExtensions.TryFromJson(DefaultJson, out var capabilities);

            // Assert
            Assert.True(success);
            Assert.NotNull(capabilities);
            Assert.True(capabilities!.SupportsSSE2);
            Assert.True(capabilities.SupportsSse41);
            Assert.True(capabilities.SupportsAvx);
            Assert.True(capabilities.SupportsAvx2);
            Assert.True(capabilities.SupportsAvx512F);
            Assert.Equal(SimdLevel.Avx512F, capabilities.BestAvailableLevel);
            Assert.Equal(64, capabilities.VectorWidthBytes);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Act
            var success = SimdCapabilitiesJsonExtensions.TryFromJson("Invalid JSON", out var capabilities);

            // Assert
            Assert.False(success);
            Assert.Null(capabilities);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SimdCapabilitiesJsonExtensions.TryFromJson(null!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyJson_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => SimdCapabilitiesJsonExtensions.TryFromJson(string.Empty, out _));
        }
    }
}