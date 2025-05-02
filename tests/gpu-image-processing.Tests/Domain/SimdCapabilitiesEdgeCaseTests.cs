#nullable enable
using FluentAssertions;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests.Domain;

public sealed class SimdCapabilitiesEdgeCaseTests
{
    [Fact]
    public void SimdLevel_None_IsZero()
    {
        SimdLevel.None.Should().Be(0);
    }

    [Fact]
    public void SimdLevel_Ordering_IsAscending()
    {
        ((int)SimdLevel.Sse2).Should().BeLessThan((int)SimdLevel.Sse41);
        ((int)SimdLevel.Sse41).Should().BeLessThan((int)SimdLevel.Avx);
        ((int)SimdLevel.Avx).Should().BeLessThan((int)SimdLevel.Avx2);
        ((int)SimdLevel.Avx2).Should().BeLessThan((int)SimdLevel.Avx512F);
    }

    [Fact]
    public void SimdCapabilities_DefaultValues_AllFalse()
    {
        var caps = new SimdCapabilities();
        caps.SupportsSSE2.Should().BeFalse();
        caps.SupportsSse41.Should().BeFalse();
        caps.SupportsAvx.Should().BeFalse();
    }

    [Fact]
    public void SimdCapabilities_WithInitializer_SetsCorrectly()
    {
        var caps = new SimdCapabilities
        {
            SupportsSSE2 = true,
            SupportsSse41 = true,
            SupportsAvx = false
        };

        caps.SupportsSSE2.Should().BeTrue();
        caps.SupportsSse41.Should().BeTrue();
        caps.SupportsAvx.Should().BeFalse();
    }

    [Fact]
    public void Detect_ReturnsNonNull()
    {
        var caps = SimdCapabilities.Detect();
        caps.Should().NotBeNull();
    }

    [Fact]
    public void Detect_ConsistentResults()
    {
        var caps1 = SimdCapabilities.Detect();
        var caps2 = SimdCapabilities.Detect();

        caps1.SupportsSSE2.Should().Be(caps2.SupportsSSE2);
        caps1.SupportsSse41.Should().Be(caps2.SupportsSse41);
        caps1.SupportsAvx.Should().Be(caps2.SupportsAvx);
    }
}
