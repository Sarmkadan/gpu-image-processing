#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using FluentAssertions;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using Xunit;

namespace GpuImageProcessing.Tests;

/// <summary>
/// Tests for the ImageBatch domain class.
/// </summary>
public class ImageBatchTests
{
    [Fact]
    public void Constructor_Default_InitializesPendingBatchWithUniqueId()
    {
        var before = DateTime.UtcNow;

        var first = new ImageBatch();
        var second = new ImageBatch();

        first.Id.Should().NotBe(Guid.Empty);
        first.Id.Should().NotBe(second.Id);
        first.Status.Should().Be(ProcessingStatus.Pending);
        first.Name.Should().BeEmpty();
        first.ImageIds.Should().BeEmpty();
        first.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void AddImage_NewAndDuplicateIds_AddsOnlyOnceAndUpdatesTotal()
    {
        var batch = new ImageBatch();
        var imageId = Guid.NewGuid();

        batch.AddImage(imageId).Should().BeTrue();
        batch.AddImage(imageId).Should().BeFalse();

        batch.ImageIds.Should().ContainSingle().Which.Should().Be(imageId);
        batch.TotalImages.Should().Be(1);
    }

    [Fact]
    public void AddImage_AtMaxBatchSize_ReturnsFalseAndDoesNotGrow()
    {
        var batch = new ImageBatch();
        for (var i = 0; i < AppConstants.Processing.MaxBatchSize; i++)
            batch.AddImage(Guid.NewGuid()).Should().BeTrue();

        var added = batch.AddImage(Guid.NewGuid());

        added.Should().BeFalse();
        batch.ImageIds.Should().HaveCount(AppConstants.Processing.MaxBatchSize);
        batch.TotalImages.Should().Be(AppConstants.Processing.MaxBatchSize);
    }

    [Fact]
    public void RemoveImage_UnknownId_ReturnsFalseAndKeepsTotal()
    {
        var batch = new ImageBatch();
        var imageId = Guid.NewGuid();
        batch.AddImage(imageId);

        var removed = batch.RemoveImage(Guid.NewGuid());

        removed.Should().BeFalse();
        batch.ImageIds.Should().ContainSingle().Which.Should().Be(imageId);
        batch.TotalImages.Should().Be(1);
    }

    [Fact]
    public void MarkImageProcessed_MixedResults_ComputesProgressAndSuccessRate()
    {
        var batch = new ImageBatch();
        batch.GetProgressPercentage().Should().Be(0.0);
        batch.GetSuccessRate().Should().Be(0.0);

        for (var i = 0; i < 4; i++)
            batch.AddImage(Guid.NewGuid());

        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: true);
        batch.MarkImageProcessed(success: false);

        batch.ProcessedImages.Should().Be(3);
        batch.FailedImages.Should().Be(1);
        batch.GetProgressPercentage().Should().BeApproximately(100.0, 0.0001);
        batch.GetSuccessRate().Should().BeApproximately(75.0, 0.0001);
    }

    [Fact]
    public void Start_ThenComplete_SetsStatusAndTimestamps()
    {
        var batch = new ImageBatch();

        batch.Start();
        batch.Status.Should().Be(ProcessingStatus.Processing);
        batch.StartedAt.Should().NotBe(default(DateTime));

        batch.Complete();
        batch.Status.Should().Be(ProcessingStatus.Completed);
        batch.CompletedAt.Should().NotBe(default(DateTime));
        batch.CompletedAt.Should().BeOnOrAfter(batch.StartedAt);
    }

    [Fact]
    public void Validate_MissingImagesFiltersOrOutputDirectory_ReturnsFalse()
    {
        var batch = new ImageBatch();

        batch.Validate().Should().BeFalse();

        batch.AddImage(Guid.NewGuid());
        batch.Validate().Should().BeFalse();

        batch.AddFilter(Guid.NewGuid());
        batch.Validate().Should().BeFalse();

        batch.OutputDirectory = "   ";
        batch.Validate().Should().BeFalse();

        batch.OutputDirectory = "/tmp/out";
        batch.Validate().Should().BeTrue();
    }

    [Fact]
    public void GetEstimatedRemainingTime_NotProcessing_ReturnsNull()
    {
        var batch = new ImageBatch();
        batch.AddImage(Guid.NewGuid());
        batch.AddImage(Guid.NewGuid());
        batch.MarkImageProcessed();

        // Pending batch with progress: no estimate until Start() is called.
        batch.GetEstimatedRemainingTime().Should().BeNull();

        batch.Start();
        batch.MarkImageProcessed();
        batch.Complete();

        // Completed batch: no estimate either.
        batch.GetEstimatedRemainingTime().Should().BeNull();
    }
}
