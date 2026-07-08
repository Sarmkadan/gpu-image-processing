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

public class FilterChainTests
{
    [Fact]
    public void AddStep_DefaultOrder_AppendsStepToEnd()
    {
        // Arrange
        var chain = new FilterChain { Name = "My Chain" };
        var filterId1 = Guid.NewGuid();
        var filterId2 = Guid.NewGuid();

        // Act
        chain.AddStep(filterId1);
        chain.AddStep(filterId2);

        // Assert
        chain.Steps.Should().HaveCount(2);
        chain.Steps[0].FilterId.Should().Be(filterId1);
        chain.Steps[1].FilterId.Should().Be(filterId2);
    }

    [Fact]
    public void RemoveStep_ExistingFilter_ReturnsTrue_AndReordersRemainingSteps()
    {
        // Arrange
        var chain = new FilterChain { Name = "My Chain" };
        var filterId1 = Guid.NewGuid();
        var filterId2 = Guid.NewGuid();
        var filterId3 = Guid.NewGuid();
        chain.AddStep(filterId1);
        chain.AddStep(filterId2);
        chain.AddStep(filterId3);

        // Act
        var removed = chain.RemoveStep(filterId2);

        // Assert
        removed.Should().BeTrue();
        chain.Steps.Should().HaveCount(2);
        chain.Steps[0].Order.Should().Be(0);
        chain.Steps[1].Order.Should().Be(1);
    }

    [Fact]
    public void RemoveStep_NonExistentFilter_ReturnsFalse()
    {
        // Arrange
        var chain = new FilterChain { Name = "My Chain" };
        chain.AddStep(Guid.NewGuid());

        // Act
        var removed = chain.RemoveStep(Guid.NewGuid());

        // Assert
        removed.Should().BeFalse();
        chain.Steps.Should().HaveCount(1);
    }

    [Fact]
    public void ReorderSteps_MismatchedFilterCount_ThrowsArgumentException()
    {
        // Arrange
        var chain = new FilterChain { Name = "My Chain" };
        chain.AddStep(Guid.NewGuid());
        chain.AddStep(Guid.NewGuid());
        var wrongList = new List<Guid> { Guid.NewGuid() }; // 1 entry, chain has 2

        // Act
        Action act = () => chain.ReorderSteps(wrongList);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*match current step count*");
    }

    [Fact]
    public void GetEnabledSteps_WithMixedEnabledState_ReturnsOnlyEnabledSteps()
    {
        // Arrange
        var chain = new FilterChain { Name = "My Chain" };
        var filterId1 = Guid.NewGuid();
        var filterId2 = Guid.NewGuid();
        chain.AddStep(filterId1);
        chain.AddStep(filterId2);
        chain.Steps[0].IsEnabled = false;
        chain.Steps[1].IsEnabled = true;

        // Act
        var enabled = chain.GetEnabledSteps();

        // Assert
        enabled.Should().HaveCount(1);
        enabled[0].FilterId.Should().Be(filterId2);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy_WithSuffixedName()
    {
        // Arrange
        var original = new FilterChain { Name = "Pipeline" };
        original.AddStep(Guid.NewGuid());

        // Act
        var clone = original.Clone();
        clone.Steps.Clear();

        // Assert
        clone.Name.Should().Be("Pipeline (Copy)");
        original.Steps.Should().HaveCount(1); // original is unaffected by clone mutation
    }

    [Fact]
    public void FilterConfiguration_Validate_ParameterWithoutMatchingType_ReturnsFalse()
    {
        // Arrange
        var config = new FilterConfiguration
        {
            Name = "Blur",
            FilterType = FilterType.Blur,
            MaxThreadsPerBlock = 256
        };
        config.Parameters["radius"] = 5.0f;
        // intentionally omitting ParameterTypes["radius"]

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeFalse();
    }
}
