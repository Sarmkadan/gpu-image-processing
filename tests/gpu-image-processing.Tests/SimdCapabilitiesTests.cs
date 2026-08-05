namespace GpuImageProcessing.Tests.Domain
{
    using System;
    using System.Numerics;
    using GpuImageProcessing.Domain;
    using Xunit;

    public class SimdCapabilitiesTests
    {
        [Fact]
        public void Construct_WithAllFalse_PropertiesAreFalse()
        {
            var caps = new SimdCapabilities
            {
                SupportsSSE2 = false,
                SupportsSse41 = false,
                SupportsAvx = false,
                SupportsAvx2 = false,
                SupportsAvx512F = false,
                BestAvailableLevel = SimdLevel.None,
                VectorWidthBytes = 0
            };

            Assert.False(caps.SupportsSSE2);
            Assert.False(caps.SupportsSse41);
            Assert.False(caps.SupportsAvx);
            Assert.False(caps.SupportsAvx2);
            Assert.False(caps.SupportsAvx512F);
            Assert.Equal(SimdLevel.None, caps.BestAvailableLevel);
            Assert.Equal(0, caps.VectorWidthBytes);
        }

        [Fact]
        public void Construct_WithAllTrue_PropertiesAreTrue()
        {
            var caps = new SimdCapabilities
            {
                SupportsSSE2 = true,
                SupportsSse41 = true,
                SupportsAvx = true,
                SupportsAvx2 = true,
                SupportsAvx512F = true,
                BestAvailableLevel = SimdLevel.Avx512F,
                VectorWidthBytes = 64
            };

            Assert.True(caps.SupportsSSE2);
            Assert.True(caps.SupportsSse41);
            Assert.True(caps.SupportsAvx);
            Assert.True(caps.SupportsAvx2);
            Assert.True(caps.SupportsAvx512F);
            Assert.Equal(SimdLevel.Avx512F, caps.BestAvailableLevel);
            Assert.Equal(64, caps.VectorWidthBytes);
        }

        [Fact]
        public void Detect_ReturnsConsistentCapabilities()
        {
            var caps = SimdCapabilities.Detect();

            // Check that BestAvailableLevel is consistent with the boolean flags.
            Assert.Equal(caps.SupportsSSE2, caps.BestAvailableLevel >= SimdLevel.Sse2);
            Assert.Equal(caps.SupportsSse41, caps.BestAvailableLevel >= SimdLevel.Sse41);
            Assert.Equal(caps.SupportsAvx, caps.BestAvailableLevel >= SimdLevel.Avx);
            Assert.Equal(caps.SupportsAvx2, caps.BestAvailableLevel >= SimdLevel.Avx2);
            Assert.Equal(caps.SupportsAvx512F, caps.BestAvailableLevel >= SimdLevel.Avx512F);

            // Check that VectorWidthBytes is consistent with the level.
            int expectedWidth = caps.SupportsAvx512F ? 64
                            : caps.SupportsAvx2 || caps.SupportsAvx ? 32
                            : caps.SupportsSSE2 || caps.SupportsSse41 ? 16
                            : Vector<float>.Count * sizeof(float);

            Assert.Equal(expectedWidth, caps.VectorWidthBytes);
        }

        [Fact]
        public void ToString_ReturnsExpectedFormat()
        {
            var caps = new SimdCapabilities
            {
                SupportsSSE2 = true,
                SupportsSse41 = false,
                SupportsAvx = true,
                SupportsAvx2 = false,
                SupportsAvx512F = false,
                BestAvailableLevel = SimdLevel.Avx,
                VectorWidthBytes = 32
            };

            var result = caps.ToString();
            Assert.Contains("SimdLevel=Avx", result);
            Assert.Contains("VectorWidth=32B", result);
            Assert.Contains("[SSE2=True SSE4.1=False AVX=True AVX2=False AVX-512F=False]", result);
        }

        [Fact]
        public void IsAnySimdAvailable_ReturnsTrueWhenAnySimdIsSupported()
        {
            var caps = new SimdCapabilities
            {
                SupportsSSE2 = true,
                SupportsSse41 = false,
                SupportsAvx = false,
                SupportsAvx2 = false,
                SupportsAvx512F = false,
                BestAvailableLevel = SimdLevel.Sse2,
                VectorWidthBytes = 16
            };

            Assert.True(caps.IsAnySimdAvailable);
        }

        [Fact]
        public void IsAnySimdAvailable_ReturnsFalseWhenNoSimdIsSupported()
        {
            var caps = new SimdCapabilities
            {
                SupportsSSE2 = false,
                SupportsSse41 = false,
                SupportsAvx = false,
                SupportsAvx2 = false,
                SupportsAvx512F = false,
                BestAvailableLevel = SimdLevel.None,
                VectorWidthBytes = 0
            };

            Assert.False(caps.IsAnySimdAvailable);
        }
    }
}