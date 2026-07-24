using Xunit;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Core;
using System.Text.Json;

namespace GpuImageProcessing.Tests.Domain;

public class ProcessingResultJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_WithDefaultOptions_ReturnsValidJson()
    {
        // Arrange
        var processingResult = CreateSampleProcessingResult();

        // Act
        var json = processingResult.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("imageId", json); // Verify key fields are in JSON
        Assert.Contains("outputPath", json);

        // Verify it's valid JSON
        var deserialized = JsonSerializer.Deserialize<ProcessingResult>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(ProcessingStatus.Completed, deserialized.Status);
        Assert.True(deserialized.IsSuccessful);
    }

    [Fact]
    public void ToJson_HappyPath_WithIndentedOptions_ReturnsFormattedJson()
    {
        // Arrange
        var processingResult = CreateSampleProcessingResult();

        // Act
        var json = processingResult.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains(Environment.NewLine, json); // Should contain newlines for formatting

        // Verify it's still valid JSON
        var deserialized = JsonSerializer.Deserialize<ProcessingResult>(json);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        ProcessingResult? nullResult = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullResult!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_DeserializesCorrectly()
    {
        // Arrange
        var processingResult = CreateSampleProcessingResult();
        var json = processingResult.ToJson();

        // Act
        var result = ProcessingResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(processingResult.Status, result.Status);
        Assert.Equal(processingResult.IsSuccessful, result.IsSuccessful);
        Assert.Equal(processingResult.FiltersApplied.Count, result.FiltersApplied.Count);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ProcessingResultJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProcessingResultJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_EmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProcessingResultJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json {{{";

        // Act & Assert
        Assert.Throws<JsonException>(() => ProcessingResultJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ProcessingResultJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => ProcessingResultJsonExtensions.FromJson(string.Empty));
        Assert.Throws<ArgumentException>(() => ProcessingResultJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializes()
    {
        // Arrange
        var processingResult = CreateSampleProcessingResult();
        var json = processingResult.ToJson();
        ProcessingResult? result = null;

        // Act
        var success = ProcessingResultJsonExtensions.TryFromJson(json, out result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(processingResult.Id, result.Id);
    }

    [Fact]
    public void TryFromJson_NullJson_ReturnsFalseAndNull()
    {
        // Arrange
        ProcessingResult? result = new();

        // Act
        var success = ProcessingResultJsonExtensions.TryFromJson(null, out result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_EmptyOrWhitespaceJson_ReturnsFalseAndNull()
    {
        // Arrange
        ProcessingResult? result1 = new();
        ProcessingResult? result2 = new();
        ProcessingResult? result3 = new();

        // Act
        var success1 = ProcessingResultJsonExtensions.TryFromJson(string.Empty, out result1);
        var success2 = ProcessingResultJsonExtensions.TryFromJson("   ", out result2);
        var success3 = ProcessingResultJsonExtensions.TryFromJson(null, out result3);

        // Assert
        Assert.False(success1);
        Assert.Null(result1);

        Assert.False(success2);
        Assert.Null(result2);

        Assert.False(success3);
        Assert.Null(result3);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json {{{";
        ProcessingResult? result = new();

        // Act
        var success = ProcessingResultJsonExtensions.TryFromJson(invalidJson, out result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void RoundTrip_SerializationDeserialization_PreservesAllData()
    {
        // Arrange
        var original = CreateSampleProcessingResult();

        // Act
        var json = original.ToJson();
        var deserialized = ProcessingResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Status, deserialized.Status);
        Assert.Equal(original.IsSuccessful, deserialized.IsSuccessful);
        Assert.Equal(original.FiltersApplied.Count, deserialized.FiltersApplied.Count);
        Assert.Equal(original.Metrics.CpuUsagePercent, deserialized.Metrics.CpuUsagePercent);
    }

    [Fact]
    public void RoundTrip_WithFailedStatus_PreservesErrorDetails()
    {
        // Arrange
        var original = CreateSampleProcessingResult();
        original.Fail("Test error message", 42);

        // Act
        var json = original.ToJson();
        var deserialized = ProcessingResultJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.False(deserialized.IsSuccessful);
        Assert.Equal("Test error message", deserialized.ErrorMessage);
        Assert.Equal(42, deserialized.ErrorCode);
        Assert.Equal(ProcessingStatus.Failed, deserialized.Status);
    }

    private static ProcessingResult CreateSampleProcessingResult()
    {
        var result = new ProcessingResult
        {
            ImageId = Guid.NewGuid(),
            OutputPath = "/path/to/output.jpg",
            Status = ProcessingStatus.Completed,
            IsSuccessful = true,
            ResultMetadata = new Dictionary<string, object>
            {
                { "testKey", "testValue" },
                { "number", 42 }
            }
        };

        result.Complete("/path/to/output.jpg");

        result.AddFilterApplied("Grayscale", FilterType.Grayscale, 12.5);
        result.AddFilterApplied("Blur", FilterType.Blur, 8.2);

        result.Metrics.CpuUsagePercent = 45.5;
        result.Metrics.MemoryUsedBytes = 1024 * 1024 * 100; // 100MB
        result.Metrics.GpuMemoryUsedBytes = 1024 * 1024 * 50; // 50MB
        result.Metrics.GpuUtilizationPercent = 85.2;
        result.Metrics.AverageExecutionTimeMs = 15.3;
        result.Metrics.TotalOperationsCount = 10;
        result.Metrics.RecordExecution(12.5);
        result.Metrics.RecordExecution(8.2);

        return result;
    }

    private static void AssertProcessingResultsEqual(ProcessingResult expected, ProcessingResult actual)
    {
        Assert.Equal(expected.ImageId, actual.ImageId);
        Assert.Equal(expected.OutputPath, actual.OutputPath);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.StartedAt, actual.StartedAt);
        Assert.Equal(expected.IsSuccessful, actual.IsSuccessful);
        Assert.Equal(expected.FiltersApplied.Count, actual.FiltersApplied.Count);
        Assert.Equal(expected.Metrics.CpuUsagePercent, actual.Metrics.CpuUsagePercent);
        Assert.Equal(expected.ResultMetadata.Count, actual.ResultMetadata.Count);

        // Check processing time is reasonable (within 1 second due to serialization overhead)
        Assert.True(Math.Abs(expected.ProcessingTimeMilliseconds - actual.ProcessingTimeMilliseconds) < 1000);

        // Check error details match if present
        if (expected.ErrorMessage != null)
        {
            Assert.Equal(expected.ErrorMessage, actual.ErrorMessage);
            Assert.Equal(expected.ErrorCode, actual.ErrorCode);
        }
    }
}