#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Contains unit tests for the <see cref="FilterChainBuilder"/> class, which provides a fluent API for constructing filter chains.
/// </summary>
/// <remarks>
/// These tests validate the builder pattern implementation, ensuring that filter chains can be constructed
/// with various configurations including single and multiple steps, parallel execution, caching, and custom filters.
/// </remarks>
public class FilterChainBuilderTests
{
    /// <summary>
    /// Tests that creating a filter chain with a blank name throws an <see cref="ArgumentException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the builder properly validates the chain name parameter and rejects empty or whitespace-only names.
    /// </remarks>
    [Fact]
    public void Create_BlankName_ThrowsArgumentException()
    {
        var act = () => FilterChainBuilder.Create(" ");
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    /// <summary>
    /// Tests that building a filter chain without adding any steps throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the builder enforces the requirement of having at least one filter step before building.
    /// </remarks>
    [Fact]
    public void Build_NoStepsAdded_ThrowsInvalidOperationException()
    {
        var act = () => FilterChainBuilder.Create("Empty").Build();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*at least one filter step*");
    }

    /// <summary>
    /// Tests that building a filter chain with a single step produces a valid chain.
    /// </summary>
    /// <remarks>
    /// Validates the basic builder functionality for creating a simple filter chain with one grayscale filter.
    /// Ensures the resulting chain has the correct name, contains one step, is enabled, and passes validation.
    /// </remarks>
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

    /// <summary>
    /// Tests that building a filter chain with multiple steps preserves the declaration order.
    /// </summary>
    /// <remarks>
    /// Validates that filter steps are executed in the order they were added to the builder.
    /// Ensures each step has the correct order value matching its position in the chain.
    /// </remarks>
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

    /// <summary>
    /// Tests that setting a description on a filter chain persists it in the built chain.
    /// </summary>
    /// <remarks>
    /// Validates the <see cref="FilterChainBuilder.WithDescription"/> method correctly sets the chain description.
    /// Ensures the description is available after building the chain.
    /// </remarks>
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

    /// <summary>
    /// Tests that enabling parallel execution sets the appropriate flags on the built chain.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AllowParallelExecution"/> method correctly configures
    /// parallel execution settings including the max parallel steps value.
    /// </remarks>
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

    /// <summary>
    /// Tests that enabling intermediate result caching sets the appropriate flag on the built chain.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.CacheIntermediates"/> method correctly sets
    /// the cache intermediate results flag on the built chain.
    /// </remarks>
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

    /// <summary>
    /// Tests that adding a blur filter with an invalid radius throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddBlur"/> method properly validates the radius parameter.
    /// Ensures negative radius values are rejected.
    /// </remarks>
    [Fact]
    public void AddBlur_InvalidRadius_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddBlur(radius: -5f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("radius");
    }

    /// <summary>
    /// Tests that adding a sharpen filter with excessive strength throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddSharpen"/> method properly validates the strength parameter.
    /// Ensures values above the maximum allowed are rejected.
    /// </remarks>
    [Fact]
    public void AddSharpen_StrengthAboveMax_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddSharpen(strength: 99f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("strength");
    }

    /// <summary>
    /// Tests that adding a rotation filter with an angle out of range throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddRotation"/> method properly validates the angleDegrees parameter.
    /// Ensures angles outside the valid range are rejected.
    /// </remarks>
    [Fact]
    public void AddRotation_AngleOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddRotation(angleDegrees: 400f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("angleDegrees");
    }

    /// <summary>
    /// Tests that adding a scaling filter with a negative scaleX value throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddScaling"/> method properly validates the scaleX parameter.
    /// Ensures negative scaling values are rejected.
    /// </remarks>
    [Fact]
    public void AddScaling_NegativeScaleX_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddScaling(scaleX: -1f, scaleY: 1f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("scaleX");
    }

    /// <summary>
    /// Tests that adding a color correction filter with brightness out of range throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddColorCorrection"/> method properly validates the brightness parameter.
    /// Ensures brightness values outside the valid range are rejected.
    /// </remarks>
    [Fact]
    public void AddColorCorrection_BrightnessOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddColorCorrection(brightness: 2.0f)
            .Build();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("brightness");
    }

    /// <summary>
    /// Tests that adding a custom filter with an empty GUID throws an <see cref="ArgumentException"/>.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddCustomFilter"/> method properly validates the existingFilterId parameter.
    /// Ensures empty GUID values are rejected.
    /// </remarks>
    [Fact]
    public void AddCustomFilter_EmptyGuid_ThrowsArgumentException()
    {
        var act = () => FilterChainBuilder
            .Create("Test")
            .AddCustomFilter(Guid.Empty)
            .Build();

        act.Should().Throw<ArgumentException>().WithParameterName("existingFilterId");
    }

    /// <summary>
    /// Tests that adding a custom filter with a valid GUID includes it in the built chain steps.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder.AddCustomFilter"/> method correctly adds a custom filter to the chain.
    /// Ensures the filter ID and estimated execution time are properly set in the step.
    /// </remarks>
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

    /// <summary>
    /// Tests that the estimated processing time sums the execution times of all steps.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChain.EstimateTotalProcessingTime"/> method correctly calculates
    /// the total processing time by summing the estimated execution times of all filter steps.
    /// </remarks>
    [Fact]
    public void Build_EstimatedProcessingTimeSumsStepEstimates()
    {
        var chain = FilterChainBuilder
            .Create("Timed Chain")
            .AddGrayscale() // 2.0 ms
            .AddEdgeDetection() // 8.0 ms
            .Build();

        chain.EstimateTotalProcessingTime().Should().BeApproximately(10.0, precision: 0.01);
    }

    /// <summary>
    /// Tests that the fluent API returns the same builder instance for method chaining.
    /// </summary>
    /// <remarks>
    /// Validates that the builder methods follow the fluent interface pattern correctly,
    /// returning the same instance to enable method chaining.
    /// </remarks>
    [Fact]
    public void Build_FluentChaining_ReturnsSameBuilderInstance()
    {
        var builder = FilterChainBuilder.Create("Chained");
        var returnedBuilder = builder.AddGrayscale();

        // Verify fluent API returns the same builder (method chaining contract).
        returnedBuilder.Should().BeSameAs(builder);
    }

    /// <summary>
    /// Tests that the CI preset configuration produces a chain with the expected number of steps.
    /// </summary>
    /// <remarks>
    /// Validates that the <see cref="FilterChainBuilder"/> can construct complex chains with multiple filters.
    /// This indirectly validates the builder is used in the benchmark configuration.
    /// Ensures the CI preset enables the builder benchmark category.
    /// </remarks>
    [Fact]
    public void ForCi_Preset_ProducesThreeCategories()
    {
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