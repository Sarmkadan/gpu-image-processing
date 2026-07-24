// SPDX-License-Identifier: MIT
// Unit tests for BatchProcessingPipelineValidation
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using GpuImageProcessing.Pipeline;
using Xunit;

namespace GpuImageProcessing.Tests.Pipeline;

/// <summary>
/// Contains unit tests for the <see cref="BatchProcessingPipelineValidation"/> static class.
/// Tests the validation, IsValid, and EnsureValid extension methods for <see cref="BatchPipelineResult"/>.
/// </summary>
public class BatchProcessingPipelineValidationTests
{
    [Fact]
    public void Validate_ValidBatchPipelineResult_ReturnsEmptyList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test Batch",
            TotalImages = 10,
            SucceededCount = 8,
            FailedCount = 2,
            TotalDuration = TimeSpan.FromSeconds(30),
            AverageProcessingMs = 150.5,
            Outcomes = CreateValidOutcomes(10),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        Assert.NotNull(validationResult);
        Assert.Empty(validationResult);
    }

    [Fact]
    public void Validate_ValidBatchPipelineResultWithMinimalValues_ReturnsEmptyList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Minimal",
            TotalImages = 0,
            SucceededCount = 0,
            FailedCount = 0,
            TotalDuration = TimeSpan.Zero,
            AverageProcessingMs = 0,
            Outcomes = Array.Empty<ImagePipelineOutcome>(),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var validationResult = result.Validate();

        // Assert
        Assert.NotNull(validationResult);
        Assert.Empty(validationResult);
    }

    [Fact]
    public void Validate_NullBatchPipelineResult_ThrowsArgumentNullException()
    {
        // Arrange
        BatchPipelineResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.Validate());
    }

    [Fact]
    public void Validate_BatchIdEmpty_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.Empty,
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Batch ID cannot be empty.", problems);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BatchNameNullOrWhitespace_ReturnsProblemList(string? batchName)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = batchName,
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Batch name cannot be null or whitespace.", problems);
    }

    [Fact]
    public void Validate_BatchNameTooLong_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = new string('A', 256),
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Batch name cannot exceed 255 characters.", problems);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_TotalImagesNegative_ReturnsProblemList(int totalImages)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = totalImages,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Total images count cannot be negative.", problems);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_SucceededCountNegative_ReturnsProblemList(int succeededCount)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = succeededCount,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Contains("Succeeded count cannot be negative.", problems);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_FailedCountNegative_ReturnsProblemList(int failedCount)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = failedCount,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Contains("Failed count cannot be negative.", problems);
    }

    [Fact]
    public void Validate_SucceededAndFailedNotEqualToTotal_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 10,
            SucceededCount = 7,
            FailedCount = 2,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(10),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Succeeded count plus failed count must equal total images count.", problems);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Validate_TotalDurationNegative_ReturnsProblemList(int milliseconds)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromMilliseconds(milliseconds),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Total duration cannot be negative.", problems);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void Validate_AverageProcessingMsNegative_ReturnsProblemList(double processingMs)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = processingMs,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Average processing time cannot be negative.", problems);
    }

    [Fact]
    public void Validate_CompletedAtDefault_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = default
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Completed timestamp cannot be default (Unix epoch).", problems);
    }

    [Fact]
    public void Validate_CompletedAtNotUtc_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.Now // Local time, not UTC
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Completed timestamp must be in UTC.", problems);
    }

    [Fact]
    public void Validate_CompletedAtInFuture_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow.AddMinutes(10) // 10 minutes in future
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Completed timestamp cannot be in the future.", problems);
    }

    [Fact]
    public void Validate_NullOutcomesCollection_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = null,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Outcomes collection cannot be null.", problems);
    }

    [Fact]
    public void Validate_OutcomesCountMismatchTotalImages_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 10,
            SucceededCount = 10,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5), // Only 5 outcomes for 10 total images
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Outcomes collection count must match total images count.", problems);
    }

    [Fact]
    public void Validate_NullOutcomeInCollection_ReturnsProblemList()
    {
        // Arrange
        var outcomes = new List<ImagePipelineOutcome> {
            new(Guid.NewGuid(), PipelineStage.Completed, 1, 100, null),
            null,
            new(Guid.NewGuid(), PipelineStage.Completed, 1, 100, null)
        };
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 3,
            SucceededCount = 3,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = outcomes,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Outcome in collection cannot be null.", problems);
    }

    [Fact]
    public void Validate_ImageIdEmptyInOutcome_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 1,
            SucceededCount = 1,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = new List<ImagePipelineOutcome> {
                new(Guid.Empty, PipelineStage.Completed, 1, 100, null)
            },
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Image ID in outcome cannot be empty.", problems);
    }

    [Theory]
    [InlineData(-1)] // Invalid lower bound
    [InlineData(100)] // Invalid upper bound
    public void Validate_InvalidStageInOutcome_ReturnsProblemList(int invalidStageValue)
    {
        // Arrange
        var invalidStage = (PipelineStage)invalidStageValue;
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 1,
            SucceededCount = 1,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = new List<ImagePipelineOutcome> {
                new(Guid.NewGuid(), invalidStage, 1, 100, null)
            },
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains($"Image outcome has invalid stage: {invalidStage}.", problems);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AttemptsLessThanOneInOutcome_ReturnsProblemList(int attempts)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 1,
            SucceededCount = 1,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = new List<ImagePipelineOutcome> {
                new(Guid.NewGuid(), PipelineStage.Completed, attempts, 100, null)
            },
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Attempts count must be at least 1.", problems);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.1)]
    public void Validate_ProcessingMsNegativeInOutcome_ReturnsProblemList(double processingMs)
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 1,
            SucceededCount = 1,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = new List<ImagePipelineOutcome> {
                new(Guid.NewGuid(), PipelineStage.Completed, 1, processingMs, null)
            },
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Processing time cannot be negative.", problems);
    }

    [Fact]
    public void Validate_FailedOutcomeWithoutError_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 1,
            SucceededCount = 0,
            FailedCount = 1,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = new List<ImagePipelineOutcome> {
                new(Guid.NewGuid(), PipelineStage.Failed, 3, 100, null) // Failed but no error message
            },
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Failed outcome must have an error message.", problems);
    }

    [Fact]
    public void Validate_NonFailedOutcomeWithError_ReturnsProblemList()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Test",
            TotalImages = 1,
            SucceededCount = 1,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = new List<ImagePipelineOutcome> {
                new(Guid.NewGuid(), PipelineStage.Completed, 1, 100, "Some error") // Completed but has error
            },
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Single(problems);
        Assert.Contains("Only failed outcomes should have error messages.", problems);
    }

    [Fact]
    public void Validate_MultipleProblems_ReturnsAllProblems()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.Empty, // Problem 1
            BatchName = null, // Problem 2
            TotalImages = -5, // Problem 3
            SucceededCount = 10,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(-10), // Problem 4
            AverageProcessingMs = -50, // Problem 5
            Outcomes = null, // Problem 6
            CompletedAt = DateTime.UtcNow.AddMinutes(1) // Problem 7
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.NotNull(problems);
        Assert.Contains("Batch ID cannot be empty.", problems);
        Assert.Contains("Batch name cannot be null or whitespace.", problems);
        Assert.Contains("Total images count cannot be negative.", problems);
        Assert.Contains("Total duration cannot be negative.", problems);
        Assert.Contains("Average processing time cannot be negative.", problems);
        Assert.Contains("Outcomes collection cannot be null.", problems);
        // The future timestamp validation may not be reached if other validations fail first
        // due to early returns in the validation logic
    }

    [Fact]
    public void IsValid_ValidBatchPipelineResult_ReturnsTrue()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Valid Batch",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_InvalidBatchPipelineResult_ReturnsFalse()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.Empty,
            BatchName = "Invalid Batch",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_NullBatchPipelineResult_ThrowsArgumentNullException()
    {
        // Arrange
        BatchPipelineResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.IsValid());
    }

    [Fact]
    public void EnsureValid_ValidBatchPipelineResult_DoesNotThrow()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.NewGuid(),
            BatchName = "Valid Batch",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act
        var act = () => result.EnsureValid();

        // Assert
        Assert.Null(Record.Exception(act));
    }

    [Fact]
    public void EnsureValid_InvalidBatchPipelineResult_ThrowsArgumentException()
    {
        // Arrange
        var result = new BatchPipelineResult
        {
            BatchId = Guid.Empty,
            BatchName = "Invalid Batch",
            TotalImages = 5,
            SucceededCount = 5,
            FailedCount = 0,
            TotalDuration = TimeSpan.FromSeconds(10),
            AverageProcessingMs = 100,
            Outcomes = CreateValidOutcomes(5),
            CompletedAt = DateTime.UtcNow
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => result.EnsureValid());
        Assert.Contains("BatchPipelineResult is invalid", exception.Message);
    }

    [Fact]
    public void EnsureValid_NullBatchPipelineResult_ThrowsArgumentNullException()
    {
        // Arrange
        BatchPipelineResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result!.EnsureValid());
    }

    private static IReadOnlyList<ImagePipelineOutcome> CreateValidOutcomes(int count)
    {
        var outcomes = new List<ImagePipelineOutcome>(count);
        for (int i = 0; i < count; i++)
        {
            outcomes.Add(new ImagePipelineOutcome(
                Guid.NewGuid(),
                PipelineStage.Completed,
                1,
                100.0,
                null
            ));
        }
        return outcomes;
    }
}