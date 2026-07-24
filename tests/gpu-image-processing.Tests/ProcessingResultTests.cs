// tests/gpu-image-processing.Tests/ProcessingResultTests.cs
#nullable enable

using System;
using Xunit;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;
using FluentAssertions;

namespace GpuImageProcessing.Tests.Domain;

public class ProcessingResultTests
{
    [Fact]
    public void Constructor_InitializesProperties_WithDefaultValues()
    {
        // Act
        var result = new ProcessingResult();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
        result.ImageId.Should().Be(Guid.Empty);
        result.OutputPath.Should().BeEmpty();
        result.Status.Should().Be(ProcessingStatus.Pending);
        result.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.CompletedAt.Should().Be(DateTime.MinValue);
        result.ProcessingTimeMilliseconds.Should().Be(0);
        result.ErrorMessage.Should().BeNull();
        result.ErrorCode.Should().Be(0);
        result.FiltersApplied.Should().NotBeNull();
        result.FiltersApplied.Should().BeEmpty();
        result.Metrics.Should().NotBeNull();
        result.ResultMetadata.Should().NotBeNull();
        result.ResultMetadata.Should().BeEmpty();
        result.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void Complete_SetsPropertiesCorrectly_WhenCalled()
    {
        // Arrange
        var result = new ProcessingResult();
        var outputPath = "/path/to/output.jpg";
        var startTime = result.StartedAt;

        // Act
        result.Complete(outputPath);

        // Assert
        result.OutputPath.Should().Be(outputPath);
        result.Status.Should().Be(ProcessingStatus.Completed);
        result.IsSuccessful.Should().BeTrue();
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.ProcessingTimeMilliseconds.Should().BeGreaterThan(0);
        result.ProcessingTimeMilliseconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Complete_SetsProcessingTimeCorrectly()
    {
        // Arrange
        var result = new ProcessingResult();
        var startTime = result.StartedAt;
        System.Threading.Thread.Sleep(10); // Ensure some time passes

        // Act
        result.Complete("/output.jpg");

        // Assert
        var processingTime = result.ProcessingTimeMilliseconds;
        processingTime.Should().BeGreaterThan(0);
        processingTime.Should().BeGreaterThanOrEqualTo((long)(result.CompletedAt - startTime).TotalMilliseconds);
    }

    [Fact]
    public void Fail_SetsPropertiesCorrectly_WithErrorMessageAndCode()
    {
        // Arrange
        var result = new ProcessingResult();
        var errorMessage = "File not found";
        var errorCode = 404;
        var startTime = result.StartedAt;

        // Act
        result.Fail(errorMessage, errorCode);

        // Assert
        result.ErrorMessage.Should().Be(errorMessage);
        result.ErrorCode.Should().Be(errorCode);
        result.Status.Should().Be(ProcessingStatus.Failed);
        result.IsSuccessful.Should().BeFalse();
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.ProcessingTimeMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Fail_SetsPropertiesCorrectly_WithDefaultErrorCode()
    {
        // Arrange
        var result = new ProcessingResult();
        var errorMessage = "Invalid format";

        // Act
        result.Fail(errorMessage);

        // Assert
        result.ErrorMessage.Should().Be(errorMessage);
        result.ErrorCode.Should().Be(0);
        result.Status.Should().Be(ProcessingStatus.Failed);
        result.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public void AddFilterApplied_AddsFilterToCollection()
    {
        // Arrange
        var result = new ProcessingResult();
        var initialCount = result.FiltersApplied.Count;
        var filterName = "Grayscale";
        var filterType = FilterType.Grayscale;
        var executionTimeMs = 12.5;

        // Act
        result.AddFilterApplied(filterName, filterType, executionTimeMs);

        // Assert
        result.FiltersApplied.Should().HaveCount(initialCount + 1);
        var addedFilter = result.FiltersApplied[^1];
        addedFilter.FilterName.Should().Be(filterName);
        addedFilter.FilterType.Should().Be(filterType);
        addedFilter.ExecutionTimeMilliseconds.Should().Be(executionTimeMs);
        addedFilter.AppliedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddFilterApplied_AddsMultipleFilters()
    {
        // Arrange
        var result = new ProcessingResult();
        var filter1 = ("Blur", FilterType.Blur, 5.2);
        var filter2 = ("Sharpen", FilterType.Sharpen, 8.7);
        var filter3 = ("EdgeDetection", FilterType.EdgeDetection, 15.3);

        // Act
        result.AddFilterApplied(filter1.Item1, filter1.Item2, filter1.Item3);
        result.AddFilterApplied(filter2.Item1, filter2.Item2, filter2.Item3);
        result.AddFilterApplied(filter3.Item1, filter3.Item2, filter3.Item3);

        // Assert
        result.FiltersApplied.Should().HaveCount(3);
        result.FiltersApplied[0].FilterName.Should().Be(filter1.Item1);
        result.FiltersApplied[1].FilterName.Should().Be(filter2.Item1);
        result.FiltersApplied[2].FilterName.Should().Be(filter3.Item1);
    }

    [Fact]
    public void GetTotalFilterExecutionTime_ReturnsZero_WhenNoFiltersApplied()
    {
        // Arrange
        var result = new ProcessingResult();

        // Act
        var totalTime = result.GetTotalFilterExecutionTime();

        // Assert
        totalTime.Should().Be(0);
    }

    [Fact]
    public void GetTotalFilterExecutionTime_ReturnsSum_WhenFiltersApplied()
    {
        // Arrange
        var result = new ProcessingResult();
        result.AddFilterApplied("Filter1", FilterType.Grayscale, 10.5);
        result.AddFilterApplied("Filter2", FilterType.Blur, 20.3);
        result.AddFilterApplied("Filter3", FilterType.Sharpen, 15.2);

        // Act
        var totalTime = result.GetTotalFilterExecutionTime();

        // Assert
        totalTime.Should().BeApproximately(46.0, 0.01);
    }

    [Fact]
    public void GetTotalFilterExecutionTime_ReturnsCorrectSum_WithManyFilters()
    {
        // Arrange
        var result = new ProcessingResult();
        for (int i = 0; i < 100; i++)
        {
            result.AddFilterApplied($"Filter{i}", FilterType.Grayscale, i + 1.0);
        }

        // Act
        var totalTime = result.GetTotalFilterExecutionTime();

        // Assert
        totalTime.Should().BeApproximately(5050.0, 0.01); // Sum of 1 to 100
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        // Arrange
        var result = new ProcessingResult();
        var id = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var outputPath = "/custom/output.png";
        var status = ProcessingStatus.Processing;
        var startedAt = DateTime.UtcNow.AddHours(-1);
        var completedAt = DateTime.UtcNow;
        var processingTimeMs = 12345L;
        var errorMessage = "Custom error";
        var errorCode = 500;
        var isSuccessful = true;

        // Act
        result.Id = id;
        result.ImageId = imageId;
        result.OutputPath = outputPath;
        result.Status = status;
        result.StartedAt = startedAt;
        result.CompletedAt = completedAt;
        result.ProcessingTimeMilliseconds = processingTimeMs;
        result.ErrorMessage = errorMessage;
        result.ErrorCode = errorCode;
        result.IsSuccessful = isSuccessful;

        // Assert
        result.Id.Should().Be(id);
        result.ImageId.Should().Be(imageId);
        result.OutputPath.Should().Be(outputPath);
        result.Status.Should().Be(status);
        result.StartedAt.Should().Be(startedAt);
        result.CompletedAt.Should().Be(completedAt);
        result.ProcessingTimeMilliseconds.Should().Be(processingTimeMs);
        result.ErrorMessage.Should().Be(errorMessage);
        result.ErrorCode.Should().Be(errorCode);
        result.IsSuccessful.Should().Be(isSuccessful);
    }

    [Fact]
    public void Complete_UpdatesMetricsAndMetadata()
    {
        // Arrange
        var result = new ProcessingResult();
        result.Metrics.CpuUsagePercent = 45.5;
        result.Metrics.GpuUtilizationPercent = 85.2;
        result.ResultMetadata["customKey"] = "customValue";

        // Act
        result.Complete("/output/test.jpg");

        // Assert
        result.Metrics.Should().NotBeNull();
        result.ResultMetadata.Should().ContainKey("customKey");
        result.ResultMetadata["customKey"].Should().Be("customValue");
    }

    [Fact]
    public void Fail_UpdatesMetricsAndMetadata()
    {
        // Arrange
        var result = new ProcessingResult();
        result.Metrics.CpuUsagePercent = 30.0;
        result.ResultMetadata["testKey"] = "testValue";

        // Act
        result.Fail("Test error", 123);

        // Assert
        result.Metrics.Should().NotBeNull();
        result.ResultMetadata.Should().ContainKey("testKey");
        result.ResultMetadata["testKey"].Should().Be("testValue");
        result.ErrorMessage.Should().Be("Test error");
        result.ErrorCode.Should().Be(123);
    }

    [Fact]
    public void FiltersApplied_CollectionOperationsWorkCorrectly()
    {
        // Arrange
        var result = new ProcessingResult();

        // Act - Add via method
        result.AddFilterApplied("TestFilter", FilterType.Rotation, 5.5);

        // Assert - Verify collection state
        result.FiltersApplied.Should().HaveCount(1);
        result.FiltersApplied[0].FilterName.Should().Be("TestFilter");

        // Act - Clear
        result.FiltersApplied.Clear();

        // Assert
        result.FiltersApplied.Should().BeEmpty();
    }

    [Fact]
    public void ProcessingTimeMilliseconds_ReflectsActualTime_AfterComplete()
    {
        // Arrange
        var result = new ProcessingResult();
        var beforeComplete = DateTime.UtcNow;
        System.Threading.Thread.Sleep(20);

        // Act
        result.Complete("/output.jpg");

        // Assert
        result.ProcessingTimeMilliseconds.Should().BeGreaterThanOrEqualTo(20);
        result.CompletedAt.Should().BeAfter(beforeComplete);
    }

    [Fact]
    public void ProcessingTimeMilliseconds_ReflectsActualTime_AfterFail()
    {
        // Arrange
        var result = new ProcessingResult();
        var beforeFail = DateTime.UtcNow;
        System.Threading.Thread.Sleep(15);

        // Act
        result.Fail("Error occurred");

        // Assert
        result.ProcessingTimeMilliseconds.Should().BeGreaterThanOrEqualTo(15);
        result.CompletedAt.Should().BeAfter(beforeFail);
    }
}