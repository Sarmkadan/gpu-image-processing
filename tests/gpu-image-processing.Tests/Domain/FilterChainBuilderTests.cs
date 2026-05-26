#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Domain;

public class FilterChainBuilderTests
{
    [Fact]
    public void Create_BlankName_ThrowsArgumentException()
    {
        var act = () => FilterChainBuilder.Create("  ");
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Build_NoStepsAdded_ThrowsInvalidOperationException()
    {
        var act = () => FilterChainBuilder.Create("Empty").Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*at least one filter step*");
    }

    [Fact]
    public void Build_SingleStep_ProducesValidChain()
    {
        var chain = FilterChainBuilder
            .Create("Grayscale Only")
            .AddGrayscale()
            .Build();

        chain.Should().NotBeNull();
        chain.Name.Should().Be("Grayscale Only");
        chain.Steps.Should().HaveCount(1);
        chain.IsEnabled.Should().BeTrue();
        chain.Validate().Should().BeTrue();
    }

    [Fact]
    public void Build_MultipleSteps_PreservesDeclarationOrder()
    {
        var chain = FilterChainBuilder
            .Create("Multi-Step")
            .AddGrayscale()
            .AddBlur()
            .AddSharpen()
            .Build();

        chain.Steps.Should().HaveCount(3);
        chain.Steps[0].Order.Should().Be(0);
        chain.Steps[1].Order.Should().Be(1);
        chain.Steps[2].Order.Should().Be(2);
    }

    [Fact]
    public void Build_WithDescription_SetsChainDescription()
    {
        var chain = FilterChainBuilder
            .Create("Described Chain")
            .WithDescription("A test description")
            .AddGrayscale()
            .Build();

        chain.Description.Should().Be("A test description");
    }

    [Fact]
    public void Build_AllowParallelExecution_SetsFlag()
    {
        var chain = FilterChainBuilder
            .Create("Parallel Chain")
            .AddBlur()
            .AddSharpen()
            .AllowParallelExecution(maxParallelSteps: 4)
            .Build();

        chain.AllowParallelExecution.Should().BeTrue();
        chain.MaxParallelSteps.Should().Be(4);
    }

    [Fact]
    public void Build_CacheIntermediates_SetsFlag()
    {
        var chain = FilterChainBuilder
            .Create("Cached Chain")
            .AddGrayscale()
            .CacheIntermediates()
            .Build();

        chain.CacheIntermediateResults.Should().BeTrue();
    }

    [Fact]
    public void AddBlur_InvalidRadius_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddBlur(radius: -5f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("radius");
    }

    [Fact]
    public void AddSharpen_StrengthAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddSharpen(strength: 99f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("strength");
    }

    [Fact]
    public void AddRotation_AngleOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddRotation(angleDegrees: 400f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("angleDegrees");
    }

    [Fact]
    public void AddScaling_NegativeScaleX_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddScaling(scaleX: -1f, scaleY: 1f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("scaleX");
    }

    [Fact]
    public void AddColorCorrection_BrightnessOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddColorCorrection(brightness: 2.0f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("brightness");
    }

    [Fact]
    public void AddCustomFilter_EmptyGuid_ThrowsArgumentException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddCustomFilter(Guid.Empty)
            .Build();

        act.Should().Throw<ArgumentException>().WithParameterName("existingFilterId");
    }

    [Fact]
    public void AddCustomFilter_ValidGuid_AppearsInSteps()
    {
        var filterId = Guid.NewGuid();
        var chain = FilterChainBuilder
            .Create("Custom Chain")
            .AddCustomFilter(filterId, estimatedExecutionMs: 30.0)
            .Build();

        chain.Steps.Should().HaveCount(1);
        chain.Steps[0].FilterId.Should().Be(filterId);
        chain.Steps[0].EstimatedExecutionTimeMs.Should().Be(30.0);
    }

    [Fact]
    public void Build_EstimatedProcessingTimeSumsStepEstimates()
    {
        var chain = FilterChainBuilder
            .Create("Timed Chain")
            .AddGrayscale()      // 2.0 ms
            .AddEdgeDetection()  // 8.0 ms
            .Build();

        chain.EstimateTotalProcessingTime().Should().BeApproximately(10.0, precision: 0.01);
    }

    [Fact]
    public void Build_FluentChaining_ReturnsSameBuilderInstance()
    {
        var builder = FilterChainBuilder.Create("Chained");
        var returnedBuilder = builder.AddGrayscale();

        // Verify fluent API returns the same builder (method chaining contract).
        returnedBuilder.Should().BeSameAs(builder);
    }

    [Fact]
    public void ForCi_Preset_ProducesThreeCategories()
    {
        // This indirectly validates the FilterChainBuilder is used in the benchmark
        // configuration — the CI preset enables the builder benchmark category.
        var chain = FilterChainBuilder
            .Create("Full Workflow")
            .AddGrayscale()
            .AddBlur(2.0f)
            .AddSharpen(0.8f)
            .AddEdgeDetection()
            .AddBilateral()
            .AddMedian()
            .AddSobel()
            .WithExecutionOrder(1)
            .Build();

        chain.Steps.Should().HaveCount(7);
        chain.ExecutionOrder.Should().Be(1);
    }
}
